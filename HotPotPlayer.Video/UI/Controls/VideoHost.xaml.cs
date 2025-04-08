using DirectN;
using HotPotPlayer.Models;
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
        }

        int _currentWidth;
        int _currentHeight;
        float _currentScaleX;
        float _currentScaleY;
        Rectangle _currentWindowBounds;

        bool _isSwapchainInited = false;

        private void Host_CompositionScaleChanged(SwapChainPanel sender, object args)
        {
            _currentScaleX = Host.CompositionScaleX;
            _currentScaleY = Host.CompositionScaleY;
            _currentWidth = (int)Math.Ceiling(Host.CompositionScaleX * Host.ActualWidth);
            _currentHeight = (int)Math.Ceiling(Host.CompositionScaleY * Host.ActualHeight);
            if (_isSwapchainInited)
            {
                VideoPlayer.UpdatePanelScale(_currentScaleX, _currentScaleY);
                VideoPlayer.UpdatePanelSize(_currentWidth, _currentHeight);
            }
        }

        private void Host_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _currentWidth = (int)Math.Ceiling(Host.CompositionScaleX * Host.ActualWidth);
            _currentHeight = (int)Math.Ceiling(Host.CompositionScaleY * Host.ActualHeight);
            _currentWindowBounds = new System.Drawing.Rectangle { X = (int)App.Bounds.Left, Y = (int)App.Bounds.Right, Width = 640, Height = 480 };
            if (_isSwapchainInited)
            {
                VideoPlayer.UpdatePanelSize(_currentWidth, _currentHeight);
            }
        }

        private void UserControlBase_Loaded(object sender, RoutedEventArgs e)
        {
            if (VideoPlayer.SwapChain != IntPtr.Zero)
            {
                var swapchain1 = (IDXGISwapChain1)Marshal.GetObjectForIUnknown(VideoPlayer.SwapChain);
                var nativepanel = Host.As<ISwapChainPanelNative>();
                nativepanel.SetSwapChain(swapchain1);
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
            var swapchain1 = (IDXGISwapChain1)Marshal.GetObjectForIUnknown(ptr);
            var nativepanel = Host.As<ISwapChainPanelNative>();
            UIQueue.TryEnqueue(() =>
            {
                nativepanel.SetSwapChain(swapchain1);
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
    }

}
