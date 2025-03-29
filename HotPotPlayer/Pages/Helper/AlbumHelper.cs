using HotPotPlayer.Controls;
using HotPotPlayer.Extensions;
using HotPotPlayer.Helpers;
using HotPotPlayer.Models;
using HotPotPlayer.Models.CloudMusic;
using HotPotPlayer.Services;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Pages.Helper
{
    internal class AlbumHelper: HelperBase
    {
        internal static void AlbumAdd(SplitButton sender, SplitButtonClickEventArgs args)
        {
            var selectedAlbum = sender.Tag as AlbumItem;
            AlbumAddOne(selectedAlbum);
        }

        internal static void AlbumAddOne(AlbumItem selectedAlbum)
        {
            MusicPlayer.AddToPlayList(selectedAlbum);
        }

        internal static void AlbumAddToPlayList(string playList, AlbumItem selectedAlbum)
        {
            JellyfinMusicService.AddAlbumToPlayList(playList, selectedAlbum);
        }

        internal static void MusicAddToPlayList(string playList, MusicItem music)
        {
            JellyfinMusicService.AddMusicToPlayList(playList, music);
        }

        internal static void MusicAddToPlayList(string playList, BaseItemDto music)
        {

        }

        internal static void AlbumDetailClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var selectedAlbum = button.Tag as BaseItemDto;
            App.NavigateTo("MusicSub.Album", selectedAlbum);
        }

        internal static void AlbumInfoClick(object sender, RoutedEventArgs e)
        {
            MusicPlayer.HidePlayScreen();
            var el = sender as FrameworkElement;
            var music = el.Tag as BaseItemDto;
            var targetPage = music switch
            {
                _ => "MusicSub.Album"
            };
            App.NavigateTo(targetPage, music);
        }

        internal static void AlbumPlay(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var selectedAlbum = button.Tag as BaseItemDto;
            MusicPlayer.PlayNext(selectedAlbum);
        }

        internal static void ArtistClick(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton button)
            {
                if (button.Tag is CloudMusicItem cm)
                {
                    var artist = cm.Artists2;
                    if (artist.Count == 1)
                    {
                        MusicPlayer.HidePlayScreen();
                        App.NavigateTo("CloudMusicSub.Artist", artist[0]);
                    }
                    else
                    {
                        if (button.ContextFlyout == null)
                        {
                            var flyout = new MenuFlyout();
                            foreach (var a in artist)
                            {
                                var item = new MenuFlyoutItem
                                {
                                    Text = a.Name,
                                    Icon = new SymbolIcon { Symbol = Symbol.Contact },
                                    Tag = a
                                };
                                item.Click += ArtistClick;
                                flyout.Items.Add(item);
                            }
                            button.ContextFlyout = flyout;
                        }
                        button.ContextFlyout.ShowAt(button);
                    }
                }
                else
                {
                    var music = button.Tag as BaseItemDto;
                    var artists = music.ArtistItems;
                    if (artists.Count == 1)
                    {
                        MusicPlayer.HidePlayScreen();
                        App.NavigateTo("MusicSub.Artist", artists[0].Id);
                    }
                    else
                    {
                        if (button.ContextFlyout == null)
                        {
                            var flyout = new MenuFlyout();
                            foreach (var a in artists)
                            {
                                var item = new MenuFlyoutItem
                                {
                                    Text = a.Name,
                                    Icon = new SymbolIcon { Symbol = Symbol.Contact }
                                };
                                item.Click += ArtistClick;
                                flyout.Items.Add(item);
                            }
                            button.ContextFlyout = flyout;
                        }
                        button.ContextFlyout.ShowAt(button);
                    }
                }
            }
            else if (sender is MenuFlyoutItem menuItem)
            {
                MusicPlayer.HidePlayScreen();
                if (menuItem.Tag is CloudArtistItem c)
                {
                    App.NavigateTo("CloudMusicSub.Artist", c);
                }
                else
                {
                    var artistId = (Guid)menuItem.Tag;
                    App.NavigateTo("MusicSub.Artist", artistId);
                }
            }
            else if (sender is TextBlock t)
            {
                if (t.Tag is CloudMusicItem cm)
                {
                    var artist = cm.Artists2;
                    if (artist.Count == 1)
                    {
                        MusicPlayer.HidePlayScreen();
                        App.NavigateTo("CloudMusicSub.Artist", artist[0]);
                    }
                    else
                    {
                        var flyout = new MenuFlyout();
                        foreach (var a in artist)
                        {
                            var item = new MenuFlyoutItem
                            {
                                Text = a.Name,
                                Icon = new SymbolIcon { Symbol = Symbol.Contact },
                                Tag = a
                            };
                            item.Click += ArtistClick;
                            flyout.Items.Add(item);
                        }
                        t.ContextFlyout = flyout;
                        t.ContextFlyout.ShowAt(t);
                    }
                }
                else
                {
                    var music = t.Tag as BaseItemDto;
                    var artists = music.ArtistItems;
                    if (artists.Count == 1)
                    {
                        MusicPlayer.HidePlayScreen();
                        App.NavigateTo("MusicSub.Artist", artists[0].Id);
                    }
                    else
                    {
                        if (t.ContextFlyout == null)
                        {
                            var flyout = new MenuFlyout();
                            foreach (var a in artists)
                            {
                                var item = new MenuFlyoutItem
                                {
                                    Text = a.Name,
                                    Icon = new SymbolIcon { Symbol = Symbol.Contact }
                                };
                                item.Click += ArtistClick;
                                flyout.Items.Add(item);
                            }
                            t.ContextFlyout = flyout;
                        }
                        t.ContextFlyout.ShowAt(t);
                    }
                }
            }
        }

        internal static void ArtistClick2(object sender, RoutedEventArgs e)
        {
            var button = sender as HyperlinkButton;
            if (button.Tag is CloudAlbumItem c)
            {
                var artist = c.AlbumArtist;
                if (artist != null)
                {
                    App.NavigateTo("CloudMusicSub.Artist", artist);
                }
            }
            else
            {
                var music = button.Tag as BaseItemDto;
                var artists = music.ArtistItems;
                if (artists.Count == 1)
                {
                    App.NavigateTo("MusicSub.Artist", artists[0].Id);
                }
                else
                {
                    var flyout = new MenuFlyout();
                    foreach (var a in artists)
                    {
                        var item = new MenuFlyoutItem
                        {
                            Text = a.Name,
                            Icon = new SymbolIcon { Symbol = Symbol.Contact }
                        };
                        item.Click += ArtistClick;
                        flyout.Items.Add(item);
                    }
                    button.ContextFlyout = flyout;
                    button.ContextFlyout.ShowAt(button);
                }
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
                i.Click += (s, e) => MusicPlayer.PlayNext(music);
                flyout.Items.Add(i);
                i = new MenuFlyoutItem
                {
                    Text = "下一个播放",
                    Icon = new FontIcon { FontFamily = (FontFamily)Application.Current.Resources["SegoeIcons"], Glyph = "\uECC8" },
                };
                i.Click += (s, e) => MusicPlayer.AddToPlayListNext(music);
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
                i.Click += (s, e) => MusicPlayer.AddToPlayListLast(music);
                sub.Items.Add(i);
                sub.Items.Add(new MenuFlyoutSeparator());
                i = new MenuFlyoutItem
                {
                    Text = "新建播放队列",
                    Icon = new SymbolIcon { Symbol = Symbol.Add },
                };
                i.Click += (s, e) => AddToNewPlayList(music);
                sub.Items.Add(i);

                foreach (var item in JellyfinMusicService.LocalPlayListList)
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
                    Icon = new FontIcon { FontFamily = (FontFamily)Application.Current.Resources["SegoeIcons"], Glyph = "\uE946" },
                };
                i.Click += (s , e) => App.NavigateTo("MusicSub.Info", music, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                flyout.Items.Add(i);
                flyout.Items.Add(new MenuFlyoutSeparator());
                i = new MenuFlyoutItem
                {
                    Text = "选择",
                    Icon = new FontIcon { FontFamily = (FontFamily)Application.Current.Resources["SegoeIcons"], Glyph = "\uE762" },
                };
                flyout.Items.Add(i);

                button.ContextFlyout = flyout;
            }
            button.ContextFlyout.ShowAt(button);
        }

        

        internal static void MusicItemClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                var music = (MusicItem)button.Tag;
                MusicPlayer.PlayNextContinue(music);
            }
            else if(sender is ListView l)
            {
                var e2 = e as ItemClickEventArgs;
                var music = e2.ClickedItem as BaseItemDto;
                MusicPlayer.PlayNextContinue(music);
            }

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
                i.Click += (s, e) => MusicPlayer.PlayNext(music);
                flyout.Items.Add(i);
                i = new MenuFlyoutItem
                {
                    Text = "下一个播放",
                    Icon = new FontIcon { FontFamily = (FontFamily)Application.Current.Resources["SegoeIcons"], Glyph = "\uECC8" },
                };
                i.Click += (s, e) => MusicPlayer.AddToPlayListNext(music);
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
                i.Click += (s, e) => MusicPlayer.AddToPlayListLast(music);
                sub.Items.Add(i);
                sub.Items.Add(new MenuFlyoutSeparator());
                i = new MenuFlyoutItem
                {
                    Text = "新建播放队列",
                    Icon = new SymbolIcon { Symbol = Symbol.Add },
                };
                sub.Items.Add(i);

                foreach (var item in JellyfinMusicService.LocalPlayListList)
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
                i.Click += (s, e) => JellyfinMusicService.PlayListMusicDelete(music);
                flyout.Items.Add(i);
                i = new MenuFlyoutItem
                {
                    Text = "上移",
                    Icon = new SymbolIcon { Symbol = Symbol.Up },
                };
                i.Click += (s, e) => JellyfinMusicService.PlayListMusicUp(music);
                flyout.Items.Add(i);
                i = new MenuFlyoutItem
                {
                    Text = "下移",
                    Icon = new FontIcon { FontFamily = (FontFamily)Application.Current.Resources["SegoeIcons"], Glyph = "\uE74B" },
                };
                i.Click += (s, e) => JellyfinMusicService.PlayListMusicDown(music);
                flyout.Items.Add(i);
                i = new MenuFlyoutItem
                {
                    Text = "属性",
                    Icon = new FontIcon { FontFamily = (FontFamily)Application.Current.Resources["SegoeIcons"], Glyph = "\uE946" },
                };
                i.Click += (s, e) => App.NavigateTo("MusicSub.Info", music, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                flyout.Items.Add(i);
                flyout.Items.Add(new MenuFlyoutSeparator());
                i = new MenuFlyoutItem
                {
                    Text = "选择",
                    Icon = new FontIcon { FontFamily = (FontFamily)Application.Current.Resources["SegoeIcons"], Glyph = "\uE762" },
                };
                flyout.Items.Add(i);

                button.ContextFlyout = flyout;
            }
            button.ContextFlyout.ShowAt(button);
        }

        public static async void AddToNewPlayList(MusicItem music)
        {
            ContentDialog dialog = new()
            {
                Title = "新建播放列表",
                PrimaryButtonText = "确定",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                Content = new NewPlayListDialog(),
                XamlRoot = XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var page = dialog.Content as NewPlayListDialog;
                var title = page.Title.Text;
                if (!string.IsNullOrEmpty(title))
                {
                    JellyfinMusicService.NewPlayList(title, music);
                }
            }
        }

        public static void AddToNewPlayList(BaseItemDto music)
        {

        }

        public static void InitSplitButtonFlyout(SplitButton targetButton, AlbumItem album)
        {
            if (targetButton.Flyout != null)
            {
                return;
            }
            var flyout = new MenuFlyout();
            var i1 = new MenuFlyoutItem
            {
                Text = "当前列表",
                Icon = new SymbolIcon { Symbol = Symbol.MusicInfo },
            };
            i1.Click += (s, a) => AlbumAddOne(album);
            flyout.Items.Add(i1);
            var i2 = new MenuFlyoutSeparator();
            flyout.Items.Add(i2);
            i1 = new MenuFlyoutItem
            {
                Text = "新建播放队列",
                Icon = new SymbolIcon { Symbol = Symbol.Add },
            };
            flyout.Items.Add(i1);
            foreach (var item in JellyfinMusicService.LocalPlayListList)
            {
                var i = new MenuFlyoutItem
                {
                    Text = item.Title,
                    Tag = item
                };
                i.Click += (s, a) => AlbumAddToPlayList(item.Title, album);
                flyout.Items.Add(i);
            }
            targetButton.Flyout = flyout;
        }

        public static void SuppressTap(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}
