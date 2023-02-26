using CommunityToolkit.Mvvm.ComponentModel;
using DirectN;
using HotPotPlayer.Bilibili.Models.Danmaku;
using HotPotPlayer.Bilibili.Models.Video;
using HotPotPlayer.BiliBili;
using HotPotPlayer.Extensions;
using HotPotPlayer.Models;
using HotPotPlayer.Models.BiliBili;
using HotPotPlayer.Services;
using HotPotPlayer.Video.Extensions;
using HotPotPlayer.Video.Models;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Mpv.NET.API;
using Mpv.NET.API.Structs;
using Mpv.NET.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Display;
using Windows.UI.Core;
using WinRT;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Video.UI.Controls
{
    public sealed partial class VideoControl : UserControlBase
    {
        public VideoControl()
        {
            this.InitializeComponent();
            UIQueue = DispatcherQueue.GetForCurrentThread();
            PlaySlider.AddHandler(PointerReleasedEvent, new PointerEventHandler(PlaySlider_OnPointerReleased), true);
            PlaySlider.AddHandler(PointerPressedEvent, new PointerEventHandler(PlaySlider_OnPointerPressed), true);

            //Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
            IsPbpOn = Config.GetConfig("IsPbpOn", false);
        }

        private async void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            if (args.EventType.ToString().Contains("KeyUP") && _currentMediaInited)
            {
                var virtualKey = args.VirtualKey;
                switch (virtualKey)
                {
                    case Windows.System.VirtualKey.LeftButton:
                        break;
                    case Windows.System.VirtualKey.RightButton:
                        break;
                    case Windows.System.VirtualKey.Left:
                        await _mpv.SeekAsync(-10, true);
                        break;
                    case Windows.System.VirtualKey.Up:
                        break;
                    case Windows.System.VirtualKey.Right:
                        await _mpv.SeekAsync(10, true);
                        break;
                    case Windows.System.VirtualKey.Down:
                        break;
                    default:
                        break;
                }
            }
        }

        #region DepsProp

        public bool IsFullPage
        {
            get { return (bool)GetValue(IsFullPageProperty); }
            set { SetValue(IsFullPageProperty, value); }
        }

        public static readonly DependencyProperty IsFullPageProperty =
            DependencyProperty.Register("IsFullPage", typeof(bool), typeof(VideoControl), new PropertyMetadata(default));

        public VideoPlayInfo Source
        {
            get { return (VideoPlayInfo)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(VideoPlayInfo), typeof(VideoControl), new PropertyMetadata(default(VideoPlayInfo), SourceChanged));

        private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var h = (VideoControl)d;
            var info = (VideoPlayInfo)e.NewValue;
            h.OnSourceChanged(info);
        }

        public bool IsFullPageHost
        {
            get { return (bool)GetValue(IsFullPageHostProperty); }
            set { SetValue(IsFullPageHostProperty, value); }
        }

        public static readonly DependencyProperty IsFullPageHostProperty =
            DependencyProperty.Register("IsFullPageHost", typeof(bool), typeof(VideoControl), new PropertyMetadata(default));

        public DMData DmData
        {
            get { return (DMData)GetValue(DmDataProperty); }
            set { SetValue(DmDataProperty, value); }
        }

        public static readonly DependencyProperty DmDataProperty =
            DependencyProperty.Register("DmData", typeof(DMData), typeof(VideoControl), new PropertyMetadata(default));

        public Pbp Pbp
        {
            get { return (Pbp)GetValue(PbpProperty); }
            set { SetValue(PbpProperty, value); }
        }

        public static readonly DependencyProperty PbpProperty =
            DependencyProperty.Register("Pbp", typeof(Pbp), typeof(VideoControl), new PropertyMetadata(default));

        #endregion

        #region ObsProp
        [ObservableProperty]
        [AlsoNotifyChangeFor(nameof(CurrentPlayItem))]
        private ObservableCollection<VideoItem> currentPlayList;

        private int currentPlayIndex;
        public int CurrentPlayIndex
        {
            get => currentPlayIndex;
            set
            {
                if (currentPlayIndex != value)
                {
                    Set(ref currentPlayIndex, value);
                    _mpv.PlaylistPlayIndex(currentPlayIndex);
                    OnPropertyChanged(propertyName: nameof(CurrentPlayItem));
                }
            }
        }

        public VideoItem CurrentPlayItem => CurrentPlayList[CurrentPlayIndex];

        private bool isPlayListBarVisible;
        public bool IsPlayListBarVisible
        {
            get => isPlayListBarVisible;
            set
            {
                if (isPlayListBarVisible != value)
                {
                    Set(ref isPlayListBarVisible, value);
                    AppWindow.SetTitleBarForegroundColor(isPlayListBarVisible);
                }
            }
        }

        [ObservableProperty]
        private List<string> definitions;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private bool isPlaying;

        private TimeSpan _currentTime;
        public TimeSpan CurrentTime
        {
            get => _currentTime;
            set
            {
                if (_suppressCurrentTimeTrigger) return;
                Set(ref _currentTime, value);
            }
        }

        [ObservableProperty]
        private TimeSpan? currentPlayingDuration;

        [ObservableProperty]
        private PlayMode playMode;

        public int Volume
        {
            get
            {
                if (_mpv == null)
                {
                    return 100;
                }
                return _mpv.Volume;
            }
            set
            {
                if (_mpv != null && value != _mpv.Volume)
                {
                    _mpv.Volume = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool playBarVisible;

        public bool PlayBarVisible
        {
            get => playBarVisible;
            set
            {
                Set(ref playBarVisible, value, newV =>
                {
                    ShowCursor(newV ? 1 : 0);
                    if (newV == true)
                    {
                        _inActiveTimer ??= InitInActiveTimer();
                        _inActiveTimer.Start();
                    }
                });
            }
        }

        private string selectedDefinition;

        public string SelectedDefinition
        {
            get => selectedDefinition;
            set => Set(ref selectedDefinition, value, nv =>
            {
                if (_currentMediaInited && !string.IsNullOrEmpty(nv))
                {
                    StartPlay(nv);
                }
            });
        }

        private bool isVideoInfoOn;
        public bool IsVideoInfoOn
        {
            get => isVideoInfoOn;
            set => Set(ref isVideoInfoOn, value, nv =>
            {
                _mpv.API.Command("script-binding", "stats/display-stats-toggle");
            });
        }

        private CodecStrategy selectedCodecStrategy;
        public CodecStrategy SelectedCodecStrategy
        {
            get => selectedCodecStrategy;
            set => Set(ref selectedCodecStrategy, value, nv =>
            {
                Config.SetConfig("CodecStrategy", nv, true);
            });
        }

        private bool isPbpOn;
        public bool IsPbpOn
        {
            get => isPbpOn;
            set => Set(ref isPbpOn, value, nv =>
            {
                Config.SetConfig("IsPbpOn", nv, true);
            });
        }

        public bool IsFullScreen
        {
            get => AppWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen;
            set
            {
                if ((AppWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen) != value)
                {
                    AppWindow.SetPresenter(value ? AppWindowPresenterKind.FullScreen : AppWindowPresenterKind.Default);
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Prop
        MpvPlayer _mpv;
        bool _suppressCurrentTimeTrigger;
        DispatcherTimer _inActiveTimer;
        bool _currentMediaInited;
        bool _currentMediaEnded;

        IntPtr _devicePtr;
        ID3D11Device _device;
        IntPtr _swapChain1Ptr;
        IDXGISwapChain1 _swapChain1;
        readonly DispatcherQueue UIQueue;
        bool _swapChainLoaded;
        object _criticalLock = new object();
        object _criticalLock2 = new object();
        int _currentWidth;
        int _currentHeight;
        float _currentScaleX;
        float _currentScaleY;
        System.Drawing.Rectangle _currentWindowBounds;
        Random _random;

        #endregion

        #region Event
        public event Action OnMediaLoaded;
        public event Action OnToggleFullPage;
        public event Action OnToggleFullScreen;


        #endregion

        private void OnSourceChanged(VideoPlayInfo info)
        {
            currentPlayList = new ObservableCollection<VideoItem>(info.VideoItems);
            currentPlayIndex = info.Index;
            StartPlay();
        }

        void InitMpv(bool isFullPageHost, int width, int height, float scalex, float scaley)
        {
            _mpv = new MpvPlayer(App.MainWindowHandle, @"NativeLibs\mpv-2.dll", width, height, scalex, scaley, _currentWindowBounds)
            {
                AutoPlay = true,
                Volume = 100,
                LogLevel = MpvLogLevel.Debug,
                Loop = false,
                LoopPlaylist = isFullPageHost,
            };
            _mpv.SetD3DInitCallback(D3DInitCallback);
            _mpv.MediaResumed += MediaResumed;
            _mpv.MediaPaused += MediaPaused;
            _mpv.MediaLoaded += MediaLoaded;
            _mpv.MediaFinished += MediaFinished;
            _mpv.PositionChanged += PositionChanged;
            _mpv.MediaUnloaded += MediaUnloaded;
        }

        private void MediaUnloaded(object sender, EventArgs e)
        {
            
        }

        private void PositionChanged(object sender, MpvPlayerPositionChangedEventArgs e)
        {
            UIQueue.TryEnqueue(() =>
            {
                CurrentTime = e.NewPosition;
                //CurrentPlayingDuration = _mpv.Duration;
            });
        }

        private void MediaFinished(object sender, EventArgs e)
        {
            UIQueue.TryEnqueue(() =>
            {
                IsPlaying = false;
                DM.Pause();
            });
            //DisplayReq.RequestRelease();
            _currentMediaEnded = true;
        }


        private async void MediaLoaded(object sender, EventArgs e)
        {
            _currentMediaEnded = false;
            App?.Taskbar.AddPlayButtons();
            //DisplayReq.RequestActive();

            UIQueue.TryEnqueue(() => 
            {
                OnMediaLoaded?.Invoke();
                Title = CurrentPlayList[CurrentPlayIndex].Title;
                IsPlaying = true;
                CurrentPlayingDuration = _mpv.Duration;
                OnPropertyChanged(propertyName: nameof(Volume));
            });

            await Task.Run(async () =>
            {
                await Task.Delay(1000);
                _currentMediaInited = true;
                UIQueue.TryEnqueue(() => PlayBarVisible = true);
            });
        }

        private void MediaPaused(object sender, EventArgs e)
        {
            UIQueue.TryEnqueue(() =>
            {
                IsPlaying = false;
                DM.Pause();
            });
            //DisplayReq.RequestRelease();
        }

        private void MediaResumed(object sender, EventArgs e)
        {
            UIQueue.TryEnqueue(() =>
            {
                IsPlaying = true;
                DM.Resume();
            });
            //DisplayReq.RequestActive();
        }


        private void D3DInitCallback(IntPtr d3d11Device, IntPtr swapChain)
        {
            _swapChain1Ptr = swapChain;
            _devicePtr = d3d11Device;
            UIQueue.TryEnqueue(() =>
            {
                _swapChain1 = (IDXGISwapChain1)Marshal.GetObjectForIUnknown(_swapChain1Ptr);
                _device = (ID3D11Device)Marshal.GetObjectForIUnknown(_devicePtr);
                //_swapChain1 = ObjectReference<IDXGISwapChain1>.FromAbi(swapChain).Vftbl;
                var nativepanel = Host.As<ISwapChainPanelNative>();
                _swapChain1.GetDesc1(out var desp);
                nativepanel.SetSwapChain(_swapChain1);
                _swapChainLoaded = true;
            });
        }

        public void StartPlay(string selectedDefinition = "")
        {
            if (!Host.IsLoaded || Host.ActualSize.X <= 1 || Host.ActualSize.Y <= 1 || CurrentPlayList == null)
            {
                return;
            }
            var isFullPageHost = IsFullPageHost;

            // 在独立线程初始化MPV
            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                if (_mpv == null)
                {
                    InitMpv(isFullPageHost, _currentWidth, _currentHeight, _currentScaleX, _currentScaleY);
                }

                //_mpv.API.SetPropertyString("vo", "gpu");
                _mpv.API.SetPropertyString("vo", "gpu-next");
                _mpv.API.SetPropertyString("gpu-context", "d3d11");
                _mpv.API.SetPropertyString("hwdec", "d3d11va");
                _mpv.API.SetPropertyString("d3d11-composition", "yes");
                _mpv.API.SetPropertyString("target-colorspace-hint", "yes"); //HDR passthrough

                if (CurrentPlayList[CurrentPlayIndex] is BiliBiliVideoItem bv)
                {
                    _mpv.API.SetPropertyString("user-agent", BiliAPI.UserAgent);
                    _mpv.API.SetPropertyString("cookies", "yes");
                    _mpv.API.SetPropertyString("ytdl", "no");
                    _mpv.API.SetPropertyString("cookies-file", GetCookieFile());
                    _mpv.API.SetPropertyString("http-header-fields", "Referer: http://www.bilibili.com/");
                    //_mpv.API.SetPropertyString("demuxer-lavf-o", $"headers=\"Referer: http://www.bilibili.com/\r\nUserAgent: {BiliAPI.UserAgent}\r\n\"");
                    //_mpv.API.SetPropertyString("demuxer-lavf-probescore", "1");

                    IEnumerable<string> videourls;
                    if (bv.DashVideos == null)
                    {
                        videourls = CurrentPlayList.Cast<BiliBiliVideoItem>().Select(b => b.Urls[0].Url);
                        _mpv.LoadPlaylist(videourls);
                    }
                    else
                    {
                        //var mpd = bv.WriteToMPD(Config);
                        SelectedCodecStrategy = Config.GetConfig("CodecStrategy", CodecStrategy.Default);
                        (var sel, var vurl) = bv.GetPreferVideoUrl(selectedDefinition, SelectedCodecStrategy);
                        if (!_currentMediaInited)
                        {
                            SelectedDefinition = sel;
                            UIQueue.TryEnqueue(() => Definitions = bv.Videos.Keys.ToList());
                        }
                        var aurl = bv.GetPreferAudioUrl();
                        //BiliBiliService.Proxy.AudioUrl = bv.GetPreferAudioUrl();
                        //BiliBiliService.Proxy.CookieString = BiliBiliService.API.CookieString;
                        if (sel.Contains("杜比") || sel.Contains("HDR")) _mpv.API.SetPropertyString("vo", "gpu");
                        //_mpv.Load(BiliBiliService.Proxy.VideoUrl);
                        //_mpv.Load(mpd);
                        //_mpv.Load("http://localhost:18909/video.m4s");
                        var edl = bv.GetEdlProtocal(vurl, aurl);
                        //_mpv.PlaylistClear();
                        //if (_currentMediaInited)
                        //{
                        //    _mpv.Stop();
                        //}
                        _mpv.LoadAsync(edl, true);
                    }
                }
                else
                {
                    _mpv.LoadPlaylist(CurrentPlayList.Select(f => f.Source.FullName));
                    _mpv.PlaylistPlayIndex(CurrentPlayIndex);
                }

            });
        }

        private string GetCookieFile()
        {
            var cookieFile = Path.Combine(Config.CookieFolder, "mpvCookie.txt");
            var cookie = BiliBiliService.API.Cookies;
            var netcookies = cookie.Select(c => c.ToNetScape()).ToList();
            File.WriteAllLines(cookieFile, netcookies);
            return cookieFile;
        }

        private void Host_Loaded(object sender, RoutedEventArgs e)
        {
            StartPlay();
        }

        private void Host_CompositionScaleChanged(SwapChainPanel sender, object args)
        {
            _currentScaleX = Host.CompositionScaleX;
            _currentScaleY = Host.CompositionScaleY;
            _currentWidth = (int)Math.Ceiling(Host.CompositionScaleX * Host.ActualWidth);
            _currentHeight = (int)Math.Ceiling(Host.CompositionScaleY * Host.ActualHeight);
            UpdateScale();
            UpdateSize();
        }

        private void Host_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _currentWidth = (int)Math.Ceiling(Host.CompositionScaleX*Host.ActualWidth);
            _currentHeight = (int)Math.Ceiling(Host.CompositionScaleY*Host.ActualHeight);
            _currentWindowBounds = new System.Drawing.Rectangle { X = (int)App.Bounds.Left, Y = (int)App.Bounds.Right, Width = 640, Height = 480 };

            UpdateSize();
        }

        private void Host_Unloaded(object sender, RoutedEventArgs e)
        {
            DM.Pause();
            _swapChainLoaded = false;
            _mpv.MediaPaused -= MediaPaused;
            _mpv.MediaResumed -= MediaResumed;
            _mpv.MediaLoaded -= MediaLoaded;
            _mpv.MediaFinished -= MediaFinished;
            _mpv.Dispose();
            //DisplayReq.RequestRelease();
        }

        void UpdateSize()
        {
            if (Host is null || !_swapChainLoaded || _currentMediaEnded || _mpv == null)
                return;

            lock (_criticalLock)
            {
                _mpv.SetPanelSize(_currentWidth, _currentHeight);
            }
        }

        void UpdateScale()
        {
            if (Host is null || !_swapChainLoaded || _currentMediaEnded)
                return;

            lock (_criticalLock2)
            {
                _mpv.SetPanelScale(_currentScaleX, _currentScaleY);
            }
        }

        public void Close()
        {
            _mpv.StopAsync();
            IsFullScreen = false;
        }

        public void Stop()
        {
            _mpv.StopAsync();
        }


        void StopInactiveTimer()
        {
            _inActiveTimer?.Stop();
        }

        void StartInactiveTimer()
        {
            _inActiveTimer?.Start();
        }

        DispatcherTimer InitInActiveTimer()
        {
            var t = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            t.Tick += InActiveTimerTick;
            return t;
        }

        private void InActiveTimerTick(object sender, object e)
        {
            _inActiveTimer.Stop();
            PlayBarVisible = false;
        }

        

        private void PlayButtonClick(object sender, RoutedEventArgs e)
        {
            if(_mpv.IsPlaying)
            {
                _mpv.Pause();
            }
            else
            {
                _mpv.Resume();
            }
        }

        private void PlaySlider_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _suppressCurrentTimeTrigger = true;
        }

        private async void PlaySlider_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _suppressCurrentTimeTrigger = false;
            TimeSpan to = GetToTime();
            await _mpv.SeekAsync(to);
            DM.Refresh();
        }

        private TimeSpan GetToTime()
        {
            if (CurrentPlayingDuration == null)
            {
                return TimeSpan.Zero;
            }
            var percent100 = (int)PlaySlider.Value;
            var v = percent100 * ((TimeSpan)CurrentPlayingDuration).Ticks / 100;
            var to = TimeSpan.FromTicks(v);
            return to;
        }

        private void PlayModeButtonClick(object sender, RoutedEventArgs e)
        {
            switch (PlayMode)
            {
                case PlayMode.Loop:
                    PlayMode = PlayMode.SingleLoop;
                    _mpv.Loop = true;
                    break;
                case PlayMode.SingleLoop:
                    PlayMode = PlayMode.Shuffle;
                    _mpv.Loop = true;
                    break;
                case PlayMode.Shuffle:
                    PlayMode = PlayMode.Loop;
                    _mpv.Loop = true;
                    break;
                default:
                    break;
            }
        }

        private void PlayNextButtonClick(object sender, RoutedEventArgs e)
        {
            if (CurrentPlayIndex != -1)
            {
                if (PlayMode == PlayMode.Loop)
                {
                    var index = CurrentPlayIndex + 1;
                    if (index >= CurrentPlayList.Count)
                    {
                        index = 0;
                    }
                    PlayNext(index);
                }
                else if (PlayMode == PlayMode.SingleLoop)
                {
                    PlayNext(CurrentPlayIndex);
                }
                else if (PlayMode == PlayMode.Shuffle)
                {
                    _random ??= new Random();
                    var index = _random.Next(CurrentPlayList.Count);
                    PlayNext(index);
                }
            }
        }

        void PlayNext(int index)
        {
            CurrentPlayIndex = index;
            _mpv.PlaylistPlayIndex(index);
        }


        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (!_currentMediaInited) return;
            PlayBarVisible = true;
        }

        private void PlayBar_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            StopInactiveTimer();
        }

        private void PlayBar_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            StartInactiveTimer();
        }

        private void Grid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_currentMediaInited) return;
            PlayBarVisible = true;
        }

        private void NavigateBackClick(object sender, RoutedEventArgs e)
        {
            App.NavigateBack();
        }

        private void Host_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            PlayBarVisible = !PlayBarVisible;
        }


        [DllImport("user32.dll", EntryPoint = "ShowCursor", CharSet = CharSet.Auto)]
        public extern static void ShowCursor(int status);



        private void ToggleFullScreenClick(object sender, RoutedEventArgs e)
        {
            OnToggleFullScreen?.Invoke();
            IsFullScreen = !IsFullScreen;
        }

        private void ToggleFullPageClick(object sender, RoutedEventArgs e)
        {
            OnToggleFullPage?.Invoke();
        }

        private Visibility GetTogglePlayListBarVisibility(bool isFullPageHost)
        {
            return isFullPageHost ? Visibility.Visible : Visibility.Collapsed;
        }

        private Visibility GetToggleFullPageVisibility(bool isFullPageHost)
        {
            return isFullPageHost ? Visibility.Collapsed : Visibility.Visible;
        }

        private void VideoPlayListBar_OnDismiss()
        {
            IsPlayListBarVisible = false;
        }

        private void TogglePlayListBarVisibilityClick(object sender, RoutedEventArgs e)
        {
            IsPlayListBarVisible = !IsPlayListBarVisible;
        }

    }

    [ComImport, Guid("63aad0b8-7c24-40ff-85a8-640d944cc325"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ISwapChainPanelNative
    {
        [PreserveSig]
        HRESULT SetSwapChain(IDXGISwapChain1 swapChain);
    }

}
