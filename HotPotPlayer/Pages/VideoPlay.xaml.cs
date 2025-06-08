using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
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
        public partial VideoPlayerView View { get; set; }

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

        [ObservableProperty]
        public partial int SelectedEpisode { get; set; }

        private async void OnCurrentPlayingChanged(BaseItemDto @new)
        {
            if (CurrentPlaying.Etag == "Bilibili")
            {
                IsFullPageHost = false;
                View = BiliBiliService.GetVideoInfoFromCache(@new.PlaylistItemId);
                Video = View.Information;
                LikeButton.IsChecked = View.Operation.IsLiked;
                CoinButton.IsChecked = View.Operation.IsCoined;
                FavorButton.IsChecked = View.Operation.IsFavorited;
                if (RelatedVideos == null)
                {
                    RelatedVideos = new ObservableCollection<VideoInformation>(View.Recommends);
                }
                else
                {
                    RelatedVideos.Clear();
                    RelatedVideos.AddRange(View.Recommends);
                }
                if(Tags == null)
                {
                    Tags = new ObservableCollection<BiliTag>(View.Tags);
                }
                else
                {
                    Tags.Clear();
                    Tags.AddRange(View.Tags);
                }
                OnLineCount = await BiliBiliService.GetOnlineViewerAsync(@new.PlaylistItemId, @new.ProgramId);
                Replies = new ReplyItemCollection(BiliBiliService)
                {
                    Oid = @new.PlaylistItemId,
                    Type = Richasy.BiliKernel.Models.CommentTargetType.Video,
                    Sort = Richasy.BiliKernel.Models.CommentSortType.Hot
                };
                sortSelectGuard = true;
                CommentSortSelector.SelectedItem = CommentSortHot;
                sortSelectGuard = false;

                if (View.Seasons != null)
                {
                    DetermineSelectedEpisode();
                }
            }
            else
            {
                IsFullPageHost = true;
            }
        }

        private async void DetermineSelectedEpisode()
        {
            for (int i = 0; i < View.Seasons[0].Videos.Count; i++)
            {
                if (View.Seasons[0].Videos[i].Identifier.Id == Video.Identifier.Id)
                {
                    selectedEpisodeGurad = true;
                    SelectedEpisode = i;
                    selectedEpisodeGurad = false;
                    await UgcSeasonList.SmoothScrollIntoViewWithIndexAsync(i, itemPlacement: ScrollItemPlacement.Center);
                    break;
                }
            }
        }

        bool selectedEpisodeGurad;

        partial void OnSelectedEpisodeChanged(int value)
        {
            if (selectedEpisodeGurad)
            {
                return;
            }
            if (value == -1)
            {
                return;
            }
            var sel = View.Seasons[0].Videos[value];
            VideoPlayer.PlayNext(sel.ToBaseItemDto());
        }

        private void UserAvatar_Tapped(object sender, TappedRoutedEventArgs e)
        {
            UserAvatarFlyout.LoadUserCardBundle();
            UserAvatar.ContextFlyout.ShowAt(UserAvatar);
        }

        private void RelateVideoClick(object sender, ItemClickEventArgs e)
        {
            var v = e.ClickedItem as VideoInformation;
            VideoPlayer.PlayNext(v.ToBaseItemDto());
        }

        private void TagClick(object sender, ItemClickEventArgs e)
        {
            var v = e.ClickedItem as BiliTag;
            VideoPlayer.VisualState = VideoPlayVisualState.TinyHidden;
            Task.Run(VideoPlayer.PauseAsStop);
            NavigateTo("BilibiliSub.Search", new SearchRequest
            {
                Keyword = v.Name,
                DoSearch = true
            });
        }

        private async void LikeClick(object sender, RoutedEventArgs e)
        {
            var b = sender as ToggleButton;
            try
            {
                await BiliBiliService.ToggleVideoLikeAsync(Video.Identifier.Id, b.IsChecked ?? false);
            }
            catch (Exception)
            {
                b.IsChecked = !b.IsChecked;
            }
        }
        private void CoinClick(object sender, RoutedEventArgs e)
        {
            var b = sender as ToggleButton;
            b.ContextFlyout.ShowAt(b);
            b.IsChecked = !b.IsChecked; //保持按钮按下前状态
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
            ShareFl.Init();
            var b = sender as FrameworkElement;
            b.ContextFlyout.ShowAt(b);
        }

        private async void CoinConfirmClick(object sender, int c)
        {
            CoinButton.ContextFlyout.Hide();
            try
            {
                await BiliBiliService.CoinVideoAsync(Video.Identifier.Id, c, false);
                CoinButton.IsChecked = true;
            }
            catch (Exception)
            {
                CoinButton.IsChecked = false;
            }
        }

        bool sortSelectGuard = false;
        private void CommentSortSelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            if (sortSelectGuard) return;
            if (Replies == null) { return; }
            SelectorBarItem selectedItem = sender.SelectedItem;
            int currentSelectedIndex = sender.Items.IndexOf(selectedItem);
            Replies = new ReplyItemCollection(BiliBiliService)
            {
                Oid = Video.Identifier.Id,
                Type = Richasy.BiliKernel.Models.CommentTargetType.Video,
                Sort = currentSelectedIndex == 0 ? Richasy.BiliKernel.Models.CommentSortType.Hot : Richasy.BiliKernel.Models.CommentSortType.Time
            };
        }

        Visibility GetSeasonVisible(VideoPlayerView view)
        {
            if(view == null) return Visibility.Collapsed;
            var isSeason = view.Seasons != null && view.Seasons.Count > 0;
            return isSeason ? Visibility.Visible : Visibility.Collapsed;
        }

        string GetSelectedEpisodeAndAll(int selectedEpisode, VideoPlayerView view)
        {
            if (view?.Seasons == null)
            {
                return "-";
            }
            return $"({selectedEpisode + 1}/{view.Seasons[0].Videos.Count})";
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
