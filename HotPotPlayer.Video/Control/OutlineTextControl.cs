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

namespace HotPotPlayer.Video.Control
{
    public class OutlineTextControl : Microsoft.UI.Xaml.Controls.Control
    {
        private CompositionDrawingSurface _drawingSurface;

        public OutlineTextControl()
        {
            var compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            var graphicsDevice = CanvasComposition.CreateCompositionGraphicsDevice(compositor, CanvasDevice.GetSharedDevice());
            var spriteTextVisual = compositor.CreateSpriteVisual();

            ElementCompositionPreview.SetElementChildVisual(this, spriteTextVisual);
            SizeChanged += (s, e) =>
            {
                _drawingSurface = graphicsDevice.CreateDrawingSurface(e.NewSize, DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
                DrawText();
                var maskSurfaceBrush = compositor.CreateSurfaceBrush(_drawingSurface);
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


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(OutlineTextControl), new PropertyMetadata(default));

        public Color FontColor
        {
            get { return (Color)GetValue(FontColorProperty); }
            set { SetValue(FontColorProperty, value); }
        }

        public static readonly DependencyProperty FontColorProperty =
            DependencyProperty.Register("FontColor", typeof(Color), typeof(OutlineTextControl), new PropertyMetadata(Colors.White));


        public Color OutlineColor
        {
            get { return (Color)GetValue(OutlineColorProperty); }
            set { SetValue(OutlineColorProperty, value); }
        }

        public static readonly DependencyProperty OutlineColorProperty =
            DependencyProperty.Register("OutlineColor", typeof(Color), typeof(OutlineTextControl), new PropertyMetadata(Colors.Black));



        public double OutlineThickness
        {
            get { return (double)GetValue(OutlineThicknessProperty); }
            set { SetValue(OutlineThicknessProperty, value); }
        }

        public static readonly DependencyProperty OutlineThicknessProperty =
            DependencyProperty.Register("OutlineThickness", typeof(double), typeof(OutlineTextControl), new PropertyMetadata(1.0));

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
            session.DrawTextLayout(textLayout, 0, 0, FontColor);
            session.DrawGeometry(geometry, OutlineColor, (float)OutlineThickness, dashedStroke);
            
            //session.DrawTextLayout(textLayout, offset, offset, FontColor);
        }
    }
}
