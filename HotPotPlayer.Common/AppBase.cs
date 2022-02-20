﻿using HotPotPlayer.Models;
using HotPotPlayer.Services;
using Microsoft.UI.Xaml;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer
{
    public abstract class AppBase: Application
    {
        ConfigBase config;
        public ConfigBase Config => config ??= new AppConfig();

        LocalMusicService localMusicService;
        public LocalMusicService LocalMusicService => localMusicService ??= new LocalMusicService(Config);

        NetEaseMusicService netEaseMusicService;
        public NetEaseMusicService NetEaseMusicService => netEaseMusicService ??= new NetEaseMusicService();

        LocalVideoService localVideoService;
        public LocalVideoService LocalVideoService => localVideoService ??= new LocalVideoService(Config);

        MusicPlayer musicPlayer;
        public MusicPlayer MusicPlayer => musicPlayer ??= new MusicPlayer(Config);
    }
}