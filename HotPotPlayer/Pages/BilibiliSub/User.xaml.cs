// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Bilibili.Models.User;
using HotPotPlayer.Models.BiliBili;
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

namespace HotPotPlayer.Pages.BilibiliSub
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class User : PageBase
    {
        public User()
        {
            this.InitializeComponent();
        }

        [ObservableProperty]
        private UserCardBundle userCardBundle;

        [ObservableProperty]
        private UserVideoInfoItemCollection userVideoInfoItems;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UserCardBundle = e.Parameter as UserCardBundle;

            var userVideoInfo = (await BiliBiliService.API.GetUserVideoInfo(UserCardBundle.Card.Mid, 1, 20)).Data;
            UserVideoInfoItems = new UserVideoInfoItemCollection(userVideoInfo, UserCardBundle.Card.Mid, BiliBiliService);
        }

        private async void BiliVideoClick(object sender, ItemClickEventArgs e)
        {
            var v = e.ClickedItem as UserVideoInfoItem;
            var v2 = (await BiliBiliService.API.GetVideoInfo(v.BVid)).Data;
            NavigateTo("BilibiliSub.BiliVideoPlay", v2);
        }
    }
}
