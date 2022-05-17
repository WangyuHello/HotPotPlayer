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
    }
}
