// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Bilibili.Models.HomeVideo;
using HotPotPlayer.Bilibili.Models.Search;
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

namespace HotPotPlayer.Pages.BilibiliSub
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Search : PageBase
    {
        public Search()
        {
            this.InitializeComponent();
        }

        [ObservableProperty]
        List<SearchResultData> videoResult;

        private async void Search_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var search = await BiliBiliService.API.SearchAsync(sender.Text);
            var video = search.Data.Result.FirstOrDefault(r => r.ResultType == "video");
            VideoResult = video.Data;
        }

        private void SearchVideoClick(object sender, ItemClickEventArgs e)
        {
            var v = e.ClickedItem as SearchResultData;
            PlayVideo(v.Bvid);
        }
    }
}
