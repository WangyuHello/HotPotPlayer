using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.UI.Templates.Selectors
{

    public class SelectedTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SelectedTemplate { get; set; }
        public DataTemplate NormalTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (container is ListViewItem cont)
            {
                if (cont.Tag != null && long.TryParse(cont.Tag.ToString(), out var token))
                {
                    cont.UnregisterPropertyChangedCallback(ListViewItem.IsSelectedProperty, token);
                }

                cont.Tag = cont.RegisterPropertyChangedCallback(ListViewItem.IsSelectedProperty, (s, e) =>
                {
                    cont.ContentTemplateSelector = null;
                    cont.ContentTemplateSelector = this;
                });

                if (cont.IsSelected)
                {
                    return SelectedTemplate;
                }

                return NormalTemplate;
            }

            return NormalTemplate;
        }
    }
}
