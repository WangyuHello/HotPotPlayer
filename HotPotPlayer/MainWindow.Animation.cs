using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer
{
    public sealed partial class MainWindow : Window
    {
        private void StartPopAnimation(Button targetButton)
        {
            var stackOffset = ButtonStack.ActualOffset;
            var y = targetButton.ActualOffset;
            var targetOffset = y.Y - 15;
            if (targetButton.Name == "Music" || targetButton.Name == "Video" || targetButton.Name == "Bilibili" || targetButton.Name == "CloudMusic")
            {
                targetOffset = stackOffset.Y + y.Y - 15;
            }
            CreateOrUpdateSpringAnimation(targetOffset);
            ButtonPop.StartAnimation(popMoveAnimation);
        }

        private void StartPopAnimation(string targetButton)
        {
            var button = targetButton switch
            {
                "Music" => Music,
                "Video" => Video,
                "Bilibili" => Bilibili,
                "CloudMusic" => CloudMusic,
                "Setting" => Setting,
                _ => throw new NotImplementedException(),
            };
            StartPopAnimation(button);
        }

        SpringVector3NaturalMotionAnimation popMoveAnimation;

        private void CreateOrUpdateSpringAnimation(float finalValue)
        {
            if (popMoveAnimation == null)
            {
                popMoveAnimation = Compositor.CreateSpringVector3Animation();
                popMoveAnimation.Target = "Translation";
            }
            popMoveAnimation.FinalValue = new Vector3(0, finalValue, 0);
        }
    }
}
