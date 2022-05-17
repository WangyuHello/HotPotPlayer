using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Models.CloudMusic
{
    public record CloudUserItem
    {
        [JsonProperty("nickname")]
        public string NickName { get; set; }

        [JsonProperty("userId")]
        public long UserId { get; set; }

        [JsonProperty("avatarUrl")]
        public string AvatarUrl { get; set; }

        [JsonProperty("backgroundUrl")]
        public string BackgroundUrl { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("province")]
        public long Province { get; set; }

        [JsonProperty("city")]
        public long City { get; set; }

        [JsonProperty("followeds")]
        public int Followeds { get; set; }

        [JsonProperty("follows")]
        public int Follows { get; set; }
    }

    public record LevelItem 
    {
        [JsonProperty("progress")]
        public double Progress { get; set; }

        [JsonProperty("nextPlayCount")]
        public int NextPlayCount { get; set; }

        [JsonProperty("nextLoginCount")]
        public int NextLoginCount { get; set; }

        [JsonProperty("nowPlayCount")]
        public int NowPlayCount { get; set; }

        [JsonProperty("nowLoginCount")]
        public int NowLoginCount { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }
    }

}
