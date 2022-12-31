using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Services.BiliBili.User
{
    public class UserVideoInfo
    {
        [JsonProperty("page")] public UserVideoInfoPage Page { get; set; }
        [JsonProperty("list")] public UserVideoInfoList List { get; set; }
    }

    public class UserVideoInfoList
    {
        [JsonProperty("vlist")] public List<UserVideoInfoItem> VList { get; set; }
    }

    public class UserVideoInfoItem
    {
        [JsonProperty("aid")] public string Aid { get; set; }
        [JsonProperty("author")] public string Author { get; set; }
        [JsonProperty("bvid")] public string BVid { get; set; }
        [JsonProperty("comment")] public int Comment { get; set; }
        [JsonProperty("created")] public int Created { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("length")] public string Length { get; set; }
        [JsonProperty("pic")] public string Pic { get; set; }
        [JsonProperty("play")] public int Play { get; set; }
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("video_review")] public int VideoReview { get; set; }


        public string GetViews()
        {
            int v = Play;
            if (v >= 10000)
            {
                var v2 = (double)v / 10000;
                return $"{v2.ToString("F1")}万";
            }
            else
            {
                return v.ToString();
            }
        }

        public string GetDanMaku()
        {
            int v = VideoReview;
            if (v >= 10000)
            {
                var v2 = (double)v / 10000;
                return $"{v2.ToString("F1")}万";
            }
            else
            {
                return v.ToString();
            }
        }

        public string GetUpDateTime()
        {
            int i = Created;
            var ts = TimeSpan.FromSeconds(i);
            var time = new DateTime(ts.Ticks);
            return $"{time.Year + 1969}-{time.Month}-{time.Day} {ts:hh\\:mm\\:ss}";
        }
    }

    public class UserVideoInfoPage
    {
        [JsonProperty("count")] public int Count { get; set; }
        [JsonProperty("pn")] public int Pn { get; set; }
        [JsonProperty("ps")] public int Ps { get; set; }
    }
}
