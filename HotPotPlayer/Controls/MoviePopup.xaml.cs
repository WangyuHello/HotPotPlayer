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
    public sealed partial class MoviePopup : UserControlBase
    {
        public MoviePopup()
        {
            this.InitializeComponent();
        }

        public BaseItemDto Movie
        {
            get { return (BaseItemDto)GetValue(MovieProperty); }
            set { SetValue(MovieProperty, value); }
        }

        public static readonly DependencyProperty MovieProperty =
            DependencyProperty.Register("Movie", typeof(BaseItemDto), typeof(MoviePopup), new PropertyMetadata(default(BaseItemDto), MovieChanged));

        [ObservableProperty]
        private BaseItemDto movieInfo;

        private static async void MovieChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var @this = (MoviePopup)d;
            var movie = e.NewValue as BaseItemDto;
            if (movie.IsFolder.Value) return;

            @this.MovieInfo = await @this.JellyfinMusicService.GetItemInfoAsync(movie);
        }

    }
}
