using HotPotPlayer.Models;
using HotPotPlayer.Services;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer
{
    public partial class App : Application
    {
        internal Lazy<LocalMusicService> LocalMusicService = new();
        internal Lazy<MusicPlayer> MusicPlayer = new();
    }
}
