using HotPotPlayer.Extensions;
using HotPotPlayer.Models;
using HotPotPlayer.Services.Video;
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
        public LocalVideoService(ConfigBase config) : base(config) { }

        enum LocalVideoState
        {
            Idle,
            FirstLoading,
            NonFirstLoading,
            InitLoadingComplete,
            NoLibraryAccess
        }

        public event Action<List<SingleVideoItemsGroup>, List<SeriesItem>> OnVideoChanged;
        public event Action OnFirstLoadingStarted;
        public event Action OnNonFirstLoadingStarted;
        public event Action OnLoadingEnded;
        public event Action OnNoLibraryAccess;

        static readonly List<string> SupportedExt = new() { ".mkv", ".mp4" };

        private List<FileInfo> GetVideoFilesFromLibrary()
        {
            var libs = Config.VideoLibrary;
            List<FileInfo> files = new();
            foreach (var lib in libs)
            {
                var di = new DirectoryInfo(lib.Path);
                if (!di.Exists) continue;
                files.AddRange(di.GetFiles("*.*", SearchOption.AllDirectories).Where(f => SupportedExt.Contains(f.Extension)));
            }

            return files;
        }

        BackgroundWorker localVideoBackgroundWorker;


        public void StartLoadLocalVideo()
        {
            localVideoBackgroundWorker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            localVideoBackgroundWorker.DoWork += LocalVideoBackgroundWorker_DoWork;
            localVideoBackgroundWorker.ProgressChanged += LocalVideoBackgroundWorker_ProgressChanged;
            localVideoBackgroundWorker.RunWorkerCompleted += LocalVideoBackgroundWorker_RunWorkerCompleted;
            if (localVideoBackgroundWorker.IsBusy)
            {
                return;
            }
            localVideoBackgroundWorker.RunWorkerAsync();
        }

        string GetDbPath()
        {
            var dbPath = Path.Combine(Config.DatabaseFolder, "LocalVideo.db");
            return dbPath;
        }

        string _dbPath;
        string DbFilePath
        {
            get => _dbPath ??= GetDbPath();
        }

        private void LocalVideoBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OnLoadingEnded?.Invoke();
            if (e.Result == null)
            {
                return;
            }
            var (singleVideoGroup, series) = ((List<SingleVideoItemsGroup> a, List<SeriesItem> b))e.Result;
            OnVideoChanged?.Invoke(singleVideoGroup, series);
        }

        private void LocalVideoBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var state = (LocalVideoState)e.ProgressPercentage;
            switch (state)
            {
                case LocalVideoState.Idle:
                    break;
                case LocalVideoState.FirstLoading:
                    OnFirstLoadingStarted?.Invoke();
                    break;
                case LocalVideoState.NonFirstLoading:
                    OnNonFirstLoadingStarted?.Invoke();
                    break;
                case LocalVideoState.InitLoadingComplete:
                    OnLoadingEnded?.Invoke();
                    var (singleVideoGroup, series) = ((List<SingleVideoItemsGroup> a, List<SeriesItem> b))e.UserState;
                    OnVideoChanged?.Invoke(singleVideoGroup, series);
                    break;
                case LocalVideoState.NoLibraryAccess:
                    OnNoLibraryAccess?.Invoke();
                    break;
                default:
                    break;
            }
        }


        private void LocalVideoBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //Task.Delay(200).Wait();
            var dbFilePath = DbFilePath;
            if (Config.VideoLibrary == null)
            {
                localVideoBackgroundWorker.ReportProgress((int)LocalVideoState.NoLibraryAccess);
                return;
            }

            if (File.Exists(dbFilePath))
            {
                using var _db = Realm.GetInstance(dbFilePath);
                var singleVideos_ = _db.All<SingleVideoItemsDb>().First().ToOrigin();
                var series_ = _db.All<SeriesItemDb>().AsEnumerable().Select(s => s.ToOrigin()).ToList();
                var singleVideosGroup_ = GroupSingleVideosByYear(singleVideos_);

                localVideoBackgroundWorker.ReportProgress((int)LocalVideoState.InitLoadingComplete, (singleVideosGroup_, series_));
                
                bool hasUpdate = CheckVideoLibraryHasUpdate(_db);
                if (hasUpdate)
                {
                    localVideoBackgroundWorker.ReportProgress((int)LocalVideoState.NonFirstLoading);
                }
                else
                {
                    return;
                }
            }
            else
            {
                //如果没有返回空集，并开始后台线程扫描
                localVideoBackgroundWorker.ReportProgress((int)LocalVideoState.FirstLoading);
            }

            var files = GetVideoFilesFromLibrary();
            var (singleVideos, series) = GroupVideosToSeries(files);
            var singleVideos2 = AddVideoInfo(singleVideos);
            var singleVideosGroup = GroupSingleVideosByYear(singleVideos2);

            using var _db2 = Realm.GetInstance(dbFilePath);
            _db2.Write(() =>
            {
                _db2.Add(singleVideos2.ToDb(), update: true);
                _db2.Add(series.Select(s => s.ToDb()), update: true);
            });

            e.Result = (singleVideosGroup, series);
        }

        public sealed class CustomEqComparer : EqualityComparer<VideoItemDb>
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

        private bool CheckVideoLibraryHasUpdate(Realm _db)
        {
            var files = GetVideoFilesFromLibrary();
            var (singleVideos, series) = GroupVideosToSeries(files);

            var currentFiles = singleVideos.Videos.Concat(series.SelectMany(s => s.Videos)).Select(c => c.ToDb());
            var dbFiles = _db.All<VideoItemDb>().ToList();

            var exc = currentFiles.Except(dbFiles, new CustomEqComparer());
            var exc2 = exc.Where(d => Directory.Exists(Path.GetPathRoot(d.Source)));
            return exc2.Any();
        }

        SingleVideoItems AddVideoInfo(SingleVideoItems s)
        {
            s.Videos.ForEach(f =>
            {
                using var tfile = TagLib.File.Create(f.Source.FullName);
                var tTitle = tfile.Tag.Title;
                var title = string.IsNullOrEmpty(tTitle) ? Path.GetFileNameWithoutExtension(f.Source.Name) : tTitle;

                f.Title = title;
                f.Duration = tfile.Properties.Duration;
                f.Cover = VideoInfoHelper.SaveVideoThumbnail(f.Source, Config);
            });
            return s;
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

        static (SingleVideoItems, List<SeriesItem>) GroupVideosToSeries(List<FileInfo> files)
        {
            var group = files.GroupBy(f => f.Directory.FullName);
            var singleVideos = new List<VideoItem>();
            var seriesVideos = new List<SeriesItem>();
            var series = group.Select(g =>
            {
                var series = new SeriesItem
                {
                    Source = g.First().Directory,
                    Title = g.First().Directory.Name,
                    Videos = g.Select(f => new VideoItem
                    {
                        Source = f,
                        Title = Path.GetFileNameWithoutExtension(f.Name),
                        LastWriteTime = f.LastWriteTime,
                    }).ToList()
                };
                return series;
            }).ToList();
            var detectedSeriesPath = new List<string>();
            foreach (var item in series)
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
            var s = new SingleVideoItems
            {
                Videos = singleVideos
            };
            return (s, seriesVideos);
        }
    
        static List<SingleVideoItemsGroup> GroupSingleVideosByYear(SingleVideoItems s)
        {
            var r = s.Videos.GroupBy(v => v.LastWriteTime.Year).Select(g => new SingleVideoItemsGroup
            {
                Year = g.Key,
                Items = new ObservableCollection<VideoItem>(g)
            }).ToList();
            r.Sort((a, b) => b.Year.CompareTo(a.Year));
            return r;
        }
    }
}
