using HotPotPlayer.Services;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer
{
    public interface IComponentServiceLocator
    {
        ConfigBase Config => ((AppBase)Application.Current).Config;
        NetEaseMusicService CloudMusicService => ((AppBase)Application.Current).NetEaseMusicService;
        LocalMusicService LocalMusicService => ((AppBase)Application.Current).LocalMusicService;
        MusicPlayer MusicPlayer => ((AppBase)Application.Current).MusicPlayer;
    }
}
