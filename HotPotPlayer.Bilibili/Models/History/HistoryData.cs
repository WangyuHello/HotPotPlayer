using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Bilibili.Models.History
{
    public class HistoryData
    {
        [JsonProperty("cursor")] public HistoryCursor? Cursor { get; set; }

        [JsonProperty("list")] public List<HistoryItem>? List { get; set; }
    }

    public class HistoryCursor
    {
        [JsonProperty("max")] public int Max { get; set; }
        [JsonProperty("view_at")] public int ViewAt { get; set; }
        [JsonProperty("business")] public string? Business { get; set; }
        [JsonProperty("ps")] public int Ps { get; set; }
    }
}
