using Danmaku.Core;
using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Richasy.BiliKernel.Models;
using Richasy.BiliKernel.Models.Danmaku;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Video.UI.Controls
{
    public sealed partial class VideoDanmakuPanel : UserControl
    {
        private List<DanmakuItem> _cachedDanmakus = [];
        private int _lastProgress;
        private DanmakuFrostMaster _danmakuController;

        public VideoDanmakuPanel()
        {
            InitializeComponent();
        }

        public bool ShowDanmaku
        {
            get { return (bool)GetValue(ShowDanmakuProperty); }
            set { SetValue(ShowDanmakuProperty, value); }
        }

        public static readonly DependencyProperty ShowDanmakuProperty =
            DependencyProperty.Register("ShowDanmaku", typeof(bool), typeof(VideoDanmakuPanel), new PropertyMetadata(false));

        public double DanmakuOpacity
        {
            get { return (double)GetValue(DanmakuOpacityProperty); }
            set { SetValue(DanmakuOpacityProperty, value); }
        }

        public static readonly DependencyProperty DanmakuOpacityProperty =
            DependencyProperty.Register("DanmakuOpacity", typeof(double), typeof(VideoDanmakuPanel), new PropertyMetadata(1.0));

        public bool IsDanmakuBold
        {
            get { return (bool)GetValue(IsDanmakuBoldProperty); }
            set { SetValue(IsDanmakuBoldProperty, value); }
        }

        public static readonly DependencyProperty IsDanmakuBoldProperty =
            DependencyProperty.Register("IsDanmakuBold", typeof(bool), typeof(VideoDanmakuPanel), new PropertyMetadata(false));

        public double DanmakuSpeed
        {
            get { return (double)GetValue(DanmakuSpeedProperty); }
            set { SetValue(DanmakuSpeedProperty, value); }
        }

        public static readonly DependencyProperty DanmakuSpeedProperty =
            DependencyProperty.Register("DanmakuSpeed", typeof(double), typeof(VideoDanmakuPanel), new PropertyMetadata(0));


        public double ExtraSpeed
        {
            get { return (double)GetValue(ExtraSpeedProperty); }
            set { SetValue(ExtraSpeedProperty, value); }
        }

        public static readonly DependencyProperty ExtraSpeedProperty =
            DependencyProperty.Register("ExtraSpeed", typeof(double), typeof(VideoDanmakuPanel), new PropertyMetadata(0));

        public double DanmakuArea
        {
            get { return (double)GetValue(DanmakuAreaProperty); }
            set { SetValue(DanmakuAreaProperty, value); }
        }

        public static readonly DependencyProperty DanmakuAreaProperty =
            DependencyProperty.Register("DanmakuArea", typeof(double), typeof(VideoDanmakuPanel), new PropertyMetadata(0));

        public Windows.UI.Color Color
        {
            get { return (Windows.UI.Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Windows.UI.Color), typeof(VideoDanmakuPanel), new PropertyMetadata(Windows.UI.Color.FromArgb(0,0,0,0)));

        public DanmakuLocation Location
        {
            get { return (DanmakuLocation)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        public static readonly DependencyProperty LocationProperty =
            DependencyProperty.Register("Location", typeof(DanmakuLocation), typeof(VideoDanmakuPanel), new PropertyMetadata(DanmakuLocation.Scroll));

        public bool IsStandardSize
        {
            get { return (bool)GetValue(IsStandardSizeProperty); }
            set { SetValue(IsStandardSizeProperty, value); }
        }

        public static readonly DependencyProperty IsStandardSizeProperty =
            DependencyProperty.Register("IsStandardSize", typeof(bool), typeof(VideoDanmakuPanel), new PropertyMetadata(true));

        private static DanmakuFontSize GetFontSize(double fontSize)
        {
            return fontSize switch
            {
                0.5 => DanmakuFontSize.Smallest,
                1 => DanmakuFontSize.Smaller,
                2.0 => DanmakuFontSize.Larger,
                2.5 => DanmakuFontSize.Largest,
                _ => DanmakuFontSize.Normal,
            };
        }

        private async void OnDanmakuListAddedAsync(object sender, IReadOnlyList<DanmakuInformation> e)
        {
            var items = BilibiliDanmakuParser.GetDanmakuList(e, true);
            var isFirstLoad = _cachedDanmakus.Count == 0;
            _cachedDanmakus = _cachedDanmakus.Concat(items).Distinct().ToList();
            if (isFirstLoad)
            {
                await Task.Delay(250);
                Redraw(true);
            }
            else
            {
                _danmakuController?.AddDanmakuList(items);
            }
        }

        private void OnRequestClearDanmaku(object sender, EventArgs e)
        {
            if (ShowDanmaku)
            {
                _cachedDanmakus.Clear();
                _lastProgress = 0;
            }

            _danmakuController?.Clear();
        }

        private void OnRequestAddSingleDanmaku(object sender, string e)
        {
            var model = new DanmakuItem
            {
                StartMs = Convert.ToUInt32(_lastProgress * 1000),
                Mode = (DanmakuMode)((int)Location),
                TextColor = Color,
                BaseFontSize = IsStandardSize ? 20 : 24,
                Text = e,
                HasOutline = true,
            };

            _danmakuController?.AddRealtimeDanmaku(model, true);
        }

        private void OnRequestResetStyle(object sender, EventArgs e)
    => ResetDanmakuStyle();

        private void OnExtraSpeedChanged(object sender, EventArgs e)
            => ResetSpeed();

        private void OnProgressChanged(object sender, int e)
        {
            _lastProgress = e;
            _danmakuController?.UpdateTime(Convert.ToUInt32(e * 1000));
        }

        private void OnResumeDanmaku(object sender, EventArgs e)
            => _danmakuController?.Resume();

        private void OnPauseDanmaku(object sender, EventArgs e)
            => _danmakuController?.Pause();

        private void OnRedrawDanmaku(object sender, EventArgs e)
            => Redraw();

        private void ResetDanmakuStyle()
        {
            if (_danmakuController is null)
            {
                return;
            }

            if (!ShowDanmaku)
            {
                _danmakuController.Pause();
            }

            _danmakuController.SetRollingDensity(-1);
            _danmakuController.SetOpacity(DanmakuOpacity);
            _danmakuController.SetBorderColor(Colors.Gray);
            _danmakuController.SetRollingAreaRatio(Convert.ToInt32(DanmakuArea * 10));
            _danmakuController.SetDanmakuFontSizeOffset(GetFontSize(FontSize));
            //_danmakuController.SetFontFamilyName(FontFamily.Source);
            _danmakuController.SetFontFamilyName("Segoe UI");
            _danmakuController.SetIsTextBold(IsDanmakuBold);
            ResetSpeed();
        }

        private void ResetSpeed()
        {
            var finalSpeed = DanmakuSpeed * 5 * ExtraSpeed;
            _danmakuController?.SetRollingSpeed(finalSpeed);
        }

        private void Redraw(bool force = false)
        {
            if (!force && (_danmakuController is null))
            {
                return;
            }

            DispatcherQueue?.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
            {
                _danmakuController?.Close();
                _danmakuController = new DanmakuFrostMaster(RootGrid);

                if (_cachedDanmakus.Count != 0)
                {
                    _danmakuController.AddDanmakuList(_cachedDanmakus);
                }

                _danmakuController.UpdateTime(Convert.ToUInt32(_lastProgress * 1000));
                ResetDanmakuStyle();
            });
        }
    }
}
