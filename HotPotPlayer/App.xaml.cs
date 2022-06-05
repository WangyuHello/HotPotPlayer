using HotPotPlayer.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using WinRT;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinUIEx;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media.Animation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : AppBase
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            MainWindow = new MainWindow();
            InitMainWindow(args);
            MainWindow.Activate();
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            Config.SaveSettings();
            ShutDown();
        }

        private void InitMainWindow(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            MainWindow.Title = "HotPotPlayer";
            MainWindow.CenterOnScreen(1070, 760);
            MainWindow.TrySetAcrylicBackdrop();
            MainWindow.Closed += MainWindow_Closed;

            var firstArg = args.Arguments; //尚不支持，永远为null
            var args2 = Environment.GetCommandLineArgs();
            var firstArg2 = args2.Length > 1 ? args2[1] : null;
            if (!string.IsNullOrEmpty(firstArg2) && File.Exists(firstArg2))
            {
                //InitPageName == null
                InitMediaFile = new FileInfo(firstArg2);
            }
            else
            {
                MainWindow.InitPageName = "Music";
            }

            Config.MainWindowHandle = MainWindowHandle;
        }

        public override void NavigateTo(string name, object parameter = null, NavigationTransitionInfo trans = null)
        {
            MainWindow.NavigateTo(name, parameter, trans);
        }

        public MainWindow MainWindow;
        public override IntPtr MainWindowHandle => MainWindow.GetWindowHandle();
        public override XamlRoot XamlRoot => MainWindow.Content.XamlRoot;
    }
}
