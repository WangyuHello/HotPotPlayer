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

namespace HotPotPlayer.Services.BiliBili
{
    public class BiliAPI
    {
        readonly HttpClientFactory _httpClientFactory;

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
                    client.Timeout = TimeSpan.FromSeconds(8);
                }));

        }
        public enum ResponseEnum { App, Web }
        AccountToken token;
        public CookieCollection Cookies { get; set; } = new CookieCollection();

        private async Task<string> GetAsync(string url, ResponseEnum responseEnum, Dictionary<string, string> keyValues = null, bool IsAcess = true, string BuildString = "&platform=android&device=android&actionKey=appkey&build=5442100&mobi_app=android_comic")
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
                    appClient.DefaultRequestHeaders.Add("Cookie", string.Join("; ", Cookies.Cast<System.Net.Cookie>().Select(t => t.Name + "=" + t.Value)));
                    HttpResponseMessage apphr = await appClient.GetAsync(url).ConfigureAwait(false);
                    apphr.Headers.Add("Accept_Encoding", "gzip,deflate");
                    apphr.EnsureSuccessStatusCode();
                    var appencodeResults = await apphr.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    string appstr = Encoding.UTF8.GetString(appencodeResults, 0, appencodeResults.Length);
                    return appstr;
                case ResponseEnum.Web:
                    var webClient = _httpClientFactory.CreateClient("web");
                    var cookieStr = string.Join("; ", Cookies.Cast<System.Net.Cookie>().Select(t => t.Name + "=" + t.Value));
                    webClient.DefaultRequestHeaders.Add("Cookie", cookieStr);
                    webClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 Edg/107.0.1418.62");
                    //if (keyValues != null)
                    //{
                    //    foreach (var item in keyValues)
                    //    {
                    //        //webClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                    //    }
                    //}
                    string url2 = keyValues == null ? url : QueryHelpers.AddQueryString(url, keyValues);
                    HttpResponseMessage webhr = await webClient.GetAsync(url2).ConfigureAwait(false);
                    webhr.EnsureSuccessStatusCode();

                    //if (webhr.Headers.TryGetValues("Set-Cookie", out var rawSetCookie))
                    //{
                    //    Cookies.Add(ParseCookies(rawSetCookie));
                    //}

                    var webencodeResults = await webhr.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    string webstr = Encoding.UTF8.GetString(webencodeResults, 0, webencodeResults.Length);
                    return webstr;
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

        public async Task<JObject> GetLoginInfoAsync()
        {
            var r = await GetAsync("http://api.bilibili.com/x/web-interface/nav/stat", ResponseEnum.Web);
            return JObject.Parse(r);
        }

        public async Task<JObject> GetQrCodePoll(string key)
        {
            var r = await GetAsync("http://passport.bilibili.com/x/passport-login/web/qrcode/poll", ResponseEnum.Web, 
                new Dictionary<string, string> { ["qrcode_key"] = key});
            var j = JObject.Parse(r);

            if (j["data"]["code"].Value<int>() == 0)
            {
                var url = j["data"]["url"].Value<string>();
                ParseUrlCookies(Cookies, url);
            }

            return j;
        }


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
    }
}
