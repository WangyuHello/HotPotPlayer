using HotPotPlayer.Bilibili.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Bilibili.Models.Search
{
    public class SearchResultData
    {
        [JsonProperty("type")] public string? Type { get; set; }
        [JsonProperty("id")] public string? Id { get; set; }
        [JsonProperty("author")] public string? Author { get; set; }
        [JsonProperty("mid")] public string? Mid { get; set; }
        [JsonProperty("aid")] public string? Aid { get; set; }
        [JsonProperty("bvid")] public string? Bvid { get; set; }
        [JsonProperty("title")] public string? Title { get; set; }
        public string GetTitle()
        {
            if (string.IsNullOrEmpty(Title))
            {
                return string.Empty;
            }
            StringBuilder sb = new();
            int state = 0;
            for (int i = 0; i < Title.Length; i++)
            {
                if (state == 0)
                {
                    if (Title[i] == '<')
                    {
                        state = 1;
                        continue;
                    }
                    else
                    {
                        sb.Append(Title[i]);
                    }
                }
                else if(state == 1)
                {
                    if (Title[i] == '>')
                    {
                        state = 0;
                    }
                }
            }
            return sb.ToString();
        }

        [JsonProperty("description")] public string? Description { get; set; }
        [JsonProperty("pic")] public string? Pic { get; set; }

        public string GetCover()
        {
            if (string.IsNullOrEmpty(Pic))
            {
                return string.Empty;
            }
            if (Pic.StartsWith("//"))
            {
                return "https:" + Pic;
            }
            return Pic;
        }
        [JsonProperty("play")] public int Play { get; set; }
        [JsonProperty("video_review")] public int VideoReview { get; set; }
        [JsonProperty("favorites")] public int Favorites { get; set; }
        [JsonProperty("pubdate")] public string? PubDate { get; set; }
        public string? GetPubDate()
        {
            return PubDate?.GetDateTime();
        }
        [JsonProperty("duration")] public string? Duration { get; set; }
    }
}
