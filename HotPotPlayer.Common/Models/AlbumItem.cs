using HotPotPlayer.Extensions;
using HotPotPlayer.Helpers;
using Microsoft.UI.Xaml.Media.Imaging;
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
        public virtual string[] Artists { get; set; }
        public int Year { get; set; }
        public Uri Cover { get; set; }
        public Color MainColor { get; set; }
        public List<MusicItem> MusicItems { get; set; }
        public virtual string[] AllArtists { get; set; }

        public string GetArtists()
        {
            if (Artists == null)
            {
                return string.Empty;
            }
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
}
