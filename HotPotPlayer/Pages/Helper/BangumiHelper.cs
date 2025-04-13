using HotPotPlayer.Helpers;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.UI.Xaml;
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
            App.PlayVideos(video, 0);
        }
    }
}
