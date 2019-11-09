using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Collections;
using System.Linq;
#pragma warning disable IDE1006
namespace WinPart
{
    class Program
    {
        static List<Device> drives;
        static bool longInputSupport, debug;
        static void Main(string[] args)
        {
            List<string> li = new List<string>(args.Select(s => {
                if (s.StartsWith("-") || s.StartsWith("/"))
                    s = s.Remove(0, 1);
                return s;
            }));
            longInputSupport = li.Contains("li");
            debug = li.Contains("debug");
            bool run = true;
            if (li.Contains("help") || li.Contains("?"))
            {
                Console.WriteLine("WinPart is a tool for manipulating storage.\r\n\r\nWinPart [-li] [-help] [-debug]\r\n\r\nli     Enables large inputs (over 1 char)\r\nhelp   Shows this message\r\ndebug  Troubleshooting");
                run = false;
            }
            if (run)
                mainWindow();
        }

        static void mainWindow()
        {
            Console.WriteLine("Use \"WinPart -help\" for CMD Args");
            Console.WriteLine("Welcome to WinPart. Choose one of the disks by their number to continue.");
            try
            {
                drives = new List<Device>() { };
                int i = 0;
                foreach (ManagementObject mo in new ManagementObjectSearcher("select * from Win32_DiskDrive").Get())
                {
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
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
            if (debug) Console.ReadKey();
        }

        static string getInput()
        {
            Console.Write("\r\n==> ");
            var out_ = "";
            if (longInputSupport)
                out_ = Console.ReadLine();
            else
                out_ = Console.ReadKey().KeyChar.ToString();
            Console.Write("\r\n\r\n");
            return out_;
        }

        public static  void printInfo(ManagementObject mo, string s)
        {
            string[] tmp1 = { };
            PropertyData tmp2;
            try
            {
                tmp1 = s.Split(" ".ToCharArray());
                if (tmp1[tmp1.Length - 1].EndsWith("[]"))
                {
                    tmp2 = mo.Properties[tmp1[tmp1.Length - 1].Replace("[]", "")];
                    string tmp3 = "[ ";
                    IList list = (Array)tmp2.Value;
                    for (int i = 0; i < list.Count; i++)
                    {
                        tmp3 += list[i].ToString();
                        if (i + 1 < list.Count) tmp3 += " | ";
                    }
                    tmp3 += " ]";
                    Console.WriteLine(tmp2.Name + ": " + tmp3);
                }
                else
                {
                    tmp2 = mo.Properties[tmp1[tmp1.Length - 1]];
                    Console.WriteLine(tmp2.Name + ": " + tmp2.Value);
                }
            }
            catch (Exception e) { Console.Write("Could not read: " + tmp1[tmp1.Length - 1]); if (debug) Console.Write(" due to: " + e.ToString()); Console.Write("\r\n"); }
        }
    }
}
