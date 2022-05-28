using HotPotPlayer.Controls;
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
    public static class CommentHelper
    {
        static App App => (App)Application.Current;
        static MusicPlayer Player => ((App)Application.Current).MusicPlayer;
        static NetEaseMusicService CloudMusicService => ((App)Application.Current).NetEaseMusicService;

        public async static void ShowCommentFloor(object sender, RoutedEventArgs e)
        {
            
            var element = sender as FrameworkElement;
            var comment = element.DataContext as CloudCommentItem;
            var music = Player.CurrentPlaying as CloudMusicItem;

            ObservableCollection<CloudCommentItem> l = new(await CloudMusicService.GetSongCommentFloorAsync(music.SId, comment.CommentId));

            ContentDialog dialog = new()
            {
                Title = "回复",
                CloseButtonText = "关闭",
                DefaultButton = ContentDialogButton.Close,
                Content = new CommentFloorDialog(l, comment),
                XamlRoot = App.MainWindow.Content.XamlRoot,
            };

            var result = await dialog.ShowAsync();
        }

        public static void ShowCommentInput(object sender, RoutedEventArgs e)
        {

        }
    }
}
