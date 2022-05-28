using HotPotPlayer.Models;
using HotPotPlayer.Models.CloudMusic;
using HotPotPlayer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
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

namespace HotPotPlayer.Pages.CloudMusicSub
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Self : PageBase
    {
        public Self()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private CloudPlayListItem _selectedPlayList;
        public CloudPlayListItem SelectedPlayList
        {
            get => _selectedPlayList;
            set => Set(ref _selectedPlayList, value);
        }

        private void LikeList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var m = e.ClickedItem as MusicItem;
            MusicPlayer.PlayNext(m, NetEaseMusicService.LikeList);
        }

        private async void PlayLists_ItemClick(object sender, ItemClickEventArgs e)
        {
            var m = e.ClickedItem as CloudPlayListItem;
            SelectedPlayList = await NetEaseMusicService.GetPlayListAsync(m.PlId);
            PlayListPopupOverlay.Visibility = Visibility.Visible;
        }

        private void PlayListPopupOverlay_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //var anim = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backwardsAnimation2", PlayListPopupTarget);
            //anim.Configuration = new BasicConnectedAnimationConfiguration();
            //await PlayListGridView.TryStartConnectedAnimationAsync(anim, SelectedPlayList, "PlayListCardConnectedElement");
            PlayListPopupOverlay.Visibility = Visibility.Collapsed;
        }
    }
}
