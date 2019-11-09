using System;
using System.Collections.Generic;
using System.Management;
using WinPart.Properties;
#pragma warning disable IDE1006
namespace WinPart
{
    class Partition
    {
        public List<LogicalDrive> logicalDrives;
        public string name;
        public ManagementObject mo;
        public Partition getFromMO(ManagementObject MO)
        {
            mo = MO;
            logicalDrives = new List<LogicalDrive>() { };
            foreach (ManagementObject m in new ManagementObjectSearcher(string.Format("associators of {{{0}}} where AssocClass = Win32_LogicalDiskToPartition", MO.Path.RelativePath)).Get())
                logicalDrives.Add(new LogicalDrive().getFromMO(m));
            name = (string)MO.Properties["Type"].Value;
            return this;
        }

        public void getInfo()
        {
            foreach (string s in Resources.Partition.Split(new[] { "\r\n" }, StringSplitOptions.None))
                Program.printInfo(mo, s);
        }
    }
}
