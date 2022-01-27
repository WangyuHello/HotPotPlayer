using HotPotPlayer.Extensions;
using HotPotPlayer.Models;
using HotPotPlayer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HotPotPlayer.Pages.Helper
{
    internal class AlbumHelper
    {
        static MusicPlayer Player => ((App)Application.Current).MusicPlayer.Value;
        static MainWindow MainWindow => ((App)Application.Current).MainWindow;

        internal static void AlbumAdd(SplitButton sender, SplitButtonClickEventArgs args)
        {
            var selectedAlbum = sender.Tag as AlbumItem;
            AlbumAddOne(selectedAlbum);
        }

        internal static void AlbumAddOne(AlbumItem selectedAlbum)
        {
            Player.AddToPlayList(selectedAlbum);
        }

        internal static void AlbumAddToPlayList(string playList, AlbumItem selectedAlbum)
        {

        }

        internal static void AlbumDetailClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var selectedAlbum = button.Tag as AlbumItem;
            MainWindow.NavigateTo("MusicSub.Album", selectedAlbum);
        }

        internal static void AlbumInfoClick(object sender, RoutedEventArgs e)
        {
            var music = (MusicItem)((HyperlinkButton)sender).Tag;
            MainWindow.NavigateTo("MusicSub.Album", music);
        }

        internal static void AlbumPlay(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var selectedAlbum = button.Tag as AlbumItem;
            Player.PlayNext(selectedAlbum);
        }

        internal static void ArtistClick(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton button)
            {
                var artist = (string)button.Content;
                var segs = artist.GetArtists();
                if (segs.Length == 1)
                {
                    MainWindow.NavigateTo("MusicSub.Artist", artist);
                }
                else
                {
                    if (button.ContextFlyout == null)
                    {
                        var flyout = new MenuFlyout();
                        foreach (var a in segs)
                        {
                            var item = new MenuFlyoutItem
                            {
                                Text = a,
                            };
                            item.Click += ArtistClick;
                            flyout.Items.Add(item);
                        }
                        button.ContextFlyout = flyout;
                    }
                    button.ContextFlyout.ShowAt(button);
                }
            }
            else if (sender is MenuFlyoutItem menuItem)
            {
                var artist = menuItem.Text;
                MainWindow.NavigateTo("MusicSub.Artist", artist);
            }
        }

        internal static void ArtistClick2(object sender, RoutedEventArgs e)
        {
            var button = sender as HyperlinkButton;
            var artist = (string)button.Content;
            var segs = artist.GetArtists();
            if (segs.Length == 1)
            {
                MainWindow.NavigateTo("MusicSub.Artist", artist);
            }
            else
            {
                var flyout = new MenuFlyout();
                foreach (var a in segs)
                {
                    var item = new MenuFlyoutItem
                    {
                        Text = a,
                    };
                    item.Click += ArtistClick;
                    flyout.Items.Add(item);
                }
                button.ContextFlyout = flyout;
                button.ContextFlyout.ShowAt(button);
            }
        }
    }
}
