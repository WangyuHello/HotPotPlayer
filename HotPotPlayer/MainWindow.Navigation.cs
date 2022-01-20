using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
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
        public string InitPageName { get; set; }

        private string _selectedPageName;

        public string SelectedPageName
        {
            get => _selectedPageName;
            set => Set(ref _selectedPageName, value);
        }

        private void MainSidebar_SelectedPageNameChanged(string name)
        {
            MainFrame.Navigate(Type.GetType("HotPotPlayer.Pages." + name), null, new DrillInNavigationTransitionInfo());
        }

        private void MainSidebar_OnBackClick()
        {
            var topName = MainFrame.BackStack.Last().SourcePageType.Name;
            MainFrame.GoBack();
            SelectedPageName = topName;
        }

        public void NavigateInit()
        {
            if (string.IsNullOrEmpty(InitPageName))
            {
                return;
            }
            MainSidebar_SelectedPageNameChanged(InitPageName);
            SelectedPageName = InitPageName;
        }


    }
}
