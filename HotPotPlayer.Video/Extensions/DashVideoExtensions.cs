using HotPotPlayer.Bilibili.Models.Video;

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
