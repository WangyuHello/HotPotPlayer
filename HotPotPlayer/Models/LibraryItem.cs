using HotPotPlayer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Models
{
    public record LibraryItem
    {
        public string Path { get; set; }
        public bool IsSystemLibrary { get; set; }

        public bool GetRemoveVisible => !IsSystemLibrary;

        public bool GetAvailable => System.IO.Directory.Exists(Path);

        private bool? _isRemovableDisk;
        public bool IsRemovableDisk
        {
            get
            {
                if (_isRemovableDisk == null)
                {
                    var disk = RemovableDiskHelper.RemovableDisks;
                    foreach (var item in disk)
                    {
                        if (Path.StartsWith(item))
                        {
                            _isRemovableDisk = true;
                            return true;
                        }
                    }
                    _isRemovableDisk = false;
                }
                return _isRemovableDisk.Value;
            }
        }
    }
}
