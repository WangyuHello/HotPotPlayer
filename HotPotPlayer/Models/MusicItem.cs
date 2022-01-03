using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Models
{
    internal record MusicItem
    {
        public string Title { get; set; }
        public string[] Artist { get; set; }
        public string Album { get; set; }
        public TimeSpan Duration { get; set; }

        public Uri Source { get; set; }
        public FileInfo File { get; set; }
    }
}
