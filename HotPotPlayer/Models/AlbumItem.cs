using HotPotPlayer.Extensions;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using MongoDB.Bson;
using Realms;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Color = Windows.UI.Color;

namespace HotPotPlayer.Models
{
    public record AlbumItem
    {
        public string Title { get; set; }
        public string[] Artists { get; set; }
        public uint Year { get; set; }
        public string Cover { get; set; }
        public List<MusicItem> MusicItems { get; set; }
        public Color MainColor { get; set; }

        public bool IsPlayList { get; set; }

        public string GetArtists()
        {
            return string.Join(", ", Artists);
        }

        public AlbumItemDb ToDb()
        {
            var db = new AlbumItemDb
            {
                Title = Title,
                Artists = string.Join(',', Artists),
                Year = (int)Year,
                Cover = Cover,
                IsPlayList = IsPlayList,
                MainColor = MainColor.ToInt(),
            };
            foreach (var item in MusicItems)
            {
                db.MusicItems.Add(item.ToDb());
            }
            return db;
        }

        public void SetPlayListCover()
        {
            Cover = MusicItems.First().Cover;
        }
    }

    public class AlbumItemDb: RealmObject
    {
        [PrimaryKey]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        public string Title { get; set; }
        public string Artists { get; set; }
        public int Year { get; set; }
        public string Cover { get; set; }
        public IList<MusicItemDb> MusicItems { get; }
        public bool IsPlayList { get; set; }
        public int MainColor { get; set; }

        public AlbumItem ToOrigin()
        {
            return new AlbumItem
            {
                Title = Title,
                Artists = Artists.Split(','),
                Year = (uint)Year,
                Cover = Cover,
                MusicItems = MusicItems.Select(i => i.ToOrigin()).ToList(),
                IsPlayList = IsPlayList,
                MainColor = MainColor.ToColor(),
            };
        }
    }
}
