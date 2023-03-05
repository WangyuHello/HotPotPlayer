// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using HotPotPlayer.Bilibili.Models.Nav;
using HotPotPlayer.Bilibili.Models.Dynamic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Controls.BilibiliSub
{
    public sealed partial class Header : UserControl
    {
        public Header()
        {
            this.InitializeComponent();
        }

        public NavData NavData
        {
            get { return (NavData)GetValue(NavDataProperty); }
            set { SetValue(NavDataProperty, value); }
        }

        public static readonly DependencyProperty NavDataProperty =
            DependencyProperty.Register("NavData", typeof(NavData), typeof(Header), new PropertyMetadata(null));


        public NavStatData NavStatData
        {
            get { return (NavStatData)GetValue(NavStatDataProperty); }
            set { SetValue(NavStatDataProperty, value); }
        }

        public static readonly DependencyProperty NavStatDataProperty =
            DependencyProperty.Register("NavStatData", typeof(NavStatData), typeof(Header), new PropertyMetadata(null));


        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(Header), new PropertyMetadata(0, OnSelectedIndexChanged));

        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Header)d).SelectedIndexChanged((int)e.NewValue);
        }

        public EntranceData EntranceData
        {
            get { return (EntranceData)GetValue(EntranceDataProperty); }
            set { SetValue(EntranceDataProperty, value); }
        }

        public static readonly DependencyProperty EntranceDataProperty =
            DependencyProperty.Register("EntranceData", typeof(EntranceData), typeof(Header), new PropertyMetadata(null));

        void SelectedIndexChanged(int ind)
        {
            var headers = HeaderContainer.Children;
            for (int i = 0; i < headers.Count; i++)
            {
                var h = headers[i] as ToggleButton;
                if (h == null)
                {
                    ind++;
                    continue;
                }
                if (i == ind)
                {
                    h.IsChecked = true;
                }
                else
                {
                    h.IsChecked = false;
                }
            }
        }

        public event Action OnRefreshClick;

        private void RefreshClick(object sender, RoutedEventArgs args)
        {
            OnRefreshClick?.Invoke();
        }

        private void Avatar_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Avatar.ContextFlyout.ShowAt(Avatar);
        }

        private void HeaderItemClick(object sender, RoutedEventArgs e)
        {
            var headers = HeaderContainer.Children;
            var b = sender as UIElement;
            var index = headers.Where(u => u is ToggleButton).ToList().IndexOf(b);
            SelectedIndex = index;
            SelectedIndexChanged(index);
        }

        private void HeaderItemTapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            var headers = HeaderContainer.Children;
            var b = sender as UIElement;
            var index = headers.Where(u => u is ToggleButton).ToList().IndexOf(b);
            SelectedIndex = index;
            SelectedIndexChanged(index);
        }

        public Visibility GetDynamicEntranceVisible(EntranceData d)
        {
            if (d != null && d.UpdateInfo.Item.Count != 0)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }
    }
}
