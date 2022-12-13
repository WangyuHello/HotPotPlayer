using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace BiliBiliAPI.Models.Videos
{
    /// <summary>
    /// 视频基本信息
    /// </summary>
    public class VideosContent
    {
        /// <summary>
        /// BV号
        /// </summary>
        [JsonProperty("bvid")]
        public string Bvid { get; set; }
        /// <summary>
        /// AV号
        /// </summary>
        [JsonProperty("aid")]
        public string Aid { get; set; }

        /// <summary>
        /// 分P
        /// </summary>
        [JsonProperty("videos")]
        public string PageCount { get; set; }

        /// <summary>
        /// 分区内容
        /// </summary>
        [JsonProperty("tid")]
        public string Tid { get; set; }

        /// <summary>
        /// 小分区文字
        /// </summary>
        [JsonProperty("tname")]
        public string TidSmallName { get; set; }

        /// <summary>
        /// 1为原创，2为转载
        /// </summary>
        [JsonProperty("copyright")]
        public string Copyright { get; set; }

        /// <summary>
        /// 视频封面
        /// </summary>
        [JsonProperty("pic")]
        public string VideoImage { get; set; }

        /// <summary>
        /// 稿件标题
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// 发布时间的时间戳
        /// </summary>
        [JsonProperty("pubdate")]
        public string UpDataTime { get; set; }

        /// <summary>
        /// 用户投稿信息
        /// </summary>
        [JsonProperty("ctime")]
        public string UserUpdata { get; set; }

        /// <summary>
        /// 视频简介
        /// </summary>
        [JsonProperty("desc")]
        public string VideoDEsc { get; set; }

        /// <summary>
        /// 新版视频简介
        /// </summary>
        [JsonProperty("desc_v2")]
        public List<NewDesc> Desc_V2 { get; set; }

        /// <summary>
        /// 视频状态信息
        /// </summary>
        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("duration")]
        public string All_Duration { get; set; }

        /// <summary>
        /// 撞车了，这个是AID
        /// </summary>
        [JsonProperty("forward")]
        public string Forward { get; set; }

        /// <summary>
        /// 参与活动的ID
        /// </summary>
        [JsonProperty("mission_id")]
        public string Mission_id { get; set; }

        /// <summary>
        /// 重新定向URL，番剧和影视存在
        /// </summary>
        [JsonProperty("redirect_url")]
        public string Redirect_Url { get; set; }

        /// <summary>
        /// 视频同步发布的的动态的文字内容
        /// </summary>
        [JsonProperty("dynamic")]
        public string dynamic { get; set; }

        /// <summary>
        /// 第一P视频的CID
        /// </summary>
        [JsonProperty("cid")]
        public string First_Cid { get; set; }


        [JsonProperty("rights")]
        public CopyRight Right { get; set; }

        [JsonProperty("owner")]
        public Up Up { get; set; }

        [JsonProperty("stat")]
        public Stat Stat { get; set; }

        [JsonProperty("pages")]
        public List<Page> Pages { get; set; }


        [JsonProperty("first_frame")]
        public string First_Image { get; set; }

        /// <summary>
        /// 用户对于该视频的推广信息（投币，点赞，收藏）
        /// </summary>
        [JsonProperty("req_user")]
        public ReqUser ReqUser { get; set; }

        [JsonProperty("tag")]
        public List<Tag> Tag { get; set; }

        [JsonProperty("relates")]
        public List<Relates> Relates { get; set; }

        [JsonProperty("history")]
        public History History { get; set; }
    }


    public class History
    {
        [JsonProperty("cid")]
        public string Cid { get; set; }

        /// <summary>
        /// 播放进度，以秒为单位
        /// </summary>
        [JsonProperty("progress")]
        public string Progress { get; set;   }
    }

    public class Relates
    {
        public string aid { get; set; }
        public string pic { get; set; }
        public string title { get; set; }

        [JsonProperty("owner")]
        public Owner Owner { get; set; }

        [JsonProperty("stat")]
        public RelatesStat Stat { get; set; }

        [JsonProperty("goto")]
        public string Goto { get; set; }

        [JsonProperty("param")]
        public string Param { get; set; }

        [JsonProperty("cid")]
        public string CId { get; set; }
    }

    public class RelatesStat
    {
        public string aid { get; set; }
        public string view { get; set; }
        public string danmaku { get; set; }
        public string reply { get; set; }
        public string favorite { get; set; }
        public string coin { get; set; }
        public string share { get; set; }
        /// <summary>
        /// 当前排行
        /// </summary>
        public string now_rank { get; set; }

        /// <summary>
        /// 历史排行
        /// </summary>
        public string his_rank { get; set; }

        public string like { get; set; }
        public string dislike { get; set; }
    }

    public class Owner
    {
        public string mid { get; set; }
        public string name { get; set; }
        public string stat { get; set; }
    }

    public class Tag
    {
        [JsonProperty("tag_id")]
        public string ID { get; set; }

        [JsonProperty("tag_name")]
        public string Name { get; set; }

        [JsonProperty("cover")]
        public string Conver { get; set; }

        [JsonProperty("likes")]
        public string Likes { get; set; }

        [JsonProperty("hates")]
        public string Hates { get; set; }

        [JsonProperty("liked")]
        public string liked { get; set; }

        [JsonProperty("hated")]
        public string Hated { get; set; }

        [JsonProperty("attribute")]
        public string Attribute { get; set; }

        [JsonProperty("is_activity")]
        public string Is_activity { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("tag_type")]
        public string Tag_Type { get; set; }
    }

    public class NewDesc
    {
        /// <summary>
        /// 包含换行符的简介信息
        /// </summary>
        [JsonProperty("raw_text")]
        public string Text { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("biz_id")]
        public string Biz_Id { get; set; }
    }

    public class CopyRight
    {

        /// <summary>
        /// 是否支持充电
        /// </summary>
        public string elec { get; set; }

        /// <summary>
        /// 是否能下载，1为ok，0为不行
        /// </summary>
        public string download { get; set; }

        /// <summary>
        /// 是否为电影
        /// </summary>
        public string movie { get; set; }

        /// <summary>
        /// 是否需要额外付费
        /// </summary>
        public string pay { get; set; } 
        
        /// <summary>
        /// 是否高码率
        /// </summary>
        public string hd5 { get; set; }

        /// <summary>
        /// 是否显示转载，1为显示，0为不显示
        /// </summary>
        public string no_reprint { get; set; }

        /// <summary>
        /// 是否为UGC付费电影
        /// </summary>
        public string ugc_pay { get; set; }

        /// <summary>
        /// 是否为互动视频
        /// </summary>
        public string is_stein_gate { get; set; }

        /// <summary>
        /// 是否联合投稿
        /// </summary>
        public string is_cooperation { get; set; }
    }

    public class Up
    {
        /// <summary>
        /// UP主的UID
        /// </summary>
        [JsonProperty("mid")]
        public string mid { get; set; }

        [JsonProperty("face")]
        public string Face { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class ReqUser
    {
        [JsonProperty("attention")]
        public string Attention { get; set; } = "0";

        /// <summary>
        /// 作用尚不明确
        /// </summary>
        [JsonProperty("guest_attention")]
        public string Guest_attention { get; set; }

        /// <summary>
        /// 是否收藏
        /// </summary>
        [JsonProperty("favorite")]
        public string Favorite { get; set; } = "0";

        /// <summary>
        /// 是否点赞
        /// </summary>
        [JsonProperty("like")]
        public string Like { get; set; } = "0";

        /// <summary>
        /// 受否投币
        /// </summary>
        [JsonProperty("coin")]
        public string Coin { get; set; } = "0";
    }

    public class Stat
    {
        /// <summary>
        /// AV号
        /// </summary>
        [JsonProperty("aid")]
        public string Aid { get; set; }

        /// <summary>
        /// 播放数量
        /// </summary>
        [JsonProperty("view")]
        public string Views { get; set; }

        /// <summary>
        /// 弹幕数量
        /// </summary>
        [JsonProperty("danmaku")]
        public string DanMaku { get; set; }

        /// <summary>
        /// 评论
        /// </summary>
        [JsonProperty("评论数量")]
        public string Reply { get; set; }

        /// <summary>
        ///收藏
        /// </summary>
        [JsonProperty("favorite")]
        public string Favorite { get; set; }

        /// <summary>
        /// 投币
        /// </summary>
        [JsonProperty("coin")]
        public string coin { get; set; }

        /// <summary>
        /// 分享
        /// </summary>
        [JsonProperty("share")]
        public string Share { get; set; }

        /// <summary>
        /// 当前排名
        /// </summary>
        [JsonProperty("now_rank")]
        public string Now_Rank { get; set; }

        /// <summary>
        /// 历史最高排名
        /// </summary>
        [JsonProperty("his_rank")]
        public string His_Rank { get; set; }

        /// <summary>
        /// 点赞
        /// </summary>
        [JsonProperty("like")]
        public string Like { get; set; }

        /// <summary>
        /// 点踩的数量
        /// </summary>
        [JsonProperty("dislike")]
        public string DisLike { get; set; }

        /// <summary>
        /// 评分
        /// </summary>
        [JsonProperty("evaluation")]
        public string Evaluation { get; set; }

        /// <summary>
        /// 警告信息
        /// </summary>
        [JsonProperty("argue_msg")]
        public string ArgueMessage { get; set; }
    }

    public class Page
    {
        [JsonProperty("cid")]
        public string Cid { get; set; }

        /// <summary>
        /// 当前分页
        /// </summary>
        [JsonProperty("page")]
        public string page { get; set; }

        /// <summary>
        /// 来源，默认为B站，qq为腾讯，hunan为芒果
        /// </summary>
        [JsonProperty("from")]
        public string From { get; set; }

        /// <summary>
        /// 分P标题
        /// </summary>
        [JsonProperty("part")]
        public string Title { get; set; }

        /// <summary>
        /// 分P时常，以秒为单位
        /// </summary>
        [JsonProperty("duration")]
        public string Duration { get; set; }

        /// <summary>
        /// 站外视频vid
        /// </summary>
        [JsonProperty("vid")]
        public string Vid { get; set; }

        /// <summary>
        /// 站外Link
        /// </summary>
        [JsonProperty("weblink")]
        public string Link { get; set; }

        /// <summary>
        /// 第一P的视频分辨率
        /// </summary>
        [JsonProperty("dimension")]
        public PageDimension PageDim { get; set; }

    }

    public class SubTitle
    {
        [JsonProperty("allow_submit")]
        public string Allow_SubMit { get; set; }

        [JsonProperty("list")]
        public List<SubTitleList> List { get; set; }
    }

    public class SubTitleList
    {
        [JsonProperty("id_str")]
        public string id { get; set; }

        [JsonProperty("lan")]
        public string lang { get; set; }

        [JsonProperty("lan_doc")]
        public string Name { get; set; }

        [JsonProperty("subtitle_url")]
        public string SubUrl { get; set; }
    }

    public class PageDimension
    {

        /// <summary>
        /// 宽度
        /// </summary>
        public string Width { get; set; }

        /// <summary>
        /// 高度
        /// </summary>
        public string Height { get; set; }

        /// <summary>
        /// 是否宽高对换，0为正常，1为对换
        /// </summary>
        [JsonProperty("rotate")]
        public string Rotate { get; set; }
    }
}
