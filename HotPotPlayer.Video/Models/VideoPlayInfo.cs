using HotPotPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Video.Models
{
    public record VideoPlayInfo
    {
        public IEnumerable<VideoItem> VideoItems { get; set; }
        public int Index { get; set; }
        public bool ImmediateLoad { get; set; }
    }
}
