using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Bilibili.Models.Dynamic
{
    public class EntranceData
    {
        [JsonProperty("entrance")] public Entrance? Entrance { get; set; }
        [JsonProperty("update_info")] public UpdateInfo? UpdateInfo { get; set; }
    }

    public class Entrance
    {
        [JsonProperty("icon")] public string? Icon { get; set; }
        [JsonProperty("mid")] public string? Mid { get; set; }
        [JsonProperty("type")] public string? Type { get; set; }
    }

    public class UpdateInfo
    {
        [JsonProperty("item")] public UpdateInfoItem? Item { get; set; }
        [JsonProperty("type")] public string? Type { get; set; }
    }

    public class UpdateInfoItem
    {
        [JsonProperty("count")] public int Count { get; set; }
    }
}
