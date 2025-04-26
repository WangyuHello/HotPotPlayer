using Windows.Media.Casting;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace HotPotPlayer.UI.Controls
{
    /// <summary>
    /// The ImageEx control extends the default Image platform control improving the performance and responsiveness of your Apps.
    /// Source images are downloaded asynchronously showing a load indicator while in progress.
    /// Once downloaded, the source image is stored in the App local cache to preserve resources and load time next time the image needs to be displayed.
    /// </summary>
    public partial class ImageEx2
    {
        /// <summary>
        /// Identifies the <see cref="NineGrid"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty NineGridProperty = DependencyProperty.Register(nameof(NineGrid), typeof(Thickness), typeof(ImageEx2), new PropertyMetadata(default(Thickness)));

        /// <summary>
        /// Gets or sets the nine-grid used by the image.
        /// </summary>
        public Thickness NineGrid
        {
            get { return (Thickness)GetValue(NineGridProperty); }
            set { SetValue(NineGridProperty, value); }
        }

        /// <summary>
        /// Returns the image as a <see cref="CastingSource"/>.
        /// </summary>
        /// <returns>The image as a <see cref="CastingSource"/>.</returns>
        public CastingSource GetAsCastingSource()
        {
            if (IsInitialized && Image is Image image)
            {
                return image.GetAsCastingSource();
            }

            return null;
        }
    }
}
