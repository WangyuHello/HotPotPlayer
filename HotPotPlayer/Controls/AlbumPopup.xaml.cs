using HotPotPlayer.Models;
using HotPotPlayer.Pages.Helper;
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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Controls
{
    public sealed partial class AlbumPopup : UserControl, INotifyPropertyChanged
    {
        public AlbumPopup()
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

        static MusicPlayer MusicPlayer => ((App)Application.Current).MusicPlayer;

        public AlbumItem Album
        {
            get { return (AlbumItem)GetValue(AlbumProperty); }
            set { SetValue(AlbumProperty, value); }
        }

        public static readonly DependencyProperty AlbumProperty =
            DependencyProperty.Register("Album", typeof(AlbumItem), typeof(AlbumPopup), new PropertyMetadata(default(AlbumItem), AlbumChanged));

        private static void AlbumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AlbumPopup)d).InitSplitButtonFlyout();
        }

        static LocalMusicService MusicService => ((App)Application.Current).LocalMusicService;

        private void InitSplitButtonFlyout()
        {
            if (AlbumSplitButton.Flyout != null)
            {
                return;
            }
            var flyout = new MenuFlyout();
            var i1 = new MenuFlyoutItem
            {
                Text = "当前列表",
                Icon = new SymbolIcon { Symbol = Symbol.MusicInfo },
            };
            i1.Click += (s, a) => AlbumHelper.AlbumAddOne(Album);
            flyout.Items.Add(i1);
            var i2 = new MenuFlyoutSeparator();
            flyout.Items.Add(i2);
            i1 = new MenuFlyoutItem
            {
                Text = "新建播放队列",
                Icon = new SymbolIcon { Symbol = Symbol.Add },
            };
            flyout.Items.Add(i1);
            foreach (var item in MusicService.LocalPlayListList)
            {
                var i = new MenuFlyoutItem
                {
                    Text = item.Title,
                    Tag = item
                };
                i.Click += (s, a) => AlbumHelper.AlbumAddToPlayList(item.Title, Album);
                flyout.Items.Add(i);
            }
            AlbumSplitButton.Flyout = flyout;
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var music = e.ClickedItem as MusicItem;
            MusicPlayer.PlayNext(music, Album);
        }
    }
}
