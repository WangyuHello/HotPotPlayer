using HotPotPlayer.Models;
using HotPotPlayer.Services;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WinUIEx;
using WinRT;

namespace HotPotPlayer
{
    public sealed partial class MainWindow
    {
        private AppWindow m_AppWindow;

        public void SetAppTitleBar()
        {
            m_AppWindow = this.GetAppWindow();
            m_AppWindow.Changed += AppWindow_Changed;
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                var titleBar = m_AppWindow.TitleBar;
                titleBar.ExtendsContentIntoTitleBar = true;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                titleBar.ButtonForegroundColor = Colors.Black;
                AppTitleBar.Loaded += AppTitleBar_Loaded;
                AppTitleBar.SizeChanged += AppTitleBar_SizeChanged;
                MusicPlayer.PropertyChanged += MusicPlayer_PropertyChanged;
            }
            else
            {
                // Title bar customization using these APIs is currently
                // supported only on Windows 11. In other cases, hide
                // the custom title bar element.
                AppTitleBar.Visibility = Visibility.Collapsed;

                // Show alternative UI for any functionality in
                // the title bar, such as search.
            }
        }

        private void MusicPlayer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsPlayListBarVisible")
            {
                var musicPlayer = (MusicPlayer)sender;
                if (musicPlayer.IsPlayListBarVisible)
                {
                    m_AppWindow.TitleBar.ButtonForegroundColor = Colors.White;
                    var visual = ElementCompositionPreview.GetElementVisual(ContentRoot);
                    visual.Scale = new Vector3(0.8f, 0.8f, 1);
                    visual.Offset = new Vector3(-300, 0, 0);
                }
                else
                {
                    m_AppWindow.TitleBar.ButtonForegroundColor = Colors.Black;
                    var visual = ElementCompositionPreview.GetElementVisual(ContentRoot);
                    visual.Scale = new Vector3(1, 1, 1);
                    visual.Offset = new Vector3(0, 0, 0);
                }
            }
            else if(e.PropertyName == "IsPlayScreenVisible")
            {
                var musicPlayer = (MusicPlayer)sender;
                SetDragRegionForCustomTitleBar(m_AppWindow, !musicPlayer.IsPlayScreenVisible);
            }

        }

        private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (AppWindowTitleBar.IsCustomizationSupported()
                && m_AppWindow.TitleBar.ExtendsContentIntoTitleBar)
            {
                // Update drag region if the size of the title bar changes.
                SetDragRegionForCustomTitleBar(m_AppWindow);
            }
        }

        private void AppTitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                SetDragRegionForCustomTitleBar(m_AppWindow);
            }
        }

        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
        {
            if (args.DidPresenterChange
                && AppWindowTitleBar.IsCustomizationSupported())
            {
                switch (sender.Presenter.Kind)
                {
                    case AppWindowPresenterKind.CompactOverlay:
                        // Compact overlay - hide custom title bar
                        // and use the default system title bar instead.
                        AppTitleBar.Visibility = Visibility.Collapsed;
                        sender.TitleBar.ResetToDefault();
                        break;

                    case AppWindowPresenterKind.FullScreen:
                        // Full screen - hide the custom title bar
                        // and the default system title bar.
                        AppTitleBar.Visibility = Visibility.Collapsed;
                        sender.TitleBar.ExtendsContentIntoTitleBar = true;
                        break;

                    case AppWindowPresenterKind.Overlapped:
                        // Normal - hide the system title bar
                        // and use the custom title bar instead.
                        AppTitleBar.Visibility = Visibility.Visible;
                        sender.TitleBar.ExtendsContentIntoTitleBar = true;
                        SetDragRegionForCustomTitleBar(sender);
                        break;

                    default:
                        // Use the default system title bar.
                        sender.TitleBar.ResetToDefault();
                        break;
                }
            }
        }

        private void SetDragRegionForCustomTitleBar(AppWindow appWindow, bool isSearchVisible = true)
        {
            if (AppWindowTitleBar.IsCustomizationSupported()
                && appWindow.TitleBar.ExtendsContentIntoTitleBar)
            {
                double scaleAdjustment = Root.XamlRoot.RasterizationScale;

                RightPaddingColumn.Width = new GridLength(appWindow.TitleBar.RightInset / scaleAdjustment);

                List<Windows.Graphics.RectInt32> dragRectsList = new();

                if (isSearchVisible)
                {
                    Windows.Graphics.RectInt32 dragRectL;
                    dragRectL.X = (int)(60 * scaleAdjustment);
                    dragRectL.Y = 0;
                    dragRectL.Height = (int)(AppTitleBar.ActualHeight * scaleAdjustment);
                    dragRectL.Width = (int)(LeftDragColumn.ActualWidth * scaleAdjustment);
                    dragRectsList.Add(dragRectL);

                    Windows.Graphics.RectInt32 dragRectR;
                    dragRectR.X = (int)((60
                                        + LeftDragColumn.ActualWidth
                                        + SearchColumn.ActualWidth) * scaleAdjustment);
                    dragRectR.Y = 0;
                    dragRectR.Height = (int)(AppTitleBar.ActualHeight * scaleAdjustment);
                    dragRectR.Width = (int)(RightDragColumn.ActualWidth * scaleAdjustment);
                    dragRectsList.Add(dragRectR);
                }
                else
                {
                    Windows.Graphics.RectInt32 dragRectL;
                    dragRectL.X = (int)(60 * scaleAdjustment);
                    dragRectL.Y = 0;
                    dragRectL.Height = (int)(AppTitleBar.ActualHeight * scaleAdjustment);
                    dragRectL.Width = (int)(AppTitleBar.ActualWidth * scaleAdjustment - appWindow.TitleBar.RightInset);
                    dragRectsList.Add(dragRectL);
                }
                

                Windows.Graphics.RectInt32[] dragRects = dragRectsList.ToArray();

                appWindow.TitleBar.SetDragRectangles(dragRects);
            }
        }

        double GetTitleBarSearchVisible(bool isPlayScreenVisible)
        {
            return isPlayScreenVisible ? 0 : 1;
        }

        private async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var musics = await MusicService.QueryMusicAsync(sender.Text);
                sender.ItemsSource = musics;
            }
            else
            {
 
            }
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var music = args.SelectedItem as MusicItem;
            NavigateTo("MusicSub.Info", music, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            sender.ItemsSource = null;
        }


        WindowsSystemDispatcherQueueHelper m_wsdqHelper; // See separate sample below for implementation
        MicaController m_micaController;
        SystemBackdropConfiguration m_configurationSource;

        public bool TrySetMicaBackdrop()
        {
            if (MicaController.IsSupported())
            {
                m_wsdqHelper = new WindowsSystemDispatcherQueueHelper();
                m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

                // Hooking up the policy object
                m_configurationSource = new SystemBackdropConfiguration();
                this.Activated += Window_Activated;
                this.Closed += Window_Closed;

                // Initial configuration state.
                m_configurationSource.IsInputActive = true;
                switch (((FrameworkElement)this.Content).ActualTheme)
                {
                    case ElementTheme.Dark: m_configurationSource.Theme = SystemBackdropTheme.Dark; break;
                    case ElementTheme.Light: m_configurationSource.Theme = SystemBackdropTheme.Light; break;
                    case ElementTheme.Default: m_configurationSource.Theme = SystemBackdropTheme.Default; break;
                }

                m_micaController = new MicaController();

                // Enable the system backdrop.
                // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
                m_micaController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                m_micaController.SetSystemBackdropConfiguration(m_configurationSource);
                return true; // succeeded
            }

            return false; // Mica is not supported on this system
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            // Make sure any Mica/Acrylic controller is disposed so it doesn't try to
            // use this closed window.
            if (m_micaController != null)
            {
                m_micaController.Dispose();
                m_micaController = null;
            }
            this.Activated -= Window_Activated;
            m_configurationSource = null;
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            m_configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
        }
    }

    class WindowsSystemDispatcherQueueHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        struct DispatcherQueueOptions
        {
            internal int dwSize;
            internal int threadType;
            internal int apartmentType;
        }

        [DllImport("CoreMessaging.dll")]
        private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object dispatcherQueueController);

        object m_dispatcherQueueController = null;
        public void EnsureWindowsSystemDispatcherQueueController()
        {
            if (Windows.System.DispatcherQueue.GetForCurrentThread() != null)
            {
                // one already exists, so we'll just use it.
                return;
            }

            if (m_dispatcherQueueController == null)
            {
                DispatcherQueueOptions options;
                options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
                options.threadType = 2;    // DQTYPE_THREAD_CURRENT
                options.apartmentType = 2; // DQTAT_COM_STA

                CreateDispatcherQueueController(options, ref m_dispatcherQueueController);
            }
        }
    }
}
