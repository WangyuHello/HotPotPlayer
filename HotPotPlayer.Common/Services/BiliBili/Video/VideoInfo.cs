using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Services.BiliBili.Video
{
    public class VideoInfo
    {
        [JsonProperty("from")]
        public string from { get; set; }
        [JsonProperty("result")]
        public string result { get; set; }
        [JsonProperty("message")]
        public string message { get; set; }
        [JsonProperty("format")]
        public string Format { get; set; }

        /// <summary>
        /// 视频长度
        /// </summary>
        [JsonProperty("timelength")]
        public string TimeLength { get; set; }

        [JsonProperty("accept_format")]
        public string VideoFormat { get; set; }

        /// <summary>
        /// 视频清晰度列表
        /// </summary>
        [JsonProperty("accept_description")]
        public List<string> Description { get; set; }

        /// <summary>
        /// 视频流编码ID
        /// </summary>
        [JsonProperty("video_codecid")]
        public string VideoCodeCid { get; set; }

        [JsonProperty("durl")]
        public List<Durl> DUrl { get; set; }

        [JsonProperty("dash")]
        public Dash Dash { get; set; }

        [JsonProperty("last_play_time")]
        public long LastPlay { get; set; }

        [JsonProperty("last_play_cid")]
        public long LastCid { get; set; }

        [JsonProperty("accept_quality")]
        public List<int> Quality { get; set; }


        [JsonProperty("support_formats")]
        public List<Support_Formats> Support_Formats { get; set; }
    }


    public class Dash
    {
        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("minBufferTime")]
        public string MinBufferTime { get; set; }

        [JsonProperty("min_buffer_time")]
        public string Min_buffer_time { get; set; }

        [JsonProperty("video")]
        public List<DashVideo> DashVideos { get; set; }

        [JsonProperty("audio")]
        public List<DashVideo> DashAudios { get; set; }
    }

    public class DashVideo
    {
        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("baseUrl")]
        public string BaseUrl { get; set; }

        [JsonProperty("base_url")]
        public string Base_Url { get; set; }

        [JsonProperty("backupUrl")]
        public List<string> BackupUrl { get; set; }

        [JsonProperty("backup_url")]
        public List<string> Backup_Url { get; set; }

        /// <summary>
        /// 视频流格式
        /// </summary>
        [JsonProperty("mimeType")]
        public string MimeType { get; set; }

        /// <summary>
        /// 视频流编码器
        /// </summary>
        [JsonProperty("codecs")]
        public string Codecs { get; set; }

        [JsonProperty("height")]
        public string Height { get; set; }

        [JsonProperty("width")]
        public string Width { get; set; }

        [JsonProperty("frameRate")]
        public string FPS { get; set; }

        [JsonProperty("SegmentBase")]
        public SegmentBase SegmentBase { get; set; }

        [JsonProperty("bandwidth")]
        public string BandWidth { get; set; }

    }

    public class SegmentBase
    {
        [JsonProperty("Initialization")]
        public string Initialization { get; set; }

        [JsonProperty("indexRange")]
        public string IndexRange { get; set; }
    }
    public class Support_Formats
    {
        [JsonProperty("quality")]
        public string Quality { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("new_description")]
        public string New_description { get; set; }

        [JsonProperty("display_desc")]
        public string Display_Desc { get; set; }

        [JsonProperty("superscript")]
        public string SuperScript { get; set; }

        [JsonProperty("codecs")]
        public List<string> Codec { get; set; }
    }

    public class Durl
    {
        [JsonProperty("order")]
        public int Order { get; set; }

        [JsonProperty("length")]
        public int Length { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
