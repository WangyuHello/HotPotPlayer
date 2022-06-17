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
using Windows.UI;
using Windows.Graphics;

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
                Root.Loaded += AppTitleBar_Loaded;
                Root.SizeChanged += AppTitleBar_SizeChanged;
                MusicPlayer.PropertyChanged += MusicPlayer_PropertyChanged;
            }
            else
            {
                // Title bar customization using these APIs is currently
                // supported only on Windows 11. In other cases, hide
                // the custom title bar element.
                //AppTitleBar.Visibility = Visibility.Collapsed;

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
                    visual.Offset = new Vector3(-280, 0, 0);
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
                        //AppTitleBar.Visibility = Visibility.Collapsed;
                        sender.TitleBar.ResetToDefault();
                        break;

                    case AppWindowPresenterKind.FullScreen:
                        // Full screen - hide the custom title bar
                        // and the default system title bar.
                        //AppTitleBar.Visibility = Visibility.Collapsed;
                        sender.TitleBar.ExtendsContentIntoTitleBar = true;
                        break;

                    case AppWindowPresenterKind.Overlapped:
                        // Normal - hide the system title bar
                        // and use the custom title bar instead.
                        //AppTitleBar.Visibility = Visibility.Visible;
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

        public void SetDragRegionForCustomTitleBar(AppWindow appWindow = null, List<(double x1, double x2)> dragRegionExcept = null)
        {
            appWindow ??= m_AppWindow;
            if (AppWindowTitleBar.IsCustomizationSupported() && appWindow.TitleBar.ExtendsContentIntoTitleBar)
            {
                double scaleAdjustment = Root.XamlRoot.RasterizationScale;

                //RightPaddingColumn.Width = new GridLength(appWindow.TitleBar.RightInset / scaleAdjustment);

                List<RectInt32> dragRectsList = new();

                if (dragRegionExcept == null)
                {
                    RectInt32 rect;
                    rect.X = (int)(60 * scaleAdjustment);
                    rect.Y = 0;
                    rect.Height = (int)(42 * scaleAdjustment);
                    rect.Width = (int)((Root.ActualWidth - 60 - 300) * scaleAdjustment);
                    dragRectsList.Add(rect);
                }
                else
                {
                    RectInt32 first;
                    first.X = (int)(60 * scaleAdjustment);
                    first.Y = 0;
                    first.Height = (int)(42 * scaleAdjustment);
                    first.Width = (int)(dragRegionExcept[0].x1 * scaleAdjustment);
                    dragRectsList.Add(first);

                    if (dragRegionExcept.Count > 1)
                    {
                        for (int i = 0; i < dragRegionExcept.Count - 1; i++)
                        {
                            dragRectsList.Add(new RectInt32
                            {
                                X = (int)((60 + dragRegionExcept[0].x2) * scaleAdjustment),
                                Y = 0,
                                Height = (int)(42 * scaleAdjustment),
                                Width = (int)((dragRegionExcept[1].x1 - dragRegionExcept[0].x2) * scaleAdjustment)
                            });
                        }
                    }

                    RectInt32 last;
                    last.X = (int)((60 + dragRegionExcept.Last().x2) * scaleAdjustment);
                    last.Y = 0;
                    last.Height = (int)(42 * scaleAdjustment);
                    last.Width = (int)((Root.ActualWidth - 60 - dragRegionExcept.Last().x2 - 300) * scaleAdjustment);
                    dragRectsList.Add(last);
                }


                RectInt32[] dragRects = dragRectsList.ToArray();

                appWindow.TitleBar.SetDragRectangles(dragRects);
            }
        }

        WindowsSystemDispatcherQueueHelper m_wsdqHelper; // See separate sample below for implementation
        MicaController m_micaController;
        DesktopAcrylicController m_acrylicController;
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
                m_micaController.FallbackColor = Color.FromArgb(255, 0xf1, 0xf3, 0xf6);
                //m_micaController.TintColor = Color.FromArgb(255, 0xf1, 0xf3, 0xf6);
                //m_micaController.TintOpacity = 0.8f;
                //m_micaController.LuminosityOpacity = 0.65f;
                return true; // succeeded
            }

            return false; // Mica is not supported on this system
        }

        public bool TrySetAcrylicBackdrop()
        {
            if (DesktopAcrylicController.IsSupported())
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

                m_acrylicController = new DesktopAcrylicController();

                // Enable the system backdrop.
                // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
                m_acrylicController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                m_acrylicController.SetSystemBackdropConfiguration(m_configurationSource);
                m_acrylicController.FallbackColor = Color.FromArgb(255, 0xf1, 0xf3, 0xf6);
                m_acrylicController.TintColor = Color.FromArgb(255, 0xde, 0xe3, 0xee);
                m_acrylicController.TintOpacity = 0.98f;
                m_acrylicController.LuminosityOpacity = 0.5f;
                return true; // succeeded
            }

            return false; // Acrylic is not supported on this system
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
            if (m_acrylicController != null)
            {
                m_acrylicController.Dispose();
                m_acrylicController = null;
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
