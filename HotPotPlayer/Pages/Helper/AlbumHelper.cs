using HotPotPlayer.Controls;
using HotPotPlayer.Extensions;
using HotPotPlayer.Models;
using HotPotPlayer.Models.CloudMusic;
using HotPotPlayer.Services;
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
    internal class AlbumHelper
    {
        static MusicPlayer Player => ((App)Application.Current).MusicPlayer;
        static MainWindow MainWindow => ((App)Application.Current).MainWindow;
        static LocalMusicService MusicService => ((App) Application.Current).LocalMusicService;
        static App App => (App)Application.Current;
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
            Player.HidePlayScreen();
            var el = sender as FrameworkElement;
            var music = el.Tag as MusicItem;
            var targetPage = music switch
            {
                CloudMusicItem => "CloudMusicSub.Album",
                _ => "MusicSub.Album"
            };
            MainWindow.NavigateTo(targetPage, music);
        }

        internal static void AlbumPlay(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var selectedAlbum = button.Tag as AlbumItem;
            Player.PlayNext(selectedAlbum);
        }

        internal static void ArtistClick(object sender, RoutedEventArgs e)
        {
            Player.HidePlayScreen();
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
            else if (sender is MenuFlyoutItem menuItem)
            {
                if (menuItem.Tag is CloudArtistItem c)
                {
                    MainWindow.NavigateTo("CloudMusicSub.Artist", c);
                }
                else
                {
                    var artist = menuItem.Text;
                    MainWindow.NavigateTo("MusicSub.Artist", artist);
                }
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
                        Icon = new SymbolIcon { Symbol = Symbol.Contact }
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
                i.Click += (s, e) => AddToNewPlayList(music);
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
                i.Click += (s , e) => MainWindow.NavigateTo("MusicSub.Info", music, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
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
            FrameworkElement target = null;
            MusicItem music = null;
            if (sender is Button button)
            {
                music = (MusicItem)button.Tag;
                target = button;
            }
            else if(sender is Grid g)
            {
                var e2 = e as RightTappedRoutedEventArgs;
                music = ((FrameworkElement)e2.OriginalSource).DataContext as MusicItem;
                target = g;
            }
            if (target == null || music == null) return;

            if (target.ContextFlyout == null)
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
                i.Click += (s, e) => AddToNewPlayList(music);
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
                i.Click += (s, e) => MainWindow.NavigateTo("MusicSub.Info", music, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                flyout.Items.Add(i);
                flyout.Items.Add(new MenuFlyoutSeparator());
                i = new MenuFlyoutItem
                {
                    Text = "选择",
                    Icon = new FontIcon { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = "\uE762" },
                };
                flyout.Items.Add(i);

                target.ContextFlyout = flyout;
            }
            target.ContextFlyout.ShowAt(target);
        }

        internal static void MusicItemClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                var music = (MusicItem)button.Tag;
                Player.PlayNextContinue(music);
            }
            else if(sender is ListView l)
            {
                var e2 = e as ItemClickEventArgs;
                var music = e2.ClickedItem as MusicItem;
                Player.PlayNextContinue(music);
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
                i.Click += (s, e) => MainWindow.NavigateTo("MusicSub.Info", music, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
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

        public static async void AddToNewPlayList(MusicItem music)
        {
            ContentDialog dialog = new()
            {
                Title = "新建播放列表",
                PrimaryButtonText = "确定",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                Content = new NewPlayListDialog(),
                XamlRoot = App.MainWindow.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var page = dialog.Content as NewPlayListDialog;
                var title = page.Title.Text;
                if (!string.IsNullOrEmpty(title))
                {
                    MusicService.NewPlayList(title, music);
                }
            }
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
            i1.Click += (s, a) => AlbumHelper.AlbumAddOne(album);
            flyout.Items.Add(i1);
            var i2 = new MenuFlyoutSeparator();
            flyout.Items.Add(i2);
            i1 = new MenuFlyoutItem
            {
                Text = "新建播放队列",
                Icon = new SymbolIcon { Symbol = Symbol.Add },
            };
            flyout.Items.Add(i1);
            foreach (var item in MusicService.LocalPlayListList)
            {
                var i = new MenuFlyoutItem
                {
                    Text = item.Title,
                    Tag = item
                };
                i.Click += (s, a) => AlbumHelper.AlbumAddToPlayList(item.Title, album);
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
