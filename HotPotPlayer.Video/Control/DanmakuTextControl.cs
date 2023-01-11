using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Composition;
using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.UI.Composition;
using Microsoft.Graphics.DirectX;
using System.Numerics;
using Windows.UI;
using Microsoft.UI.Xaml.Media;
using HotPotPlayer.Services.BiliBili.Danmaku;
using CommunityToolkit.WinUI.UI.Animations;
using System.Reflection;

namespace HotPotPlayer.Video.Control
{
    public class DanmakuTextControl : Microsoft.UI.Xaml.Controls.Control
    {
        private CompositionDrawingSurface _drawingSurface;
        private Vector3KeyFrameAnimation _animation;
        private Visual _visual;
        private Compositor _compositor;
        private LinearEasingFunction _linear;

        public DanmakuTextControl()
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _linear = _compositor.CreateLinearEasingFunction();
            var graphicsDevice = CanvasComposition.CreateCompositionGraphicsDevice(_compositor, CanvasDevice.GetSharedDevice());
            var spriteTextVisual = _compositor.CreateSpriteVisual();

            ElementCompositionPreview.SetElementChildVisual(this, spriteTextVisual);
            _visual = ElementCompositionPreview.GetElementVisual(this);
            SizeChanged += (s, e) =>
            {
                _drawingSurface = graphicsDevice.CreateDrawingSurface(e.NewSize, DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
                DrawText();
                var maskSurfaceBrush = _compositor.CreateSurfaceBrush(_drawingSurface);
                spriteTextVisual.Brush = maskSurfaceBrush;
                spriteTextVisual.Size = e.NewSize.ToVector2();
            };
            RegisterPropertyChangedCallback(FontSizeProperty, new DependencyPropertyChangedCallback((s, e) =>
            {
                DrawText();
            }));
            RegisterPropertyChangedCallback(TextProperty, new DependencyPropertyChangedCallback((s, e) =>
            {
                DrawText();
            }));
            RegisterPropertyChangedCallback(FontColorProperty, new DependencyPropertyChangedCallback((s, e) =>
            {
                DrawText();
            }));
        }

        public void StopOffsetAnimation()
        {
            _visual.StopAnimation("Offset");
        }

        public void StartOffsetAnimation()
        {
            _visual.StartAnimation("Offset", _animation);
        }

        public void ContinueOffsetAnimation()
        {
            var curOffset = _visual.Offset;
            if ((curOffset.X - targetOffset.X) < 2)
            {
                return;
            }
            _animation = _compositor.CreateVector3KeyFrameAnimation();
            _animation.InsertKeyFrame(0f, curOffset, _linear);
            _animation.InsertKeyFrame(1f, targetOffset, _linear);
            _animation.Duration = TimeSpan.FromSeconds((curOffset.X - targetOffset.X) / Speed);
            _animation.DelayTime = TimeSpan.Zero;
            _visual.StartAnimation("Offset", _animation);
        }

        private Vector3 targetOffset;

        public double SetupOffsetAnimation(TimeSpan curTime, double slotStep, double speed, int index, double hostWidth)
        {
            _animation = _compositor.CreateVector3KeyFrameAnimation();
            var len = Dm.Content.Length * FontSize;
            var exLen = len + 200;
            _animation.InsertKeyFrame(0f, new Vector3(Convert.ToSingle(hostWidth + 1), (float)(slotStep * index), 0f), _linear);
            targetOffset = new Vector3((float)-exLen, (float)(slotStep * index), 0f);
            _animation.InsertKeyFrame(1f, targetOffset, _linear);
            _animation.Duration = TimeSpan.FromSeconds((hostWidth + exLen + 1) / speed);
            _animation.DelayTime = Dm.Time - curTime;
            _animation.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;
            ExitTime = curTime + _animation.Duration;
            Speed = speed;
            return len;
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(DanmakuTextControl), new PropertyMetadata(default));

        public Color FontColor
        {
            get { return (Color)GetValue(FontColorProperty); }
            set { SetValue(FontColorProperty, value); }
        }

        public static readonly DependencyProperty FontColorProperty =
            DependencyProperty.Register("FontColor", typeof(Color), typeof(DanmakuTextControl), new PropertyMetadata(Colors.White));

        public Color OutlineColor
        {
            get { return (Color)GetValue(OutlineColorProperty); }
            set { SetValue(OutlineColorProperty, value); }
        }

        public static readonly DependencyProperty OutlineColorProperty =
            DependencyProperty.Register("OutlineColor", typeof(Color), typeof(DanmakuTextControl), new PropertyMetadata(Colors.Black));

        public double OutlineThickness
        {
            get { return (double)GetValue(OutlineThicknessProperty); }
            set { SetValue(OutlineThicknessProperty, value); }
        }

        public static readonly DependencyProperty OutlineThicknessProperty =
            DependencyProperty.Register("OutlineThickness", typeof(double), typeof(DanmakuTextControl), new PropertyMetadata(1.0));

        public TimeSpan ExitTime { get; set; }

        public DMItem Dm { get; set; }

        public double Speed { get; set; }

        private void DrawText()
        {
            if (ActualHeight == 0 || ActualWidth == 0 || string.IsNullOrWhiteSpace(Text) || _drawingSurface == null)
                return;

            var width = (float)ActualWidth;
            var height = (float)ActualHeight;
            //using var session = CanvasComposition.CreateDrawingSession(_drawingSurface, new Windows.Foundation.Rect { X = 0, Y = 0, Width = width, Height = height}, (float)XamlRoot.RasterizationScale * 96);
            using var session = CanvasComposition.CreateDrawingSession(_drawingSurface);
            session.Clear(Colors.Transparent);

            using var textFormat = new CanvasTextFormat()
            {
                FontSize = (float)FontSize,
                Direction = CanvasTextDirection.LeftToRightThenTopToBottom,
                VerticalAlignment = CanvasVerticalAlignment.Top,
                HorizontalAlignment = CanvasHorizontalAlignment.Left,
                FontFamily = FontFamily.Source,
            };

            using var textLayout = new CanvasTextLayout(session, Text, textFormat, width, height);
            var offset = (float)(OutlineThickness / 2);
            using var geometry = CanvasGeometry.CreateText(textLayout);
            
            using var dashedStroke = new CanvasStrokeStyle()
            {
                DashStyle = CanvasDashStyle.Solid,
            };
            session.DrawGeometry(geometry, OutlineColor, (float)OutlineThickness, dashedStroke);
            session.DrawTextLayout(textLayout, 0, 0, FontColor);

            //session.DrawTextLayout(textLayout, offset, offset, FontColor);
        }
    }
}
