using HotPotPlayer.Extensions;
using HotPotPlayer.Models;
using HotPotPlayer.Services.Video;
using Microsoft.UI.Xaml;
using Realms;
using System;
using System.Collections.Generic;
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

        public event Action<List<VideoItem>> OnVideoChanged;
        public event Action OnFirstLoadingStarted;
        public event Action OnNonFirstLoadingStarted;
        public event Action OnLoadingEnded;
        public event Action OnNoLibraryAccess;

        static readonly List<string> SupportedExt = new() { ".mkv", ".mp4" };

        private static List<FileInfo> GetVideoFilesFromLibrary(List<string> libs)
        {
            List<FileInfo> files = new();
            foreach (var lib in libs)
            {
                var di = new DirectoryInfo(lib);
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
        string DbPath
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
            var videos = (List<VideoItem>)e.Result;
            OnVideoChanged?.Invoke(videos);
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
                    var videos = (List<VideoItem>)e.UserState;
                    OnVideoChanged?.Invoke(videos);
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
            var dbPath = DbPath;
            var libs = Config.VideoLibrary;
            if (libs == null)
            {
                localVideoBackgroundWorker.ReportProgress((int)LocalVideoState.NoLibraryAccess);
                return;
            }

            if (File.Exists(dbPath))
            {
                using var _db = Realm.GetInstance(dbPath);
                var videos = _db.All<VideoItemDb>().ToList().Select(d => d.ToOrigin()).ToList();

                localVideoBackgroundWorker.ReportProgress((int)LocalVideoState.InitLoadingComplete, videos);

                var files2 = GetVideoFilesFromLibrary(libs.Select(l => l.Path).ToList());

                var hasUpdate = CheckVideoHasUpdate(files2, _db);
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

            var files = GetVideoFilesFromLibrary(libs.Select(l => l.Path).ToList());
            var videos2 = GetAllVideo(files);

            using var _db2 = Realm.GetInstance(dbPath);
            _db2.Write(() =>
            {
                _db2.RemoveAll();
                _db2.Add(videos2.Select(a => a.ToDb()));
            });

            e.Result = videos2;
        }

        List<VideoItem> GetAllVideo(IEnumerable<FileInfo> files)
        {
            var r = files.Select(f =>
            {
                using var tfile = TagLib.File.Create(f.FullName);
                var tTitle = tfile.Tag.Title;
                var title = string.IsNullOrEmpty(tTitle) ? Path.GetFileNameWithoutExtension(f.Name) : tTitle;

                var r2 = new VideoItem
                {
                    Source = f,
                    Title = title,
                    Duration = tfile.Properties.Duration,
                    Cover = VideoInfoHelper.SaveVideoThumbnail(f, Config),
                    LastWriteTime = f.LastWriteTime
                };
                return r2;
            }).ToList();
            return r;
        }

        private static bool CheckVideoHasUpdate(List<FileInfo> files, Realm db)
        {
            foreach (var f in files)
            {
                var target = db.All<VideoItemDb>().FirstOrDefault(m => m.Source == f.FullName);
                if (target == null) return true;
                if (target.LastWriteTime != f.LastWriteTime.ToBinary()) return true;
            }
            return false;
        }

        static bool IsSeries(SeriesItem s)
        {
            var all = s.Videos.Select(s => s.Source.FullName).ToList();
            var allCount = all.Select(s => s.Length).Distinct().ToList();
            if (allCount.Count > 5)
            {
                return false;
            }
            return true;
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

        static (List<VideoItem>, List<SeriesItem>) GroupVideosToSeries(List<FileInfo> files)
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
                        Title = f.Name,
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
            return (singleVideos, seriesVideos);
        }
    }
}
