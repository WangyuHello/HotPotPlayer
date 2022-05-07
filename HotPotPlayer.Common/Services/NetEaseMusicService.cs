using NeteaseCloudMusicApi;
using Newtonsoft.Json.Linq;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HotPotPlayer.Services
{
    public class NetEaseMusicService: ServiceBase
    {
        readonly CloudMusicApi api = new();
        long uid;
        string username;
        
        public async Task<JObject> LoginAsync(string phone, string password)
        {
            var queries = new Dictionary<string, object>
            {
                ["phone"] = phone,
                ["password"] = password
            };

            var re = await api.RequestAsync( CloudMusicApiProviders.LoginCellphone, queries);
            return re;
		}

        public async ValueTask<bool> IsLoginAsync()
        {
            var t = await GetUidAsync();
            return t.uid != 0;
        }

        public async Task<(long uid, string username)> GetUidAsync()
        {
            var json = await api.RequestAsync(CloudMusicApiProviders.LoginStatus);
            if (!json["profile"].HasValues)
            {
                return (0, null);
            }
            long uid = json["profile"]["userId"].Value<long>();
            var username = json["profile"]["nickname"].Value<string>();
            return (uid, username);
        }

        public async Task<string> GetQrKeyAsync()
        {
            var json = await api.RequestAsync(CloudMusicApiProviders.LoginQrKey);
            return json["unikey"].Value<string>();
        }

        public async ValueTask<(int code, string message)> GetQrCheckAsync(string key)
        {
            var queries = new Dictionary<string, object>
            {
                ["key"] = key,
            };
            var json = await api.RequestAsync(CloudMusicApiProviders.LoginQrCheck, queries);

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

        public async Task GetLikeListAsync()
        {
            var json = await api.RequestAsync(CloudMusicApiProviders.UserPlaylist, new Dictionary<string, object> { ["uid"] = uid });
            json = await api.RequestAsync(CloudMusicApiProviders.PlaylistDetail, new Dictionary<string, object> { ["id"] = json["playlist"][0]["id"] });
            int[] trackIds = json["playlist"]["trackIds"].Select(t => (int)t["id"]).ToArray();
            json = await api.RequestAsync(CloudMusicApiProviders.SongDetail, new Dictionary<string, object> { ["ids"] = trackIds });
            Console.WriteLine($"我喜欢的音乐（{trackIds.Length} 首）：");
            foreach (var song in json["songs"])
                Console.WriteLine($"{string.Join(",", song["ar"].Select(t => t["name"]))} - {song["name"]}");
            Console.WriteLine();
        }

        public async Task<JObject> LogoutAsync()
        {
            var json = await api.RequestAsync(CloudMusicApiProviders.Logout);
            return json;
        }
    }
}
