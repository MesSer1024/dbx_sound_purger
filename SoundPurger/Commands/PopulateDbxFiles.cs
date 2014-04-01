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

        private DirectoryInfo[] FoldersToRemove = new DirectoryInfo[] {
            new DirectoryInfo(Path.Combine(AppSettings.Folder, "BulletCraft")),
            new DirectoryInfo(Path.Combine(AppSettings.Folder, "Bullet_Craft")),
            new DirectoryInfo(Path.Combine(AppSettings.Folder, "CamShakes")),
            new DirectoryInfo(Path.Combine(AppSettings.Folder, "Character")),
            new DirectoryInfo(Path.Combine(AppSettings.Folder, "Destruction")),
            new DirectoryInfo(Path.Combine(AppSettings.Folder, "Explosions")),
            new DirectoryInfo(Path.Combine(AppSettings.Folder, "GameSounds")),
            new DirectoryInfo(Path.Combine(AppSettings.Folder, "Levels")),
            new DirectoryInfo(Path.Combine(AppSettings.Folder, "maoism")),
            new DirectoryInfo(Path.Combine(AppSettings.Folder, "Mixers")),
            new DirectoryInfo(Path.Combine(AppSettings.Folder, "Music")),
            new DirectoryInfo(Path.Combine(AppSettings.Folder, "Objects")),
            new DirectoryInfo(Path.Combine(AppSettings.Folder, "States")),
            new DirectoryInfo(Path.Combine(AppSettings.Folder, "StreamPools")),
            new DirectoryInfo(Path.Combine(AppSettings.Folder, "UI")),
            new DirectoryInfo(Path.Combine(AppSettings.Folder, "Vehicles")),
            new DirectoryInfo(Path.Combine(AppSettings.Folder, "VO_Radio_Effects_Waves")),
            new DirectoryInfo(Path.Combine(AppSettings.Folder, "Weapons")),

        };

        public PopulateDbxFiles()
        {
            Files = new List<FileInfo>();

            foreach (var dir in FoldersToRemove)
            {
                if (!dir.Exists)
                    throw new Exception("Unable to find folder...");

                Files.AddRange(dir.GetFiles("*.dbx", SearchOption.AllDirectories));
            }
        }

    }
}
