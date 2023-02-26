using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Bilibili.Models.Search
{
    public class SearchHotRankData
    {
        [JsonProperty("trending")]
        public Trednding Trending { get; set; }
    }

    public class Trednding
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("trackid")]
        public string TrackId { get; set; }

        [JsonProperty("list")]
        public List<SearchHotRankDataItem> List { get; set; }

        [JsonProperty("top_list")]
        public object TopList { get; set; }
    }

    public class SearchHotRankDataItem
    {
        [JsonProperty("keyword")]
        public string KeyWord { get; set; }

        [JsonProperty("show_name")]
        public string ShowName { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("goto")]
        public string GoTO { get; set; }
    }
}
