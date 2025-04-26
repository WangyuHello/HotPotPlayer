using HotPotPlayer.Models;
using Jellyfin.Sdk.Generated.Models;
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
        public BaseItemDto SingleOrSeries { get; set; }
        public List<BaseItemDto> List { get; set; }
        public List<FileInfo> Files { get; set; }
        public int Index { get; set; }
        public bool ImmediateLoad { get; set; }
    }
}
