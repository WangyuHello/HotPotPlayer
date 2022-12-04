using Microsoft.UI.Xaml.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Converters
{
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var c = (Color)value;
            var p = c.ToPixel<Bgra32>();
            return Windows.UI.Color.FromArgb(p.A, p.R, p.G, p.B);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var c = (Windows.UI.Color)value;
            return Color.FromRgba(c.R, c.G, c.B, c.A);
        }
    }
}
