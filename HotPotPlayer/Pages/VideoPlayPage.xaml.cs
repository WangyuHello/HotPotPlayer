using HotPotPlayer.Video;
using HotPotPlayer.Video.Models;
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

namespace HotPotPlayer.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoPlay : PageBase
    {
        public VideoPlay()
        {
            this.InitializeComponent();
        }

        public VideoPlayInfo Source
        {
            get { return (VideoPlayInfo)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(VideoPlayInfo), typeof(VideoPlay), new PropertyMetadata(default(VideoPlayInfo)));

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.VideoPlayer.IsVideoPagePresent = true;
            base.OnNavigatedTo(e);
            var source = (VideoPlayInfo)e.Parameter;
            Source = source;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.VideoPlayer.IsVideoPagePresent = false;
            base.OnNavigatingFrom(e);
            base.VideoPlayer.Stop();
            base.VideoPlayer.ShutDown();
        }

        private async void OnVideoHostLoaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            if (Source.SingleOrSeries != null)
            {
                base.VideoPlayer.PlayNext(Source.SingleOrSeries);
            }
            else if (Source.List != null)
            {
                base.VideoPlayer.PlayNext(Source.List, Source.Index);
            }
            else if (Source.Files != null)
            {
                base.VideoPlayer.PlayNext(Source.Files, Source.Index);
            }
        }

    }
}
