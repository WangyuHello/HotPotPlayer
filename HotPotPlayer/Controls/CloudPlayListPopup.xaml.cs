using HotPotPlayer.Models;
using HotPotPlayer.Models.CloudMusic;
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
    public sealed partial class CloudPlayListPopup : UserControlBase
    {
        public CloudPlayListPopup()
        {
            this.InitializeComponent();
        }

        public PlayListItem PlayList
        {
            get { return (PlayListItem)GetValue(PlayListProperty); }
            set { SetValue(PlayListProperty, value); }
        }

        public static readonly DependencyProperty PlayListProperty =
            DependencyProperty.Register("PlayList", typeof(PlayListItem), typeof(PlayListPopup), new PropertyMetadata(default(PlayListItem)));


        string GetDescription(PlayListItem p)
        {
            if (p is CloudPlayListItem c)
            {
                return c.Description;
            }
            return string.Empty;
        }
        private void PlayListPlay(object sender, RoutedEventArgs e)
        {
            MusicPlayer.PlayNext(PlayList);
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var music = e.ClickedItem as MusicItem;
            MusicPlayer.PlayNext(music, PlayList);
        }
    }
}
