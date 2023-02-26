using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Bilibili.Models
{
    public class BiliResult<T>
    {
        [JsonProperty("code")]
        public string? Code { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonProperty("ttl")]
        public string? TTl { get; set; }

        [JsonProperty("data")]
        public T? Data { get; set; }
    }

    public class BiliResult
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonProperty("ttl")]
        public string? TTl { get; set; }
    }


    public enum VideoIDType
    {
        AV, BV
    }
}
