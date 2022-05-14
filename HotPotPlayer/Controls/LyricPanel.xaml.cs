using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
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
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Controls
{
    public sealed partial class LyricPanel : UserControl
    {
        public LyricPanel()
        {
            this.InitializeComponent();
        }

        public string RawLyric
        {
            get { return (string)GetValue(RawLyricProperty); }
            set { SetValue(RawLyricProperty, value); }
        }

        public static readonly DependencyProperty RawLyricProperty =
            DependencyProperty.Register("RawLyric", typeof(string), typeof(LyricPanel), new PropertyMetadata(string.Empty));



        private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {

        }

        private void Canvas_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {

        }

        private void Canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {

        }

        private void Canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {

        }
    }
}
