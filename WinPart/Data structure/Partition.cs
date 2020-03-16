using System;
using System.Collections.Generic;
using System.Management;
using WinPart.Properties;

namespace WinPart.Data_structure
{
    internal class Partition
    {
        private ManagementBaseObject _mo;
        public List<LogicalDrive> LogicalDrives;
        public string Name;

        public Partition GetFromMo(ManagementObject mo)
        {
            _mo = mo;
            LogicalDrives = new List<LogicalDrive>();
            foreach (ManagementBaseObject m in new ManagementObjectSearcher(
                $"associators of {{{mo.Path.RelativePath}}} where AssocClass = Win32_LogicalDiskToPartition").Get())
                LogicalDrives.Add(new LogicalDrive().GetFromMo(m));
            Name = (string) mo.Properties["Type"].Value;
            return this;
        }

        public void GetInfo()
        {
            foreach (string s in Resources.Partition.Split(new[] {"\r\n"}, StringSplitOptions.None))
                Program.PrintInfo(_mo, s);
        }
    }
}