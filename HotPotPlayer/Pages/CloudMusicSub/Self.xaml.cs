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
using System.IO;
using System.Linq;
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
    public sealed partial class Self : Page
    {
        public Self()
        {
            this.InitializeComponent();
        }
        NetEaseMusicService CloudMusicService => ((App)Application.Current).NetEaseMusicService;
        MusicPlayer Player => ((App)Application.Current).MusicPlayer;

        bool IsFirstNavigate = true;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (!IsFirstNavigate)
            {
                return;
            }

            await CloudMusicService.InitUserAsync();

            IsFirstNavigate = false;
        }

        private void LikeList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var m = e.ClickedItem as MusicItem;
            Player.PlayNext(m, CloudMusicService.LikeList);
        }
    }
}
