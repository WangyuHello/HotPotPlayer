using HotPotPlayer.Bilibili.Models.Dynamic;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotPotPlayer.UI.Extensions
{
    public static class BiliUIExtensions
    {
        public static RichTextBlock GenRichTextBlock(this ModuleDesc node)
        {

            var b = new RichTextBlock
            {
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                IsTextSelectionEnabled = true,
            };
            b.Blocks.Add(GenRichText(node));
            return b;
            
        }

        public static Paragraph GenRichText(this ModuleDesc node)
        {
                var par = new Paragraph();
                IEnumerable<Inline> inlines = node.RichTextNodes.Select<DescNodes, Inline>(r =>
                    r.Type switch
                    {
                        "RICH_TEXT_NODE_TYPE_EMOJI" => new InlineUIContainer
                        {
                            Child = new Image
                            {
                                Source = new BitmapImage(new Uri(r.Emoji.IconUrl))
                                {
                                    DecodePixelHeight = 15,
                                    DecodePixelWidth = 15
                                },
                                Width = 15,
                                Height = 15,
                            }
                        },
                        _ => new Run
                        {
                            Text = r.Text,
                            FontWeight = r.Type switch
                            {
                                "RICH_TEXT_NODE_TYPE_AT" => FontWeights.Bold,
                                _ => FontWeights.Normal,
                            }
                        },
                    }
                );

                foreach (var item in inlines)
                {
                    par.Inlines.Add(item);
                }
                return par;
            
        }


    }
}
