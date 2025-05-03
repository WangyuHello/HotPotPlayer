using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;

namespace HotPotPlayer.Extensions
{
    public static class FrameExtensions
    {
        /// <summary>
        /// Navigates to a page and returns the instance of the page if it succeeded,
        /// otherwise returns null.
        /// </summary>
        /// <typeparam name="TPage"></typeparam>
        /// <param name="frame"></param>
        /// <param name="transitionInfo">The navigation transition.
        /// Example: <see cref="DrillInNavigationTransitionInfo"/> or
        /// <see cref="SlideNavigationTransitionInfo"/></param>
        /// <returns></returns>
        public static TPage Navigate<TPage>(
            this Frame frame,
            object parameter = null,
            NavigationTransitionInfo transitionInfo = null)
            where TPage : Page
        {
            TPage view = null;
            void OnNavigated(object s, NavigationEventArgs args)
            {
                frame.Navigated -= OnNavigated;
                view = args.Content as TPage;
            }

            frame.Navigated += OnNavigated;

            frame.Navigate(typeof(TPage), parameter, transitionInfo);
            return view;
        }
    }
}
