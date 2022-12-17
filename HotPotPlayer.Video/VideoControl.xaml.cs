using CommunityToolkit.Mvvm.ComponentModel;
using DirectN;
using HotPotPlayer.Extensions;
using HotPotPlayer.Models;
using HotPotPlayer.Models.BiliBili;
using HotPotPlayer.Services;
using HotPotPlayer.Services.BiliBili;
using HotPotPlayer.Video.Extensions;
using HotPotPlayer.Video.GlesInterop;
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
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Display;
using Windows.UI.Core;
using WinRT;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Video
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
        }

        private async void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            if (args.EventType.ToString().Contains("KeyUP") && mediaInited)
            {
                var virtualKey = args.VirtualKey;
                switch (virtualKey)
                {
                    case Windows.System.VirtualKey.LeftButton:
                        break;
                    case Windows.System.VirtualKey.RightButton:
                        break;
                    case Windows.System.VirtualKey.Left:
                        await Mpv.SeekAsync(-10, true);
                        break;
                    case Windows.System.VirtualKey.Up:
                        break;
                    case Windows.System.VirtualKey.Right:
                        await Mpv.SeekAsync(10, true);
                        break;
                    case Windows.System.VirtualKey.Down:
                        break;
                    default:
                        break;
                }
            }
        }

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
            h.currentPlayList = new ObservableCollection<VideoItem>(info.VideoItems);
            h.currentPlayIndex = info.Index;
        }

        public bool NoTitleBar
        {
            get { return (bool)GetValue(NoTitleBarProperty); }
            set { SetValue(NoTitleBarProperty, value); }
        }

        public static readonly DependencyProperty NoTitleBarProperty =
            DependencyProperty.Register("NoTitleBar", typeof(bool), typeof(VideoControl), new PropertyMetadata(default));


        bool mediaInited;
        //DisplayRequest _displayReq;
        //DisplayRequest DisplayReq => _displayReq;
        MpvPlayer _mpv;

        MpvPlayer Mpv
        {
            get
            {
                if (_mpv == null)
                {

                    //_mpv = new MpvPlayer(@"NativeLibs\mpv-2.dll")
                    _mpv = new MpvPlayer(App.MainWindowHandle, @"NativeLibs\mpv-2.dll", (int)Math.Ceiling(Host.CompositionScaleX * Host.ActualWidth), (int)Math.Ceiling(Host.CompositionScaleY*Host.ActualHeight), Host.CompositionScaleX, Host.CompositionScaleY, new System.Drawing.Rectangle { X = (int)App.Bounds.Left, Y = (int)App.Bounds.Right, Width = (int)App.Bounds.Top, Height = (int)App.Bounds.Bottom})
                    {
                        AutoPlay = true,
                        Volume = 100,
                        LogLevel = MpvLogLevel.Debug,
                        Loop = false,
                        LoopPlaylist = true,
                    };
                    _mpv.SetD3DInitCallback(D3DInitCallback);
                    _mpv.MediaResumed += MediaResumed;
                    _mpv.MediaPaused += MediaPaused;
                    _mpv.MediaLoaded += MediaLoaded;
                    _mpv.MediaFinished += MediaFinished;
                    _mpv.PositionChanged += PositionChanged;
                }

                return _mpv;
            }
        }

        private void PositionChanged(object sender, MpvPlayerPositionChangedEventArgs e)
        {
            UIQueue.TryEnqueue(() =>
            {
                CurrentTime = e.NewPosition;
                //CurrentPlayingDuration = Mpv.Duration;
            });
        }

        private void MediaFinished(object sender, EventArgs e)
        {
            UIQueue.TryEnqueue(() => IsPlaying = false);
            //DisplayReq.RequestRelease();
        }

        public event Action OnMediaLoaded;

        private async void MediaLoaded(object sender, EventArgs e)
        {
            App?.Taskbar.AddPlayButtons();
            //DisplayReq.RequestActive();

            UIQueue.TryEnqueue(() => 
            {
                Title = CurrentPlayList[CurrentPlayIndex].Title;
                IsPlaying = true;
                CurrentPlayingDuration = _mpv.Duration;
                OnPropertyChanged(propertyName: nameof(Volume));
                OnMediaLoaded?.Invoke();

            });

            await Task.Run(async () =>
            {
                await Task.Delay(1000);
                mediaInited = true;
                UIQueue.TryEnqueue(() => PlayBarVisible = true);
            });
        }

        private void MediaPaused(object sender, EventArgs e)
        {
            UIQueue.TryEnqueue(() => IsPlaying = false);
            //DisplayReq.RequestRelease();
        }

        private void MediaResumed(object sender, EventArgs e)
        {
            UIQueue.TryEnqueue(() => IsPlaying = true);
            //DisplayReq.RequestActive();
        }

        ID3D11Device _device;
        IDXGISwapChain1 _swapChain1;
        readonly DispatcherQueue UIQueue;

        private void D3DInitCallback(IntPtr d3d11Device, IntPtr swapChain)
        {
            UIQueue.TryEnqueue(() =>
            {
                _swapChain1 = (IDXGISwapChain1)Marshal.GetObjectForIUnknown(swapChain);
                _device = (ID3D11Device)Marshal.GetObjectForIUnknown(d3d11Device);
                //_swapChain1 = ObjectReference<IDXGISwapChain1>.FromAbi(swapChain).Vftbl;
                var nativepanel = Host.As<ISwapChainPanelNative>();
                _swapChain1.GetDesc1(out var desp);
                nativepanel.SetSwapChain(_swapChain1);
                _swapChainLoaded = true;
            });
        }

        public void StartPlay(string selectedDefinition = "")
        {
            //Mpv.API.SetPropertyString("vo", "gpu");
            Mpv.API.SetPropertyString("vo", "gpu-next");
            Mpv.API.SetPropertyString("gpu-context", "d3d11");
            Mpv.API.SetPropertyString("hwdec", "d3d11va");
            Mpv.API.SetPropertyString("d3d11-composition", "yes");
            Mpv.API.SetPropertyString("target-colorspace-hint", "yes"); //HDR passthrough

            if (CurrentPlayList[CurrentPlayIndex] is BiliBiliVideoItem bv)
            {
                Mpv.API.SetPropertyString("user-agent", BiliAPI.UserAgent);
                Mpv.API.SetPropertyString("cookies", "yes");
                Mpv.API.SetPropertyString("ytdl", "no");
                Mpv.API.SetPropertyString("cookies-file", GetCookieFile());
                Mpv.API.SetPropertyString("http-header-fields", "Referer: http://www.bilibili.com/");
                //Mpv.API.SetPropertyString("demuxer-lavf-o", $"headers=\"Referer: http://www.bilibili.com/\r\nUserAgent: {BiliAPI.UserAgent}\r\n\"");

                IEnumerable<string> videourls;
                if (bv.DashVideos == null)
                {
                    videourls = CurrentPlayList.Cast<BiliBiliVideoItem>().Select(b => b.Urls[0].Url);
                    Mpv.LoadPlaylist(videourls);
                }
                else
                {
                    var mpd = bv.WriteToMPD(Config);
                    (var sel, BiliBiliService.Proxy.VideoUrl) = bv.GetPreferVideoUrl(selectedDefinition);
                    if (!mediaInited)
                    {
                        SelectedDefinition = sel;
                        Definitions = bv.Videos.Keys.ToList();
                    }
                    BiliBiliService.Proxy.AudioUrl = bv.GetPreferAudioUrl();
                    BiliBiliService.Proxy.CookieString = BiliBiliService.API.CookieString;
                    if (sel.Contains("杜比") || sel.Contains("HDR")) Mpv.API.SetPropertyString("vo", "gpu");
                    //Mpv.Load(BiliBiliService.Proxy.VideoUrl);
                    Mpv.Load(mpd);
                }
            }
            else
            {
                Mpv.LoadPlaylist(CurrentPlayList.Select(f => f.Source.FullName));
            }
            Mpv.PlaylistPlayIndex(CurrentPlayIndex);

            //Mpv.Resume();
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
            if (CurrentPlayList != null)
                StartPlay();
            //_displayReq = new DisplayRequest();
        }

        private void Host_CompositionScaleChanged(SwapChainPanel sender, object args)
        {
            CurrentCompositionScaleX = Host.CompositionScaleX;
            CurrentCompositionScaleY = Host.CompositionScaleY;
            CurrentWidth = (int)Math.Ceiling(Host.CompositionScaleX * Host.ActualWidth);
            CurrentHeight = (int)Math.Ceiling(Host.CompositionScaleY * Host.ActualHeight);
            UpdateScale();
            UpdateSize();
        }

        private void Host_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CurrentWidth = (int)Math.Ceiling(Host.CompositionScaleX*Host.ActualWidth);
            CurrentHeight = (int)Math.Ceiling(Host.CompositionScaleY*Host.ActualHeight);
            UpdateSize();
        }

        private void Host_Unloaded(object sender, RoutedEventArgs e)
        {
            _swapChainLoaded = false;
            Mpv.MediaPaused -= MediaPaused;
            Mpv.MediaResumed -= MediaResumed;
            Mpv.MediaLoaded -= MediaLoaded;
            Mpv.MediaFinished -= MediaFinished;
            Mpv.Dispose();
            //DisplayReq.RequestRelease();
        }

        bool _swapChainLoaded;

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
                    Mpv.PlaylistPlayIndex(currentPlayIndex);
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
                if(isPlayListBarVisible != value)
                {
                    Set(ref  isPlayListBarVisible, value);
                    AppWindow.SetTitleBarForegroundColor(isPlayListBarVisible);
                }
            }
        }

        [ObservableProperty]
        private List<string> definitions;

        object _CriticalLock = new object();
        object _CriticalLock2 = new object();
        int CurrentWidth;
        int CurrentHeight;
        float CurrentCompositionScaleX;
        float CurrentCompositionScaleY;

        void UpdateSize()
        {
            if (Host is null || !_swapChainLoaded)
                return;

            lock (_CriticalLock)
            {
                Mpv.SetPanelSize(CurrentWidth, CurrentHeight);
            }
        }

        void UpdateScale()
        {
            if (Host is null || !_swapChainLoaded)
                return;

            lock (_CriticalLock2)
            {
                Mpv.SetPanelScale(CurrentCompositionScaleX, CurrentCompositionScaleY);
            }
        }

        public void Close()
        {
            Mpv.Stop();
            IsFullScreen = false;
        }

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private bool isPlaying;

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

        DispatcherTimer _inActiveTimer;

        private bool playBarVisible;

        public bool PlayBarVisible
        {
            get => playBarVisible;
            set 
            {
                Set(ref playBarVisible, value, newV =>
                {
                    if(!NoTitleBar)
                    {
                        TitleBarVisible = value;
                        OnPropertyChanged(propertyName: nameof(TitleBarVisible));
                    }
                    ShowCursor(newV ? 1 : 0);
                    if (!NoTitleBar)
                    {
                        AppWindow.SetTitleBarForegroundColor(newV);
                    }
                    if (newV == true)
                    {
                        _inActiveTimer ??= InitInActiveTimer();
                        _inActiveTimer.Start();
                    }
                });
            }
        }

        public bool TitleBarVisible { get; set; }

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

        string GetPlayButtonIcon(bool isPlaying, bool hasError)
        {
            if (hasError)
            {
                return "\uE106";
            }
            return isPlaying ? "\uE103" : "\uF5B0";
        }

        double GetSliderValue(TimeSpan current, TimeSpan? total)
        {
            if (total == null)
            {
                return 0;
            }
            return 100 * current.Ticks / ((TimeSpan)total).Ticks;
        }

        string GetDuration(TimeSpan? duration)
        {
            if (duration == null)
            {
                return "--:--";
            }
            return ((TimeSpan)duration).ToString("mm\\:ss");
        }

        private void PlayButtonClick(object sender, RoutedEventArgs e)
        {
            if(Mpv.IsPlaying)
            {
                Mpv.Pause();
            }
            else
            {
                Mpv.Resume();
            }
        }

        private void ToggleStatusClick(object sender, RoutedEventArgs e)
        {
            Mpv.API.Command("script-binding", "stats/display-stats-toggle");
        }

        private void PlaySlider_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            SuppressCurrentTimeTrigger = true;
        }

        private async void PlaySlider_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            SuppressCurrentTimeTrigger = false;
            TimeSpan to = GetToTime();
            await _mpv.SeekAsync(to);
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

        Random random;
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
                    random ??= new Random();
                    var index = random.Next(CurrentPlayList.Count);
                    PlayNext(index);
                }
            }
        }

        void PlayNext(int index)
        {
            CurrentPlayIndex = index;
            Mpv.PlaylistPlayIndex(index);
        }

        const string Loop = "\uE1CD";
        const string SingleLoop = "\uE1CC";
        const string Shuffle = "\uE8B1";
        string GetPlayModeIcon(PlayMode playMode)
        {
            return playMode switch
            {
                PlayMode.Loop => Loop,
                PlayMode.SingleLoop => SingleLoop,
                PlayMode.Shuffle => Shuffle,
                _ => Loop,
            };
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (!mediaInited) return;
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
            if (!mediaInited) return;
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

        string GetFullScreenIcon(bool isFullScreen)
        {
            return isFullScreen ? "\uE1D8" : "\uE1D9";
        }



        public bool IsFullPage
        {
            get { return (bool)GetValue(IsFullPageProperty); }
            set { SetValue(IsFullPageProperty, value); }
        }

        public static readonly DependencyProperty IsFullPageProperty =
            DependencyProperty.Register("IsFullPage", typeof(bool), typeof(VideoControl), new PropertyMetadata(default));


        string GetFullPageIcon(bool isFullPage)
        {
            return isFullPage ? "\uE744" : "\uE9A6";
        }

        public event Action OnToggleFullScreen;
        private void ToggleFullScreenClick(object sender, RoutedEventArgs e)
        {
            OnToggleFullScreen?.Invoke();
            IsFullScreen = !IsFullScreen;
        }

        public event Action OnToggleFullPage;
        private void ToggleFullPageClick(object sender, RoutedEventArgs e)
        {
            OnToggleFullPage?.Invoke();
        }

        private Visibility GetTogglePlayListBarVisibility(bool notitlebar)
        {
            return notitlebar ? Visibility.Collapsed : Visibility.Visible;
        }

        private Visibility GetToggleFullPageVisibility(bool notitlebar)
        {
            return notitlebar ? Visibility.Visible : Visibility.Collapsed;
        }

        private void VideoPlayListBar_OnDismiss()
        {
            IsPlayListBarVisible = false;
        }

        private void TogglePlayListBarVisibilityClick(object sender, RoutedEventArgs e)
        {
            IsPlayListBarVisible = !IsPlayListBarVisible;
        }

        
        private string selectedDefinition;

        public string SelectedDefinition
        {
            get => selectedDefinition;
            set => Set(ref selectedDefinition, value, nv =>
            {
                if(mediaInited && !string.IsNullOrEmpty(nv))
                {
                    StartPlay(nv);
                }
            });
        }
    }

    [ComImport, Guid("63aad0b8-7c24-40ff-85a8-640d944cc325"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ISwapChainPanelNative
    {
        [PreserveSig]
        HRESULT SetSwapChain(IDXGISwapChain1 swapChain);
    }

}
