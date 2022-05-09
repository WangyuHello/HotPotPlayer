using HotPotPlayer.Models;
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
    internal static class ListHelper
    {
        static App App => (App)Application.Current;
        static MusicPlayer Player => ((App)Application.Current).MusicPlayer;
        public static void PlayMusicInList(object sender, ItemClickEventArgs e)
        {
            if (sender is GridView g)
            {
                var list = (IEnumerable<MusicItem>)g.ItemsSource;
                var music = e.ClickedItem as MusicItem;
                Player.PlayNext(music, list);
            }
        }

        public static void PlayVideo(object sender, ItemClickEventArgs e)
        {
            var video = e.ClickedItem as VideoItem;
            App.PlayVideo(video.Source);
        }
    }
}
