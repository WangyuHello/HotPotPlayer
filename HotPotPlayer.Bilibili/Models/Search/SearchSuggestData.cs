using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Bilibili.Models.Search
{
    public class SearchSuggestData
    {
        [JsonProperty("result")]
        public Result Result { get; set; }

        [JsonProperty("stoken")]
        public string Stoken { get; set; }
    }

    public class Result
    {
        [JsonProperty("tag")]
        public List<ResultTags> Values { get; set; }  
    }

    public class ResultTags
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("spid")]
        public int SPID { get; set; }

        [JsonProperty("ref")]
        public string Ref { get; set; }
    }
}
