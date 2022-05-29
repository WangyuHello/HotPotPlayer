using HotPotPlayer.Models;
using HotPotPlayer.Models.CloudMusic;
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

namespace HotPotPlayer.Pages.CloudMusicSub
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

        private List<CloudSearchHotItem> _searchHotItems;
        public List<CloudSearchHotItem> SearchHotItems
        {
            get => _searchHotItems;
            set => Set(ref _searchHotItems, value);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SearchHotItems = await NetEaseMusicService.SearchHotDetailAsync();
        }

        private async void Search_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput || args.Reason == AutoSuggestionBoxTextChangeReason.ProgrammaticChange)
            {
                var songs = await NetEaseMusicService.SearchSongAsync(sender.Text);
                sender.ItemsSource = songs;
            }
        }

        private async void Search_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var songs = await NetEaseMusicService.SearchSongAsync(sender.Text);
            sender.ItemsSource = songs;
        }

        private void Search_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var music = args.SelectedItem as MusicItem;
            MusicPlayer.PlayNext(music);
        }

        private void SearchHotGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var hot = e.ClickedItem as CloudSearchHotItem;
            SearchBox.Text = hot.SearchWord;
        }
    }
}
