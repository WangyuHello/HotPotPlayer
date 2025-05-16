using HotPotPlayer.Bilibili.Models.Dynamic;
using Microsoft.Kiota.Abstractions;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media.Imaging;
using Richasy.BiliKernel.Models.Appearance;
using Richasy.BiliKernel.Models.Media;
using Richasy.BiliKernel.Models.Moment;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotPotPlayer.UI.Extensions
{
    public static class BiliUIExtensions
    {
        public static Visibility IsDescVisible(MomentInformation moment)
        {
            var vis = moment.Description != null && !string.IsNullOrEmpty(moment.Description.Text);
            return vis ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility IsArticleVisible(MomentInformation moment)
        {
            return moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Article ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility IsSingleImage(MomentInformation moment)
        {
            var isImage = moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Image;
            if (isImage && moment.Data is List<BiliImage> images)
            {
                var isSingle = images.Count == 1;
                return isSingle ? Visibility.Visible : Visibility.Collapsed;
            }
            var isVideo = moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Video;
            if (isVideo)
            {
                return isVideo ? Visibility.Visible : Visibility.Collapsed;
            }
            var isArticle = moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Article;
            if (isArticle)
            {

            }
            return isImage ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Uri GetSingleImageSource(MomentInformation moment)
        {
            var isImage = moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Image;
            if (isImage && moment.Data is List<BiliImage> images)
            {
                return images[0].Uri;
            }
            var isVideo = moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Video;
            if (isVideo && moment.Data is VideoInformation v)
            {
                return v.Identifier.Cover.Uri;
            }
            var isArticle = moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Article;
            if (isArticle)
            {

            }
            return null;
        }

        public static double GetSingleImageWidth(MomentInformation moment)
        {
            var isImage = moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Image;
            if (isImage && moment.Data is List<BiliImage> images)
            {
                return images[0].Width > 560 ? double.NaN : images[0].Width;
            }
            var isVideo = moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Video;
            if (isVideo && moment.Data is VideoInformation v)
            {
                return v.Identifier.Cover.Width > 560 ? double.NaN : v.Identifier.Cover.Width;
            }
            var isArticle = moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Article;
            if (isArticle)
            {

            }
            return double.NaN;
        }

        //public static double GetSingleImageHeight(MomentInformation moment)
        //{
        //    var isImage = moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Image;
        //    if (isImage && moment.Data is List<BiliImage> images)
        //    {
        //        return images[0].Height > 560 ? double.NaN : images[0].Height;
        //    }
        //    var isVideo = moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Video;
        //    if (isVideo && moment.Data is VideoInformation v)
        //    {
        //        return v.Identifier.Cover.Height;
        //    }
        //    var isArticle = moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Article;
        //    if (isArticle)
        //    {

        //    }
        //    return double.NaN;
        //}

        public static string GetDuration(VideoInformation v)
        {
            var t = TimeSpan.FromSeconds(v.Duration ?? 0);
            if (t.Hours > 0)
            {
                return t.ToString("hh\\:mm\\:ss");
            }
            else
            {
                return t.ToString("mm\\:ss");
            }
        }

        public static string GetDateTime(this int i)
        {
            var ts = TimeSpan.FromSeconds(i);
            var time = new DateTime(ts.Ticks);
            time = time.AddYears(1969);
            var t2 = time.ToLocalTime();
            return $"{time.Year}-{time.Month}-{time.Day} {t2:HH\\:mm\\:ss}";
        }

        //public static string GetViewDateTime(VideoInformation video)
        //{
            
        //}

        public static Visibility HasRcmdReasonContent(this VideoInformation v)
        {
            return v.ExtensionData.ContainsKey("RecommendReason") ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility NoRcmdReasonContent(this VideoInformation v)
        {
            return v.ExtensionData.ContainsKey("RecommendReason") ? Visibility.Collapsed : Visibility.Visible;
        }

        public static string GetRcmdReason(this VideoInformation v)
        {
            v.ExtensionData.TryGetValue("RecommendReason", out var s);
            if (s != null) return s.ToString();
            return string.Empty;
        }

        public static string GetUpDateTime(this VideoInformation v)
        {
            var time = v.PublishTime;
            if (time is DateTimeOffset d)
            {
                return $"{d.Month}-{d.Day}";
            }
            return string.Empty;
        }

        public static string GetPlayCount(VideoInformation video)
        {
            double v = video.CommunityInformation.PlayCount ?? 0;
            return NumberFormat(v);
        }

        private static string NumberFormat(double v)
        {
            if (v >= 10000)
            {
                var v2 = v / 10000;
                return $"{v2:F1}万";
            }
            else
            {
                return v.ToString();
            }
        }

        public static string GetDanmakuCount(VideoInformation video)
        {
            double v = video.CommunityInformation.DanmakuCount ?? 0;
            return NumberFormat(v);
        }

        public static string GetCommentCount(VideoInformation video)
        {
            double v = video.CommunityInformation.CommentCount ?? 0;
            return NumberFormat(v);
        }

        public static RichTextBlock GenRichTextBlock(this EmoteText node)
        {
            var b = new RichTextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                IsTextSelectionEnabled = true,
            };
            if (node != null)
            {
                b.Blocks.Add(GenRichText(node));
            }
            return b;
            
        }

        public static Paragraph GenRichText(this EmoteText node)
        {
            var par = new Paragraph();
            //IEnumerable<Inline> inlines = node.RichTextNodes.Select<DescNodes, Inline>(r =>
            //    r.Type switch
            //    {
            //        "RICH_TEXT_NODE_TYPE_EMOJI" => new InlineUIContainer
            //        {
            //            Child = new Image
            //            {
            //                Source = new BitmapImage(new Uri(r.Emoji.IconUrl))
            //                {
            //                    DecodePixelHeight = 15,
            //                    DecodePixelWidth = 15
            //                },
            //                Width = 15,
            //                Height = 15,
            //            }
            //        },
            //        _ => new Run
            //        {
            //            Text = r.Text,
            //            FontWeight = r.Type switch
            //            {
            //                "RICH_TEXT_NODE_TYPE_AT" => FontWeights.Bold,
            //                _ => FontWeights.Normal,
            //            }
            //        },
            //    }
            //);

            //foreach (var item in inlines)
            //{
            //    par.Inlines.Add(item);
            //}
            var inline = new Run
            {
                Text = node.Text,
            };
            par.Inlines.Add(inline);
            return par;
        }
    }
}
