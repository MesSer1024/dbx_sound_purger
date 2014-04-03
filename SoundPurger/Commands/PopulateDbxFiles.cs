using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundPurger.Commands
{
    class PopulateDbxFiles
    {
        public List<FileInfo> Files { get; set; }

        public PopulateDbxFiles(DirectoryInfo rootFolder, string searchPattern = "*.dbx")
        {
            Files = new List<FileInfo>();
            Files.AddRange(rootFolder.GetFiles(searchPattern, SearchOption.AllDirectories));
        }

    }
}
