﻿using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Storage;

namespace HotPotPlayer.Helpers
{
    public class ImageCacheEx: ImageCacheBaseEx<BitmapImage>
    {
        private const string DateAccessedProperty = "System.DateAccessed";

        /// <summary>
        /// Private singleton field.
        /// </summary>
        [ThreadStatic]
        private static ImageCacheEx _instance;

        private List<string> _extendedPropertyNames = new List<string>();

        /// <summary>
        /// Gets public singleton property.
        /// </summary>
        public static ImageCacheEx Instance => _instance ?? (_instance = new ImageCacheEx());

        /// <summary>
        /// Gets or sets which DispatcherQueue is used to dispatch UI updates.
        /// </summary>
        public DispatcherQueue DispatcherQueue { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ImageCache"/> class.
        /// </summary>
        /// <param name="dispatcherQueue">The DispatcherQueue that should be used to dispatch UI updates, or null if this is being called from the UI thread.</param>
        public ImageCacheEx(DispatcherQueue dispatcherQueue = null): base()
        {
            DispatcherQueue = dispatcherQueue ?? DispatcherQueue.GetForCurrentThread();
            _extendedPropertyNames.Add(DateAccessedProperty);
            MaxMemoryCacheCount = 200;
            CacheDuration = TimeSpan.FromDays(7);
        }

        /// <summary>
        /// Cache specific hooks to process items from HTTP response
        /// </summary>
        /// <param name="stream">input stream</param>
        /// <param name="initializerKeyValues">key value pairs used when initializing instance of generic type</param>
        /// <returns>awaitable task</returns>
        protected override async Task<BitmapImage> InitializeTypeAsync(Stream stream, List<KeyValuePair<string, object>> initializerKeyValues = null)
        {
            if (stream.Length == 0)
            {
                throw new FileNotFoundException();
            }

            return await DispatcherQueue.EnqueueAsync(async () =>
            {
                BitmapImage image = new BitmapImage();

                if (initializerKeyValues != null && initializerKeyValues.Count > 0)
                {
                    foreach (var kvp in initializerKeyValues)
                    {
                        if (string.IsNullOrWhiteSpace(kvp.Key))
                        {
                            continue;
                        }

                        var propInfo = image.GetType().GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.Instance);

                        if (propInfo != null && propInfo.CanWrite)
                        {
                            propInfo.SetValue(image, kvp.Value);
                        }
                    }
                }

                await image.SetSourceAsync(stream.AsRandomAccessStream()).AsTask().ConfigureAwait(false);

                return image;
            });
        }

        /// <summary>
        /// Cache specific hooks to process items from HTTP response
        /// </summary>
        /// <param name="baseFile">storage file</param>
        /// <param name="initializerKeyValues">key value pairs used when initializing instance of generic type</param>
        /// <returns>awaitable task</returns>
        protected override async Task<BitmapImage> InitializeTypeAsync(StorageFile baseFile, List<KeyValuePair<string, object>> initializerKeyValues = null)
        {
            using (var stream = await baseFile.OpenStreamForReadAsync().ConfigureAwait(false))
            {
                return await InitializeTypeAsync(stream, initializerKeyValues).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Override-able method that checks whether file is valid or not.
        /// </summary>
        /// <param name="file">storage file</param>
        /// <param name="duration">cache duration</param>
        /// <param name="treatNullFileAsOutOfDate">option to mark uninitialized file as expired</param>
        /// <returns>bool indicate whether file has expired or not</returns>
        protected override async Task<bool> IsFileOutOfDateAsync(StorageFile file, TimeSpan duration, bool treatNullFileAsOutOfDate = true)
        {
            if (file == null)
            {
                return treatNullFileAsOutOfDate;
            }

            // Get extended properties.
            IDictionary<string, object> extraProperties =
                await file.Properties.RetrievePropertiesAsync(_extendedPropertyNames).AsTask().ConfigureAwait(false);

            // Get date-accessed property.
            var propValue = extraProperties[DateAccessedProperty];

            if (propValue != null)
            {
                var lastAccess = propValue as DateTimeOffset?;

                if (lastAccess.HasValue)
                {
                    return DateTime.Now.Subtract(lastAccess.Value.DateTime) > duration;
                }
            }

            var properties = await file.GetBasicPropertiesAsync().AsTask().ConfigureAwait(false);

            return properties.Size == 0 || DateTime.Now.Subtract(properties.DateModified.DateTime) > duration;
        }


        string[] _videoExt = new[] { ".mkv", ".mp4" };
        string[] _audioExt = new[] { ".flac", ".wav", ".m4a", ".mp3", ".opus", ".ogg" };
        protected override string[] VideoExt => _videoExt;

        protected override string[] AudioExt => _audioExt;
    }
}
