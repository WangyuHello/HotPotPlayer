using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Controls.BilibiliSub
{
    class DanmakuRenderer
    {
        public DanmakuRenderer() 
        {
            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasSwapChain swapChain = new CanvasSwapChain(device, 800, 600, 96);
        }
    }
}
