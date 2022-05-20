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
    public static class LikeHelper
    {
        static NetEaseMusicService CloudMusicService => ((App) Application.Current).NetEaseMusicService;
        
        public static void Like(object sender, RoutedEventArgs e)
        {
            var s = (FrameworkElement)sender;
            var m = s.DataContext as CloudMusicItem;
            CloudMusicService.Like(m);
        }

        public static SolidColorBrush GetLikeButtonForeground(MusicItem m)
        {
            if (m is CloudMusicItem c)
            {
                return CloudMusicService.GetSongLiked(c) ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
            }
            return new SolidColorBrush(Colors.Black);
        }

        public static string GetLikeButtonGlyph(MusicItem m)
        {
            if (m is CloudMusicItem c)
            {
                return CloudMusicService.GetSongLiked(c) ? "\uEB52" : "\uEB51";
            }
            return "\uEB51";
        }
    }

    public class LikeForegroundConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return LikeHelper.GetLikeButtonForeground((MusicItem)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
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
