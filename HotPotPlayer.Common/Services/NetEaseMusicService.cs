﻿using CommunityToolkit.Mvvm.ComponentModel;
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
    public partial class NetEaseMusicService: ServiceBaseWithConfig
    {
        JellyfinMusicService LocalMusic;
        public NetEaseMusicService(ConfigBase config, DispatcherQueue uiThread = null, AppBase app = null, JellyfinMusicService localMusic = null) : base(config, uiThread, app)
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

        [ObservableProperty]
        public partial CloudUserItem Self { get; set; }

        [ObservableProperty]
        public partial LevelItem Level { get; set; }

        [ObservableProperty]
        public partial CloudPlayListItem LikeList { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<CloudPlayListItem> UserPlayLists { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<CloudPlayListItem> SubscribePlayLists { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<CloudMusicItem> RecommedList { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<CloudPlayListItem> RecommedPlayList { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<Toplist> TopList { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<CloudArtistItem> TopArtists { get; set; }

        public async Task InitAsync()
        {
            //https://stackoverflow.com/questions/17197699/awaiting-multiple-tasks-with-different-results
            var recList = await GetRecommendListAsync();
            var recPlayList = await GetRecommendPlayListAsync();
            var topL = await GetTopListDigestAsync();
            var topA = await GetTopArtistsAsync();

            RecommedList ??= new(recList);
            RecommedPlayList ??= new(recPlayList);
            TopList ??= new(topL);
            TopArtists ??= new(topA);

            await InitUserAsync();
        }

        public async Task InitLevelAsync()
        {
            Self = await GetUserDetail();
            Level = await GetUserLevelAsync();
        }

        public async Task InitUserAsync()
        {
            var (likeList, lists) = await GetLikeListAsync();
            LikeList = likeList;
            UserPlayLists ??= new(lists.Where(l => !l.Subscribed));
            SubscribePlayLists ??= new(lists.Where(l => l.Subscribed));
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
            var t = await GetLoginStatusAsync();
            return t != 0;
        }

        public async Task<long> GetLoginStatusAsync()
        {
            var json = await Api.RequestAsync(CloudMusicApiProviders.LoginStatus);
            if (!json["profile"].HasValues)
            {
                return 0;
            }
            Self = json["profile"].ToUser();
            return Self.UserId;
        }

        public async Task<CloudUserItem> GetUserDetail(long? uid = null)
        {
            uid ??= Self.UserId;
            var json = await Api.RequestAsync(CloudMusicApiProviders.UserDetail, new Dictionary<string, object> { ["uid"] = uid });
            return json["profile"].ToUser();
        }

        public async Task<LevelItem> GetUserLevelAsync()
        {
            var json = await Api.RequestAsync(CloudMusicApiProviders.UserLevel);
            return json["data"].ToLevel();
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

        async Task<Uri> GetSongSourceAsync(string id, string name, string[] artist)
        {
            if (LocalMusic != null)
            {
                var local = await LocalMusic.QueryMusicAsync(name, true);
                var l = local.Where(l => l.Artists.Select(a => a.GetArtists()).SelectMany(s => s).Intersect(artist).Any()).FirstOrDefault();
                if (l!=null)
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

        bool NotifiyWhenFail(JObject j)
        {
            if (j["code"] != null && j["code"].Value<int>() == 502)
            {
                var msg = j["msg"].Value<string>();
                UIQueue?.TryEnqueue(() =>
                {
                    App?.ShowToast(new ToastInfo { Text = msg });
                });
                return false;
            }
            return true;
        }

        public async Task<(CloudPlayListItem like, List<CloudPlayListItem> lists)> GetLikeListAsync()
        {
            return await Task.Run(async () =>
            {
                var json = await Api.RequestAsync(CloudMusicApiProviders.UserPlaylist, new Dictionary<string, object> { ["uid"] = Self.UserId });
                var playlists = json["playlist"].ToArray().Select(s => s.ToPlayListItem()).ToList();
                var like = await GetPlayListAsync(playlists[0].PlId);
                return (like, playlists.Skip(1).ToList());
            });
        }

        public async Task<List<CloudMusicItem>> GetRecommendListAsync()
        {
            return await Task.Run(async () =>
            {
                var json = await Api.RequestAsync(CloudMusicApiProviders.RecommendSongs, new Dictionary<string, object> { ["uid"] = Self.UserId });
                return json["data"]["dailySongs"].ToArray().Select(s => {
                    var i = s.ToMusicItem();
                    i.GetSource = () => GetSongSourceAsync(i.SId, i.OriginalTitle, i.Artists).Result;
                    return i;
                }).ToList();
            });
        }

        public async Task<List<CloudPlayListItem>> GetRecommendPlayListAsync()
        {
            return await Task.Run(async () =>
            {
                var json = await Api.RequestAsync(CloudMusicApiProviders.RecommendResource, new Dictionary<string, object> { ["uid"] = Self.UserId });
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
                    i.GetSource = () => GetSongSourceAsync(i.SId, i.OriginalTitle, i.Artists).Result;
                    return i;
                }).ToList();
                return (album, songs);
            });
        }

        public async Task<CloudAlbumItem> GetAlbumAsync2(string albumId)
        {
            return await Task.Run(async () =>
            {
                var json = await Api.RequestAsync(CloudMusicApiProviders.Album, new Dictionary<string, object> { ["id"] = albumId });
                var album = json["album"].ToAlbum();
                var songs = json["songs"].ToArray().Select(s => {
                    var i = s.ToMusicItem();
                    i.GetSource = () => GetSongSourceAsync(i.SId, i.OriginalTitle, i.Artists).Result;
                    return i;
                });
                album.MusicItems = new List<MusicItem>(songs);
                return album;
            });
        }

        public async Task<CloudPlayListItem> GetPlayListAsync(string playListId)
        {
            return await Task.Run(async () =>
            {
                var json = await Api.RequestAsync(CloudMusicApiProviders.PlaylistDetail, new Dictionary<string, object> { ["id"] = playListId });
                if(!NotifiyWhenFail(json)) return null;
                var playList = json["playlist"].ToPlayListItem(true);
                var json2 = await Api.RequestAsync(CloudMusicApiProviders.SongDetail, new Dictionary<string, object> { ["ids"] = string.Join(",", playList.TrackIds.Take(500)) });
                if(!NotifiyWhenFail(json2)) return null;
                playList.MusicItems = new ObservableCollection<MusicItem>(json2["songs"].ToArray().Select(s =>
                {
                    var c = s.ToMusicItem();
                    c.GetSource = () => GetSongSourceAsync(c.SId, c.OriginalTitle, c.Artists).Result;
                    return c;
                }));
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

        public async Task<bool> LogoutAsync()
        {
            var json = await Api.RequestAsync(CloudMusicApiProviders.Logout);
            var code = json["code"].Value<int>();
            if (code == 200)
            {
                Api.ClearCookie();
                Api.ClearCache();
                return true;
            }
            return false;
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

        public async Task<List<CloudCommentItem>> GetSongCommentFloorAsync(string songid, long id)
        {
            return await Task.Run(async () =>
            {
                var json = await Api.RequestAsync(CloudMusicApiProviders.CommentFloor, new Dictionary<string, object> { ["parentCommentId"] = id, ["type"] = 0, ["id"] = songid });
                var l = json["data"]["comments"].ToArray().Select(s => {
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
                    i.GetSource = () => GetSongSourceAsync(i.SId, i.OriginalTitle, i.Artists).Result;
                    return i;
                }).ToList();
                return songs;
            });
        }

        public void GetSimilarPlayList(string id)
        {
            Task.Run(async () =>
            {
                var json = await Api.RequestAsync(CloudMusicApiProviders.SimiPlayList, new Dictionary<string, object> { ["id"] = id });

            });
        }

        public async Task GetSimilarUserAsync(string id)
        {
            var json = await Api.RequestAsync(CloudMusicApiProviders.SimiUser, new Dictionary<string, object> { ["id"] = id });
        }

        public async Task<List<LyricItem>> GetLyric(string id)
        {
            return await Task.Run(async () =>
            {
                var json = await Api.RequestAsync(CloudMusicApiProviders.Lyric, new Dictionary<string, object> { ["id"] = id });

                var lyRaw = json["lrc"]["lyric"].Value<string>();
                var ly = lyRaw.ParseLyric();
                if (json["tlyric"] != null)
                {
                    var transRaw = json["tlyric"]["lyric"].Value<string>();
                    var trans = transRaw.ParseLyric();
                    MergeLyric(ly, trans);
                }
                return ly;
            });
        }

        static void MergeLyric(List<LyricItem> l, List<LyricItem> tr)
        {
            int j = 0;
            for (int i = 0; i < tr.Count; i++)
            {
                while (j <= l.Count - 1)
                {
                    if (l[j].Time == tr[i].Time)
                    {
                        l[j] = new LyricItem
                        {
                            Time = l[j].Time,
                            Content = l[j].Content,
                            Translate = tr[i].Content,
                        };
                        j++;
                        break;
                    }
                    j++;
                }
            }
        }

        public bool GetSongLiked(CloudMusicItem c)
        {
            if (LikeList == null)
            {
                return false;
            }
            if (LikeList.MusicItems.Contains(c, new CloudMusicItemComparer()))
            {
                return true;
            }
            return false;
        }

        public async Task<List<CloudMusicItem>> GetArtistSongsAsync(string id, string order = "hot")
        {
            var json = await Api.RequestAsync(CloudMusicApiProviders.ArtistSongs, new Dictionary<string, object> { ["id"] = id });
            int[] trackIds = json["songs"].Select(t => (int)t["id"]).ToArray();
            json = await Api.RequestAsync(CloudMusicApiProviders.SongDetail, new Dictionary<string, object> { ["ids"] = string.Join(",", trackIds) });
            var songs = json["songs"].ToArray().Select(s =>
            {
                var i = s.ToMusicItem();
                i.GetSource = () => GetSongSourceAsync(i.SId, i.OriginalTitle, i.Artists).Result;
                return i;
            }).ToList();
            return songs;
        }

        public async Task<List<CloudAlbumItem>> GetArtistAlbumsAsync(string id)
        {
            var json = await Api.RequestAsync(CloudMusicApiProviders.ArtistAlbum, new Dictionary<string, object> { ["id"] = id });
            var albums = json["hotAlbums"].ToArray().Select(s =>
            {
                var i = s.ToAlbum();
                return i;
            }).ToList();
            return albums;
        }

        public async Task<CloudArtistItem> GetArtistDetailAsync(string id)
        {
            var json = await Api.RequestAsync(CloudMusicApiProviders.ArtistDetail, new Dictionary<string, object> { ["id"] = id });
            var ar = json["data"]["artist"].ToArtist();
            return ar;
        }

        public async void AddMusicToPlayList(CloudPlayListItem pl, CloudMusicItem c)
        {
            var json = await Api.RequestAsync(CloudMusicApiProviders.PlaylistTracks, new Dictionary<string, object> { ["op"] = "add", ["pid"] = pl.PlId, ["tracks"] = c.SId });
            NotifiyWhenFail(json);
            if (json["code"].Value<int>() == 200)
            {
                App.ShowToast(new ToastInfo { Text = $"已将 {c.Title} 添加到 {pl.Title}" });
            }
        }

        public async void Like(CloudMusicItem c, bool like = true)
        {
            var json = await Api.RequestAsync(CloudMusicApiProviders.Like, new Dictionary<string, object> { ["id"] = c.SId, ["like"] = like });
            NotifiyWhenFail(json);
            var (likeList, lists) = await GetLikeListAsync();
            LikeList = likeList;
            UserPlayLists = new(lists.Where(l => !l.Subscribed));
            SubscribePlayLists = new(lists.Where(l => l.Subscribed));
            if (json["code"].Value<int>() == 200)
            {
                var likeStr = like ? $"已喜欢 {c.Title}" : $"已取消喜欢 {c.Title}";
                App.ShowToast(new ToastInfo { Text = likeStr });
            }
        }

        public async Task<List<CloudMusicItem>> SearchSongAsync(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return null;
            }
            var json = await Api.RequestAsync(CloudMusicApiProviders.Cloudsearch, new Dictionary<string, object> { ["keywords"] = keyword });
            if (json["result"]["songs"] != null)
            {
                var songs = json["result"]["songs"].ToArray().Select(s =>
                {
                    var i = s.ToMusicItem();
                    i.GetSource = () => GetSongSourceAsync(i.SId, i.OriginalTitle, i.Artists).Result;
                    return i;
                }).ToList();
                return songs;
            }
            return null;
        }

        public async Task<List<CloudSearchHotItem>> SearchHotDetailAsync()
        {
            var json = await Api.RequestAsync(CloudMusicApiProviders.SearchHotDetail);
            var hots = json["data"].ToArray().Select(s => s.ToSearchHotItem()).ToList();
            return hots;
        }
    }
}
