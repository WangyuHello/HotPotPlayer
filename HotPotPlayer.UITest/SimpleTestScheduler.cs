using HotPotPlayer.Test.Tests;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Test
{
    internal static class SimpleTestScheduler
    {
        internal static void Start()
        {
            var t = new VideoTest((AppBase)Application.Current);
            t.TestMethod1();
        }
    }
}
