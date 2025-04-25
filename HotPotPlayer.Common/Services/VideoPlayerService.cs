using CommunityToolkit.Mvvm.ComponentModel;
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
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Windows.Media;

namespace HotPotPlayer.Services
{
    public partial class VideoPlayerService : ServiceBaseWithConfig
    {
        public VideoPlayerService(ConfigBase config, DispatcherQueue uiThread = null, AppBase app = null) : base(config, uiThread, app)
        {
            _playerStarter = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };

            _playerStarter.RunWorkerCompleted += PlayerStarterCompleted;
            _playerStarter.DoWork += PlayerStarterDoWork;

            Config.SaveConfigWhenExit("Volume", () => (Volume != 0, Volume));
        }

        private PlayerState _state;

        public PlayerState State
        {
            get => _state;
            set => Set(ref _state, value);
        }

        private BaseItemDto _currentPlaying;
        public BaseItemDto CurrentPlaying
        {
            get => _currentPlaying;
            set => Set(ref _currentPlaying, value);
        }

        public TimeSpan? CurrentPlayingDuration
        {
            get => _mpv?.Duration;
        }

        private int _currentPlayingIndex = -1;
        public int CurrentPlayingIndex
        {
            get => _currentPlayingIndex;
            set => Set(ref _currentPlayingIndex, value);
        }

        private ObservableCollection<BaseItemDto> _currentPlayList;
        public ObservableCollection<BaseItemDto> CurrentPlayList
        {
            get => _currentPlayList;
            set => Set(ref _currentPlayList, value);
        }

        public bool SuppressCurrentTimeTrigger { get; set; }

        private TimeSpan _currentTime;

        public TimeSpan CurrentTime
        {
            get => _currentTime;
            set
            {
                if (SuppressCurrentTimeTrigger) return;
                Set(ref _currentTime, value);
            }
        }

        private bool _isPlaying;

        public bool IsPlaying
        {
            get => _isPlaying;
            set => Set(ref _isPlaying, value, nowPlaying =>
            {
                Task.Run(() =>
                {
                    if (nowPlaying)
                    {
                        //if (SMTC != null)
                        //{
                        //    SMTC.PlaybackStatus = MediaPlaybackStatus.Playing;
                        //}
                    }
                    else
                    {
                        //if (SMTC != null)
                        //{
                        //    SMTC.PlaybackStatus = MediaPlaybackStatus.Paused;
                        //}
                    }
                });
            });
        }

        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            set => Set(ref _hasError, value);
        }

        private bool _isPlayListBarVisible;
        public bool IsPlayListBarVisible
        {
            get => _isPlayListBarVisible;
            set => Set(ref _isPlayListBarVisible, value);
        }

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
                    return 0;
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
                    RaisePropertyChanged(nameof(Volume));
                }
            }
        }

        private PlayMode _playMode;
        public PlayMode PlayMode
        {
            get => _playMode;
            set => Set(ref _playMode, value);
        }

        private bool isVideoPagePresent;
        public bool IsVideoPagePresent
        {
            get => isVideoPagePresent;
            set => Set(ref isVideoPagePresent, value);
        }

        public event EventHandler<MpvVideoGeometryInitEventArgs> VideoGeometryInit;
        public event EventHandler<IntPtr> SwapChainInited;
        public IntPtr SwapChain { get; set; }
        MpvPlayer _mpv;
        bool _isVideoSwitching;
        readonly BackgroundWorker _playerStarter;

        public float ScaleX { get; set; }
        public float ScaleY { get; set; }

        public async void PlayNext(BaseItemDto video)
        {
            if (video.IsFolder.Value)
            {

                var seasons = await App.JellyfinMusicService.GetSeasons(video);
                var season = seasons.FirstOrDefault();
                var episodes = await App.JellyfinMusicService.GetEpisodes(season);
                CurrentPlayList = [ .. episodes];
            }
            else
            {
                CurrentPlayList = [ video ];
            }
            PlayNext(0);
        }

        public void PlayNext(List<FileInfo> files, int index)
        {
            var fakeDtos = files.Select(f => new BaseItemDto { Path = f.FullName });
            CurrentPlayList = [ .. fakeDtos];
            PlayNext(index);
        }

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
                CurrentTime = TimeSpan.Zero;
                IsPlaying = false;
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
                _mpv.Pause();
                IsPlaying = false;
            }
            else
            {
                _mpv.Resume();
                IsPlaying = true;
            }
        }

        public async void PlayTo(TimeSpan to)
        {
            await _mpv.SeekAsync(to);
        }

        public void Stop()
        {
            _mpv.Stop();
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
            //UpdateSmtcPosition();
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
        }

        private void MediaFinished(object sender, EventArgs e)
        {
            UIQueue.TryEnqueue(() =>
            {
                IsPlaying = false;
            });
            if (!_isVideoSwitching)
            {
                UIQueue.TryEnqueue(PlayNext);
            }
        }

        private void MediaLoaded(object sender, EventArgs e)
        {
            _mpv.Resume();
            UIQueue.TryEnqueue(() =>
            {
                IsPlaying = true;
                RaisePropertyChanged(nameof(Volume));
                RaisePropertyChanged(nameof(CurrentPlayingDuration));
            });
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

        private void PlayerStarterDoWork(object sender, DoWorkEventArgs e)
        {
            _isVideoSwitching = true;
            var index = (int)e.Argument;
            var video = CurrentPlayList[index];

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
                    //_mpv.API.SetPropertyDouble("display-fps-override", 120d);
                    //_mpv.API.SetPropertyString("gpu-debug", "yes");
                    //_mpv.API.SetPropertyString("vo", "gpu-next");
                    _mpv.API.SetPropertyString("vo", "gpu");
                    _mpv.API.SetPropertyString("gpu-context", "d3d11");
                    _mpv.API.SetPropertyString("hwdec", "d3d11va");
                    _mpv.API.SetPropertyString("d3d11-composition", "yes");
                    //_mpv.API.SetPropertyString("target-colorspace-hint", "yes"); //HDR passthrough
                    _mpv.MediaResumed += MediaResumed;
                    _mpv.MediaPaused += MediaPaused;
                    _mpv.MediaLoaded += MediaLoaded;
                    _mpv.MediaFinished += MediaFinished;
                    _mpv.PositionChanged += PositionChanged;
                    _mpv.MediaUnloaded += MediaUnloaded;
                    _mpv.MediaStartedSeeking += MediaStartedSeeking;
                    _mpv.MediaEndedSeeking += MediaEndedSeeking;
                    _mpv.API.VideoGeometryInit += VideoGeometryInit;
                    _mpv.API.SwapChainInited += OnSwapChainInited;
                }
                var lists = CurrentPlayList.Select(v =>
                {
                    if (v.Path != null)
                    {
                        return v.Path;
                    }
                    else
                    {
                        return App.JellyfinMusicService.GetVideoStream(v);
                    }
                });
                _mpv.LoadPlaylist(lists, true);
                _mpv.PlaylistPlayIndex(index);
                e.Result = ValueTuple.Create(index, false);

                //_smtc ??= InitSmtc();
                //UpdateMstcInfo((int)e.Argument);
            }
            catch (Exception ex)
            {
                e.Result = (index, ex);
            }

            _isVideoSwitching = false;
        }

        private void OnSwapChainInited(object sender, IntPtr swapchain)
        {
            SwapChain = swapchain;
            SwapChainInited?.Invoke(sender, swapchain);
        }

        private void PlayerStarterCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Result is (int index, bool intercept))
                {
                    HasError = false;
                    IsPlaying = true;
                    var video = CurrentPlayList[index];
                    //music.IsIntercept = intercept;
                    CurrentPlaying = video;
                    CurrentPlayingIndex = index;
                    RaisePropertyChanged(nameof(Volume));
                }
                else if (e.Result is (int index2, Exception _playException))
                {
                    App?.ShowToast(new ToastInfo { Text = "播放错误 " + _playException.Message });
                    HasError = true;
                    if (index2 != CurrentPlayList.Count - 1)
                    {
                        CurrentPlayingIndex = index2;
                        PlayNext();
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        public void UpdatePanelScale(float scaleX, float scaleY)
        {
            _mpv.API.SetPanelScale(scaleX, scaleY);
        }

        public void UpdatePanelSize(int width, int height)
        {
            _mpv.API.SetPanelSize(width, height);
        }

        public void ShutDown()
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
            SwapChain = 0;
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
            _mpv?.Dispose();
            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
