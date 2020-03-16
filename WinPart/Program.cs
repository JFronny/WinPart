using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using WinPart.Data_structure;
using static System.Environment;

namespace WinPart
{
    internal static class Program
    {
        private static List<Device> _drives;
        private static bool _longInputSupport, _debug;

        private static void Main(string[] args)
        {
            List<string> li = new List<string>(args.Select(s =>
            {
                if (s.StartsWith("-") || s.StartsWith("/"))
                    s = s.Remove(0, 1);
                return s;
            }));
            _longInputSupport = li.Contains("li");
            _debug = li.Contains("debug");
            bool run = true;
            if (li.Contains("help") || li.Contains("?"))
            {
                Console.WriteLine(@"WinPart is a tool for manipulating storage.

WinPart [-li] [-help] [-debug]
li     Enables large inputs (over 1 char)
help   Shows this message
debug  Troubleshooting");
                run = false;
            }
            if (!run) return;
            Console.WriteLine("Use \"WinPart -help\" for CMD Args");
            Console.WriteLine("Welcome to WinPart. Choose one of the disks by their number to continue.");
            try
            {
                _drives = new List<Device>();
                int i = 0;
                foreach (ManagementObject mo in new ManagementObjectSearcher("select * from Win32_DiskDrive").Get())
                {
                    i++;
                    _drives.Add(new Device().GetFromMo(mo));
                    Console.WriteLine($"{i}: {_drives[^1].Name}");
                }
                Device drive = _drives[int.Parse(GetInput()) - 1];
                Console.WriteLine(@"Choose one of the operations by their number to continue.
1: Info
2: Open");
                switch (int.Parse(GetInput()))
                {
                    case 1:
                        drive.GetInfo();
                        break;
                    case 2:
                        Console.WriteLine(@"Processing...
Choose one of the partitions by their number to continue.");
                        for (i = 0; i < drive.Partitions.Count; i++)
                            Console.WriteLine($"{i + 1}: {drive.Partitions[i].Name}");
                        Partition partition = drive.Partitions[int.Parse(GetInput()) - 1];
                        Console.WriteLine(@"Choose one of the operations by their number to continue.
1: Info
2: Open");
                        switch (int.Parse(GetInput()))
                        {
                            case 1:
                                partition.GetInfo();
                                break;
                            case 2:
                                Console.WriteLine(@"Processing...
Choose one of the logical drives by their number to continue.");
                                for (i = 0; i < partition.LogicalDrives.Count; i++)
                                    Console.WriteLine($"{i + 1}: {partition.LogicalDrives[i].Name}");
                                LogicalDrive logicalDrive = partition.LogicalDrives[int.Parse(GetInput()) - 1];
                                Console.WriteLine(@"Choose one of the operations by their number to continue.
1: Info
2: Explorer");
                                switch (int.Parse(GetInput()))
                                {
                                    case 1:
                                        logicalDrive.GetInfo();
                                        break;
                                    case 2:
                                        Process.Start(logicalDrive.Name);
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
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            if (_debug) Console.ReadKey();
        }

        private static string GetInput()
        {
            Console.Write("\r\n==> ");
            string output = _longInputSupport ? Console.ReadLine() : Console.ReadKey().KeyChar.ToString();
            Console.Write("\r\n\r\n");
            return output;
        }

        public static void PrintInfo(ManagementBaseObject mo, string s)
        {
            string[] tmp1 = { };
            try
            {
                tmp1 = s.Split(" ".ToCharArray());
                PropertyData tmp2;
                if (tmp1[^1].EndsWith("[]"))
                {
                    tmp2 = mo.Properties[tmp1[^1].Replace("[]", "")];
                    string tmp3 = "[ ";
                    IList list = (Array) tmp2.Value;
                    for (int i = 0; i < list.Count; i++)
                    {
                        tmp3 += list[i].ToString();
                        if (i + 1 < list.Count) tmp3 += " | ";
                    }
                    tmp3 += " ]";
                    Console.WriteLine($"{tmp2.Name}: {tmp3}");
                }
                else
                {
                    tmp2 = mo.Properties[tmp1[^1]];
                    Console.WriteLine($"{tmp2.Name}: {tmp2.Value}");
                }
            }
            catch (Exception e)
            {
                Console.Write($"Could not read: {tmp1[^1]}");
                if (_debug) Console.Write($" due to: {e}");
                Console.Write(NewLine);
            }
        }
    }
}