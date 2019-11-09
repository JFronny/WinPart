using System;
using System.Management;
using WinPart.Properties;
#pragma warning disable IDE1006
namespace WinPart
{
    class LogicalDrive
    {
        public string name;
        public ManagementObject mo;
        public LogicalDrive getFromMO(ManagementObject MO)
        {
            name = (string)MO.Properties["Name"].Value;
            mo = MO;
            return this;
        }

        public void getInfo()
        {
            foreach (string s in Resources.LogicalDrive.Split(new[] { "\r\n" }, StringSplitOptions.None))
                Program.printInfo(mo, s);
        }
    }
}
