using HotPotPlayer.Extensions;
using Microsoft.UI.Xaml;
using MongoDB.Bson;
using Realms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace HotPotPlayer.Models
{
    public record MusicItem
    {
        public string Title { get; set; }
        public string[] Artists { get; set; }
        public string Album { get; set; }
        public uint Year { get; set; }
        public TimeSpan Duration { get; set; }
        public int Track { get; set; }
        public string Cover { get; set; }
        public string Source { get; set; }
        public FileInfo File { get; set; }
        public Color MainColor { get; set; }

        public string AlbumSignature => Album+Year;

        public AlbumItem AlbumRef { get; set; }

        public MusicItemDb ToDb()
        {
            return new MusicItemDb
            {
                Title = Title,
                Artists = string.Join(',', Artists),
                Album = Album,
                Year = (int)Year,
                Duration = Duration.Ticks,
                Track = Track,
                Cover = Cover,
                Source = Source,
                File = File.FullName,
                MainColor = MainColor.ToInt(),
            };
        }

        public string GetArtists()
        {
            return string.Join(", ", Artists);
        }

        public Visibility GetTrackVisibility()
        {
            if (AlbumRef == null || AlbumRef.IsPlayList)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }
    }

    public class MusicItemDb: RealmObject
    {
        [PrimaryKey]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        public string Title { get; set; }
        public string Artists { get; set; }
        public string Album { get; set; }
        public int Year { get; set; }
        public long Duration { get; set; }
        public int Track { get; set; }
        public string Cover { get; set; }
        public string Source { get; set; }
        public string File { get; set; }
        public int MainColor { get; set; }

        //[Backlink(nameof(AlbumItemDb.MusicItems))]
        //public IQueryable<AlbumItemDb> AlbumRef { get; }

        public MusicItem ToOrigin()
        {
            return new MusicItem
            {
                Title = Title,
                Artists = Artists.Split(','),
                Album = Album,
                Year = (uint)Year,
                Duration = TimeSpan.FromTicks(Duration),
                Track = Track,
                Cover = Cover,
                Source = Source,
                File = new FileInfo(File),
                MainColor = MainColor.ToColor(),
            };
        }
    }
}
