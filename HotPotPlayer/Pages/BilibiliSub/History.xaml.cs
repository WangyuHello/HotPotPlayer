// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Bilibili.Models.Dynamic;
using HotPotPlayer.Bilibili.Models.History;
using HotPotPlayer.Bilibili.Models.HomeVideo;
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
    public sealed partial class History : UserControlBase
    {
        public History()
        {
            this.InitializeComponent();
        }

        [ObservableProperty]
        public partial HistoryCollection HistoryItems { get; set; }

        bool isFirstLoad = true;
        public async void LoadHistoryAsync(bool force = false)
        {
            if (!force && !isFirstLoad)
            {
                return;
            }
            var data = (await BiliBiliService.API.History()).Data;
            HistoryItems = new HistoryCollection(data, BiliBiliService);
            isFirstLoad = false;
        }

        private void BiliHistoryClick(object sender, ItemClickEventArgs e)
        {
            var v = e.ClickedItem as HistoryItem;
            NavigateTo("BilibiliSub.BiliVideoPlay", v.History.Bvid);
        }
    }
}
