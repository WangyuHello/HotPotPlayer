using HotPotPlayer.Models;
using HotPotPlayer.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;

namespace HotPotPlayer.Test
{
    public class VideoTest
    {
        [Fact]
        public void Test1()
        {
            var config = new MockConfig();
            config.ClearDb();
            var videoService = new LocalVideoService(config);
            videoService.OnVideoChanged += VideoService_OnVideoChanged;
            videoService.StartLoadLocalVideo();
        }

        [Fact]
        public void Test2()
        {
            var config = new MockConfig();
            var videoService = new LocalVideoService(config);
            videoService.OnVideoChanged += VideoService_OnVideoChanged;
            videoService.StartLoadLocalVideo();
        }

        private void VideoService_OnVideoChanged(List<SingleVideoItemsGroup> arg1, List<SeriesItem> arg2)
        {
            Debug.WriteLine("");
        }
    }
}