using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Services
{
    public enum LocalServiceState
    {
        Idle,
        Loading,
        InitComplete,
        Complete,
        NoLibraryAccess
    }
}
