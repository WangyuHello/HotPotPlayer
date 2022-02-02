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
    }
}
