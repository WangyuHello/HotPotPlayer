using CommunityToolkit.Mvvm.ComponentModel;
using DirectN;
using HotPotPlayer.Models;
using HotPotPlayer.Services;
using Microsoft.UI.Dispatching;
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Video.UI.Controls
{
    public sealed partial class VideoHost : UserControlBase
    {
        public VideoHost()
        {
            this.InitializeComponent();
            _uiQueue = DispatcherQueue.GetForCurrentThread();
        }

        private DispatcherQueue _uiQueue;

        int _currentWidth;
        int _currentHeight;
        float _currentScaleX;
        float _currentScaleY;
        Rectangle _currentWindowBounds;
        IDXGISwapChain1 _swapchain;
        ISwapChainPanelNative _swapChainPanelNative;

        bool _isSwapchainInited = false;

        private void Host_CompositionScaleChanged(SwapChainPanel sender, object args)
        {
            _currentScaleX = Host.CompositionScaleX;
            _currentScaleY = Host.CompositionScaleY;
            _currentWidth = (int)Math.Ceiling(Host.CompositionScaleX * Host.ActualWidth);
            _currentHeight = (int)Math.Ceiling(Host.CompositionScaleY * Host.ActualHeight);
            //if (_isSwapchainInited)
            //{
            //    VideoPlayer.UpdatePanelScale(_currentScaleX, _currentScaleY);
            //    VideoPlayer.UpdatePanelSize(_currentWidth, _currentHeight);
            //}
        }

        private void Host_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _currentWidth = (int)Math.Ceiling(Host.CompositionScaleX * Host.ActualWidth);
            _currentHeight = (int)Math.Ceiling(Host.CompositionScaleY * Host.ActualHeight);
            _currentWindowBounds = new System.Drawing.Rectangle { X = (int)App.Bounds.Left, Y = (int)App.Bounds.Right, Width = 640, Height = 480 };
            //if (_isSwapchainInited)
            //{
            //    VideoPlayer.UpdatePanelSize(_currentWidth, _currentHeight);
            //}
        }

        private void UserControlBase_Loaded(object sender, RoutedEventArgs e)
        {
            if (VideoPlayer.SwapChain != IntPtr.Zero)
            {
                _swapchain = (IDXGISwapChain1)Marshal.GetObjectForIUnknown(VideoPlayer.SwapChain);
                _swapChainPanelNative = Host.As<ISwapChainPanelNative>();
                _swapChainPanelNative.SetSwapChain(_swapchain);
                _isSwapchainInited = true;
            }
            else
            {
                _isSwapchainInited = false;
            }
            VideoPlayer.SwapChainInited += VideoPlayer_SwapchainInited;
            VideoPlayer.VideoGeometryInit += VideoPlayer_VideoGeometryInit;
        }

        private void UserControlBase_Unloaded(object sender, RoutedEventArgs e)
        {
            VideoPlayer.SwapChainInited -= VideoPlayer_SwapchainInited;
            VideoPlayer.VideoGeometryInit -= VideoPlayer_VideoGeometryInit;
            _isSwapchainInited = false;
        }

        private void VideoPlayer_SwapchainInited(object sender, nint ptr)
        {
            _uiQueue.TryEnqueue(() =>
            {
                _swapchain = (IDXGISwapChain1)Marshal.GetObjectForIUnknown(ptr);
                _swapChainPanelNative = Host.As<ISwapChainPanelNative>();
                _swapChainPanelNative.SetSwapChain(_swapchain);
                _isSwapchainInited = true;
            });
        }

        private void VideoPlayer_VideoGeometryInit(object sender, MpvVideoGeometryInitEventArgs args)
        {
            args.Width = _currentWidth;
            args.Height = _currentHeight;
            args.ScaleX = _currentScaleX;
            args.ScaleY = _currentScaleY;
            args.Bounds = _currentWindowBounds;
        }

        [ObservableProperty]
        private bool isFullPage;

        [ObservableProperty]
        private bool isFullScreen;


        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(VideoHost), new PropertyMetadata(default(string)));


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
            VideoPlayer.PlayNext();
        }

        private void PlayModeButtonClick(object sender, RoutedEventArgs e)
        {
            VideoPlayer.TogglePlayMode();
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

        private void NavigateBackClick(object sender, RoutedEventArgs e)
        {
            App.NavigateBack();
        }

    }

}
