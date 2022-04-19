using HotPotPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Extensions
{
    public static class FileInfoExtensions
    {
        public static MusicItem ToMusicItem(this FileInfo f)
        {
            using var tfile = TagLib.File.Create(f.FullName);
            //var duration = await GetMusicDurationAsync(f);
            var item = new MusicItem
            {
                Source = f,
                Title = tfile.Tag.Title,
                Artists = tfile.Tag.Performers,
                Album = tfile.Tag.Album,
                Year = (int)tfile.Tag.Year,
                Cover = new Uri(f.FullName),
                //Duration = duration,
                Duration = tfile.Properties.Duration,
                Track = (int)tfile.Tag.Track,
                LastWriteTime = f.LastWriteTime,
                AlbumArtists = tfile.Tag.AlbumArtists,
                Disc = (int)tfile.Tag.Disc,
            };

            return item;
        }
    }
}
