using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Controls;
using HotPotPlayer.Bilibili.Models.Search;
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

        [ObservableProperty]
        private BaseItemDto selectedMusicLibraryDto;

        [ObservableProperty]
        private List<BaseItemDto> musicLibraryDto;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var info = await JellyfinMusicService.GetPublicSystemInfo();
            if (info == null) return;
            JellyfinServers = [info];
            MusicLibraryDto = JellyfinMusicService.MusicLibraryDto;
            SelectedMusicLibraryDto = JellyfinMusicService.SelectedMusicLibraryDto;
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
            ContentDialog dialog = new()
            {
                XamlRoot = App.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "确认删除应用数据？",
                PrimaryButtonText = "确认",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                FontFamily = Application.Current.Resources["MiSansNormal"] as Microsoft.UI.Xaml.Media.FontFamily,
            };
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await ApplicationData.Current.ClearAsync();
                App.ShowToast(new ToastInfo
                {
                    Text = "已删除应用数据"
                });
            }
        }

        private async void ClearConfigClick(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = App.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "确认删除应用配置？",
                PrimaryButtonText = "确认",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                FontFamily = Application.Current.Resources["MiSansNormal"] as Microsoft.UI.Xaml.Media.FontFamily,
            };
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Config.ResetSettings();
                App.ShowToast(new ToastInfo
                {
                    Text = "已删除应用配置"
                });
            }
        }

        private async void ClearCacheClick(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = App.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "确认删除应用缓存？",
                PrimaryButtonText = "确认",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                FontFamily = Application.Current.Resources["MiSansNormal"] as Microsoft.UI.Xaml.Media.FontFamily,
            };
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
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
                App.ShowToast(new ToastInfo
                {
                    Text = "已删除应用缓存"
                });
            }
        }

        private async void SettingCardClick(object sender, RoutedEventArgs e)
        {
            var b = sender as SettingsCard;
            var server = b.Tag as PublicSystemInfo;
            bool _ = await Windows.System.Launcher.LaunchUriAsync(new Uri(server.LocalAddress));
        }

        private void MusicLibrary_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private async void AddJellyfinServer(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();

            dialog.XamlRoot = App.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "添加Jellyfin服务器";
            dialog.PrimaryButtonText = "测试并保存";
            dialog.CloseButtonText = "取消";
            dialog.DefaultButton = ContentDialogButton.Primary;
            var dialogContent = new AddJellyfinServerDialog();
            dialogContent.ValidateChanged += ValidateChanged;
            dialog.Content = dialogContent;
            dialog.IsPrimaryButtonEnabled = false;
            dialog.FontFamily = Application.Current.Resources["MiSansNormal"] as Microsoft.UI.Xaml.Media.FontFamily;

            var result = await dialog.ShowAsync();

            void ValidateChanged(bool valid)
            {
                dialog.IsPrimaryButtonEnabled = valid;
            }

            if (result == ContentDialogResult.Primary)
            {
                if (string.IsNullOrEmpty(dialogContent.Url.Text) ||
                    string.IsNullOrEmpty(dialogContent.UserName.Text) ||
                    string.IsNullOrEmpty(dialogContent.Password.Password))
                {
                    return;
                }

                var prefix = dialogContent.UrlPrefix.SelectedIndex switch
                {
                    0 => "http://",
                    1 => "https://",
                    _ => "http://",
                };

                var (info, msg) = await JellyfinMusicService.TryGetSystemInfoPublicAsync(prefix + dialogContent.Url.Text);
                if (info == null)
                {
                    App.ShowToast(new ToastInfo { Text = msg });
                    return;
                }

                var (loginResult, message) = await JellyfinMusicService.TryLoginAsync(prefix + dialogContent.Url.Text, dialogContent.UserName.Text, dialogContent.Password.Password);
                if (!loginResult)
                {
                    App.ShowToast(new ToastInfo { Text = message });
                    return;
                }

                App.ShowToast(new ToastInfo { Text = "登录成功" });
                Config.SetConfig("JellyfinUrl", prefix + dialogContent.Url.Text);
                Config.SetConfig("JellyfinUserName", dialogContent.UserName.Text);
                Config.SetConfig("JellyfinPassword", dialogContent.Password.Password);

                JellyfinMusicService.Reset();

                JellyfinServers = [await JellyfinMusicService.GetPublicSystemInfo()];
                MusicLibraryDto = JellyfinMusicService.MusicLibraryDto;
                SelectedMusicLibraryDto = JellyfinMusicService.SelectedMusicLibraryDto;
            }

            dialogContent.ValidateChanged -= ValidateChanged;
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
