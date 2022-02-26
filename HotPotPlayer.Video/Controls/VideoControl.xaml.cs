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

        WriteableBitmap _bitmap = new WriteableBitmap(1920, 1080);
        IntPtr? _renderTarget;
        int bufferSize = 4 * 1920 * 1080;

        unsafe void OnNewFrameDrawed()
        {
            if (videoRoot == null || _renderTarget == null)
            {
                return;
            }

            using var bstream = _bitmap.PixelBuffer.AsStream();
            bstream.Write(new Span<byte>(_renderTarget.Value.ToPointer(), bufferSize));
        }

        private static void OnVideoSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (VideoControl)d;

            c._renderTarget ??= Marshal.AllocHGlobal(c.bufferSize);
        }
    }
}
