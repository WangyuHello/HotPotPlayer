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
            //NetEaseMusicService.Like(m, !isLike);
        }

        public static void PlayScreenLike()
        {
            // TODO cloudmusic
            //if (MusicPlayer.CurrentPlaying is CloudMusicItem c)
            //{
            //    var like = NetEaseMusicService.GetSongLiked(c);
            //    //NetEaseMusicService.Like(c, !like);
            //}
        }
    }

    public class LikeGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var chk = (bool)value;
            return chk ? "\uEB52" : "\uEB51";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
