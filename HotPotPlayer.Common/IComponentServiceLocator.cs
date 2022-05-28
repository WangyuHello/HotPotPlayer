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
        public ConfigBase Config { get; }
        public NetEaseMusicService NetEaseMusicService { get; }
        public LocalMusicService LocalMusicService { get; }
        public MusicPlayer MusicPlayer { get; }
    }
}
