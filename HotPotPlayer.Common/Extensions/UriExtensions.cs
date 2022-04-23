using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HotPotPlayer.Extensions
{
    public static class UriExtensions
    {
        public static string GetLocalPath(this Uri uri)
        {
            var origin = uri.OriginalString;
            var s = origin[8..]; //去掉file:///
            //s = Regex.Unescape(s);
            return s;
        }
    }
}
