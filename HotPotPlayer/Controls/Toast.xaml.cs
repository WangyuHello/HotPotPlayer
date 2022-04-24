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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Controls
{
    public sealed partial class Toast : UserControl
    {
        public Toast()
        {
            this.InitializeComponent();
        }

        public ToastInfo ToastInfo
        {
            get { return (ToastInfo)GetValue(ToastInfoProperty); }
            set { SetValue(ToastInfoProperty, value); }
        }

        public static readonly DependencyProperty ToastInfoProperty =
            DependencyProperty.Register("ToastInfo", typeof(ToastInfo), typeof(Toast), new PropertyMetadata(default(ToastInfo)));


        private void Dismiss(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).MainWindow.DismissToast();
        }
    }
}
