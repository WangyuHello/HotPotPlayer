using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Services.BiliBili.Video
{
    public class Videos
    {
        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("list")]
        public List<TopVideo> List { get; set; }
    }

    public class PopularVideos
    {
        [JsonProperty("no_more")]
        public bool NoMore { get; set; }

        [JsonProperty("list")]
        public List<VideoContent> List { get; set; }
    }

    public class TopVideo: VideoContent
    {
        [JsonProperty("short_link")]
        public string Link { get; set; }

        [JsonProperty("pic")]
        public string Cover { get; set; }

        [JsonProperty("short_link_v2")]
        public string Link_V2 { get; set; }

        [JsonProperty("first_frame")]
        public string First_Frame { get; set; }

        [JsonProperty("pub_location")]
        public string IP_City { get; set;}

        [JsonProperty("score")]
        public string Score { get; set; }


        [JsonProperty("pts")]
        public string Pts { get; set; }

        [JsonProperty("redirect_url")]
        public string RedirectUrl { get; set; }

    }
}
