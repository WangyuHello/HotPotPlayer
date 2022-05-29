using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Models.CloudMusic
{
    public record CloudSearchHotItem
    {
        [JsonProperty("searchWord")]
        public string SearchWord { get; set; }

        [JsonProperty("score")]
        public long Score { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
