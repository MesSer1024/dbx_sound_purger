using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundPurger
{
    class SavedData
    {
        public List<FileInfo> RemovableFiles { get; set; }
        public Dictionary<string, DiceAsset> AllAssets { get; set; }
    }
}
