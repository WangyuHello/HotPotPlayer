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
    internal class ListHelper: HelperBase
    {
        public static void PlayMusicInList(object sender, ItemClickEventArgs e)
        {
            if (sender is ListViewBase g)
            {
                var list = (IEnumerable<MusicItem>)g.ItemsSource;
                var music = e.ClickedItem as MusicItem;
                MusicPlayer.PlayNext(music, list);
            }
        }

        public static void PlayVideo(object sender, ItemClickEventArgs e)
        {
            var video = e.ClickedItem as VideoItem;
            //App.PlayVideo(video);
        }

        public static async void RightTapMusicInListClick(object sender, RoutedEventArgs e)
        {
            var _element = (FrameworkElement)sender;
            var music = (BaseItemDto)_element.DataContext;

            if (_element.ContextFlyout == null)
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
                i.Click += (s, e) => AlbumHelper.AddToNewPlayList(music);
                sub.Items.Add(i);

                if (false) //TODO cloudmusic
                {
                    //foreach (var item in NetEaseMusicService.UserPlayLists)
                    //{
                    //    i = new MenuFlyoutItem
                    //    {
                    //        Text = item.Title,
                    //    };
                    //    i.Click += (s, e) => NetEaseMusicService.AddMusicToPlayList(item, c);
                    //    sub.Items.Add(i);
                    //}
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
                        sub.Items.Add(i);
                    }
                }

                flyout.Items.Add(sub);
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

                _element.ContextFlyout = flyout;
            }
            _element.ContextFlyout.ShowAt(_element);
        }

        public static async void RightTapMusicInPlayListClick(object sender, RoutedEventArgs e)
        {
            var _element = (FrameworkElement)sender;
            var music = (BaseItemDto)_element.DataContext;

            if (_element.ContextFlyout == null)
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

                if (false) // TODO cloudmusic
                {
                    //foreach (var item in NetEaseMusicService.UserPlayLists)
                    //{
                    //    i = new MenuFlyoutItem
                    //    {
                    //        Text = item.Title,
                    //    };
                    //    i.Click += (s, e) => NetEaseMusicService.AddMusicToPlayList(item, c);
                    //    sub.Items.Add(i);
                    //}
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
                        sub.Items.Add(i);
                    }
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

                _element.ContextFlyout = flyout;
            }
            _element.ContextFlyout.ShowAt(_element);
        }

        public static async void RightTapMusicInCloudPlayListClick(object sender, RoutedEventArgs e)
        {
            var _element = (FrameworkElement)sender;
            var music = (MusicItem)_element.DataContext;

            if (_element.ContextFlyout == null)
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

                if (music is CloudMusicItem c)
                {
                    foreach (var item in NetEaseMusicService.UserPlayLists)
                    {
                        i = new MenuFlyoutItem
                        {
                            Text = item.Title,
                        };
                        i.Click += (s, e) => NetEaseMusicService.AddMusicToPlayList(item, c);
                        sub.Items.Add(i);
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
                        sub.Items.Add(i);
                    }
                }


                flyout.Items.Add(sub);
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

                _element.ContextFlyout = flyout;
            }
            _element.ContextFlyout.ShowAt(_element);
        }

        public static async void RightTapMusicInArtistMusicListClick(object sender, RoutedEventArgs e)
        {
            var _element = (FrameworkElement)sender;
            var music = (MusicItem)_element.DataContext;

            if (_element.ContextFlyout == null)
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

                if (music is CloudMusicItem c)
                {
                    foreach (var item in NetEaseMusicService.UserPlayLists)
                    {
                        i = new MenuFlyoutItem
                        {
                            Text = item.Title,
                        };
                        i.Click += (s, e) => NetEaseMusicService.AddMusicToPlayList(item, c);
                        sub.Items.Add(i);
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
                        sub.Items.Add(i);
                    }
                }
                flyout.Items.Add(sub);

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

                _element.ContextFlyout = flyout;
            }
            _element.ContextFlyout.ShowAt(_element);
        }

        public static async void RightTapMusicInCurrentPlayListClick(object sender, RoutedEventArgs e)
        {
            FrameworkElement target = null;
            BaseItemDto music = null;
            if (sender is Button button)
            {
                music = (BaseItemDto)button.Tag;
                target = button;
            }
            else if (sender is Grid g)
            {
                var e2 = e as RightTappedRoutedEventArgs;
                music = ((FrameworkElement)e2.OriginalSource).DataContext as BaseItemDto;
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
                i.Click += (s, e) => MusicPlayer.AddToPlayListLast(music);
                sub.Items.Add(i);
                sub.Items.Add(new MenuFlyoutSeparator());
                i = new MenuFlyoutItem
                {
                    Text = "新建播放队列",
                    Icon = new SymbolIcon { Symbol = Symbol.Add },
                };
                i.Click += (s, e) => AlbumHelper.AddToNewPlayList(music);
                sub.Items.Add(i);

                if (false) // TODO Cloudmusic
                {
                    //foreach (var item in NetEaseMusicService.UserPlayLists)
                    //{
                    //    i = new MenuFlyoutItem
                    //    {
                    //        Text = item.Title,
                    //    };
                    //    i.Click += (s, e) => NetEaseMusicService.AddMusicToPlayList(item, c);
                    //    sub.Items.Add(i);
                    //}
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
                        sub.Items.Add(i);
                    }
                }

                flyout.Items.Add(sub);
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

                target.ContextFlyout = flyout;
            }
            target.ContextFlyout.ShowAt(target);
        }
    }
}
