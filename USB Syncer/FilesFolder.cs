using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace USB_Syncer
{
    class FilesFolder
    {
        public string name;
        public string path;
        public FileInfo[] files;

        public FilesFolder(string name, string path, FileInfo[] files)
        {
            this.name = name;
            this.path = path;
            this.files = files;
        }
    }
}
