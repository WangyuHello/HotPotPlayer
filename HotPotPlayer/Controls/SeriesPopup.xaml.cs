using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Models;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
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
        public partial BaseItemDto SeriesInfo { get; set; }

        [ObservableProperty]
        public partial List<BaseItemDto> Seasons { get; set; }

        [ObservableProperty]
        public partial List<BaseItemDto> SelectedSeasonVideoItems { get; set; }

        [ObservableProperty]
        public partial IEnumerable<CustomChapterInfo> CustomChapters { get; set; }

        [ObservableProperty]
        public partial bool IsBackdropExpanded { get; set; }

        private static async void SeriesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var @this = (SeriesPopup)d;
            var series = e.NewValue as BaseItemDto;

            @this.SeriesInfo = await @this.JellyfinMusicService.GetItemInfoAsync(series);

            @this.SeasonSelector.Items.Clear();
            if (series.IsFolder.Value)
            {
                //Series
                @this.Seasons = await @this.JellyfinMusicService.GetSeasons(series);
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
            else
            {
                //Movie
                @this.Seasons = null;
                @this.SelectedSeasonVideoItems = null;

                @this.CustomChapters = @this.SeriesInfo.Chapters.Select((c, i) => new CustomChapterInfo
                {
                    ImageTag = c.ImageTag,
                    Index = i,
                    ParentId = @this.SeriesInfo.Id.Value,
                    Name = c.Name,
                    StartPositionTicks = c.StartPositionTicks,
                });
            }
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
            var index = SelectedSeasonVideoItems.IndexOf(video);
            VideoPlayer.PlayNext(SelectedSeasonVideoItems, index);
        }

        private void BackdropExpand_Click(object sender, RoutedEventArgs e)
        {
            IsBackdropExpanded = !IsBackdropExpanded;
            SetBackdropOffset(IsBackdropExpanded);
        }

        private void Backdrop_Tapped(object sender, TappedRoutedEventArgs e)
        {
            IsBackdropExpanded = !IsBackdropExpanded;
            SetBackdropOffset(IsBackdropExpanded);
        }

        private string GetBackdropExpandIcon(bool isBackdropExpanded)
        {
            return IsBackdropExpanded ? "\uE70E" : "\uE70D";
        }

        private Visibility GetMoviePlayVisible(BaseItemDto item)
        {
            return item.IsFolder.Value ? Visibility.Collapsed : Visibility.Visible;
        }

        private IEnumerable<string> GetVideoStreams(BaseItemDto info)
        {
            return info?.MediaSources?.SelectMany(s => s.MediaStreams.Where(m => m.Type == MediaStream_Type.Video))?.Select(s => s.DisplayTitle);
        }

        private int GetVideoStreamsSelectIndex(BaseItemDto info)
        {
            return 0;
        }

        private IEnumerable<string> GetAudioStreams(BaseItemDto info)
        {
            return info?.MediaStreams?.Where(m => m.Type == MediaStream_Type.Audio)?.Select(s => s.DisplayTitle);
        }

        private int GetAudioStreamsSelectIndex(BaseItemDto info)
        {
            return 0;
        }

        private IEnumerable<BaseItemPerson> GetDirector(List<BaseItemPerson> people)
        {
            return people.Where(p => p.Type == BaseItemPerson_Type.Director);
        }

        private Visibility GetDirectorVisible(List<BaseItemPerson> people)
        {
            return people.FirstOrDefault(p => p.Type == BaseItemPerson_Type.Director) == null ? Visibility.Collapsed : Visibility.Visible;
        }

        private IEnumerable<BaseItemPerson> GetWriter(List<BaseItemPerson> people)
        {
            return people.Where(p => p.Type == BaseItemPerson_Type.Writer);
        }

        private string GetWriterTitle(BaseItemDto series)
        {
            if (series == null)
            {
                return null;
            }
            return series.IsFolder.Value ? "作者" : "编剧";
        }

        private Visibility GetChapterVisible(BaseItemDto series)
        {
            if (series == null)
            {
                return Visibility.Collapsed;
            }
            return (series.Chapters == null || series.Chapters.Count == 0) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void SetBackdropOffset(bool isBackdropExpanded)
        {
            var backdropVisual = ElementCompositionPreview.GetElementVisual(Backdrop);
            var mainInfoVisual = ElementCompositionPreview.GetElementVisual(MainInfo);
            var leftPanelVisual = ElementCompositionPreview.GetElementVisual(LeftPanel);

            if (isBackdropExpanded)
            {
                backdropVisual.Offset = new System.Numerics.Vector3(0, 0, 0);
                mainInfoVisual.Offset = new System.Numerics.Vector3(0, 484f, 0);
                leftPanelVisual.Offset = new System.Numerics.Vector3(0, 20f, 0);

            }
            else
            {
                backdropVisual.Offset = new System.Numerics.Vector3(0, -142f, 0);
                mainInfoVisual.Offset = new System.Numerics.Vector3(0, 200f, 0);
                leftPanelVisual.Offset = new System.Numerics.Vector3(0, -40f, 0);
            }
        }
    }
}
