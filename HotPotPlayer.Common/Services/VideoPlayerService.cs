using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
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

        [ObservableProperty]
        private bool isVideoPagePresent;
    }
}
