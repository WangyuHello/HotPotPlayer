﻿using HotPotPlayer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer
{
    public class UserControlBase: UserControl, INotifyPropertyChanged, IComponentServiceLocator
    {
        public ConfigBase Config => ((IComponentServiceLocator)Application.Current).Config;

        public NetEaseMusicService NetEaseMusicService => ((IComponentServiceLocator)Application.Current).NetEaseMusicService;

        public LocalMusicService LocalMusicService => ((IComponentServiceLocator)Application.Current).LocalMusicService;

        public MusicPlayer MusicPlayer => ((IComponentServiceLocator)Application.Current).MusicPlayer;

        public event PropertyChangedEventHandler PropertyChanged;
        public void Set<T>(ref T oldValue, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
            {
                oldValue = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}