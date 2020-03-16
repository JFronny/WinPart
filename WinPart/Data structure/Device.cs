using System;
using System.Collections.Generic;
using System.Management;
using WinPart.Properties;

namespace WinPart.Data_structure
{
    internal class Device
    {
        private ManagementBaseObject _mo;
        public string Name;
        public List<Partition> Partitions;

        public Device GetFromMo(ManagementObject mo)
        {
            _mo = mo;
            Partitions = new List<Partition>();
            foreach (ManagementObject m in new ManagementObjectSearcher(
                $"associators of {{{mo.Path.RelativePath}}} where AssocClass = Win32_DiskDriveToDiskPartition").Get())
                Partitions.Add(new Partition().GetFromMo(m));
            Name = (string) mo.Properties["Model"].Value;
            return this;
        }

        public void GetInfo()
        {
            foreach (string s in Resources.Device.Split(new[] {"\r\n"}, StringSplitOptions.None))
                Program.PrintInfo(_mo, s);
        }
    }
}