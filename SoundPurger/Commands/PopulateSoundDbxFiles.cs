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

        private string _soundFolder = null;

        private DirectoryInfo[] SoundFoldersToRemove;

        public PopulateSoundDbxFiles()
        {
            Files = new List<FileInfo>();

            _soundFolder = Path.Combine(AppSettings.RootFolder, @"./Source/Sound");

            //folders to keep
                //Core
                //StreamPools
                //Test

            SoundFoldersToRemove = new DirectoryInfo[] {
                        new DirectoryInfo(Path.Combine(_soundFolder, "BulletCraft")),
                        new DirectoryInfo(Path.Combine(_soundFolder, "Bullet_Craft")),
                        new DirectoryInfo(Path.Combine(_soundFolder, "CamShakes")),
                        new DirectoryInfo(Path.Combine(_soundFolder, "Character")),
                        new DirectoryInfo(Path.Combine(_soundFolder, "Destruction")),
                        new DirectoryInfo(Path.Combine(_soundFolder, "Explosions")),
                        new DirectoryInfo(Path.Combine(_soundFolder, "GameSounds")),
                        new DirectoryInfo(Path.Combine(_soundFolder, "Levels")),
                        new DirectoryInfo(Path.Combine(_soundFolder, "maoism")),
                        new DirectoryInfo(Path.Combine(_soundFolder, "Mixers")),
                        new DirectoryInfo(Path.Combine(_soundFolder, "Music")),
                        new DirectoryInfo(Path.Combine(_soundFolder, "Objects")),
                        new DirectoryInfo(Path.Combine(_soundFolder, "States")),
                        new DirectoryInfo(Path.Combine(_soundFolder, "UI")),
                        new DirectoryInfo(Path.Combine(_soundFolder, "Vehicles")),
                        new DirectoryInfo(Path.Combine(_soundFolder, "VO_Radio_Effects_Waves")),
                        new DirectoryInfo(Path.Combine(_soundFolder, "Weapons")),
                    };

            foreach (var dir in SoundFoldersToRemove)
            {
                if (!dir.Exists)
                    throw new Exception("Unable to find folder...");

                Files.AddRange(dir.GetFiles("*.dbx", SearchOption.AllDirectories));
            }
        }
    }
}
