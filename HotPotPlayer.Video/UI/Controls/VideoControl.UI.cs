using HotPotPlayer.Bilibili.Models.Danmaku;
using HotPotPlayer.Models;
using HotPotPlayer.Services;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Video.UI.Controls
{
    public partial class VideoControl
    {
        #region UIConv
        public Visibility IsDmVisible(DMData data)
        {
            return data != null && data.Dms != null && data.Dms.Any() ? Visibility.Visible : Visibility.Collapsed;
        }

        string GetPlayButtonIcon(bool isPlaying, bool hasError)
        {
            if (hasError)
            {
                return "\uE106";
            }
            return isPlaying ? "\uE103" : "\uF5B0";
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
            var dur = (TimeSpan)duration;
            if (dur.Hours > 0)
            {
                return dur.ToString("hh\\:mm\\:ss");
            }
            return dur.ToString("mm\\:ss");
        }

        public Visibility GetTitleBarVisible(bool playBarVisible, bool isFullPage)
        {
            return (playBarVisible && isFullPage) ? Visibility.Visible : Visibility.Collapsed;
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

        private Visibility GetPlayModeButtonVisible(bool isFullPageHost)
        {
            return isFullPageHost ? Visibility.Visible : Visibility.Collapsed;
        }

        string GetFullScreenIcon(bool isFullScreen)
        {
            return isFullScreen ? "\uE1D8" : "\uE1D9";
        }

        string GetFullPageIcon(bool isFullPage)
        {
            return isFullPage ? "\uE744" : "\uE9A6";
        }

        private Visibility GetIndicatorVisible(string selectedDef)
        {
            return selectedDef switch
            {
                "HDR 真彩" => Visibility.Visible,
                "4K 超清" => Visibility.Visible,
                "杜比视界" => Visibility.Visible,
                "8K 超高清" => Visibility.Visible,
                _ => Visibility.Collapsed
            };
        }

        private string GetIndicator(string selectedDef)
        {
            return selectedDef switch
            {
                "HDR 真彩" => "HDR",
                "4K 超清" => "4K",
                "杜比视界" => "HDR",
                "8K 超高清" => "8K",
                _ => ""
            };
        }
        #endregion
    }
}
