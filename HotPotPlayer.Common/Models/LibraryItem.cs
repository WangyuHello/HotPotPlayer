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
                _isRemovableDisk ??= RemovableDiskHelper.IsRemovableDisk(Path);
                return _isRemovableDisk.Value;
            }
        }

        private string _removableDiskType;
        public string RemovableDiskType => _removableDiskType ??= RemovableDiskHelper.GetRemovableDiskType(Path);
    }
}
