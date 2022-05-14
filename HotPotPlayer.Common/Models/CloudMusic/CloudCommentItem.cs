using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Models.CloudMusic
{
    public record CloudCommentItem
    {
        [JsonProperty("user")]
        public CloudUserItem User { get; set; }

        [JsonProperty("commentId")]
        public long CommentId { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("timeStr")]
        public string TimeStr { get; set; }

        [JsonProperty("likedCount")]
        public int LikedCount { get; set; }

        [JsonProperty("liked")]
        public bool Liked { get; set; }
    }
}
