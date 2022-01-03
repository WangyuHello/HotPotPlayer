using HotPotPlayer.Models;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Services
{
    internal class LocalMusicService
    {
        public List<MusicItem> GetAllMusicFromLibrary()
        {
            var libs = ((App)Application.Current).MusicLibrary;
            List<FileInfo> files = new();
            foreach (var lib in libs)
            {
                GetAllMusic(files, new DirectoryInfo(lib));
            }

            var r = files.Select(f =>
            {
                var tfile = TagLib.File.Create(f.FullName);

                return new MusicItem
                {
                    Title = tfile.Tag.Title,
                    Artist = tfile.Tag.Performers,
                    Album = tfile.Tag.Album,
                    Duration = tfile.Properties.Duration,
                    File = f,
                    Source = new Uri(f.FullName)
                };
            }).ToList();

            return r;
        }

        private void GetAllMusic(List<FileInfo> files, DirectoryInfo dir)
        {
            foreach(var d in dir.GetDirectories())
            {
                files.AddRange(d.GetFiles("*.flac"));
                GetAllMusic(files, d);
            }
        }

        public LocalMusicService()
        {
            localMusicBackgroundWorker.DoWork += LocalMusicBackgroundWorker_DoWork;
            localMusicBackgroundWorker.RunWorkerCompleted += LocalMusicBackgroundWorker_RunWorkerCompleted;
        }

        public void StartLoadLocalMusic()
        {
            localMusicBackgroundWorker.WorkerSupportsCancellation = true;
            if (localMusicBackgroundWorker.IsBusy)
            {
                return;
            }
            localMusicBackgroundWorker.RunWorkerAsync();
        }

        private void LocalMusicBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.WriteLine("LoadLocalMusic Complete");
        }

        private void LocalMusicBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            GetAllMusicFromLibrary();
        }

        public BackgroundWorker localMusicBackgroundWorker = new BackgroundWorker();
    }
}
