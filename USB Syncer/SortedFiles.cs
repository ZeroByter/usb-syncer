using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace USB_Syncer
{
    class SortedFiles
    {
        private Dictionary<string, List<string>> files = new Dictionary<string, List<string>>();
        private Dictionary<string, FileInfo> filesData = new Dictionary<string, FileInfo>();


        public void Add(FileInfo file)
        {
            var directory = file.DirectoryName;

            var filesList = new List<string>();

            if(!files.TryGetValue(directory, out filesList))
            {
                filesList = new List<string>();
            }

            filesList.Add(file.FullName);

            files[directory] = filesList;
            filesData[file.FullName] = file;
        }

        public Dictionary<string, List<string>> GetAll()
        {
            return files;
        }

        public List<string> GetAllExcept(string directory)
        {
            var allExcept = new List<string>();

            foreach(var pair in files)
            {
                if (pair.Key == directory) continue;

                allExcept.Add(pair.Key);
            }

            return allExcept;
        }

        public FileInfo GetFile(string fullName)
        {
            if(filesData.TryGetValue(fullName, out FileInfo file))
            {
                return file;
            }

            return null;
        }
    }
}
