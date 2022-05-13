using HotPotPlayer.Models.CloudMusic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Extensions
{
    public static class CloudMusicExtension
    {
        //https://github.com/HyPlayer/HyPlayer/blob/491b0b2ae5feaaf5632986f5dbe76b02a0313bbb/HyPlayer/Classes/NCSong.cs#L170
        public static CloudMusicItem ToMusicItem(this JToken song)
        {
            if (song == null) return null;
            var alpath = "album";
            var arpath = "artists";
            var dtpath = "duration";
            if (song[alpath] == null)
                alpath = "al";
            if (song[arpath] == null)
                arpath = "ar";
            if (song[dtpath] == null)
                dtpath = "dt";
            var NCSong = new CloudMusicItem
            {
                Album2 = song[alpath].ToAlbum(),
                SId = song["id"].ToString(),
                Title = song["name"].ToString(),
                Duration = TimeSpan.FromMilliseconds(double.Parse(song[dtpath].ToString())),
                Artists2 = new List<CloudArtistItem>(),
                Track = song["no"].Value<int>()
            };
            if (song[arpath].HasValues)
                song[arpath].ToList().ForEach(t => { NCSong.Artists2.Add(t.ToArtist()); });
            else
                NCSong.Artists2.Add(new CloudArtistItem());
            if (song["alia"] != null)
                NCSong.Alias = string.Join(" / ", song["alia"].ToArray().Select(t => t.ToString()));
            if (song["tns"] != null)
                NCSong.TransName = string.Join(" / ", song["tns"].ToArray().Select(t => t.ToString()));
            if (song["mv"] != null) NCSong.MvId = song["mv"].ToObject<int>();
            return NCSong;
        }

        public static CloudPlayListItem ToPlayListItem(this JToken json)
        {
            var picpath = "picUrl";
            var descpath = "description";
            var subcountpath = "subscribedCount";
            var playcountpath = "playCount";
            if (json[picpath] == null)
                picpath = "coverImgUrl";
            if (json[descpath] == null)
                descpath = "copywriter";
            if (json[subcountpath] == null)
                subcountpath = "bookCount";
            if (json[playcountpath] == null) playcountpath = "playcount";

            var ncp = new CloudPlayListItem
            {
                Cover2 = new Uri(json[picpath].ToString()),
                Description = json[descpath].ToString(),
                Title = json["name"].ToString(),
                PlId = json["id"].ToString(),
                PlayCount = json[playcountpath].Value<long>(),
                Subscribed = !(json["subscribed"] == null || json["subscribed"].ToString() == "False"),
                TrackCount = json["trackCount"].Value<long>()
            };

            if (json[subcountpath] != null) ncp.BookCount = json[subcountpath].Value<long>();
            if (json["tracks"] != null) // 只有前20首
            {
                ncp.MusicItems = new(json["tracks"].ToArray().Select(t => t.ToMusicItem()));
            }

            return ncp;
        }

        public static CloudAlbumItem ToAlbum(this JToken album)
        {
            if (!album.HasValues) return new CloudAlbumItem();
            var arpath = "artists";
            if (album[arpath] == null)
                arpath = "ar";
            var al = new CloudAlbumItem
            {
                Alias = album["alias"] != null
                ? string.Join(" / ", album["alias"].ToArray().Select(t => t.ToString()))
                : "",
                Cover = new Uri(album["picUrl"].ToString()),
                Description = album["description"] != null ? album["description"].ToString() : "",
                Id = album["id"].ToString(),
                Title = album["name"].ToString(),
                Artists2 = new List<CloudArtistItem>()
            };
            if (album["artist"] != null)
            {
                al.AlbumArtist = album["artist"].ToArtist();
            }
            if (album[arpath] != null)
                album[arpath].ToList().ForEach(t => { al.Artists2.Add(t.ToArtist()); });

            return al;
        }

        public static CloudArtistItem ToArtist(this JToken artist)
        {
            var art = new CloudArtistItem
            {
                Id = artist["id"].ToString(),
                Name = artist["name"].ToString()
            };
            if (artist["alias"] != null)
                art.Alias = string.Join(" / ", artist["alias"].Select(t => t.ToString()).ToArray());
            if (artist["trans"] != null) art.TransName = artist["trans"].ToString();
            if (artist["picUrl"] != null) art.Avatar = artist["picUrl"].ToString();
            return art;
        }

        public static Toplist ToToplist(this JToken j)
        {
            var t = JsonConvert.DeserializeObject<Toplist>(j.ToString());
            return t;
        }
    }
}
