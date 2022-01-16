using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Controls
{
    public sealed partial class VolumePresenter : UserControl
    {
        public VolumePresenter()
        {
            this.InitializeComponent();
        }

        public float? Volume
        {
            get { return (float)GetValue(VolumeProperty); }
            set { SetValue(VolumeProperty, value); }
        }

        public static readonly DependencyProperty VolumeProperty =
            DependencyProperty.Register("Volume", typeof(float?), typeof(VolumePresenter), new PropertyMetadata(0));

           
        string GetVolumeText(float? v)
        {
            var v2 = (int)(v*100);
            return v2 + "%";
        }

        Vector3 GetVolumeTranslation(float? v)
        {
            if (v == null)
            {
                return Vector3.Zero;
            }
            var x = -72 * (1 - (float)v);
            return new Vector3(x, 0, 0);
        }
    }
}
