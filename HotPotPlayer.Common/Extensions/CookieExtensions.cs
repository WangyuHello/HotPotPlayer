using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Extensions
{
    public static class CookieExtensions
    {
        public static string ToNetScape(this Cookie cookie)
        {
            var secure = cookie.Secure ? "TRUE" : "FALSE";
            return $"{cookie.Domain}   TRUE  {cookie.Path}   {secure}  {((DateTimeOffset)cookie.Expires).ToUnixTimeSeconds()}  {cookie.Name}  {cookie.Value}";
        }
    }
}
