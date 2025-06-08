// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Bilibili.Models.HomeVideo;
using HotPotPlayer.Bilibili.Models.Search;
using HotPotPlayer.Extensions;
using HotPotPlayer.Models.BiliBili;
using HotPotPlayer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Richasy.BiliKernel.Models.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
        public partial SearchVideoCollection VideoSearchResult { get; set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var req = e.Parameter as SearchRequest;
            SearchBox.Text = req.Keyword;
            if (req.DoSearch)
            {
                DoSearch(req.Keyword);
            }
        }

        private void DoSearch(string keyword)
        {
            if (VideoSearchResult == null)
            {
                VideoSearchResult = new SearchVideoCollection(BiliBiliService)
                {
                    Keyword = keyword
                };
            }
            else
            {
                VideoSearchResult.Keyword = keyword;
                VideoSearchResult.Reset();
            }
        }

        private void Search_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            DoSearch(sender.Text);
        }

        private void SearchVideoClick(object sender, ItemClickEventArgs e)
        {
            var v = e.ClickedItem as VideoInformation;
            VideoPlayer.PlayNext(v.ToBaseItemDto());
        }
    }
}
