using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinRT;

namespace HotPotPlayer.Animations
{
    public class OutOfOrderEntranceThemeAnimation : Transition
    {
        protected OutOfOrderEntranceThemeAnimation(DerivedComposed _) : base(_)
        {
        }

        protected internal OutOfOrderEntranceThemeAnimation(IObjectReference objRef) : base(objRef)
        {
        }
    }
}
