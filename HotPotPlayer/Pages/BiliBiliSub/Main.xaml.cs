// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Models.BiliBili;
using HotPotPlayer.Services.BiliBili.HomeVideo;
using HotPotPlayer.Services.BiliBili.Video;
using HotPotPlayer.Video;
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
    public sealed partial class Main : UserControlBase
    {
        public Main()
        {
            this.InitializeComponent();
        }

        [ObservableProperty]
        private PopularVideos popularVideos;

        [ObservableProperty]
        private HomeData recVideos;

        public async void LoadPopularVideosAsync()
        {
            PopularVideos = (await BiliBiliService.API.GetPopularVideo()).Data;
            RecVideos = (await BiliBiliService.API.GetRecVideo()).Data;
        }

        private async void BiliVideoClick(object sender, ItemClickEventArgs e)
        {
            var v = e.ClickedItem as HomeDataItem;
            var v2 = (await BiliBiliService.API.GetVideoInfo(v.Bvid)).Data;
            NavigateTo("BilibiliSub.BiliVideoPlay", v2);
        }
    }
}
