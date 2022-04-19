using CommunityToolkit.Common.Collections;
using HotPotPlayer.Extensions;
using HotPotPlayer.Models;
using Microsoft.UI.Dispatching;
using Realms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace HotPotPlayer.Services
{
    public class LocalMusicService: ServiceBaseWithConfig
    {
        public LocalMusicService(ConfigBase config, DispatcherQueue uiQueue = null) : base(config, uiQueue) { }
        
        #region State
        public enum LocalMusicState
        {
            Idle,
            FirstLoading,
            NonFirstLoading,
            InitComplete,
            Complete,
            NoLibraryAccess
        }

        private LocalMusicState _state = LocalMusicState.Idle;

        public LocalMusicState State
        {
            get => _state;
            set => Set(ref _state, value);
        }

        #endregion
        #region Property
        private ObservableGroupedCollection<int, AlbumItem> _localAlbumGroup;
        private ReadOnlyObservableGroupedCollection<int, AlbumItem> _localAlbumGroup2;

        public ReadOnlyObservableGroupedCollection<int, AlbumItem> LocalAlbumGroup
        {
            get => _localAlbumGroup2;
            set => Set(ref _localAlbumGroup2, value);
        }

        private ObservableCollection<PlayListItem> _localPlayListList;

        public ObservableCollection<PlayListItem> LocalPlayListList
        {
            get => _localPlayListList;
            set => Set(ref _localPlayListList, value);
        }
        #endregion
        #region Field
        public static readonly string[] SupportedExt = new[] { ".flac", ".wav", ".m4a", ".mp3" };

        BackgroundWorker _loader;
        BackgroundWorker Loader
        {
            get
            {
                if (_loader == null)
                {
                    _loader = new BackgroundWorker
                    {
                        WorkerSupportsCancellation = true,
                    };
                    _loader.DoWork += LoadLocalMusic;
                    _loader.RunWorkerCompleted += LoadLocalCompleted;
                }
                return _loader;
            }
        }

        string _dbPath;
        string DbPath => _dbPath ??= Path.Combine(Config.DatabaseFolder, "LocalMusic.db");


        List<FileSystemWatcher> _watchers;
        #endregion

        private List<FileInfo> GetMusicFilesFromLibrary()
        {
            var libs = Config.MusicLibrary.Select(s => s.Path);
            List<FileInfo> files = new();
            foreach (var lib in libs)
            {
                var di = new DirectoryInfo(lib);
                files.AddRange(di.GetFiles("*.*", SearchOption.AllDirectories).Where(f => SupportedExt.Contains(f.Extension)));
            }

            return files;
        }

        static IEnumerable<AlbumItem> GroupAllMusicIntoAlbum(IEnumerable<MusicItem> allMusic)
        {
            var albums = allMusic.GroupBy(m2 => m2.AlbumSignature).Select(g2 => 
            {
                var music = g2.ToList();
                music.Sort((a,b) => a.DiscTrack.CompareTo(b.DiscTrack));

                var albumArtists = g2.First().AlbumArtists.Length == 0 ? g2.First().Artists : g2.First().AlbumArtists;
                var allArtists = g2.SelectMany(m => m.Artists).Concat(albumArtists).Distinct().ToArray();

                var i = new AlbumItem
                {
                    Title = g2.First().Album,
                    Artists = albumArtists,
                    Year = g2.First().Year,
                    MusicItems = music,
                    AllArtists = allArtists
                };
                foreach (var item in i.MusicItems)
                {
                    item.AlbumRef = i;
                }
                return i;
            });
            return albums;
        }

        static IEnumerable<IGrouping<int, AlbumItem>> GroupAllAlbumByYear(IEnumerable<AlbumItem> albums)
        {
            var r = albums.GroupBy(m => m.Year).OrderByDescending(g => g.Key);
            return r;
        }

        /// <summary>
        /// 启动加载本地音乐
        /// </summary>
        public void StartLoadLocalMusic()
        {
            if (Loader.IsBusy)
            {
                return;
            }
            Loader.RunWorkerAsync();
        }

        void LoadLocalCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            State = LocalMusicState.Complete;
        }

        void EnqueueChangeState(LocalMusicState newState)
        {
            UIQueue?.TryEnqueue(() =>
            {
                State = newState;
            });
        }

        void LoadLocalMusic(object sender, DoWorkEventArgs e)
        {
            bool? fromWatcher = (bool?)e.Argument;
            //检查是否有缓存好的数据库
            var libs = Config.MusicLibrary;
            if (libs == null)
            {
                EnqueueChangeState(LocalMusicState.NoLibraryAccess);
                return;
            }

            // 获取DB，如果没有其会自动创建
            using var db = Realm.GetInstance(DbPath);

            if (fromWatcher != null && (bool)fromWatcher)
            {
                goto CheckLocalUpdate;
            }
            // 获取DB内容
            var dbAlbums = db.All<AlbumItemDb>();
            var dbPlayLists = db.All<PlayListItemDb>();

            // 转换为非数据库类型
            var dbAlbumList_ = dbAlbums.AsEnumerable().Select(d => d.ToOrigin()).ToList();
            var dbPlayListList = dbPlayLists.AsEnumerable().Select(d => d.ToOrigin()).ToList();

            // Album分组
            var dbAlbumGroups = GroupAllAlbumByYear(dbAlbumList_);
            _localAlbumGroup = new(dbAlbumGroups);
            UIQueue?.TryEnqueue(() =>
            {
                LocalAlbumGroup = new(_localAlbumGroup);
                LocalPlayListList = new(dbPlayListList);
                State = LocalMusicState.InitComplete;
            });

            CheckLocalUpdate:
            // 查询本地文件变动
            var localMusicFiles = GetMusicFilesFromLibrary();
            var localPlayListFiles = GetPlaylistFilesFromLibrary();
            var (removeList, addOrUpdateList) = CheckMusicHasUpdate(db, localMusicFiles);
            var playListHasUpdate = CheckPlayListHasUpdate(db, localPlayListFiles);

            // 应用更改
            ObservableGroupedCollection<int, AlbumItem> newAlbumGroup = null;
            ObservableCollection<PlayListItem> newPlayListList = null;
            if (addOrUpdateList != null && addOrUpdateList.Any())
            {
                newAlbumGroup = new(AddOrUpdateMusicAndSave(db, addOrUpdateList));
            }
            if (removeList != null && removeList.Any())
            {
                newAlbumGroup = new(RemoveMusicAndSave(db, removeList));
            }
            if (playListHasUpdate)
            {
                newPlayListList = new(ScanAllPlayList(db, localPlayListFiles));
            }

            if (newAlbumGroup != null)
            {
                _localAlbumGroup = newAlbumGroup;
                UIQueue?.TryEnqueue(() =>
                {
                    LocalAlbumGroup = new(_localAlbumGroup);
                });
            };
            if (newPlayListList != null)
            {
                UIQueue?.TryEnqueue(() =>
                {
                    LocalPlayListList = newPlayListList;
                });
            }

            // 最后启动文件系统监控
            InitFileSystemWatcher();
        }

        private static IEnumerable<IGrouping<int, AlbumItem>> RemoveMusicAndSave(Realm db, IEnumerable<string> removeList)
        {
            db.Write(() =>
            {
                foreach (var item in removeList)
                {
                    db.Remove(db.Find<MusicItemDb>(item));
                }
            });

            var allmusic = db.All<MusicItemDb>().AsEnumerable().Select(d => d.ToOrigin());
            var albums = GroupAllMusicIntoAlbum(allmusic);

            db.Write(() =>
            {
                var existAlbum = db.All<AlbumItemDb>();
                var existMusic = db.All<MusicItemDb>();
                db.RemoveRange(existAlbum);
                db.RemoveRange(existMusic);
                db.Add(albums.Select(a => a.ToDb()));
            });

            var groups = GroupAllAlbumByYear(albums);
            return groups;
        }

        private static IEnumerable<IGrouping<int, AlbumItem>> AddOrUpdateMusicAndSave(Realm db, IEnumerable<FileInfo> addOrUpdateList)
        {
            var addOrUpdateMusic = addOrUpdateList.Select(f => f.ToMusicItem()).ToList();
            db.Write(() =>
            {
                db.Add(addOrUpdateMusic.Select(a => a.ToDb()), update: true);
            });

            var allmusic = db.All<MusicItemDb>().AsEnumerable().Select(d => d.ToOrigin());
            var albums = GroupAllMusicIntoAlbum(allmusic).ToList();

            db.Write(() =>
            {
                var existAlbum = db.All<AlbumItemDb>();
                db.RemoveRange(existAlbum);
                db.Add(albums.Select(a => a.ToDb()), update: true);
            });

            var groups = GroupAllAlbumByYear(albums);
            return groups;
        }

        sealed class MusicItemComparer : EqualityComparer<MusicItemDb>
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

        sealed class PlayListItemComparer : EqualityComparer<PlayListItemDb>
        {
            public override bool Equals(PlayListItemDb x, PlayListItemDb y)
            {
                if (x.Source == y.Source && x.LastWriteTime == y.LastWriteTime)
                    return true;
                return false;
            }

            public override int GetHashCode(PlayListItemDb obj)
            {
                return obj.Source.GetHashCode() + obj.LastWriteTime.GetHashCode();
            }
        }

        private static (IEnumerable<string> removeList, IEnumerable<FileInfo> addOrUpdateList) CheckMusicHasUpdate(Realm db, List<FileInfo> files)
        {
            var currentFiles = files.Select(c => new MusicItemDb
            {
                Source = c.FullName,
                LastWriteTime = c.LastWriteTime.ToBinary()
            });
            var dbFiles = db.All<MusicItemDb>().ToList();

            var newFiles = currentFiles.Except(dbFiles, new MusicItemComparer());
            var exc2 = newFiles.Where(d => Directory.Exists(Path.GetPathRoot(d.Source)))
                .Select(s => new FileInfo(s.Source));

            var removeFileKeys = dbFiles.Except(currentFiles, new MusicItemComparer())
                .Select(d => d.Source);

            return (removeFileKeys, exc2);
        }

        private List<PlayListItem> ScanAllPlayList(Realm db, List<FileInfo> playListsFile)
        {
            var playLists = GetAllPlaylistItem(db, playListsFile);
            db.Write(() =>
            {
                var exist = db.All<PlayListItemDb>();
                db.RemoveRange(exist);
                db.Add(playLists.Select(a => a.ToDb()), update: true);
            });
            return playLists;
        }


        private static bool CheckPlayListHasUpdate(Realm db, List<FileInfo> current)
        {
            var dbFiles = db.All<PlayListItemDb>().ToList();
            var currentFiles = current.Select(c => new PlayListItemDb
            {
                Source = c.FullName,
                LastWriteTime = c.LastWriteTime.ToBinary()
            });

            var newFiles = currentFiles.Except(dbFiles, new PlayListItemComparer());
            var removeFiles = dbFiles.Except(currentFiles, new PlayListItemComparer());

            return newFiles.Any() || removeFiles.Any();
        }

        private List<FileInfo> GetPlaylistFilesFromLibrary()
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
                //pl.SetPlayListCover(Config);
                return pl;
            }).ToList();

            return r;
        }

        #region FileSystemWatcher
        private void InitFileSystemWatcher()
        {
            _watchers ??= Config.MusicLibrary.Select(l =>
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
            if (!Loader.IsBusy)
            {
                Loader.RunWorkerAsync(true);
            }
        }

        private void WatcherMusicRenamed(object sender, RenamedEventArgs e)
        {
            if (!Loader.IsBusy)
            {
                Loader.RunWorkerAsync(true);
            }
        }

        private void WatcherMusicCreated(object sender, FileSystemEventArgs e)
        {
            if (!Loader.IsBusy)
            {
                Loader.RunWorkerAsync(true);
            }
        }
        #endregion

        List<AlbumItem> QueryArtistAlbum(string artistName)
        {
            using var db = Realm.GetInstance(DbPath);
            var result = db.All<AlbumItemDb>().Where(a => a.AllArtists.Contains(artistName)).AsEnumerable().Select(d => d.ToOrigin()).ToList();
            return result;
        }

        public (IEnumerable<IGrouping<int, AlbumItem>>, List<MusicItem>) GetArtistAlbumGroup(string artistName)
        {
            var album = QueryArtistAlbum(artistName);
            var group = GroupAllAlbumByYear(album);
            var music = album.SelectMany(a => a.MusicItems).ToList();
            return (group, music);
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
            Loader?.Dispose();
            _watchers?.ForEach(fsw => fsw.Dispose());
        }
    }
}
