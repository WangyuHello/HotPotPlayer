using HotPotPlayer.Bilibili.Models.Reply;
using HotPotPlayer.Controls;
using HotPotPlayer.Controls.BilibiliSub;
using HotPotPlayer.Helpers;
using HotPotPlayer.Models.CloudMusic;
using HotPotPlayer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
        public async static void ShowCommentFloor(object sender, RoutedEventArgs e)
        {
            
            var element = sender as FrameworkElement;
            var comment = element.DataContext as CloudCommentItem;
            var music = MusicPlayer.CurrentPlaying as CloudMusicItem;

            ObservableCollection<CloudCommentItem> l = new(await NetEaseMusicService.GetSongCommentFloorAsync(music.SId, comment.CommentId));

            ContentDialog dialog = new()
            {
                Title = $"回复({l.Count})",
                CloseButtonText = "关闭",
                DefaultButton = ContentDialogButton.Close,
                Content = new CommentFloorDialog(l, comment),
                XamlRoot = XamlRoot,
                Style = App.Resources["DefaultContentDialogStyle"] as Style
            };

            var result = await dialog.ShowAsync();
        }

        public static void ShowCommentInput(object sender, RoutedEventArgs e)
        {

        }

        public async static void ShowNestedReply(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            var reply = element.DataContext as Reply;

            ContentDialog dialog = new()
            {
                Title = $"回复({reply.TheReplies.Count})",
                CloseButtonText = "关闭",
                DefaultButton = ContentDialogButton.Close,
                Content = new NestedReplyDialog(reply),
                XamlRoot = XamlRoot,
                Style = App.Resources["DefaultContentDialogStyle"] as Style
            };

            var result = await dialog.ShowAsync();
        }
    }
}
