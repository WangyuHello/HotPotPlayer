using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Bilibili.Models.Nav
{
    public class NavStatData
    {
        [JsonProperty("following")] public int Following { get; set; }
        [JsonProperty("follower")] public int Follower { get; set; }
        [JsonProperty("dynamic_count")] public int DynamicCount { get; set; }
    }
}
