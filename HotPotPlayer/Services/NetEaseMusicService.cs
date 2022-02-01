using NeteaseCloudMusicApi;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HotPotPlayer.Services
{
    internal class NetEaseMusicService
    {
        readonly CloudMusicApi api = new();
        long uid;
        
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

        public async Task<long> GetUidAsync()
        {
            var json = await api.RequestAsync(CloudMusicApiProviders.LoginStatus);
            long uid = (long)json["profile"]["userId"];
            return uid;
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
