using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Models.CloudMusic
{
    public record Toplist
    {
        [JsonProperty("updateFrequency")]
        public string UpdateFrequency { get; set; }

        [JsonProperty("coverImgUrl")]
        public string CoverImgUrl { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("tracks")]
        public List<TopListTrack> Tracks { get; set; }
    }

    public record TopListTrack
    {
        [JsonProperty("first")]
        public string First { get; set; }

        [JsonProperty("second")]
        public string Second { get; set; }

        public string GetSecond()
        {
            return "- " + Second;
        }
    }
}
