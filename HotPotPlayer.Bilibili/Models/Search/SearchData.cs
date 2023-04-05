using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Bilibili.Models.Search
{
    public class SearchData
    {
        [JsonProperty("seid")] public string? SEId { get; set; }

        [JsonProperty("result")] public SearchResult[]? Result { get; set; }
    }
}
