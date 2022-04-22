using HotPotPlayer.Extensions;
using HotPotPlayer.Helpers;
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

namespace HotPotPlayer.Models
{
    public record AlbumItem
    {
        public string Title { get; set; }
        public string[] Artists { get; set; }
        public int Year { get; set; }
        public Uri Cover { get; set; }
        public Color MainColor { get; set; }
        public List<MusicItem> MusicItems { get; set; }
        public string[] AllArtists { get; set; }

        public string GetArtists()
        {
            var t = string.Join(", ", Artists);
            if (string.IsNullOrEmpty(t))
            {
                t = AllArtists.FirstOrDefault();
            }
            return t;
        }

        public async Task<BitmapImage> GetCoverAsync()
        {
            return await ImageCacheEx.Instance.GetFromCacheAsync(Cover).ConfigureAwait(false);
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
