using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Bilibili.Models.Search
{
    public class SearchDefaultData
    {
        [JsonProperty("seid")] public string? Seid { get; set; }
        [JsonProperty("id")] public string? Id { get; set; }
        [JsonProperty("type")] public int Type { get; set; }
        [JsonProperty("show_name")] public string? ShowName { get; set; }
        [JsonProperty("goto_type")] public int GotoType { get; set; }
        [JsonProperty("goto_value")] public string? GotoValue { get; set; }
        [JsonProperty("url")] public string? Url { get; set; }
    }
}
