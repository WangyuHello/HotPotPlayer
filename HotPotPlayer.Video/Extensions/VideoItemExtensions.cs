using HotPotPlayer.Models.BiliBili;
using HotPotPlayer.Video.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Video.Extensions
{
    public static class VideoItemExtensions
    {
        public static string WriteToMPD(this BiliBiliVideoItem video, ConfigBase config)
        {
            var file = Path.Combine(config.CacheFolder, "video.mpd");
            var dash = new Dash(video);
            dash.WriteToFile(file);
            return file;
        }

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
