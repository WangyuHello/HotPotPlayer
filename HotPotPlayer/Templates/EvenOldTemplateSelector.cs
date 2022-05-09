using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Templates
{
    public class EvenOldTemplateSelector : DataTemplateSelector
    {
        public DataTemplate EvenTemplate { get; set; }
        public DataTemplate OddTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var listView = (Microsoft.UI.Xaml.Controls.ListView)ItemsControl.ItemsControlFromItemContainer(container);

            var list = listView.Items;
            var ind = list.IndexOf(item);
            if (IsOdd(ind))
            {
                return OddTemplate;
            }
            return EvenTemplate;
        }

        public static bool IsOdd(int n)
        {
            return Convert.ToBoolean(n % 2);
        }
    }
}
