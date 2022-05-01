using HotPotPlayer.Extensions;
using MongoDB.Bson;
using Realms;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Models
{
    public record MusicItem
    {
        public FileInfo Source { get; set; }
        public string Title { get; set; }
        public string[] Artists { get; set; }
        public string Album { get; set; }
        public int Year { get; set; }
        public TimeSpan Duration { get; set; }
        public int Track { get; set; }
        public string Genre { get; set; }
        public long BitRate { get; set; }
        public int SampleRate { get; set; }
        public int BitDepth { get; set; }

        public Uri Cover { get; set; }
        public Color MainColor { get; set; }
        public DateTime LastWriteTime { get; set; }
        public string[] AlbumArtists { get; set; }
        public int Disc { get; set; }

        public AlbumItem AlbumRef { get; set; }

        public PlayListItem PlayListRef { get; set; }

        public string AlbumSignature => Album+Year;
        public int DiscTrack => (Disc << 16) | Track;

        public string GetArtists()
        {
            return string.Join(", ", Artists);
        }
        public string GetAlbumArtists()
        {
            return string.Join(", ", AlbumArtists);
        }

        public string GetBitRate()
        {
            return ((double)BitRate / 1000) + " kb/s";
        }
    }

    public class MusicItemDb: RealmObject
    {
        /// <summary>
        /// 文件的路径作为Key，确保唯一性
        /// </summary>
        [PrimaryKey]
        public string Source { get; set; }
        public string Title { get; set; }
        public string Artists { get; set; }
        public string Album { get; set; }
        public int Year { get; set; }
        public long Duration { get; set; }
        public int Track { get; set; }
        public string Genre { get; set; }
        public long BitRate { get; set; }
        public int SampleRate { get; set; }
        public int BitDepth { get; set; }

        public string Cover { get; set; }
        public int MainColor { get; set; }
        public long LastWriteTime { get; set; }
        public string AlbumArtists { get; set; }
        public int Disc { get; set; }

        /// <summary>
        /// 每个音乐属于唯一一个专辑
        /// </summary>
        [Backlink(nameof(AlbumItemDb.MusicItems))]
        public IQueryable<AlbumItemDb> AlbumRef { get; }
    }

    sealed class MusicItemDbComparer : EqualityComparer<MusicItemDb>
    {
        public override bool Equals(MusicItemDb x, MusicItemDb y)
        {
            if (x.Source == y.Source && x.LastWriteTime == y.LastWriteTime)
                return true;
            return false;
        }

        public override int GetHashCode(MusicItemDb obj)
        {
            return obj.Source.GetHashCode() + obj.LastWriteTime.GetHashCode();
        }
    }

    sealed class MusicItemComparer : EqualityComparer<MusicItem>
    {
        public override bool Equals(MusicItem x, MusicItem y)
        {
            if (x.Source == y.Source)
                return true;
            return false;
        }

        public override int GetHashCode(MusicItem obj)
        {
            return obj.Source.GetHashCode();
        }
    }
}
