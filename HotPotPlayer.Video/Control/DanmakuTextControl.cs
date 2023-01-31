using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Composition;
using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.UI.Composition;
using Microsoft.Graphics.DirectX;
using System.Numerics;
using Windows.UI;
using Microsoft.UI.Xaml.Media;
using HotPotPlayer.Services.BiliBili.Danmaku;
using CommunityToolkit.WinUI.UI.Animations;
using System.Reflection;
using Windows.Foundation;

namespace HotPotPlayer.Video.Control
{
    public class DanmakuTextControl : Microsoft.UI.Xaml.Controls.Control
    {
        private Vector3KeyFrameAnimation _animation;
        private readonly Visual _visual;
        private readonly Compositor _compositor;
        private readonly LinearEasingFunction _linear;

        public DanmakuTextControl(Visual drawText)
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _linear = _compositor.CreateLinearEasingFunction();
            ElementCompositionPreview.SetElementChildVisual(this, drawText);
            _visual = ElementCompositionPreview.GetElementVisual(this);
        }

        public void SetVisual(Visual visual)
        {
            ElementCompositionPreview.SetElementChildVisual(this, visual);
        }

        public void StopOffsetAnimation()
        {
            _visual.StopAnimation("Offset");
        }

        public void StartOffsetAnimation()
        {
            _visual.StartAnimation("Offset", _animation);
        }

        public void ContinueOffsetAnimation()
        {
            var curOffset = _visual.Offset;
            if ((curOffset.X - targetOffset.X) < 2)
            {
                return;
            }
            _animation = _compositor.CreateVector3KeyFrameAnimation();
            _animation.InsertKeyFrame(0f, curOffset, _linear);
            _animation.InsertKeyFrame(1f, targetOffset, _linear);
            _animation.Duration = TimeSpan.FromSeconds((curOffset.X - targetOffset.X) / Speed);
            _animation.DelayTime = TimeSpan.Zero;
            _visual.StartAnimation("Offset", _animation);
        }

        private Vector3 targetOffset;

        public void SetupOffsetAnimation(TimeSpan curTime, float len, double slotStep, double speed, int index, double hostWidth)
        {
            _animation = _compositor.CreateVector3KeyFrameAnimation();
            var exLen = len + 200;
            _animation.InsertKeyFrame(0f, new Vector3(Convert.ToSingle(hostWidth + 1), (float)(slotStep * index), 0f), _linear);
            targetOffset = new Vector3((float)-exLen, (float)(slotStep * index), 0f);
            _animation.InsertKeyFrame(1f, targetOffset, _linear);
            _animation.Duration = TimeSpan.FromSeconds((hostWidth + exLen + 1) / speed);
            _animation.DelayTime = Dm.Time - curTime;
            _animation.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;
            ExitTime = curTime + _animation.Duration;
            Speed = speed;
        }

        public TimeSpan ExitTime { get; set; }

        public DMItem Dm { get; set; }

        public double Speed { get; set; }

    }
}
