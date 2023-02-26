// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using HotPotPlayer.Bilibili.Models.Danmaku;
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

namespace HotPotPlayer.Video.UI.Controls
{
    public sealed partial class PbpViewer : UserControl
    {
        public PbpViewer()
        {
            this.InitializeComponent();
        }

        public Pbp Pbp
        {
            get { return (Pbp)GetValue(PbpProperty); }
            set { SetValue(PbpProperty, value); }
        }

        public static readonly DependencyProperty PbpProperty =
            DependencyProperty.Register("Pbp", typeof(Pbp), typeof(PbpViewer), new PropertyMetadata(default, PbpChanged));

        private static void PbpChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var v = d as PbpViewer;
            v.Draw((Pbp)e.NewValue);
        }

        private void Draw(Pbp pbp)
        {
            if (Root.ActualWidth == 0 || pbp == null)
            {
                return;
            }
            if (pbp.Events == null || pbp.Events.Default == null)
            {
                return;
            }

            var dt = pbp.StepSec;
            var width = Root.ActualWidth;
            var dx = width * dt / pbp.MaxTime;
            var height = Root.ActualHeight;
            var ys = pbp.Events.Default;
            var yMax = ys.Max();

            PathFigure UpFigure = new PathFigure
            {
                StartPoint = new Point(0, height / 2 * (1 - 0.8 * ys[0] / yMax))
            };
            PathFigure DownFigure = new PathFigure
            {
                StartPoint = new Point(0, height / 2 * (1 - 0.8 * ys[0] / yMax))
            };

            PathSegmentCollection segs1 = new();
            PathSegmentCollection segs2 = new();

            for (int i = 1; i < ys.Count; i++)
            {
                segs1.Add(new BezierSegment
                {
                    Point1 = new Point(dx * (i - 1) + 0.75 * dx, height / 2 * (1 - 0.8 * ys[i - 1] / yMax)),
                    Point2 = new Point(dx * (i - 1) + 0.25 * dx, height / 2 * (1 - 0.8 * ys[i] / yMax)),
                    Point3 = new Point(dx * i, height / 2 * (1 - 0.8 * ys[i] / yMax))
                });
            }

            for (int i = 1; i < ys.Count; i++)
            {
                segs2.Add(new BezierSegment
                {
                    Point1 = new Point(dx * (i - 1) + 0.75 * dx, height / 2 * (1 + 0.8 * ys[i - 1] / yMax)),
                    Point2 = new Point(dx * (i - 1) + 0.25 * dx, height / 2 * (1 + 0.8 * ys[i] / yMax)),
                    Point3 = new Point(dx * i, height / 2 * (1 + 0.8 * ys[i] / yMax))
                });
            }

            if (ys.Last() > 0)
            {
                segs1.Add(new LineSegment
                {
                    Point = new Point(width, height / 2)
                });
                segs2.Add(new LineSegment
                {
                    Point = new Point(width, height / 2)
                });
            }

            UpFigure.Segments = segs1;
            DownFigure.Segments = segs2;

            PathFigureCollection pthFigureCollection = new()
            {
                UpFigure,
                DownFigure
            };

            PathGeometry pthGeometry = new PathGeometry
            {
                Figures = pthFigureCollection
            };

            Root.Data = pthGeometry;
        }

        Size prevSize;

        private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var dw = Math.Abs(prevSize.Width - e.NewSize.Width);
            //var dh = Math.Abs(prevSize.Height - e.NewSize.Height);
            if (dw < 20) return;
            prevSize = e.NewSize;
            Draw(Pbp);
        }
    }
}
