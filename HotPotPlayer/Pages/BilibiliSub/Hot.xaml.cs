using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer;
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
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Pages.BilibiliSub
{
    public sealed partial class Hot : UserControlBase
    {
        public Hot()
        {
            InitializeComponent();
        }

        [ObservableProperty]
        public partial HotVideoCollection HotVideos { get; set; }

        bool isFirstLoad = true;

        public void LoadHotVideos(bool force = false)
        {
            if (!force && !isFirstLoad)
            {
                return;
            }
            if (HotVideos == null)
            {
                HotVideos = new HotVideoCollection(BiliBiliService);
            }
            else
            {
                HotVideos.Reset();
            }
            isFirstLoad = false;
        }

        private void BiliVideoClick(object sender, ItemClickEventArgs e)
        {
            var v = e.ClickedItem as VideoInformation;
            var dto = v.ToBaseItemDto();
            VideoPlayer.PlayNext(dto);
        }
    }
}
