// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using HotPotPlayer.Services.BiliBili.Danmaku;
using Microsoft.Graphics.Canvas;
using Microsoft.UI;
using System.Numerics;
using Windows.Graphics.Display;
using Microsoft.Graphics.Canvas.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Controls.BilibiliSub
{
    public sealed partial class DanmakuRenderer : UserControlBase
    {
        public DanmakuRenderer()
        {
            this.InitializeComponent();

            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasSwapChain swapChain = new CanvasSwapChain(device, 800, 600, 96);
            Host.SwapChain = swapChain;
        }


        public DMData DmData
        {
            get { return (DMData)GetValue(DmDataProperty); }
            set { SetValue(DmDataProperty, value); }
        }

        public static readonly DependencyProperty DmDataProperty =
            DependencyProperty.Register("DmData", typeof(DMData), typeof(DanmakuRenderer), new PropertyMetadata(default, DMChanged));

        private static void DMChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DanmakuRenderer)d).Draw(e.NewValue as DMData);
        }

        private void Draw(DMData n)
        {
            using var ds = Host.SwapChain.CreateDrawingSession(Colors.Wheat);
            foreach (var item in n.Dms)
            {
                ds.DrawText(item.Content, new Vector2(0), Colors.Black);
            }

            Host.SwapChain.Present();
        }

        private void Host_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Host.SwapChain.ResizeBuffers(e.NewSize);
        }
    }
}
