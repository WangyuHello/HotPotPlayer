using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Helpers;
using HotPotPlayer.Models;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.UI.Dispatching;
using Mpv.NET.API;
using Mpv.NET.Player;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
        private SystemMediaTransportControls _smtc;
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
        private PlayerState state;

        [ObservableProperty]
        private BaseItemDto currentPlaying;

        public TimeSpan? CurrentPlayingDuration
        {
            get => _mpv?.Duration;
        }

        [ObservableProperty]
        private int currentPlayingIndex = -1;

        [ObservableProperty]
        private ObservableCollection<BaseItemDto> currentPlayList;

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

        private bool _isPlaying;

        public bool IsPlaying
        {
            get => _isPlaying;
            set => SetProperty(ref _isPlaying, value, nowPlaying =>
            {
                Task.Run(async () =>
                {
                    if (nowPlaying)
                    {
                        if (SMTC != null)
                        {
                            SMTC.PlaybackStatus = MediaPlaybackStatus.Playing;
                        }
                        App?.Taskbar.SetProgressState(TaskbarHelper.TaskbarStates.Normal);
                    }
                    else
                    {
                        if (SMTC != null)
                        {
                            SMTC.PlaybackStatus = MediaPlaybackStatus.Paused;
                        }
                        App?.Taskbar.SetProgressState(TaskbarHelper.TaskbarStates.Paused);
                        await App.JellyfinMusicService.ReportProgress(CurrentPlaying, CurrentTime.Ticks, true);
                    }
                });
            });
        }

        [ObservableProperty]
        private bool hasError;

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
                    if (_mpv != null)
                    {
                        _mpv.Volume = value;
                    }
                    OnPropertyChanged(nameof(Volume));
                }
            }
        }

        [ObservableProperty]
        private PlayMode playMode;

        private SystemMediaTransportControls SMTC
        {
            get => _smtc;
        }
        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/apps/desktop/modernize/winrt-com-interop-csharp
        /// </summary>
        /// <returns></returns>
        SystemMediaTransportControls InitSmtc()
        {
            var smtc = SystemMediaTransportControlsInterop.GetForWindow(Config.MainWindowHandle);
            smtc.IsEnabled = true;
            smtc.ButtonPressed += SystemMediaControls_ButtonPressed;
            smtc.PlaybackRateChangeRequested += SystemMediaControls_PlaybackRateChangeRequested;
            smtc.PlaybackPositionChangeRequested += SystemMediaControls_PlaybackPositionChangeRequested;
            smtc.AutoRepeatModeChangeRequested += SystemMediaControls_AutoRepeatModeChangeRequested;
            smtc.PropertyChanged += SystemMediaControls_PropertyChanged;
            smtc.IsPlayEnabled = true;
            smtc.IsPauseEnabled = true;
            smtc.IsStopEnabled = true;
            smtc.IsNextEnabled = true;
            smtc.IsPreviousEnabled = true;
            smtc.PlaybackStatus = MediaPlaybackStatus.Closed;

            App?.Taskbar.InitTaskBarButtons();

            return smtc;
        }

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

        protected virtual void OnPlayNextStateChange() { }

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

                OnPlayNextStateChange();
                State = PlayerState.Loading;

                _playerStarter.RunWorkerAsync(index);
            }
        }

        Random random;
        public void PlayNext()
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
            IsPlaying = false;
            App?.Taskbar.SetProgressState(TaskbarHelper.TaskbarStates.NoProgress);
            App.JellyfinMusicService.ReportStop(CurrentPlaying, CurrentTime.Ticks);
        }

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
            UpdateSmtcPosition();
            UpdateJellyfinPosition();
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
            });
        }

        private void MediaStartedSeeking(object sender, EventArgs e)
        {
            UIQueue.TryEnqueue(() =>
            {
                IsPlaying = false;
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
            UIQueue.TryEnqueue(() =>
            {
                CurrentTime = e.NewPosition;
            });
            //UpdateSmtcPosition();
        }

        private void MediaFinished(object sender, EventArgs e)
        {
            _playerTimer.Stop();
            UIQueue.TryEnqueue(() =>
            {
                IsPlaying = false;
            });
            if (!_isMediaSwitching)
            {
                UIQueue.TryEnqueue(PlayNext);
            }
        }

        public Action OnMediaLoaded;

        protected readonly ManualResetEvent _event = new ManualResetEvent(false);
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
            OnMediaLoaded?.Invoke();
        }

        private void MediaPaused(object sender, EventArgs e)
        {
            UIQueue.TryEnqueue(() =>
            {
                IsPlaying = false;
            });
        }

        private void MediaResumed(object sender, EventArgs e)
        {
            UIQueue.TryEnqueue(() =>
            {
                IsPlaying = true;
            });
        }

        protected virtual void SetupMpvInitProperty(MpvPlayer mpv) { }
        protected virtual void SetupMpvPropertyBeforePlay(MpvPlayer mpv, BaseItemDto media) { }
        protected virtual IEnumerable<string> GetMediaSources(ObservableCollection<BaseItemDto> list) { return []; }
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
                var lists = GetMediaSources(CurrentPlayList);
                _mpv.LoadPlaylist(lists, true);
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
                        info = media;
                    }
                }
                else
                {
                    info = CurrentPlayList[index];
                }

                e.Result = ValueTuple.Create(index, info);
                _smtc ??= InitSmtc();
                InitMstcInfo(info);

                DoAfterPlay(index);
            }
            catch (Exception ex)
            {
                e.Result = (index, ex);
            }

            _isMediaSwitching = false;
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
                        PlayNext();
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
            _event.Set();
        }

        private void SystemMediaControls_PropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            if (args.Property == SystemMediaTransportControlsProperty.SoundLevel)
            {
                switch (SMTC.SoundLevel)
                {
                    case SoundLevel.Full:
                    case SoundLevel.Low:

                        break;
                    case SoundLevel.Muted:

                        break;
                }
            }
        }

        private void SystemMediaControls_AutoRepeatModeChangeRequested(SystemMediaTransportControls sender, AutoRepeatModeChangeRequestedEventArgs args)
        {
            switch (args.RequestedAutoRepeatMode)
            {
                case MediaPlaybackAutoRepeatMode.None:
                    PlayMode = PlayMode.Loop;
                    break;
                case MediaPlaybackAutoRepeatMode.List:
                    PlayMode = PlayMode.Loop;
                    break;
                case MediaPlaybackAutoRepeatMode.Track:
                    PlayMode = PlayMode.SingleLoop;
                    break;
            }

            SMTC.AutoRepeatMode = args.RequestedAutoRepeatMode;
        }

        private void UpdateSmtcPosition()
        {
            var timelineProperties = new SystemMediaTransportControlsTimelineProperties
            {
                StartTime = TimeSpan.FromSeconds(0),
                MinSeekTime = TimeSpan.FromSeconds(0),
                Position = CurrentTime,
                MaxSeekTime = CurrentPlayingDuration ?? TimeSpan.Zero,
                EndTime = CurrentPlayingDuration ?? TimeSpan.Zero
            };

            SMTC?.UpdateTimelineProperties(timelineProperties);

            App?.Taskbar.SetProgressValue(CurrentTime.TotalSeconds, CurrentPlayingDuration.Value.TotalSeconds);
        }

        private void SystemMediaControls_PlaybackPositionChangeRequested(SystemMediaTransportControls sender, PlaybackPositionChangeRequestedEventArgs args)
        {
            if (args.RequestedPlaybackPosition.Duration() <= (CurrentPlayingDuration ?? TimeSpan.Zero) &&
            args.RequestedPlaybackPosition.Duration().TotalSeconds >= 0)
            {
                if (State == PlayerState.Playing)
                {
                    PlayTo(args.RequestedPlaybackPosition.Duration());
                    UpdateSmtcPosition();
                }
            }
        }

        private void SystemMediaControls_PlaybackRateChangeRequested(SystemMediaTransportControls sender, PlaybackRateChangeRequestedEventArgs args)
        {
            if (args.RequestedPlaybackRate >= 0 && args.RequestedPlaybackRate <= 2)
            {
                SMTC.PlaybackRate = args.RequestedPlaybackRate;
            }
        }

        private void SystemMediaControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
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
                    UIQueue.TryEnqueue(PlayNext);
                    break;

                case SystemMediaTransportControlsButton.Previous:
                    UIQueue.TryEnqueue(PlayPrevious);
                    break;
            }
        }

        protected virtual void SetupMstcInfo(BaseItemDto media, SystemMediaTransportControlsDisplayUpdater updater)
        {
            updater.Type = MediaPlaybackType.Video;
            updater.VideoProperties.Title = media.Name;
            updater.VideoProperties.Subtitle = media.Id == null ? "文件" : "Jellyfin";
        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/system-media-transport-controls
        /// </summary>
        /// <param name="index"></param>
        private void InitMstcInfo(BaseItemDto media)
        {
            SMTC.PlaybackStatus = MediaPlaybackStatus.Playing;

            SystemMediaTransportControlsDisplayUpdater updater = SMTC.DisplayUpdater;

            SetupMstcInfo(media, updater);

            if (media.Id != null)
            {
                var uri = App.JellyfinMusicService.GetPrimaryJellyfinImageSmall(media.ImageTags, media.Id);

                if (uri != null)
                {
                    updater.Thumbnail = RandomAccessStreamReference.CreateFromUri(uri);
                }
            }

            SMTC.DisplayUpdater.Update();

            App?.Taskbar.SetProgressState(TaskbarHelper.TaskbarStates.NoProgress);
            App?.Taskbar.SetProgressValue(0, 100);
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
            _playerStarter?.Dispose();
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
