using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Models;
using HotPotPlayer.Pages.Helper;
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
    public sealed partial class AlbumPopup : UserControlBase
    {
        public AlbumPopup()
        {
            this.InitializeComponent();
        }

        public BaseItemDto Album
        {
            get { return (BaseItemDto)GetValue(AlbumProperty); }
            set { SetValue(AlbumProperty, value); }
        }

        public static readonly DependencyProperty AlbumProperty =
            DependencyProperty.Register("Album", typeof(BaseItemDto), typeof(AlbumPopup), new PropertyMetadata(default(BaseItemDto)));

        public List<BaseItemDto> AlbumMusicItems
        {
            get { return (List<BaseItemDto>)GetValue(AlbumMusicItemsProperty); }
            set { SetValue(AlbumMusicItemsProperty, value); }
        }

        public static readonly DependencyProperty AlbumMusicItemsProperty =
            DependencyProperty.Register("AlbumMusicItems", typeof(List<BaseItemDto>), typeof(AlbumPopup), new PropertyMetadata(default(List<BaseItemDto>)));

        public BaseItemDto AlbumInfo
        {
            get { return (BaseItemDto)GetValue(AlbumInfoProperty); }
            set { SetValue(AlbumInfoProperty, value); }
        }

        public static readonly DependencyProperty AlbumInfoProperty =
            DependencyProperty.Register("AlbumInfo", typeof(BaseItemDto), typeof(AlbumPopup), new PropertyMetadata(default(BaseItemDto)));

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var music = e.ClickedItem as BaseItemDto;
            MusicPlayer.PlayNext(music, Album);
        }

        bool coverOpened = false;
        private void Cover_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CoverHeight.Height = new GridLength(coverOpened ? 200 : 320);
            coverOpened = !coverOpened;
        }
    }
}
