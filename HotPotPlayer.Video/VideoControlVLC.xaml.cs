using LibVLCSharp.Platforms.UWP;
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

namespace HotPotPlayer.Controls
{
    public sealed partial class VideoControlVLC : UserControl, INotifyPropertyChanged
    {
        public VideoControlVLC()
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

        private MediaPlayer _mediaPlayer;
        private LibVLC LibVLC { get; set; }
        public MediaPlayer MediaPlayer
        {
            get => _mediaPlayer;
            private set => Set(ref _mediaPlayer, value);
        }


        private void VideoView_Initialized(object sender, InitializedEventArgs eventArgs)
        {
            Core.Initialize();
            LibVLC = new LibVLC(enableDebugLogs: true, eventArgs.SwapChainOptions);
            MediaPlayer = new MediaPlayer(LibVLC);
            using var media = new Media(LibVLC, @"D:\视频\【Animenz】Bios（10周年版）-_罪恶王冠_OST.459129031.mkv");
            MediaPlayer.Play(media);
        }
    }
}
