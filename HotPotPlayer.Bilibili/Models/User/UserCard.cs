using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Bilibili.Models.User
{
    public class UserCardBundle
    {

        [JsonProperty("card")] public Card Card { get; set; }
        [JsonProperty("space")] public Space Space { get; set; }
        [JsonProperty("following")] public bool Following { get; set; }
        [JsonProperty("follower")] public string Follower { get; set; }
        [JsonProperty("like_num")] public string LikeNum { get; set; }

        public string GetFriend => Card.Friend + " 关注";
        public string GetFans => Card.Fans + " 粉丝";
        public string GetLikeNum => LikeNum + " 获赞";

        public string GetSign => string.IsNullOrEmpty(Card.Sign) ? "这个人不神秘只是不知道该写什么" : Card.Sign;
    }

    public class Card
    {
        [JsonProperty("mid")] public string Mid { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("sex")] public string Sex { get; set; }
        [JsonProperty("face")] public string Face { get; set; }
        [JsonProperty("fans")] public int Fans { get; set; }
        [JsonProperty("friend")] public int Friend { get; set; }
        [JsonProperty("attention")] public int Attention { get; set; }
        [JsonProperty("sign")] public string Sign { get; set; }
        [JsonProperty("level_info")] public LevelInfo LevelInfo { get; set; }
    }

    public class LevelInfo
    {
        [JsonProperty("current_level")] public int CurrentLevel { get; set; }
    }

    public class Space
    {
        [JsonProperty("s_img")] public string SmallImage { get; set; }
        [JsonProperty("l_img")] public string LargeImage { get; set; }
    }
}
