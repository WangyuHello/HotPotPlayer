using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Helpers;
using HotPotPlayer.Models;
using HotPotPlayer.Models.BiliBili;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.UI.Dispatching;
using Mpv.NET.API;
using Mpv.NET.Player;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Windows.Media;
using Windows.Storage.Streams;

namespace HotPotPlayer.Services
{
    public partial class PlayerService : ServiceBaseWithConfig
    {

        protected readonly System.Timers.Timer _playerTimer;
        protected MpvPlayer _mpv;
        private readonly BackgroundWorker _playerStarter;
        bool _isMediaSwitching;

        public PlayerService(ConfigBase config, DispatcherQueue uiThread = null, AppBase app = null) : base(config, uiThread, app)
        {
            _playerStarter = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };

            _playerStarter.RunWorkerCompleted += PlayerStarterCompleted;
            _playerStarter.DoWork += PlayerStarterDoWork;
            _playerTimer = new System.Timers.Timer(500)
            {
                AutoReset = true
            };
            _playerTimer.Elapsed += PlayerTimerElapsed;

            Config.SaveConfigWhenExit("Volume", () => (Volume != 0, Volume));
        }

        [ObservableProperty]
        public partial PlayerState State { get; set; }

        [ObservableProperty]
        public partial BaseItemDto CurrentPlaying { get; set; }

        public TimeSpan? CurrentPlayingDuration
        {
            get => _mpv?.Duration;
        }

        private int currentPlayingIndex = -1;
        public int CurrentPlayingIndex
        {
            get => currentPlayingIndex;
            set => SetProperty(ref currentPlayingIndex, value);
        }

        [ObservableProperty]
        public partial ObservableCollection<BaseItemDto> CurrentPlayList { get; set; }

        public bool SuppressCurrentTimeTrigger { get; set; }

        private TimeSpan _currentTime;

        public TimeSpan CurrentTime
        {
            get => _currentTime;
            set
            {
                if (SuppressCurrentTimeTrigger) return;
                SetProperty(ref _currentTime, value);
            }
        }

        [ObservableProperty]
        public partial bool IsPlaying { get; set; }

        partial void OnIsPlayingChanged(bool value)
        {
            var nowPlaying = value;
            Task.Run(async () =>
            {
                CustomPlayOrPause(nowPlaying);
                if (nowPlaying)
                {
                    App.SetSmtcStatus(MediaPlaybackStatus.Playing);
                }
                else
                {
                    App.SetSmtcStatus(MediaPlaybackStatus.Paused);
                    await App.JellyfinMusicService.ReportProgress(CurrentPlaying, CurrentTime.Ticks, true);
                }
            });
        }

        protected virtual void CustomPlayOrPause(bool playing) { }

        [ObservableProperty]
        public partial bool HasError { get; set; }

        public int Volume
        {
            get
            {
                if (_mpv == null)
                {
                    var volume = Config.GetConfig<int?>("Volume");
                    if (volume != null)
                    {
                        return volume.Value;
                    }
                    Config.SetConfig("Volume", 50);
                    return 50;
                }
                else
                {
                    return _mpv.Volume;
                }
            }
            set
            {
                if (value != _mpv.Volume)
                {
                    _mpv?.Volume = value;
                    OnPropertyChanged(nameof(Volume));
                }
            }
        }

        [ObservableProperty]
        public partial PlayMode PlayMode { get; set; }

        public virtual void PlayNext(BaseItemDto video) { }

        public void PlayNext(List<BaseItemDto> files, int index)
        {
            if (CurrentPlaying != null && CurrentPlaying.Id != null)
            {
                App.JellyfinMusicService.ReportStop(CurrentPlaying, CurrentTime.Ticks);
            }
            CurrentPlayList = [.. files];
            PlayNext(index);
        }

        public void PlayNext(List<FileInfo> files, int index)
        {
            var fakeDtos = files.Select(f => new BaseItemDto
            {
                Path = f.FullName,
                Name = f.Name
            });
            CurrentPlayList = [.. fakeDtos];
            PlayNext(index);
        }

        public void PlayNextInCurrentList(BaseItemDto v)
        {
            if (CurrentPlaying != null && CurrentPlaying.Id != null)
            {
                App.JellyfinMusicService.ReportStop(CurrentPlaying, CurrentTime.Ticks);
            }
            var i = CurrentPlayList.IndexOf(v);
            if (i != CurrentPlayingIndex)
            {
                PlayNext(i);
            }
        }

        protected virtual void OnPlayNextStateChange(int? index) { }
        protected virtual void BeforePlayerStarter() { }

        /// <summary>
        /// 主方法
        /// </summary>
        /// <param name="index"></param>
        public void PlayNext(int? index)
        {
            if (index == null)
            {
                return;
            }
            if (CurrentPlayList == null || CurrentPlayList.Count == 0)
            {
                return;
            }
            if ((index >= 0) && (index < CurrentPlayList.Count))
            {
                if (_playerStarter.IsBusy)
                {
                    return;
                }
                _playerTimer.Stop();
                CurrentTime = TimeSpan.Zero;
                IsPlaying = false;

                OnPlayNextStateChange(index);
                State = PlayerState.Loading;

                BeforePlayerStarter();
                _playerStarter.RunWorkerAsync(index);
            }
        }

        Random random;
        public void PlayNextInPlayList()
        {
            if (CurrentPlayingIndex != -1)
            {
                if (PlayMode == PlayMode.Loop)
                {
                    var index = CurrentPlayingIndex + 1;
                    if (index >= CurrentPlayList.Count)
                    {
                        index = 0;
                    }
                    PlayNext(index);
                }
                else if (PlayMode == PlayMode.SingleLoop)
                {
                    PlayNext(CurrentPlayingIndex);
                }
                else if (PlayMode == PlayMode.Shuffle)
                {
                    random ??= new Random();
                    var index = random.Next(CurrentPlayList.Count);
                    PlayNext(index);
                }
            }
        }

        public void PlayPrevious()
        {
            if (CurrentPlayingIndex != -1)
            {
                var index = CurrentPlayingIndex - 1;
                if (index < 0)
                {
                    index = CurrentPlayList.Count - 1;
                }
                PlayNext(index);
            }
        }

        public void PlayOrPause()
        {
            if (_mpv == null)
            {
                return;
            }
            if (_mpv.IsPlaying)
            {
                _playerTimer.Stop();
                _mpv.Pause();
                IsPlaying = false;

            }
            else
            {
                _mpv.Resume();
                _playerTimer.Start();
                IsPlaying = true;
            }
        }

        public async void PlayTo(TimeSpan to)
        {
            await _mpv.SeekAsync(to);
        }

        public async void PlayStepForward(TimeSpan delta)
        {
            var t = _mpv.Position + delta;
            if (t > CurrentPlayingDuration)
            {

            }
            else
            {
                await _mpv.SeekAsync(t);
            }
        }

        public async void PlayStepBackward(TimeSpan delta)
        {
            var t = TimeSpan.Zero;
            if (_mpv.Position > delta)
            {
                t = _mpv.Position - delta;
            }
            await _mpv.SeekAsync(t);
        }

        public void Stop()
        {
            _playerTimer.Stop();
            _mpv.Stop();
        }

        public void PauseAsStop()
        {
            _playerTimer.Stop();
            _mpv.Pause();
            UIQueue.TryEnqueue(() =>
            {
                IsPlaying = false;
            });
            CustomPauseAsStop();
            App.SetSmtcStatus(MediaPlaybackStatus.Stopped);
            App.JellyfinMusicService.ReportStop(CurrentPlaying, CurrentTime.Ticks);
        }

        protected virtual void CustomPauseAsStop() { }

        public void TogglePlayMode()
        {
            PlayMode = PlayMode switch
            {
                PlayMode.Loop => PlayMode.SingleLoop,
                PlayMode.SingleLoop => PlayMode.Shuffle,
                PlayMode.Shuffle => PlayMode.Loop,
                _ => PlayMode.Loop,
            };
        }

        public void Command(params string[] args)
        {
            _mpv?.API.Command(args);
        }

        public void SetPropertyString(string key, string value)
        {
            _mpv?.API.SetPropertyString(key, value);
        }

        public void SetPropertyLong(string key, long value)
        {
            _mpv?.API.SetPropertyLong(key, value);
        }

        public string GetPropertyString(string key)
        {
            return _mpv?.API.GetPropertyString(key);
        }

        public long? GetPropertyLong(string key)
        {
            return _mpv?.API.GetPropertyLong(key);
        }

        private void PlayerTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var time = _mpv.Position;
            UIQueue.TryEnqueue(() =>
            {
                try
                {
                    CurrentTime = time;
                }
                catch (Exception)
                {

                }
            });
            App.SetSmtcPosition(CurrentTime, CurrentPlayingDuration);
            UpdateJellyfinPosition();
            CustomReportProgress(CurrentPlaying, CurrentTime, CurrentPlayingDuration);
        }

        protected virtual void CustomReportProgress(BaseItemDto currentPlaying, TimeSpan CurrentTime, TimeSpan? CurrentPlayingDuration)
        {

        }

        private async void UpdateJellyfinPosition()
        {
            await App.JellyfinMusicService.ReportProgress(CurrentPlaying, CurrentTime.Ticks, !IsPlaying);
        }

        private void MediaEndedSeeking(object sender, EventArgs e)
        {
            UIQueue.TryEnqueue(() =>
            {
                IsPlaying = true;
                State = PlayerState.Playing;
            });
        }

        private void MediaStartedSeeking(object sender, EventArgs e)
        {
            UIQueue.TryEnqueue(() =>
            {
                IsPlaying = false;
                State = PlayerState.Loading;
            });
        }

        private void MediaUnloaded(object sender, EventArgs e)
        {
            UIQueue.TryEnqueue(() =>
            {
                IsPlaying = false;
            });
        }

        private void PositionChanged(object sender, MpvPlayerPositionChangedEventArgs e)
        {
            //UIQueue.TryEnqueue(() =>
            //{
            //    CurrentTime = e.NewPosition;
            //});
            //UpdateSmtcPosition();
        }

        private void MediaFinished(object sender, EventArgs e)
        {
            _playerTimer.Stop();
            UIQueue.TryEnqueue(() =>
            {
                IsPlaying = false;
            });
            if (PlayMode != PlayMode.Single)
            {
                if (!_isMediaSwitching)
                {
                    UIQueue.TryEnqueue(PlayNextInPlayList);
                }
            }
        }

        public event Action OnMediaLoaded;

        protected readonly ManualResetEvent _event = new(false);
        protected virtual void CustomMediaLoaded() { }
        private void MediaLoaded(object sender, EventArgs e)
        {
            var restore = Config.GetConfig("EnableRestorePrevLocation", false, true);
            if (restore)
            {
                _event.WaitOne();
                var prevPosition = new TimeSpan(CurrentPlaying?.UserData?.PlaybackPositionTicks ?? 0);
                if (prevPosition > CurrentPlayingDuration) prevPosition = TimeSpan.Zero;
                _mpv.Position = prevPosition;
            }

            _mpv.Resume();
            UIQueue.TryEnqueue(() =>
            {
                IsPlaying = true;
                OnPropertyChanged(nameof(Volume));
                OnPropertyChanged(nameof(CurrentPlayingDuration));
            });
            CustomMediaLoaded();
            OnMediaLoaded?.Invoke();
        }

        private void MediaPaused(object sender, EventArgs e)
        {
            UIQueue.TryEnqueue(() =>
            {
                IsPlaying = false;
            });
        }

        protected virtual void CustomMediaResumed() { }

        private void MediaResumed(object sender, EventArgs e)
        {
            UIQueue.TryEnqueue(() =>
            {
                IsPlaying = true;
            });
            CustomMediaResumed();
        }

        protected virtual void SetupMpvInitProperty(MpvPlayer mpv) { }
        protected virtual void SetupMpvPropertyBeforePlay(MpvPlayer mpv, BaseItemDto media) { }
        protected virtual IEnumerable<(string video, string audio)> GetMediaSources(ObservableCollection<BaseItemDto> list) { return []; }
        protected virtual void DoAfterPlay(int index) { }
        protected virtual void SetupMpvEvent(MpvPlayer mpv) { }
        protected virtual bool UpdateDetailedInfo => true;

        private void PlayerStarterDoWork(object sender, DoWorkEventArgs e)
        {
            _isMediaSwitching = true;
            var index = (int)e.Argument;
            var media = CurrentPlayList[index];
            _event.Reset();

            try
            {
                if (_mpv == null)
                {
                    _mpv = new MpvPlayer(@"NativeLibs\mpv-2.dll")
                    {
                        AutoPlay = false,
                        Volume = Volume,
                        LogLevel = MpvLogLevel.None,
                        Loop = false,
                        LoopPlaylist = false,
                    };
                    SetupMpvInitProperty(_mpv);

                    _mpv.MediaResumed += MediaResumed;
                    _mpv.MediaPaused += MediaPaused;
                    _mpv.MediaLoaded += MediaLoaded;
                    _mpv.MediaFinished += MediaFinished;
                    _mpv.PositionChanged += PositionChanged;
                    _mpv.MediaUnloaded += MediaUnloaded;
                    _mpv.MediaStartedSeeking += MediaStartedSeeking;
                    _mpv.MediaEndedSeeking += MediaEndedSeeking;

                    SetupMpvEvent(_mpv);
                }

                SetupMpvPropertyBeforePlay(_mpv, media);
                var lists = GetMediaSources(CurrentPlayList).ToArray();
                if (string.IsNullOrEmpty(lists[0].audio))
                {
                    _mpv.LoadPlaylist(lists.Select(l => l.video), true);
                }
                else
                {
                    var edl = GetEdlProtocal(lists[0].video, lists[0].audio);
                    _mpv.Load(edl, true);
                }
                _mpv.PlaylistPlayIndex(index);

                BaseItemDto info = null;
                if (UpdateDetailedInfo)
                {
                    if (media.Id != null)
                    {
                        info = App.JellyfinMusicService.GetItemInfoAsync(CurrentPlayList[index]).Result;
                    }
                    else
                    {
                        info = SetCustomInfo(media);
                    }
                }
                else
                {
                    info = CurrentPlayList[index];
                }

                e.Result = ValueTuple.Create(index, info);
                if(App.SMTC == null)
                {
                    App.InitSmtc();
                }
                InitMstcInfo(info);

                DoAfterPlay(index);
            }
            catch (Exception ex)
            {
                e.Result = (index, ex);
            }

            _isMediaSwitching = false;
        }

        protected virtual BaseItemDto SetCustomInfo(BaseItemDto info)
        {
            return info;
        }

        private static string GetEdlProtocal(string vurl, string aurl)
        {
            var sb = new StringBuilder();
            sb.Append("edl://!new_stream;!no_clip;!no_chapters;");
            var url = vurl;
            var urlLen = url.Length;
            sb.Append($"%{urlLen}%{url}");
            if (!string.IsNullOrEmpty(aurl))
            {
                sb.Append(";!new_stream;!no_clip;!no_chapters;");
                url = aurl;
                urlLen = url.Length;
                sb.Append($"%{urlLen}%{url}");
            }
            //sb.Append(";");

            return sb.ToString();
        }

        protected virtual void OnPlayerStaterComplete() { }

        private void PlayerStarterCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OnPlayerStaterComplete();
            try
            {
                if (e.Result is (int index, BaseItemDto info))
                {
                    HasError = false;
                    IsPlaying = true;
                    State = PlayerState.Playing;
                    if (UpdateDetailedInfo)
                    {
                        CurrentPlaying = info;
                    }
                    else
                    {
                        CurrentPlaying = CurrentPlayList[index];
                    }
                    CurrentPlayingIndex = index;
                    _playerTimer.Start();
                    OnPropertyChanged(nameof(Volume));
                }
                else if (e.Result is (int index2, Exception _playException))
                {
                    App?.ShowToast(new ToastInfo { Text = "播放错误 " + _playException.Message });
                    HasError = true;
                    State = PlayerState.Error;
                    if (index2 != CurrentPlayList.Count - 1)
                    {
                        CurrentPlayingIndex = index2;
                        PlayNextInPlayList();
                    }
                }
                else
                {
                    State = PlayerState.Idle;
                }
            }
            catch (Exception)
            {

            }
            CustomMediaInited(CurrentPlaying);
            _event.Set();
        }

        public virtual void CustomMediaInited(BaseItemDto current) { }

        protected virtual void SetupMstcInfo(BaseItemDto media, SystemMediaTransportControlsDisplayUpdater updater)
        {
            updater.Type = MediaPlaybackType.Video;
            updater.VideoProperties.Title = media.Name;
            updater.VideoProperties.Subtitle = media.Etag == "Bilibili" ? "Bilibili" : media.Id == null ? "文件" : "Jellyfin";
        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/system-media-transport-controls
        /// </summary>
        /// <param name="index"></param>
        private void InitMstcInfo(BaseItemDto media)
        {
            App.SetSmtcStatus(MediaPlaybackStatus.Playing, true);

            SystemMediaTransportControlsDisplayUpdater updater = App.SMTC.DisplayUpdater;

            SetupMstcInfo(media, updater);

            if (media.Id != null)
            {
                var uri = App.JellyfinMusicService.GetPrimaryJellyfinImageSmall(media.ImageTags, media.Id);

                if (uri != null)
                {
                    updater.Thumbnail = RandomAccessStreamReference.CreateFromUri(uri);
                }
            }
            else if(media.Etag == "Bilibili")
            {
                var uri = new Uri(media.Overview);
                if (uri != null)
                {
                    updater.Thumbnail = RandomAccessStreamReference.CreateFromUri(uri);
                }
            }

            App.SMTC.DisplayUpdater.Update();
        }

        public void SMTCPlaybackPositionChangeRequested(SystemMediaTransportControls sender, PlaybackPositionChangeRequestedEventArgs args)
        {
            if (args.RequestedPlaybackPosition.Duration() <= (CurrentPlayingDuration ?? TimeSpan.Zero) &&
            args.RequestedPlaybackPosition.Duration().TotalSeconds >= 0)
            {
                if (State == PlayerState.Playing)
                {
                    PlayTo(args.RequestedPlaybackPosition.Duration());
                    App.SetSmtcPosition(CurrentTime, CurrentPlayingDuration);
                }
            }
        }

        public void SMTCButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    UIQueue.TryEnqueue(PlayOrPause);
                    break;

                case SystemMediaTransportControlsButton.Pause:
                    UIQueue.TryEnqueue(PlayOrPause);
                    break;

                case SystemMediaTransportControlsButton.Stop:
                    UIQueue.TryEnqueue(PlayOrPause);
                    break;

                case SystemMediaTransportControlsButton.Next:
                    UIQueue.TryEnqueue(PlayNextInPlayList);
                    break;

                case SystemMediaTransportControlsButton.Previous:
                    UIQueue.TryEnqueue(PlayPrevious);
                    break;
            }
        }

        public virtual void ShutDown()
        {
            _mpv?.Stop();
            if (_mpv != null)
            {
                _mpv.MediaPaused -= MediaPaused;
                _mpv.MediaResumed -= MediaResumed;
                _mpv.MediaLoaded -= MediaLoaded;
                _mpv.MediaFinished -= MediaFinished;
                _mpv.MediaStartedSeeking -= MediaStartedSeeking;
                _mpv.MediaEndedSeeking -= MediaEndedSeeking;
            }
            _mpv?.Dispose();
            _mpv = null;
        }

        public override void Dispose()
        {
            _mpv?.Stop();
            if (_mpv != null)
            {
                _mpv.MediaPaused -= MediaPaused;
                _mpv.MediaResumed -= MediaResumed;
                _mpv.MediaLoaded -= MediaLoaded;
                _mpv.MediaFinished -= MediaFinished;
                _mpv.MediaStartedSeeking -= MediaStartedSeeking;
                _mpv.MediaEndedSeeking -= MediaEndedSeeking;
            }
            //_mpv?.Dispose();
            //base.Dispose();
            //GC.SuppressFinalize(this);
        }
    }
}
