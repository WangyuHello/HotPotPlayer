using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HotPotPlayer.Models.CloudMusic
{
    internal class CloudMusicItemComparer : IEqualityComparer<MusicItem>
    {
        public bool Equals(MusicItem x, MusicItem y)
        {
            var x1 = x as CloudMusicItem;
            var y1 = y as CloudMusicItem;
            return x1.SId.Equals(y1.SId);
        }

        public int GetHashCode([DisallowNull] MusicItem obj)
        {
            var x1 = obj as CloudMusicItem;
            return x1.SId.GetHashCode();
        }
    }
}