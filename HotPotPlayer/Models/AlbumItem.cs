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
        public int Year { get; set; }
        public string Cover { get; set; }
        public Color MainColor { get; set; }
        public List<MusicItem> MusicItems { get; set; }
        public string[] AllArtists { get; set; }

        public string GetArtists()
        {
            return string.Join(", ", Artists);
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
        public int MainColor { get; set; }
        public IList<MusicItemDb> MusicItems { get; }
        public string AllArtists { get; set; }
    }
}
