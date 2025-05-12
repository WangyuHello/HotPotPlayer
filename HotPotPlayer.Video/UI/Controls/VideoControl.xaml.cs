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
            PlaySlider.AddHandler(PointerReleasedEvent, new PointerEventHandler(PlaySlider_OnPointerReleased), true);
            PlaySlider.AddHandler(PointerPressedEvent, new PointerEventHandler(PlaySlider_OnPointerPressed), true);

            //Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
            IsPbpOn = Config.GetConfig("IsPbpOn", false);
        }

        private async void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            if (args.EventType.ToString().Contains("KeyUP") && _mediaInited)
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

        public bool IsFullScreen
        {
            get { return (bool)GetValue(IsFullScreenProperty); }
            set { SetValue(IsFullScreenProperty, value); }
        }

        public static readonly DependencyProperty IsFullScreenProperty =
            DependencyProperty.Register("IsFullScreen", typeof(bool), typeof(VideoControl), new PropertyMetadata(false));

        #endregion

        #region ObsProp
        [ObservableProperty]
        //[AlsoNotifyChangeFor(nameof(CurrentPlayItem))]
        public partial ObservableCollection<VideoItem> CurrentPlayList { get; set; }

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
        public partial List<string> Definitions { get; set; }

        [ObservableProperty]
        public partial string Title { get; set; }

        [ObservableProperty]
        public partial bool IsPlaying { get; set; }

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
        public partial TimeSpan? CurrentPlayingDuration { get; set; }

        [ObservableProperty]
        public partial PlayMode PlayMode { get; set; }

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
        bool selectedDefinitionGuard;
        public string SelectedDefinition
        {
            get => selectedDefinition;
            set => Set(ref selectedDefinition, value, nv =>
            {
                if (!selectedDefinitionGuard && !string.IsNullOrEmpty(nv))
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

        #endregion

        #region Prop
        MpvPlayer _mpv;
        bool _suppressCurrentTimeTrigger;
        DispatcherTimer _inActiveTimer;
        bool _mediaInited;
        AutoResetEvent _fence = new(false);

        IntPtr _devicePtr;
        ID3D11Device _device;
        IntPtr _swapChain1Ptr;
        IDXGISwapChain1 _swapChain1;
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
            //currentPlayList = new ObservableCollection<VideoItem>(info.VideoItems);
            currentPlayIndex = info.Index;
            StartPlay(immediateInit: info.ImmediateLoad);
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
            DisposeMpv();
            //DisplayReq.RequestRelease();
        }

        void UpdateSize()
        {
            if (Host is null || !_swapChainLoaded ||  _mpv == null)
                return;

            lock (_criticalLock)
            {
                _mpv.SetPanelSize(_currentWidth, _currentHeight);
            }
        }

        void UpdateScale()
        {
            if (Host is null || !_swapChainLoaded || _mpv == null)
                return;

            lock (_criticalLock2)
            {
                _mpv.SetPanelScale(_currentScaleX, _currentScaleY);
            }
        }

        public void Close()
        {
            _mpv?.StopAsync();
        }

        public void Stop()
        {
            _mpv?.StopAsync();
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
            if(_mpv == null)
            {
                StartPlay(immediateInit: true);
            }
            else if(_mpv.IsPlaying)
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
            if (!_mediaInited) return;
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
            if (!_mediaInited) return;
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


}
