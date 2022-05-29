using HotPotPlayer.Services;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Helpers
{
    public class HelperBase
    {
        protected static MusicPlayer MusicPlayer => ((AppBase)Application.Current).MusicPlayer;
        protected static LocalMusicService LocalMusicService => ((AppBase)Application.Current).LocalMusicService;
        protected static LocalVideoService LocalVideoService => ((AppBase)Application.Current).LocalVideoService;
        protected static NetEaseMusicService NetEaseMusicService => ((AppBase)Application.Current).NetEaseMusicService;
        protected static AppBase App => (AppBase)Application.Current;
        protected static XamlRoot XamlRoot => ((AppBase)Application.Current).XamlRoot;
    }
}
