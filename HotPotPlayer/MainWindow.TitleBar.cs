using HotPotPlayer.Extensions;
using HotPotPlayer.Services;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Windows.Graphics;

namespace HotPotPlayer
{
    public sealed partial class MainWindow
    {
        private AppWindow m_AppWindow;

        public void SetAppTitleBar()
        {
            m_AppWindow = this.AppWindow;
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
                var musicPlayer = (MusicPlayerService)sender;
                m_AppWindow.SetTitleBarForegroundColor(musicPlayer.IsPlayListBarVisible);
                if (musicPlayer.IsPlayListBarVisible)
                {
                    var visual = ElementCompositionPreview.GetElementVisual(ContentRoot);
                    visual.Scale = new Vector3(0.8f, 0.8f, 1);
                    visual.Offset = new Vector3(-280, 0, 0);
                }
                else
                {
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
                //double scaleAdjustment = Root.XamlRoot.RasterizationScale;
                double scaleAdjustment = Root.RasterizationScale;

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

        public void SetDragRegionForCustomTitleBar(RectangleF[] dragArea, AppWindow appWindow = null)
        {
            appWindow ??= m_AppWindow;
            if (!AppWindowTitleBar.IsCustomizationSupported() || !appWindow.TitleBar.ExtendsContentIntoTitleBar) return;

            double scaleAdjustment = Root.XamlRoot.RasterizationScale;

            float leftBarWidth = 60;
            var rects = dragArea.Select(r => new RectInt32((int)((r.X + leftBarWidth)* scaleAdjustment), (int)(r.Y * scaleAdjustment), (int)(r.Width * scaleAdjustment), (int)(r.Height * scaleAdjustment))).ToArray();
            appWindow.TitleBar.SetDragRectangles(rects);
        }
    }
}
