using BiliBiliAPI.Models.Videos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Models.BiliBili
{
    public record BiliBiliVideoItem : VideoItem
    {
        public List<DashVideo> DashVideos { get; set; }

        public List<DashVideo> DashAudio { get; set; }

        public List<Durl> Urls { get; set; }
    }
}
