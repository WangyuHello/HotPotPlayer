using Blurhash;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HotPotPlayer.Models
{
    public record ImageSourceWithBlur
    {
        public BaseItemDto_ImageBlurHashes BlurHashes { get; set; }
        public BaseItemDto_ImageTags ImageTags { get; set; }
        public List<string> BackdropImageTags { get; set; }
        public string Label { get; set; }
        public Guid Parent { get; set; }

        public async Task<WriteableBitmap> GetBlurImageAsync(int width, int height, CancellationToken cancel)
        {
            if (BlurHashes == null) return null;
            string blur = string.Empty;
            if (Label == "Primary")
            {
                if (BlurHashes.Primary == null) return null;
                if (BlurHashes.Primary.AdditionalData == null) return null;
                if (BlurHashes.Primary.AdditionalData.Count == 0) return null;
                blur = BlurHashes.Primary.AdditionalData.First().Value.ToString();

            }
            else if (Label == "Backdrop")
            {
                if (BlurHashes.Backdrop == null) return null;
                if (BlurHashes.Backdrop.AdditionalData == null) return null;
                if (BlurHashes.Backdrop.AdditionalData.Count == 0) return null;
                blur = BlurHashes.Backdrop.AdditionalData.First().Value.ToString();
            }

            var wb = new WriteableBitmap(width, height);
            var data = await Task.Run(() =>
            {
                var pixelData = new Pixel[width, height];
                Core.Decode(blur, pixelData, 1);

                var data = new byte[width * height * 4];
                var index = 0;
                for (var yPixel = 0; yPixel < height; yPixel++)
                    for (var xPixel = 0; xPixel < width; xPixel++)
                    {
                        if (cancel.IsCancellationRequested) return null;
                        var pixel = pixelData[xPixel, yPixel];

                        data[index++] = (byte)MathUtils.LinearTosRgb(pixel.Blue);
                        data[index++] = (byte)MathUtils.LinearTosRgb(pixel.Green);
                        data[index++] = (byte)MathUtils.LinearTosRgb(pixel.Red);
                        data[index++] = 255;
                    }

                return new Memory<byte>(data);
            });

            if (cancel.IsCancellationRequested) return null;
            using Stream stream = wb.PixelBuffer.AsStream();
            await stream.WriteAsync(data, cancel);

            return wb;
        }

        public Uri GetImageUri(int width, int height)
        {
            var JellyfinMusicService = ((IComponentServiceLocator)Application.Current).JellyfinMusicService;
            if (Label == "Primary")
            {
                return JellyfinMusicService.GetPrimaryJellyfinImageWidth(ImageTags, Parent, width);
            }
            else if(Label == "Backdrop")
            {
                return JellyfinMusicService.GetBackdropJellyfinImage(BackdropImageTags, Parent, width);
            }
            return null;
        }
    }
}
