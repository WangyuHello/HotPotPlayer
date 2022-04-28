using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.DirectX;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Video
{
    public sealed partial class VideoHost : UserControl
    {
        public VideoHost()
        {
            this.InitializeComponent();
        }

        public FileInfo Source
        {
            get { return (FileInfo)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(FileInfo), typeof(VideoHost), new PropertyMetadata(default(FileInfo), SourceChanged));


        private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var h = (VideoHost)d;
            h.StartPlay((FileInfo)e.NewValue);
        }


        byte[] _buffer;
        int _bufferSize;
        int VideoWidth = 1920;
        int VideoHeight = 1080;
        DispatcherQueueController _eventLoop;
        DispatcherQueue _eventQueue;
        CanvasSwapChain _swapChain;
        CanvasDevice _device;
        double _scale;
        CanvasBitmap _bitmap;

        MpvPlayer _mpv;
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

        private unsafe void OnMpvRenderUpdate(IntPtr cbCtx)
        {
            _eventQueue.TryEnqueue(() =>
            {
                var flags = Mpv.API.RenderContextUpdate();
                if (flags == MpvRenderUpdateFlag.MPV_RENDER_UPDATE_FRAME)
                {
                    // 进行视频渲染 BGR0格式
                    fixed(byte* renderTargetPtr = _buffer)
                    {
                        Mpv.API.RenderContextRender(VideoWidth, VideoHeight, renderTargetPtr);
                    }
                    // 绘图
                    using (var ds = _swapChain.CreateDrawingSession(Colors.White))
                    {
                        _bitmap.SetPixelBytes(_buffer, 0, 0, VideoWidth, VideoHeight);
                        ds.DrawImage(_bitmap);
                    }
                    _swapChain.Present();
                }
            });
        }

        async void StartPlay(FileInfo file)
        {
            await Task.Run(() =>
            {
                if (_eventLoop == null)
                {
                    _eventLoop = DispatcherQueueController.CreateOnDedicatedThread();
                    _eventQueue = _eventLoop.DispatcherQueue;
                }

                _bufferSize = 4 * VideoWidth * VideoHeight;
                _buffer = new byte[_bufferSize];

                _bitmap = CanvasBitmap.CreateFromBytes(_device, _buffer, VideoWidth, VideoHeight, DirectXPixelFormat.B8G8R8A8UIntNormalized, (float)(96 * _scale), CanvasAlphaMode.Ignore);
                GC.TryStartNoGCRegion(_bufferSize * 2);

                Mpv.API.RenderContextSetUpdateCallback(OnMpvRenderUpdate, IntPtr.Zero);

                Mpv.API.SetPropertyString("gpu-context", "d3d11");
                Mpv.API.SetPropertyString("hwdec", "d3d11va");
#if DEBUG
                Mpv.API.Command("script-binding", "stats/display-stats-toggle");
#endif
                Mpv.LoadAsync(file.FullName);
            });
        }

        private void Host_Loaded(object sender, RoutedEventArgs e)
        {
            _scale = XamlRoot.RasterizationScale;
            _device = new CanvasDevice();
            _swapChain = new CanvasSwapChain(_device, (float)Host.ActualWidth, (float)Host.ActualHeight, (float)(96 * _scale), DirectXPixelFormat.B8G8R8A8UIntNormalized, 3, CanvasAlphaMode.Ignore);
            Host.SwapChain = _swapChain;
            VideoWidth = (int)(_scale * Host.ActualWidth);
            VideoHeight = (int)(_scale * Host.ActualHeight);
        }
    }
}
