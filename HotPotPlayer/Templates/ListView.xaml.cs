using HotPotPlayer.Controls.BilibiliSub;
using HotPotPlayer.Pages.BilibiliSub;
using HotPotPlayer.Services.BiliBili.Dynamic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Templates
{
    internal partial class ListView
    {
        public ListView()
        {
            InitializeComponent();
        }

        private void RichTextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            var rich = sender as RichTextBlock;
            var data = rich.DataContext as DynamicItem;
            if (!data.Modules.HasInteraction || rich.Tag != null) { return; }
            rich.Blocks.Add(data.Modules.ModuleInteraction.Items[0].Desc.GenRichText);
            rich.Tag = 1;
        }

        private void RichTextBlock_Loaded2(object sender, RoutedEventArgs e)
        {
            var rich = sender as RichTextBlock;
            var data = rich.DataContext as DynamicItem;
            if (!data.Modules.ModuleDynamic.HasDesc || rich.Tag != null) { return; }
            rich.Blocks.Add(data.Modules.ModuleDynamic.Desc.GenRichText);
            rich.Tag = 1;
        }

        private void UserAvatarClick(object sender, RoutedEventArgs e)
        {
            var flyout = ((UIElement)sender).ContextFlyout as Flyout;
            var card = flyout.Content as UserCard;
            card.LoadUserCardBundle();
            flyout.ShowAt(sender as FrameworkElement);
        }

        private void DynamicCommentClick(object sender, RoutedEventArgs e)
        {
            var ui = sender as FrameworkElement;
            var dynamic = GetDynamicParent(ui);

            static Dynamic GetDynamicParent(DependencyObject v)
            {
                while (v != null)
                {
                    v = VisualTreeHelper.GetParent(v);
                    if (v is Dynamic)
                        break;
                }
                return v as Dynamic;
            }

            dynamic.ToggleComment(ui.DataContext as DynamicItem);
        }

        private void StaffTapped(object sender, TappedRoutedEventArgs e)
        {
            var grid = sender as Grid;
            var flyout = grid.ContextFlyout as Flyout;
            var f = flyout.Content as UserCard;
            f.LoadUserCardBundle();
            flyout.ShowAt(grid);
        }
    }
}
