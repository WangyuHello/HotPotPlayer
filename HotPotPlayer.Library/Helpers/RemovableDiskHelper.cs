using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace HotPotPlayer.Helpers
{
    public static class RemovableDiskHelper
    {
        public static List<string> GetRemovableDisk()
        {
            var lstDisk = new List<string>();
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
                        lstDisk.Add(disk.Properties["Name"].Value.ToString());
                    }
                }

                //Console.WriteLine("-------------------------------------------------------------------------------------------");
            }
            return lstDisk;
        }
    }
}
