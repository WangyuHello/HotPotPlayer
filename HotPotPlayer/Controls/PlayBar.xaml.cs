using HotPotPlayer.Extensions;
using HotPotPlayer.Models;
using HotPotPlayer.Models.CloudMusic;
using HotPotPlayer.Pages.Helper;
using HotPotPlayer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Controls
{
    public sealed partial class PlayBar : UserControlBase
    {
        public PlayBar()
        {
            this.InitializeComponent();
            PlaySlider.AddHandler(PointerReleasedEvent, new PointerEventHandler(PlaySlider_OnPointerReleased), true);
            PlaySlider.AddHandler(PointerPressedEvent, new PointerEventHandler(PlaySlider_OnPointerPressed), true);
        }

        string GetSubtitle(MusicItem music)
        {
            if (music == null)
            {
                return string.Empty;
            }
            return $"{music.GetArtists()} · {music.Album}";
        }

        Symbol GetPlayButtonSymbol(bool isPlaying, bool hasError)
        {
            if (hasError)
            {
                return Symbol.Clear;
            }
            return isPlaying ? Symbol.Pause : Symbol.Play;
        }

        double GetSliderValue(TimeSpan current, TimeSpan? total)
        {
            if (total == null)
            {
                return 0;
            }
            return 100 * current.Ticks / ((TimeSpan)total).Ticks;
        }

        string GetDuration(TimeSpan? duration)
        {
            if (duration == null)
            {
                return "--:--";
            }
            return ((TimeSpan)duration).ToString("mm\\:ss");
        }

        const string Loop = "\uE1CD";
        const string SingleLoop = "\uE1CC";
        const string Shuffle = "\uE8B1";
        string GetPlayModeIcon(PlayMode playMode)
        {
            return playMode switch
            {
                PlayMode.Loop => Loop,
                PlayMode.SingleLoop => SingleLoop,
                PlayMode.Shuffle => Shuffle,
                _ => Loop,
            };
        }

        Visibility GetCoverVisibility(bool isPlayScreenVisible)
        {
            return isPlayScreenVisible ? Visibility.Collapsed : Visibility.Visible; 
        }

        bool GetCoverLoad(bool isPlayScreenVisible)
        {
            return !isPlayScreenVisible;
        }

        private void PlayButtonClick(object sender, RoutedEventArgs e)
        {
            MusicPlayer.PlayOrPause();
        }

        private void PlaySlider_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            MusicPlayer.SuppressCurrentTimeTrigger = true;
        }

        private void PlaySlider_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            MusicPlayer.SuppressCurrentTimeTrigger = false;
            TimeSpan to = GetToTime();
            MusicPlayer.PlayTo(to);
        }

        private TimeSpan GetToTime()
        {
            if (MusicPlayer.CurrentPlayingDuration == null)
            {
                return TimeSpan.Zero;
            }
            var percent100 = (int)PlaySlider.Value;
            var v = percent100 * ((TimeSpan)MusicPlayer.CurrentPlayingDuration).Ticks / 100;
            var to = TimeSpan.FromTicks(v);
            return to;
        }

        private void PlayPreviousButtonClick(object sender, RoutedEventArgs e)
        {
            MusicPlayer.PlayPrevious();
        }

        private void PlayNextButtonClick(object sender, RoutedEventArgs e)
        {
            MusicPlayer.PlayNext();
        }


        private void PlayModeButtonClick(object sender, RoutedEventArgs e)
        {
            MusicPlayer.TogglePlayMode();
        }

        private void SubtitleClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var flyout = new MenuFlyout();

            if (MusicPlayer.CurrentPlaying is CloudMusicItem c)
            {
                var artists = c.Artists2;
                foreach (var a in artists)
                {
                    var item = new MenuFlyoutItem
                    {
                        Text = a.Name,
                        Icon = new SymbolIcon { Symbol = Symbol.Contact },
                        Tag = a
                    };
                    item.Click += AlbumHelper.ArtistClick;
                    flyout.Items.Add(item);
                }
            }
            else
            {
                var artists = MusicPlayer.CurrentPlaying.GetArtists().GetArtists();
                foreach (var a in artists)
                {
                    var item = new MenuFlyoutItem
                    {
                        Text = a,
                        Icon = new SymbolIcon { Symbol = Symbol.Contact }
                    };
                    item.Click += AlbumHelper.ArtistClick;
                    flyout.Items.Add(item);
                }
            }

            flyout.Items.Add(new MenuFlyoutSeparator());
            var item2 = new MenuFlyoutItem
            {
                Text = MusicPlayer.CurrentPlaying.Album,
                Tag = MusicPlayer.CurrentPlaying,
                Icon = new FontIcon { FontFamily = new FontFamily("Segoe Fluent Icons"), Glyph = "\uE93C" }
            };
            item2.Click += AlbumHelper.AlbumInfoClick;
            flyout.Items.Add(item2);
            button.ContextFlyout = flyout;
            button.ContextFlyout.ShowAt(button);
        }

        private Uri _coverImage;

        public Uri CoverImage
        {
            get => _coverImage;
            set => Set(ref _coverImage, value);
        }

        private void Cover_SourceGotFromCache(ImageSource obj)
        {
            CoverImage = MusicPlayer.CurrentPlaying.Cover;
        }
    }
}
