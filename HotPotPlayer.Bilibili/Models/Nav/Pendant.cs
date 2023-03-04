using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Bilibili.Models.Nav
{
    public class Pendant
    {
        [JsonProperty("pid")] public int Pid { get; set; }
        [JsonProperty("name")] public string? Name { get; set; }
        [JsonProperty("image")] public string? Image { get; set; }
        [JsonProperty("expire")] public int Expire { get; set; }
    }
}
