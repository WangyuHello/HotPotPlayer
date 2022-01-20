using HotPotPlayer.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Realms;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HotPotPlayer.Services
{
    internal class LocalMusicService: IDisposable
    {
        enum LocalMusicState
        {
            Idle,
            FirstLoading,
            NonFirstLoading,
            InitLoadingComplete
        }

        public event Action<List<AlbumDataGroup>, List<AlbumItem>> OnAlbumGroupChanged;
        public event Action OnFirstLoadingStarted;
        public event Action OnNonFirstLoadingStarted;
        public event Action OnLoadingEnded;

        //FileSystemWatcher _fsw;

        static List<string> GetMusicLibrary => ((App)Application.Current).MusicLibrary;

        static readonly List<string> SupportedExt = new() { ".flac", ".wav", ".m4a", ".mp3" };

        static List<MusicItem> GetAllMusic(IEnumerable<FileInfo> files)
        {
            var r = files.Select(f =>
            {
                using var tfile = TagLib.File.Create(f.FullName);
                var item = new MusicItem
                {
                    Title = tfile.Tag.Title,
                    Artists = tfile.Tag.Performers,
                    Album = tfile.Tag.Album,
                    Duration = tfile.Properties.Duration,
                    Track = (int)tfile.Tag.Track,
                    File = f,
                    Source = f.FullName,
                    Year = tfile.Tag.Year,
                };

                return item;
            }).ToList();

            return r;
        }

        private static List<FileInfo> GetMusicFilesFromLibrary()
        {
            var libs = GetMusicLibrary;
            List<FileInfo> files = new();
            foreach (var lib in libs)
            {
                var di = new DirectoryInfo(lib);
                files.AddRange(di.GetFiles("*.*", SearchOption.AllDirectories).Where(f => SupportedExt.Contains(f.Extension)));
            }

            return files;
        }

        List<AlbumItem> GroupAllMusicIntoAlbum(List<MusicItem> allMusic)
        {
            var albums = allMusic.GroupBy(m2 => m2.AlbumSignature).Select(g2 => {
                var music = g2.ToList();
                music.Sort((a,b) => a.Track.CompareTo(b.Track));

                var (cover, color) = WriteCoverToLocalCache(g2.First());
                var i = new AlbumItem
                {
                    Year = g2.First().Year,
                    Title = g2.First().Album,
                    Artists = g2.First().Artists,
                    Cover = cover,
                    MusicItems = music,
                    MainColor = color
                };
                foreach (var item in i.MusicItems)
                {
                    item.Cover = i.Cover;
                    item.MainColor = i.MainColor;
                }
                return i;
            }).ToList();
            return albums;
        }

        static List<AlbumDataGroup> GroupAllMusic(List<AlbumItem> albums)
        {
            var r = albums.GroupBy(m => m.Year).Select(g => 
            {
                var albumList = g.ToList();
                albumList.Sort((a,b) => (a.Title ?? "").CompareTo(b.Title ?? ""));
                var r = new AlbumDataGroup()
                {
                    Year = g.Key,
                    Items = new ObservableCollection<AlbumItem>(albumList)
                };
                foreach (var a in r.Items)
                {
                    foreach (var item in a.MusicItems)
                    {
                        item.AlbumRef = a;
                    }
                }
                return r;
            }).ToList();
            r.Sort((a, b) => b.Year.CompareTo(a.Year));
            return r;
        }

        readonly MD5 md5 = MD5.Create();

        (string, Windows.UI.Color) WriteCoverToLocalCache(MusicItem m)
        {
            var baseDir = ((App)Application.Current).CacheFolder;
            var albumCoverDir = Path.Combine(baseDir, "Cover");
            if (!Directory.Exists(albumCoverDir))
            {
                Directory.CreateDirectory(albumCoverDir);
            }

            var tag = TagLib.File.Create(m.File.FullName);
            Span<byte> binary = tag.Tag.Pictures?.FirstOrDefault()?.Data?.Data;

            if (binary != null && binary.Length != 0)
            {
                var buffer = md5.ComputeHash(binary.ToArray());
                var hashName = Convert.ToHexString(buffer);
                var albumCoverName = Path.Combine(albumCoverDir, hashName);

                using var image = Image.Load<Rgba32>(binary);
                var width = image.Width;
                var height = image.Height;
                image.Mutate(x => x.Resize(400, 400*height/width));
                var color = GetMainColor(image);
                image.SaveAsPng(albumCoverName);

                return (albumCoverName, color);
            }
            return (string.Empty, Colors.White);
        }

        private static Windows.UI.Color GetMainColor(Image<Rgba32> image)
        {
            var wQua = image.Width >> 2;
            var hQua = image.Height >> 2;
            var centerX = image.Width >> 1;
            var centerY = image.Height >> 1;
            var pix = image[centerX, centerY];
            var pix1 = image[wQua, hQua];
            var pix2 = image[3*wQua, hQua];
            var pix3 = image[wQua, 3*hQua];
            var pix4 = image[3*wQua, 3* hQua];
            int a = (pix.A + pix1.A + pix2.A + pix3.A + pix4.A) / 5;
            int r = (pix.R + pix1.R + pix2.R + pix3.R + pix4.R) / 5;
            int g = (pix.G + pix1.G + pix2.G + pix3.G + pix4.G) / 5;
            int b = (pix.B + pix1.B + pix2.B + pix3.B + pix4.B) / 5;
            return Windows.UI.Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);
        }

        IDisposable _albumsToken;
        IDisposable _playListToken;

        public void StartLoadLocalMusic()
        {
            localMusicBackgroundWorker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            localMusicBackgroundWorker.DoWork += LocalMusicBackgroundWorker_DoWork;
            localMusicBackgroundWorker.ProgressChanged += LocalMusicBackgroundWorker_ProgressChanged;
            localMusicBackgroundWorker.RunWorkerCompleted += LocalMusicBackgroundWorker_RunWorkerCompleted;
            if (localMusicBackgroundWorker.IsBusy)
            {
                return;
            }
            localMusicBackgroundWorker.RunWorkerAsync();
        }

        private void OnPlayListChange(IRealmCollection<AlbumItemDb> sender, ChangeSet changes, Exception error)
        {
            
        }

        private void OnAlbumChange(IRealmCollection<AlbumItemDb> sender, ChangeSet changes, Exception error)
        {
            if (changes == null)
            {
                //首次启动，数据在sender中
                return;
            }

            foreach (var i in changes.DeletedIndices)
            {
                // ... handle deletions ...
            }
            foreach (var i in changes.InsertedIndices)
            {
                // ... handle insertions ...
            }
            foreach (var i in changes.NewModifiedIndices)
            {
                // ... handle modifications ...
            }
        }

        private void LocalMusicBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var state = (LocalMusicState)e.ProgressPercentage;
            switch (state)
            {
                case LocalMusicState.Idle:
                    break;
                case LocalMusicState.FirstLoading:
                    OnFirstLoadingStarted?.Invoke();
                    break;
                case LocalMusicState.NonFirstLoading:
                    OnNonFirstLoadingStarted?.Invoke();
                    break;
                case LocalMusicState.InitLoadingComplete:
                    OnLoadingEnded?.Invoke();
                    var (albumGroup, playLists) = ((List<AlbumDataGroup> a, List<AlbumItem> b))e.UserState;
                    OnAlbumGroupChanged?.Invoke(albumGroup, playLists);
                    break;
                default:
                    break;
            }
        }

        void LocalMusicBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OnLoadingEnded?.Invoke();
            if (e.Result == null)
            {
                return;
            }
            var (albumGroup, playLists) = ((List<AlbumDataGroup> a, List<AlbumItem> b))e.Result;
            OnAlbumGroupChanged?.Invoke(albumGroup, playLists);
        }

        static string GetDbPath()
        {
            var baseDir = ((App)Application.Current).CacheFolder;
            var dbDir = Path.Combine(baseDir, "Db");
            if (!Directory.Exists(dbDir)) { Directory.CreateDirectory(dbDir); }
            var dbPath = Path.Combine(dbDir, "LocalMusic.db");
            return dbPath;
        }

        Realm _db;

        void LocalMusicBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {            
            //检查是否有缓存好的数据库
            var dbPath = GetDbPath();
            if (File.Exists(dbPath))
            {
                //如果有就返回数据库
                _db = Realm.GetInstance(dbPath);
                var albums_ = _db.All<AlbumItemDb>().Where(d => !d.IsPlayList);
                var albumList_ = albums_.ToList().Select(d => d.ToOrigin()).ToList();
                var playLists_ = _db.All<AlbumItemDb>().Where(d => d.IsPlayList);
                var playListList = playLists_.ToList().Select(d => d.ToOrigin()).ToList();

                _albumsToken = albums_.SubscribeForNotifications(OnAlbumChange);
                _playListToken = playLists_.SubscribeForNotifications(OnPlayListChange);

                var groupsDb = GroupAllMusic(albumList_);

                localMusicBackgroundWorker.ReportProgress(3, (groupsDb, playListList));

                var localDbList = _db.All<MusicItemDb>().ToList().Select(m => m.File);
                var files2 = GetMusicFilesFromLibrary().Select(f => f.FullName);

                var newFiles = files2.Except(localDbList);
                var delFiles = localDbList.Except(files2);
                if (newFiles.Any() || delFiles.Any())
                {
                    //如果有更新，开始后台线程扫描
                    localMusicBackgroundWorker.ReportProgress(2);
                }
                else
                {
                    return;
                }
            }
            else
            {
                //如果没有返回空集，并开始后台线程扫描
                localMusicBackgroundWorker.ReportProgress(1);
            }

            var files = GetMusicFilesFromLibrary();
            var allmusic = GetAllMusic(files);
            var albums = GroupAllMusicIntoAlbum(allmusic);
            var groups = GroupAllMusic(albums);

            _db = Realm.GetInstance(dbPath);
            _db.Write(() =>
            {
                _db.RemoveAll();
                _db.Add(albums.Select(a => a.ToDb()));
            });

            var playLists = GetAllPlaylists(_db);
            _db.Write(() =>
            {
                _db.Add(playLists.Select(a => a.ToDb()));
            });

            var albums2 = _db.All<AlbumItemDb>().Where(d => !d.IsPlayList);
            var playLists2 = _db.All<AlbumItemDb>().Where(d => d.IsPlayList);

            _albumsToken = albums2.SubscribeForNotifications(OnAlbumChange);
            _playListToken = playLists2.SubscribeForNotifications(OnPlayListChange);

            e.Result = (groups, playLists);

            //InitFileSystemWatcher();
        }

        BackgroundWorker localMusicBackgroundWorker;

        private static List<AlbumItem> GetAllPlaylists(Realm db)
        {
            var libs = ((App)Application.Current).MusicPlayList;
            List<FileInfo> files = new();
            foreach (var lib in libs)
            {
                var di = new DirectoryInfo(lib);
                files.AddRange(di.GetFiles("*.zpl", SearchOption.AllDirectories));
            }

            var r = files.Select(f =>
            {
                var ost_doc = XDocument.Load(f.FullName);
                var smil = ost_doc.Root;
                var body = smil.Elements().FirstOrDefault(n => n.Name == "body");
                var head = smil.Elements().FirstOrDefault(n => n.Name == "head");
                var title = head.Elements().FirstOrDefault(n => n.Name == "title").Value;
                var seq = body.Elements().FirstOrDefault(n => n.Name == "seq");
                var srcs = seq.Elements().Select(m => m.Attribute("src").Value);
                var files = srcs.Select(path =>
                {
                    var musicFromDb = db.All<MusicItemDb>().Where(d => d.File == path).FirstOrDefault();
                    var origin = musicFromDb?.ToOrigin();
                    return origin;
                }).ToList();
                files.RemoveAll(f => f == null);

                var pl = new AlbumItem
                {
                    Title = title,
                    MusicItems = files,
                    Artists = Array.Empty<string>(),
                    IsPlayList = true,
                };
                pl.SetPlayListCover();
                return pl;
            }).ToList();

            return r;
        }

        //private void InitFileSystemWatcher()
        //{
        //    _fsw = new FileSystemWatcher
        //    {
        //        Path = GetMusicLibrary.First(),
        //        IncludeSubdirectories = true,
        //        Filter = "*.flac|*.wav|*.m4a|*.mp3",
        //        NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName
        //    };

        //    _fsw.Created += MusicCreated;
        //    _fsw.Renamed += MusicRenamed;
        //}

        //private void MusicRenamed(object sender, RenamedEventArgs e)
        //{
        //    var old = e.OldFullPath;
        //    var newPath = e.FullPath;
        //    using var db = Realm.GetInstance(GetDbPath());
        //    db.Write(() =>
        //    {
        //        var music = db.All<MusicItemDb>().Where(m => m.File == old).First();
        //        music.File = newPath;
        //        music.Source = newPath;
        //    });
        //}

        //private void MusicCreated(object sender, FileSystemEventArgs e)
        //{
            
        //}

        public void Dispose()
        {
            localMusicBackgroundWorker?.Dispose();
            _albumsToken?.Dispose();
            _playListToken?.Dispose();
            _db?.Dispose();
            //_fsw?.Dispose();
        }
    }
}
