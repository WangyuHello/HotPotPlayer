using CommunityToolkit.WinUI;
using HotPotPlayer.Bilibili.Models.Reply;
using HotPotPlayer.Controls;
using HotPotPlayer.Controls.BilibiliSub;
using HotPotPlayer.Helpers;
using HotPotPlayer.Models.CloudMusic;
using HotPotPlayer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Richasy.BiliKernel.Models.Comment;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Pages.Helper
{
    public class CommentHelper: HelperBase
    {
        public static void ShowCommentFloor(object sender, RoutedEventArgs e)
        {
            
            var element = sender as FrameworkElement;
            var comment = element.DataContext as CloudCommentItem;
            //TODO cloudmusic
            //var music = MusicPlayer.CurrentPlaying as CloudMusicItem;

            //ObservableCollection<CloudCommentItem> l = new(await NetEaseMusicService.GetSongCommentFloorAsync(music.SId, comment.CommentId));

            //ContentDialog dialog = new()
            //{
            //    Title = $"回复({l.Count})",
            //    CloseButtonText = "关闭",
            //    DefaultButton = ContentDialogButton.Close,
            //    Content = new CommentFloorDialog(l, comment),
            //    XamlRoot = element.XamlRoot,
            //    Style = App.Resources["DefaultContentDialogStyle"] as Style
            //};

            //var result = await dialog.ShowAsync();
        }

        public static void ShowCommentInput(object sender, RoutedEventArgs e)
        {

        }

        public async static void ShowNestedReply(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            var reply = element.DataContext as CommentInformation;

            ContentDialog dialog = new()
            {
                Title = $"回复({reply.CommunityInformation.ChildCount})",
                CloseButtonText = "关闭",
                DefaultButton = ContentDialogButton.Close,
                Content = new NestedReplyDialog(reply),
                XamlRoot = element.XamlRoot,
                Style = App.Resources["DefaultContentDialogStyle"] as Style
            };
            _ = await dialog.ShowAsync();
        }
    }
}
