using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HotPotPlayer.Services
{
    public class ServiceBase : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void Set<T>(ref T oldValue, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
            {
                oldValue = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void Set<T>(ref T oldValue, T newValue, Action<T> callback, [CallerMemberName] string propertyName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
            {
                oldValue = newValue;
                try
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                    callback?.Invoke(newValue);
                }
                catch (Exception)
                {

                }
            }
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnPropertyChanged(PropertyChangedEventArgs args = null, [CallerMemberName] string propertyName = "")
        {
            args ??= new PropertyChangedEventArgs(propertyName);
            PropertyChanged?.Invoke(this, args);
        }

        public ServiceBase() { }

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
