using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Services.BiliBili.HomeVideo
{
    public class Video
    {
        [JsonProperty("items")]
        public List<Item> Item { get; set; }
       
    }

    public class HotVideo
    {

        [JsonProperty("Config")]
        public Config Config { get; set; }

        [JsonProperty("data")]
        public List<Item> Item { get; set; }
    }

    public class Config
    {

        [JsonProperty("item_title")]
        public string Title { get; set; }

        [JsonProperty("head_image")]
        public string Icon { get; set; }

        [JsonProperty("top_items")]
        public List<Top_Item> Top_Item { get; set; }

        [JsonProperty("share_info")]
        public Share_Info Share_Info { get; set; }
    }

    public class Share_Info
    {
        [JsonProperty("current_title")]
        public string Title { get; set; }

        [JsonProperty("share_title")]
        public string Share_Title { get; set; }

        [JsonProperty("share_sub_title")]
        public string Sub_Title { get; set; }

        [JsonProperty("share_icon")]
        public string Icon { get; set; }

        [JsonProperty("current_top_photo")]
        public string Back_Image { get; set; }
    }

    public class Top_Item
    {
        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("module_id")]
        public string Rank { get; set; }

        [JsonProperty("uri")]
        public string StartUri { get; set; }

        [JsonProperty("entrance_id")]
        public string Entrance_id { get; set; }
    }

    public class Item
    {
        [JsonProperty("card_type")]
        public string Card_Type { get; set; }

        [JsonProperty("card_goto")]
        public string Card_Goto { get; set; }

        [JsonProperty("goto")]
        public string Goto { get; set; }

        [JsonProperty("param")]
        public string Param { get; set; }

        [JsonProperty("cover")]
        public string Cover { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("three_point")]
        public Three_Point Three_Point { get; set; }

        [JsonProperty("args")]
        public UpArgs UpArg { get; set; }

        [JsonProperty("player_args")]
        public PlayerArgs PlayArg { get; set; }

        [JsonProperty("idx")]
        public string idx { get; set; }

        /// <summary>
        /// 播放数量（字符串）
        /// </summary>
        [JsonProperty("cover_left_text_2")]
        public string PlayerCount { get; set; }

        /// <summary>
        /// 历史弹幕数量
        /// </summary>
        [JsonProperty("cover_left_text_3")]
        public string LeftText { get; set; }

        [JsonProperty("desc")]
        public string Desc { get; set; }

        [JsonProperty("cover_left_text_1")]
        public string DurationText { get; set; }

        [JsonProperty("three_point_v2")]
        public List<Three_Point_V2> Three_Point_V2 { get; set; }


        [JsonProperty("mask")]
        public Mask Mask { get; set; }
    }

    public class Three_Point_V2
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("reasons")]
        public List<Dis_resource> Reasons { get; set; }

    }

    public class Mask
    {
        [JsonProperty("avatar")]
        public UP Avatar { get; set; }

        [JsonProperty("button")]
        public MaskButton MaskButton{get;set;}
    }

    public class UP
    {
        [JsonProperty("cover")]
        public string cover { get; set; }

        [JsonProperty("text")]
        public string Name { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("event")]
        public string Event{ get;set; }

        [JsonProperty("event_V2")]
        public string Event_V2 { get; set; }

        [JsonProperty("up_id")]
        public string up_id { get; set; }
    }

    public class MaskButton
    {
        [JsonProperty("text")]
        public string LikeState { get; set; }

        [JsonProperty("param")]
        public string commandparam { get; set; }

        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("event_V2")]
        public string Event_V2 { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class PlayerArgs
    {
        [JsonProperty("aid")]
        public string Aid { get; set; }

        [JsonProperty("cid")]
        public string cid { get; set; }

        [JsonProperty("type")]
        public string VideoType { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        
    }

    public class UpArgs
    {
        [JsonProperty("up_id")]
        public string Mid { get; set; }

        [JsonProperty("up_name")]
        public string Name { get; set; }

        [JsonProperty("rname")]
        public string RName { get; set; }

        [JsonProperty("rid")]
        public string Rid { get; set; }

        
    }

    public class Three_Point
    {
        [JsonProperty("dislike_reasons")]
        public List<Dis_resource> dis_Resources { get; set; }

        [JsonProperty("feedbacks")]
        public List<Dis_resource> FeedBacks { get; set; }

        [JsonProperty("watch_later")]
        public string Watch_Later { get; set; }
    }

    public class Dis_resource
    {
        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("toast")]
        public string Toast { get; set; }
    }
}
