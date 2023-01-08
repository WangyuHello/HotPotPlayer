// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using HotPotPlayer.Services.BiliBili.Danmaku;
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

namespace HotPotPlayer.Video.Control
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
            //v.Draw((Pbp)e.NewValue);
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

            Root.Points.Clear();
            for (int i = 0; i < ys.Count; i++)
            {
                Root.Points.Add(new Point(dx * i, height / 2 * ( 1 - ys[i] / yMax)));
            }

            for (int i = ys.Count - 1; i >= 0; i--)
            {
                Root.Points.Add(new Point(dx * i, height / 2 * (1 + ys[i] / yMax)));
            }
        }

        private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Draw(Pbp);
        }
    }
}
