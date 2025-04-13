using CommunityToolkit.Mvvm.ComponentModel;
using Jellyfin.Sdk.Generated.Models;
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
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Controls
{
    public sealed partial class SeriesPopup : UserControlBase
    {
        public SeriesPopup()
        {
            this.InitializeComponent();
        }

        public BaseItemDto Series
        {
            get { return (BaseItemDto)GetValue(SeriesProperty); }
            set { SetValue(SeriesProperty, value); }
        }

        public static readonly DependencyProperty SeriesProperty =
            DependencyProperty.Register("Series", typeof(BaseItemDto), typeof(SeriesPopup), new PropertyMetadata(default(BaseItemDto), SeriesChanged));

        [ObservableProperty]
        private BaseItemDto seriesInfo;

        [ObservableProperty]
        private List<BaseItemDto> seasons;

        [ObservableProperty]
        private List<BaseItemDto> selectedSeasonVideoItems;

        private static async void SeriesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var @this = (SeriesPopup)d;
            var series = e.NewValue as BaseItemDto;
            if (!series.IsFolder.Value) return;

            @this.SeriesInfo = await @this.JellyfinMusicService.GetItemInfoAsync(series);
            @this.Seasons = await @this.JellyfinMusicService.GetSeasons(series);

            @this.SeasonSelector.Items.Clear();
            int i = 0;
            foreach (var season in @this.Seasons)
            {
                @this.SeasonSelector.Items.Add(new SelectorBarItem
                {
                    IsSelected = i == 0,
                    Text = season.Name,
                    FontSize = 16,
                    FontFamily = (FontFamily)Application.Current.Resources["MiSansRegular"]
                });
                i++;
            }
            @this.SelectedSeasonVideoItems = await @this.JellyfinMusicService.GetEpisodes(@this.Seasons[0]);
        }

        private async void SeasonSelector_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            SelectorBarItem selectedItem = sender.SelectedItem;
            if (selectedItem == null) { return; }
            int currentSelectedIndex = sender.Items.IndexOf(selectedItem);

            var season = Seasons[currentSelectedIndex];
            SelectedSeasonVideoItems = await JellyfinMusicService.GetEpisodes(season);
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var video = e.ClickedItem as BaseItemDto;
        }
    }
}
