using HotPotPlayer.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Extensions
{
    public static class LyricExtensions
    {
        public static LyricItem[] Parse(this string raw)
        {
            var l = raw.Split('\n').Where(s => !string.IsNullOrEmpty(s)).Select(s => s.ParseOne()).ToArray();
            return l;
        }

        static LyricItem ParseOne(this string s)
        {
            //[00:00.000] 作词 : Eir/Takahiro Yasuda
            int state = 0;
            int ti1 = 0;
            int ti2 = 0;
            var ci = 0;
            for (int i = 0; i < s.Length; i++)
            {
                var c = s[i];
                switch (state)
                {
                    case 0:
                        if (c == '[')
                        {
                            ti1 = i + 1;
                            state = 1;
                        }
                        break;
                    case 1:
                        if (c == ']')
                        {
                            ti2 = i;
                            ci = i + 1;
                            state = 0;
                        }
                        break;
                    default:
                        break;
                }
            }
            var timeStr = s[ti1..ti2];
            var ts = timeStr.Split(':', '.');
            var m = int.Parse(ts[0]);
            var sc = int.Parse(ts[1]);
            var f = int.Parse(ts[2]);
            var content = s[ci..];
            return new LyricItem
            {
                Time = new TimeSpan(0,0,m,sc,f),
                Content = content,
            };
        }
    }
}
