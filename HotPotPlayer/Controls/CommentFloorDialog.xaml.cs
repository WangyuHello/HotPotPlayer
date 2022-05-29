using HotPotPlayer.Models.CloudMusic;
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.Controls
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CommentFloorDialog : PageBase
    {
        public CommentFloorDialog(ObservableCollection<CloudCommentItem> comments, CloudCommentItem current)
        {
            Current = current;
            Comments = comments;
            this.InitializeComponent();
        }

        CloudCommentItem Current;
        ObservableCollection<CloudCommentItem> Comments;
    }
}
