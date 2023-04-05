using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Bilibili.Models.Search
{
    public class SearchResult
    {
        [JsonProperty("result_type")] public string? ResultType { get; set; }
        [JsonProperty("data")] public List<SearchResultData>? Data { get; set; }
    }
}
