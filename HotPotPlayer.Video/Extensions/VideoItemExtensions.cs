using HotPotPlayer.Models.BiliBili;
using System.Text;

namespace HotPotPlayer.Video.Extensions
{
    public static class VideoItemExtensions
    {
        public static string GetEdlProtocal(this BiliBiliVideoItem video, string vurl, string aurl)
        {
            var sb = new StringBuilder();
            sb.Append("edl://!new_stream;!no_clip;!no_chapters;");
            var url = vurl;
            var urlLen = url.Length;
            sb.Append($"%{urlLen}%{url}");
            sb.Append(";!new_stream;!no_clip;!no_chapters;");
            url = aurl;
            urlLen = url.Length;
            sb.Append($"%{urlLen}%{url}");
            //sb.Append(";");


            return sb.ToString();
        }
    }
}
