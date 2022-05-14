using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HotPotPlayer.Models.CloudMusic
{
    internal class CloudMusicItemComparer : IEqualityComparer<CloudMusicItem>
    {
        public bool Equals(CloudMusicItem x, CloudMusicItem y)
        {
            return x.SId.Equals(y.SId);
        }

        public int GetHashCode([DisallowNull] CloudMusicItem obj)
        {
            return obj.SId.GetHashCode();
        }
    }
}