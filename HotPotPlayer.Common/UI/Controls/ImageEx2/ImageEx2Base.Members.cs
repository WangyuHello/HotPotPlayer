﻿using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace HotPotPlayer.UI.Controls
{
    /// <summary>
    /// Base Code for ImageEx
    /// </summary>
    public partial class ImageEx2Base
    {
        /// <summary>
        /// Identifies the <see cref="Stretch"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(nameof(Stretch), typeof(Stretch), typeof(ImageEx2Base), new PropertyMetadata(Stretch.Uniform));

        /// <summary>
        /// Identifies the <see cref="DecodePixelHeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DecodePixelHeightProperty = DependencyProperty.Register(nameof(DecodePixelHeight), typeof(int), typeof(ImageEx2Base), new PropertyMetadata(0));

        /// <summary>
        /// Identifies the <see cref="DecodePixelType"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DecodePixelTypeProperty = DependencyProperty.Register(nameof(DecodePixelType), typeof(int), typeof(ImageEx2Base), new PropertyMetadata(DecodePixelType.Physical));

        /// <summary>
        /// Identifies the <see cref="DecodePixelWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DecodePixelWidthProperty = DependencyProperty.Register(nameof(DecodePixelWidth), typeof(int), typeof(ImageEx2Base), new PropertyMetadata(0));

        /// <summary>
        /// Identifies the <see cref="IsCacheEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsCacheEnabledProperty = DependencyProperty.Register(nameof(IsCacheEnabled), typeof(bool), typeof(ImageEx2Base), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="EnableLazyLoading"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EnableLazyLoadingProperty = DependencyProperty.Register(nameof(EnableLazyLoading), typeof(bool), typeof(ImageEx2Base), new PropertyMetadata(false, EnableLazyLoadingChanged));

        /// <summary>
        /// Identifies the <see cref="LazyLoadingThreshold"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LazyLoadingThresholdProperty = DependencyProperty.Register(nameof(LazyLoadingThreshold), typeof(double), typeof(ImageEx2Base), new PropertyMetadata(default(double), LazyLoadingThresholdChanged));

        /// <summary>
        /// Event raised if the image failed loading.
        /// </summary>
        public event ImageEx2FailedEventHandler ImageExFailed;

        /// <summary>
        /// Event raised when the image is successfully loaded and opened.
        /// </summary>
        public event ImageEx2OpenedEventHandler ImageExOpened;

        /// <summary>
        /// Event raised when the control is initialized.
        /// </summary>
        public event EventHandler ImageExInitialized;

        /// <summary>
        /// Gets a value indicating whether control has been initialized.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Gets or sets DecodePixelHeight for underlying bitmap
        /// </summary>
        public int DecodePixelHeight
        {
            get { return (int)GetValue(DecodePixelHeightProperty); }
            set { SetValue(DecodePixelHeightProperty, value); }
        }

        /// <summary>
        /// Gets or sets DecodePixelType for underlying bitmap
        /// </summary>
        public DecodePixelType DecodePixelType
        {
            get { return (DecodePixelType)GetValue(DecodePixelTypeProperty); }
            set { SetValue(DecodePixelTypeProperty, value); }
        }

        /// <summary>
        /// Gets or sets DecodePixelWidth for underlying bitmap
        /// </summary>
        public int DecodePixelWidth
        {
            get { return (int)GetValue(DecodePixelWidthProperty); }
            set { SetValue(DecodePixelWidthProperty, value); }
        }

        /// <summary>
        /// Gets or sets the stretch behavior of the image
        /// </summary>
        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets cache state
        /// </summary>
        public bool IsCacheEnabled
        {
            get { return (bool)GetValue(IsCacheEnabledProperty); }
            set { SetValue(IsCacheEnabledProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets is lazy loading enable. (17763 or higher supported)
        /// </summary>
        /// <remarks>Windows 10 build 17763 or higher required.</remarks>
        public bool EnableLazyLoading
        {
            get { return (bool)GetValue(EnableLazyLoadingProperty); }
            set { SetValue(EnableLazyLoadingProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating the threshold for triggering lazy loading.
        /// </summary>
        public double LazyLoadingThreshold
        {
            get { return (double)GetValue(LazyLoadingThresholdProperty); }
            set { SetValue(LazyLoadingThresholdProperty, value); }
        }

        private static void EnableLazyLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageEx2Base control)
            {
                var value = (bool)e.NewValue;
                if (value)
                {
                    control.LayoutUpdated += control.ImageExBase_LayoutUpdated;

                    control.InvalidateLazyLoading();
                }
                else
                {
                    control.LayoutUpdated -= control.ImageExBase_LayoutUpdated;
                }
            }
        }

        private static void LazyLoadingThresholdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageEx2Base control && control.EnableLazyLoading)
            {
                control.InvalidateLazyLoading();
            }
        }
    }
}
