using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Helpers
{
    public record RemovableDisk
    {
        public string Disk { get; set; }
        public string Type { get; set; }
    }

    public static class RemovableDiskHelper
    {
        private static List<RemovableDisk> _removableDisks;
        public static List<RemovableDisk> RemovableDisks => _removableDisks ??= GetRemovableDisk();

        private static List<RemovableDisk> GetRemovableDisk()
        {
            var lstDisk = new List<RemovableDisk>();
            var mgtCls = new ManagementClass("Win32_DiskDrive");
            var disks = mgtCls.GetInstances();
            foreach (ManagementObject mo in disks)
            {
                //if (mo.Properties["InterfaceType"].Value.ToString() != "SCSI" 
                //    && mo.Properties["InterfaceType"].Value.ToString() != "USB"
                //    )
                //    continue;

                if (mo.Properties["MediaType"].Value == null ||
                    mo.Properties["MediaType"].Value.ToString() != "External hard disk media")
                {
                    continue;
                }

                //foreach (var prop in mo.Properties)
                //{
                //    Console.WriteLine(prop.Name + "\t" + prop.Value);
                //}

                foreach (ManagementObject diskPartition in mo.GetRelated("Win32_DiskPartition"))
                {
                    foreach (ManagementBaseObject disk in diskPartition.GetRelated("Win32_LogicalDisk"))
                    {
                        lstDisk.Add(new RemovableDisk
                        {
                            Disk = disk.Properties["Name"].Value.ToString(),
                            Type = mo.Properties["InterfaceType"].Value.ToString(),
                        });
                    }
                }

            }
            return lstDisk;
        }

        public static bool IsRemovableDisk(string path)
        {
            foreach (var item in RemovableDisks)
            {
                if (path.StartsWith(item.Disk))
                {
                    return true;
                }
            }
            return false;
        }

        public static string GetRemovableDiskType(string path)
        {
            foreach (var item in RemovableDisks)
            {
                if (path.StartsWith(item.Disk))
                {
                    return item.Type;
                }
            }
            return string.Empty;
        }
    }
}
