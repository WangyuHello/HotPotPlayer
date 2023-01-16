using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Extensions
{
    public static class IntExtensions
    {
        public static string ToHumanString(this int v)
        {
            if (v >= 10000)
            {
                var v2 = (double)v / 10000;
                return $"{v2:F1}万";
            }
            else
            {
                return v.ToString();
            }
        }
    }
}
