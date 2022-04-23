using HotPotPlayer.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Threading.Tasks;

namespace HotPotPlayer.Media
{
    public class AcrylicBrushFromImage: AcrylicBrush
    {
        public Uri ImageSource
        {
            get { return (Uri)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(Uri), typeof(AcrylicBrushFromImage), new PropertyMetadata(default(Uri), ImageChanged));


        private static void ImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var brush = d as AcrylicBrushFromImage;
            if (brush == null) { return; }
            if (e.NewValue == null || e.NewValue.Equals(e.OldValue)) { return; }
            brush.SetImage((Uri)e.NewValue);
        }

        private async void SetImage(Uri uri)
        {
            var imgFile = await ImageCacheEx.Instance.GetFileFromCacheAsync(uri);
            using var image = Image.Load<Rgba32>(imgFile.Path);
            var color = GetMainColor(image);
            TintColor = color;
        }

        static Windows.UI.Color GetMainColor(Image<Rgba32> image)
        {
            var wQua = image.Width >> 2;
            var hQua = image.Height >> 2;
            var centerX = image.Width >> 1;
            var centerY = image.Height >> 1;
            var pix = image[centerX, centerY];
            var pix1 = image[wQua, hQua];
            var pix2 = image[3 * wQua, hQua];
            var pix3 = image[wQua, 3 * hQua];
            var pix4 = image[3 * wQua, 3 * hQua];
            int a = (pix.A + pix1.A + pix2.A + pix3.A + pix4.A) / 5;
            int r = (pix.R + pix1.R + pix2.R + pix3.R + pix4.R) / 5;
            int g = (pix.G + pix1.G + pix2.G + pix3.G + pix4.G) / 5;
            int b = (pix.B + pix1.B + pix2.B + pix3.B + pix4.B) / 5;
            return Windows.UI.Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);
        }
    }
}
