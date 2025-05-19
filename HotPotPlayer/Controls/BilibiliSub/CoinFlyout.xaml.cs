// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Controls.BilibiliSub
{
    public sealed partial class CoinFlyout : UserControl
    {
        public CoinFlyout()
        {
            this.InitializeComponent();
        }

        public bool IsOriginal
        {
            get { return (bool)GetValue(CopyRightProperty); }
            set { SetValue(CopyRightProperty, value); }
        }

        public static readonly DependencyProperty CopyRightProperty =
            DependencyProperty.Register("IsOriginal", typeof(bool), typeof(CoinFlyout), new PropertyMetadata(true));


        public event EventHandler<int> CoinConfirmed;

        private void ConfirmClick(object sender, RoutedEventArgs e)
        {
            int coin = 0;
            if(Coin1.IsChecked.HasValue && Coin1.IsChecked.Value)
            {
                coin = 1;
            }
            if (Coin2.IsChecked.HasValue && Coin2.IsChecked.Value)
            {
                coin = 2;
            }
            CoinConfirmed?.Invoke(this, coin);
        }

        private void Coin1Click(object sender, RoutedEventArgs e)
        {
            Coin2.IsChecked = false;
            if (Coin1.IsChecked.HasValue && Coin1.IsChecked.Value)
            {
                Confirm.IsEnabled = true;
            }
            else
            {
                Confirm.IsEnabled = false;
            }
        }

        private void Coin2Click(object sender, RoutedEventArgs e)
        {
            Coin1.IsChecked = false;
            if (Coin2.IsChecked.HasValue && Coin2.IsChecked.Value)
            {
                Confirm.IsEnabled = true;
            }
            else
            {
                Confirm.IsEnabled = false;
            }
        }
    }
}
