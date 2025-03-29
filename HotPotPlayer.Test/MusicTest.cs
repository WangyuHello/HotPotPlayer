using HotPotPlayer.Models;
using HotPotPlayer.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
            var musicService = new JellyfinMusicService(config);
            musicService.LoadJellyfinMusicAsync();
        }

        [Fact]
        public void Test2()
        {
            var config = new MockConfig();
            var musicService = new JellyfinMusicService(config);
            musicService.LoadJellyfinMusicAsync();
        }

        [Fact]
        public void Test3()
        {
            var config = new MockConfig2();
            config.ClearDb();
            var musicService = new JellyfinMusicService(config);
            musicService.LoadJellyfinMusicAsync();
        }

        [Fact]
        public void Test4()
        {
            var config = new MockConfig2();
            var musicService = new JellyfinMusicService(config);
            musicService.LoadJellyfinMusicAsync();
        }

        MockConfig2 config;

        [Fact]
        public void TestPlayList()
        {
            config = new MockConfig2();
            config.ClearDb();
            var musicService = new JellyfinMusicService(config);
            musicService.PropertyChanged += MusicService_PropertyChanged;
            musicService.LoadJellyfinMusicAsync();
            Console.Read();
        }

        private void MusicService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var service = (JellyfinMusicService)sender;
            if (e.PropertyName == "State" && service.State == LocalServiceState.Complete)
            {
                var m1 = service.LocalAlbumGroup[0][3].MusicItems[0];
                var m2 = service.LocalAlbumGroup[0][4].MusicItems[1];

                PlayListItem pl = new PlayListItem("Test", new DirectoryInfo(config.MusicPlayListDirectory[0].Path));
                pl.AddMusic(m1);
                pl.AddMusic(m2);
                pl.Write();
            }
        }
    }
}