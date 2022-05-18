using HotPotPlayer.Models;
using HotPotPlayer.Services;
using HotPotPlayer.Video;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer
{
    public partial class App : AppBase
    {
        public FileInfo InitMediaFile { get; set; }

        public void PlayVideo(FileInfo video)
        {
            VideoWindow v = new VideoWindow()
            {
                MediaFile = video
            };
            v.Activate();
            v.Closed += VideoWindowClosed;
        }

        private void VideoWindowClosed(object sender, WindowEventArgs args)
        {
            MainWindow.Activate();
        }

        public override void ShowToast(ToastInfo toast)
        {
            MainWindow.ShowToast(toast);
        }
    }
}
