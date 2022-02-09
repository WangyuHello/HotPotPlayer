using HotPotPlayer.Models;
using HotPotPlayer.Services;
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
            var videoService = new LocalVideoService(config);
            videoService.OnVideoChanged += VideoService_OnVideoChanged;
            videoService.StartLoadLocalVideo();
        }

        private void VideoService_OnVideoChanged(List<VideoItem> v)
        {
            foreach (var item in v)
            {
                Debug.WriteLine(item);
            }
        }
    }
}