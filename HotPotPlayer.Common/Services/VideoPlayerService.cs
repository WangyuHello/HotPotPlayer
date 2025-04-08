using CommunityToolkit.Mvvm.ComponentModel;
using HotPotPlayer.Models;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Services
{
    public partial class VideoPlayerService : ServiceBaseWithConfig
    {
        public VideoPlayerService(ConfigBase config, DispatcherQueue uiThread = null, AppBase app = null) : base(config, uiThread, app)
        {
        }

        private bool isVideoPagePresent;
        public bool IsVideoPagePresent
        {
            get => isVideoPagePresent;
            set => Set(ref isVideoPagePresent, value);
        }

        public event Action<VideoHostGeometry> OnVideoGeometryInit;
        public event Action<IntPtr> OnSwapchainInited;
        public IntPtr SwapChain { get; set; }

        public void UpdatePanelScale(float scaleX, float scaleY)
        {

        }

        public void UpdatePanelSize(int width, int height)
        {

        }
    }
}
