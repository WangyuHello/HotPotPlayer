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

            token = new AccountToken();
        }
        public enum ResponseEnum { App, Web }
        AccountToken token;

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
                    appClient.DefaultRequestHeaders.Add("Cookie", token.CookieString);
                    HttpResponseMessage apphr = await appClient.GetAsync(url).ConfigureAwait(false);
                    apphr.Headers.Add("Accept_Encoding", "gzip,deflate");
                    apphr.EnsureSuccessStatusCode();
                    var appencodeResults = await apphr.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    string appstr = Encoding.UTF8.GetString(appencodeResults, 0, appencodeResults.Length);
                    return appstr;
                case ResponseEnum.Web:
                    var webClient = _httpClientFactory.CreateClient("web");
                    webClient.DefaultRequestHeaders.Add("Cookie", token.CookieString);
                    webClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 Edg/107.0.1418.62");
                    if (keyValues != null)
                    {
                        foreach (var item in keyValues)
                        {
                            webClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                        }
                    }
                    HttpResponseMessage webhr = await webClient.GetAsync(url).ConfigureAwait(false);
                    webhr.EnsureSuccessStatusCode();
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
        public async Task<(string token, string url)> 申请二维码()
        {
            var r = await GetAsync("http://passport.bilibili.com/x/passport-login/web/qrcode/generate", ResponseEnum.Web);
            var j = JObject.Parse(r);
            var url = j["data"]["url"].Value<string>();
            var token = j["data"]["qrcode_key"].Value<string>();
            return (token, url);
        }

        public static byte[] GetQrImgByte(string url)
        {
            var qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            BitmapByteQRCode qrCode = new(qrCodeData);
            byte[] qrCodeAsBitmapByteArr = qrCode.GetGraphic(20);
            return qrCodeAsBitmapByteArr;
        }
    }
}
