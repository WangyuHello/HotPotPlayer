using CommunityToolkit.Mvvm.Collections;
using HotPotPlayer.Extensions;
using HotPotPlayer.Models;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Realms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Services
{
    public class LocalVideoService: ServiceBaseWithConfig
    {
        public LocalVideoService(ConfigBase config, DispatcherQueue uiQueue = null, AppBase app = null) : base(config, uiQueue, app) { }

        #region Property
        private LocalServiceState _state = LocalServiceState.Idle;

        public LocalServiceState State
        {
            get => _state;
            set => Set(ref _state, value);
        }

        private readonly ObservableGroupedCollection<int, VideoItem> _localVideoGroup = new();
        private ReadOnlyObservableGroupedCollection<int, VideoItem> _localVideoGroup2;

        public ReadOnlyObservableGroupedCollection<int, VideoItem> LocalVideoGroup
        {
            get => _localVideoGroup2 ??= new(_localVideoGroup);
        }

        private readonly ObservableGroupedCollection<int, SeriesItem> _localSeriesGroup = new();
        private ReadOnlyObservableGroupedCollection<int, SeriesItem> _localSeriesGroup2;

        public ReadOnlyObservableGroupedCollection<int, SeriesItem> LocalSeriesGroup
        {
            get => _localSeriesGroup2 ??= new(_localSeriesGroup);
        }

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
                    _loader.DoWork += LoadLocalVideo;
                    _loader.RunWorkerCompleted += LocalLocalComplete;
                }
                return _loader;
            }
        }


        string _dbPath;
        string DbPath => _dbPath ??= Path.Combine(Config.DatabaseFolder, "LocalVideo.db");

        List<FileSystemWatcher> _watchers;
        #endregion

        public void StartLoadLocalVideo()
        {
            if (Loader.IsBusy)
            {
                return;
            }
            Loader.RunWorkerAsync();
        }

        void EnqueueChangeState(LocalServiceState newState)
        {
            UIQueue?.TryEnqueue(() =>
            {
                State = newState;
            });
        }

        private void LocalLocalComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            EnqueueChangeState(LocalServiceState.Complete);
        }


        void LoadLocalVideo(object sender, DoWorkEventArgs e)
        {
            bool? fromWatcher = (bool?)e.Argument;

            //检查是否有缓存好的数据库
            var libs = Config.VideoLibrary;
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
            var dbVideos = db.All<SingleVideoItemsDb>();
            var dbSeries = db.All<SeriesItemDb>();

            // 转换为非数据库类型
            var dbVideosList = dbVideos.AsEnumerable().Select(d => d.ToOrigin()).ToList();
            var dbSeriesList = dbSeries.AsEnumerable().Select(d => d.ToOrigin()).ToList();

            // Video按年分组
            bool isVideosEmpty = dbVideosList.Count == 0;
            var dbVideoGroups = isVideosEmpty ? null : GroupSingleVideosByYear(dbVideosList.First());
            var dbSeriesGroups = GroupSeriesByYear(dbSeriesList);
            UIQueue?.TryEnqueue(() =>
            {
                if (!isVideosEmpty)
                {
                    foreach (var item in dbVideoGroups)
                    {
                        _localVideoGroup.AddGroup(item.Key, item);
                    }
                }
                foreach (var item in dbSeriesGroups)
                {
                    _localSeriesGroup.AddGroup(item.Key, item);
                }
                State = LocalServiceState.InitComplete;
            });

            CheckLocalUpdate:
            // 查询本地文件变动
            var localVideoFiles = Config.GetVideoFilesFromLibrary();
            var (removeList, addOrUpdateList) = CheckVideoHasUpdate(db, localVideoFiles);

            // 应用更改
            IEnumerable<IGrouping<int, VideoItem>> newVideoGroup = null;
            IEnumerable<IGrouping<int, SeriesItem>> newSeriesGroup = null;

            if (addOrUpdateList != null && addOrUpdateList.Any())
            {
                EnqueueChangeState(LocalServiceState.Loading);
                (newVideoGroup, newSeriesGroup) = AddOrUpdateVideoAndSave(db, addOrUpdateList);
            }
            if (removeList != null && removeList.Any())
            {
                EnqueueChangeState(LocalServiceState.Loading);
                (newVideoGroup, newSeriesGroup) = RemoveVideoAndSave(db, removeList);
            }

            if (newVideoGroup != null)
            {
#if DEBUG
                if (UIQueue == null)
                {
                    _localVideoGroup.Clear();
                    foreach (var item in newVideoGroup)
                    {
                        _localVideoGroup.AddGroup(item.Key, item);
                    }
                }
                else
                {
#endif
                    UIQueue.TryEnqueue(() =>
                    {
                        _localVideoGroup.Clear();
                        foreach (var item in newVideoGroup)
                        {
                            _localVideoGroup.AddGroup(item.Key, item);
                        }
                    });
#if DEBUG
                }
#endif
            };

            if (newSeriesGroup != null)
            {
#if DEBUG
                if (UIQueue == null)
                {
                    _localSeriesGroup.Clear();
                    foreach (var item in newSeriesGroup)
                    {
                        _localSeriesGroup.AddGroup(item.Key, item);
                    }
                }
                else
                {
#endif
                    UIQueue.TryEnqueue(() =>
                    {
                        _localSeriesGroup.Clear();
                        foreach (var item in newSeriesGroup)
                        {
                            _localSeriesGroup.AddGroup(item.Key, item);
                        }
                    });
#if DEBUG
                }
#endif
            };
            // 最后启动文件系统监控
            InitFileSystemWatcher();
        }

        static (IEnumerable<IGrouping<int, VideoItem>> newVideoGroup, IEnumerable<IGrouping<int, SeriesItem>> newSeriesGroup) AddOrUpdateVideoAndSave(Realm db, IEnumerable<FileInfo> addOrUpdateList)
        {
            var addOrUpdateVideo = addOrUpdateList.Select(f => f.ToVideoItem()).ToList();
            db.Write(() =>
            {
                db.Add(addOrUpdateVideo.Select(a => a.ToDb()), update: true);
            });

            var allVideo = db.All<VideoItemDb>().AsEnumerable().Select(d => d.ToOrigin());
            var (videos, series) = GroupVideosToSeries(allVideo);

            db.Write(() =>
            {
                db.Add(videos.ToDb(), update: true);
                db.Add(series.Select(a => a.ToDb()), update: true);
            });

            var videoGroup = GroupSingleVideosByYear(videos);
            var seriesGroup = GroupSeriesByYear(series);

            return (videoGroup, seriesGroup);
        }

        static (IEnumerable<IGrouping<int, VideoItem>> newVideoGroup, IEnumerable<IGrouping<int, SeriesItem>> newSeriesGroup) RemoveVideoAndSave(Realm db, IEnumerable<string> removeList)
        {
            db.Write(() =>
            {
                foreach (var item in removeList)
                {
                    db.Remove(db.Find<VideoItemDb>(item));
                }
            });

            var allVideo = db.All<VideoItemDb>().AsEnumerable().Select(d => d.ToOrigin());
            var (videos, series) = GroupVideosToSeries(allVideo);

            db.Write(() =>
            {
                var existSeries = db.All<SeriesItemDb>();
                db.RemoveRange(existSeries);
                db.Add(series.Select(a => a.ToDb()), true);
                db.Add(videos.ToDb(), true);
            });

            var videoGroup = GroupSingleVideosByYear(videos);
            var seriesGroup = GroupSeriesByYear(series);

            return (videoGroup, seriesGroup);
        }

        public sealed class VideoItemDbEqComparer : EqualityComparer<VideoItemDb>
        {
            public override bool Equals(VideoItemDb x, VideoItemDb y)
            {
                if (x.Source == y.Source && x.LastWriteTime == y.LastWriteTime)
                    return true;
                return false;
            }

            public override int GetHashCode(VideoItemDb obj)
            {
                return obj.Source.GetHashCode() + obj.LastWriteTime.GetHashCode();
            }
        }

        static (IEnumerable<string> removeList, IEnumerable<FileInfo> addOrUpdateList) CheckVideoHasUpdate(Realm db, List<FileInfo> files)
        {
            var currentFiles = files.Select(c => new VideoItemDb
            {
                Source = c.FullName,
                LastWriteTime = c.LastWriteTime.ToBinary()
            });
            var dbFiles = db.All<VideoItemDb>().ToList();

            var newFiles = currentFiles.Except(dbFiles, new VideoItemDbEqComparer());
            var exc2 = newFiles.Where(d => Directory.Exists(Path.GetPathRoot(d.Source)))
                .Select(s => new FileInfo(s.Source));

            var removeFileKeys = dbFiles.Except(currentFiles, new VideoItemDbEqComparer())
                .Select(d => d.Source);

            return (removeFileKeys, exc2);
        }

        static bool IsSeries(SeriesItem s)
        {
            var sign = s.Source.GetFiles("*.series");
            return sign.Any();
            //var all = s.Videos.Select(s => s.Source.FullName).ToList();
            //var allCount = all.Select(s => s.Length).Distinct().ToList();
            //var th = (int)(all.Count * 0.2);
            //if (allCount.Count > th)
            //{
            //    return false;
            //}
            //foreach (var item in s.Videos)
            //{
            //    var dir = item.Source.Directory.Name;
            //    var name = item.Source.Name;
            //    var inter = new string(dir.Intersect(name).ToArray());
            //    if (name.Contains(inter)) return true;
            //}
            //return true;
        }

        static bool HasDetected(string path, List<string> detectPath)
        {
            foreach (var item in detectPath)
            {
                if (path.Contains(item))
                {
                    return true;
                }
            }
            return false;
        }

        static (SingleVideoItems single, List<SeriesItem> series) GroupVideosToSeries(IEnumerable<VideoItem> files)
        {
            var group = files.GroupBy(f => f.Source.Directory.FullName);
            var singleVideos = new List<VideoItem>();
            var seriesVideos = new List<SeriesItem>();
            var rawSeriesGroupByDirectory = group.Select(g =>
            {
                return new SeriesItem
                {
                    Source = g.First().Source.Directory,
                    Title = g.First().Source.Directory.Name,
                    Cover = g.First().Cover,
                    Videos = g.ToList()
                };
            }).ToList();

            var detectedSeriesPath = new List<string>();
            foreach (var item in rawSeriesGroupByDirectory)
            {
                if (HasDetected(item.Source.FullName, detectedSeriesPath))
                {
                    continue;
                }
                else if (IsSeries(item))
                {
                    seriesVideos.Add(item);
                    detectedSeriesPath.Add(item.Source.FullName);
                }
                else
                {
                    singleVideos.AddRange(item.Videos);
                }
            }
            var single = new SingleVideoItems
            {
                Videos = singleVideos
            };
            return (single, seriesVideos);
        }
    
        static IEnumerable<IGrouping<int, VideoItem>> GroupSingleVideosByYear(SingleVideoItems s)
        {
            var r = s.Videos.GroupBy(m => m.Source.LastWriteTime.Year).OrderByDescending(g => g.Key);
            return r;
        }

        static IEnumerable<IGrouping<int, SeriesItem>> GroupSeriesByYear(List<SeriesItem> s)
        {
            var r = s.GroupBy(m => m.Year).OrderByDescending(g => g.Key);
            return r;
        }

        #region FileSystemWatcher
        private void InitFileSystemWatcher()
        {
            _watchers ??= Config.VideoLibrary.Select(l =>
            {
                var fsw = new FileSystemWatcher
                {
                    Path = l.Path,
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName
                };

                fsw.Created += WatcherVideoCreated;
                fsw.Renamed += WatcherVideoRenamed;
                fsw.Deleted += WatcherVideoDeleted;

                fsw.EnableRaisingEvents = true;
                return fsw;
            }).ToList();
        }

        private void WatcherVideoDeleted(object sender, FileSystemEventArgs e)
        {
            if (!Loader.IsBusy)
            {
                Loader.RunWorkerAsync(true);
            }
        }

        private void WatcherVideoRenamed(object sender, RenamedEventArgs e)
        {
            if (!Loader.IsBusy)
            {
                Loader.RunWorkerAsync(true);
            }
        }

        private void WatcherVideoCreated(object sender, FileSystemEventArgs e)
        {
            if (!Loader.IsBusy)
            {
                Loader.RunWorkerAsync(true);
            }
        }
        #endregion
    }
}
