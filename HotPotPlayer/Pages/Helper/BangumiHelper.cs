using HotPotPlayer.Extensions;
using HotPotPlayer.Helpers;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.UI.Xaml;
using Richasy.BiliKernel.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Pages.Helper
{
    internal class BangumiHelper : HelperBase
    {
        public static void SeriesPlay(object sender, RoutedEventArgs e)
        {
            var video = (sender as FrameworkElement).Tag as BaseItemDto;
            VideoPlayer.PlayNext(video);
        }

        public static void BiliPlay(object sender, RoutedEventArgs e)
        {
            var video = (sender as FrameworkElement).Tag as VideoInformation;
            var dto = video.ToBaseItemDto();
            VideoPlayer.PlayNext(dto);
        }
        public static void BiliAddToPlayList(object sender, RoutedEventArgs e)
        {

        }
    }
}
