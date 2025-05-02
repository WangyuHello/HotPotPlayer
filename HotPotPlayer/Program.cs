using System;
using System.Threading;
using System.Threading.Tasks;
using HotPotPlayer.Interop.Helper;
using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppLifecycle;
using Windows.ApplicationModel.Activation;
using Windows.Storage;

namespace HotPotPlayer
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            WinRT.ComWrappersSupport.InitializeComWrappers();
            bool isRedirect = DecideRedirection();
            if (!isRedirect)
            {
                Microsoft.UI.Xaml.Application.Start((p) =>
                {
                    var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                    SynchronizationContext.SetSynchronizationContext(context);
                    _ = new App();
                });
            }
        }

        private static bool DecideRedirection()
        {
            bool isRedirect = false;

            AppActivationArguments args = AppInstance.GetCurrent().GetActivatedEventArgs();
            ExtendedActivationKind kind = args.Kind;

            try
            {
                AppInstance keyInstance = AppInstance.FindOrRegisterForKey("hhotpotplayerr");

                if (keyInstance.IsCurrent)
                {
                    keyInstance.Activated += OnActivated;
                }
                else
                {
                    isRedirect = true;
                    RedirectActivationTo(args, keyInstance);
                }
            }

            catch (Exception)
            {

            }

            return isRedirect;
        }

        private static IntPtr redirectEventHandle = IntPtr.Zero;

        // Do the redirection on another thread, and use a non-blocking
        // wait method to wait for the redirection to complete.
        public static void RedirectActivationTo(AppActivationArguments args, AppInstance keyInstance)
        {
            redirectEventHandle = WindowHelper.CreateEvent(IntPtr.Zero, true, false, null);
            Task.Run(() =>
            {
                keyInstance.RedirectActivationToAsync(args).AsTask().Wait();
                WindowHelper.SetEvent(redirectEventHandle);
            });
            uint CWMO_DEFAULT = 0;
            uint INFINITE = 0xFFFFFFFF;
            _ = WindowHelper.CoWaitForMultipleObjects(CWMO_DEFAULT, INFINITE, 1, new IntPtr[] { redirectEventHandle }, out uint handleIndex);
        }

        private static void OnActivated(object sender, AppActivationArguments args)
        {
            ExtendedActivationKind kind = args.Kind;
        }
    }
}
