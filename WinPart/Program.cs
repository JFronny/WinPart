using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using CommandLine.Utility;
#pragma warning disable IDE1006
namespace WinPart {
    class Program {
        List<Device> drives;
        bool longInputSupport;
        static void Main(string[] args) => new Program().main(new Arguments(args));
        void main(Arguments args) {
            longInputSupport = args["li"] != null;
            bool run = true;
            if (args["help"] != null) {
                Console.WriteLine("WinPart is a tool for manipulating storage.\r\n\r\nWinPart [-li] [-help]\r\n\r\nli    Enables large inputs (over 1 char)\r\nhelp  Shows this message");
                run = false;
            }
            if (run)
                mainWindow();
        }

        void mainWindow() {
            Console.Clear();
            Console.WriteLine("Use \"WinPart -help\" for CMD Args");
            Console.WriteLine("Welcome to WinPart. Choose one of the disks by their number to continue.");
            try {
                drives = new List<Device>() { };
                int i = 0;
                foreach (ManagementObject mo in new ManagementObjectSearcher("select * from Win32_DiskDrive").Get()) {
                    i++;
                    drives.Add(new Device().getFromMO(mo));
                    Console.WriteLine(i.ToString() + ": " + drives[drives.Count - 1].name);
                }
                Device drive = drives[int.Parse(getInput()) - 1];
                Console.WriteLine("Choose one of the operations by their number to continue.\r\n1: Info\r\n2: Open");
                switch (int.Parse(getInput()))
                {
                    case 1:
                        drive.getInfo();
                        break;
                    case 2:
                        Console.WriteLine("Processing...\r\nChoose one of the partitions by their number to continue.");
                        for (i = 0; i < drive.partitions.Count; i++)
                            Console.WriteLine((i + 1).ToString() + ": " + drive.partitions[i].name);
                        Partition partition = drive.partitions[int.Parse(getInput()) - 1];
                        Console.WriteLine("Choose one of the operations by their number to continue.\r\n1: Info\r\n2: Open");
                        switch (int.Parse(getInput()))
                        {
                            case 1:
                                partition.getInfo();
                                break;
                            case 2:
                                Console.WriteLine("Processing...\r\nChoose one of the logical drives by their number to continue.");
                                for (i = 0; i < partition.logicalDrives.Count; i++)
                                    Console.WriteLine((i + 1).ToString() + ": " + partition.logicalDrives[i].name);
                                LogicalDrive logicalDrive = partition.logicalDrives[int.Parse(getInput()) - 1];
                                Console.WriteLine("Choose one of the operations by their number to continue.\r\n1: Info\r\n2: Explorer");
                                switch (int.Parse(getInput()))
                                {
                                    case 1:
                                        logicalDrive.getInfo();
                                        break;
                                    case 2:
                                        Process.Start(logicalDrive.name);
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            default:
                                Console.WriteLine("Invalid.");
                                break;
                        }
                        break;
                    default:
                        Console.WriteLine("Invalid.");
                        break;
                }
            } catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        string getInput() {
            Console.Write("\r\n==> ");
            var out_ = "";
            if (longInputSupport)
                out_ = Console.ReadLine();
            else
                out_ = Console.ReadKey().KeyChar.ToString();
            Console.Write("\r\n\r\n");
            return out_;
        }
    }

    class Device {
        public List<Partition> partitions;
        public string name;
        public ManagementObject mo;
        public Device getFromMO(ManagementObject MO) {
            partitions = new List<Partition>() { };
            foreach (ManagementObject m in new ManagementObjectSearcher(string.Format("associators of {{{0}}} where AssocClass = Win32_DiskDriveToDiskPartition", MO.Path.RelativePath)).Get())
                partitions.Add(new Partition().getFromMO(m));
            name = (string)MO.Properties["DeviceId"].Value;
            mo = MO;
            return this;
        }

        public void getInfo() {
            Console.WriteLine("PhysicalName: " + Convert.ToString(mo.Properties["Name"].Value));
            Console.WriteLine("DiskName: " + Convert.ToString(mo.Properties["Caption"].Value));
            Console.WriteLine("DiskModel: " + Convert.ToString(mo.Properties["Model"].Value));
            Console.WriteLine("DiskInterface: " + Convert.ToString(mo.Properties["InterfaceType"].Value));
            Console.WriteLine("Capabilities: " + (ushort[])mo.Properties["Capabilities"].Value);
            Console.WriteLine("MediaLoaded: " + Convert.ToBoolean(mo.Properties["MediaLoaded"].Value));
            Console.WriteLine("MediaType: " + Convert.ToString(mo.Properties["MediaType"].Value));
            Console.WriteLine("MediaSignature: " + Convert.ToUInt32(mo.Properties["Signature"].Value));
            Console.WriteLine("MediaStatus: " + Convert.ToString(mo.Properties["Status"].Value));
        }
    }

    class Partition {
        public List<LogicalDrive> logicalDrives;
        public string name;
        public ManagementObject mo;
        public Partition getFromMO(ManagementObject MO) {
            logicalDrives = new List<LogicalDrive>() { };
            foreach (ManagementObject m in new ManagementObjectSearcher(string.Format("associators of {{{0}}} where AssocClass = Win32_LogicalDiskToPartition", MO.Path.RelativePath)).Get())
                logicalDrives.Add(new LogicalDrive().getFromMO(m));
            name = MO.ToString();
            mo = MO;
            return this;
        }

        public void getInfo()
        {
            throw new NotImplementedException();
        }
    }

    class LogicalDrive {
        public string name;
        public ManagementObject mo;
        public LogicalDrive getFromMO(ManagementObject MO) {
            name = (string)MO.Properties["Name"].Value;
            mo = MO;
            return this;
        }

        public void getInfo() {
            Console.WriteLine("DriveName: " + mo.Properties["Name"].Value);
            Console.WriteLine("DriveId: " + mo.Properties["DeviceId"].Value);
            Console.WriteLine("DriveCompressed: " + mo.Properties["Compressed"].Value);
            Console.WriteLine("DriveType: " + mo.Properties["DriveType"].Value);
            Console.WriteLine("FileSystem: " + mo.Properties["FileSystem"].Value);
            Console.WriteLine("FreeSpace: " + mo.Properties["FreeSpace"].Value);
            Console.WriteLine("TotalSpace: " + mo.Properties["Size"].Value);
            Console.WriteLine("DriveMediaType: " + mo.Properties["MediaType"].Value);
            Console.WriteLine("VolumeName: " + mo.Properties["VolumeName"].Value);
            Console.WriteLine("VolumeSerial: " + mo.Properties["VolumeSerialNumber"].Value);
        }
    }
}
