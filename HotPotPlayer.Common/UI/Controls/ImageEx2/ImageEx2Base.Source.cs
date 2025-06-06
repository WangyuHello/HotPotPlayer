﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using HotPotPlayer.Helpers;
using HotPotPlayer.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace HotPotPlayer.UI.Controls
{
    /// <summary>
    /// Base code for ImageEx
    /// </summary>
    public partial class ImageEx2Base
    {
        /// <summary>
        /// Identifies the <see cref="Source"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(object), typeof(ImageEx2Base), new PropertyMetadata(null, SourceChanged));

        //// Used to track if we get a new request, so we can cancel any potential custom cache loading.
        private CancellationTokenSource _tokenSource;

        private object _lazyLoadingSource;

        /// <summary>
        /// Gets or sets the source used by the image
        /// </summary>
        public object Source
        {
            get { return GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ImageEx2Base control)
            {
                return;
            }

            if (e.OldValue == null || e.NewValue == null || !e.OldValue.Equals(e.NewValue))
            {
                if (e.NewValue == null || !control.EnableLazyLoading || control._isInViewport)
                {
                    control._lazyLoadingSource = null;
                    control.SetSource(e.NewValue);
                }
                else
                {
                    control._lazyLoadingSource = e.NewValue;
                }
            }
        }

        private static bool IsHttpUri(Uri uri)
        {
            return uri.IsAbsoluteUri && (uri.Scheme == "http" || uri.Scheme == "https");
        }

        /// <summary>
        /// Method to call to assign an <see cref="ImageSource"/> value to the underlying <see cref="Image"/> powering <see cref="ImageExBase"/>.
        /// </summary>
        /// <param name="source"><see cref="ImageSource"/> to assign to the image.</param>
        private void AttachSource(ImageSource source)
        {
            // Setting the source at this point should call ImageExOpened/VisualStateManager.GoToState
            // as we register to both the ImageOpened/ImageFailed events of the underlying control.
            // We only need to call those methods if we fail in other cases before we get here.
            if (Image is Image image)
            {
                image.Source = source;
            }
            else if (Image is ImageBrush brush)
            {
                brush.ImageSource = source;
            }

            if (source == null)
            {
                VisualStateManager.GoToState(this, UnloadedState, true);
            }
            else if ((source is BitmapSource { PixelHeight: > 0, PixelWidth: > 0 }))
            {
                VisualStateManager.GoToState(this, LoadedState, true);
                ImageExOpened?.Invoke(this, new ImageEx2OpenedEventArgs());
            }
        }

        private async void SetSource(object source)
        {
            if (!IsInitialized)
            {
                return;
            }

            _tokenSource?.Cancel();

            _tokenSource = new CancellationTokenSource();

            AttachSource(null);

            if (source == null)
            {
                return;
            }

            VisualStateManager.GoToState(this, LoadingState, true);

            if (source is ImageSource imageSource)
            {
                AttachSource(imageSource);
                return;
            }

            var uri = source as Uri;

            if (source is ImageSourceWithBlur imageWithBlur)
            {
                var blurImage = await imageWithBlur.GetBlurImageAsync(DecodePixelWidth, DecodePixelHeight, _tokenSource.Token);
                var placeholdImage = PlaceholdImage as Image;
                placeholdImage.Source = blurImage;
                uri = imageWithBlur.GetImageUri(DecodePixelWidth*2, DecodePixelHeight*2);
            }

            if (uri == null)
            {
                var url = source as string ?? source.ToString();
                if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
                {
                    VisualStateManager.GoToState(this, FailedState, true);
                    ImageExFailed?.Invoke(this, new ImageEx2FailedEventArgs(new UriFormatException("Invalid uri specified.")));
                    return;
                }
            }

            if (!IsHttpUri(uri) && !uri.IsAbsoluteUri)
            {
                uri = new Uri("ms-appx:///" + uri.OriginalString.TrimStart('/'));
            }

            try
            {
                await LoadImageAsync(uri, _tokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // nothing to do as cancellation has been requested.
            }
            catch (Exception e)
            {
                VisualStateManager.GoToState(this, FailedState, true);
                ImageExFailed?.Invoke(this, new ImageEx2FailedEventArgs(e));
            }
        }

        private async Task LoadImageAsync(Uri imageUri, CancellationToken token)
        {
            //await Task.Delay(2000, token); Test
            if (imageUri != null)
            {
                if (IsCacheEnabled)
                {
                    var img = await ProvideCachedResourceAsync(imageUri, token);
                    SourceGotFromCache?.Invoke(img);
                    if (!_tokenSource.IsCancellationRequested)
                    {
                        // Only attach our image if we still have a valid request.
                        AttachSource(img);
                    }
                }
                else if (string.Equals(imageUri.Scheme, "data", StringComparison.OrdinalIgnoreCase))
                {
                    var source = imageUri.OriginalString;
                    const string base64Head = "base64,";
                    var index = source.IndexOf(base64Head);
                    if (index >= 0)
                    {
                        var bytes = Convert.FromBase64String(source.Substring(index + base64Head.Length));
                        var bitmap = new BitmapImage();
                        await bitmap.SetSourceAsync(new MemoryStream(bytes).AsRandomAccessStream());

                        if (!_tokenSource.IsCancellationRequested)
                        {
                            AttachSource(bitmap);
                        }
                    }
                }
                else
                {
                    AttachSource(new BitmapImage(imageUri)
                    {
                        CreateOptions = BitmapCreateOptions.IgnoreImageCache
                    });
                }
            }
        }

        /// <summary>
        /// This method is provided in case a developer would like their own custom caching strategy for <see cref="ImageExBase"/>.
        /// By default it uses the built-in UWP cache provided by <see cref="BitmapImage"/> and
        /// the <see cref="Image"/> control itself. This method should return an <see cref="ImageSource"/>
        /// value of the image specified by the provided uri parameter.
        /// A <see cref="CancellationToken"/> is provided in case the current request is invalidated
        /// (e.g. the container is recycled before the original image is loaded).
        /// The Toolkit also has an image cache helper which can be used as well:
        /// <see cref="CacheBase{T}.GetFromCacheAsync(Uri, bool, CancellationToken, List{KeyValuePair{string, object}})"/> in <see cref="ImageCache"/>.
        /// </summary>
        /// <example>
        /// <code>
        ///     var propValues = new List&lt;KeyValuePair&lt;string, object>>();
        ///
        ///     if (DecodePixelHeight > 0)
        ///     {
        ///         propValues.Add(new KeyValuePair&lt;string, object>(nameof(DecodePixelHeight), DecodePixelHeight));
        ///     }
        ///     if (DecodePixelWidth > 0)
        ///     {
        ///         propValues.Add(new KeyValuePair&lt;string, object>(nameof(DecodePixelWidth), DecodePixelWidth));
        ///     }
        ///     if (propValues.Count > 0)
        ///     {
        ///         propValues.Add(new KeyValuePair&lt;string, object>(nameof(DecodePixelType), DecodePixelType));
        ///     }
        ///
        ///     // A token is provided here as well to cancel the request to the cache,
        ///     // if a new image is requested.
        ///     return await ImageCache.Instance.GetFromCacheAsync(imageUri, true, token, propValues);
        /// </code>
        /// </example>
        /// <param name="imageUri"><see cref="Uri"/> of the image to load from the cache.</param>
        /// <param name="token">A <see cref="CancellationToken"/> which is used to signal when the current request is outdated.</param>
        /// <returns><see cref="Task"/></returns>
        protected virtual async Task<ImageSource> ProvideCachedResourceAsync(Uri imageUri, CancellationToken token)
        {
            var propValues = new List<KeyValuePair<string, object>>();
            
            if (Width > 0)
            {
                var decW = (int)(XamlRoot.RasterizationScale * Width);
                propValues.Add(new KeyValuePair<string, object>(nameof(DecodePixelWidth), decW));
            }
            var image = await ImageCacheEx.Instance.GetFromCacheAsync(imageUri, true, token, propValues);
            return image;
        }

        public event Action<ImageSource> SourceGotFromCache;
    }
}
