using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Services.BiliBili.Dynamic
{
    public class DynamicUp_UpDateList
    {
        [JsonProperty("live_users")]
        public Dynamic_Live_Users LiveInfo { get; set; }

        [JsonProperty("my_info")]
        public Dynamic_MyInfo MyInfo { get; set; }

        [JsonProperty("up_list")]
        public List<Dynamic_Live_Items> UpList { get; set; }
    }

    public class UpListItem
    {
        [JsonProperty("face")] public string Cover { get; set; }

        [JsonProperty("is_reserve_recall")] public bool IsReserve { get; set; }

        [JsonProperty("has_update")]public bool IsUpDate { get; set; }

        [JsonProperty("mid")] public string Mid { get; set; }

        [JsonProperty("title")] public string Title { get; set; }

        [JsonProperty("uname")] public string UpName { get; set; }
    }


    public class Dynamic_MyInfo
    {

        [JsonProperty("dync")]
        public string Dync { get; set; }

        [JsonProperty("face")]
        public string Cover { get; set; }

        [JsonProperty("face_nft")] public int Face_Nft { get; set; }

        [JsonProperty("follower")] public int Follower { get; set; }

        [JsonProperty("following")] public int Following { get; set; }

        [JsonProperty("mid")]public int Mid { get; set; }

        [JsonProperty("name")]public string Name { get; set; }

        [JsonProperty("space_bg")]public string Back_Image { get; set; }

    }

    public class Dynamic_Live_Users
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("group")]
        public string Group { get; set; }

        [JsonProperty("items")]
        public List<Dynamic_Live_Items> Items { get; set; }
    }

    public class Dynamic_Live_Items: UpListItem
    {

        [JsonProperty("jump_url")] public string Url { get; set; }
    }
}
