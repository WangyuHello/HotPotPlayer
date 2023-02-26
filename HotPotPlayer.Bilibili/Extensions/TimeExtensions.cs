using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Bilibili.Extensions
{
    public static class TimeExtensions
    {
        public static string GetDuration(this int i)
        {
            var time = TimeSpan.FromSeconds(i);
            if (time.Hours > 0)
            {
                return time.ToString("hh\\:mm\\:ss");
            }
            else
            {
                return time.ToString("mm\\:ss");
            }
        }

        public static string GetDuration(this string dur)
        {
            var i = int.Parse(dur);
            return i.GetDuration();
        }

        public static string GetDateTime(this int i)
        {
            var ts = TimeSpan.FromSeconds(i);
            var time = new DateTime(ts.Ticks);
            time = time.AddYears(1969);
            var t2 = time.ToLocalTime();
            return $"{time.Year}-{time.Month}-{time.Day} {t2:HH\\:mm\\:ss}";
        }

        public static string GetDateTime(this string s)
        {
            var i = int.Parse(s);
            return i.GetDateTime();
        }
    }
}
