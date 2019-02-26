using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using CommandLine.Utility;
using WinPart.Properties;
using System.Collections;
#pragma warning disable IDE1006
namespace WinPart {
    class Program {
        List<Device> drives;
        bool longInputSupport, debug;
        static void Main(string[] args) => new Program().main(new Arguments(args));
        void main(Arguments args) {
            longInputSupport = args["li"] != null;
            debug = args["debug"] != null;
            bool run = true;
            if ((args["help"] != null) || (args["?"] != null)) {
                Console.WriteLine("WinPart is a tool for manipulating storage.\r\n\r\nWinPart [-li] [-help] [-debug]\r\n\r\nli     Enables large inputs (over 1 char)\r\nhelp   Shows this message\r\ndebug  Troubleshooting");
                run = false;
            }
            if (run)
                mainWindow();
        }

        void mainWindow() {
            Console.WriteLine("Use \"WinPart -help\" for CMD Args");
            Console.WriteLine("Welcome to WinPart. Choose one of the disks by their number to continue.");
            try {
                drives = new List<Device>() { };
                int i = 0;
                foreach (ManagementObject mo in new ManagementObjectSearcher("select * from Win32_DiskDrive").Get()) {
                    i++;
                    drives.Add(new Device().getFromMO(mo, this));
                    Console.WriteLine(i.ToString() + ": " + drives[drives.Count - 1].name);
                }
                Device drive = drives[int.Parse(getInput()) - 1];
                Console.WriteLine("Choose one of the operations by their number to continue.\r\n1: Info\r\n2: Open");
                switch (int.Parse(getInput())) {
                    case 1:
                        drive.getInfo();
                        break;
                    case 2:
                        Console.WriteLine("Processing...\r\nChoose one of the partitions by their number to continue.");
                        for (i = 0; i < drive.partitions.Count; i++)
                            Console.WriteLine((i + 1).ToString() + ": " + drive.partitions[i].name);
                        Partition partition = drive.partitions[int.Parse(getInput()) - 1];
                        Console.WriteLine("Choose one of the operations by their number to continue.\r\n1: Info\r\n2: Open");
                        switch (int.Parse(getInput())) {
                            case 1:
                                partition.getInfo();
                                break;
                            case 2:
                                Console.WriteLine("Processing...\r\nChoose one of the logical drives by their number to continue.");
                                for (i = 0; i < partition.logicalDrives.Count; i++)
                                    Console.WriteLine((i + 1).ToString() + ": " + partition.logicalDrives[i].name);
                                LogicalDrive logicalDrive = partition.logicalDrives[int.Parse(getInput()) - 1];
                                Console.WriteLine("Choose one of the operations by their number to continue.\r\n1: Info\r\n2: Explorer");
                                switch (int.Parse(getInput())) {
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
            if (debug) Console.ReadKey();
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

        public void printInfo(ManagementObject mo, string s) {
            string[] tmp1 = { };
            PropertyData tmp2;
            try {
                tmp1 = s.Split(" ".ToCharArray());
                if (tmp1[tmp1.Length - 1].EndsWith("[]")) {
                    tmp2 = mo.Properties[tmp1[tmp1.Length - 1].Replace("[]", "")];
                    string tmp3 = "[ ";
                    object[] list = (object[])tmp2.Value;
                    for (int i = 0; i < list.Length; i++) {
                        tmp3 += list[i].ToString();
                        if (i + 1 < list.Length) tmp3 += " | ";
                    }
                    tmp3 += " ]";
                    Console.WriteLine(tmp2.Name + ": " + tmp3);
                } else {
                    tmp2 = mo.Properties[tmp1[tmp1.Length - 1]];
                    Console.WriteLine(tmp2.Name + ": " + tmp2.Value);
                }
            } catch (Exception e) { Console.Write("Could not read: " + tmp1[tmp1.Length - 1]); if (debug) Console.Write(" due to: " + e.ToString()); Console.Write("\r\n"); }
        }
    }

    class Device {
        public Program program;
        public List<Partition> partitions;
        public string name;
        public ManagementObject mo;
        public Device getFromMO(ManagementObject MO, Program program_) {
            mo = MO;
            program = program_;
            partitions = new List<Partition>() { };
            foreach (ManagementObject m in new ManagementObjectSearcher(string.Format("associators of {{{0}}} where AssocClass = Win32_DiskDriveToDiskPartition", MO.Path.RelativePath)).Get())
                partitions.Add(new Partition().getFromMO(m, program));
            name = (string)MO.Properties["Model"].Value;
            return this;
        }

        public void getInfo() {
            foreach (string s in Resources.Device.Split(new[] { "\r\n" }, StringSplitOptions.None))
                program.printInfo(mo, s);
        }
    }

    class Partition {
        public Program program;
        public List<LogicalDrive> logicalDrives;
        public string name;
        public ManagementObject mo;
        public Partition getFromMO(ManagementObject MO, Program program_) {
            mo = MO;
            program = program_;
            logicalDrives = new List<LogicalDrive>() { };
            foreach (ManagementObject m in new ManagementObjectSearcher(string.Format("associators of {{{0}}} where AssocClass = Win32_LogicalDiskToPartition", MO.Path.RelativePath)).Get())
                logicalDrives.Add(new LogicalDrive().getFromMO(m, program));
            name = MO.ToString();
            return this;
        }

        public void getInfo() {
            foreach (string s in Resources.Partition.Split(new[] { "\r\n" }, StringSplitOptions.None))
                program.printInfo(mo, s);
        }
    }

    class LogicalDrive {
        public Program program;
        public string name;
        public ManagementObject mo;
        public LogicalDrive getFromMO(ManagementObject MO, Program program_) {
            name = (string)MO.Properties["Name"].Value;
            mo = MO;
            program = program_;
            return this;
        }

        public void getInfo() {
            foreach (string s in Resources.Logical.Split(new[] { "\r\n" }, StringSplitOptions.None))
                program.printInfo(mo, s);
        }
    }
}
