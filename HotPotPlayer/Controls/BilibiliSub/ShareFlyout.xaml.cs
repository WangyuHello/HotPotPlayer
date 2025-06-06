// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using HotPotPlayer.Bilibili.Models.Video;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using QRCoder;
using Richasy.BiliKernel.Models.Media;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Controls.BilibiliSub
{
    public sealed partial class ShareFlyout : UserControlBase
    {
        public ShareFlyout()
        {
            this.InitializeComponent();
        }

        public VideoInformation Video
        {
            get { return (VideoInformation)GetValue(VideoProperty); }
            set { SetValue(VideoProperty, value); }
        }

        public static readonly DependencyProperty VideoProperty =
            DependencyProperty.Register("Video", typeof(VideoInformation), typeof(ShareFlyout), new PropertyMetadata(default));

        private async void OpenWebClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://www.bilibili.com/video/" + Video.BvId));
        }

        byte[] GetQrImgByte(string url)
        {
            var qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            BitmapByteQRCode qrCode = new(qrCodeData);
            byte[] qrCodeAsBitmapByteArr = qrCode.GetGraphic(20);
            return qrCodeAsBitmapByteArr;
        }

        byte[] qrData;
        private async Task SetShareQrImage(VideoInformation video)
        {
            qrData = GetQrImgByte("https://m.bilibili.com/video/"+video.BvId);
            BitmapImage image = new();
            var stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(qrData.AsBuffer());
            stream.Seek(0);
            await image.SetSourceAsync(stream);
            QR.Source = image;
        }

        public async void Init()
        {
            await SetShareQrImage(Video);
        }

        private async void ShareImageClick(object sender, RoutedEventArgs e)
        {
            
            //using Image<Rgba32> image = new(1320, 600, new Rgba32(255,255,255,255));

            //var coverUrl = Video.VideoImage;

            //using var qr = Image.Load(qrData);
            //using var client = new HttpClient();
            //using var coverStream = await client.GetStreamAsync(coverUrl);
            //using var cover = Image.Load(coverStream);

            //qr.Mutate(x => x.Resize(380, 380));
            //cover.Mutate(x => x.Resize(640, 400));

            //image.Mutate(x => x.DrawImage(cover, new SixLabors.ImageSharp.Point(100, 100), 1f));
            //image.Mutate(x => x.DrawImage(qr, new SixLabors.ImageSharp.Point(840, 90), 1f));
            //var font = SystemFonts.CreateFont("SimHei", 28, SixLabors.Fonts.FontStyle.Regular);
            //var font2 = SystemFonts.CreateFont("SimHei", 20, SixLabors.Fonts.FontStyle.Regular);
            //image.Mutate(x => x.DrawText("手机扫码观看/分享", font, Color.Black, new PointF(910, 460)));
            //image.Mutate(x => x.DrawText(Video.Title, font2, Color.White, new PointF(104, 476)));

            InMemoryRandomAccessStream buf = new();
            RenderTargetBitmap bitmap = new();
            await bitmap.RenderAsync(this);
            var pixelBuffer = await bitmap.GetPixelsAsync();
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, buf);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Ignore,
                (uint)bitmap.PixelWidth,
                (uint)bitmap.PixelHeight,
                300,
                300,
                pixelBuffer.ToArray());
            await encoder.FlushAsync();

            //image.Save(buf.AsStreamForWrite(), new PngEncoder());
            DataPackage dataPackage = new()
            {
                RequestedOperation = DataPackageOperation.Copy
            };
            dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromStream(buf));
            Clipboard.SetContent(dataPackage);

            ShowToast(new Models.ToastInfo { Text = "已复制到剪贴板" });
        }

        private void ShareLinkClick(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new()
            {
                RequestedOperation = DataPackageOperation.Copy
            };
            dataPackage.SetText("https://www.bilibili.com/video/" + Video.BvId);
            Clipboard.SetContent(dataPackage);

            ShowToast(new Models.ToastInfo { Text = "已复制到剪贴板" });
        }
    }
}
