using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Bilibili.Models.Video
{
    public class VideoHonorReply
    {
        [JsonProperty("honor")] public List<VideoHonor>? Honor { get; set; }
    }

    public class VideoHonor
    {
        [JsonProperty("aid")] public string? Aid { get; set; }
        [JsonProperty("type")] public int Type { get; set; }
        [JsonProperty("desc")] public string? Desc { get; set; }
        [JsonProperty("weekly_recommend_num")] public int WeeklyRecommendNum { get; set; }
    }
}
