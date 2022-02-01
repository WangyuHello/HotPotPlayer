using HotPotPlayer.Models;
using HotPotPlayer.Services;
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
    public partial class App : Application
    {
        internal Lazy<LocalMusicService> LocalMusicService = new();
        internal Lazy<NetEaseMusicService> NetEaseMusicService = new();
        internal Lazy<LocalVideoService> LocalVideoService = new();
        internal Lazy<MusicPlayer> MusicPlayer = new();
        public FileInfo InitMediaFile { get; set; }

        public void PlayVideo(FileInfo video)
        {
            var path = Convert.ToBase64String(Encoding.UTF8.GetBytes(video.FullName));
            var info = new ProcessStartInfo("HotPotPlayer.VideoHost.exe", path)
            {
                WindowStyle = ProcessWindowStyle.Normal,
                UseShellExecute = false
            };
            Process.Start(info);
            //VideoHost.Program.Start(video);
        }
    }
}
