// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HotPotPlayer.Models;
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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Video.UI.Controls
{
    public sealed partial class VideoPlayListBar : UserControlBase
    {
        public VideoPlayListBar()
        {
            this.InitializeComponent();
        }

        public ObservableCollection<BaseItemDto> CurrentPlayList
        {
            get { return (ObservableCollection<BaseItemDto>)GetValue(CurrentPlayListProperty); }
            set { SetValue(CurrentPlayListProperty, value); }
        }

        public static readonly DependencyProperty CurrentPlayListProperty =
            DependencyProperty.Register("CurrentPlayList", typeof(ObservableCollection<BaseItemDto>), typeof(VideoPlayListBar), new PropertyMetadata(default(ObservableCollection<BaseItemDto>)));


        public int CurrentPlayIndex
        {
            get { return (int)GetValue(CurrentPlayIndexProperty); }
            set { SetValue(CurrentPlayIndexProperty, value); }
        }

        public static readonly DependencyProperty CurrentPlayIndexProperty =
            DependencyProperty.Register("CurrentPlayIndex", typeof(int), typeof(VideoPlayListBar), new PropertyMetadata(0));


        public event Action OnDismiss;
        public event ItemClickEventHandler PlayListItemClicked;

        private void RootTapped(object sender, TappedRoutedEventArgs e)
        {
            OnDismiss?.Invoke();
        }

        private void BackClick(object sender, RoutedEventArgs e)
        {
            OnDismiss?.Invoke();
        }

        private void InnerTapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void PlayList_ItemClick(object sender, ItemClickEventArgs e)
        {
            PlayListItemClicked?.Invoke(sender, e);
        }
    }
}
