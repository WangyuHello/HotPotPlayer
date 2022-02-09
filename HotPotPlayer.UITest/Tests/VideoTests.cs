using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HotPotPlayer.Test.Tests
{
    [TestClass]
    public class VideoTest
    {
        AppBase App { get; init; }

        public VideoTest(AppBase app)
        {
            App = app;
        }

        [TestMethod]
        public void TestMethod1()
        {
            App.LocalVideoService.StartLoadLocalVideo();
        }
    }
}
