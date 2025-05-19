using CommunityToolkit.Mvvm.ComponentModel;
using DirectN.Extensions.Utilities;
using HotPotPlayer.Bilibili.Models.Video;
using HotPotPlayer.Extensions;
using HotPotPlayer.Models.BiliBili;
using HotPotPlayer.Pages.BilibiliSub;
using HotPotPlayer.Services;
using HotPotPlayer.Video;
using HotPotPlayer.Video.Models;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Richasy.BiliKernel.Http;
using Richasy.BiliKernel.Models.Base;
using Richasy.BiliKernel.Models.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoPlay : PageBase
    {
        public VideoPlay()
        {
            IsTabStop = true;
            this.InitializeComponent();
            KeyDown += TheHost.OnKeyDown;
            //Focus(FocusState.Programmatic);
            //LostFocus += (s, arg) => { Focus(FocusState.Programmatic); };
        }

        public BaseItemDto CurrentPlaying
        {
            get { return (BaseItemDto)GetValue(CurrentPlayingProperty); }
            set { SetValue(CurrentPlayingProperty, value); }
        }

        public static readonly DependencyProperty CurrentPlayingProperty =
            DependencyProperty.Register("CurrentPlaying", typeof(BaseItemDto), typeof(VideoPlay), new PropertyMetadata(default, CurrentPlayingChanged));

        private static void CurrentPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var page = (VideoPlay)d;
            page.OnCurrentPlayingChanged(e.NewValue as  BaseItemDto);
        }

        [ObservableProperty]
        public partial bool IsFullPageHost { get; set; }

        [ObservableProperty]
        public partial VideoInformation Video { get; set; }

        [ObservableProperty]
        public partial string OnLineCount {  get; set; }

        [ObservableProperty]
        public partial ObservableCollection<VideoInformation> RelatedVideos { get; set; }

        [ObservableProperty]
        public partial ReplyItemCollection Replies { get; set; }

        [ObservableProperty]    
        public partial ObservableCollection<BiliTag> Tags { get; set; }

        [ObservableProperty]
        public partial bool IsAdditionLoading { get; set; }

        private async void OnCurrentPlayingChanged(BaseItemDto @new)
        {
            if (CurrentPlaying.Etag == "Bilibili")
            {
                IsFullPageHost = false;
                var view = BiliBiliService.GetVideoInfoFromCache(@new.PlaylistItemId);
                Video = view.Information;
                LikeButton.IsChecked = view.Operation.IsLiked;
                CoinButton.IsChecked = view.Operation.IsCoined;
                FavorButton.IsChecked = view.Operation.IsFavorited;
                if (RelatedVideos == null)
                {
                    RelatedVideos = new ObservableCollection<VideoInformation>(view.Recommends);
                }
                else
                {
                    RelatedVideos.Clear();
                    RelatedVideos.AddRange(view.Recommends);
                }
                if(Tags == null)
                {
                    Tags = new ObservableCollection<BiliTag>(view.Tags);
                }
                else
                {
                    Tags.Clear();
                    Tags.AddRange(view.Tags);
                }
                OnLineCount = await BiliBiliService.GetOnlineViewerAsync(@new.PlaylistItemId, @new.ProgramId);
                Replies = new ReplyItemCollection(BiliBiliService)
                {
                    Oid = @new.PlaylistItemId,
                    Type = Richasy.BiliKernel.Models.CommentTargetType.Video
                };
            }
            else
            {
                IsFullPageHost = true;
            }
        }

        private void UserAvatar_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //UserAvatarFlyout.LoadUserCardBundle();
            //UserAvatar.ContextFlyout.ShowAt(UserAvatar);
        }

        private void RelateVideoClick(object sender, ItemClickEventArgs e)
        {
            var v = e.ClickedItem as VideoInformation;
            VideoPlayer.PlayNext(v.ToBaseItemDto());
        }

        private void TagClick(object sender, ItemClickEventArgs e)
        {
            //var v = e.ClickedItem as Tag;
            //NavigateTo("BilibiliSub.Search", new SearchRequest
            //{
            //    Keyword = v.Name,
            //    DoSearch = true
            //});
        }

        private async void LikeClick(object sender, RoutedEventArgs e)
        {
            var b = sender as ToggleButton;
            await BiliBiliService.ToggleVideoLikeAsync(Video.Identifier.Id, b.IsChecked ?? false);
            //var r = await BiliBiliService.API.Like(aid, bvid, !IsLike);
            //if (r.Code == 0)
            //{
            //    IsLike = !IsLike;
            //    if (IsLike)
            //    {
            //        Likes++;
            //    }
            //    else
            //    {
            //        Likes--;
            //    }
            //}
        }
        private void CoinClick(object sender, RoutedEventArgs e)
        {
            var b = sender as ToggleButton;
            b.ContextFlyout.ShowAt(b);
            b.IsChecked = false;
        }

        private void FavorClick(object sender, RoutedEventArgs e)
        {
            //var r = await BiliBiliService.API.Favor(aid);
            //if (r)
            //{
            //    IsFavor = true;
            //    if (IsFavor)
            //    {
            //        Favors++;
            //    }
            //    else
            //    {
            //        Favors--;
            //    }
            //}
            //var b = sender as ToggleButton;
            //b.IsChecked = IsFavor;
        }

        private void ShareClick(object sender, RoutedEventArgs e)
        {
            //ShareFl.Init();
            //var b = sender as FrameworkElement;
            //b.ContextFlyout.ShowAt(b);
        }

        private async void CoinConfirmClick(object sender, int c)
        {
            CoinButton.ContextFlyout.Hide();
            await BiliBiliService.CoinVideoAsync(Video.Identifier.Id, c, false);
            CoinButton.IsChecked = true;
        }

        Visibility IsSingleStaff(VideoInformation video)
        {
            if (video == null) return Visibility.Collapsed;
            var isSingle = video.Collaborators == null || video.Collaborators.Count == 0;
            return isSingle ? Visibility.Visible : Visibility.Collapsed;
        }

        Visibility GetIsMultiStaff(VideoInformation video)
        {
            if (video == null) return Visibility.Collapsed;
            var ismulti = video.Collaborators != null && video.Collaborators.Any();
            return ismulti ? Visibility.Visible : Visibility.Collapsed;
        }

        bool GetIsOriginal(VideoInformation video)
        {
            if (video == null) return true;
            video.ExtensionData.TryGetValue("IsOriginal", out var i);
            if (i is bool b) return b;
            return true;
        }

        string GetDescription(VideoInformation video)
        {
            if (video == null) return string.Empty;
            video.ExtensionData.TryGetValue("Description", out var desc);
            return desc.ToString();
        }

        GridLength GetCommentWidth(VideoPlayVisualState state)
        {
            return state != VideoPlayVisualState.FullHost ? new GridLength(0) : new GridLength(400);
        }
        GridLength GetTitleHeight(VideoPlayVisualState state)
        {
            return state != VideoPlayVisualState.FullHost ? new GridLength(0) : new GridLength(0, GridUnitType.Auto);
        }

        Thickness GetRootPadding(VideoPlayVisualState state)
        {
            return state != VideoPlayVisualState.FullHost ? new Thickness(0) : new Thickness(36, 28, 28, 0);
        }
    }
}
