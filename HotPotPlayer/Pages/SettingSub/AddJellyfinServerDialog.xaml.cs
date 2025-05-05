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

namespace HotPotPlayer.Pages.SettingSub
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddJellyfinServerDialog : Page
    {
        public AddJellyfinServerDialog()
        {
            this.InitializeComponent();
        }

        public event Action<bool> ValidateChanged;

        private void Url_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = ((TextBox)sender).Text;
            if (string.IsNullOrEmpty(txt))
            {
                ValidateChanged?.Invoke(false);
                return;
            }
            var httpUrl = $"http://{txt}";
            var httpsUrl = $"http://{txt}";
            var rawUrl = txt;

            var suc1 = Uri.TryCreate(httpUrl, UriKind.RelativeOrAbsolute, out var httpUri);
            if (suc1)
            {
                ValidateChanged?.Invoke(true);
                return;
            }
            var suc2 = Uri.TryCreate(httpsUrl, UriKind.RelativeOrAbsolute, out var httpsUri);
            if (suc2)
            {
                ValidateChanged?.Invoke(true);
                return;
            }
            var suc3 = Uri.TryCreate(rawUrl, UriKind.RelativeOrAbsolute, out var rawUri);
            if (suc3)
            {
                ValidateChanged?.Invoke(true);
                return;
            }

        }

        private void UserName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
