using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Video
{
    public sealed partial class VideoControl3 : UserControlBase
    {
        public VideoControl3()
        {
            this.InitializeComponent();
        }

        public FileInfo Source
        {
            get { return (FileInfo)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(FileInfo), typeof(VideoControl3), new PropertyMetadata(default(FileInfo), SourceChanged));

        private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var h = (VideoControl3)d;
            h.StartPlay((FileInfo)e.NewValue);
        }
        private async void StartPlay(FileInfo file)
        {
            var f = await StorageFile.GetFileFromPathAsync(file.FullName);
            mediaPlayerElement.Source = MediaSource.CreateFromStorageFile(f);
            mediaPlayerElement.MediaPlayer.Play();
        }

        public void Stop()
        {
            mediaPlayerElement.MediaPlayer.Pause();
        }
    }
}
