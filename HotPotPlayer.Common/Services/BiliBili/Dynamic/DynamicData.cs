
using BiliBiliAPI.Models.Account;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Services.BiliBili.Dynamic
{
    public enum DynamicType
    {
        Video, All, AnimationPGC, Read
    }

    public class DynamicData
    {
        [JsonProperty("has_more")]public bool HasMore { get; set; }
        [JsonProperty("offset")]public string OffSet { get; set; }

        [JsonProperty("update_num")]public int UpDateNum { get; set; }

        [JsonProperty("items")]public List<DynamicItem> DynamicItems { get; set; }
    }

    public class DynamicItem
    {
        [JsonProperty("modules")] public DynamicItemModule Modules { get; set; }
        [JsonProperty("basic")] public DynamicItemBasic Basic { get; set; }
        [JsonProperty("id_str")] public string ID { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("visible")] public bool Visible { get; set; }
        [JsonProperty("orig")] public DynamicItem Origin { get; set; }

        public bool HasOrigin => Origin != null;
    }

    public class DynamicItemModule
    {
        [JsonProperty("module_stat")]public ModuleStat ModuleState { get; set; }

        [JsonProperty("module_author")]public ModuleAuthor ModuleAuthor { get; set; }

        [JsonProperty("module_dynamic")]public ModuleDynamic ModuleDynamic { get; set; }
    }

    public class ModuleDynamic
    {
        [JsonProperty("additional")]public Additional Additional { get; set; }
        [JsonProperty("topic")]public object Topic { get; set; }

        [JsonProperty("desc")]public ModuleDesc Desc { get; set; }

        [JsonProperty("major")]public ModuleMajor Major { get; set; }

        public bool HasDesc => Desc != null && !string.IsNullOrEmpty(Desc.Text);

        public bool HasArchive => Major != null && Major.Archive != null;

    }

    public class ModuleMajor
    {
        [JsonProperty("type")]public string Type { get; set; }

        [JsonProperty("draw")]public MajorDraw Draw { get; set; }

        [JsonProperty("archive")]public MajorAcrchive Archive { get; set; }

        [JsonProperty("pgc")]public MajorPgc PGC { get; set; }

        [JsonProperty("ugc_season")]public UGCSeason UGC_Season { get; set; }

        public bool IsSingleDraw => Draw != null && Draw.Items.Count == 1;

        public bool IsMultiDraw => Draw != null && Draw.Items.Count > 1;

    }

    public class UGCSeason
    {
        [JsonProperty("badge")]public PgcBadge UgcBadge { get; set; }

        [JsonProperty("cover")]public string Cover { get; set; }

        [JsonProperty("desc")]public string Desc { get; set; }
        [JsonProperty("duration_text")]public string DurationText { get; set; }

        [JsonProperty("jump_url")]public string Jump_Url { get; set; }

        [JsonProperty("title")]public string Title { get; set; }

        [JsonProperty("stat")]public AV_Stat Stat { get; set; }
    }

    public class MajorPgc
    {
        [JsonProperty("badge")]public PgcBadge Pgc_Badge { get; set; }

        [JsonProperty("cover")]public string Cover { get; set; }

        [JsonProperty("epid")]public string Epid { get; set; }

        [JsonProperty("jump_url")]public string Jump_Url { get; set; }

        [JsonProperty("season_id")]public string Season_id { get; set; }

        [JsonProperty("sub_type")]public string Sub_Type { get; set; }

        [JsonProperty("title")]public string Title { get; set; }

        [JsonProperty("stat")]public AV_Stat Stat { get; set; }

        [JsonProperty("type")]public int Type { get; set; }
    }


    public class PgcBadge
    {
        public string bg_color { get; set; }

        public string color { get; set; }
        public string text { get; set; }
    }

    public class Live_Info
    {
        [JsonProperty("cover")]public string Cover { get; set; }
        [JsonProperty("parent_area_id")]public int Parent_Area_ID { get; set; }
        [JsonProperty("online")]public int OnLine { get; set; }

        [JsonProperty("live_status")]public int Live_Status { get; set; }

        [JsonProperty("room_paid_type")]public int Room_PaidType { get; set; }

        [JsonProperty("area_id")]public int Area_id { get; set; }

        [JsonProperty("uid")]public string Uid { get; set; }

        [JsonProperty("room_type")]public int Room_Type { get; set; }

        [JsonProperty("parent_area_name")]public string AreaName { get; set; }

        [JsonProperty("live_screen_type")]public int LiveScreenType { get; set; }

        [JsonProperty("link")]public string Link { get; set; }
        [JsonProperty("room_id")]public string Room_id { get; set; }

        [JsonProperty("title")]public string Title { get; set; }

        [JsonProperty("area_name")]public string Area_Name { get; set; }

        [JsonProperty("live_start_time")]public string LiveStartTime { get; set; }

        [JsonProperty("live_id")]public string LiveID { get; set; }

        [JsonProperty("watched_show")]public Watch_Show Watch_Show { get; set; }

        [JsonProperty("play_type")]public string PlayType { get; set; }

    }

    public class Watch_Show
    {
        [JsonProperty("switch")]public bool Switch { get; set; }
        [JsonProperty("num")]public int Num { get; set; }
        [JsonProperty("text_small")]public string ViewCount { get; set; }
        [JsonProperty("text_large")]public string ViewCountTwo { get; set; }

        [JsonProperty("icon")]public string Icon { get; set; }

        [JsonProperty("icon_location")]public string IconLocation { get; set; }

        [JsonProperty("icon_web")]public string Icon_Web { get; set; }
    }

    public class MajorAcrchive
    {
        [JsonProperty("aid")]public string Aid { get; set; }

        [JsonProperty("bvid")]public string Bvid { get; set; }

        [JsonProperty("cover")]public string Cover { get; set; }

        [JsonProperty("desc")]public string Desc { get; set; }

        [JsonProperty("disable_preview")]public int DisablePreview { get; set; }

        [JsonProperty("duration_text")]public string DurationText { get; set; }

        [JsonProperty("jump_url")]public string JumpUrl { get; set; }

        [JsonProperty("title")]public string Title { get; set; }

        [JsonProperty("type")]public int Type { get; set; }

        [JsonProperty("stat")]public AV_Stat State { get; set; }

    }

    public class AV_Stat
    {
        [JsonProperty("danmaku")]public string DanmakuCount { get; set; }
        [JsonProperty("play")]public string ViewCount { get; set; }

    }

    public class MajorDraw
    {
        [JsonProperty("id")]public string ID { get; set; }

        [JsonProperty("items")]public List<DrawItem> Items { get; set; }
    }


    public class DrawItem
    {
        [JsonProperty("height")]public int Height { get; set; }
        [JsonProperty("size")]public double Size { get; set; }
        [JsonProperty("src")]public string Source { get; set; }
        [JsonProperty("width")]public int Width { get; set; }

        [JsonProperty("tags")]public List<object> Tags { get; set; }
    }

    public class ModuleDesc
    {
        [JsonProperty("rich_text_nodes")]public List<DescNodes> Text_Nodes { get; set; } 
        [JsonProperty("text")]public string Text { get; set; }
    }

    public class DescNodes
    {
        [JsonProperty("emoji")] public Emoji Emoji { get; set; }
        [JsonProperty("orig_text")] public string OrigeText { get; set; }
        [JsonProperty("rid")] public string Rid { get; set; }
        [JsonProperty("text")] public string Text { get; set; }

        [JsonProperty("type")]public string Type { get; set; }
    }

    public class Emoji
    {
        [JsonProperty("icon_url")]public string Cover { get; set; }

        [JsonProperty("size")]public string Size { get; set; }

        [JsonProperty("text")]public string Text { get; set; }

        [JsonProperty("type")]public string Type { get; set; }
    }

    public class Additional
    {
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("reserve")] public Reserve Reserve { get; set; }

    }

    public class Reserve
    {
        [JsonProperty("jump_url")]public string Jump_Url { get; set; }

        [JsonProperty("reserve_total")]public int Total { get; set; }

        [JsonProperty("rid")]public long Rid { get; set; }

        [JsonProperty("state")]public int State { get; set; }

        [JsonProperty("stype")]public int Stype { get; set; }

        [JsonProperty("title")]public string title { get; set; }

        [JsonProperty("up_mid")]public string Up_Mid { get; set; }

        [JsonProperty("desc1")]public Reserve_Desc Desc1 { get; set; }
        [JsonProperty("desc2")]public Reserve_Desc Desc2 { get; set; }
    }

    public class Reserve_Desc
    {
        [JsonProperty("style")]public int Style { get; set; }
        [JsonProperty("text")]public string Text { get; set; }
    }

    public class ModuleAuthor
    {
        [JsonProperty("face")]public string Face { get;set; }
        [JsonProperty("face_nft")]public bool Face_NFT { get; set; }

        [JsonIgnore()]
        [JsonProperty("following")]
        public bool Following { get; set; }

        [JsonProperty("jump_url")]public string JumpUrl { get; set; }

        [JsonProperty("lable")]public string Lable { get; set; }

        [JsonProperty("mid")]public string Mid { get; set; }

        [JsonProperty("name")]public string Name { get; set; }

        [JsonProperty("pub_action")]public string Dynamic_Message { get; set; }

        [JsonProperty("pub_location_text")]public string Pub_Location_Text { get; set; }
        [JsonProperty("pub_time")]public string PubTime { get; set; }

        [JsonProperty("pub_ts")]public string Dynamic_DateTime { get;set; }

        [JsonProperty("type")]public string UpType { get; set; }


        [JsonProperty("official_verify")]public Official_verify Official_Verify { get; set; }

        [JsonProperty("pendant")]public Pendant Pendant { get; set; }

        [JsonProperty("decorate")]public Decorate Decorate { get; set; }
    }

    public class Decorate
    {
        [JsonProperty("card_url")]public string Card_Url { get; set; }
        [JsonProperty("id")]public string ID { get; set; }

        [JsonProperty("jump_url")]public string URI { get; set; }
        [JsonProperty("name")]public string Name { get; set; }

        [JsonProperty("fan")]public Fan Fan { get; set; }
        [JsonProperty("type")]public int Type { get; set; }
    }

    public class Fan
    {
        [JsonProperty("color")]public string Color { get; set; }

        [JsonProperty("is_fan")]public string IsFan { get; set; }
        [JsonProperty("numstr")]public string Num_Str { get; set; }
        [JsonProperty("number")]public int Number { get; set; } 
        
    }

    public class Pendant
    {
        [JsonProperty("expire")]public int Expire { get; set; }
        [JsonProperty("image")]public string Image { get; set; }
        [JsonProperty("image_enhance")]public string Image_Enhance { get; set; }
        [JsonProperty("name")]public string Name { get;set; }
        [JsonProperty("pid")]public string Pid { get; set; }
    }

    public class Official_verify
    {
        [JsonProperty("desc")]public string Desc { get;set; }

        [JsonProperty("type")]public int Type { get; set; }
    }

    public class ModuleStat
    {

        [JsonProperty("comment")]public Comment Comment { get; set; }

        [JsonProperty("forward")]public Comment Forward { get; set; }

        [JsonProperty("like")]public Like Like { get; set; }
    }

    public class Like:Comment
    {
        [JsonProperty("status")]public bool Status { get; set; }
    }

    public class Comment
    {
        [JsonProperty("count")]public int Count { get; set; }
        [JsonProperty("forbidden")]public bool Forbidden { get;set; }

    }

    public class DynamicItemBasic
    {
        [JsonProperty("comment_id_str")] public string BasicID { get; set; }

        [JsonProperty("comment_type")]public int CommentType { get; set; }

        [JsonProperty("rid_str")]public string Rid { get; set; }

        [JsonProperty("like_icon")]public Basic_Icon Icon { get; set; }
    }

    public class Basic_Icon
    {
        [JsonProperty("action_url")]public string ActionUid { get; set; }

        [JsonProperty("end_url")]public string EndUrl { get; set; }

        [JsonProperty("id")]public int ID { get; set; }

        [JsonProperty("start_url")]public string StartUrl { get; set; }
    }
}
