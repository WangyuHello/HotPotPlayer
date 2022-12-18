using HotPotPlayer.Services.BiliBili.HomeVideo;
using HotPotPlayer.Services.BiliBili.Video;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Models.BiliBili
{
    public record BiliBiliVideoItem : VideoItem
    {
        public List<DashVideo> DashVideos { get; set; }

        public List<DashVideo> DashAudios { get; set; }

        public Dolby Dolby { get; set; }

        public Flac Flac { get; set; }

        public Dictionary<string, Dictionary<string, DashVideo>> Videos { get; set; }

        public string MinBufferTime { get; set; }

        public List<Durl> Urls { get; set; }

        public static BiliBiliVideoItem FromRaw(VideoInfo videoInfo, VideoContent videosContent)
        {
            var dict = new Dictionary<string, Dictionary<string, DashVideo>>();

            foreach (var item in videoInfo.Support_Formats) 
            {
                var desc = item.New_description;
                var id = item.Quality;
                var dash = videoInfo.Dash.DashVideos.Where(d => d.ID == id).ToDictionary(d => GetCodecName(d));
                dict.Add(desc, dash);
            }

            var video = new BiliBiliVideoItem
            {
                DashVideos = videoInfo?.Dash?.DashVideos,
                DashAudios = videoInfo?.Dash?.DashAudios,
                Videos = dict,
                Dolby = videoInfo?.Dash?.Dolby,
                Flac = videoInfo?.Dash?.Flac,
                Urls = videoInfo?.DUrl,
                Title = videosContent.Title,
                MinBufferTime = videoInfo?.Dash?.MinBufferTime,
                Duration = TimeSpan.FromMilliseconds(long.Parse(videoInfo.TimeLength)),
                Cover = new Uri(videosContent.VideoImage)
            };
            return video;
        }

        public static BiliBiliVideoItem FromRaw(VideoInfo videoInfo, HomeDataItem videosContent)
        {
            var dict = new Dictionary<string, Dictionary<string, DashVideo>>();

            foreach (var item in videoInfo.Support_Formats)
            {
                var desc = item.New_description;
                var id = item.Quality;
                var dash = videoInfo.Dash.DashVideos.Where(d => d.ID == id).ToDictionary(d => GetCodecName(d));
                dict.Add(desc, dash);
            }

            var video = new BiliBiliVideoItem
            {
                DashVideos = videoInfo?.Dash?.DashVideos,
                DashAudios = videoInfo?.Dash?.DashAudios,
                Videos = dict,
                Dolby = videoInfo?.Dash?.Dolby,
                Flac = videoInfo?.Dash?.Flac,
                Urls = videoInfo?.DUrl,
                Title = videosContent.Title,
                MinBufferTime = videoInfo?.Dash?.MinBufferTime,
                Duration = TimeSpan.FromMilliseconds(long.Parse(videoInfo.TimeLength)),
                Cover = new Uri(videosContent.Cover)
            };
            return video;
        }

        private static string GetCodecName(DashVideo d)
        {
            var name = "AVC";
            if (d.Codecs.StartsWith("avc"))
            {
                name = "AVC";
            }
            else if(d.Codecs.StartsWith("hev"))
            {
                name = "HEVC";
            }
            else if(d.Codecs.StartsWith("av0"))
            {
                name = "AV1";
            }
            return name;
        }

        public (string definition, string url) GetPreferVideoUrl(string definition = "")
        {

            var best = definition switch {
                "" => Videos.First().Value,
                null => Videos.First().Value,
                _ => Videos[definition]
            };
            var bestDefi = definition switch {
                "" => Videos.First().Key,
                null => Videos.First().Key,
                _ => definition
            };
            string url;
            if (best.ContainsKey("HEVC"))
            {
                url = best["HEVC"].BaseUrl;
            }
            else if (best.ContainsKey("AV1"))
            {
                url = best["AV1"].BaseUrl;
            }
            else
            {
                url = best["AVC"].BaseUrl;
            }
            return (bestDefi, url);
        }

        public string GetPreferAudioUrl()
        {
            string best = null;
            if (Dolby != null && Dolby.Audio != null)
            {
                best = Dolby.Audio.FirstOrDefault().Base_Url;
            }
            else if (Flac != null)
            {
                best = Flac.DashAudios.Base_Url;
            }
            
            if(string.IsNullOrEmpty(best)) 
            {
                best = DashAudios.First().BaseUrl;
            }
            return best;
        }
    }
}
