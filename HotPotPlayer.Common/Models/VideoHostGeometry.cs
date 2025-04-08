using System.Drawing;

namespace HotPotPlayer.Models
{
    public record VideoHostGeometry
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public float ScaleX { get; set; }
        public float ScaleY { get; set; }
        public Rectangle Bounds { get; set; }
    }
}
