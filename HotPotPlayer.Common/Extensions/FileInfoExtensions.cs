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
            return null;
        }

        public static VideoItem ToVideoItem(this FileInfo f)
        {
            string title = "";
            TimeSpan dur = TimeSpan.Zero;
            try
            {
                using var tfile = TagLib.File.Create(f.FullName);
                var tTitle = tfile.Tag.Title;
                title = string.IsNullOrEmpty(tTitle) ? Path.GetFileNameWithoutExtension(f.Name) : tTitle;
                dur = tfile.Properties.Duration;
            }
            catch (Exception)
            {

            }

            return new VideoItem
            {
                Source = f,
                Title = title,
                Duration = dur,
                Cover = new Uri(f.FullName),
                LastWriteTime = f.LastWriteTime
            };
        }

        public static VideoItem ToVideoItemLight(this FileInfo f)
        {
            TimeSpan dur = TimeSpan.Zero;
            var title = Path.GetFileNameWithoutExtension(f.Name);

            return new VideoItem
            {
                Source = f,
                Title = title,
                Duration = dur,
                LastWriteTime = f.LastWriteTime
            };
        }
    }
}
