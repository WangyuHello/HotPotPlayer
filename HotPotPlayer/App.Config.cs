using HotPotPlayer.Models;
using HotPotPlayer.Services;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer
{
    public partial class App : Application
    {
        internal LocalMusicService LocalMusicService = new();

        internal List<string> MusicLibrary { get; set; } = new List<string> { @"D:\Music" };

        internal List<MusicItem> LocalMusics { get; set; }
    }
}
