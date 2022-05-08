using HotPotPlayer.Models;
using HotPotPlayer.Services;
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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CloudMusic : Page, INotifyPropertyChanged
    {
        public CloudMusic()
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
        NetEaseMusicService CloudMusicService => ((App)Application.Current).NetEaseMusicService;
        MusicPlayer MusicPlayer => ((App)Application.Current).MusicPlayer;
        MainWindow MainWindow => ((App)Application.Current).MainWindow;

        private string _nickName;

        public string NickName
        {
            get => _nickName;
            set => Set(ref _nickName, value);
        }

        private ObservableCollection<CloudMusicItem> _likeList;

        public ObservableCollection<CloudMusicItem> LikeList
        {
            get => _likeList;
            set => Set(ref _likeList, value);
        }
        bool IsFirstNavigate = true;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (!IsFirstNavigate)
            {
                return;
            }
            if (!await CloudMusicService.IsLoginAsync())
            {
                MainWindow.NavigateTo("CloudMusicSub.Login");
            }

            var (uid, name) = await CloudMusicService.GetUidAsync();
            NickName = name;
            var likeList = await CloudMusicService.GetLikeListAsync();
            LikeList ??= new ObservableCollection<CloudMusicItem>();
            LikeList.Clear();
            foreach (var item in likeList)
            {
                LikeList.Add(item);
            }
            IsFirstNavigate = false;
        }

        private void MusicItemClick(object sender, RoutedEventArgs e)
        {
            var music = ((Button)sender).Tag as CloudMusicItem;
            MusicPlayer.PlayNext(music);
        }
    }
}
