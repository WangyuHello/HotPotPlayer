using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Models;
using HotPotPlayer.Models.CloudMusic;
using HotPotPlayer.Services;
using Jellyfin.Sdk.Generated.Models;
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
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Controls
{
    public sealed partial class PlayListPopup : UserControlBase
    {
        public PlayListPopup()
        {
            this.InitializeComponent();
        }

        public BaseItemDto PlayList
        {
            get { return (BaseItemDto)GetValue(PlayListProperty); }
            set { SetValue(PlayListProperty, value); }
        }

        public static readonly DependencyProperty PlayListProperty =
            DependencyProperty.Register("PlayList", typeof(BaseItemDto), typeof(PlayListPopup), new PropertyMetadata(default(BaseItemDto)));

        public BaseItemDto PlayListInfo
        {
            get { return (BaseItemDto)GetValue(PlayListInfoProperty); }
            set { SetValue(PlayListInfoProperty, value); }
        }

        public static readonly DependencyProperty PlayListInfoProperty =
            DependencyProperty.Register("PlayListInfo", typeof(BaseItemDto), typeof(PlayListPopup), new PropertyMetadata(default(BaseItemDto)));

        public List<BaseItemDto> PlayListMusicItems
        {
            get { return (List<BaseItemDto>)GetValue(PlayListMusicItemsProperty); }
            set { SetValue(PlayListMusicItemsProperty, value); }
        }

        public static readonly DependencyProperty PlayListMusicItemsProperty =
            DependencyProperty.Register("PlayListMusicItems", typeof(List<BaseItemDto>), typeof(PlayListPopup), new PropertyMetadata(default(List<BaseItemDto>)));

        string GetDescription(BaseItemDto p)
        {
            //if (p is CloudPlayListItem c)
            //{
            //    return c.Description;
            //}
            return p.Overview;
        }
        private void PlayListPlay(object sender, RoutedEventArgs e)
        {
            MusicPlayer.PlayNext(PlayListMusicItems);
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var music = e.ClickedItem as BaseItemDto;
            MusicPlayer.PlayNext(music, PlayListMusicItems);
        }

        bool coverOpened = false;
        private void Cover_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CoverHeight.Height = new GridLength(coverOpened ? 200 : 320);
            coverOpened = !coverOpened;
        }
    }
}
