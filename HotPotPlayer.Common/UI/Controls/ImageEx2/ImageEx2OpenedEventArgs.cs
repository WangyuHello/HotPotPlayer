using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.UI.Controls
{
    /// <summary>
    /// A delegate for <see cref="ImageEx"/> opened.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event arguments.</param>
    public delegate void ImageEx2OpenedEventHandler(object sender, ImageEx2OpenedEventArgs e);

    /// <summary>
    /// Provides data for the <see cref="ImageEx"/> ImageOpened event.
    /// </summary>
    public class ImageEx2OpenedEventArgs : EventArgs { }
}
