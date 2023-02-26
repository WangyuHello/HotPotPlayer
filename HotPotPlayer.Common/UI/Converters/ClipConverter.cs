using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Sensors;
using Windows.Foundation;

namespace HotPotPlayer.UI.Converters
{
    public class ClipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var userCont = value as Rectangle;
            RectangleGeometry rectangle = new RectangleGeometry();
            rectangle.Rect = new Rect(0, 0, userCont.ActualWidth, userCont.ActualHeight);
            return rectangle;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
