using HotPotPlayer.Video.GlesInterop;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Mpv.NET.API;
using Mpv.NET.Player;
using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Video
{
    public sealed partial class VideoControl : UserControl
    {
        public VideoControl()
        {
            this.InitializeComponent();
            lastCompositionScaleX = Host.CompositionScaleX;
            lastCompositionScaleY = Host.CompositionScaleY;
            ContentsScale = Host.CompositionScaleX;
        }

        public FileInfo Source
        {
            get { return (FileInfo)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(FileInfo), typeof(VideoControl), new PropertyMetadata(default(FileInfo), SourceChanged));

        private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var h = (VideoControl)d;
            h.StartPlay((FileInfo)e.NewValue);
        }

        public IntPtr Hwnd { get; set; }

        GlesContext glcontext;
        MpvPlayer _mpv;
        DispatcherQueueController _eventLoop;
        DispatcherQueue _eventQueue;

        private bool isVisible = true;
        private bool isLoaded = false;
        private double lastCompositionScaleX = 0.0;
        private double lastCompositionScaleY = 0.0;
        public double ContentsScale { get; private set; }

        private bool pendingSizeChange = false;

        MpvPlayer Mpv
        {
            get
            {
                if (_mpv == null)
                {
                    _mpv = new MpvPlayer(@"NativeLibs\mpv-2.dll", true)
                    {
                        AutoPlay = true,
                        Volume = 100,
                        LogLevel = MpvLogLevel.Debug
                    };
                }
                return _mpv;
            }
        }

        private void StartPlay(FileInfo file)
        {
            Task.Run(() =>
            {
                if (_eventLoop == null)
                {
                    _eventLoop = DispatcherQueueController.CreateOnDedicatedThread();
                    _eventQueue = _eventLoop.DispatcherQueue;
                }


                Mpv.API.RenderContextSetUpdateCallback(OnMpvRenderUpdate, IntPtr.Zero);

                Mpv.API.SetPropertyString("gpu-context", "d3d11");
                Mpv.API.SetPropertyString("hwdec", "d3d11va");
#if DEBUG
                Mpv.API.Command("script-binding", "stats/display-stats-toggle");
#endif
                Mpv.LoadAsync(file.FullName);
            });
        }

        private void OnMpvRenderUpdate(IntPtr cbCtx)
        {
            _eventQueue.TryEnqueue(() =>
            {
                var flags = Mpv.API.RenderContextUpdate();
                if (flags == MpvRenderUpdateFlag.MPV_RENDER_UPDATE_FRAME)
                {
                    RenderFrame();
                }
            });
        }

        private void Host_Loaded(object sender, RoutedEventArgs e)
        {
            isLoaded = true;
            ContentsScale = Host.CompositionScaleX;
            glcontext = new GlesContext();
            EnsureRenderSurface();
        }

        private void Host_CompositionScaleChanged(SwapChainPanel sender, object args)
        {
            if (lastCompositionScaleX == Host.CompositionScaleX && lastCompositionScaleY == Host.CompositionScaleY)
            {
                return;
            }

            lastCompositionScaleX = Host.CompositionScaleX;
            lastCompositionScaleY = Host.CompositionScaleY;

            pendingSizeChange = true;

            ContentsScale = Host.CompositionScaleX;

            DestroyRenderSurface();
            EnsureRenderSurface();
        }

        private void Host_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            pendingSizeChange = true;

            EnsureRenderSurface();
        }

        private void EnsureRenderSurface()
        {
            if (isLoaded && glcontext?.HasSurface != true && ActualWidth > 0 && ActualHeight > 0)
            {
                // detach and re-attach the size events as we need to go after the event added by ANGLE
                // otherwise our size will still be the old size

                Host.SizeChanged -= Host_SizeChanged;
                Host.CompositionScaleChanged -= Host_CompositionScaleChanged;

                glcontext.CreateSurface(Host, null, Host.CompositionScaleX, Hwnd);

                Host.SizeChanged += Host_SizeChanged;
                Host.CompositionScaleChanged += Host_CompositionScaleChanged;
            }
        }

        private void DestroyRenderSurface()
        {
            glcontext?.DestroySurface();
        }

        private void RenderFrame()
        {
            if (!isLoaded || !isVisible || glcontext?.HasSurface != true)
                return;

            glcontext.MakeCurrent();

            if (pendingSizeChange)
            {
                pendingSizeChange = false;
            }

            glcontext.GetSurfaceDimensions(out var panelWidth, out var panelHeight);
            glcontext.SetViewportSize(panelWidth, panelHeight);

            //MPV渲染

            if (!glcontext.SwapBuffers())
            {
                // The call to eglSwapBuffers might not be successful (i.e. due to Device Lost)
                // If the call fails, then we must reinitialize EGL and the GL resources.
            }
        }

        private void Host_Unloaded(object sender, RoutedEventArgs e)
        {
            Host.CompositionScaleChanged -= Host_CompositionScaleChanged;
            Host.SizeChanged -= Host_SizeChanged;

            DestroyRenderSurface();

            isLoaded = false;

            glcontext?.Dispose();
            glcontext = null;
        }
    }
}
