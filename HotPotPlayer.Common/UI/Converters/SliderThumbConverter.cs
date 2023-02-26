using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.UI.Converters
{
    public class SliderThumbConverter : DependencyObject, IValueConverter
    {
        public TimeSpan? TotalTime
        {
            get { return (TimeSpan?)GetValue(TotalTimeProperty); }
            set { SetValue(TotalTimeProperty, value); }
        }

        public static readonly DependencyProperty TotalTimeProperty =
            DependencyProperty.Register("TotalTime", typeof(TimeSpan?), typeof(SliderThumbConverter), new PropertyMetadata(TimeSpan.Zero));


        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (TotalTime == null)
            {
                return "--:--";
            }
            var percent100 = (int)(double)value;
            var v = percent100 * ((TimeSpan)TotalTime).Ticks / 100;
            var t = TimeSpan.FromTicks(v);
            if (t.Hours > 0)
            {
                return t.ToString("hh\\:mm\\:ss");
            }
            return t.ToString("mm\\:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
