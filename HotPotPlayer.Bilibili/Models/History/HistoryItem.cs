using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Bilibili.Models.History
{
    public class HistoryItem
    {
        [JsonProperty("title")] public string? Title { get; set; }
        [JsonProperty("cover")] public string? Cover { get; set; }

        [JsonProperty("history")] public HistoryInfo? History { get; set; }
        [JsonProperty("author_name")] public string? AuthorName { get; set; }
        [JsonProperty("author_face")] public string? AuthorFace { get; set; }
        [JsonProperty("author_mid")] public long AuthorMid { get; set; }
        [JsonProperty("view_at")] public int ViewAt { get; set; }
        [JsonProperty("progress")] public int Progress { get; set; }
        [JsonProperty("duration")] public int Duration { get; set; }
    }

    public class HistoryInfo
    {
        [JsonProperty("oid")] public int Oid { get; set; }
        [JsonProperty("epid")] public int Epid { get; set; }
        [JsonProperty("bvid")] public string? Bvid { get; set; }
        [JsonProperty("cid")] public int Cid { get; set; }
    }
}
