using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer
{
    public sealed partial class MainWindow : Window
    {
        private void BackClick(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                var topName = MainFrame.BackStack.Last().SourcePageType.Name;
                StartPopAnimation(topName);
                SelectedPageName = topName;
                MainFrame.GoBack();
            }
        }

        private string _SelectedPageName;
        public string SelectedPageName
        {
            get => _SelectedPageName;
            set => Set(ref _SelectedPageName, value);
        }

        private bool GetEnableState(string name, string selectedName)
        {
            return name != selectedName;
        }

        private Visibility GetPopVisibility(string selected)
        {
            if (!string.IsNullOrEmpty(selected))
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        private void NavigateClick(object sender, RoutedEventArgs e)
        {
            var b = sender as Button;
            if (b.Name != SelectedPageName)
            {
                StartPopAnimation(b);
                MainFrame.Navigate(Type.GetType("HotPotPlayer.Pages." + b.Name));
                SelectedPageName = b.Name;
            }
        }

        public void NavigateInit(string page)
        {
            //StartPopAnimation(Setting);
            MainFrame.Navigate(Type.GetType("HotPotPlayer.Pages."+page));
            SelectedPageName = page;
        }
    }
}
