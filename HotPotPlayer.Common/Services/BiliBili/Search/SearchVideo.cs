using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Services.BiliBili.Search
{
    public class SearchVideo
    {
        [JsonProperty("trackid")]
        public string trackid { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("nav")]
        public List<Nav> Navs { get; set; }

        [JsonProperty("item")]
        public List<Item> Items { get; set; }   
    }


    public class Nav
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("total")]
        public string Total { get; set; }

        [JsonProperty("pages")]
        public string PageCount { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class Item
    {
        [JsonProperty("trackid")]
        public string TrackId { get; set; }

        [JsonProperty("linktype")]
        public string LinkType { get; set; }

        [JsonProperty("position")]
        public string Positon { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("cover")]
        public string Cover { get; set; }

        [JsonProperty("uri")]
        public string LinkUri { get; set; }

        [JsonProperty("param")]
        public string LinkParam { get; set; }

        [JsonProperty("goto")]
        public string Goto { get; set; }

        [JsonProperty("play")]
        public long PlayCount { get; set; }

        [JsonProperty("danmaku")]
        public long DanmakuCount { get; set; }

        [JsonProperty("author")]
        public string UpName { get; set; }

        [JsonProperty("ptime")]
        public long CreateTime { get; set; }

        [JsonProperty("show_card_desc_2")]
        public string ShowTime { get; set; }

        [JsonProperty("duration")]
        public string DurationText { get; set; }

        [JsonProperty("mid")]
        public string Mid { get; set; }

        [JsonProperty("face")]
        public string UpCover { get; set; }
    }

    public class SearchAnimation_Movie
    {
        [JsonProperty("items")]
        public List<Aniation_Movie_Item> Items { get; set; }
    }

    public class Aniation_Movie_Item
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("cover")]
        public string cover { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("param")]
        public string UriParam { get; set; }

        [JsonProperty("goto")]
        public string Goto { get; set; }

        [JsonProperty("ptime")]
        public long CreateTime { get; set; }

        [JsonProperty("season_type_name")]
        public string TypeName { get; set; }

        [JsonProperty("media_type")]
        public int Media_type { get; set; }

        [JsonProperty("style")]
        public string Tag { get; set; }

        [JsonProperty("styles")]
        public string Tag2 { get; set; }

        [JsonProperty("styles_v2")]
        public string Tag2_V2 { get; set; }

        [JsonProperty("cv")]
        public string CV { get; set; }

        /// <summary>
        /// 评分
        /// </summary>
        [JsonProperty("rating")]
        public string Rating { get; set; }

        [JsonProperty("area")]
        public string Country { get; set; }

        [JsonProperty("staff")]
        public string By { get; set; }

        [JsonProperty("badge")]
        public string TypeStr { get; set; }

        [JsonProperty("episodes")]
        public List<Episodes> Episodes { get; set; }
    }

    public class Episodes
    {
        [JsonProperty("position")]
        public int Position { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("param")]
        public string Uriparam { get; set; }

        [JsonProperty("index")]
        public string Index { get; set; }


    }
}
