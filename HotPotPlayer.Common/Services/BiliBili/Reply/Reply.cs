using HotPotPlayer.Services.BiliBili.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Services.BiliBili.Reply
{
    public class Replies
    {
        [JsonProperty("page")] public Page Page { get; set; }
        [JsonProperty("config")] public Config Config { get; set; }
        [JsonProperty("replies")] public List<Reply> TheReplies { get; set; }
        [JsonProperty("hots")] public List<Reply> Hots { get; set; }
        [JsonProperty("upper")] public Reply Upper { get; set; }
    }

    public class Page
    {
        [JsonProperty("num")] public int Num { get; set; }
        [JsonProperty("size")] public int Size { get; set; }
        [JsonProperty("count")] public int Count { get; set; }
        [JsonProperty("acount")] public int Acount { get; set; }
    }

    public class Config
    {
        [JsonProperty("showadmin")] public int ShowAdmin { get; set; }
        [JsonProperty("showentry")] public int ShowEntry { get; set; }
        [JsonProperty("showfloor")] public int ShowFloor { get; set; }
        [JsonProperty("showtopic")] public int ShowTopic { get; set; }
        [JsonProperty("show_up_flag")] public bool ShowUpFlag { get; set; }
        [JsonProperty("read_only")] public bool ReadOnly { get; set; }
        [JsonProperty("show_del_log")] public bool ShowDelLog { get; set; }
    }

    public class Reply
    {
        [JsonProperty("rpid")] public string RPId { get; set; }
        [JsonProperty("oid")] public string OId { get; set; }
        [JsonProperty("type")] public int Type { get; set; }
        [JsonProperty("mid")] public string Mid { get; set; }
        [JsonProperty("root")] public string Root { get; set; }
        [JsonProperty("parent")] public string Parent { get; set; }
        [JsonProperty("dialog")] public string Dialog { get; set; }
        [JsonProperty("count")] public int Count { get; set; }
        [JsonProperty("ctime")] public string Time { get; set; }
        [JsonProperty("like")] public int Like { get; set; }
        [JsonProperty("member")] public Member Member { get; set; }
        [JsonProperty("content")] public Content Content { get; set; }
        [JsonProperty("replies")] public List<Reply> TheReplies { get; set; }

        public bool HasNestedReply => TheReplies != null && TheReplies.Any();

        public string GetNestedReplyStr => $"共{TheReplies?.Count}条回复";

        public string TimeStr
        {
            get
            {
                int i = int.Parse(Time);
                var ts = TimeSpan.FromSeconds(i);
                var time = new DateTime(ts.Ticks);
                time = time.AddYears(1969);
                return $"{time.Year}-{time.Month}-{time.Day} {ts:hh\\:mm}";
            }
        }
    }

    public class Member
    {
        [JsonProperty("mid")] public string Mid { get; set; }
        [JsonProperty("uname")] public string UName { get; set; }
        [JsonProperty("avatar")] public string Avatar { get; set; }
        [JsonProperty("level_info")] public LevelInfo LevelInfo { get; set; }
    }

    public class Content
    {
        [JsonProperty("message")] public string Message { get; set; }
    }
}
