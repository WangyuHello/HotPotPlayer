using CommunityToolkit.WinUI;
using HotPotPlayer.Helpers;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotPotPlayer.UI.Controls
{
    public sealed partial class TransitionImage : UserControlBase
    {
        public TransitionImage()
        {
            InitializeComponent();
        }

        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(Uri), typeof(TransitionImage), new PropertyMetadata(default, SourceChanged));

        private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TransitionImage)d).OnSourceChanged(e.NewValue as Uri);
        }

        private Uri _prevUri;
        private Uri _currentUri;
        public event Action<ImageSource> SourceGotFromCache;
        private CancellationTokenSource _tokenSource;
        readonly Compositor _compositor = CompositionTarget.GetCompositorForCurrentThread();
        private bool _image1OnLeft = true;
        private bool _image1Center = true;

        private ExpressionAnimation _expressionAnim;
        private SpringVector3NaturalMotionAnimation _springAnimation;

        private async void OnSourceChanged(Uri newSource)
        {
            _tokenSource?.Cancel();
            _tokenSource = new CancellationTokenSource();

            var img = await ProvideCachedResourceAsync(newSource, _tokenSource.Token);
            SourceGotFromCache?.Invoke(img);
            if (!_tokenSource.IsCancellationRequested)
            {
                if (newSource == _prevUri)
                {
                    // Backward transition
                    if (_image1OnLeft)
                    {
                        if (_image1Center)
                        {
                            //   -  -  -
                            //      1  2  -->
                            //   2  1
                            //      2  1

                            Image2.Source = img;

                            _expressionAnim ??= _compositor.CreateExpressionAnimation();
                            _expressionAnim.Expression = "Vector3(source.Translation.X + 2 * source.ActualSize.X, 0, 0)";
                            _expressionAnim.Target = "Translation";
                            _expressionAnim.SetExpressionReferenceParameter("source", Image2);
                            Image1.StartAnimation(_expressionAnim);

                            _springAnimation ??= _compositor.CreateSpringVector3Animation();
                            _springAnimation.Target = "Translation";
                            _springAnimation.InitialValue = new Vector3(-160, 0, 0);
                            _springAnimation.FinalValue = new Vector3(-80, 0, 0);
                            Image2.StartAnimation(_springAnimation);

                            _image1Center = false;
                            _image1OnLeft = false;
                        }
                        else
                        {
                            //   -  -  -
                            //   1  2     -->
                            //
                            //      1  2

                            Image1.Source = img;

                            _expressionAnim ??= _compositor.CreateExpressionAnimation();
                            _expressionAnim.Expression = "Vector3(source.Translation.X, 0, 0)";
                            _expressionAnim.Target = "Translation";
                            _expressionAnim.SetExpressionReferenceParameter("source", Image1);
                            Image2.StartAnimation(_expressionAnim);

                            _springAnimation ??= _compositor.CreateSpringVector3Animation();
                            _springAnimation.Target = "Translation";
                            _springAnimation.InitialValue = new Vector3(-80, 0, 0);
                            _springAnimation.FinalValue = new Vector3(0, 0, 0);
                            Image1.StartAnimation(_springAnimation);

                            _image1Center = true;
                            _image1OnLeft = true;
                        }
                    }
                    else
                    {
                        if (_image1Center)
                        {
                            //   -  -  -
                            //   2  1     -->
                            //
                            //      2  1

                            Image2.Source = img;

                            _expressionAnim ??= _compositor.CreateExpressionAnimation();
                            _expressionAnim.Expression = "Vector3(source.Translation.X + 2 * source.ActualSize.X, 0, 0)";
                            _expressionAnim.Target = "Translation";
                            _expressionAnim.SetExpressionReferenceParameter("source", Image2);
                            Image1.StartAnimation(_expressionAnim);

                            _springAnimation ??= _compositor.CreateSpringVector3Animation();
                            _springAnimation.Target = "Translation";
                            _springAnimation.InitialValue = new Vector3(-160, 0, 0);
                            _springAnimation.FinalValue = new Vector3(-80, 0, 0);
                            Image2.StartAnimation(_springAnimation);

                            _image1Center = false;
                            _image1OnLeft = false;
                        }
                        else
                        {
                            //   -  -  -
                            //      2  1  -->
                            //   1  2
                            //      1  2

                            Image1.Source = img;

                            _expressionAnim ??= _compositor.CreateExpressionAnimation();
                            _expressionAnim.Expression = "Vector3(source.Translation.X, 0, 0)";
                            _expressionAnim.Target = "Translation";
                            _expressionAnim.SetExpressionReferenceParameter("source", Image1);
                            Image2.StartAnimation(_expressionAnim);

                            _springAnimation ??= _compositor.CreateSpringVector3Animation();
                            _springAnimation.Target = "Translation";
                            _springAnimation.InitialValue = new Vector3(-80, 0, 0);
                            _springAnimation.FinalValue = new Vector3(0, 0, 0);
                            Image1.StartAnimation(_springAnimation);

                            _image1Center = true;
                            _image1OnLeft = true;
                        }
                    }
                }
                else
                {
                    // Forward transition
                    if (_image1OnLeft)
                    {
                        if (_image1Center)
                        {
                            //   -  -  -
                            //      1  2 <--
                            //
                            //   1  2

                            Image2.Source = img;

                            _expressionAnim ??= _compositor.CreateExpressionAnimation();
                            _expressionAnim.Expression = "Vector3(source.Translation.X, 0, 0)";
                            _expressionAnim.Target = "Translation";
                            _expressionAnim.SetExpressionReferenceParameter("source", Image2);
                            Image1.StartAnimation(_expressionAnim);

                            _springAnimation ??= _compositor.CreateSpringVector3Animation();
                            _springAnimation.Target = "Translation";
                            _springAnimation.InitialValue = new Vector3(0, 0, 0);
                            _springAnimation.FinalValue = new Vector3(-80, 0, 0);
                            Image2.StartAnimation(_springAnimation);

                            _image1Center = false;
                            _image1OnLeft = true;
                        }
                        else
                        {
                            //   -  -  -
                            //   1  2    
                            //      2  1  <--
                            //   2  1  

                            Image1.Source = img;

                            _expressionAnim ??= _compositor.CreateExpressionAnimation();
                            _expressionAnim.Expression = "Vector3(source.Translation.X - 2 * source.ActualSize.X, 0, 0)";
                            _expressionAnim.Target = "Translation";
                            _expressionAnim.SetExpressionReferenceParameter("source", Image1);
                            Image2.StartAnimation(_expressionAnim);

                            _springAnimation ??= _compositor.CreateSpringVector3Animation();
                            _springAnimation.Target = "Translation";
                            _springAnimation.InitialValue = new Vector3(80, 0, 0);
                            _springAnimation.FinalValue = new Vector3(0, 0, 0);
                            Image1.StartAnimation(_springAnimation);

                            _image1Center = true;
                            _image1OnLeft = false;
                        }
                    }
                    else
                    {
                        if (_image1Center)
                        {
                            //   -  -  -
                            //   2  1    
                            //      1  2  <--
                            //   1  2

                            Image2.Source = img;

                            _expressionAnim ??= _compositor.CreateExpressionAnimation();
                            _expressionAnim.Expression = "Vector3(source.Translation.X, 0, 0)";
                            _expressionAnim.Target = "Translation";
                            _expressionAnim.SetExpressionReferenceParameter("source", Image2);
                            Image1.StartAnimation(_expressionAnim);

                            _springAnimation ??= _compositor.CreateSpringVector3Animation();
                            _springAnimation.Target = "Translation";
                            _springAnimation.InitialValue = new Vector3(0, 0, 0);
                            _springAnimation.FinalValue = new Vector3(-80, 0, 0);
                            Image2.StartAnimation(_springAnimation);

                            _image1Center = false;
                            _image1OnLeft= true;
                        }
                        else 
                        {
                            //   -  -  -
                            //      2  1  <--
                            //
                            //   2  1

                            Image1.Source = img;

                            _expressionAnim ??= _compositor.CreateExpressionAnimation();
                            _expressionAnim.Expression = "Vector3(source.Translation.X - 2 * source.ActualSize.X, 0, 0)";
                            _expressionAnim.Target = "Translation";
                            _expressionAnim.SetExpressionReferenceParameter("source", Image1);
                            Image2.StartAnimation(_expressionAnim);

                            _springAnimation ??= _compositor.CreateSpringVector3Animation();
                            _springAnimation.Target = "Translation";
                            _springAnimation.InitialValue = new Vector3(80, 0, 0);
                            _springAnimation.FinalValue = new Vector3(0, 0, 0);
                            Image1.StartAnimation(_springAnimation);

                            _image1Center = true;
                            _image1OnLeft = false;
                        }

                    }
                }

                _prevUri = _currentUri;
                _currentUri = newSource;
            }
        }

        private static async Task<ImageSource> ProvideCachedResourceAsync(Uri imageUri, CancellationToken token)
        {
            var image = await ImageCacheEx.Instance.GetFromCacheAsync(imageUri, true, token);
            return image;
        }
    }
}
