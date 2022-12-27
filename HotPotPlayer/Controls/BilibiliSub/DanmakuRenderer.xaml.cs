// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using HotPotPlayer.Services.BiliBili.Danmaku;
using Microsoft.Graphics.Canvas;
using Microsoft.UI;
using System.Numerics;
using Windows.Graphics.Display;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Hosting;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Controls.BilibiliSub
{
    public sealed partial class DanmakuRenderer : UserControlBase
    {
        public DanmakuRenderer()
        {
            this.InitializeComponent();
            _tickTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _tickTimer.Tick += DmTick;
            _compositor = App.MainWindow.Compositor;
        }

        private void DmTick(object sender, object e)
        {
            var now = DateTime.Now;
            var delta = now - _baseTime;
            var secs = Convert.ToInt32(delta.TotalSeconds);
            if (_timeLine.ContainsKey(secs))
            {
                var l = _timeLine[secs];
                foreach (var item in l)
                {
                    TextBlock tb = new TextBlock();
                    tb.Text = item.Content;
                    tb.Foreground = new SolidColorBrush(Colors.White);
                    tb.FontSize = 16;
                    var visual = ElementCompositionPreview.GetElementVisual(tb);
                    Vector3KeyFrameAnimation animation = _compositor.CreateVector3KeyFrameAnimation();
                    animation.InsertKeyFrame(1f, new Vector3(Convert.ToSingle(Host.ActualWidth), 0f, 0f));
                    animation.Duration = TimeSpan.FromSeconds(5);
                    animation.Direction = Microsoft.UI.Composition.AnimationDirection.Reverse;
                    Host.Children.Add(tb);
                    visual.StartAnimation("Offset", animation);
                }
            }
        }

        public DMData DmData
        {
            get { return (DMData)GetValue(DmDataProperty); }
            set { SetValue(DmDataProperty, value); }
        }

        public static readonly DependencyProperty DmDataProperty =
            DependencyProperty.Register("DmData", typeof(DMData), typeof(DanmakuRenderer), new PropertyMetadata(default, DMChanged));

        private static void DMChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DanmakuRenderer)d).Start(e.NewValue as DMData);
        }

        DateTime _baseTime;
        DispatcherTimer _tickTimer;
        Dictionary<int, List<DMItem>> _timeLine;
        Compositor _compositor;

        private void Start(DMData n)
        {
            _timeLine = new Dictionary<int, List<DMItem>>();
            for (int i = 0; i < n.Dms.Count; i++)
            {
                var d = n.Dms[i];
                AddToTimeline(Convert.ToInt32(d.Time.TotalSeconds), d);
            }

            void AddToTimeline(int t, DMItem item)
            {
                if (_timeLine.ContainsKey(t))
                {
                    var l = _timeLine[t];
                    l.Add(item);
                }
                else
                {
                    _timeLine.Add(t, new List<DMItem> { item });
                }
            }

            _tickTimer.Start();
            _baseTime = DateTime.Now;
        }

    }
}
