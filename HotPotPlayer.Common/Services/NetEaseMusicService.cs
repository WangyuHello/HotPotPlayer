using HotPotPlayer.Extensions;
using HotPotPlayer.Models.CloudMusic;
using Microsoft.UI.Dispatching;
using NeteaseCloudMusicApi;
using Newtonsoft.Json.Linq;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HotPotPlayer.Services
{
    public class NetEaseMusicService: ServiceBaseWithConfig
    {
        public NetEaseMusicService(ConfigBase config, DispatcherQueue uiThread = null, AppBase app = null) : base(config, uiThread, app)
        {
            Config.SaveConfigWhenExit(() => _api.SaveCookie(Config));
        }

        CloudMusicApi _api;

        CloudMusicApi Api
        {
            get
            {
                if (_api == null)
                {
                    _api = new CloudMusicApi();
                    _api.LoadCookie(Config);
                }
                return _api;
            }
        }

        long uid;
        string username;


        public async Task<JObject> LoginAsync(string phone, string password)
        {
            var queries = new Dictionary<string, object>
            {
                ["phone"] = phone,
                ["password"] = password
            };

            var re = await Api.RequestAsync( CloudMusicApiProviders.LoginCellphone, queries);
            return re;
		}

        public async ValueTask<bool> IsLoginAsync()
        {
            var t = await GetUidAsync();
            return t.uid != 0;
        }

        public async Task<(long uid, string username)> GetUidAsync()
        {
            var json = await Api.RequestAsync(CloudMusicApiProviders.LoginStatus);
            if (!json["profile"].HasValues)
            {
                return (0, null);
            }
            uid = json["profile"]["userId"].Value<long>();
            username = json["profile"]["nickname"].Value<string>();
            return (uid, username);
        }

        public async Task<string> GetQrKeyAsync()
        {
            var json = await Api.RequestAsync(CloudMusicApiProviders.LoginQrKey);
            return json["unikey"].Value<string>();
        }

        public async ValueTask<(int code, string message)> GetQrCheckAsync(string key)
        {
            var queries = new Dictionary<string, object>
            {
                ["key"] = key,
            };
            var json = await Api.RequestAsync(CloudMusicApiProviders.LoginQrCheck, queries);

            return (json["code"].Value<int>(), json["message"].Value<string>());
        }

        public byte[] GetQrImgByte(string key)
        {
            var url = $"https://music.163.com/login?codekey={key}";
            var qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
            byte[] qrCodeAsBitmapByteArr = qrCode.GetGraphic(20);
            return qrCodeAsBitmapByteArr;
        }

        public async Task<List<CloudMusicItem>> GetLikeListAsync()
        {
            var json = await Api.RequestAsync(CloudMusicApiProviders.UserPlaylist, new Dictionary<string, object> { ["uid"] = uid });
            json = await Api.RequestAsync(CloudMusicApiProviders.PlaylistDetail, new Dictionary<string, object> { ["id"] = json["playlist"][0]["id"] });
            int[] trackIds = json["playlist"]["trackIds"].Select(t => (int)t["id"]).ToArray();
            json = await Api.RequestAsync(CloudMusicApiProviders.SongDetail, new Dictionary<string, object> { ["ids"] = string.Join(",", trackIds) });
            return json["songs"].ToArray().Select(s => {
                var i = s.ToMusicItem();
                i.GetSource = () => GetSongUrlAsync(i.SId).Result;
                return i;
            }).ToList();
        }

        public async Task<string> GetSongUrlAsync(string id)
        {
            var json = await Api.RequestAsync(CloudMusicApiProviders.SongUrl, new Dictionary<string, object> { ["id"] = id });
            var url = json["data"][0]["url"].Value<string>();
            return url;
        }

        public async Task<JObject> LogoutAsync()
        {
            var json = await Api.RequestAsync(CloudMusicApiProviders.Logout);
            return json;
        }
    }
}
