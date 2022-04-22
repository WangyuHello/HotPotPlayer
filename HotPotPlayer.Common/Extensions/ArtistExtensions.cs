using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Extensions
{
    public static class ArtistExtensions
    {
        public static string[] GetArtists(this string s)
        {
            return s.Split(new char[] { ',', '×', '&', '、', '/', '・' }).Select(s => s.Trim()).ToArray();
        }
    }
}
