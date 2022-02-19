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
            musicService.OnAlbumGroupChanged += MusicService_OnAlbumGroupChanged; ;
            musicService.StartLoadLocalMusic();
        }

        private void MusicService_OnAlbumGroupChanged(List<AlbumGroup> arg1, List<PlayListItem> arg2)
        {
            Console.WriteLine();
        }

        [Fact]
        public void Test2()
        {
            var config = new MockConfig();
            var musicService = new LocalMusicService(config);
            musicService.OnAlbumGroupChanged += MusicService_OnAlbumGroupChanged; ;
            musicService.StartLoadLocalMusic();
        }


    }
}