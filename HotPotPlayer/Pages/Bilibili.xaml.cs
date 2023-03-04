using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Models.BiliBili;
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Bilibili : PageBase
    {
        public Bilibili()
        {
            this.InitializeComponent();
        }

        int selectedSubPage;
        public int SelectedSubPage
        {
            get => selectedSubPage;
            set => Set(ref selectedSubPage, value, nv =>
            {
                if (nv == 1)
                {
                    BiliDynamic.LoadDynamicAsync();
                }
                else if(nv == 2)
                {
                    BiliHistory.LoadHistoryAsync();
                }
            });
        }

        bool IsFirstNavigate = true;
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (!IsFirstNavigate)
            {
                return;
            }
            if (!(await BiliBiliService.IsLoginAsync()))
            {
                NavigateTo("BilibiliSub.Login");
            }
            BiliMain.LoadPopularVideosAsync();
            IsFirstNavigate = false;
        }

        private void RefreshClick(object sender, RoutedEventArgs args)
        {
            if (SelectedSubPage == 0)
            {
                BiliMain.LoadPopularVideosAsync();
            }
            else if(SelectedSubPage == 1)
            {
                BiliDynamic.LoadDynamicAsync(true);
            }
            else if(SelectedSubPage == 2)
            {
                BiliHistory.LoadHistoryAsync(true);
            }
        }

        //private async void BVPlay(object sender, RoutedEventArgs args)
        //{
        //    var bv = BVID.Text;
        //    if (string.IsNullOrEmpty(bv))
        //    {
        //        return;
        //    }
        //    var video = (await BiliBiliService.API.GetVideoInfo(bv)).Data;
        //    NavigateTo("BilibiliSub.BiliVideoPlay", video);
        //}

        public override RectangleF[] GetTitleBarDragArea()
        {
            const float pivotHeaderWidth = 340;
            const float threeButtonWidth = 140;
            const float height = 64;

            return new RectangleF[]
            {
                new RectangleF(0, 0, 24, height),
                new RectangleF(24, 0, 260, 18),
                new RectangleF(24, 50, 260, 14), //284
                new RectangleF(284, 0, (float)((Root.ActualWidth-pivotHeaderWidth)/2 - 284), height),
                new RectangleF((float)(Root.ActualWidth/2 + pivotHeaderWidth/2), 0, (float)((Root.ActualWidth-pivotHeaderWidth)/2 - threeButtonWidth), height),
                new RectangleF((float)((Root.ActualWidth-pivotHeaderWidth)/2), 0, pivotHeaderWidth, 14),
                new RectangleF((float)((Root.ActualWidth-pivotHeaderWidth)/2), 50, pivotHeaderWidth, 14),
            };
        }

        private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var drag = GetTitleBarDragArea();
            if (drag != null) { App.SetDragRegionForTitleBar(drag); }
            
        }
    }
}
