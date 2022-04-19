using HotPotPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Extensions
{
    internal static class ModelExtensions
    {
        public static MusicItemDb ToDb(this MusicItem i)
        {
            return new MusicItemDb
            {
                Source = i.Source.FullName,
                Title = i.Title,
                Artists = string.Join(',', i.Artists),
                Album = i.Album,
                Year = i.Year,
                Duration = i.Duration.Ticks,
                Track = i.Track,
                Cover = i.Cover.ToString(),
                MainColor = i.MainColor.ToInt(),
                LastWriteTime = i.LastWriteTime.ToBinary(),
                AlbumArtists = string.Join(',', i.AlbumArtists),
                Disc = i.Disc,
            };
        }

        public static string GetKey(this MusicItem i)
        {
            return i.Source.FullName;
        }

        public static MusicItem ToOrigin(this MusicItemDb i)
        {
            return new MusicItem
            {
                Source = new FileInfo(i.Source),
                Title = i.Title,
                Artists = i.Artists.Split(','),
                Album = i.Album,
                Year = i.Year,
                Duration = TimeSpan.FromTicks(i.Duration),
                Track = i.Track,
                Cover = new Uri(i.Cover),
                MainColor = i.MainColor.ToColor(),
                LastWriteTime = DateTime.FromBinary(i.LastWriteTime),
                AlbumArtists = i.AlbumArtists.Split(','),
                Disc = i.Disc,
            };
        }

        public static AlbumItem GetAlbum(this MusicItemDb i)
        {
            return i.AlbumRef.First().ToOrigin();
        }

        public static AlbumItemDb ToDb(this AlbumItem i)
        {
            var r = new AlbumItemDb
            {
                Title = i.Title,
                Artists = string.Join(',', i.Artists),
                Year = i.Year,
                Cover = i.Cover.ToString(),
                MainColor = i.MainColor.ToInt(),
                AllArtists = string.Join(',', i.AllArtists),
            };
            foreach (var item in i.MusicItems)
            {
                r.MusicItems.Add(item.ToDb());
            }
            return r;
        }

        public static AlbumItem ToOrigin(this AlbumItemDb i)
        {
            return new AlbumItem
            {
                Title = i.Title,
                Artists = i.Artists.Split(','),
                Year = i.Year,
                Cover = new Uri(i.Cover),
                MusicItems = i.MusicItems.Select(i => i.ToOrigin()).ToList(),
                MainColor = i.MainColor.ToColor(),
                AllArtists = i.AllArtists.Split(','),
            };
        }

        public static PlayListItemDb ToDb(this PlayListItem i)
        {
            var r = new PlayListItemDb
            {
                Source = i.Source.FullName,
                Title = i.Title,
                Year = i.Year,
                Cover = i.Cover.ToString(),
                LastWriteTime = i.LastWriteTime.ToBinary(),
            };
            foreach (var item in i.MusicItems)
            {
                r.MusicItems.Add(item.ToDb());
            }
            return r;
        }

        public static PlayListItem ToOrigin(this PlayListItemDb i)
        {
            return new PlayListItem
            {
                Source = new FileInfo(i.Source),
                Title = i.Title,
                Year = i.Year,
                Cover = new Uri(i.Cover),
                LastWriteTime = DateTime.FromBinary(i.LastWriteTime),
                MusicItems = i.MusicItems.Select(m => m.ToOrigin()).ToList(),
            };
        }

        public static VideoItemDb ToDb(this VideoItem i)
        {
            return new VideoItemDb
            {
                Source = i.Source.FullName,
                Title = i.Title,
                Duration = i.Duration.Ticks,
                Cover = i.Cover,
                LastWriteTime = i.LastWriteTime.ToBinary()
            };
        }

        public static VideoItem ToOrigin(this VideoItemDb i)
        {
            return new VideoItem
            {
                Source = new FileInfo(i.Source),
                Title = i.Title,
                Duration = TimeSpan.FromTicks(i.Duration),
                Cover = i.Cover,
                LastWriteTime = DateTime.FromBinary(i.LastWriteTime)
            };
        }

        public static SeriesItemDb ToDb(this SeriesItem i)
        {
            var r = new SeriesItemDb
            {
                Source = i.Source.FullName,
                Title = i.Title,
                Cover = i.Cover,
            };
            foreach (var item in i.Videos)
            {
                r.Videos.Add(item.ToDb());
            }
            return r;
        }

        public static SeriesItem ToOrigin(this SeriesItemDb i)
        {
            return new SeriesItem
            {
                Source = new DirectoryInfo(i.Source),
                Title = i.Title,
                Cover = i.Cover,
                Videos = i.Videos.Select(s => s.ToOrigin()).ToList()
            };
        }

        public static SingleVideoItemsDb ToDb(this SingleVideoItems i)
        {
            var r = new SingleVideoItemsDb();
            foreach (var item in i.Videos)
            {
                r.Videos.Add(item.ToDb());
            }
            return r;
        }

        public static SingleVideoItems ToOrigin(this SingleVideoItemsDb i)
        {
            return new SingleVideoItems
            {
                Videos = i.Videos.Select(s => s.ToOrigin()).ToList()
            };
        }
    }
}
