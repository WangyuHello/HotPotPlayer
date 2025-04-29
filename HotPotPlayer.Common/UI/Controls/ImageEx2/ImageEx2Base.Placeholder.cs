using Blurhash;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Drawing.Blurhash;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace HotPotPlayer.UI.Controls
{
    /// <summary>
    /// Base code for ImageEx
    /// </summary>
    public partial class ImageEx2Base
    {
        /// <summary>
        /// Identifies the <see cref="PlaceholderSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PlaceholderSourceProperty = DependencyProperty.Register(
            nameof(PlaceholderSource),
            typeof(object),
            typeof(ImageEx2Base),
            new PropertyMetadata(default(object), PlaceholderSourceChanged));

        /// <summary>
        /// Identifies the <see cref="PlaceholderStretch"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PlaceholderStretchProperty = DependencyProperty.Register(
            nameof(PlaceholderStretch),
            typeof(Stretch),
            typeof(ImageEx2Base),
            new PropertyMetadata(default(Stretch)));

        /// <summary>
        /// Gets or sets the placeholder source.
        /// </summary>
        /// <value>
        /// The placeholder source.
        /// </value>
        public object PlaceholderSource
        {
            get { return (object)GetValue(PlaceholderSourceProperty); }
            set { SetValue(PlaceholderSourceProperty, value); }
        }

        private static void PlaceholderSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageEx2Base control)
            {
                control.OnPlaceholderSourceChanged(e);
            }
        }

        /// <summary>
        /// Invoked when Placeholder source has changed
        /// </summary>
        /// <param name="e">Event args</param>
        protected virtual async void OnPlaceholderSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            if(PlaceholderImage is Image image)
            {
                if (e.NewValue is ImageSource s)
                {
                    image.Source = s;
                }
                else if (e.NewValue is string blur)
                {
                    var width = DecodePixelWidth;
                    var height = DecodePixelHeight;
                    var wb = new WriteableBitmap(width, height);
                    var data = await Task.Run(() =>
                    {
                        var pixelData = new Pixel[width, height];
                        Core.Decode(blur, pixelData, 1.5);

                        var data = new byte[width * height * 4];
                        var index = 0;
                        for (var yPixel = 0; yPixel < height; yPixel++)
                        for (var xPixel = 0; xPixel < width; xPixel++)
                        {
                            var pixel = pixelData[xPixel, yPixel];

                            data[index++] = (byte)MathUtils.LinearTosRgb(pixel.Blue);
                            data[index++] = (byte)MathUtils.LinearTosRgb(pixel.Green);
                            data[index++] = (byte)MathUtils.LinearTosRgb(pixel.Red);
                            data[index++] = 255;
                        }

                        return new Memory<byte>(data);
                    });

                    using Stream stream = wb.PixelBuffer.AsStream();
                    await stream.WriteAsync(data);
                    image.Source = wb;
                }
            }
        }

        /// <summary>
        /// Gets or sets the placeholder stretch.
        /// </summary>
        /// <value>
        /// The placeholder stretch.
        /// </value>
        public Stretch PlaceholderStretch
        {
            get { return (Stretch)GetValue(PlaceholderStretchProperty); }
            set { SetValue(PlaceholderStretchProperty, value); }
        }
    }
}