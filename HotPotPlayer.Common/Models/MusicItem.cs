using HotPotPlayer.Extensions;
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
        public virtual string Title { get; set; }
        public virtual string[] Artists { get; set; }
        public virtual string Album { get; set; }
        public int Year { get; set; }
        public TimeSpan Duration { get; set; }
        public int Track { get; set; }
        public string Genre { get; set; }
        public long BitRate { get; set; }
        public int SampleRate { get; set; }
        public int BitDepth { get; set; }
        public virtual Uri Cover { get; set; }
        public Color MainColor { get; set; }
        public DateTime LastWriteTime { get; set; }
        public string[] AlbumArtists { get; set; }
        public int Disc { get; set; }
        public bool IsLike { get; set; }

        public AlbumItem AlbumRef { get; set; }

        public PlayListItem PlayListRef { get; set; }

        public string AlbumSignature => Album+Year;
        public int DiscTrack => (Disc << 16) | Track;

        /// <summary>
        /// 如果是网络音乐但是用本地音乐播放
        /// </summary>
        public bool IsIntercept { get; set; }

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

        public string GetSampleRate()
        {
            return SampleRate + " Hz";
        }

        public string GetBitDepth()
        {
            return BitDepth switch
            {
                0 => "未知",
                _ => BitDepth + " bit"
            };
        }
    }

    sealed class MusicItemComparer : EqualityComparer<MusicItem>
    {
        public override bool Equals(MusicItem x, MusicItem y)
        {
            if (x.Source.FullName == y.Source.FullName)
                return true;
            return false;
        }

        public override int GetHashCode(MusicItem obj)
        {
            return obj.Source.GetHashCode();
        }
    }
}
