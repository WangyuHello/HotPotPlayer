using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Extensions
{
    internal static class ColorExtension
    {
        public static int ToInt(this Color color)
        {
            var pix = color.ToPixel<Bgra32>();
            return (pix.A << 24) + (pix.R << 16) + (pix.G << 8) + pix.B;
        }

        public static Color ToColor(this int color)
        {
            byte a = (byte)((color >> 24) & 0xFF);
            byte r = (byte)((color >> 16) & 0xFF);
            byte g = (byte)((color >> 8) & 0xFF);
            byte b = (byte)(color & 0xFF);
            return Color.FromRgba(r, g, b, a);
        }

        public static Color GetMainColor(this Image<Rgba32> image)
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
            return Color.FromRgba((byte)r, (byte)g, (byte)b, (byte)a);
        }
    }
}
