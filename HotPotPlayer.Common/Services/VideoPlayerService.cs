using DirectN;
using DirectN.Extensions;
using DirectN.Extensions.Com;
using HotPotPlayer.Models;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.UI.Dispatching;
using Mpv.NET.API;
using Mpv.NET.Player;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Windows.Devices.Enumeration;
using Windows.Media;

namespace HotPotPlayer.Services
{
    public enum VideoPlayVisualState
    {
        TinyHidden,
        FullHost,
        FullWindow,
        FullScreen,
        SmallHost
    }

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

        private VideoPlayVisualState visualState;
        public VideoPlayVisualState VisualState
        {
            get => visualState;
            set => Set(ref visualState, value);
        }

        public event EventHandler<MpvVideoGeometryInitEventArgs> VideoGeometryInit;
        public event EventHandler<IntPtr> SwapChainInited;
        public event Action OnMediaLoaded;
        public IntPtr SwapChain { get; set; }
        MpvPlayer _mpv;
        bool _isVideoSwitching;
        readonly BackgroundWorker _playerStarter;

        private float _currentScaleX;
        private float _currentScaleY;
        private int _currentWidth;
        private int _currentHeight;
        private Rectangle _currentBounds;

        private IComObject<ID3D11Device> _device;
        private IComObject<ID3D11DeviceContext> _deviceContext;
        private IComObject<IDXGISwapChain1> _swapChain;

        public float ScaleX { get; set; }
        public float ScaleY { get; set; }

        public async void PlayNext(BaseItemDto video)
        {
            if (video.IsFolder.Value)
            {
                VisualState = VideoPlayVisualState.FullWindow;
                State = PlayerState.Loading;

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

        public void PlayNext(List<BaseItemDto> files, int index)
        {
            CurrentPlayList = [.. files];
            PlayNext(index);
        }

        public void PlayNext(List<FileInfo> files, int index)
        {
            var fakeDtos = files.Select(f => new BaseItemDto { Path = f.FullName });
            CurrentPlayList = [ .. fakeDtos];
            PlayNext(index);
        }

        public void PlayNextInCurrentList(BaseItemDto v)
        {
            var i = CurrentPlayList.IndexOf(v);
            if (i != CurrentPlayingIndex)
            {
                PlayNext(i);
            }
        }

        public async void PlayNext(int? index)
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

                VisualState = VideoPlayVisualState.FullWindow;
                State = PlayerState.Loading;

                if (SwapChain == IntPtr.Zero)
                {
                    await Task.Delay(1000);
                }
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

        public void Pause()
        {
            _mpv.Pause();
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

        public void SetPropertyLong(string  key, long value)
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
                    if (v.Path != null && v.Id == null)
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

                var videoInfo = App.JellyfinMusicService.GetItemInfoAsync(CurrentPlayList[index]).Result;

                e.Result = ValueTuple.Create(index, videoInfo, false);
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
                if (e.Result is (int index, BaseItemDto info, bool intercept))
                {
                    HasError = false;
                    IsPlaying = true;
                    State = PlayerState.Playing;
                    //var video = CurrentPlayList[index];
                    //music.IsIntercept = intercept;
                    CurrentPlaying = info;
                    CurrentPlayingIndex = index;
                    RaisePropertyChanged(nameof(Volume));
                    OnMediaLoaded?.Invoke();
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
        }

        public void UpdatePanelScale(float scaleX, float scaleY)
        {
            _currentScaleX = scaleX;
            _currentScaleY = scaleY;
            _mpv.API.SetPanelScale(scaleX, scaleY);
        }

        public void UpdatePanelSize(int width, int height)
        {
            _currentWidth = width;
            _currentHeight = height;
            _mpv.API.SetPanelSize(width, height);
        }

        public void UpdatePanelBounds(Rectangle bounds)
        {
            _currentBounds = bounds;
        }

        public IComObject<IDXGISwapChain1> GetOrCreateSwapChain()
        {
            if(_swapChain != null && _device != null) return _swapChain;

            _device = D3D11Functions.D3D11CreateDevice(null!, D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_HARDWARE, 0, out _deviceContext);

            var desc = new DXGI_SWAP_CHAIN_DESC1
            {
                Format = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM,
                BufferUsage = DXGI_USAGE.DXGI_USAGE_RENDER_TARGET_OUTPUT,
                BufferCount = 2,
                SampleDesc = new DXGI_SAMPLE_DESC { Count = 1 },
                SwapEffect = DXGI_SWAP_EFFECT.DXGI_SWAP_EFFECT_FLIP_DISCARD,
                Scaling = DXGI_SCALING.DXGI_SCALING_STRETCH,
                Width = (uint)_currentWidth,
                Height = (uint)_currentHeight,
            };

            using var dxgiDevice = _device.As<IDXGIDevice1>()!;
            using var adapter = dxgiDevice.GetAdapter();
            using var fac = adapter.GetFactory2()!;

            _swapChain = fac.CreateSwapChainForComposition<IDXGISwapChain1>(dxgiDevice, desc);
            return _swapChain;
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
