using HotPotPlayer.Helpers;
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
    }
}