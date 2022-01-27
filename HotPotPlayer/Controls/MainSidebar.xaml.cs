using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
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

namespace HotPotPlayer.Controls
{
    public sealed partial class MainSidebar : UserControl
    {
        public MainSidebar()
        {
            this.InitializeComponent();
        }

        public bool IsBackEnable
        {
            get { return (bool)GetValue(IsBackEnableProperty); }
            set { SetValue(IsBackEnableProperty, value); }
        }

        public static readonly DependencyProperty IsBackEnableProperty =
            DependencyProperty.Register("IsBackEnable", typeof(bool), typeof(MainSidebar), new PropertyMetadata(false));

        public string SelectedPageName
        {
            get { return (string)GetValue(SelectedPageNameProperty); }
            set { SetValue(SelectedPageNameProperty, value); }
        }

        public static readonly DependencyProperty SelectedPageNameProperty =
            DependencyProperty.Register("SelectedPageName", typeof(string), typeof(MainSidebar), new PropertyMetadata(string.Empty, SelectedPageNameCallback));

        private static void SelectedPageNameCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sidbar = (MainSidebar)d;
            var button = e.NewValue switch
            {
                "Music" => sidbar.Music,
                "Video" => sidbar.Video,
                "Bilibili" => sidbar.Bilibili,
                "CloudMusic" => sidbar.CloudMusic,
                "Setting" => sidbar.Setting,
                _ => null,
            };
            if (button != null)
            {
                sidbar.StartPopAnimation(button);
            }
        }

        public event Action<string> SelectedPageNameChanged;
        public event Action OnBackClick;

        private bool GetEnableState(string name, string selectedName)
        {
            return !selectedName.StartsWith(name);
        }

        private ImageSource GetBilibiliImage(string selectedName)
        {
            return selectedName == "Bilibili" ? (BitmapSource)Resources["BilibiliBlue"] : (BitmapSource)Resources["Bilibili"];
        }

        private Visibility GetPopVisibility(string selected)
        {
            if (!string.IsNullOrEmpty(selected))
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        private void NavigateClick(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            var name = (string)b.Tag;
            SelectedPageNameChanged?.Invoke(name);
            SelectedPageName = name;
        }

        private void BackClick(object sender, RoutedEventArgs e)
        {
            OnBackClick?.Invoke();
        }

        ExpressionAnimation popMoveAnimation;
        SpringScalarNaturalMotionAnimation popMoveAnimation2;
        readonly Compositor _compositor = CompositionTarget.GetCompositorForCurrentThread();

        private void StartPopAnimation(Button targetButton)
        {
            if (popMoveAnimation == null)
            {
                popMoveAnimation = _compositor.CreateExpressionAnimation();
                popMoveAnimation.Expression = "Vector3(0, source.ActualOffset.Y - 12, 0)";
                popMoveAnimation.Target = "Translation";
            }
            if (popMoveAnimation2 == null)
            {
                popMoveAnimation2 = _compositor.CreateSpringScalarAnimation();
                popMoveAnimation2.Target = "Translation.Y";
            }
            popMoveAnimation2.FinalValue = targetButton.ActualOffset.Y - 12;
            popMoveAnimation.SetExpressionReferenceParameter("source", targetButton);
            ButtonPop.StartAnimation(popMoveAnimation2);
            //ButtonPop.StartAnimation(popMoveAnimation);
        }
    }
}
