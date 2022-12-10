using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Services
{
    public class BiliBiliService : ServiceBaseWithConfig
    {
        public BiliBiliService(ConfigBase config, DispatcherQueue uiThread = null, AppBase app = null) : base(config, uiThread, app)
        {
        }


    }
}
