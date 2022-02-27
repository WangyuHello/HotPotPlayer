using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Mpv.NET.Player;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Video.Controls
{
    public sealed partial class VideoControl : UserControl, INotifyPropertyChanged
    {
        readonly WriteableBitmap _bitmap;
        IntPtr _renderTarget;
        int bufferSize = 4 * 1920 * 1080;

        bool mpvInited;
        MpvPlayer _mpv;
        MpvPlayer Mpv
        {
            get
            {
                if (_mpv == null)
                {
                    _renderTarget = Marshal.AllocHGlobal(bufferSize);
                    _mpv = new MpvPlayer(@"NativeLibs\mpv-2.dll", _renderTarget, OnNewFrameDrawed);
                    mpvInited = true;
                }
                return _mpv;
            }
        }

        public VideoControl()
        {
            this.InitializeComponent();
            _bitmap = new WriteableBitmap(1920, 1080);
            videoRoot.Source = _bitmap;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Set<T>(ref T oldValue, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
            {
                oldValue = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public FileInfo VideoSource
        {
            get { return (FileInfo)GetValue(VideoSourceProperty); }
            set { SetValue(VideoSourceProperty, value); }
        }

        public static readonly DependencyProperty VideoSourceProperty =
            DependencyProperty.Register("VideoSource", typeof(FileInfo), typeof(VideoControl), new PropertyMetadata(default(FileInfo), OnVideoSourceChanged));


        unsafe void OnNewFrameDrawed(IntPtr context)
        {
            if (videoRoot == null || !mpvInited)
            {
                return;
            }
            DispatcherQueue.TryEnqueue(() =>
            {
                using var bstream = _bitmap.PixelBuffer.AsStream();
                bstream.Write(new Span<byte>(_renderTarget.ToPointer(), bufferSize));
                Mpv.API.RenderReportSwap();
            });
        }

        private static void OnVideoSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (VideoControl)d;
            var file = (FileInfo)e.NewValue;

            //c.Mpv.API.SetPropertyString("vo", "gpu");
            c.Mpv.API.SetPropertyString("gpu-context", "d3d11");
            c.Mpv.API.SetPropertyString("hwdec", "d3d11va");
#if DEBUG
            c.Mpv.API.Command("script-binding", "stats/display-stats-toggle");
#endif
            c.Mpv.Load(file.FullName);
            c.Mpv.API.StartRender();
        }
    }
}
