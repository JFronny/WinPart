using System;
using System.Collections.Generic;
using System.Management;
using WinPart.Properties;
#pragma warning disable IDE1006
namespace WinPart
{
    class Device
    {
        public List<Partition> partitions;
        public string name;
        public ManagementObject mo;
        public Device getFromMO(ManagementObject MO)
        {
            mo = MO;
            partitions = new List<Partition>() { };
            foreach (ManagementObject m in new ManagementObjectSearcher(string.Format("associators of {{{0}}} where AssocClass = Win32_DiskDriveToDiskPartition", MO.Path.RelativePath)).Get())
                partitions.Add(new Partition().getFromMO(m));
            name = (string)MO.Properties["Model"].Value;
            return this;
        }

        public void getInfo()
        {
            foreach (string s in Resources.Device.Split(new[] { "\r\n" }, StringSplitOptions.None))
                Program.printInfo(mo, s);
        }
    }
}
