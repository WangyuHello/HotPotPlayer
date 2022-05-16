using HotPotPlayer.Extensions;
using HotPotPlayer.Models;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
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
    public sealed partial class LyricPanel : UserControl
    {
        public LyricPanel()
        {
            this.InitializeComponent();
        }

        public List<LyricItem> Lyric
        {
            get { return (List<LyricItem>)GetValue(LyricProperty); }
            set { SetValue(LyricProperty, value); }
        }

        public static readonly DependencyProperty LyricProperty =
            DependencyProperty.Register("Lyric", typeof(List<LyricItem>), typeof(LyricPanel), new PropertyMetadata(null, OnLyricChanged));

        private static void OnLyricChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LyricPanel)d).LyricChanged((List<LyricItem>)e.NewValue);
        }

        public TimeSpan CurrentTime
        {
            get { return (TimeSpan)GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }

        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register("CurrentTime", typeof(TimeSpan), typeof(LyricPanel), new PropertyMetadata(default(TimeSpan), OnTimeChanged));

        private static void OnTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LyricPanel)d).TimeChanged((TimeSpan)e.NewValue);
        }

        void TimeChanged(TimeSpan time, bool forceUpdate = false)
        {
            if (lyricItems == null) return;
            if (index == lyricItems.Count - 1)
            {
                if (forceUpdate)
                {
                    Canvas.Invalidate();
                }
            }
            else
            {
                var curIndex = index;
                int i = 0;
                if (time < lyricItems[0].Time)
                {
                    goto get;
                }
                for (; i < lyricItems.Count - 1; i++)
                {
                    if (time >= lyricItems[i].Time && time < lyricItems[i+1].Time)
                    {
                        break;
                    }
                }
                get:
                index = i;
                if (index != curIndex || forceUpdate)
                {
                    //Index变化，刷新
                    Canvas.Invalidate();
                }
            }
        }

        int index;
        List<LyricItem> lyricItems;
        void LyricChanged(List<LyricItem> raw)
        {
            lyricItems = raw;
            index = 0;
            TimeChanged(TimeSpan.Zero, true);
        }

        float centerHeight = 500;
        float centerWidth = 500;
        int count = 6;
        int textHeight = 60;

        private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (lyricItems == null)
            {
                return;
            }
            var lower = index >= count ? index - count : 0;
            var upper = index <= lyricItems.Count - 1 - count ? index + count : lyricItems.Count - 1;
            for (int i = lower; i <= upper; i++)
            {
                var i2 = i - index;
                var delta = i2 * textHeight;
                var y = centerHeight + delta;
                var s = lyricItems[i].Content;
                var color = i2 == 0 ? Colors.Black : Colors.Gray;
                args.DrawingSession.DrawText(s, centerWidth, y, color, _format);
                if (!string.IsNullOrEmpty(lyricItems[i].Translate))
                {
                    args.DrawingSession.DrawText(lyricItems[i].Translate, centerWidth, y+20, color, _format);
                }
            }
        }

        CanvasTextFormat _format;

        private void Canvas_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            _format = new CanvasTextFormat
            {
                HorizontalAlignment = CanvasHorizontalAlignment.Center,
                WordWrapping = CanvasWordWrapping.Wrap,
                FontFamily = "ms-appx:///Assets/Font/MiSans-Regular.ttf#MiSans",
                FontSize = 16,
            };
        }

        private void Canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {

        }

        private void Canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {

        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            centerHeight = (float)(e.NewSize.Height / 2);
            centerWidth = (float)(e.NewSize.Width / 2);
            count = (int)(e.NewSize.Height / textHeight) / 2;
        }
    }
}
