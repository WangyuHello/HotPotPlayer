using LibVLCSharp.Platforms.WindowsApp;
using LibVLCSharp.Shared;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Video
{
    public sealed partial class VideoControl2 : UserControl, INotifyPropertyChanged
    {
        public VideoControl2()
        {
            this.InitializeComponent();
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

        public FileInfo Source
        {
            get { return (FileInfo)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(FileInfo), typeof(VideoControl2), new PropertyMetadata(default(FileInfo), SourceChanged));

        private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var h = (VideoControl2)d;
            h.StartPlay((FileInfo)e.NewValue);
        }

        private void StartPlay(FileInfo file)
        {
            using var media = new LibVLCSharp.Shared.Media(LibVLC, file.FullName, FromType.FromPath);
            MediaPlayer.Play(media);
        }

        MediaPlayer _mediaPlayer;

        MediaPlayer MediaPlayer
        {
            get => _mediaPlayer;
            set => Set(ref _mediaPlayer, value);
        }
        private LibVLC LibVLC { get; set; }

        private void VideoView_Initialized(object sender, InitializedEventArgs e)
        {
            LibVLC = new LibVLC(enableDebugLogs: true, e.SwapChainOptions);
            MediaPlayer = new MediaPlayer(LibVLC);
        }
    }
}
