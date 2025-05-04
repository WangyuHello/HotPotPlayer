using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Controls;
using HotPotPlayer.Models;
using HotPotPlayer.Pages.SettingSub;
using HotPotPlayer.Services;
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
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
    public sealed partial class Setting : PageBase
    {
        public Setting()
        {
            this.InitializeComponent();
        }

        [ObservableProperty]
        private ObservableCollection<PublicSystemInfo> jellyfinServers;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await App.JellyfinMusicService.JellyfinLoginAsync();
            JellyfinServers = [App.JellyfinMusicService.SystemInfo];
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

        private void ClearCacheClick(object sender, RoutedEventArgs e)
        {
            var cache = Config.CacheFolder;
            var di = new DirectoryInfo(cache);
            var temp = di.Parent.GetDirectories("TempState").FirstOrDefault();
            if (temp == null) return;
            var sub = temp.GetDirectories();
            foreach (var item in sub)
            {
                item.Delete(true);
            }
        }

        private async void SettingCardClick(object sender, RoutedEventArgs e)
        {
            var b = sender as SettingsCard;
            var server = b.Tag as PublicSystemInfo;
            bool _ = await Windows.System.Launcher.LaunchUriAsync(new Uri(server.LocalAddress));
        }

        private async void AddJellyfinServer(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = App.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "添加Jellyfin服务器";
            dialog.PrimaryButtonText = "保存";
            dialog.CloseButtonText = "取消";
            dialog.DefaultButton = ContentDialogButton.Primary;
            var dialogContent = new AddJellyfinServerDialog();
            dialog.Content = dialogContent;
            dialog.IsPrimaryButtonEnabled = false;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                if (string.IsNullOrEmpty(dialogContent.Url.Text) ||
                    string.IsNullOrEmpty(dialogContent.UserName.Text) ||
                    string.IsNullOrEmpty(dialogContent.Password.Password))
                {
                    return;
                }
                Config.SetConfig("JellyfinUrl", dialogContent.Url.Text);
                Config.SetConfig("JellyfinUserName", dialogContent.UserName.Text);
                Config.SetConfig("JellyfinPassword", dialogContent.Password.Password);
            }
            //var folderPicker = new FolderPicker
            //{
            //    SuggestedStartLocation = PickerLocationId.ComputerFolder
            //};
            //WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, App.MainWindowHandle);

            //var folder = await folderPicker.PickSingleFolderAsync();
            //if (folder != null)
            //{
            //    var path = folder.Path;
            //    if (MusicLibrary == null)
            //    {
            //        MusicLibrary = new ObservableCollection<LibraryItem>
            //        {
            //            new LibraryItem {Path = path}
            //        };
            //        Config.MusicLibrary = MusicLibrary.Select(s => s).ToList();
            //    }
            //    if (!MusicLibrary.Where(s => s.Path == path).Any())
            //    {
            //        MusicLibrary.Add(new LibraryItem { Path = path });
            //        Config.MusicLibrary = MusicLibrary.Select(s => s).ToList();
            //    }
            //}
        }

        private void JellyfinServerRemoveClick(object sender, RoutedEventArgs e)
        {

        }

        public override RectangleF[] GetTitleBarDragArea()
        {
            return new RectangleF[]
            {
                new RectangleF(0, 0, (float)(ActualWidth), 28),
            };
        }
        private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var drag = GetTitleBarDragArea();
            if (drag != null) { App.SetDragRegionForTitleBar(drag); }
        }
    }
}
