// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HotPotPlayer.Bilibili.Models.Video;
using HotPotPlayer.Models;
using HotPotPlayer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Video.UI.Controls
{
    public sealed partial class InfoViewer : UserControlBase
    {
        public InfoViewer()
        {
            this.InitializeComponent();
        }

        public bool IsFullPageHost
        {
            get { return (bool)GetValue(IsFullPageHostProperty); }
            set { SetValue(IsFullPageHostProperty, value); }
        }

        public static readonly DependencyProperty IsFullPageHostProperty =
            DependencyProperty.Register("IsFullPageHost", typeof(bool), typeof(InfoViewer), new PropertyMetadata(false));


        public List<string> Definitions
        {
            get { return (List<string>)GetValue(DefinitionsProperty); }
            set { SetValue(DefinitionsProperty, value); }
        }

        public static readonly DependencyProperty DefinitionsProperty =
            DependencyProperty.Register("Definitions", typeof(List<string>), typeof(InfoViewer), new PropertyMetadata(default));

        public string SelectedDefinition
        {
            get { return (string)GetValue(SelectedDefinitionProperty); }
            set { SetValue(SelectedDefinitionProperty, value); }
        }

        public static readonly DependencyProperty SelectedDefinitionProperty =
            DependencyProperty.Register("SelectedDefinition", typeof(string), typeof(InfoViewer), new PropertyMetadata(default));

        public event EventHandler<SelectionChangedEventArgs> DefinitionSelectionChanged;
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DefinitionSelectionChanged?.Invoke(sender, e);
        }

        public bool IsVideoInfoOn
        {
            get { return (bool)GetValue(IsVideoInfoOnProperty); }
            set { SetValue(IsVideoInfoOnProperty, value); }
        }

        public static readonly DependencyProperty IsVideoInfoOnProperty =
            DependencyProperty.Register("IsVideoInfoOn", typeof(bool), typeof(InfoViewer), new PropertyMetadata(default));


        public bool IsPbpOn
        {
            get { return (bool)GetValue(IsPbpOnProperty); }
            set { SetValue(IsPbpOnProperty, value); }
        }

        public static readonly DependencyProperty IsPbpOnProperty =
            DependencyProperty.Register("IsPbpOn", typeof(bool), typeof(InfoViewer), new PropertyMetadata(default));

        public CodecStrategy SelectedCodecStrategy
        {
            get { return (CodecStrategy)GetValue(SelectedCodecStrategyProperty); }
            set { SetValue(SelectedCodecStrategyProperty, value); }
        }

        public static readonly DependencyProperty SelectedCodecStrategyProperty =
            DependencyProperty.Register("SelectedCodecStrategy", typeof(CodecStrategy), typeof(InfoViewer), new PropertyMetadata(CodecStrategy.Default, SelectedCodecStrategyChanged));

        private static void SelectedCodecStrategyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var i = d as InfoViewer;
            i.Default.IsChecked = false;
            i.AV1First.IsChecked = false;
            i.HEVCFirst.IsChecked = false;
            i.AVCFirst.IsChecked = false;
            var sel = (CodecStrategy)e.NewValue;
            switch (sel)
            {
                case CodecStrategy.Default:
                    i.Default.IsChecked = true;
                    break;
                case CodecStrategy.AV1First:
                    i.AV1First.IsChecked = true;
                    break;
                case CodecStrategy.HEVCFirst:
                    i.HEVCFirst.IsChecked = true;
                    break;
                case CodecStrategy.AVCFirst:
                    i.AVCFirst.IsChecked = true;
                    break;
                default:
                    break;
            }
        }

        private void CodecSelectClick(object sender, RoutedEventArgs e)
        {
            var b = sender as ToggleButton;
            var tag = b.Tag as string;
            SelectedCodecStrategy = tag switch
            {
                "Default" => CodecStrategy.Default,
                "AV1First" => CodecStrategy.AV1First,
                "HEVCFirst" => CodecStrategy.HEVCFirst,
                "AVCFirst" => CodecStrategy.AVCFirst,
                _ => CodecStrategy.Default,
            };
        }


        public PlayMode SelectedPlayMode
        {
            get { return (PlayMode)GetValue(SelectedPlayModeProperty); }
            set { SetValue(SelectedPlayModeProperty, value); }
        }

        public static readonly DependencyProperty SelectedPlayModeProperty =
            DependencyProperty.Register("SelectedPlayMode", typeof(PlayMode), typeof(InfoViewer), new PropertyMetadata(default, SelectedPlayModeChanged));


        private static void SelectedPlayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var i = d as InfoViewer;
            i.Loop.IsChecked = false;
            i.Single.IsChecked = false;
            var sel = (PlayMode)e.NewValue;
            switch (sel)
            {
                case PlayMode.Loop:
                    i.Loop.IsChecked = true;
                    break;
                case PlayMode.SingleLoop:
                    i.Single.IsChecked = true;
                    break;
                default:
                    i.Loop.IsChecked = true;
                    break;
            }
        }

        private void PlayModeSelectClick(object sender, RoutedEventArgs e)
        {
            var b = sender as ToggleButton;
            var tag = b.Tag as string;
            SelectedPlayMode = tag switch
            {
                "Loop" => PlayMode.Loop,
                "SingleLoop" => PlayMode.SingleLoop,
                _ => PlayMode.SingleLoop,
            };
        }

        private Visibility GetNonFullPageHostVisible(bool fullpagehost)
        {
            return fullpagehost ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
