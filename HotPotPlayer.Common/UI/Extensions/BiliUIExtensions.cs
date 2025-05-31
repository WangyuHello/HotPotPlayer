using HotPotPlayer.Bilibili.Models.Dynamic;
using HotPotPlayer.Extensions;
using Microsoft.Kiota.Abstractions;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Richasy.BiliKernel.Models.Appearance;
using Richasy.BiliKernel.Models.Comment;
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
            if (moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Forward)
            {
                var m2 = moment.Data as MomentInformation;
                return IsDescVisible(m2);
            }
            var vis = moment.Description != null && !string.IsNullOrEmpty(moment.Description.Text);
            return vis ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility IsArticleVisible(MomentInformation moment)
        {
            return moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Article ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility IsSingleImage(MomentInformation moment)
        {
            if (moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Forward)
            {
                var m2 = moment.Data as MomentInformation;
                return IsSingleImage(m2);
            }
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
        public static Visibility IsMultiImage(MomentInformation moment)
        {
            if (moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Forward)
            {
                var m2 = moment.Data as MomentInformation;
                return IsMultiImage(m2);
            }
            var isImage = moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Image;
            if (isImage && moment.Data is List<BiliImage> images)
            {
                var isMulti = images.Count != 1;
                return isMulti ? Visibility.Visible : Visibility.Collapsed;
            }
            var isVideo = moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Video;
            if (isVideo)
            {
                return isVideo ? Visibility.Collapsed : Visibility.Visible;
            }
            var isArticle = moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Article;
            if (isArticle)
            {

            }
            return Visibility.Collapsed;
        }

        public static Uri GetSingleImageSource(MomentInformation moment)
        {
            if (moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Forward)
            {
                var m2 = moment.Data as MomentInformation;
                return GetSingleImageSource(m2);
            }
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

        public static List<BiliImage> GetMultiImageSource(MomentInformation moment)
        {
            if (moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Forward)
            {
                var m2 = moment.Data as MomentInformation;
                return GetMultiImageSource(m2);
            }
            var isImage = moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Image;
            if (isImage && moment.Data is List<BiliImage> images)
            {
                return images;
            }
            return null;
        }

        public static double GetSingleImageWidth(MomentInformation moment)
        {
            if (moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Forward)
            {
                var m2 = moment.Data as MomentInformation;
                return GetSingleImageWidth(m2);
            }
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
        public static Visibility GetVideoStatVisible(MomentInformation moment)
        {
            if (moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Forward)
            {
                var m2 = moment.Data as MomentInformation;
                return GetVideoStatVisible(m2);
            }
            var isVideo = moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Video;
            return isVideo ? Visibility.Visible : Visibility.Collapsed;
        }

        public static string GetVideoDuration(MomentInformation moment)
        {
            if (moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Forward)
            {
                var m2 = moment.Data as MomentInformation;
                return GetVideoDuration(m2);
            }
            if (moment.Data is VideoInformation video)
            {
                return GetDuration(video);
            }
            return string.Empty;
        }

        public static string GetVideoPlayCount(MomentInformation moment)
        {
            if (moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Forward)
            {
                var m2 = moment.Data as MomentInformation;
                return GetVideoPlayCount(m2);
            }
            if (moment.Data is VideoInformation video)
            {
                return GetPlayCount(video) + "观看";
            }
            return string.Empty;
        }

        public static string GetVideoDanmakuCount(MomentInformation moment)
        {
            if (moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Forward)
            {
                var m2 = moment.Data as MomentInformation;
                return GetVideoDanmakuCount(m2);
            }
            if (moment.Data is VideoInformation video)
            {
                return GetDanmakuCount(video) + "弹幕";
            }
            return string.Empty;
        }

        public static string GetTitle(MomentInformation moment)
        {
            if (moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Forward)
            {
                var m2 = moment.Data as MomentInformation;
                return GetTitle(m2);
            }
            if (moment.Data is VideoInformation video)
            {
                return video.Identifier.Title;
            }
            return string.Empty;
        }

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
            if (v == null)
            {
                return string.Empty;
            }
            var time = v.PublishTime;
            if (time is DateTimeOffset d)
            {
                return GetDateTimeStr(d);
            }
            return string.Empty;
        }

        public static string GetDateTimeStr(DateTimeOffset d)
        {
            return d.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string GetPlayCount(VideoInformation video)
        {
            double v = video.CommunityInformation.PlayCount ?? 0;
            return NumberFormat(v);
        }

        public static string NumberFormat(int v)
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

        public static string NumberFormat(double? v)
        {
            v ??= 0;
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

        public static Visibility GetForwardingVisible(MomentInformation moment)
        {
            var isForwarding = moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Forward;
            return isForwarding ? Visibility.Visible : Visibility.Collapsed;
        }

        public static RichTextBlock GetMainContent(MomentInformation moment)
        {
            if (moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Forward)
            {
                var m2 = moment.Data as MomentInformation;
                return GetMainContent(m2);
            }
            return GenRichTextBlock(moment.Description);
        }

        public static RichTextBlock GenRichTextBlock(this EmoteText node)
        {
            var b = new RichTextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                IsTextSelectionEnabled = true,
                FontFamily = (FontFamily)Application.Current.Resources["MiSansNormal"]
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
            if (node.Emotes == null || node.Emotes.Count == 0)
            {
                var inline = new Run
                {
                    Text = node.Text,
                };
                par.Inlines.Add(inline);
                return par;
            }
            var inlines = new List<Inline>();
            var text = node.Text;
            int state = 0;
            int start = -1;
            int end = -1;
            int end2 = -1;
            string token = string.Empty;

            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (state == 0)
                {
                    if (c == '[')
                    {
                        state = 1;
                        start = i;
                    }
                    if (i == text.Length - 1)
                    {
                        // Last one
                        inlines.Add(new Run
                        {
                            Text = text[(end+1)..]
                        });
                    }
                }
                else
                {
                    if (c == ']')
                    {
                        state = 0;
                        end2 = i;
                        token = text[start..(end2+1)];
                        var has = node.Emotes.TryGetValue(token, out var image);
                        if (has)
                        {
                            if (start > end + 1)
                            {
                                inlines.Add(new Run
                                {
                                    Text = text[(end+1)..start]
                                });
                            }
                            inlines.Add(new InlineUIContainer
                            {
                                Child = new Image
                                {
                                    Source = new BitmapImage(image.Uri)
                                    {
                                        DecodePixelWidth = 48,
                                        DecodePixelHeight = 48,
                                    },
                                    Width = 48,
                                    Height = 48,
                                }
                            });
                        }
                        end = end2;
                    }
                }
            }

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

            foreach (var item in inlines)
            {
                par.Inlines.Add(item);
            }

            return par;
        }

        public static SolidColorBrush GetForwardingBackground(MomentInformation moment)
        {
            var isforwarding = moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Forward;
            return new SolidColorBrush(isforwarding ? Windows.UI.Color.FromArgb(255, 246, 247, 248) : 
                Windows.UI.Color.FromArgb(255,255,255,255));
        }

        public static Uri GetForwardingUserAvatar(MomentInformation moment)
        {
            var isforwarding = moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Forward;
            if (isforwarding)
            {
                var m2 = moment.Data as MomentInformation;
                return m2.User.Avatar.Uri;
            }
            return null;
        }

        public static string GetForwardingUserName(MomentInformation moment)
        {
            var isforwarding = moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Forward;
            if (isforwarding)
            {
                var m2 = moment.Data as MomentInformation;
                return m2.User.Name;
            }
            return null;
        }

        public static Thickness GetForwardingWrapperPadding(MomentInformation moment)
        {
            var isforwarding = moment.MomentType == Richasy.BiliKernel.Models.MomentItemType.Forward;
            if (isforwarding)
            {
                return new Thickness(16, 16, 16, 16);
            }
            return new Thickness(0, 0, 0, 0);
        }

        public static Visibility GetNestedReplyVisible(CommentInformation comment)
        {
            var hasNested = comment.CommunityInformation.ChildCount > 0;
            return hasNested ? Visibility.Visible : Visibility.Collapsed;
        }

        public static string GetNestedReplyStr(CommentInformation comment)
        {
            return $"共{comment.CommunityInformation.ChildCount}条回复 >";
        } 

    }
}
