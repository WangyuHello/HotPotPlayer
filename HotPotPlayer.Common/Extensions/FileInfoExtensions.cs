using HotPotPlayer.Models;
using HotPotPlayer.Services.FFmpeg;
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
            using var tfile = TagLib.File.Create(f.FullName, TagLib.ReadStyle.PictureLazy);
            var (duration, bitrate, sampleRate, bitDepth) = MediaInfoHelper.GetAudioAdditionalInfo(f);
            var item = new MusicItem
            {
                Source = f,
                Title = tfile.Tag.Title,
                Artists = tfile.Tag.Performers,
                Album = tfile.Tag.Album,
                Year = (int)tfile.Tag.Year,
                Cover = new Uri(f.FullName),
                Duration = duration,
                Genre = tfile.Tag.JoinedGenres,
                BitRate = bitrate,
                SampleRate = sampleRate,
                BitDepth = bitDepth,
                Track = (int)tfile.Tag.Track,
                LastWriteTime = f.LastWriteTime,
                AlbumArtists = tfile.Tag.AlbumArtists,
                Disc = (int)tfile.Tag.Disc,
            };

            return item;
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
    }
}
