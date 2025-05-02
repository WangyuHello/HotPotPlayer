using DirectN.Extensions.Utilities;
using HotPotPlayer.Interop.Helper;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using WinUIEx;

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
            mainWindow = new MainWindow();
            InitMainWindow(args);
            MainWindow.Activate();
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            WindowHelper.GetWindowRect(MainWindowHandle, out WindowHelper.MyRect windowRect);
            var scale = WindowHelper.GetDpiForWindow(MainWindowHandle) / 96d;
            var width = Convert.ToInt32((windowRect.Right - windowRect.Left) / scale);
            var height = Convert.ToInt32((windowRect.Bottom - windowRect.Top) / scale);

            Config.SetConfig("width", width);
            Config.SetConfig("height", height);
            Config.SetConfig("initpage", MainWindow.GetSavePageName());
            Config.SaveSettings();
            ShutDown();
        }

        private void InitMainWindow(LaunchActivatedEventArgs args)
        {
            MainWindow.Title = "HotPotPlayer";
            var width = Config.GetConfig("width", 1420);
            var height = Config.GetConfig("height", 1100);
            MainWindow.CenterOnScreen(width, height);
            MainWindow.TrySetAcrylicBackdrop();
            MainWindow.Closed += MainWindow_Closed;
            MainWindow.SizeChanged += MainWindow_SizeChanged;
            SetIconAsync();

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
                MainWindow.InitPageName = Config.GetConfig("initpage", "Music");
            }

            Config.MainWindowHandle = MainWindowHandle;
        }

        private async void SetIconAsync()
        {
            Uri uri = new("ms-appx:///Assets/icon.ico");
            StorageFile storageFile = null;
            try
            {
                storageFile = await StorageFile.GetFileFromApplicationUriAsync(uri);
            }
            catch (Exception)
            {
                // Use default icon.
            }

            if (storageFile is not null)
            {
                AppWindow.SetIcon(storageFile.Path);
            }
        }

        private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            bounds = mainWindow.Bounds;
        }

        public override void NavigateTo(string name, object parameter = null, NavigationTransitionInfo trans = null)
        {
            MainWindow.NavigateTo(name, parameter, trans);
        }

        public override void NavigateBack(bool force = false)
        {
            MainWindow.NavigateBack(force);
        }

        public override void SetDragRegionForTitleBar(RectangleF[] dragArea)
        {
            mainWindow.SetDragRegionForCustomTitleBar(dragArea);
        }

        private MainWindow mainWindow;
        private Rect bounds;
        public override MainWindow MainWindow => mainWindow;
        public override IntPtr MainWindowHandle => MainWindow.GetWindowHandle();
        public override Rect Bounds => bounds;
        public override XamlRoot XamlRoot => MainWindow.Content.XamlRoot;
        public override AppWindow AppWindow => MainWindow.AppWindow;
    }
}
