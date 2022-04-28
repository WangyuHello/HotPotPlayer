using HotPotPlayer.Services.FFmpeg;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas;
using Windows.Graphics.DirectX;
using Microsoft.Graphics.Canvas.Effects;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Video
{
    public sealed partial class SoftwareVideoHost : UserControl
    {
        public SoftwareVideoHost()
        {
            this.InitializeComponent();
            _ui = DispatcherQueue.GetForCurrentThread();
        }

        public FileInfo Source
        {
            get { return (FileInfo)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(FileInfo), typeof(SoftwareVideoHost), new PropertyMetadata(default(FileInfo), SourceChanged));


        private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var h = (SoftwareVideoHost)d;
            h.StartRender((FileInfo)e.NewValue);
        }

        bool isRendering;
        CancellationTokenSource _tokenSource;
        CancellationToken _token;
        MediaHelper _player;
        DispatcherQueue _ui;
        DateTime _previousTime;

        void StartRender(FileInfo file)
        {
            if (isRendering)
            {
                _tokenSource.Cancel();
                _player.Dispose();
                isRendering = false;
            }
            try
            {
                _tokenSource = new CancellationTokenSource();
                _token = _tokenSource.Token;
                Task.Run(async () =>
                {
                    isRendering = true;
                    _player = new MediaHelper(file);

                    try
                    {
                        while (!_token.IsCancellationRequested && _player.TryDecodeNextFrame())
                        {
                            var now = DateTime.Now;
                            var elapse = now - _previousTime;
                            var delta = 16 - elapse.TotalMilliseconds;
                            _previousTime = now;
                            _ui.TryEnqueue(() =>
                            {
                                Host.Invalidate();
                            });

                            if (delta > 0)
                            {
                                await Task.Delay((int)delta);
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }

                }, _token);
            }
            catch (Exception)
            {

            }
        }

        CanvasBitmap _bitmap;
        ScaleEffect effect;

        private void Host_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (_player != null && _player.Buffer != null)
            {
                if (_bitmap == null)
                {
                    _bitmap = CanvasBitmap.CreateFromBytes(sender, _player.Buffer, _player.Width, _player.Height, DirectXPixelFormat.B8G8R8A8UIntNormalized);
                }
                else
                {
                    _bitmap.SetPixelBytes(_player.Buffer, 0, 0, _player.Width, _player.Height);
                }
                var hostWidth = Host.ActualWidth;
                var hostHeight = Host.ActualHeight;

                var wide = _player.Width > _player.Height;
                var ratio = wide ? hostWidth / _player.Width : hostHeight / _player.Height;
                effect = new ScaleEffect()
                {
                    Source = _bitmap,
                };
                effect.Scale = new System.Numerics.Vector2((float)ratio, (float)ratio);

                var x = wide ? 0 : (hostWidth - _player.Width * ratio) / 2;
                var y = wide ? (hostHeight - _player.Height * ratio) / 2 : 0;
                args.DrawingSession.DrawImage(effect, (float)x , (float)y);
                effect.Dispose();
            }
        }
    }

}
