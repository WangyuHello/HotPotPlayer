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
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HotPotPlayer.Services
{
    public class LocalMusicService: ServiceBaseWithConfig
    {
        public LocalMusicService(ConfigBase config, DispatcherQueue uiQueue = null, AppBase app = null) : base(config, uiQueue, app) { }
        
        #region State
        private LocalServiceState _state = LocalServiceState.Idle;

        public LocalServiceState State
        {
            get => _state;
            set => Set(ref _state, value);
        }

        #endregion
        #region Property
        private ObservableGroupedCollection<int, AlbumItem> _localAlbumGroup = new();
        private ReadOnlyObservableGroupedCollection<int, AlbumItem> _localAlbumGroup2;

        public ReadOnlyObservableGroupedCollection<int, AlbumItem> LocalAlbumGroup
        {
            get => _localAlbumGroup2 ??= new(_localAlbumGroup);
        }

        private ObservableCollection<PlayListItem> _localPlayListList;

        public ObservableCollection<PlayListItem> LocalPlayListList
        {
            get => _localPlayListList;
            set => Set(ref _localPlayListList, value);
        }
        #endregion
        #region Field

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
                    AllArtists = allArtists,
                    Cover = g2.First().Cover,
                };
                foreach (var item in i.MusicItems)
                {
                    item.AlbumRef = i;
                    item.Cover = i.Cover;
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
            EnqueueChangeState(LocalServiceState.Complete);
        }

        void EnqueueChangeState(LocalServiceState newState)
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
                EnqueueChangeState(LocalServiceState.NoLibraryAccess);
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
            var dbAlbumList = dbAlbums.AsEnumerable().Select(d => d.ToOrigin()).ToList();
            var dbPlayListList = dbPlayLists.AsEnumerable().Select(d => d.ToOrigin()).ToList();
            SetPlayListRef(dbPlayListList);

            // Album分组
            var dbAlbumGroups = GroupAllAlbumByYear(dbAlbumList);
            UIQueue?.TryEnqueue(() =>
            {
                foreach (var item in dbAlbumGroups)
                {
                    _localAlbumGroup.AddGroup(item.Key, item);
                }
                LocalPlayListList = new(dbPlayListList);
                State = LocalServiceState.InitComplete;
            });

        CheckLocalUpdate:
            // 查询本地文件变动
            var localMusicFiles = Config.GetMusicFilesFromLibrary();
            var localPlayListFiles = Config.GetPlaylistFilesFromLibrary();
            var (removeList, addOrUpdateList) = CheckMusicHasUpdate(db, localMusicFiles);
            var playListHasUpdate = CheckPlayListHasUpdate(db, localPlayListFiles);

            // 应用更改
            IEnumerable<IGrouping<int, AlbumItem>> newAlbumGroup = null;
            ObservableCollection<PlayListItem> newPlayListList = null;
            if (addOrUpdateList != null && addOrUpdateList.Any())
            {
                EnqueueChangeState(LocalServiceState.Loading);
                newAlbumGroup = AddOrUpdateMusicAndSave(db, addOrUpdateList);
            }
            if (removeList != null && removeList.Any())
            {
                EnqueueChangeState(LocalServiceState.Loading);
                newAlbumGroup = RemoveMusicAndSave(db, removeList);
            }
            if (playListHasUpdate)
            {
                EnqueueChangeState(LocalServiceState.Loading);
                var l = ScanAllPlayList(db, localPlayListFiles);
                SetPlayListRef(l);
                newPlayListList = new(l);
            }

            if (newAlbumGroup != null)
            {
#if DEBUG
                if (UIQueue == null)
                {
                    _localAlbumGroup.Clear();
                    foreach (var item in newAlbumGroup)
                    {
                        _localAlbumGroup.AddGroup(item.Key, item);
                    }
                }
                else
                {
#endif
                    UIQueue.TryEnqueue(() =>
                    {
                        _localAlbumGroup.Clear();
                        foreach (var item in newAlbumGroup)
                        {
                            _localAlbumGroup.AddGroup(item.Key, item);
                        }
                    });
#if DEBUG
                }
#endif
            };
            if (newPlayListList != null)
            {
#if DEBUG
                if (UIQueue == null)
                {
                    LocalPlayListList = newPlayListList;
                }
                else
                {
#endif
                    UIQueue?.TryEnqueue(() =>
                    {
                        LocalPlayListList = newPlayListList;
                    });
#if DEBUG
                }
#endif
            }

            // 最后启动文件系统监控
            InitFileSystemWatcher();
        }

        private void SetPlayListRef(List<PlayListItem> dbPlayListList)
        {
            foreach (var item in dbPlayListList)
            {
                foreach (var i2 in item.MusicItems)
                {
                    i2.PlayListRef = item;
                }
            }
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

            var newFiles = currentFiles.Except(dbFiles, new MusicItemDbComparer());
            var exc2 = newFiles.Where(d => Directory.Exists(Path.GetPathRoot(d.Source)))
                .Select(s => new FileInfo(s.Source));

            var removeFileKeys = dbFiles.Except(currentFiles, new MusicItemDbComparer())
                .Select(d => d.Source);

            return (removeFileKeys, exc2);
        }

        private static List<PlayListItem> ScanAllPlayList(Realm db, List<FileInfo> playListsFile)
        {
            var playLists = GetAllPlaylistItem(db, playListsFile).ToList();
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

        private static IEnumerable<PlayListItem> GetAllPlaylistItem(Realm db, List<FileInfo> files)
        {
            var r = files.Select(f => new PlayListItem(db, f));
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
                fsw.Changed += WatcherMusicChanged;

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
            var createFile = e.FullPath;
            if (_fileSystemWatcherFilter !=null && _fileSystemWatcherFilter.Contains(createFile))
            {
                _fileSystemWatcherFilter.Remove(createFile);
                return;
            }
            if (!Loader.IsBusy)
            {
                Loader.RunWorkerAsync(true);
            }
        }

        private void WatcherMusicChanged(object sender, FileSystemEventArgs e)
        {
            var createFile = e.FullPath;
            if (_fileSystemWatcherFilter != null && _fileSystemWatcherFilter.Contains(createFile))
            {
                _fileSystemWatcherFilter.Remove(createFile);
                return;
            }
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

        public void AddAlbumToPlayList(string playList, AlbumItem album)
        {
            var plist = LocalPlayListList.First(p => p.Title == playList);
            var plistIndex = LocalPlayListList.IndexOf(plist);
            foreach (var music in album.MusicItems)
            {
                var music2 = music with { PlayListRef = plist };
                plist.AddMusic(music2);
            }
            LocalPlayListList[plistIndex] = plist;
            Task.Run(async () =>
            {
                _fileSystemWatcherFilter = new List<string>()
                {
                    plist.Source.FullName
                };
                await plist.WriteAsync();
                using var db = Realm.GetInstance(DbPath);
                db.Write(() =>
                {
                    db.Add(plist.ToDb(), update: true);
                });
            });
        }

        public void AddMusicToPlayList(string playList, MusicItem music)
        {
            var plist = LocalPlayListList.First(p => p.Title == playList);
            var plistIndex = LocalPlayListList.IndexOf(plist);
            var music2 = music with { PlayListRef = plist };
            if (!plist.AddMusic(music2))
            {
                App?.ShowToast(new ToastInfo { Text = $"{music.Title} 已存在于 {playList}" });
                return;
            }
            LocalPlayListList[plistIndex] = plist;
            Task.Run(async () =>
            {
                _fileSystemWatcherFilter = new List<string>()
                {
                    plist.Source.FullName
                };
                await plist.WriteAsync();
                using var db = Realm.GetInstance(DbPath);
                db.Write(() =>
                {
                    db.Add(plist.ToDb(), update: true);
                });

                UIQueue.TryEnqueue(() =>
                {
                    App?.ShowToast(new ToastInfo { Text = $"已将 {music.Title} 添加到 {playList}" });
                });
            });
        }

        public void PlayListMusicDelete(MusicItem music)
        {
            if (music.PlayListRef == null)
            {
                return;
            }
            var playList = music.PlayListRef.Title;
            var plist = LocalPlayListList.First(p => p.Title == playList);
            var plistIndex = LocalPlayListList.IndexOf(plist);
            plist.DeleteMusic(music);
            LocalPlayListList[plistIndex] = plist;
            Task.Run(async () =>
            {
                _fileSystemWatcherFilter = new List<string>()
                {
                    plist.Source.FullName
                };
                await plist.WriteAsync();
                using var db = Realm.GetInstance(DbPath);
                db.Write(() =>
                {
                    db.Add(plist.ToDb(), update: true);
                });

                UIQueue.TryEnqueue(() =>
                {
                    App?.ShowToast(new ToastInfo { Text = $"已将 {music.Title} 从 {playList} 删除" });
                });
            });
        }

        public void PlayListMusicUp(MusicItem music)
        {
            if (music.PlayListRef == null)
            {
                return;
            }
            var playList = music.PlayListRef.Title;
            var plist = LocalPlayListList.First(p => p.Title == playList);
            var plistIndex = LocalPlayListList.IndexOf(plist);
            plist.UpMusic(music);
            LocalPlayListList[plistIndex] = plist;
            Task.Run(async () =>
            {
                _fileSystemWatcherFilter = new List<string>()
                {
                    plist.Source.FullName
                };
                await plist.WriteAsync();
                using var db = Realm.GetInstance(DbPath);
                db.Write(() =>
                {
                    db.Add(plist.ToDb(), update: true);
                });
            });
        }

        public void PlayListMusicDown(MusicItem music)
        {
            if (music.PlayListRef == null)
            {
                return;
            }
            var playList = music.PlayListRef.Title;
            var plist = LocalPlayListList.First(p => p.Title == playList);
            var plistIndex = LocalPlayListList.IndexOf(plist);
            plist.DownMusic(music);
            LocalPlayListList[plistIndex] = plist;
            Task.Run(async () =>
            {
                _fileSystemWatcherFilter = new List<string>()
                {
                    plist.Source.FullName
                };
                await plist.WriteAsync();
                using var db = Realm.GetInstance(DbPath);
                db.Write(() =>
                {
                    db.Add(plist.ToDb(), update: true);
                });
            });
        }

        List<string> _fileSystemWatcherFilter;

        public void NewPlayList(string title, MusicItem initItem)
        {
            var cont = LocalPlayListList.FirstOrDefault(p => p.Title == title) != null;
            if (cont)
            {
                App?.ShowToast(new ToastInfo { Text = $"已存在 {title}" });
                return;
            }
            Task.Run(async () =>
            {
                using var db = Realm.GetInstance(DbPath);
                var pl = PlayListItem.Create(title, Config.MusicPlayListDirectory.First().Path, db);
                pl.AddMusic(initItem);
                _fileSystemWatcherFilter = new List<string>()
                {
                    pl.Source.FullName
                };
                await pl.WriteAsync();
                db.Write(() =>
                {
                    db.Add(pl.ToDb(), update: true);
                });
                UIQueue.TryEnqueue(() =>
                {
                    LocalPlayListList.Add(pl);
                    App?.ShowToast(new ToastInfo { Text = $"已将 {initItem.Title} 添加到 {title}" });
                });
            });
        }

        public override void Dispose()
        {
            Loader?.Dispose();
            _watchers?.ForEach(fsw => fsw.Dispose());
        }
    }
}
