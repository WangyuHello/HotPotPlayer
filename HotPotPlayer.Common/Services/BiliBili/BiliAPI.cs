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

namespace HotPotPlayer.Services.BiliBili
{
    public class BiliAPI
    {
        readonly HttpClientFactory _httpClientFactory;
        public HttpClientFactory HttpClientFactory => _httpClientFactory;

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
                    client.DefaultRequestHeaders.Referrer = new Uri("http://www.bilibili.com/");
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
                    client.DefaultRequestHeaders.Referrer = new Uri("http://www.bilibili.com/");
                    client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                    client.Timeout = TimeSpan.FromSeconds(8);
                }));

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



        /// <summary>
        /// https://github.com/SocialSisterYi/bilibili-API-collect/blob/master/login/login_action/QR.md
        /// </summary>
        /// <returns></returns>
        public async Task<(string token, string url)> RequestQrCode()
        {
            var r = await GetAsync("http://passport.bilibili.com/x/passport-login/web/qrcode/generate", ResponseEnum.Web);
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
            var r = await GetAsync("http://api.bilibili.com/x/web-interface/nav/stat", ResponseEnum.Web);
            return JObject.Parse(r);
        }

        /// <summary>
        /// https://github.com/SocialSisterYi/bilibili-API-collect/blob/master/login/login_action/QR.md
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<JObject> GetQrCodePoll(string key)
        {
            var r = await GetAsync("http://passport.bilibili.com/x/passport-login/web/qrcode/poll", ResponseEnum.Web, 
                new Dictionary<string, string> { ["qrcode_key"] = key});
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
        public async Task<BiliResult<VideoInfo>> GetVideoUrl(string bvid, string cid, DashEnum qn = DashEnum.Dash480, FnvalEnum fnval = FnvalEnum.Dash, int fourk = 0)
        {
            var r = await GetAsync("http://api.bilibili.com/x/player/playurl", ResponseEnum.Web,
                new Dictionary<string, string> {
                    ["bvid"] = bvid,
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
            var r = await GetAsync("http://api.bilibili.com/x/web-interface/view", ResponseEnum.Web,
                new Dictionary<string, string>
                {
                    ["bvid"] = bvid,
                });
            var res = JsonConvert.DeserializeObject<BiliResult<VideoContent>>(r);
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

        public async Task<BiliResult<HomeData>> GetRecVideo()
        {
            var r = await GetAsync("https://api.bilibili.com/x/web-interface/index/top/feed/rcmd", ResponseEnum.Web,
                new Dictionary<string, string>
                {
                    ["fresh_idx"] = "1",
                    ["feed_version"] = "V1",
                    ["fresh_type"] = "4",
                    ["ps"] = "20",
                    ["plat"] = "1"
                });
            var res = JsonConvert.DeserializeObject<BiliResult<HomeData>>(r);
            return res;
        }

        public async Task<string> GetOnlineCount(string bvid, string cid)
        {
            var r = await GetAsync("http://api.bilibili.com/x/player/online/total", ResponseEnum.Web,
                new Dictionary<string, string>
                {
                    ["bvid"] = bvid,
                    ["cid"] = cid
                });
            var res = JObject.Parse(r);
            return res["data"]["total"].Value<string>();
        }

        public async Task GetVideoReplyAsync(string avid)
        {
            var r = await GetAsync("http://api.bilibili.com/x/v2/reply", ResponseEnum.Web,
                new Dictionary<string, string>
                {
                    ["type"] = "1",
                    ["oid"] = avid,
                    ["sort"] = "1",
                    ["nohot"] = "0",
                    ["ps"] = "20",
                    ["pn"] = "1"
                });
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
