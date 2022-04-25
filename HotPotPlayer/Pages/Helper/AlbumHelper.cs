using HotPotPlayer.Extensions;
using HotPotPlayer.Models;
using HotPotPlayer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Pages.Helper
{
    internal class AlbumHelper
    {
        static MusicPlayer Player => ((App)Application.Current).MusicPlayer;
        static MainWindow MainWindow => ((App)Application.Current).MainWindow;
        static LocalMusicService MusicService => ((App) Application.Current).LocalMusicService;

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
            MusicService.AddAlbumToPlayList(playList, selectedAlbum);
        }

        internal static void MusicAddToPlayList(string playList, MusicItem music)
        {
            MusicService.AddMusicToPlayList(playList, music);
        }

        internal static void AlbumDetailClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var selectedAlbum = button.Tag as AlbumItem;
            MainWindow.NavigateTo("MusicSub.Album", selectedAlbum);
        }

        internal static void AlbumInfoClick(object sender, RoutedEventArgs e)
        {
            var el = sender as FrameworkElement;
            var music = el.Tag as MusicItem;
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

        internal static void MusicClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var music = (MusicItem)button.Tag;

            if (button.ContextFlyout == null)
            {
                var flyout = new MenuFlyout();
                var i = new MenuFlyoutItem 
                { 
                    Text = "播放", 
                    Icon = new SymbolIcon { Symbol = Symbol.Play },
                };
                i.Click += (s, e) => Player.PlayNext(music);
                flyout.Items.Add(i);
                i = new MenuFlyoutItem
                {
                    Text = "下一个播放",
                    Icon = new FontIcon { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = "\uECC8" },
                };
                i.Click += (s, e) => Player.AddToPlayListNext(music);
                flyout.Items.Add(i);
                flyout.Items.Add(new MenuFlyoutSeparator());
                var sub = new MenuFlyoutSubItem
                {
                    Text = "添加到",
                    Icon = new SymbolIcon { Symbol = Symbol.Add },
                };
                i = new MenuFlyoutItem
                {
                    Text = "当前列表",
                    Icon = new SymbolIcon { Symbol = Symbol.MusicInfo },
                };
                i.Click += (s, e) => Player.AddToPlayListLast(music);
                sub.Items.Add(i);
                sub.Items.Add(new MenuFlyoutSeparator());
                i = new MenuFlyoutItem
                {
                    Text = "新建播放队列",
                    Icon = new SymbolIcon { Symbol = Symbol.Add },
                };
                sub.Items.Add(i);

                foreach (var item in MusicService.LocalPlayListList)
                {
                    i = new MenuFlyoutItem
                    {
                        Text = item.Title,
                    };
                    i.Click += (s ,e) => MusicAddToPlayList(item.Title, music);
                    sub.Items.Add(i);
                }
                flyout.Items.Add(sub);
                i = new MenuFlyoutItem
                {
                    Text = "属性",
                    Icon = new FontIcon { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = "\uE946" },
                };
                flyout.Items.Add(i);
                flyout.Items.Add(new MenuFlyoutSeparator());
                i = new MenuFlyoutItem
                {
                    Text = "选择",
                    Icon = new FontIcon { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = "\uE762" },
                };
                flyout.Items.Add(i);

                button.ContextFlyout = flyout;
            }
            button.ContextFlyout.ShowAt(button);
        }

        internal static void MusicClick2(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var music = (MusicItem)button.Tag;

            if (button.ContextFlyout == null)
            {
                var flyout = new MenuFlyout();
                var sub = new MenuFlyoutSubItem
                {
                    Text = "添加到",
                    Icon = new SymbolIcon { Symbol = Symbol.Add },
                };
                var i = new MenuFlyoutItem
                {
                    Text = "当前列表",
                    Icon = new SymbolIcon { Symbol = Symbol.MusicInfo },
                };
                i.Click += (s, e) => Player.AddToPlayListLast(music);
                sub.Items.Add(i);
                sub.Items.Add(new MenuFlyoutSeparator());
                i = new MenuFlyoutItem
                {
                    Text = "新建播放队列",
                    Icon = new SymbolIcon { Symbol = Symbol.Add },
                };
                sub.Items.Add(i);

                foreach (var item in MusicService.LocalPlayListList)
                {
                    i = new MenuFlyoutItem
                    {
                        Text = item.Title,
                    };
                    i.Click += (s, e) => MusicAddToPlayList(item.Title, music);
                    sub.Items.Add(i);
                }
                flyout.Items.Add(sub);
                i = new MenuFlyoutItem
                {
                    Text = "属性",
                    Icon = new FontIcon { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = "\uE946" },
                };
                flyout.Items.Add(i);
                flyout.Items.Add(new MenuFlyoutSeparator());
                i = new MenuFlyoutItem
                {
                    Text = "选择",
                    Icon = new FontIcon { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = "\uE762" },
                };
                flyout.Items.Add(i);

                button.ContextFlyout = flyout;
            }
            button.ContextFlyout.ShowAt(button);
        }

        internal static void MusicItemClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var music = (MusicItem)button.Tag;
            Player.PlayNextContinue(music);
        }

        internal static void PlayListMusicClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var music = (MusicItem)button.Tag;

            if (button.ContextFlyout == null)
            {
                var flyout = new MenuFlyout();
                var i = new MenuFlyoutItem
                {
                    Text = "播放",
                    Icon = new SymbolIcon { Symbol = Symbol.Play },
                };
                i.Click += (s, e) => Player.PlayNext(music);
                flyout.Items.Add(i);
                i = new MenuFlyoutItem
                {
                    Text = "下一个播放",
                    Icon = new FontIcon { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = "\uECC8" },
                };
                i.Click += (s, e) => Player.AddToPlayListNext(music);
                flyout.Items.Add(i);
                flyout.Items.Add(new MenuFlyoutSeparator());
                var sub = new MenuFlyoutSubItem
                {
                    Text = "添加到",
                    Icon = new SymbolIcon { Symbol = Symbol.Add },
                };
                i = new MenuFlyoutItem
                {
                    Text = "当前列表",
                    Icon = new SymbolIcon { Symbol = Symbol.MusicInfo },
                };
                i.Click += (s, e) => Player.AddToPlayListLast(music);
                sub.Items.Add(i);
                sub.Items.Add(new MenuFlyoutSeparator());
                i = new MenuFlyoutItem
                {
                    Text = "新建播放队列",
                    Icon = new SymbolIcon { Symbol = Symbol.Add },
                };
                sub.Items.Add(i);

                foreach (var item in MusicService.LocalPlayListList)
                {
                    i = new MenuFlyoutItem
                    {
                        Text = item.Title,
                    };
                    i.Click += (s, e) => MusicAddToPlayList(item.Title, music);
                    sub.Items.Add(i);
                }
                flyout.Items.Add(sub);
                i = new MenuFlyoutItem
                {
                    Text = "删除",
                    Icon = new SymbolIcon { Symbol = Symbol.Clear },
                };
                i.Click += (s, e) => MusicService.PlayListMusicDelete(music);
                flyout.Items.Add(i);
                i = new MenuFlyoutItem
                {
                    Text = "上移",
                    Icon = new SymbolIcon { Symbol = Symbol.Up },
                };
                i.Click += (s, e) => MusicService.PlayListMusicUp(music);
                flyout.Items.Add(i);
                i = new MenuFlyoutItem
                {
                    Text = "下移",
                    Icon = new FontIcon { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = "\uE74B" },
                };
                i.Click += (s, e) => MusicService.PlayListMusicDown(music);
                flyout.Items.Add(i);
                i = new MenuFlyoutItem
                {
                    Text = "属性",
                    Icon = new FontIcon { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = "\uE946" },
                };
                flyout.Items.Add(i);
                flyout.Items.Add(new MenuFlyoutSeparator());
                i = new MenuFlyoutItem
                {
                    Text = "选择",
                    Icon = new FontIcon { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = "\uE762" },
                };
                flyout.Items.Add(i);

                button.ContextFlyout = flyout;
            }
            button.ContextFlyout.ShowAt(button);
        }
    }
}
