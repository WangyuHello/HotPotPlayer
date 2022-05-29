using HotPotPlayer.Helpers;
using HotPotPlayer.Models;
using HotPotPlayer.Models.CloudMusic;
using HotPotPlayer.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Pages.Helper
{
    public class LikeHelper: HelperBase
    {        
        public static void Like(object sender, RoutedEventArgs e)
        {
            var s = (FrameworkElement)sender;
            var m = s.DataContext as CloudMusicItem;
            var isLike = NetEaseMusicService.GetSongLiked(m);
            NetEaseMusicService.Like(m, !isLike);
        }

        public static SolidColorBrush GetLikeButtonForeground(MusicItem m)
        {
            if (m is CloudMusicItem c)
            {
                return NetEaseMusicService.GetSongLiked(c) ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
            }
            return new SolidColorBrush(Colors.Black);
        }

        public static string GetLikeButtonGlyph(MusicItem m)
        {
            if (m is CloudMusicItem c)
            {
                return NetEaseMusicService.GetSongLiked(c) ? "\uEB52" : "\uEB51";
            }
            return "\uEB51";
        }
    }

    public class LikeGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return LikeHelper.GetLikeButtonGlyph((MusicItem)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
