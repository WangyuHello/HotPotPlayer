using CommunityToolkit.Mvvm.ComponentModel;
using DirectN;
using Google.Protobuf.WellKnownTypes;
using HotPotPlayer.Extensions;
using HotPotPlayer.Models;
using HotPotPlayer.Services;
using Jellyfin.Sdk.Generated.Models;
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using WinRT;
using PlayerState = HotPotPlayer.Models.PlayerState;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Video.UI.Controls
{
    public sealed partial class VideoHost : UserControlBase
    {
        public VideoHost()
        {
            this.InitializeComponent();
            PlaySlider.AddHandler(PointerReleasedEvent, new PointerEventHandler(PlaySlider_OnPointerReleased), true);
            PlaySlider.AddHandler(PointerPressedEvent, new PointerEventHandler(PlaySlider_OnPointerPressed), true);
            _uiQueue = DispatcherQueue;
        }

        public event Action OnToggleFullScreen;

        readonly DispatcherQueue _uiQueue;
        DispatcherTimer _inActiveTimer;

        int _currentWidth;
        int _currentHeight;
        float _currentScaleX;
        float _currentScaleY;
        Rectangle _currentWindowBounds;
        Interop.IDXGISwapChain1 _swapchain;
        Interop.ISwapChainPanelNative _swapChainPanelNative;

        bool _isSwapchainInited = false;

        private void Host_CompositionScaleChanged(SwapChainPanel sender, object args)
        {
            if (Host.CompositionScaleX == 0 || Host.CompositionScaleY == 0) return;
            _currentScaleX = Host.CompositionScaleX;
            _currentWidth = (int)Math.Ceiling(Host.CompositionScaleX * Host.ActualWidth);
            _currentScaleY = Host.CompositionScaleY;
            _currentHeight = (int)Math.Ceiling(Host.CompositionScaleY * Host.ActualHeight);
            if (_isSwapchainInited)
            {
                VideoPlayer.UpdatePanelScale(_currentScaleX, _currentScaleY);
                VideoPlayer.UpdatePanelSize(_currentWidth, _currentHeight);
            }
        }

        private void Host_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Host.ActualWidth == 0 || Host.ActualHeight == 0) return;
            _currentWidth = (int)Math.Ceiling(Host.CompositionScaleX * Host.ActualWidth);
            _currentHeight = (int)Math.Ceiling(Host.CompositionScaleY * Host.ActualHeight);
            _currentWindowBounds = new Rectangle { X = (int)App.Bounds.Left, Y = (int)App.Bounds.Right, Width = 640, Height = 480 };
            if (_isSwapchainInited)
            {
                VideoPlayer.UpdatePanelBounds(_currentWindowBounds);
                VideoPlayer.UpdatePanelSize(_currentWidth, _currentHeight);
            }
        }

        private void UserControlBase_Loaded(object sender, RoutedEventArgs e)
        {
            if (VideoPlayer.SwapChain != IntPtr.Zero)
            {
                _swapchain = (Interop.IDXGISwapChain1)Marshal.GetObjectForIUnknown(VideoPlayer.SwapChain);
                _swapChainPanelNative = Host.As<Interop.ISwapChainPanelNative>();
                _swapChainPanelNative.SetSwapChain(_swapchain);
                _isSwapchainInited = true;
            }
            else
            {
                _isSwapchainInited = false;
            }
            GetScalingFactor(out _currentScaleX, out _currentScaleY);
            VideoPlayer.SwapChainInited += VideoPlayer_SwapchainInited;
            VideoPlayer.VideoGeometryInit += VideoPlayer_VideoGeometryInit;
            VideoPlayer.DanmakuInit += VideoPlayer_DanmakuInit;
        }

        private Grid VideoPlayer_DanmakuInit()
        {
            return DanmakuHost;
        }

        private void UserControlBase_Unloaded(object sender, RoutedEventArgs e)
        {
            VideoPlayer.SwapChainInited -= VideoPlayer_SwapchainInited;
            VideoPlayer.VideoGeometryInit -= VideoPlayer_VideoGeometryInit;
            VideoPlayer.DanmakuInit -= VideoPlayer_DanmakuInit;
            _isSwapchainInited = false;
        }

        private void VideoPlayer_SwapchainInited(object sender, nint ptr)
        {
            _isSwapchainInited = true;
            _uiQueue.TryEnqueue(() =>
            {
                _swapchain = (Interop.IDXGISwapChain1)Marshal.GetObjectForIUnknown(ptr);
                _swapChainPanelNative = Host.As<Interop.ISwapChainPanelNative>();
                _swapChainPanelNative.SetSwapChain(_swapchain);
                //_isSwapchainInited = true;
                PlayBarVisible = true;
                VideoPlayer.OnSwapChainConfigured();
            });
        }

        private void VideoPlayer_VideoGeometryInit(object sender, MpvVideoGeometryInitEventArgs args)
        {
            args.Width = _currentWidth;
            args.Height = _currentHeight;
            args.ScaleX = _currentScaleX;
            args.ScaleY = _currentScaleY;
            args.Bounds = _currentWindowBounds;
            Debug.WriteLine($"{_currentWidth} {_currentHeight} {_currentScaleX} {_currentScaleY}");
        }

        public void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Left:
                    VideoPlayer.PlayStepBackward(TimeSpan.FromSeconds(10));
                    break;
                case Windows.System.VirtualKey.Right:
                    VideoPlayer.PlayStepForward(TimeSpan.FromSeconds(10));
                    break;
                default:
                    break;
            }
        }

        [ObservableProperty]
        public partial bool IsFullScreen {  get; set; }

        private bool isVideoInfoOn;
        public bool IsVideoInfoOn
        {
            get => isVideoInfoOn;
            set => Set(ref isVideoInfoOn, value, nv =>
            {
                VideoPlayer.Command("script-binding", "stats/display-stats-toggle");
            });
        }

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

        [DllImport("user32.dll", EntryPoint = "ShowCursor", CharSet = CharSet.Auto)]
        public extern static void ShowCursor(int status);

        public bool IsFullPage
        {
            get { return (bool)GetValue(IsFullPageProperty); }
            set { SetValue(IsFullPageProperty, value); }
        }

        public static readonly DependencyProperty IsFullPageProperty =
            DependencyProperty.Register("IsFullPage", typeof(bool), typeof(VideoControl), new PropertyMetadata(true));

        public bool IsFullPageHost
        {
            get { return (bool)GetValue(IsFullPageHostProperty); }
            set { SetValue(IsFullPageHostProperty, value); }
        }

        public static readonly DependencyProperty IsFullPageHostProperty =
            DependencyProperty.Register("IsFullPageHost", typeof(bool), typeof(VideoHost), new PropertyMetadata(true));

        private void PlayButtonClick(object sender, RoutedEventArgs e)
        {
            VideoPlayer.PlayOrPause();
        }

        private void PlayPreviousButtonClick(object sender, RoutedEventArgs e)
        {
            VideoPlayer.PlayPrevious();
        }

        private void PlayNextButtonClick(object sender, RoutedEventArgs e)
        {
            VideoPlayer.PlayNextInPlayList();
        }

        private void PlayModeButtonClick(object sender, RoutedEventArgs e)
        {
            VideoPlayer.TogglePlayMode();
        }

        private void ToggleFullScreenClick(object sender, RoutedEventArgs e)
        {
            IsFullScreen = !IsFullScreen;
            OnToggleFullScreen?.Invoke();
            AppWindow.SetPresenter(IsFullScreen ? AppWindowPresenterKind.FullScreen : AppWindowPresenterKind.Default);
        }
        private void ToggleFullPageClick(object sender, RoutedEventArgs e)
        {
            if(VideoPlayer.VisualState == VideoPlayVisualState.FullHost)
            {
                VideoPlayer.VisualState = VideoPlayVisualState.FullWindow;
            }
            else
            {
                VideoPlayer.VisualState = VideoPlayVisualState.FullHost;
            }
        }

        private void TogglePlayListBarVisibilityClick(object sender, RoutedEventArgs e)
        {
            IsPlayListBarVisible = !IsPlayListBarVisible;
        }

        private void PlaySlider_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            VideoPlayer.SuppressCurrentTimeTrigger = true;
        }

        private void PlaySlider_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            VideoPlayer.SuppressCurrentTimeTrigger = false;
            TimeSpan to = GetToTime();
            VideoPlayer.PlayTo(to);
        }

        private void VideoPlayListBar_PlayListItemClicked(object sender, ItemClickEventArgs e)
        {
            var v = e.ClickedItem as BaseItemDto;
            VideoPlayer.PlayNextInCurrentList(v);
        }

        private TimeSpan GetToTime()
        {
            if (VideoPlayer.CurrentPlayingDuration == null)
            {
                return TimeSpan.Zero;
            }
            var percent100 = (int)PlaySlider.Value;
            var v = percent100 * ((TimeSpan)VideoPlayer.CurrentPlayingDuration).Ticks / 100;
            var to = TimeSpan.FromTicks(v);
            return to;
        }

        string GetFullScreenIcon(bool isFullScreen)
        {
            return isFullScreen ? "\uE1D8" : "\uE1D9";
        }

        string GetFullPageIcon(bool isFullPage)
        {
            return isFullPage ? "\uE744" : "\uE9A6";
        }

        string GetDuration(TimeSpan? duration)
        {
            if (duration == null)
            {
                return "--:--";
            }
            var dur = (TimeSpan)duration;
            if (dur.Hours > 0)
            {
                return dur.ToString("hh\\:mm\\:ss");
            }
            return dur.ToString("mm\\:ss");
        }

        double GetSliderValue(TimeSpan current, TimeSpan? total)
        {
            if (total == null)
            {
                return 0;
            }
            if (total.Value.Ticks == 0)
            {
                return 0;
            }
            return 100 * current.Ticks / ((TimeSpan)total).Ticks;
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

        string GetPlayButtonIcon(bool isPlaying, bool hasError)
        {
            if (hasError)
            {
                return "\uE106";
            }
            return isPlaying ? "\uE103" : "\uF5B0";
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

        public Visibility GetPlayBarVisible(bool playBarVisible, VideoPlayVisualState state)
        {
            return (playBarVisible && (state == VideoPlayVisualState.FullHost || 
                state == VideoPlayVisualState.FullWindow ||
                state == VideoPlayVisualState.FullScreen)) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility GetTitleBarVisible(bool playBarVisible, VideoPlayVisualState state)
        {
            return (playBarVisible && (state == VideoPlayVisualState.FullHost ||
                state == VideoPlayVisualState.FullWindow ||
                state == VideoPlayVisualState.FullScreen ||
                state == VideoPlayVisualState.SmallHost)) ? Visibility.Visible : Visibility.Collapsed;
        }

        public SolidColorBrush GetTitleBarBackground(bool isFullPageHost, VideoPlayVisualState state)
        {
            var dark = new SolidColorBrush(new Windows.UI.Color 
            { 
                A = 0xe0,
                R = 0x42,
                G = 0x40,
                B = 0x47
            });
            var trans = new SolidColorBrush(Windows.UI.Color.FromArgb(0,0,0,0));
            if (!isFullPageHost)
            {
                return state == VideoPlayVisualState.FullHost ? trans : dark;
            }
            else
            {
                return dark;
            }
        }

        public Visibility GetTitleBarTitleVisible(bool isFullPageHost, VideoPlayVisualState state)
        {
            if (!isFullPageHost)
            {
                return state == VideoPlayVisualState.FullHost ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public Visibility GetTitleBarPlayButtonVisible(VideoPlayVisualState state)
        {
            return state switch
            {
                VideoPlayVisualState.SmallHost => Visibility.Visible,
                _ => Visibility.Collapsed,
            };
        }

        private Visibility GetLoadingVisible(PlayerState state)
        {
            return (state == PlayerState.Loading) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (!_isSwapchainInited) { return; }
            PlayBarVisible = true;
        }

        List<string> _tempSubtitleList;
        private List<string> GetSubtitleList(List<MediaStream> mediaStreams)
        {
            _tempSubtitleList = new List<string>
            {
                "关闭"
            };
            _tempSubtitleList.AddRange(mediaStreams.Where(m => m.IsTextSubtitleStream.Value).Select(m => m.DisplayTitle));
            return _tempSubtitleList;
        }

        private int GetSubtitleSelectedIndex(List<MediaStream> mediaStreams)
        {
            return _tempSubtitleList.Count == 1 ? 0 : 1; ;
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
            if (!_isSwapchainInited) { return; }
            PlayBarVisible = true;
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

        private void NavigateBackClick(object sender, RoutedEventArgs e)
        {
            // Fake page, do not need navigation
            VideoPlayer.VisualState = VideoPlayVisualState.TinyHidden;
            //_isSwapchainInited = false;
            //_swapChainPanelNative = Host.As<ISwapChainPanelNative>();
            //_swapChainPanelNative.SetSwapChain(null);
            Task.Run(VideoPlayer.PauseAsStop);
        }

        private void ToggleSmallWindowClick(object sender, RoutedEventArgs e)
        {
            if(VideoPlayer.VisualState == VideoPlayVisualState.SmallHost)
            {
                VideoPlayer.VisualState = VideoPlayVisualState.FullHost;
            }
            else
            {
                VideoPlayer.VisualState = VideoPlayVisualState.SmallHost;
            }
        }

        private const int MDT_EFFECTIVE_DPI = 0;

        private void GetScalingFactor(out float scaleX, out float scaleY)
        {
            IntPtr monitor = MonitorFromWindow(App.MainWindowHandle, 2); // Get the primary monitor
            uint dpiX, dpiY;
            GetDpiForMonitor(monitor, MDT_EFFECTIVE_DPI, out dpiX, out dpiY);

            scaleX = dpiX / 96.0f;
            scaleY = dpiY / 96.0f;
        }

        [DllImport("User32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, int flags);

        [DllImport("Shcore.dll")]
        private static extern int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

        private void Subtitle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var box = sender as ComboBox;
            var i = box.SelectedIndex;
            if (i == -1)
            {

            }
            else if (i == 0) 
            {
                VideoPlayer.SetPropertyString("sid", "no");
            }
            else
            {
                VideoPlayer.SetPropertyString("sid", "auto");
            }
        }
    }
}
