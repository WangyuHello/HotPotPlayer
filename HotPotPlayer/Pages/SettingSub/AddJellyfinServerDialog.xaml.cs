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

        private bool IsValid()
        {
            var t1 = Url.Text;
            var t2 = UserName.Text;
            var t3 = Password.Password;
            return (!string.IsNullOrEmpty(t1) && !string.IsNullOrEmpty(t2) && !string.IsNullOrEmpty(t3));
        }

        private void Url_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateChanged?.Invoke(IsValid());
        }

        private void UserName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateChanged?.Invoke(IsValid());
        }

        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ValidateChanged?.Invoke(IsValid());
        }
    }
}
