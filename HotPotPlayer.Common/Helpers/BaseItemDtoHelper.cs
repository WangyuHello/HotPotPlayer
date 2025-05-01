using HotPotPlayer.Models;
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

        public static Uri GetChapterImage(string tag, int index, Guid parentId)
        {
            var JellyfinMusicService = ((IComponentServiceLocator)Application.Current).JellyfinMusicService;
            return JellyfinMusicService.GetChapterImage(tag, index, parentId);
        }

        public static ImageSourceWithBlur GetPrimaryJellyfinImageWithBlur(BaseItemDto_ImageTags tag, BaseItemDto_ImageBlurHashes blurs, Guid? parentId)
        {
            return new ImageSourceWithBlur
            {
                ImageTags = tag,
                BlurHashes = blurs,
                Parent = parentId.Value,
                Label = "Primary"
            };
        }

        public static ImageSourceWithBlur GetBackdropJellyfinImageWithBlur(List<string> tags, BaseItemDto_ImageBlurHashes blurs, Guid? parentId)
        {
            return new ImageSourceWithBlur
            {
                BackdropImageTags = tags,
                BlurHashes = blurs,
                Parent = parentId.Value,
                Label = "Backdrop"
            };
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
        public static string GetJellyfinDuration2(long? runtimeticks)
        {
            var t = new TimeOnly(runtimeticks.Value);
            string str = string.Empty;
            if(t > new TimeOnly(10, 0))
            {
                str = t.ToString("hh小时mm分钟");
            }
            else if(t > new TimeOnly(1, 0))
            {
                str = t.ToString("h小时mm分钟");
            }
            else
            {
                str = t.ToString("mm分钟");
            }
            return str;
        }
    }
}
