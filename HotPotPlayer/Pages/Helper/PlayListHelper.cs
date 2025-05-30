﻿using HotPotPlayer.Helpers;
using HotPotPlayer.Models;
using HotPotPlayer.Models.CloudMusic;
using HotPotPlayer.Services;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Pages.Helper
{
    internal class PlayListHelper: HelperBase
    {
        public static async void AddToPlayListClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var music = button.Tag as MusicItem;
            var flyout = new MenuFlyout();
            MenuFlyoutItem i;
            if (music is CloudMusicItem c)
            {
                foreach (var item in NetEaseMusicService.UserPlayLists)
                {
                    i = new MenuFlyoutItem
                    {
                        Text = item.Title,
                    };
                    i.Click += (s, e) => NetEaseMusicService.AddMusicToPlayList(item, c);
                    flyout.Items.Add(i);
                }
            }
            else
            {
                foreach (var item in await JellyfinMusicService.GetJellyfinPlayListList())
                {
                    i = new MenuFlyoutItem
                    {
                        Text = item.Name,
                    };
                    i.Click += (s, e) => AlbumHelper.MusicAddToPlayList(item, music);
                    flyout.Items.Add(i);
                }
            }
            button.Flyout = flyout;
            button.Flyout.ShowAt(button);
        }
        
        public static async void PlayListPlay(object sender, RoutedEventArgs e)
        {
            var playList = ((Control)sender).Tag as BaseItemDto;
            var playListMusicItems = await App.JellyfinMusicService.GetPlayListMusicItemsAsync(playList);
            App.MusicPlayer.PlayNext(playListMusicItems);
        }
    }
}
