using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            DriveInfo[] drives = DriveInfo.GetDrives();

            foreach (var drive in drives)
            {
                var fullPath = Path.Combine(drive.Name, "USBSyncer.exe");

                if (File.Exists(fullPath))
                {
                    Process.Start(fullPath);
                }
            }
        }
    }
}
