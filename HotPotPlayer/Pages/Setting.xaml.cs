using HotPotPlayer.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Setting : Page, INotifyPropertyChanged
    {
        public Setting()
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

        public bool GetLibraryWarningVisible(ObservableCollection<LibraryItem> lib)
        {
            //return true;
            return lib == null;
        }

        static App App => (App)Application.Current;

        static ConfigBase Config => App.Config;

        private bool _isVideoLibraryWarningVisible;

        public bool IsVideoLibraryWarningVisible
        {
            get => _isVideoLibraryWarningVisible;
            set => Set(ref _isVideoLibraryWarningVisible, value);
        }

        private ObservableCollection<LibraryItem> _musicLibrary;

        public ObservableCollection<LibraryItem> MusicLibrary
        {
            get => _musicLibrary;
            set => Set(ref _musicLibrary, value);
        }

        private ObservableCollection<LibraryItem> _videoLibrary;

        public ObservableCollection<LibraryItem> VideoLibrary
        {
            get => _videoLibrary;
            set => Set(ref _videoLibrary, value);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var musicLib = Config.MusicLibrary;
            if (musicLib != null)
            {
                MusicLibrary = new ObservableCollection<LibraryItem>(musicLib);
            }
            var videoLib = Config.VideoLibrary;
            if (videoLib != null)
            {
                VideoLibrary = new ObservableCollection<LibraryItem>(videoLib);
            }
        }

        private async void LaunchMusicSettingClick(object sender, RoutedEventArgs e)
        {
            bool _ = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-musiclibrary"));
        }

        private async void LaunchVideoSettingClick(object sender, RoutedEventArgs e)
        {
            bool _ = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-videos"));
        }

        private async void OpenInstalledLocationClick(object sender, RoutedEventArgs e)
        {
            var loc = Windows.ApplicationModel.Package.Current.InstalledLocation;
            bool _ = await Windows.System.Launcher.LaunchUriAsync(new Uri(loc.Path));
        }

        private async void OpenDataLocationClick(object sender, RoutedEventArgs e)
        {
            var loc = Config.LocalFolder;
            bool _ = await Windows.System.Launcher.LaunchUriAsync(new Uri(loc));
        }

        private async void ClearDataClick(object sender, RoutedEventArgs e)
        {
            await ApplicationData.Current.ClearAsync();

            ContentDialog dialog = new();
            dialog.Title = "清理完成";
            dialog.PrimaryButtonText = "确定";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.XamlRoot = XamlRoot;

            var _ = await dialog.ShowAsync();
        }

        private void ClearConfigClick(object sender, RoutedEventArgs e)
        {
            Config.ResetSettings();
        }

        private async void AddVideoLibrary(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, App.MainWindowHandle);

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                var path = folder.Path;
                if (VideoLibrary == null)
                {
                    VideoLibrary = new ObservableCollection<LibraryItem>
                    {
                        new LibraryItem {Path = path}
                    };
                    Config.VideoLibrary = VideoLibrary.Select(s => s).ToList();
                }
                if (!VideoLibrary.Where(s => s.Path == path).Any())
                {
                    VideoLibrary.Add(new LibraryItem { Path = path });
                    Config.VideoLibrary = VideoLibrary.Select(s => s).ToList();
                }
            }
        }

        private async void AddMusicLibrary(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, App.MainWindowHandle);

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                var path = folder.Path;
                if (MusicLibrary == null)
                {
                    MusicLibrary = new ObservableCollection<LibraryItem>
                    {
                        new LibraryItem {Path = path}
                    };
                    Config.MusicLibrary = MusicLibrary.Select(s => s).ToList();
                }
                if (!MusicLibrary.Where(s => s.Path == path).Any())
                {
                    MusicLibrary.Add(new LibraryItem { Path = path });
                    Config.MusicLibrary = MusicLibrary.Select(s => s).ToList();
                }
            }
        }

        private void MusicRemoveClick(object sender, RoutedEventArgs e)
        {
            var item = ((Button)sender).Tag as LibraryItem;
            MusicLibrary.Remove(item);
            Config.MusicLibrary = MusicLibrary.Select(s => s).ToList();
        }

        private void VideoRemoveClick(object sender, RoutedEventArgs e)
        {
            var item = ((Button)sender).Tag as LibraryItem;
            VideoLibrary.Remove(item);
            Config.VideoLibrary = VideoLibrary.Select(s => s).ToList();
        }

        private void ReloadVideoLibrary(object sender, RoutedEventArgs e)
        {
            App.LocalVideoService.StartLoadLocalVideo();
        }

        private void ReloadMusicLibrary(object sender, RoutedEventArgs e)
        {
            App.LocalMusicService.StartLoadLocalMusic();
        }
    }
}
