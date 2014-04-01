using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundPurger.Messages
{
    class FilesFoundMessage : IMessage
    {
        public FileInfo[] Files { get; set; }

        public FilesFoundMessage(FileInfo[] files)
        {
            Files = files;
        }
    }
}
