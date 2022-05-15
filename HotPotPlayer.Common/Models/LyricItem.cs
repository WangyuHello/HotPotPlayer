using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Models
{
    readonly public struct LyricItem
    {
        public TimeSpan Time { get; init; }
        public string Content { get; init; }
    }
}
