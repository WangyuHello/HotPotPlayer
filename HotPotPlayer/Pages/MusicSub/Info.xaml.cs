using HotPotPlayer.Models;
using HotPotPlayer.Pages.CloudMusicSub;
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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Pages.MusicSub
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Info : PageBase
    {
        public Info()
        {
            this.InitializeComponent();
        }

        private BaseItemDto _music;
        public BaseItemDto Music
        {
            get => _music;
            set => Set(ref _music, value);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var origin = e.Parameter as BaseItemDto;
            Music = await JellyfinMusicService.GetMusicInfoAsync(origin);
            base.OnNavigatedTo(e);
        }

        private void OpenFileClick(object sender, RoutedEventArgs e)
        {
            //var path = Music.Source.Directory;
            //await Launcher.LaunchFolderPathAsync(path.FullName);
        }

        private string GetAlbumArtists(List<NameGuidPair> artists)
        {
            return string.Join(", ", artists.Select(a => a.Name));
        }

        private string GetArtists(List<string> artists)
        {
            return string.Join(", ", artists);
        }

        private string GetGenres(List<string> geners)
        {
            return string.Join(", ", geners);
        }

        private string GetSampleRate(List<MediaStream> streams)
        {
            var audioStream = streams.FirstOrDefault();
            if (audioStream != null)
            {
                return audioStream.SampleRate.ToString();
            }
            return string.Empty;
        }
        private string GetBitDepth(List<MediaStream> streams)
        {
            var audioStream = streams.FirstOrDefault();
            if (audioStream != null)
            {
                return audioStream.BitDepth.ToString();
            }
            return string.Empty;
        }
        private string GetBitRate(List<MediaStream> streams)
        {
            var audioStream = streams.FirstOrDefault();
            if (audioStream != null)
            {
                return audioStream.BitRate.ToString();
            }
            return string.Empty;
        }

        private string GetFilePath(List<MediaSourceInfo> sources) 
        { 
            var file = sources.FirstOrDefault();
            if (file != null) 
            {
                return file.Path;
            }
            return string.Empty;
        }
    }
}
