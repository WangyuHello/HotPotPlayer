using HotPotPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Extensions
{
    internal static class CoverExtensions
    {
        public static void SetPlayListCover(this PlayListItem i)
        {
            i.Cover = i.MusicItems.First().Cover;
        }
    }
}
