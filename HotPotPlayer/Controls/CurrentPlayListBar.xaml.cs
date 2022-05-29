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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Controls
{
    public sealed partial class CurrentPlayListBar : UserControlBase
    {
        public CurrentPlayListBar()
        {
            this.InitializeComponent();
        }

        private void RootTapped(object sender, TappedRoutedEventArgs e)
        {
            MusicPlayer.IsPlayListBarVisible = false;
        }

        private void BackClick(object sender, RoutedEventArgs e)
        {
            MusicPlayer.IsPlayListBarVisible = false;
        }

        private void InnerTapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

    }

    public class SelectedTemplateSelector: DataTemplateSelector
    {
        public DataTemplate SelectedTemplate { get; set; }
        public DataTemplate NormalTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (container is ListViewItem cont)
            {
                if (cont.Tag != null && long.TryParse(cont.Tag.ToString(), out var token))
                {
                    cont.UnregisterPropertyChangedCallback(ListViewItem.IsSelectedProperty, token);
                }

                cont.Tag = cont.RegisterPropertyChangedCallback(ListViewItem.IsSelectedProperty, (s, e) =>
                {
                    cont.ContentTemplateSelector = null;
                    cont.ContentTemplateSelector = this;
                });

                if (cont.IsSelected)
                {
                    return SelectedTemplate;
                }

                return NormalTemplate;
            }

            return NormalTemplate;
        }
    }
}
