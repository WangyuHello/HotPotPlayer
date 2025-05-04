using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HotPotPlayer.Services
{
    public partial class ServiceBase : ObservableObject, IDisposable
    {
        public ServiceBase() { }

        public void SetProperty<T>(ref T oldValue, T newValue, Action<T> callback, [CallerMemberName] string propertyName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
            {
                oldValue = newValue;
                try
                {
                    OnPropertyChanged(propertyName);
                    callback?.Invoke(newValue);
                }
                catch (Exception)
                {

                }
            }
        }

        public virtual void Dispose() { }
    }

    public class ServiceBaseWithConfig: ServiceBase
    {
        protected ConfigBase Config { get; init; }
        protected DispatcherQueue UIQueue { get; init; }
        protected AppBase App { get; init; }

        public ServiceBaseWithConfig(ConfigBase config, DispatcherQueue uiThread = null, AppBase app = null)
        {
            Config = config;
            UIQueue = uiThread;
            App = app;
        }
    }
}
