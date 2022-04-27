using HotPotPlayer.Models;
using HotPotPlayer.Services.FFmpeg;
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
    public sealed partial class Info : Page, INotifyPropertyChanged
    {
        public Info()
        {
            this.InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Set<T>(ref T oldValue, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
            {
                oldValue = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private MusicItem _music;

        public MusicItem Music
        {
            get => _music;
            set => Set(ref _music, value);
        }

        private string _mediaInfo;

        public string MediaInfo
        {
            get => _mediaInfo;
            set => Set(ref _mediaInfo, value);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Music = e.Parameter as MusicItem;
            base.OnNavigatedTo(e);
            MediaInfo = await Task.Run(() =>
            {
                return MediaInfoHelper.GetAudioInfo(Music.Source);
            });
        }

        private async void OpenFileClick(object sender, RoutedEventArgs e)
        {
            var path = Music.Source.Directory;
            await Launcher.LaunchFolderPathAsync(path.FullName);
        }
    }
}
