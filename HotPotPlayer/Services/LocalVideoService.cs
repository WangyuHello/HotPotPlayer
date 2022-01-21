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
    internal class LocalVideoService
    {
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

        static List<string> GetVideoLibrary => ((App)Application.Current).VideoLibrary;
        static readonly List<string> SupportedExt = new() { ".mkv", ".mp4" };

        private static List<FileInfo> GetVideoFilesFromLibrary(List<string> libs)
        {
            List<FileInfo> files = new();
            foreach (var lib in libs)
            {
                var di = new DirectoryInfo(lib);
                files.AddRange(di.GetFiles("*.*", SearchOption.AllDirectories).Where(f => SupportedExt.Contains(f.Extension)));
            }

            return files;
        }

        BackgroundWorker localVideoBackgroundWorker;


        internal void StartLoadLocalVideo()
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

        static string GetDbPath()
        {
            var baseDir = ((App)Application.Current).LocalFolder;
            var dbDir = Path.Combine(baseDir, "Db");
            if (!Directory.Exists(dbDir)) { Directory.CreateDirectory(dbDir); }
            var dbPath = Path.Combine(dbDir, "LocalVideo.db");
            return dbPath;
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
            var dbPath = GetDbPath();
            var libs = GetVideoLibrary;
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

                var localDbList = _db.All<VideoItemDb>().ToList().Select(m => m.File);
                var files2 = GetVideoFilesFromLibrary(libs).Select(f => f.FullName);

                var newFiles = files2.Except(localDbList);
                var delFiles = localDbList.Except(files2);
                if (newFiles.Any() || delFiles.Any())
                {
                    //如果有更新，开始后台线程扫描
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

            var files = GetVideoFilesFromLibrary(libs);
            var videos2 = GetAllVideo(files);

            using var _db2 = Realm.GetInstance(dbPath);
            _db2.Write(() =>
            {
                _db2.RemoveAll();
                _db2.Add(videos2.Select(a => a.ToDb()));
            });

            e.Result = videos2;
        }

        static List<VideoItem> GetAllVideo(IEnumerable<FileInfo> files)
        {
            var r = files.Select(f =>
            {
                using var tfile = TagLib.File.Create(f.FullName);
                var tTitle = tfile.Tag.Title;
                var title = string.IsNullOrEmpty(tTitle) ? Path.GetFileNameWithoutExtension(f.Name) : tTitle;

                var r2 = new VideoItem
                {
                    Title = title,
                    Duration = tfile.Properties.Duration,
                    Source = f.FullName,
                    File = f,
                    Cover = VideoInfoHelper.SaveVideoThumbnail(f)
                };
                return r2;
            }).ToList();
            return r;
        }
    }
}
