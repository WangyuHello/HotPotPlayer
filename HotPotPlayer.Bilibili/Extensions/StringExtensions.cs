using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Bilibili.Extensions
{
    public static class StringExtensions
    {
        public static List<string> SplitJson(this string j)
        {
            List<string> result = new List<string>();
            StringBuilder sb = new();
            int state = 0;
            for (int i = 0; i < j.Length; i++)
            {
                sb.Append(j[i]);
                if (j[i] == '{')
                {
                    state += 1;
                }
                else if (j[i] == '}')
                {
                    state -= 1;
                }

                if (state == 0)
                {
                    result.Add(sb.ToString());
                    sb.Clear();
                }
            }
            return result;
        }
    }
}
