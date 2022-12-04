using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Converters
{
    public class GridIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var presenter = value as ContentPresenter;
            var parent = VisualTreeHelper.GetParent(presenter);
            parent = VisualTreeHelper.GetParent(parent);
            var item = parent as SelectorItem;

            var listView = ItemsControl.ItemsControlFromItemContainer(item);
            int index = listView.IndexFromContainer(item) + 1;
            return index.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
