using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.UI.Controls
{
    /// <summary>
    /// A delegate for <see cref="ImageEx2"/> failed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event arguments.</param>
    public delegate void ImageEx2FailedEventHandler(object sender, ImageEx2FailedEventArgs e);

    /// <summary>
    /// Provides data for the <see cref="ImageEx2"/> ImageFailed event.
    /// </summary>
    public class ImageEx2FailedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageExFailedEventArgs"/> class.
        /// </summary>
        /// <param name="errorException">exception that caused the error condition</param>
        public ImageEx2FailedEventArgs(Exception errorException)
        {
            ErrorException = errorException;
            ErrorMessage = ErrorException?.Message;
        }

        /// <summary>
        /// Gets the exception that caused the error condition.
        /// </summary>
        public Exception ErrorException { get; private set; }

        /// <summary>
        /// Gets the reason for the error condition.
        /// </summary>
        public string ErrorMessage { get; private set; }
    }
}
