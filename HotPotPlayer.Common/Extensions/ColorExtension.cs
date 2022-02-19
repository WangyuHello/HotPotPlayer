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
    }
}
