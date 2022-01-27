using HotPotPlayer.Models;
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Controls
{
    public sealed partial class PlayBar : UserControl
    {
        public PlayBar()
        {
            this.InitializeComponent();
            PlaySlider.AddHandler(PointerReleasedEvent, new PointerEventHandler(PlaySlider_OnPointerReleased), true);
            PlaySlider.AddHandler(PointerPressedEvent, new PointerEventHandler(PlaySlider_OnPointerPressed), true);
        }

        MusicPlayer MusicPlayer => ((App)Application.Current).MusicPlayer.Value;

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

        double GetSliderValue(TimeSpan current, TimeSpan total)
        {
            return 100 * current.Ticks / total.Ticks;
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
            var percent100 = (int)PlaySlider.Value;
            var v = percent100 * MusicPlayer.CurrentPlaying.Duration.Ticks / 100;
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


        private void PlayBarListClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var music = (MusicItem)button.Tag;
            MusicPlayer.PlayNextContinue(music);
        }

        private void PlayModeButtonClick(object sender, RoutedEventArgs e)
        {
            MusicPlayer.TogglePlayMode();
        }
    }
}
