using HotPotPlayer.Models;
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
    internal static class PlayListHelper
    {
        static LocalMusicService MusicService => ((App)Application.Current).LocalMusicService;
        static NetEaseMusicService CloudMusicService => ((App)Application.Current).NetEaseMusicService;

        public static void AddToPlayListClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var music = button.Tag as MusicItem;
            var flyout = new MenuFlyout();
            MenuFlyoutItem i;
            if (music is CloudMusicItem c)
            {
                foreach (var item in CloudMusicService.UserPlayLists)
                {
                    i = new MenuFlyoutItem
                    {
                        Text = item.Title,
                    };
                    i.Click += (s, e) => CloudMusicService.AddMusicToPlayList(item, c);
                    flyout.Items.Add(i);
                }
            }
            else
            {
                foreach (var item in MusicService.LocalPlayListList)
                {
                    i = new MenuFlyoutItem
                    {
                        Text = item.Title,
                    };
                    i.Click += (s, e) => AlbumHelper.MusicAddToPlayList(item.Title, music);
                    flyout.Items.Add(i);
                }
            }
            button.Flyout = flyout;
            button.Flyout.ShowAt(button);
        }
    }
}
