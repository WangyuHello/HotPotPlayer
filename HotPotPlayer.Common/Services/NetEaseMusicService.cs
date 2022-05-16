using HotPotPlayer.Extensions;
using HotPotPlayer.Models;
using HotPotPlayer.Models.CloudMusic;
using Microsoft.UI.Dispatching;
using NeteaseCloudMusicApi;
using Newtonsoft.Json.Linq;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HotPotPlayer.Services
{
    public class NetEaseMusicService: ServiceBaseWithConfig
    {
        LocalMusicService LocalMusic;
        public NetEaseMusicService(ConfigBase config, DispatcherQueue uiThread = null, AppBase app = null, LocalMusicService localMusic = null) : base(config, uiThread, app)
        {
            Config.SaveConfigWhenExit(() => _api.SaveCookie(Config));
            LocalMusic = localMusic;
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

        private string _nickName;
        public string NickName
        {
            get => _nickName;
            set => Set(ref _nickName, value);
        }

        private Uri _avatar;

        public Uri Avatar
        {
            get => _avatar;
            set => Set(ref _avatar, value);
        }

        private ObservableCollection<CloudMusicItem> _likeList;
        public ObservableCollection<CloudMusicItem> LikeList
        {
            get => _likeList;
            set => Set(ref _likeList, value);
        }

        private ObservableCollection<CloudMusicItem> _recommedList;
        public ObservableCollection<CloudMusicItem> RecommedList
        {
            get => _recommedList;
            set => Set(ref _recommedList, value);
        }

        private ObservableCollection<CloudPlayListItem> _recommedPlayList;
        public ObservableCollection<CloudPlayListItem> RecommedPlayList
        {
            get => _recommedPlayList;
            set => Set(ref _recommedPlayList, value);
        }

        private ObservableCollection<Toplist> _topList;
        public ObservableCollection<Toplist> TopList
        {
            get => _topList;
            set => Set(ref _topList, value);
        }

        private ObservableCollection<CloudArtistItem> _topArtists;
        public ObservableCollection<CloudArtistItem> TopArtists
        {
            get => _topArtists;
            set => Set(ref _topArtists, value);
        }

        public async Task InitAsync()
        {
            var likeList = await GetLikeListAsync();
            LikeList ??= new(likeList);
            
            var recList = await GetRecommendListAsync();
            RecommedList ??= new(recList);

            var recPlayList = await GetRecommendPlayListAsync();
            RecommedPlayList ??= new(recPlayList);

            var topL = await GetTopListDigestAsync();
            TopList ??= new(topL);

            var topA = await GetTopArtistsAsync();
            TopArtists ??= new(topA);
        }

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
            return t != 0;
        }

        public async Task<long> GetUidAsync()
        {
            var json = await Api.RequestAsync(CloudMusicApiProviders.LoginStatus);
            if (!json["profile"].HasValues)
            {
                return 0;
            }
            uid = json["profile"]["userId"].Value<long>();
            NickName = json["profile"]["nickname"].Value<string>();
            Avatar = new Uri(json["profile"]["avatarUrl"].Value<string>());
            return uid;
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

        async Task<Uri> GetSongSourceAsync(string id, string name, string artist)
        {
            if (LocalMusic != null)
            {
                var local = await LocalMusic.QueryMusicAsync(name, true);
                var l = local.FirstOrDefault();
                if (l!=null && l.GetArtists()==artist)
                {
                    return new Uri(l.Source.FullName);
                }
            }
            return new Uri(await GetSongUrlAsync(id));
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
                i.GetSource = () => GetSongSourceAsync(i.SId, i.OriginalTitle, i.GetArtists()).Result;
                return i;
            }).ToList();
        }

        public async Task<List<CloudMusicItem>> GetRecommendListAsync()
        {
            return await Task.Run(async () =>
            {
                var json = await Api.RequestAsync(CloudMusicApiProviders.RecommendSongs, new Dictionary<string, object> { ["uid"] = uid });
                return json["data"]["dailySongs"].ToArray().Select(s => {
                    var i = s.ToMusicItem();
                    i.GetSource = () => GetSongSourceAsync(i.SId, i.OriginalTitle, i.GetArtists()).Result;
                    return i;
                }).ToList();
            });
        }

        public async Task<List<CloudPlayListItem>> GetRecommendPlayListAsync()
        {
            return await Task.Run(async () =>
            {
                var json = await Api.RequestAsync(CloudMusicApiProviders.RecommendResource, new Dictionary<string, object> { ["uid"] = uid });
                var playList = json["recommend"].ToArray().Select(r => r.ToPlayListItem()).ToList();
                return playList;
            });
        }

        public async Task<(CloudAlbumItem album, List<CloudMusicItem> musicList)> GetAlbumAsync(string albumId)
        {
            return await Task.Run(async () =>
            {
                var json = await Api.RequestAsync(CloudMusicApiProviders.Album, new Dictionary<string, object> { ["id"] = albumId });
                var album = json["album"].ToAlbum();
                var songs = json["songs"].ToArray().Select(s => {
                    var i = s.ToMusicItem();
                    i.GetSource = () => GetSongSourceAsync(i.SId, i.OriginalTitle, i.GetArtists()).Result;
                    return i;
                }).ToList();
                return (album, songs);
            });
        }

        public async Task<CloudPlayListItem> GetPlayListAsync(string playListId)
        {
            return await Task.Run(async () =>
            {
                var json = await Api.RequestAsync(CloudMusicApiProviders.PlaylistDetail, new Dictionary<string, object> { ["id"] = playListId });
                var playList = json["playlist"].ToPlayListItem();
                foreach (var item in playList.MusicItems)
                {
                    var c = item as CloudMusicItem;
                    c.GetSource = () => GetSongSourceAsync(c.SId, c.OriginalTitle, c.GetArtists()).Result;
                }
                return playList;
            });
        }

        public async Task<List<Toplist>> GetTopListDigestAsync()
        {
            return await Task.Run(async () =>
            {
                var json = await Api.RequestAsync(CloudMusicApiProviders.ToplistDetail);
                var l = json["list"].ToArray().Select(s => {
                    var i = s.ToToplist();
                    return i;
                }).Take(4).ToList();
                return l;
            });
        }

        public async Task<List<CloudArtistItem>> GetTopArtistsAsync()
        {
            return await Task.Run(async () =>
            {
                var json = await Api.RequestAsync(CloudMusicApiProviders.TopArtists, new Dictionary<string, object> { ["limit"] = 10 });
                var l = json["artists"].ToArray().Select(s => {
                    var i = s.ToArtist();
                    return i;
                }).ToList();
                return l;
            });
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

        public async Task<List<CloudCommentItem>> GetSongCommentAsync(string id)
        {
            return await Task.Run(async () =>
            {
                var json = await Api.RequestAsync(CloudMusicApiProviders.CommentMusic, new Dictionary<string, object> { ["id"] = id });
                var l = json["hotComments"].ToArray().Select(s => {
                    var i = s.ToComment();
                    return i;
                }).ToList();
                return l;
            });
        }

        public async Task<List<CloudMusicItem>> GetSimilarSongAsync(string id)
        {
            return await Task.Run(async () =>
            {
                var json = await Api.RequestAsync(CloudMusicApiProviders.SimiSong, new Dictionary<string, object> { ["id"] = id });
                var songs = json["songs"].ToArray().Select(s =>
                {
                    var i = s.ToMusicItem();
                    i.GetSource = () => GetSongSourceAsync(i.SId, i.OriginalTitle, i.GetArtists()).Result;
                    return i;
                }).ToList();
                return songs;
            });
        }

        public async Task GetSimilarUserAsync(string id)
        {
            var json = await Api.RequestAsync(CloudMusicApiProviders.SimiUser, new Dictionary<string, object> { ["id"] = id });
        }

        public async Task<string> GetLyric(string id)
        {
            return await Task.Run(async () =>
            {
                var json = await Api.RequestAsync(CloudMusicApiProviders.Lyric, new Dictionary<string, object> { ["id"] = id });

                return json["lrc"]["lyric"].Value<string>();
            });
        }
        public bool GetSongLiked(CloudMusicItem c)
        {
            if (LikeList.Contains(c, new CloudMusicItemComparer()))
            {
                return true;
            }
            return false;
        }
    }
}
