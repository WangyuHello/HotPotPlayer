using HotPotPlayer.Models.CloudMusic;
using HotPotPlayer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Pages.Helper
{
    public static class LikeHelper
    {
        static NetEaseMusicService CloudMusicService => ((App) Application.Current).NetEaseMusicService;
        
        public static void Like(object sender, RoutedEventArgs e)
        {
            var s = (FrameworkElement)sender;
            var m = s.DataContext as CloudMusicItem;
            CloudMusicService.Like(m);
        }
    }
}
