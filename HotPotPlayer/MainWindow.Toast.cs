using HotPotPlayer.Models;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer
{
    public partial class MainWindow
    {
        private Visibility _toastVisible = Visibility.Collapsed;

        public Visibility ToastVisible
        {
            get => _toastVisible;
            set => Set(ref _toastVisible, value);
        }

        DispatcherTimer _timer;
        bool _toastOpened = false;

        DispatcherTimer InitToastTimer()
        {
            var t = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            t.Tick += ToastTimerTick;
            return t;
        }

        private void ToastTimerTick(object sender, object e)
        {
            DismissToast();
        }

        public void ShowToast(ToastInfo toast)
        {
            if (_toastOpened)
            {
                return;
            }
            _toastOpened = true;
            Toast.ToastInfo = toast;
            ToastVisible = Visibility.Visible;
            _timer ??= InitToastTimer();
            _timer.Start();
        }

        public void DismissToast()
        {
            _timer.Stop();
            _toastOpened = false;
            ToastVisible = Visibility.Collapsed;
        }
    }
}
