// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Bilibili.Models.HomeVideo;
using HotPotPlayer.Bilibili.Models.Video;
using HotPotPlayer.Models.BiliBili;
using HotPotPlayer.Services;
using HotPotPlayer.Video;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinUIEx;

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
        private PopularVideoCollection popularVideos;

        [ObservableProperty]
        private RecVideoCollection recVideos;

        public async void LoadPopularVideosAsync()
        {
            var popularVideos = (await BiliBiliService.API.GetPopularVideo()).Data;
            PopularVideos = new PopularVideoCollection(popularVideos, BiliBiliService);
            var recVideos = (await BiliBiliService.API.GetRecVideo()).Data;
            RecVideos = new RecVideoCollection(recVideos, BiliBiliService);
        }

        private void BiliVideoClick(object sender, ItemClickEventArgs e)
        {
            var v = e.ClickedItem as HomeDataItem;
            PlayVideo(v.Bvid);
        }
    }
}
