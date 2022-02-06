using HotPotPlayer.Models;
using Microsoft.UI.Xaml;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace HotPotPlayer.Extensions
{
    internal static class CoverExtensions
    {
        static readonly MD5 md5 = MD5.Create();

        public static void SetPlayListCover(this PlayListItem i, AppBase app)
        {
            var seg = i.MusicItems.Count / 9;
            var covers = Enumerable.Range(0, 9).Select(ind => i.MusicItems[ind * seg]).Select(ind => Image.Load<Rgba32>(ind.Cover));
            var smallCovers = covers.Select(co => {
                if (co.Width > co.Height)
                {
                    co.Mutate(x => x.Crop(new Rectangle((co.Width - co.Height) / 2, 0, co.Height, co.Height)).Resize(new Size(135, 135)));
                }
                else
                {
                    co.Mutate(x => x.Crop(new Rectangle(0, (co.Height - co.Width) / 2, co.Width, co.Width)).Resize(new Size(135, 135)));
                }
                //co.Mutate(x => x.ConvertToAvatar(new Size(135, 135), 0));

                return co;
            });
            using var image = new Image<Rgba32>(400, 400);
            var _ = smallCovers.Select((co, ind) =>
            {
                var row = ind / 3;
                var col = ind % 3;
                image.Mutate(x => x.DrawImage(co, new Point(col * 133, row * 133), 1));
                co.Dispose();
                return "";
            }).ToList();

            var baseDir = app.LocalFolder;
            var albumCoverDir = System.IO.Path.Combine(baseDir, "Cover");

            var _IMemoryGroup = image.GetPixelMemoryGroup();
            var _MemoryGroup = _IMemoryGroup.ToArray()[0];
            var PixelData = MemoryMarshal.AsBytes(_MemoryGroup.Span).ToArray();
            var buffer = md5.ComputeHash(PixelData);
            var hashName = Convert.ToHexString(buffer);
            var albumCoverName = System.IO.Path.Combine(albumCoverDir, hashName);

            image.SaveAsPng(albumCoverName);

            i.Cover = albumCoverName;
        }

        // https://github.com/SixLabors/Samples/blob/master/ImageSharp/AvatarWithRoundedCorner/Program.cs
        // Implements a full image mutating pipeline operating on IImageProcessingContext
        private static IImageProcessingContext ConvertToAvatar(this IImageProcessingContext processingContext, Size size, float cornerRadius)
        {
            return processingContext.Resize(new ResizeOptions
            {
                Size = size,
                Mode = ResizeMode.Crop
            }).ApplyRoundedCorners(cornerRadius);
        }


        // This method can be seen as an inline implementation of an `IImageProcessor`:
        // (The combination of `IImageOperations.Apply()` + this could be replaced with an `IImageProcessor`)
        private static IImageProcessingContext ApplyRoundedCorners(this IImageProcessingContext ctx, float cornerRadius)
        {
            Size size = ctx.GetCurrentSize();
            IPathCollection corners = BuildCorners(size.Width, size.Height, cornerRadius);

            ctx.SetGraphicsOptions(new GraphicsOptions()
            {
                Antialias = true,
                AlphaCompositionMode = PixelAlphaCompositionMode.DestOut // enforces that any part of this shape that has color is punched out of the background
            });

            // mutating in here as we already have a cloned original
            // use any color (not Transparent), so the corners will be clipped
            foreach (var c in corners)
            {
                ctx = ctx.Fill(Color.Red, c);
            }
            return ctx;
        }

        private static IPathCollection BuildCorners(int imageWidth, int imageHeight, float cornerRadius)
        {
            // first create a square
            var rect = new RectangularPolygon(-0.5f, -0.5f, cornerRadius, cornerRadius);

            // then cut out of the square a circle so we are left with a corner
            IPath cornerTopLeft = rect.Clip(new EllipsePolygon(cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius));

            // corner is now a corner shape positions top left
            //lets make 3 more positioned correctly, we can do that by translating the original around the center of the image

            float rightPos = imageWidth - cornerTopLeft.Bounds.Width + 1;
            float bottomPos = imageHeight - cornerTopLeft.Bounds.Height + 1;

            // move it across the width of the image - the width of the shape
            IPath cornerTopRight = cornerTopLeft.RotateDegree(90).Translate(rightPos, 0);
            IPath cornerBottomLeft = cornerTopLeft.RotateDegree(-90).Translate(0, bottomPos);
            IPath cornerBottomRight = cornerTopLeft.RotateDegree(180).Translate(rightPos, bottomPos);

            return new PathCollection(cornerTopLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight);
        }
    }
}
