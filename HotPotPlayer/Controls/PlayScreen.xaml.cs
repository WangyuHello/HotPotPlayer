using HotPotPlayer.Models.CloudMusic;
using HotPotPlayer.Services;
using Microsoft.UI.Input;
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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Controls
{
    public sealed partial class PlayScreen : UserControl
    {
        public PlayScreen()
        {
            this.InitializeComponent();
            MusicPlayer.PropertyChanged += MusicPlayer_PropertyChanged;
        }

        private async void MusicPlayer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            MusicPlayer m = (MusicPlayer)sender;
            if (e.PropertyName == "CurrentPlaying" || e.PropertyName == "IsPlayScreenVisible")
            {
                if (m.IsPlayScreenVisible && m.CurrentPlaying != null && m.CurrentPlaying is CloudMusicItem c)
                {
                    await CloudMusicService.GetSongCommentAsync(c.SId);
                }
            }
        }

        NetEaseMusicService CloudMusicService => ((App)Application.Current).NetEaseMusicService;
        MusicPlayer MusicPlayer => ((App)Application.Current).MusicPlayer;

        private void PlayScreen_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint currentPoint = e.GetCurrentPoint(Root);
            if (currentPoint.PointerDeviceType == PointerDeviceType.Mouse)
            {
                PointerPointProperties pointerProperties = currentPoint.Properties;
                if (pointerProperties.PointerUpdateKind == PointerUpdateKind.XButton1Released)
                {
                    MusicPlayer.HidePlayScreen();
                }
            }
            e.Handled = true;
        }
    }
}
