using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.Foundation;

namespace HotPotPlayer.UI.Controls
{
    public static class Extensions
    {
        /// <summary>
        /// Determines if a rectangle intersects with another rectangle.
        /// </summary>
        /// <param name="rect1">The first rectangle to test.</param>
        /// <param name="rect2">The second rectangle to test.</param>
        /// <returns>This method returns <see langword="true"/> if there is any intersection, otherwise <see langword="false"/>.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IntersectsWith(this Rect rect1, Rect rect2)
        {
            if (rect1.IsEmpty || rect2.IsEmpty)
            {
                return false;
            }

            return (rect1.Left <= rect2.Right) &&
                   (rect1.Right >= rect2.Left) &&
                   (rect1.Top <= rect2.Bottom) &&
                   (rect1.Bottom >= rect2.Top);
        }
    }

    /// <summary>
    /// Base Code for ImageEx
    /// </summary>
    [TemplateVisualState(Name = LoadingState, GroupName = CommonGroup)]
    [TemplateVisualState(Name = LoadedState, GroupName = CommonGroup)]
    [TemplateVisualState(Name = UnloadedState, GroupName = CommonGroup)]
    [TemplateVisualState(Name = FailedState, GroupName = CommonGroup)]
    [TemplatePart(Name = PartImage, Type = typeof(object))]
    public abstract partial class ImageEx2Base : Control
    {
        private bool _isInViewport;

        /// <summary>
        /// Image name in template
        /// </summary>
        protected const string PartImage = "Image";

        protected const string PartPlaceholderImage = "PlaceholderImage";

        /// <summary>
        /// VisualStates name in template
        /// </summary>
        protected const string CommonGroup = "CommonStates";

        /// <summary>
        /// Loading state name in template
        /// </summary>
        protected const string LoadingState = "Loading";

        /// <summary>
        /// Loaded state name in template
        /// </summary>
        protected const string LoadedState = "Loaded";

        /// <summary>
        /// Unloaded state name in template
        /// </summary>
        protected const string UnloadedState = "Unloaded";

        /// <summary>
        /// Failed name in template
        /// </summary>
        protected const string FailedState = "Failed";

        /// <summary>
        /// Gets the backing image object
        /// </summary>
        protected object Image { get; private set; }

        protected object PlaceholderImage { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageExBase"/> class.
        /// </summary>
        public ImageEx2Base() { }

        /// <summary>
        /// Attach image opened event handler
        /// </summary>
        /// <param name="handler">Routed Event Handler</param>
        protected void AttachImageOpened(RoutedEventHandler handler)
        {
            if (Image is Image image)
            {
                image.ImageOpened += handler;
            }
            else if (Image is ImageBrush brush)
            {
                brush.ImageOpened += handler;
            }
        }

        /// <summary>
        /// Remove image opened handler
        /// </summary>
        /// <param name="handler">RoutedEventHandler</param>
        protected void RemoveImageOpened(RoutedEventHandler handler)
        {
            if (Image is Image image)
            {
                image.ImageOpened -= handler;
            }
            else if (Image is ImageBrush brush)
            {
                brush.ImageOpened -= handler;
            }
        }

        /// <summary>
        /// Attach image failed event handler
        /// </summary>
        /// <param name="handler">Exception Routed Event Handler</param>
        protected void AttachImageFailed(ExceptionRoutedEventHandler handler)
        {
            if (Image is Image image)
            {
                image.ImageFailed += handler;
            }
            else if (Image is ImageBrush brush)
            {
                brush.ImageFailed += handler;
            }
        }

        /// <summary>
        /// Remove Image Failed handler
        /// </summary>
        /// <param name="handler">Exception Routed Event Handler</param>
        protected void RemoveImageFailed(ExceptionRoutedEventHandler handler)
        {
            if (Image is Image image)
            {
                image.ImageFailed -= handler;
            }
            else if (Image is ImageBrush brush)
            {
                brush.ImageFailed -= handler;
            }
        }

        /// <summary>
        /// Update the visual state of the control when its template is changed.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            RemoveImageOpened(OnImageOpened);
            RemoveImageFailed(OnImageFailed);

            Image = GetTemplateChild(PartImage) as object;
            PlaceholderImage = GetTemplateChild(PartPlaceholderImage) as object;

            IsInitialized = true;

            ImageExInitialized?.Invoke(this, EventArgs.Empty);

            if (Source == null || !EnableLazyLoading || _isInViewport)
            {
                _lazyLoadingSource = null;
                SetSource(Source);
            }
            else
            {
                _lazyLoadingSource = Source;
            }

            AttachImageOpened(OnImageOpened);
            AttachImageFailed(OnImageFailed);

            base.OnApplyTemplate();
        }

        /// <summary>
        /// Underlying <see cref="Image.ImageOpened"/> event handler.
        /// </summary>
        /// <param name="sender">Image</param>
        /// <param name="e">Event Arguments</param>
        protected virtual void OnImageOpened(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, LoadedState, true);
            ImageExOpened?.Invoke(this, new ImageEx2OpenedEventArgs());
        }

        /// <summary>
        /// Underlying <see cref="Image.ImageFailed"/> event handler.
        /// </summary>
        /// <param name="sender">Image</param>
        /// <param name="e">Event Arguments</param>
        protected virtual void OnImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, FailedState, true);
            ImageExFailed?.Invoke(this, new ImageEx2FailedEventArgs(new Exception(e.ErrorMessage)));
        }

        private void ImageExBase_LayoutUpdated(object sender, object e)
        {
            InvalidateLazyLoading();
        }

        private void InvalidateLazyLoading()
        {
            if (!IsLoaded)
            {
                _isInViewport = false;
                return;
            }

            // Find the first ascendant ScrollViewer, if not found, use the root element.
            FrameworkElement hostElement = null;
            var ascendants = this.FindAscendants().OfType<FrameworkElement>();
            foreach (var ascendant in ascendants)
            {
                hostElement = ascendant;
                if (hostElement is ScrollViewer)
                {
                    break;
                }
            }

            if (hostElement == null)
            {
                _isInViewport = false;
                return;
            }

            var controlRect = TransformToVisual(hostElement)
                .TransformBounds(new Rect(0, 0, ActualWidth, ActualHeight));
            var lazyLoadingThreshold = LazyLoadingThreshold;
            var hostRect = new Rect(
                0 - lazyLoadingThreshold,
                0 - lazyLoadingThreshold,
                hostElement.ActualWidth + (2 * lazyLoadingThreshold),
                hostElement.ActualHeight + (2 * lazyLoadingThreshold));

            if (controlRect.IntersectsWith(hostRect))
            {
                _isInViewport = true;

                if (_lazyLoadingSource != null)
                {
                    var source = _lazyLoadingSource;
                    _lazyLoadingSource = null;
                    SetSource(source);
                }
            }
            else
            {
                _isInViewport = false;
            }
        }
    }
}
