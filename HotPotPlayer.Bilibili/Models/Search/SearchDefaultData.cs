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
        [JsonProperty("seid")]
        public string Seid { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string TypeString { get; set; }

        [JsonProperty("show_name")]
        public string Title { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("goto_type")]
        public string Description { get; set; }

        [JsonProperty("goto_value")]
        public string AVUrl { get; set; }
    }
}
