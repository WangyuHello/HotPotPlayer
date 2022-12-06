using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Video
{
    public record VideoPlayInfo
    {
        public IEnumerable<FileInfo> VideoFiles { get; set; }
        public int Index { get; set; }
    }
}
