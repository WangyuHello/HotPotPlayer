using HotPotPlayer.Helpers;
using HotPotPlayer.Services;
using Xunit;

namespace HotPotPlayer.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var l = RemovableDiskHelper.RemovableDisks;
        }

        [Fact]
        public void T()
        {
            var s = new LocalVideoService();
            s.StartLoadLocalVideo();
        }
    }
}