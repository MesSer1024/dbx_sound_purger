using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundPurger.Commands
{
    class PopulateSoundDbxFiles
    {
        public List<FileInfo> Files { get; set; }

        public PopulateSoundDbxFiles()
        {
            List<FileInfo> files = new List<FileInfo>();

            getFilesInSourceSoundFolder(ref files);
            getFilesInTestFolder(ref files);
            getFilesInXpFolders(ref files);
            Files = files;
        }

        private void getFilesInTestFolder(ref List<FileInfo> files)
        {
            var subFolders = new DirectoryInfo[] {
                new DirectoryInfo(Path.Combine(AppSettings.RootFolder, @"./Source/Test/xp0/sound")),
                new DirectoryInfo(Path.Combine(AppSettings.RootFolder, @"./Source/Test/xp1/sound")),
                new DirectoryInfo(Path.Combine(AppSettings.RootFolder, @"./Source/Test/xp2/sound")),
                new DirectoryInfo(Path.Combine(AppSettings.RootFolder, @"./Source/Test/xp3/sound")),
                new DirectoryInfo(Path.Combine(AppSettings.RootFolder, @"./Source/Test/xp4/sound")),
            };

            foreach (var folder in subFolders)
            {
                if (folder.Exists)
                {
                    files.AddRange(folder.GetFiles("*.dbx", SearchOption.AllDirectories));
                }
            }
        }

        private void getFilesInXpFolders(ref List<FileInfo> files)
        {
            var subFolders = new DirectoryInfo[] {
            //    new DirectoryInfo(Path.Combine(AppSettings.RootFolder, @"./Source/xp0/sound")),
            //    new DirectoryInfo(Path.Combine(AppSettings.RootFolder, @"./Source/xp1/sound")),
            //    new DirectoryInfo(Path.Combine(AppSettings.RootFolder, @"./Source/xp2/sound")),
            //    new DirectoryInfo(Path.Combine(AppSettings.RootFolder, @"./Source/xp3/sound")),
            //    new DirectoryInfo(Path.Combine(AppSettings.RootFolder, @"./Source/xp4/sound")),
            };

            foreach (var folder in subFolders)
            {
                if (folder.Exists)
                {
                    files.AddRange(folder.GetFiles("*.dbx", SearchOption.AllDirectories));
                }
            }
        }

        private void getFilesInSourceSoundFolder(ref List<FileInfo> files)
        {
            var soundFolder = Path.Combine(AppSettings.RootFolder, @"./Source/Sound");

            var dir = new DirectoryInfo(soundFolder);
            var subFolders = dir.GetDirectories("*", SearchOption.TopDirectoryOnly);
            var foldersToKeep = new DirectoryInfo[] {
                new DirectoryInfo(Path.Combine(soundFolder, "Core")),
                new DirectoryInfo(Path.Combine(soundFolder, "StreamPools")),
                new DirectoryInfo(Path.Combine(soundFolder, "VO")),
                new DirectoryInfo(Path.Combine(soundFolder, "World")),
            };

            foreach (var folder in subFolders)
            {
                if (!folder.Exists)
                    throw new Exception("Unable to find folder...");

                bool valid = true;
                foreach (var keeper in foldersToKeep)
                {
                    if (keeper.FullName == folder.FullName)
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                    files.AddRange(folder.GetFiles("*.dbx", SearchOption.AllDirectories));
            }
        }
    }
}
