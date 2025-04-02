using Jellyfin.Sdk.Generated.Models;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Helpers
{
    public static class BaseItemDtoHelper
    {
        public static Uri GetPrimaryJellyfinImage(BaseItemDto_ImageTags tag, Guid? parentId)
        {
            var JellyfinMusicService = ((IComponentServiceLocator)Application.Current).JellyfinMusicService;
            return JellyfinMusicService.GetPrimaryJellyfinImage(tag, parentId);
        }
        public static Uri GetPrimaryJellyfinImageSmall(BaseItemDto_ImageTags tag, Guid? parentId)
        {
            var JellyfinMusicService = ((IComponentServiceLocator)Application.Current).JellyfinMusicService;
            return JellyfinMusicService.GetPrimaryJellyfinImageSmall(tag, parentId);
        }
        public static Uri GetPrimaryJellyfinImageVerySmall(BaseItemDto_ImageTags tag, Guid? parentId)
        {
            var JellyfinMusicService = ((IComponentServiceLocator)Application.Current).JellyfinMusicService;
            return JellyfinMusicService.GetPrimaryJellyfinImageVerySmall(tag, parentId);
        }

        public static string GetJellyfinArtists(List<string> artists)
        {
            return string.Join(", ", artists);
        }

        public static string GetJellyfinDuration(long? runtimeticks)
        {
            var t = new TimeOnly(runtimeticks.Value);
            var str = t.ToString("mm\\:ss");
            return str;
        }
    }
}
