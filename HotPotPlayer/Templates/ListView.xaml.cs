using HotPotPlayer.Controls.BilibiliSub;
using HotPotPlayer.Services.BiliBili.Dynamic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
    }
}
