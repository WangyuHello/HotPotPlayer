using HotPotPlayer.Models;
using HotPotPlayer.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;

namespace HotPotPlayer.Test
{
    public class MusicTest
    {
        [Fact]
        public void Test1()
        {
            var config = new MockConfig();
            config.ClearDb();
            var musicService = new LocalMusicService(config);
            musicService.StartLoadLocalMusic();
        }



        [Fact]
        public void Test2()
        {
            var config = new MockConfig();
            var musicService = new LocalMusicService(config);
            musicService.StartLoadLocalMusic();
        }

        [Fact]
        public void Test3()
        {
            var config = new MockConfig2();
            config.ClearDb();
            var musicService = new LocalMusicService(config);
            musicService.StartLoadLocalMusic();
        }

        [Fact]
        public void Test4()
        {
            var config = new MockConfig2();
            var musicService = new LocalMusicService(config);
            musicService.StartLoadLocalMusic();
        }
    }
}