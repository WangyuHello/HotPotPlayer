using HotPotPlayer.Extensions;
using HotPotPlayer.Models;
using Microsoft.UI.Dispatching;
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
    public class LocalMusicService: ServiceBaseWithConfig
    {
        public LocalMusicService(ConfigBase config) : base(config) { }

        enum LocalMusicState
        {
            Idle,
            FirstLoading,
            NonFirstLoading,
            InitLoadingComplete,
            NoLibraryAccess
        }

        public event Action<List<AlbumGroup>, List<PlayListItem>> OnAlbumGroupChanged;
        public event Action OnFirstLoadingStarted;
        public event Action OnNonFirstLoadingStarted;
        public event Action OnLoadingEnded;
        public event Action OnNoLibraryAccess;


        public static readonly List<string> SupportedExt = new() { ".flac", ".wav", ".m4a", ".mp3" };

        static List<MusicItem> GetAllMusicItem(IEnumerable<FileInfo> files)
        {
            return files.Select(FileToMusic).ToList();
        }

        public static MusicItem FileToMusic(FileInfo f)
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

        static List<AlbumGroup> GroupAllAlbumByYear(IEnumerable<AlbumItem> albums)
        {
            var r = albums.GroupBy(m => m.Year).Select(g => 
            {
                var albumList = g.ToList();
                albumList.Sort((a,b) => (a.Title ?? "").CompareTo(b.Title ?? ""));
                var r = new AlbumGroup()
                {
                    Year = g.Key,
                    Items = new ObservableCollection<AlbumItem>(albumList)
                };
                return r;
            }).ToList();
            r.Sort((a, b) => b.Year.CompareTo(a.Year));
            return r;
        }

        MD5 _md5;
        MD5 Md5 => _md5 ??= MD5.Create();

        string _albumCoverDir;
        string AlbumCoverDir
        {
            get
            {
                if (string.IsNullOrEmpty(_albumCoverDir))
                {
                    _albumCoverDir = Path.Combine(Config.LocalFolder, "Cover");
                    if (!Directory.Exists(_albumCoverDir))
                    {
                        Directory.CreateDirectory(_albumCoverDir);
                    }
                }
                return _albumCoverDir;
            }
        }

        (string, Color) WriteCoverToLocalCache(MusicItem m)
        {
            if (!string.IsNullOrEmpty(m.Cover))
            {
                return (m.Cover, m.MainColor);
            }
         
            var tag = TagLib.File.Create(m.Source.FullName);
            Span<byte> binary = tag.Tag.Pictures?.FirstOrDefault()?.Data?.Data;

            if (binary != null && binary.Length != 0)
            {
                var buffer = Md5.ComputeHash(binary.ToArray());
                var hashName = Convert.ToHexString(buffer);
                var albumCoverName = Path.Combine(AlbumCoverDir, hashName);

                using var image = Image.Load<Rgba32>(binary);
                var width = image.Width;
                var height = image.Height;
                image.Mutate(x => x.Resize(400, 400*height/width));
                var color = image.GetMainColor();
                image.SaveAsPng(albumCoverName);

                return (albumCoverName, color);
            }
            return (string.Empty, Color.White);
        }

        public void StartLoadLocalMusic()
        {
            UIQueue ??= DispatcherQueue.GetForCurrentThread();
            if (Worker.IsBusy)
            {
                return;
            }
            Worker.RunWorkerAsync();
        }

        public List<AlbumGroup> LocalAlbums = new();
        public List<PlayListItem> LocalPlayLists = new();

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
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
                    var (albumGroup, playLists) = ((List<AlbumGroup> a, List<PlayListItem> b))e.UserState;
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

        void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OnLoadingEnded?.Invoke();
            if (e.Result == null)
            {
                return;
            }
            var (albumGroup, playLists) = ((List<AlbumGroup> a, List<PlayListItem> b))e.Result;
            LocalAlbums = albumGroup;
            LocalPlayLists = playLists;
            OnAlbumGroupChanged?.Invoke(albumGroup, playLists);
        }

        string GetDbPath()
        {
            var dbPath = Path.Combine(Config.DatabaseFolder, "LocalMusic.db");
            return dbPath;
        }

        string _dbPath;
        string DbPath
        {
            get => _dbPath ??= GetDbPath();
        }

        Realm _db;

        void DoWork(object sender, DoWorkEventArgs e)
        {
            //检查是否有缓存好的数据库
            var libs = Config.MusicLibrary;
            if (libs == null)
            {
                Worker.ReportProgress((int)LocalMusicState.NoLibraryAccess);
                return;
            }
            var playListFiles = GetAllPlaylists();

            IEnumerable<string> removeList = null;
            IEnumerable<FileInfo> addOrUpdateList = null;

            if (File.Exists(DbPath))
            {
                //如果有就返回数据库
                _db = Realm.GetInstance(DbPath);
                var albums_ = _db.All<AlbumItemDb>();
                var albumList_ = albums_.AsEnumerable().Select(d => d.ToOrigin()).ToList();
                var playLists_ = _db.All<PlayListItemDb>();
                var playListList = playLists_.AsEnumerable().Select(d => d.ToOrigin()).ToList();

                var groupsDb = GroupAllAlbumByYear(albumList_);

                Worker.ReportProgress((int)LocalMusicState.InitLoadingComplete, (groupsDb, playListList));

                var files2 = GetMusicFilesFromLibrary(libs.Select(l => l.Path).ToList());

                (removeList, addOrUpdateList) = CheckMusicHasUpdate(files2);
                var playListHasUpdate = CheckPlayListHasUpdate(playListList, playListFiles);

                if (addOrUpdateList.Any() || playListHasUpdate)
                {
                    Worker.ReportProgress((int)LocalMusicState.NonFirstLoading);
                }
                else if (removeList.Any())
                {

                }
                else
                {
                    _db?.Dispose();
                    InitFileSystemWatcher();
                    return;
                }
            }
            else
            {
                //如果没有返回空集，并开始后台线程扫描
                Worker.ReportProgress((int)LocalMusicState.FirstLoading);
            }

            _db ??= Realm.GetInstance(DbPath);
            List<AlbumGroup> groups = null;
            if (addOrUpdateList != null && addOrUpdateList.Any())
            {
                groups = AddOrUpdateNewMusicAndSave(addOrUpdateList);
            }
            else if(removeList != null && removeList.Any())
            {
                groups = RemoveMusicAndSave(removeList);
            }
            else
            {
                groups = ScanAllMusicAndSave(libs.Select(l => l.Path).ToList());
            }
            List<PlayListItem> playLists = ScanAllPlayList(playListFiles);

            e.Result = (groups, playLists);
            _db?.Dispose();

            InitFileSystemWatcher();
        }

        private List<AlbumGroup> RemoveMusicAndSave(IEnumerable<string> removeList)
        {
            _db.Write(() =>
            {
                foreach (var item in removeList)
                {
                    _db.Remove(_db.Find<MusicItemDb>(item));
                }
            });

            var allmusic = _db.All<MusicItemDb>().AsEnumerable().Select(d => d.ToOrigin());
            var albums = GroupAllMusicIntoAlbum(allmusic);

            _db.Write(() =>
            {
                var existAlbum = _db.All<AlbumItemDb>();
                var existMusic = _db.All<MusicItemDb>();
                _db.RemoveRange(existAlbum);
                _db.RemoveRange(existMusic);
                _db.Add(albums.Select(a => a.ToDb()));
            });

            var groups = GroupAllAlbumByYear(albums);
            return groups;
        }

        private List<AlbumGroup> AddOrUpdateNewMusicAndSave(IEnumerable<FileInfo> addOrUpdateList)
        {
            List<AlbumGroup> groups;
            var addOrUpdateMusic = GetAllMusicItem(addOrUpdateList);
            _db.Write(() =>
            {
                _db.Add(addOrUpdateMusic.Select(a => a.ToDb()), update: true);
            });

            var allmusic = _db.All<MusicItemDb>().AsEnumerable().Select(d => d.ToOrigin());
            var albums = GroupAllMusicIntoAlbum(allmusic);

            _db.Write(() =>
            {
                var existAlbum = _db.All<AlbumItemDb>();
                _db.RemoveRange(existAlbum);
                _db.Add(albums.Select(a => a.ToDb()), update: true);
            });

            groups = GroupAllAlbumByYear(albums);
            return groups;
        }

        sealed class CustomEqComparer : EqualityComparer<MusicItemDb>
        {
            public override bool Equals(MusicItemDb x, MusicItemDb y)
            {
                if (x.Source == y.Source && x.LastWriteTime == y.LastWriteTime)
                    return true;
                return false;
            }

            public override int GetHashCode(MusicItemDb obj)
            {
                return obj.Source.GetHashCode() + obj.LastWriteTime.GetHashCode();
            }
        }

        private (IEnumerable<string> removeList, IEnumerable<FileInfo> addOrUpdateList) CheckMusicHasUpdate(List<FileInfo> files)
        {
            var currentFiles = files.Select(c => new MusicItemDb
            {
                Source = c.FullName,
                LastWriteTime = c.LastWriteTime.ToBinary()
            });
            var dbFiles = _db.All<MusicItemDb>().ToList();

            var newFiles = currentFiles.Except(dbFiles, new CustomEqComparer());
            var exc2 = newFiles.Where(d => Directory.Exists(Path.GetPathRoot(d.Source)))
                .Select(s => new FileInfo(s.Source));

            var removeFileKeys = dbFiles.Except(currentFiles, new CustomEqComparer())
                .Select(d => d.Source);

            return (removeFileKeys, exc2);
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

        private List<AlbumGroup> ScanAllMusicAndSave(List<string> libs)
        {
            var files = GetMusicFilesFromLibrary(libs);
            var allmusic = GetAllMusicItem(files);
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

        BackgroundWorker _worker;
        BackgroundWorker Worker
        {
            get
            {
                if (_worker == null)
                {
                    _worker = new BackgroundWorker
                    {
                        WorkerSupportsCancellation = true,
                        WorkerReportsProgress = true
                    };
                    _worker.DoWork += DoWork;
                    _worker.ProgressChanged += ProgressChanged;
                    _worker.RunWorkerCompleted += RunWorkerCompleted;
                }
                return _worker;
            }
        }

        BackgroundWorker _watcherWorker;
        BackgroundWorker WatcherWorker
        {
            get
            {
                if (_watcherWorker == null)
                {
                    _watcherWorker = new BackgroundWorker
                    {
                        WorkerSupportsCancellation = true,
                        WorkerReportsProgress = true
                    };
                    _watcherWorker.DoWork += WatcherDoWork;
                    _watcherWorker.ProgressChanged += WatcherProgressChanged;
                    _watcherWorker.RunWorkerCompleted += WatcherRunWorkerCompleted;
                }
                return _watcherWorker;
            }
        }

        DispatcherQueue UIQueue;

        private void WatcherProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var state = (LocalMusicState)e.ProgressPercentage;
            switch (state)
            {
                case LocalMusicState.NonFirstLoading:
                    UIQueue?.TryEnqueue(() =>
                    {
                        OnNonFirstLoadingStarted?.Invoke();
                    });
                    break;
                default:
                    break;
            }
        }

        private void WatcherRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result == null)
            {
                return;
            }
            var (albumGroup, playLists) = ((List<AlbumGroup> a, List<PlayListItem> b))e.Result;
            LocalAlbums = albumGroup;
            LocalPlayLists = playLists;
            UIQueue?.TryEnqueue(() =>
            {
                OnLoadingEnded?.Invoke();
                OnAlbumGroupChanged?.Invoke(albumGroup, playLists);
            });
        }

        private void WatcherDoWork(object sender, DoWorkEventArgs e)
        {
            WatcherWorker.ReportProgress((int)LocalMusicState.NonFirstLoading);

            _db = Realm.GetInstance(DbPath);
            var files2 = GetMusicFilesFromLibrary(Config.MusicLibrary.Select(l => l.Path).ToList());

            var (removeList, addOrUpdateList) = CheckMusicHasUpdate(files2);

            List<AlbumGroup> groups = null;
            if (addOrUpdateList != null && addOrUpdateList.Any())
            {
                groups = AddOrUpdateNewMusicAndSave(addOrUpdateList);
            }
            else if (removeList != null && removeList.Any())
            {
                groups = RemoveMusicAndSave(removeList);
            }

            var playListFiles = GetAllPlaylists();
            List<PlayListItem> playLists = ScanAllPlayList(playListFiles);

            e.Result = (groups, playLists);
            _db?.Dispose();
        }

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
            var libs = Config.MusicPlayList.Select(s => s.Path);
            List<FileInfo> files = new();
            foreach (var lib in libs)
            {
                var di = new DirectoryInfo(lib);
                if (!di.Exists) continue;
                files.AddRange(di.GetFiles("*.zpl", SearchOption.AllDirectories));
            }
            return files;
        }

        private List<PlayListItem> GetAllPlaylistItem(Realm db, List<FileInfo> files)
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
                pl.SetPlayListCover(Config);
                return pl;
            }).ToList();

            return r;
        }

        List<FileSystemWatcher> _watchers;

        private void InitFileSystemWatcher()
        {
            _watchers = Config.MusicLibrary.Select(l =>
            {
                var fsw = new FileSystemWatcher
                {
                    Path = l.Path,
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName
                };

                fsw.Created += WatcherMusicCreated;
                fsw.Renamed += WatcherMusicRenamed;
                fsw.Deleted += WatcherMusicDeleted;

                fsw.EnableRaisingEvents = true;
                return fsw;
            }).ToList();
        }

        private void WatcherMusicDeleted(object sender, FileSystemEventArgs e)
        {
            if (!WatcherWorker.IsBusy)
            {
                WatcherWorker.RunWorkerAsync();
            }
        }

        private void WatcherMusicRenamed(object sender, RenamedEventArgs e)
        {
            if (!WatcherWorker.IsBusy)
            {
                WatcherWorker.RunWorkerAsync();
            }
        }

        private void WatcherMusicCreated(object sender, FileSystemEventArgs e)
        {
            if (!WatcherWorker.IsBusy)
            {
                WatcherWorker.RunWorkerAsync();
            }
        }

        List<AlbumItem> QueryArtistAlbum(string artistName)
        {
            using var db = Realm.GetInstance(DbPath);
            var result = db.All<AlbumItemDb>().Where(a => a.AllArtists.Contains(artistName)).AsEnumerable().Select(d => d.ToOrigin()).ToList();
            return result;
        }

        public List<AlbumGroup> GetArtistAlbumGroup(string artistName)
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
            Worker?.Dispose();
            _db?.Dispose();
            //_fsw?.Dispose();
        }
    }
}
