using HotPotPlayer.Extensions;
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
using Windows.Storage;

namespace HotPotPlayer.Services
{
    public class LocalMusicService: ServiceBaseWithApp
    {
        public LocalMusicService(AppBase app) : base(app) { }

        enum LocalMusicState
        {
            Idle,
            FirstLoading,
            NonFirstLoading,
            InitLoadingComplete,
            NoLibraryAccess
        }

        public event Action<List<AlbumDataGroup>, List<PlayListItem>> OnAlbumGroupChanged;
        public event Action OnFirstLoadingStarted;
        public event Action OnNonFirstLoadingStarted;
        public event Action OnLoadingEnded;
        public event Action OnNoLibraryAccess;

        //FileSystemWatcher _fsw;

        List<LibraryItem> GetMusicLibrary => App.MusicLibrary;

        static readonly List<string> SupportedExt = new() { ".flac", ".wav", ".m4a", ".mp3" };

        static List<MusicItem> GetAllMusicItemAsync(IEnumerable<FileInfo> files)
        {
            var r = files.Select(f =>
            {
                using var tfile = TagLib.File.Create(f.FullName);
                //var duration = await GetMusicDurationAsync(f);
                var item = new MusicItem
                {
                    Source = f,
                    Title = tfile.Tag.Title,
                    Artists = tfile.Tag.Performers,
                    Album = tfile.Tag.Album,
                    Year = (int)tfile.Tag.Year,
                    //Duration = duration,
                    Duration = tfile.Properties.Duration,
                    Track = (int)tfile.Tag.Track,
                    LastWriteTime = f.LastWriteTime,
                    AlbumArtists = tfile.Tag.AlbumArtists,
                    Disc = (int)tfile.Tag.Disc,
                };

                return item;
            }).ToList();

            return r;
        }

        static async Task<TimeSpan> GetMusicDurationAsync(FileInfo f)
        {
            var file = await StorageFile.GetFileFromPathAsync(f.FullName);
            var prop = await file?.Properties?.GetMusicPropertiesAsync();
            return prop == null ? TimeSpan.Zero : prop.Duration;
        }

        private static List<FileInfo> GetMusicFilesFromLibrary(List<string> libs)
        {
            List<FileInfo> files = new();
            foreach (var lib in libs)
            {
                var di = new DirectoryInfo(lib);
                files.AddRange(di.GetFiles("*.*", SearchOption.AllDirectories).Where(f => SupportedExt.Contains(f.Extension)));
            }

            return files;
        }

        List<AlbumItem> GroupAllMusicIntoAlbum(IEnumerable<MusicItem> allMusic)
        {
            var albums = allMusic.GroupBy(m2 => m2.AlbumSignature).Select(g2 => 
            {
                var music = g2.ToList();
                music.Sort((a,b) => a.DiscTrack.CompareTo(b.DiscTrack));

                var (cover, color) = WriteCoverToLocalCache(g2.First());

                var albumArtists = g2.First().AlbumArtists.Length == 0 ? g2.First().Artists : g2.First().AlbumArtists;
                var allArtists = g2.SelectMany(m => m.Artists).Concat(albumArtists).Distinct().ToArray();

                var i = new AlbumItem
                {
                    Title = g2.First().Album,
                    Artists = albumArtists,
                    Year = g2.First().Year,
                    Cover = cover,
                    MusicItems = music,
                    MainColor = color,
                    AllArtists = allArtists
                };
                foreach (var item in i.MusicItems)
                {
                    item.Cover = i.Cover;
                    item.MainColor = i.MainColor;
                    item.AlbumRef = i;
                }
                return i;
            }).ToList();
            return albums;
        }

        static List<AlbumDataGroup> GroupAllAlbumByYear(IEnumerable<AlbumItem> albums)
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
                return r;
            }).ToList();
            r.Sort((a, b) => b.Year.CompareTo(a.Year));
            return r;
        }

        readonly MD5 md5 = MD5.Create();

        (string, Windows.UI.Color) WriteCoverToLocalCache(MusicItem m)
        {
            var baseDir = App.LocalFolder;
            var albumCoverDir = Path.Combine(baseDir, "Cover");
            if (!Directory.Exists(albumCoverDir))
            {
                Directory.CreateDirectory(albumCoverDir);
            }

            var tag = TagLib.File.Create(m.Source.FullName);
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

        public List<AlbumDataGroup> LocalAlbums;
        public List<PlayListItem> LocalPlayLists;

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
                    var (albumGroup, playLists) = ((List<AlbumDataGroup> a, List<PlayListItem> b))e.UserState;
                    LocalAlbums = albumGroup;
                    LocalPlayLists = playLists;
                    OnAlbumGroupChanged?.Invoke(albumGroup, playLists);
                    break;
                case LocalMusicState.NoLibraryAccess:
                    OnNoLibraryAccess?.Invoke();
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
            var (albumGroup, playLists) = ((List<AlbumDataGroup> a, List<PlayListItem> b))e.Result;
            LocalAlbums = albumGroup;
            LocalPlayLists = playLists;
            OnAlbumGroupChanged?.Invoke(albumGroup, playLists);
        }

        string GetDbPath()
        {
            var dbPath = Path.Combine(App.DatabaseFolder, "LocalMusic.db");
            return dbPath;
        }

        string _dbPath;
        string DbPath
        {
            get => _dbPath ??= GetDbPath();
        }

        Realm _db;

        void LocalMusicBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //检查是否有缓存好的数据库
            var libs = GetMusicLibrary;
            if (libs == null)
            {
                localMusicBackgroundWorker.ReportProgress((int)LocalMusicState.NoLibraryAccess);
                return;
            }
            var playListFiles = GetAllPlaylists();
            bool skipScanAllMusic = false;

            if (File.Exists(DbPath))
            {
                //如果有就返回数据库
                _db = Realm.GetInstance(DbPath);
                var albums_ = _db.All<AlbumItemDb>();
                var albumList_ = albums_.AsEnumerable().Select(d => d.ToOrigin()).ToList();
                var playLists_ = _db.All<PlayListItemDb>();
                var playListList = playLists_.AsEnumerable().Select(d => d.ToOrigin()).ToList();

                var groupsDb = GroupAllAlbumByYear(albumList_);

                localMusicBackgroundWorker.ReportProgress((int)LocalMusicState.InitLoadingComplete, (groupsDb, playListList));

                var files2 = GetMusicFilesFromLibrary(libs.Select(l => l.Path).ToList());

                var musicHasUpdate = CheckMusicHasUpdate(files2);
                var playListHasUpdate = CheckPlayListHasUpdate(playListList, playListFiles);

                if (musicHasUpdate)
                {
                    localMusicBackgroundWorker.ReportProgress((int)LocalMusicState.NonFirstLoading);
                }
                else if (playListHasUpdate)
                {
                    localMusicBackgroundWorker.ReportProgress((int)LocalMusicState.NonFirstLoading);
                    skipScanAllMusic = true;
                }
                else
                {
                    _db?.Dispose();
                    return;
                }
            }
            else
            {
                //如果没有返回空集，并开始后台线程扫描
                localMusicBackgroundWorker.ReportProgress((int)LocalMusicState.FirstLoading);
            }

            _db ??= Realm.GetInstance(DbPath);
            List<AlbumDataGroup> groups = skipScanAllMusic ? null : ScanAllMusic(libs.Select(l => l.Path).ToList());
            List<PlayListItem> playLists = ScanAllPlayList(playListFiles);

            e.Result = (groups, playLists);
            _db?.Dispose();
        }

        private bool CheckMusicHasUpdate(List<FileInfo> files)
        {
            foreach (var f in files)
            {
                var target = _db.All<MusicItemDb>().FirstOrDefault(m => m.Source == f.FullName);
                if (target == null) return true;
                if (target.LastWriteTime != f.LastWriteTime.ToBinary()) return true;
            }
            return false;
        }

        private List<PlayListItem> ScanAllPlayList(List<FileInfo> playListsFile)
        {
            var playLists = GetAllPlaylistItem(_db, playListsFile);
            _db.Write(() =>
            {
                var exist = _db.All<PlayListItemDb>();
                _db.RemoveRange(exist);
                _db.Add(playLists.Select(a => a.ToDb()), update: true);
            });
            return playLists;
        }

        private List<AlbumDataGroup> ScanAllMusic(List<string> libs)
        {
            var files = GetMusicFilesFromLibrary(libs);
            var allmusic = GetAllMusicItemAsync(files);
            var albums = GroupAllMusicIntoAlbum(allmusic);
            var groups = GroupAllAlbumByYear(albums);

            _db.Write(() =>
            {
                var existAlbum = _db.All<AlbumItemDb>();
                var existMusic = _db.All<MusicItemDb>();
                _db.RemoveRange(existAlbum);
                _db.RemoveRange(existMusic);
                _db.Add(albums.Select(a => a.ToDb()));
            });
            return groups;
        }

        BackgroundWorker localMusicBackgroundWorker;



        private static bool CheckPlayListHasUpdate(List<PlayListItem> stored, List<FileInfo> current)
        {
            foreach (var s in stored)
            {
                var match = current.FirstOrDefault(f => f.FullName == s.Source.FullName);
                if (match != null && match.LastWriteTime != s.LastWriteTime)
                {
                    return true;
                }
            }
            return false;
        }

        private List<FileInfo> GetAllPlaylists()
        {
            var libs = App.MusicPlayList.Select(s => s.Path);
            List<FileInfo> files = new();
            foreach (var lib in libs)
            {
                var di = new DirectoryInfo(lib);
                if (!di.Exists) continue;
                files.AddRange(di.GetFiles("*.zpl", SearchOption.AllDirectories));
            }
            return files;
        }

        private static List<PlayListItem> GetAllPlaylistItem(Realm db, List<FileInfo> files)
        {
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
                    var musicFromDb = db.All<MusicItemDb>().Where(d => d.Source == path).FirstOrDefault();
                    var origin = musicFromDb?.ToOrigin();
                    return origin;
                }).ToList();
                files.RemoveAll(f => f == null);

                var pl = new PlayListItem
                {
                    Source = f,
                    Title = title,
                    Year = f.LastWriteTime.Year,
                    LastWriteTime = f.LastWriteTime,
                    MusicItems = files,
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

        List<AlbumItem> QueryArtistAlbum(string artistName)
        {
            using var db = Realm.GetInstance(DbPath);
            var result = db.All<AlbumItemDb>().Where(a => a.AllArtists.Contains(artistName)).AsEnumerable().Select(d => d.ToOrigin()).ToList();
            return result;
        }

        public List<AlbumDataGroup> GetArtistAlbumGroup(string artistName)
        {
            var album = QueryArtistAlbum(artistName);
            var group = GroupAllAlbumByYear(album);
            return group;
        }

        public AlbumItem QueryAlbum(MusicItem musicItem)
        {
            using var db = Realm.GetInstance(DbPath);
            var musicDb = db.Find<MusicItemDb>(musicItem.GetKey());
            var album = musicDb.GetAlbum();
            return album;
        }

        public override void Dispose()
        {
            localMusicBackgroundWorker?.Dispose();
            _db?.Dispose();
            //_fsw?.Dispose();
        }
    }
}
