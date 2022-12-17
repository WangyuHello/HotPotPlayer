using HotPotPlayer.Services.BiliBili.Video;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Video.Extensions
{
    public static class DashVideoExtensions
    {
        public static string EscapeBaseUrl(this DashVideo video)
        {
            return video.BaseUrl.Replace("&", "&amp;");
        }
    }
}
