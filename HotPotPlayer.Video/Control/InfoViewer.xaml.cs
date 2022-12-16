// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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

namespace HotPotPlayer.Video.Control
{
    public sealed partial class InfoViewer : UserControlBase
    {
        public InfoViewer()
        {
            this.InitializeComponent();
        }

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
    }
}
