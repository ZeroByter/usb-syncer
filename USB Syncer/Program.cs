using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace USB_Syncer
{
    class Program
    {
        private static bool cancelAutoClose = false;

        private static async void BeginAutoClose(bool didAnyWork)
        {
            var seconds = didAnyWork ? 5 : 3;

            for(var i = 0; i < seconds; i++)
            {
                if (cancelAutoClose) return;

                Console.WriteLine($"Auto-closing in {seconds - i} seconds");
                await Task.Delay(1000);
            }

            Environment.Exit(0);
        }

        private static List<FilesFolder> ParseFoldesFromSyncerFile(string driveName, string[] rawData)
        {
            List<FilesFolder> folders = new List<FilesFolder>();

            foreach(string untrimmedLine in rawData)
            {
                string line = untrimmedLine.Trim();

                string[] lineData = line.Split(new char[] { ' ' }, 2);

                if (lineData.Length <= 1) continue;

                string name = lineData[0];
                string path = lineData[1].TrimStart('\\', '/');
                path = Path.Combine(driveName, path);

                if (!Directory.Exists(path))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Creating non-existent directory - {path}");
                    Console.ForegroundColor = ConsoleColor.White;
                    Directory.CreateDirectory(path);
                }

                string[] filePaths = Directory.GetFiles(path);
                FileInfo[] files = new FileInfo[filePaths.Length];

                for (int i = 0; i < filePaths.Length; i++)
                {
                    string filePath = filePaths[i];
                    files[i] = new FileInfo(filePath);
                }

                folders.Add(new FilesFolder(name, path, files));
            }

            return folders;
        }

        private static bool ShouldConsiderMovingFile(string file1, string file2)
        {
            return !file2.EndsWith(".usbb") && Path.GetFileName(file1).ToLower() != Path.GetFileName(file2).ToLower();
        }

        private static bool ShouldMoveFile(FileInfo file1, FileInfo file2, out List<string> reasons)
        {
            reasons = new List<string>();

            if (file2 == null)
            {
                reasons.Add("target doesn't exist");
                return true;
            }

            if (file1.Directory == file2.Directory) return false;

            var differentSize = file1.Length != file2.Length;
            var newerDate = file1.LastWriteTime > file2.LastWriteTime;

            if (differentSize) reasons.Add("different size");
            if (newerDate) reasons.Add("more recent");

            return newerDate && differentSize;
        }

        private static void Main(string[] args)
        {
            var folders = new List<FilesFolder>();
            DriveInfo[] drives = DriveInfo.GetDrives();

#if DEBUG
            string targetFileName = "USBSyncer-d.txt";
#else
            string targetFileName = "USBSyncer.txt";
#endif

            foreach (var drive in drives){
                var fullPath = Path.Combine(drive.Name, targetFileName);

                if(File.Exists(fullPath))
                {
                    folders.AddRange(ParseFoldesFromSyncerFile(drive.Name, File.ReadAllLines(fullPath)));
                }
            }

            var namedFolders = new Dictionary<string, SortedFiles>();

            foreach(var folder in folders)
            {
                SortedFiles files = new SortedFiles();

                foreach (var file in folder.files)
                {
                    if(!namedFolders.TryGetValue(folder.name, out files))
                    {
                        files = new SortedFiles();
                    }

                    files.Add(file);

                    namedFolders[folder.name] = files;
                }
            }

            bool didAnyWork = false;

            foreach(var pair in namedFolders)
            {
                foreach(var directory in pair.Value.GetAll())
                {
                    foreach(var file1Path in directory.Value)
                    {
                        foreach(var otherDirectory in pair.Value.GetAllExcept(directory.Key))
                        {
                            var file2Path = Path.Combine(otherDirectory, Path.GetFileName(file1Path));

                            var file1 = pair.Value.GetFile(file1Path);
                            var file2 = pair.Value.GetFile(file2Path);

                            if (ShouldConsiderMovingFile(file1Path, file2Path) && ShouldMoveFile(file1, file2, out List<string> reasons))
                            {
                                didAnyWork = true;

                                if(File.Exists(file2Path)) File.Copy(file2Path, file2Path + ".usbb", true);
                                File.Copy(file1Path, file2Path, true);

                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("transfered...");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"  {file1Path}");
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine($" to");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"  {file2Path}");
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Write($" because: ");
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine(string.Join(", ", reasons));
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        }
                    }
                }
            }

            if (!didAnyWork)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Did not copy any files anywhere...");
                Console.ForegroundColor = ConsoleColor.White;
            }

            BeginAutoClose(didAnyWork);

            Console.ReadKey();

            Console.CursorLeft = 0;
            Console.WriteLine("Canceled auto-close. Press any key again to close.");
            cancelAutoClose = true;

            Console.ReadKey();
        }
    }
}
