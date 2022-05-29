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
    public sealed partial class AlbumPopup : UserControlBase
    {
        public AlbumPopup()
        {
            this.InitializeComponent();
        }

        public AlbumItem Album
        {
            get { return (AlbumItem)GetValue(AlbumProperty); }
            set { SetValue(AlbumProperty, value); }
        }

        public static readonly DependencyProperty AlbumProperty =
            DependencyProperty.Register("Album", typeof(AlbumItem), typeof(AlbumPopup), new PropertyMetadata(default(AlbumItem), AlbumChanged));

        private static void AlbumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var @this = (AlbumPopup)d;
            //AlbumHelper.InitSplitButtonFlyout(@this.AlbumSplitButton, @this.Album);
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var music = e.ClickedItem as MusicItem;
            MusicPlayer.PlayNext(music, Album);
        }
    }
}
