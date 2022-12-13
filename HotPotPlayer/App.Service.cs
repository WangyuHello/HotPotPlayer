using HotPotPlayer.Models;
using HotPotPlayer.Pages;
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
using WinUIEx;

namespace HotPotPlayer
{
    public partial class App : AppBase
    {
        public FileInfo InitMediaFile { get; set; }

        public override void PlayVideo(VideoItem file)
        {
            PlayVideos(new List<VideoItem>() { file },  0);
        }

        public override void PlayVideos(IEnumerable<VideoItem> videos, int index)
        {
            MainWindow.NavigateToVideo(new VideoPlayInfo { VideoItems = videos, Index = index });
        }

        public override void ShowToast(ToastInfo toast)
        {
            MainWindow.ShowToast(toast);
        }
    }
}
