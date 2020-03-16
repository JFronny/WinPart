using System;
using System.Management;
using WinPart.Properties;

namespace WinPart.Data_structure
{
    internal class LogicalDrive
    {
        private ManagementBaseObject _mo;
        public string Name;

        public LogicalDrive GetFromMo(ManagementBaseObject mo)
        {
            Name = (string) mo.Properties["Name"].Value;
            _mo = mo;
            return this;
        }

        public void GetInfo()
        {
            foreach (string s in Resources.LogicalDrive.Split(new[] {"\r\n"}, StringSplitOptions.None))
                Program.PrintInfo(_mo, s);
        }
    }
}