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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Video.Bilibili
{
    public sealed partial class DanmakuRenderer : UserControlBase
    {
        public DanmakuRenderer()
        {
            this.InitializeComponent();
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
                if (_topTexts[i].Exit < curTime)
                {
                    _topRecycleTexts.Enqueue(_topTexts[i].Text);
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
                    if (has)
                    {
                        tb.Text = d.Content;
                        tb.Foreground = new SolidColorBrush(d.Color);
                        tb.FontSize = d.FontSize;
                    }
                    else
                    {
                        tb = new()
                        {
                            Text = d.Content,
                            Foreground = new SolidColorBrush(d.Color),
                            FontSize = d.FontSize,
                            FontFamily = (FontFamily)Application.Current.Resources["MiSansRegular"],
                            HorizontalAlignment = HorizontalAlignment.Center,
                        };
                    }

                    TopHost.Children.Add(tb);
                    _topTexts.Add(new ExitTime { Exit = curTime + TimeSpan.FromSeconds(3), Text = tb });
                }
            }

            for (int i = _bottomTexts.Count - 1; i >= 0; i--)
            {
                if (_bottomTexts[i].Exit < curTime)
                {
                    _bottomRecycleTexts.Enqueue(_bottomTexts[i].Text);
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
                    if (has)
                    {
                        tb.Text = d.Content;
                        tb.Foreground = new SolidColorBrush(d.Color);
                        tb.FontSize = d.FontSize;
                    }
                    else
                    {
                        tb = new()
                        {
                            Text = d.Content,
                            Foreground = new SolidColorBrush(d.Color),
                            FontSize = d.FontSize,
                            FontFamily = (FontFamily)Application.Current.Resources["MiSansRegular"],
                            HorizontalAlignment = HorizontalAlignment.Center,
                        };
                    }
                    BottomHost.Children.Insert(0, tb);
                    _bottomTexts.Insert(0, new ExitTime { Exit = curTime + TimeSpan.FromSeconds(3), Text = tb });
                }
            }
        }

        double SlotStep => FontSize + 8;
        LinearEasingFunction _linear;

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

                        TextBlock tb;
                        var hasText = _texts.TryPeek(out var reuseText);
                        bool isReuse = false;
                        if(hasText && curTime > reuseText.Exit)
                        {
                            tb = _texts.Dequeue().Text;
                            tb.Text = d.Content;
                            tb.Foreground = new SolidColorBrush(d.Color);
                            isReuse = true;
                        }
                        else
                        {
                            tb = new()
                            {
                                Text = d.Content,
                                Foreground = new SolidColorBrush(d.Color),
                                FontSize = FontSize,
                                FontFamily = (FontFamily)Application.Current.Resources["MiSansRegular"],
                            };
                        }

                        var visual = ElementCompositionPreview.GetElementVisual(tb);
                        var animation = _compositor.CreateVector3KeyFrameAnimation();
                        var len = d.Content.Length * FontSize;
                        var exLen = len + 200;
                        animation.InsertKeyFrame(0f, new Vector3(Convert.ToSingle(Host.ActualWidth + 1), (float)(SlotStep * i), 0f), _linear);
                        animation.InsertKeyFrame(1f, new Vector3((float)-exLen, (float)(SlotStep * i), 0f), _linear);
                        animation.Duration = TimeSpan.FromSeconds((Host.ActualWidth + exLen + 1) / Speed);
                        animation.DelayTime = d.Time - curTime;
                        animation.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;
                        var m = _masks[sec][i];
                        m.occupied = true;

                        if (!isReuse)
                        {
                            Host.Children.Add(tb);
                        }
                        visual.StartAnimation("Offset", animation);

                        _texts.Enqueue(new ExitTime
                        {
                            Text = tb,
                            Exit = curTime + animation.Duration
                        });


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


        DispatcherTimer _tickTimer;
        DispatcherTimer _topTickTimer;
        Dictionary<int, Mask[]> _masks;
        List<int> _availTime;
        Dictionary<int, List<DMItem>> _timeLine;
        Dictionary<int, List<DMItem>> _toptimeLine;
        Dictionary<int, List<DMItem>> _bottomtimeLine;
        Compositor _compositor;
        Queue<ExitTime> _texts;
        List<ExitTime> _topTexts;
        Queue<TextBlock> _topRecycleTexts;
        List<ExitTime> _bottomTexts;
        Queue<TextBlock> _bottomRecycleTexts;
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

            _texts = new Queue<ExitTime>();
            _topTexts = new List<ExitTime>();
            _bottomTexts = new List<ExitTime>();
            _topRecycleTexts = new Queue<TextBlock>();
            _bottomRecycleTexts = new Queue<TextBlock>();
            _visuals = new Queue<Visual>();
            _compositor = App.MainWindow.Compositor;
            _linear = _compositor.CreateLinearEasingFunction();
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
                var visual = ElementCompositionPreview.GetElementVisual(tb.Text);
                visual.StopAnimation("Offset");
                return true;
            }).ToList();
        }

        public void Resume()
        {
            if (DmData == null)
            {
                return;
            }
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

                Resume();
            }
        }

        class ExitTime
        {
            public TextBlock Text { get; set; }
            public TimeSpan Exit { get; set; }
        }

        class Mask
        {
            public bool occupied;
        }
    }
}
