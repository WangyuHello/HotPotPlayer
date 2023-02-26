// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HotPotPlayer.Bilibili.Models.Danmaku;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Composition;
using Microsoft.Graphics.DirectX;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI;
using HotPotPlayer.Extensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Video.UI.Controls
{
    public sealed partial class DanmakuRenderer : UserControlBase
    {
        public DanmakuRenderer()
        {
            this.InitializeComponent();
            _compositor = App.MainWindow.Compositor;
            _horizontalTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _horizontalTimer.Tick += DmTick;
            _topTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _topTimer.Tick += TopDmTick;
        }

        private void TopDmTick(object sender, object e)
        {
            var curTime = CurrentTime;
            var curSecs = Convert.ToInt32(curTime.TotalSeconds);
            if (_topDMs.ContainsKey(curSecs))
            {
                int takeIndex = 0;
                for (int i = 0; i < TotalSlot; i++)
                {
                    var tb = (DanmakuTextControl)TopHost.Children[i];
                    if (tb.ExitTime > curTime) continue;
                    if (takeIndex > _topDMs[curSecs].Count - 1) break;
                    var d = _topDMs[curSecs][takeIndex];
                    takeIndex++;

                    var drawDm = DrawDanmaku(d);
                    tb.ExitTime = curTime + TimeSpan.FromSeconds(5);
                    tb.SetVisual(drawDm);
                    tb.Width = drawDm.Size.X;
                    tb.Height = drawDm.Size.Y;
                    tb.Dm = d;

                    tb.SetupOpacityAnimation(TimeSpan.FromSeconds(5));
                    tb.StartOpacityAnimation();
                }
            }

            if (_bottomDMs.ContainsKey(curSecs))
            {
                int takeIndex = 0;
                for (int i = 0; i < TotalSlot; i++)
                {
                    var tb = (DanmakuTextControl)BottomHost.Children[TotalSlot - i - 1];
                    if (tb.ExitTime > curTime) continue;
                    if (takeIndex > _bottomDMs[curSecs].Count - 1) break;
                    var d = _bottomDMs[curSecs][takeIndex];
                    takeIndex++;

                    var drawDm = DrawDanmaku(d);
                    tb.ExitTime = curTime + TimeSpan.FromSeconds(5);
                    tb.SetVisual(drawDm);
                    tb.Width = drawDm.Size.X;
                    tb.Height = drawDm.Size.Y;
                    tb.Dm = d;

                    tb.SetupOpacityAnimation(TimeSpan.FromSeconds(5));
                    tb.StartOpacityAnimation();
                }
            }
        }

        readonly double _slotStep = 26 + 8;

        private void DmTick(object sender, object e)
        {
            var curTime = CurrentTime;
            var curSecs = Convert.ToInt32(curTime.TotalSeconds);
            for (int sec = curSecs; sec < curSecs + 5; sec++)
            {
                if (_horizontalDMs.ContainsKey(sec))
                {
                    int takeIndex = 0;
                    for (int i = 0; i < Slot; i++)
                    {
                        if (_masks[sec][i].occupied) continue;
                        if (takeIndex > _horizontalDMs[sec].Count - 1)
                        {
                            break;
                        }
                        var d = _horizontalDMs[sec][takeIndex];
                        takeIndex++;

                        if (d.Time < curTime) continue;

                        DanmakuTextControl tb;
                        var hasText = _toBeRecycledTexts.TryPeek(out var reuseText);
                        bool isReuse = false;
                        var drawDm = DrawDanmaku(d);
                        if (hasText && curTime > reuseText.ExitTime)
                        {
                            tb = _toBeRecycledTexts.Dequeue();
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
                        var len = drawDm.Size.X;
                        tb.SetupOffsetAnimation(curTime, len, _slotStep, Speed, i, Host.ActualWidth);
                        tb.StartOffsetAnimation();

                        _toBeRecycledTexts.Enqueue(tb);

                        var segs = Convert.ToInt32(Math.Floor(len * 1.3 / Speed));
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
            ((DanmakuRenderer)d).LoadDMData(e.NewValue as DMData);
            ((DanmakuRenderer)d).Refresh();
        }

        public int Slot
        {
            get { return (int)GetValue(SlotProperty); }
            set { SetValue(SlotProperty, value); }
        }

        public static readonly DependencyProperty SlotProperty =
            DependencyProperty.Register("Slot", typeof(int), typeof(DanmakuRenderer), new PropertyMetadata(0));

        private int TotalSlot;
        public double Speed
        {
            get { return (double)GetValue(SpeedProperty); }
            set { SetValue(SpeedProperty, value); }
        }

        public static readonly DependencyProperty SpeedProperty =
            DependencyProperty.Register("Speed", typeof(double), typeof(DanmakuRenderer), new PropertyMetadata(0.0));

        public double FontScale
        {
            get { return (double)GetValue(FontScaleProperty); }
            set { SetValue(FontScaleProperty, value); }
        }

        public static readonly DependencyProperty FontScaleProperty =
            DependencyProperty.Register("FontScale", typeof(double), typeof(DanmakuRenderer), new PropertyMetadata(1.0));


        public TimeSpan CurrentTime
        {
            get { return (TimeSpan)GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }

        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register("CurrentTime", typeof(TimeSpan), typeof(DanmakuRenderer), new PropertyMetadata(default));


        public bool IsPlaying
        {
            get { return (bool)GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }

        public static readonly DependencyProperty IsPlayingProperty =
            DependencyProperty.Register("IsPlaying", typeof(bool), typeof(DanmakuRenderer), new PropertyMetadata(false, IsPlayingChanged));

        private static void IsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DanmakuRenderer)d).OnIsPlayingChanged((bool)e.OldValue, (bool)e.NewValue);
        }

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


        DispatcherTimer _horizontalTimer;
        DispatcherTimer _topTimer;
        Dictionary<int, Mask[]> _masks;
        List<int> _availTime;
        Dictionary<int, List<DMItem>> _horizontalDMs;
        Dictionary<int, List<DMItem>> _topDMs;
        Dictionary<int, List<DMItem>> _bottomDMs;
        Compositor _compositor;
        Queue<DanmakuTextControl> _toBeRecycledTexts;

        private void LoadDMData(DMData n)
        {
            _horizontalDMs = new Dictionary<int, List<DMItem>>();
            _topDMs = new Dictionary<int, List<DMItem>>();
            _bottomDMs = new Dictionary<int, List<DMItem>>();
            for (int i = 0; i < n.Dms.Count; i++)
            {
                var d = n.Dms[i];
                switch (d.Type)
                {
                    case 1:
                    case 2:
                    case 3:
                        AddToTimeline(Convert.ToInt32(d.Time.TotalSeconds), d, _horizontalDMs);
                        break;
                    case 4:
                        AddToTimeline(Convert.ToInt32(d.Time.TotalSeconds), d, _topDMs);
                        break;
                    case 5:
                        AddToTimeline(Convert.ToInt32(d.Time.TotalSeconds), d, _bottomDMs);
                        break;
                    default:
                        AddToTimeline(Convert.ToInt32(d.Time.TotalSeconds), d, _horizontalDMs);
                        break;
                }

            }
            Debug.WriteLine($"DMs: {n.Dms.Count}, timeLines: {_horizontalDMs.Count}");
            _availTime = _horizontalDMs.Keys.ToList();
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

            _toBeRecycledTexts = new Queue<DanmakuTextControl>();
        }

        private void Host_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Refresh();
        }

        void OnIsPlayingChanged(bool oldVal, bool newVal)
        {
            if (!oldVal && newVal) //from pause to playing
            {
                if (_horizontalTimer.IsEnabled)
                {
                    //already playing dm
                }
                else
                {
                    Resume();
                }
            }
            else if(oldVal && !newVal)
            {
                Pause();
            }
        }

        public void Pause()
        {
            if (DmData == null)
            {
                return;
            }
            _horizontalTimer.Stop();
            _topTimer.Stop();
            _toBeRecycledTexts.Select(tb =>
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
            _horizontalTimer.Start();
            _topTimer.Start();
            if (_toBeRecycledTexts.Count == 0) return;
            _toBeRecycledTexts.Select(tb =>
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
            RefreshVerticalSlot();

            if (!IsPlaying)
            {
                return;
            }
            if (!_horizontalTimer.IsEnabled)
            {
                DmTick(null, null);

                _horizontalTimer.Start();
                _topTimer.Start();
            }
            else
            {
                _horizontalTimer.Stop();
                _topTimer.Stop();
                Host.Children.Clear();
                _toBeRecycledTexts?.Clear();
                ClearMask();
                DmTick(null, null);
                _horizontalTimer.Start();
                _topTimer.Start();
            }
        }

        private void ClearMask()
        {
            foreach (var (i, m) in _masks)
            {
                foreach (var item in m)
                {
                    item.occupied = false;
                }
            }
        }

        private void RefreshVerticalSlot()
        {
            var height = Host.ActualHeight;
            var step = _slotStep;
            TotalSlot = Convert.ToInt32(Math.Floor(height / step));

            TopHost.Children.Clear();
            TopHost.RowDefinitions.Clear();
            for (int i = 0; i < TotalSlot; i++)
            {
                TopHost.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(_slotStep) });
            }
            TopHost.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Star) });
            for (int i = 0; i < TotalSlot; i++)
            {
                var tb = new DanmakuTextControl()
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    ExitTime = TimeSpan.Zero,
                };
                tb.SetValue(Grid.RowProperty, i);
                TopHost.Children.Add(tb);
            }

            BottomHost.Children.Clear();
            BottomHost.RowDefinitions.Clear();
            BottomHost.Height = TotalSlot * _slotStep;
            for (int i = 0; i < TotalSlot; i++)
            {
                BottomHost.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(_slotStep) });
            }
            for (int i = 0; i < TotalSlot; i++)
            {
                var tb = new DanmakuTextControl() 
                {
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    ExitTime = TimeSpan.Zero,
                };
                tb.SetValue(Grid.RowProperty, i);
                BottomHost.Children.Add(tb);
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
                FontSize = dm.FontSize,
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

            //var bitmap = new CanvasRenderTarget(session, (float)drawingSize.Width, (float)drawingSize.Height);
            //using var bitmapSession = bitmap.CreateDrawingSession();
            //bitmapSession.DrawTextLayout(textLayout, 0, 0, Colors.Black);
            
            //var blur = new ShadowEffect
            //{
            //    BlurAmount = 3f,
            //    Source = bitmap,
            //};

            using var geometry = CanvasGeometry.CreateText(textLayout);

            using var dashedStroke = new CanvasStrokeStyle()
            {
                DashStyle = CanvasDashStyle.Solid,
            };
            //session.DrawImage(blur, 0, 0);
            session.DrawGeometry(geometry, OutlineColor, (float)OutlineThickness, dashedStroke);
            session.DrawTextLayout(textLayout, 0, 0, dm.Color.ToWindowsColor());

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
