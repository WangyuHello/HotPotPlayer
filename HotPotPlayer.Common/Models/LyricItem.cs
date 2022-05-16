using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Models
{
    public struct LyricItem
    {
        public TimeSpan Time { get; set; }
        public string Content { get; set; }
        public string Translate { get; set; }
    }
}
