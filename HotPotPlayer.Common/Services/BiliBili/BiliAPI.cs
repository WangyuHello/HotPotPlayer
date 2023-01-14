using HttpClientFactoryLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using BiliBiliAPI.Models.Account;
using BiliBiliAPI.ApiTools;
using QRCoder;
using System.Web;
using Microsoft.AspNetCore.WebUtilities;
using HotPotPlayer.Services.BiliBili.Video;
using BiliBiliAPI.Models;
using Newtonsoft.Json;
using HotPotPlayer.Services.BiliBili.HomeVideo;
using HotPotPlayer.Services.BiliBili.Reply;
using HotPotPlayer.Services.BiliBili.Dynamic;
using HotPotPlayer.Services.BiliBili.User;
using System.Diagnostics;
using Bilibili.Community.Service.Dm.V1;
using Grpc.Net.Client;
using Grpc.Core;
using HotPotPlayer.Services.BiliBili.Danmaku;

namespace HotPotPlayer.Services.BiliBili
{
    public class BiliAPI
    {
        readonly HttpClientFactory _httpClientFactory;
        public HttpClientFactory HttpClientFactory => _httpClientFactory;

        readonly DM.DMClient _dmClient;
        readonly Metadata _grpcHeaders;

        public BiliAPI()
        {
            _httpClientFactory = new HttpClientFactory();
            _httpClientFactory.Register("web",
                builder => builder
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var handler = new HttpClientHandler
                    {
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                        UseCookies = false,
                    };
                    handler.ServerCertificateCustomValidationCallback += (sender, cert, chaun, ssl) => { return true; };
                    return handler;
                })
                .SetHandlerLifetime(TimeSpan.FromDays(1))
                .ConfigureHttpClient(client =>
                {
                    client.DefaultRequestHeaders.Referrer = new Uri("https://www.bilibili.com/");
                    client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                    client.Timeout = TimeSpan.FromSeconds(8);
                }));
            _httpClientFactory.Register("app",
                builder => builder
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var handler = new HttpClientHandler
                    {
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                        UseCookies = false,
                    };
                    handler.ServerCertificateCustomValidationCallback += (sender, cert, chaun, ssl) => { return true; };
                    return handler;
                })
                .SetHandlerLifetime(TimeSpan.FromDays(1))
                .ConfigureHttpClient(client =>
                {
                    client.DefaultRequestHeaders.Referrer = new Uri("https://www.bilibili.com/");
                    client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                    client.Timeout = TimeSpan.FromSeconds(8);
                }));

            var channel = GrpcChannel.ForAddress("https://api.bilibili.com/x/v2/dm/web/seg.so");
            _dmClient = new DM.DMClient(channel);
            _grpcHeaders = new Metadata
            {
                { "Origin", "https://www.bilibili.com/" },
                { "Referrer", "https://www.bilibili.com/" },
            };
        }
        public enum ResponseEnum { App, Web }
        AccountToken token;
        public const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36 Edg/110.0.0.0";
        public CookieCollection Cookies { get; set; } = new CookieCollection();

        public string CookieString => string.Join("; ", Cookies.Cast<System.Net.Cookie>().Select(t => t.Name + "=" + t.Value));

        private async Task<string> GetAsync(string url, ResponseEnum responseEnum = ResponseEnum.Web, Dictionary<string, string> keyValues = null, bool IsAcess = true, string BuildString = "&platform=android&device=android&actionKey=appkey&build=5442100&mobi_app=android_comic")
        {
            switch (responseEnum)
            {
                case ResponseEnum.App:
                    if (url.IndexOf("?") > -1)
                        url += (IsAcess == true ? "&access_key=" + token.SECCDATA : "") + "&appkey=" + ApiProvider.AndroidTVKey.Appkey + BuildString + "&ts=" + ApiProvider.TimeSpanSeconds;
                    else
                        url += (IsAcess == true ? "?access_key=" + token.SECCDATA : "") + "&appkey=" + ApiProvider.AndroidTVKey.Appkey + BuildString + "&ts=" + ApiProvider.TimeSpanSeconds;
                    url += (IsAcess == true ? "&sign=" + ApiProvider.GetSign(url, ApiProvider.AndroidTVKey) : "");
                    var appClient = _httpClientFactory.CreateClient("app");
                    appClient.DefaultRequestHeaders.Add("Cookie", CookieString);
                    HttpResponseMessage apphr = await appClient.GetAsync(url).ConfigureAwait(false);
                    apphr.Headers.Add("Accept_Encoding", "gzip,deflate");
                    apphr.EnsureSuccessStatusCode();
                    var appencodeResults = await apphr.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    string appstr = Encoding.UTF8.GetString(appencodeResults, 0, appencodeResults.Length);
                    return appstr;
                case ResponseEnum.Web:
                    using (var webClient = _httpClientFactory.CreateClient("web"))
                    {
                        webClient.DefaultRequestHeaders.Add("Cookie", CookieString);
                        //if (keyValues != null)
                        //{
                        //    foreach (var item in keyValues)
                        //    {
                        //        //webClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                        //    }
                        //}
                        string url2 = keyValues == null ? url : QueryHelpers.AddQueryString(url, keyValues);
                        using var webhr = await webClient.GetAsync(url2).ConfigureAwait(false);
                        webhr.EnsureSuccessStatusCode();

                        if (webhr.Headers.TryGetValues("Set-Cookie", out var rawSetCookie))
                        {
                            Cookies.Add(ParseCookies(rawSetCookie));
                        }

                        var webencodeResults = await webhr.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                        string webstr = Encoding.UTF8.GetString(webencodeResults, 0, webencodeResults.Length);
                        return webstr;
                    }
            }
            return null;
        }

        private async Task<string> PostAsync(string url, Dictionary<string, string> keyValues = null)
        {
            using (var webClient = _httpClientFactory.CreateClient("web"))
            {
                webClient.DefaultRequestHeaders.Add("Cookie", CookieString);
                var form = new FormUrlEncodedContent(keyValues);
                using var webhr = await webClient.PostAsync(url, form).ConfigureAwait(false);
                webhr.EnsureSuccessStatusCode();

                if (webhr.Headers.TryGetValues("Set-Cookie", out var rawSetCookie))
                {
                    Cookies.Add(ParseCookies(rawSetCookie));
                }

                var webencodeResults = await webhr.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                string webstr = Encoding.UTF8.GetString(webencodeResults, 0, webencodeResults.Length);
                return webstr;
            }
        }

        private string GetCsrf()
        {
            return Cookies.First(c => c.Name == "bili_jct").Value;
        }

        /// <summary>
        /// https://github.com/SocialSisterYi/bilibili-API-collect/blob/master/login/login_action/QR.md
        /// </summary>
        /// <returns></returns>
        public async Task<(string token, string url)> RequestQrCode()
        {
            var r = await GetAsync("https://passport.bilibili.com/x/passport-login/web/qrcode/generate", ResponseEnum.Web);
            var j = JObject.Parse(r);
            var url = j["data"]["url"].Value<string>();
            var token = j["data"]["qrcode_key"].Value<string>();
            return (token, url);
        }

        /// <summary>
        /// https://github.com/SocialSisterYi/bilibili-API-collect/blob/master/login/login_info.md
        /// </summary>
        /// <returns></returns>
        public async Task<JObject> GetLoginInfoAsync()
        {
            var r = await GetAsync("https://api.bilibili.com/x/web-interface/nav/stat", ResponseEnum.Web);
            return JObject.Parse(r);
        }

        /// <summary>
        /// https://github.com/SocialSisterYi/bilibili-API-collect/blob/master/login/login_action/QR.md
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<JObject> GetQrCodePoll(string key)
        {
            var r = await GetAsync("https://passport.bilibili.com/x/passport-login/web/qrcode/poll", ResponseEnum.Web,
                new Dictionary<string, string> { ["qrcode_key"] = key });
            var j = JObject.Parse(r);
            return j;
        }

        /// <summary>
        /// https://github.com/SocialSisterYi/bilibili-API-collect/blob/master/video/videostream_url.md
        /// </summary>
        /// <param name="bvid">稿件bvid</param>
        /// <param name="cid">视频cid</param>
        /// <param name="qn">视频清晰度选择</param>
        /// <param name="fnval">视频获取方式选择</param>
        /// <param name="fourk">是否允许4K视频</param>
        /// <returns></returns>
        public async Task<BiliResult<VideoInfo>> GetVideoUrl(string bvid, string aid, string cid, DashEnum qn = DashEnum.Dash480, FnvalEnum fnval = FnvalEnum.Dash, int fourk = 0)
        {
            var idHead = bvid == null ? "aid" : "bvid";
            var idval = bvid == null ? aid : bvid;
            var r = await GetAsync("https://api.bilibili.com/x/player/playurl", ResponseEnum.Web,
                new Dictionary<string, string> {
                    [idHead] = idval,
                    ["cid"] = cid,
                    ["qn"] = ((int)qn).ToString(),
                    ["fnval"] = ((int)fnval).ToString(),
                    ["fnver"] = "0",
                    ["fourk"] = fourk.ToString(),
                });
            var res = JsonConvert.DeserializeObject<BiliResult<VideoInfo>>(r);
            return res;
        }

        /// <summary>
        /// https://github.com/SocialSisterYi/bilibili-API-collect/blob/master/video/info.md
        /// </summary>
        /// <param name="bvid">稿件bvid</param>
        /// <returns></returns>
        public async Task<BiliResult<VideoContent>> GetVideoInfo(string bvid)
        {
            var r = await GetAsync("https://api.bilibili.com/x/web-interface/view", ResponseEnum.Web,
                new Dictionary<string, string>
                {
                    ["bvid"] = bvid,
                });
            var res = JsonConvert.DeserializeObject<BiliResult<VideoContent>>(r);
            return res;
        }

        /// <summary>
        /// https://github.com/SocialSisterYi/bilibili-API-collect/blob/master/video/tags.md
        /// </summary>
        /// <param name="bvid"></param>
        /// <returns></returns>
        public async Task<BiliResult<List<Tag>>> GetVideoTags(string bvid)
        {
            var r = await GetAsync("https://api.bilibili.com/x/tag/archive/tags", ResponseEnum.Web,
                new Dictionary<string, string>
                {
                    ["bvid"] = bvid,
                });
            var res = JsonConvert.DeserializeObject<BiliResult<List<Tag>>>(r);
            return res;
        }

        /// <summary>
        /// https://github.com/SocialSisterYi/bilibili-API-collect/blob/master/ranking&dynamic/popular.md
        /// </summary>
        /// <param name="pn">页码</param>
        /// <param name="ps">每页项数</param>
        public async Task<BiliResult<PopularVideos>> GetPopularVideo(int pn = 1, int ps = 20)
        {
            var r = await GetAsync("https://api.bilibili.com/x/web-interface/popular", ResponseEnum.Web,
                new Dictionary<string, string>
                {
                    ["pn"] = pn.ToString(),
                    ["ps"] = ps.ToString()
                });
            var res = JsonConvert.DeserializeObject<BiliResult<PopularVideos>>(r);
            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<BiliResult<HomeData>> GetRecVideo(int pn = 1, int ps = 20)
        {
            var r = await GetAsync("https://api.bilibili.com/x/web-interface/index/top/feed/rcmd", ResponseEnum.Web,
                new Dictionary<string, string>
                {
                    ["fresh_idx"] = "1",
                    ["feed_version"] = "V1",
                    ["fresh_type"] = "4",
                    ["pn"] = pn.ToString(),
                    ["ps"] = ps.ToString(),
                    ["plat"] = "1"
                });
            var res = JsonConvert.DeserializeObject<BiliResult<HomeData>>(r);
            return res;
        }

        public async Task<string> GetOnlineCount(string bvid, string cid)
        {
            var r = await GetAsync("https://api.bilibili.com/x/player/online/total", ResponseEnum.Web,
                new Dictionary<string, string>
                {
                    ["bvid"] = bvid,
                    ["cid"] = cid
                });
            var res = JObject.Parse(r);
            return res["data"]["total"].Value<string>();
        }

        /// <summary>
        /// https://github.com/SocialSisterYi/bilibili-API-collect/blob/master/comment/list.md#%E8%8E%B7%E5%8F%96%E8%AF%84%E8%AE%BA%E5%8C%BA%E6%98%8E%E7%BB%86_%E6%87%92%E5%8A%A0%E8%BD%BD
        /// </summary>
        /// <param name="type"></param>
        /// <param name="oid"></param>
        /// <returns></returns>
        public async Task<BiliResult<Replies>> GetReplyAsync(string type, string oid, int next)
        {
            var r = await GetAsync("https://api.bilibili.com/x/v2/reply/main", ResponseEnum.Web,
                new Dictionary<string, string>
                {
                    ["type"] = type,
                    ["oid"] = oid,
                    ["mode"] = "3",
                    ["next"] = next.ToString(),
                    ["ps"] = "20",
                });
            var res = JsonConvert.DeserializeObject<BiliResult<Replies>>(r);
            return res;
        }

        /// <summary>
        /// https://github.com/SocialSisterYi/bilibili-API-collect/blob/master/comment/list.md
        /// </summary>
        /// <param name="avid"></param>
        /// <returns></returns>
        public async Task<BiliResult<Replies>> GetVideoReplyAsync(string avid, int next = 0)
        {
            return await GetReplyAsync("1", avid, next);
        }

        public async Task<BiliResult<Replies>> GetTextDynamicReplyAsync(string did, int next = 0)
        {
            return await GetReplyAsync("17", did, next);
        }

        public async Task<BiliResult<Replies>> GetArtileDynamicReplyAsync(string cvid, int next = 0)
        {
            return await GetReplyAsync("12", cvid, next);
        }

        public async Task<BiliResult<Replies>> GetPictureDynamicReplyAsync(string id, int next = 0)
        {
            return await GetReplyAsync("11", id, next);
        }

        public async Task<BiliResult<List<VideoContent>>> GetRelatedVideo(string bvid)
        {
            var r = await GetAsync("https://api.bilibili.com/x/web-interface/archive/related", ResponseEnum.Web,
                new Dictionary<string, string>
                {
                    ["bvid"] = bvid,
                });
            var res = JsonConvert.DeserializeObject<BiliResult<List<VideoContent>>>(r);
            return res;
        }

        public async Task<BiliResult<DynamicData>> GetDynamic(DynamicType type, string offset = "", int page = 1)
        {
            var typeStr = type switch
            {
                DynamicType.Video => "video",
                DynamicType.All => "all",
                DynamicType.AnimationPGC => "pgc",
                DynamicType.Read => "article",
                _ => throw new NotImplementedException(),
            };
            var para = new Dictionary<string, string>
            {
                ["timezone_offset"] = "-480",
                ["type"] = typeStr,
                ["page"] = page.ToString(),
            };
            if (!string.IsNullOrEmpty(offset))
            {
                para["offset"] = offset;
            }
            var r = await GetAsync("https://api.bilibili.com/x/polymer/web-dynamic/v1/feed/all", ResponseEnum.Web, para);

            var res = JsonConvert.DeserializeObject<BiliResult<DynamicData>>(r);
            return res;
        }

        /// <summary>
        /// https://github.com/SocialSisterYi/bilibili-API-collect/blob/master/search/search_request.md
        /// </summary>
        /// <param name="keyword"></param>
        //public async void Search(string keyword)
        //{
        //    var r = await GetAsync("https://api.bilibili.com/x/web-interface/search/all/v2", ResponseEnum.Web,
        //        new Dictionary<string, string>
        //        {
        //            ["keyword"] = keyword,
        //        });
        //    var res = JsonConvert.DeserializeObject<BiliResult<List<VideoContent>>>(r);
        //    return res;
        //}

        // https://github.com/SocialSisterYi/bilibili-API-collect/blob/master/user/info.md
        public async Task<BiliResult<UserCardBundle>> GetUserCardBundle(string mid, bool requestTopPhoto)
        {
            if (UserCardCache.ContainsKey(mid))
            {
                return UserCardCache[mid];
            }
            var r = await GetAsync("https://api.bilibili.com/x/web-interface/card", ResponseEnum.Web,
                new Dictionary<string, string>
                {
                    ["mid"] = mid,
                    ["photo"] = requestTopPhoto ? "true" : "false"
                });
            var res = JsonConvert.DeserializeObject<BiliResult<UserCardBundle>>(r);
            UserCardCache[mid] = res;
            return res;
        }

        /// <summary>
        /// https://github.com/SocialSisterYi/bilibili-API-collect/blob/master/video/report.md
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="cid"></param>
        /// <param name="progress"></param>
        public async Task<BiliResult> Report(string aid, string cid, int progress)
        {
            var r = await PostAsync("https://api.bilibili.com/x/v2/history/report", new Dictionary<string, string>
            {
                ["aid"] = aid,
                ["cid"] = cid,
                ["progress"] = progress.ToString(),
                ["platform"] = "android",
                ["csrf"] = GetCsrf()
            });
            var res = JsonConvert.DeserializeObject<BiliResult>(r);
            return res;
        }

        /// <summary>
        /// https://github.com/SocialSisterYi/bilibili-API-collect/blob/master/video/like_coin_fav.md#%E7%82%B9%E8%B5%9E%E8%A7%86%E9%A2%91web%E7%AB%AF
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="like"></param>
        /// <returns></returns>
        public async Task<BiliResult> Like(string aid, bool like)
        {
            var r = await PostAsync("https://api.bilibili.com/x/web-interface/archive/like", new Dictionary<string, string>
            {
                ["aid"] = aid,
                ["like"] = like ? "1" : "2",
                ["csrf"] = GetCsrf()
            });
            var res = JsonConvert.DeserializeObject<BiliResult>(r);
            return res;
        }

        public async Task<bool> Coin(string aid, int multiply)
        {
            var r = await PostAsync("https://api.bilibili.com/x/web-interface/coin/add", new Dictionary<string, string>
            {
                ["aid"] = aid,
                ["multiply"] = multiply.ToString(),
                ["csrf"] = GetCsrf()
            });
            var res = JsonConvert.DeserializeObject<BiliResult>(r);
            return res.Code == 0;
        }

        public async Task<bool> Favor(string aid)
        {
            var r = await PostAsync("https://api.bilibili.com/x/v3/fav/resource/deal", new Dictionary<string, string>
            {
                ["rid"] = aid,
                ["type"] = "2",
                ["csrf"] = GetCsrf()
            });
            var res = JsonConvert.DeserializeObject<BiliResult>(r);
            return res.Code == 0;
        }

        public async Task<bool> IsLike(string aid)
        {
            var r = await GetAsync("https://api.bilibili.com/x/web-interface/archive/has/like", ResponseEnum.Web,
                new Dictionary<string, string>
                {
                    ["aid"] = aid,
                });
            var res = JsonConvert.DeserializeObject<BiliResult<int>>(r);
            return res.Data == 1;
        }

        public async Task<int> GetCoin(string aid)
        {
            var r = await GetAsync("https://api.bilibili.com/x/web-interface/archive/coins", ResponseEnum.Web,
                new Dictionary<string, string>
                {
                    ["aid"] = aid,
                });
            var res = JObject.Parse(r);
            return res["data"]["multiply"].Value<int>();
        }

        public async Task<bool> IsFavored(string aid)
        {
            var r = await GetAsync("https://api.bilibili.com/x/v2/fav/video/favoured", ResponseEnum.Web,
                new Dictionary<string, string>
                {
                    ["aid"] = aid,
                });
            var res = JObject.Parse(r);
            return res["data"]["favoured"].Value<bool>();
        }

        private Dictionary<string, BiliResult<UserCardBundle>> userCardCache;
        private Dictionary<string, BiliResult<UserCardBundle>> UserCardCache => userCardCache ??= new Dictionary<string, BiliResult<UserCardBundle>>();

        public async Task<DmSegMobileReply> GetDM(string avid, string cid, long segmentIndex = 1)
        {
            var r = await _dmClient.DmSegMobileAsync(new DmSegMobileReq 
            { 
                Type = 1,
                Pid = long.Parse(avid),
                Oid = long.Parse(cid),
                SegmentIndex = segmentIndex
            }, _grpcHeaders);
            return r;
        }

        public async Task<DMData> GetDMXml(string cid)
        {
            var r = await GetAsync("https://api.bilibili.com/x/v1/dm/list.so", ResponseEnum.Web,
                new Dictionary<string, string>
                {
                    ["oid"] = cid,
                });
            return new DMData(r);
        }

        public async Task<BiliResult<UserVideoInfo>> GetUserVideoInfo(string mid, int pn, int ps)
        {
            var r = await GetAsync("https://api.bilibili.com/x/space/arc/search", ResponseEnum.Web,
                new Dictionary<string, string>
                {
                    ["mid"] = mid,
                    ["order"] = "pubdate",
                    ["pn"] = pn.ToString(),
                    ["ps"] = ps.ToString()
                });
            var res = JsonConvert.DeserializeObject<BiliResult<UserVideoInfo>>(r);
            return res;
        }

        public async Task<Pbp> GetPbp(string cid)
        {
            var r = await GetAsync("https://bvc.bilivideo.com/pbp/data", ResponseEnum.Web,
                new Dictionary<string, string>
                {
                    ["cid"] = cid,
                });
            var res = JsonConvert.DeserializeObject<Pbp>(r);
            return res;
        }


        #region Cookie
        public static CookieCollection ParseCookies(IEnumerable<string> cookieHeaders)
        {
            if (cookieHeaders is null)
                return new CookieCollection();

            var cookies = new CookieCollection();
            foreach (string cookieHeader in cookieHeaders)
                ParseCookies(cookies, cookieHeader);
            return cookies;
        }

        public static CookieCollection ParseCookies(string cookieHeader)
        {
            if (string.IsNullOrEmpty(cookieHeader))
                return new CookieCollection();

            var cookies = new CookieCollection();
            ParseCookies(cookies, cookieHeader);
            return cookies;
        }

        private static void ParseCookies(CookieCollection cookies, string cookieHeader)
        {
            try
            {
                var cookie = new System.Net.Cookie();
                var CookieDic = new Dictionary<string, string>();
                var arr1 = cookieHeader.Split(';').ToList();
                var arr2 = arr1[0].Trim().Split('=');
                cookie.Name = arr2[0];
                cookie.Value = arr2[1];
                arr1.RemoveAt(0);
                foreach (string cookiediac in arr1)
                {
                    try
                    {
                        string[] cookiesetarr = cookiediac.Trim().Split('=');
                        switch (cookiesetarr[0].Trim().ToLower())
                        {
                            case "expires":
                                cookie.Expires = DateTime.Parse(cookiesetarr[1].Trim());
                                break;
                            case "max-age":
                                cookie.Expires = DateTime.Now.AddSeconds(int.Parse(cookiesetarr[1]));
                                break;
                            case "domain":
                                cookie.Domain = cookiesetarr[1].Trim();
                                break;
                            case "path":
                                cookie.Path = cookiesetarr[1].Trim().Replace("%x2F", "/");
                                break;
                            case "secure":
                                cookie.Secure = cookiesetarr[1].Trim().ToLower() == "true";
                                break;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
                cookies.Add(cookie);
            }
            catch (Exception)
            {

            }
        }

        private static void ParseUrlCookies(CookieCollection cookies, string url)
        {
            var segs = url.Split(new[] { '\u0026', '?', '=' });
            for (int i = 1; i < segs.Length - 2; i+=2)
            {
                var cookie = new System.Net.Cookie();
                if (segs[i].Contains("Expire"))
                {
                    continue;
                }
                cookie.Name = segs[i];
                cookie.Value = segs[i+1];
                cookies.Add(cookie);
            }
        }
        #endregion
    }
}
