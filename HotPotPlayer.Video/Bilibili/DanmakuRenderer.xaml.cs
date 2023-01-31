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
using CommunityToolkit.WinUI.UI.Animations;
using HotPotPlayer.Video.Control;
using System.Collections;
using System.Diagnostics;
using Microsoft.Graphics.Canvas.UI.Composition;
using Microsoft.Graphics.Canvas.Text;
using System.Management;
using Microsoft.Graphics.DirectX;
using Application = Microsoft.UI.Xaml.Application;
using Microsoft.Graphics.Canvas.Geometry;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Video.Bilibili
{
    public sealed partial class DanmakuRenderer : UserControlBase
    {
        public DanmakuRenderer()
        {
            this.InitializeComponent();
            _compositor = App.MainWindow.Compositor;
            _tickTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _tickTimer.Tick += DmTick;
            _topTickTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _topTickTimer.Tick += TopDmTick;
        }

        private void TopDmTick(object sender, object e)
        {
            var curTime = CurrentTime;
            var curSecs = Convert.ToInt32(curTime.TotalSeconds);
            for (int i = 0; i < _topTexts.Count; i++)
            {
                if (_topTexts[i].ExitTime < curTime)
                {
                    _topRecycleTexts.Enqueue(_topTexts[i]);
                    _topTexts.RemoveAt(i);
                    TopHost.Children.RemoveAt(i);
                }
            }
            if (_toptimeLine.ContainsKey(curSecs))
            {
                var dms = _toptimeLine[curSecs].Take(5);
                foreach (var d in dms)
                {
                    var has = _topRecycleTexts.TryDequeue(out var tb);
                    var drawDm = DrawDanmaku(d);
                    if (has)
                    {
                        tb.ExitTime = curTime + TimeSpan.FromSeconds(3);
                        tb.SetVisual(drawDm);
                    }
                    else
                    {
                        tb = new(drawDm)
                        {
                            ExitTime = curTime + TimeSpan.FromSeconds(3)
                        };
                    }
                    tb.Width = drawDm.Size.X;
                    tb.Height = drawDm.Size.Y;
                    tb.Dm = d;

                    TopHost.Children.Add(tb);
                    _topTexts.Add(tb);
                }
            }

            for (int i = _bottomTexts.Count - 1; i >= 0; i--)
            {
                if (_bottomTexts[i].ExitTime < curTime)
                {
                    _bottomRecycleTexts.Enqueue(_bottomTexts[i]);
                    _bottomTexts.RemoveAt(i);
                    BottomHost.Children.RemoveAt(i);
                }
            }
            if (_bottomtimeLine.ContainsKey(curSecs))
            {
                var dms = _bottomtimeLine[curSecs].Take(5);
                foreach (var d in dms)
                {
                    var has = _bottomRecycleTexts.TryDequeue(out var tb);
                    var drawDm = DrawDanmaku(d);
                    if (has)
                    {
                        tb.ExitTime = curTime + TimeSpan.FromSeconds(3);
                        tb.SetVisual(drawDm);
                    }
                    else
                    {
                        tb = new(drawDm)
                        {
                            ExitTime = curTime + TimeSpan.FromSeconds(3)
                        };
                    }
                    tb.Width = drawDm.Size.X;
                    tb.Height = drawDm.Size.Y;
                    tb.Dm = d;
                    BottomHost.Children.Insert(0, tb);
                    _bottomTexts.Insert(0, tb);
                }
            }
        }

        double SlotStep => FontSize + 8;

        private void DmTick(object sender, object e)
        {
            var curTime = CurrentTime;
            var curSecs = Convert.ToInt32(curTime.TotalSeconds);
            for (int sec = curSecs; sec < curSecs + 5; sec++)
            {
                if (_timeLine.ContainsKey(sec))
                {
                    for (int i = 0; i < Slot; i++)
                    {
                        if (_masks[sec][i].occupied) continue;
                        if (i > _timeLine[sec].Count - 1)
                        {
                            break;
                        }
                        var d = _timeLine[sec][i];

                        if (d.Time < curTime) continue;

                        DanmakuTextControl tb;
                        var hasText = _texts.TryPeek(out var reuseText);
                        bool isReuse = false;
                        var drawDm = DrawDanmaku(d);
                        if(hasText && curTime > reuseText.ExitTime)
                        {
                            tb = _texts.Dequeue();
                            tb.SetVisual(drawDm);
                            isReuse = true;
                        }
                        else
                        {
                            tb = new(drawDm)
                            {
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Top
                            };
                        }
                        tb.Width = drawDm.Size.X;
                        tb.Height = drawDm.Size.Y;
                        tb.Dm = d;

                        var m = _masks[sec][i];
                        m.occupied = true;

                        if (!isReuse)
                        {
                            Host.Children.Add(tb);
                        }
                        var len = tb.SetupOffsetAnimation(curTime, SlotStep, Speed, i, Host.ActualWidth);
                        tb.StartOffsetAnimation();

                        _texts.Enqueue(tb);


                        var segs = Convert.ToInt32(Math.Floor((len*1.3) / Speed));
                        for (int s = 0; s < segs; s++)
                        {
                            if (_masks.ContainsKey(sec + s + 1))
                            {
                                var m2 = _masks[sec + s + 1][i];
                                m2.occupied = true;
                            }
                        }
                    }
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
            ((DanmakuRenderer)d).Load(e.NewValue as DMData);
            ((DanmakuRenderer)d).Refresh();
        }

        public int Slot
        {
            get { return (int)GetValue(SlotProperty); }
            set { SetValue(SlotProperty, value); }
        }

        public static readonly DependencyProperty SlotProperty =
            DependencyProperty.Register("Slot", typeof(int), typeof(DanmakuRenderer), new PropertyMetadata(0));


        public double Speed
        {
            get { return (double)GetValue(SpeedProperty); }
            set { SetValue(SpeedProperty, value); }
        }

        public static readonly DependencyProperty SpeedProperty =
            DependencyProperty.Register("Speed", typeof(double), typeof(DanmakuRenderer), new PropertyMetadata(0.0));


        public TimeSpan CurrentTime
        {
            get { return (TimeSpan)GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }

        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register("CurrentTime", typeof(TimeSpan), typeof(DanmakuRenderer), new PropertyMetadata(default));

        public double OutlineThickness
        {
            get { return (double)GetValue(OutlineThicknessProperty); }
            set { SetValue(OutlineThicknessProperty, value); }
        }

        public static readonly DependencyProperty OutlineThicknessProperty =
            DependencyProperty.Register("OutlineThickness", typeof(double), typeof(DanmakuRenderer), new PropertyMetadata(1.8));

        public Color OutlineColor
        {
            get { return (Color)GetValue(OutlineColorProperty); }
            set { SetValue(OutlineColorProperty, value); }
        }

        public static readonly DependencyProperty OutlineColorProperty =
            DependencyProperty.Register("OutlineColor", typeof(Color), typeof(DanmakuRenderer), new PropertyMetadata(Colors.Black));


        DispatcherTimer _tickTimer;
        DispatcherTimer _topTickTimer;
        Dictionary<int, Mask[]> _masks;
        List<int> _availTime;
        Dictionary<int, List<DMItem>> _timeLine;
        Dictionary<int, List<DMItem>> _toptimeLine;
        Dictionary<int, List<DMItem>> _bottomtimeLine;
        Compositor _compositor;
        Queue<DanmakuTextControl> _texts;
        List<DanmakuTextControl> _topTexts;
        Queue<DanmakuTextControl> _topRecycleTexts;
        List<DanmakuTextControl> _bottomTexts;
        Queue<DanmakuTextControl> _bottomRecycleTexts;
        Queue<Visual> _visuals;

        private void Load(DMData n)
        {
            _timeLine = new Dictionary<int, List<DMItem>>();
            _toptimeLine = new Dictionary<int, List<DMItem>>();
            _bottomtimeLine = new Dictionary<int, List<DMItem>>();
            for (int i = 0; i < n.Dms.Count; i++)
            {
                var d = n.Dms[i];
                switch (d.Type)
                {
                    case 1:
                    case 2:
                    case 3:
                        AddToTimeline(Convert.ToInt32(d.Time.TotalSeconds), d, _timeLine);
                        break;
                    case 4:
                        AddToTimeline(Convert.ToInt32(d.Time.TotalSeconds), d, _toptimeLine);
                        break;
                    case 5:
                        AddToTimeline(Convert.ToInt32(d.Time.TotalSeconds), d, _bottomtimeLine);
                        break;
                    default:
                        AddToTimeline(Convert.ToInt32(d.Time.TotalSeconds), d, _timeLine);
                        break;
                }

            }
            Debug.WriteLine($"DMs: {n.Dms.Count}, timeLines: {_timeLine.Count}");
            _availTime = _timeLine.Keys.ToList();
            _masks = _availTime.ToDictionary(a => a, b => Enumerable.Range(0, Slot).Select(x => new Mask()).ToArray());

            static void AddToTimeline(int t, DMItem item, Dictionary<int, List<DMItem>> container)
            {
                if (container.ContainsKey(t))
                {
                    var l = container[t];
                    l.Add(item);
                }
                else
                {
                    container.Add(t, new List<DMItem> { item });
                }
            }

            _texts = new Queue<DanmakuTextControl>();
            _topTexts = new List<DanmakuTextControl>();
            _bottomTexts = new List<DanmakuTextControl>();
            _topRecycleTexts = new Queue<DanmakuTextControl>();
            _bottomRecycleTexts = new Queue<DanmakuTextControl>();
            _visuals = new Queue<Visual>();
        }

        private void Host_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Refresh();
        }

        public void Pause()
        {
            if (DmData == null)
            {
                return;
            }
            _tickTimer.Stop();
            _topTickTimer.Stop();
            _texts.Select(tb =>
            {
                tb.StopOffsetAnimation();
                return true;
            }).ToList();
        }

        public void Resume()
        {
            if (DmData == null)
            {
                return;
            }
            if (_texts.Count == 0)
            {
                return;
            }
            _tickTimer.Start();
            _topTickTimer.Start();
            _texts.Select(tb =>
            {
                tb.ContinueOffsetAnimation();
                return true;
            }).ToList();
        }

        public void Refresh()
        {
            if (DmData == null)
            {
                return;
            }
            if (Host.ActualWidth == 0)
            {
                return;
            }
            if (!_tickTimer.IsEnabled)
            {
                DmTick(null, null);

                _tickTimer.Start();
                _topTickTimer.Start();
            }
            else
            {
                _tickTimer.Stop();
                _topTickTimer.Stop();
                Host.Children.Clear();
                TopHost.Children.Clear();
                BottomHost.Children.Clear();
                _texts?.Clear();
                _topTexts?.Clear();
                _bottomTexts?.Clear();
                _topRecycleTexts?.Clear();
                _bottomRecycleTexts?.Clear();
                foreach (var (i, m) in _masks)
                {
                    foreach (var item in m)
                    {
                        item.occupied = false;
                    }
                }
                DmTick(null, null);
                _tickTimer.Start();
                _topTickTimer.Start();
            }
        }

        CanvasDevice _device;
        CanvasDevice Device => _device ??= CanvasDevice.GetSharedDevice();

        CompositionGraphicsDevice _graphicsDevice;
        CompositionGraphicsDevice GraphicsDevice => _graphicsDevice ??= CanvasComposition.CreateCompositionGraphicsDevice(_compositor, _device);

        Visual DrawDanmaku(DMItem dm)
        {
            var spriteTextVisual = _compositor.CreateSpriteVisual();

            var textFormat = new CanvasTextFormat()
            {
                FontSize = (float)FontSize,
                Direction = CanvasTextDirection.LeftToRightThenTopToBottom,
                VerticalAlignment = CanvasVerticalAlignment.Top,
                HorizontalAlignment = CanvasHorizontalAlignment.Left,
                FontFamily = FontFamily.Source,
            };
            var textLayout = new CanvasTextLayout(Device, dm.Content, textFormat, dm.FontSize * dm.Content.Length * 2, dm.FontSize);

            var expandAmount = OutlineThickness * 2;
            var textWidth = textLayout.LayoutBounds.Width;
            var textHeight = textLayout.LayoutBounds.Height;
            var expandTextWidth = textWidth + expandAmount;
            var expandTextHeight = textHeight + expandAmount;

            var drawingSize = new Size(expandTextWidth * XamlRoot.RasterizationScale, expandTextHeight * XamlRoot.RasterizationScale);

            var drawingSurface = GraphicsDevice.CreateDrawingSurface(drawingSize, DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);

            using var session = CanvasComposition.CreateDrawingSession(drawingSurface, new Rect(0, 0, drawingSize.Width, drawingSize.Height), (float)(96 * XamlRoot.RasterizationScale));
            session.Clear(Colors.Transparent);

            using var geometry = CanvasGeometry.CreateText(textLayout);

            using var dashedStroke = new CanvasStrokeStyle()
            {
                DashStyle = CanvasDashStyle.Solid,
            };
            session.DrawGeometry(geometry, OutlineColor, (float)OutlineThickness, dashedStroke);
            session.DrawTextLayout(textLayout, 0, 0, dm.Color);

            var maskSurfaceBrush = _compositor.CreateSurfaceBrush(drawingSurface);
            spriteTextVisual.Brush = maskSurfaceBrush;
            spriteTextVisual.Size = new Vector2((float)expandTextWidth, (float)expandTextHeight);
            return spriteTextVisual;
        }

        class Mask
        {
            public bool occupied;
        }
    }
}
