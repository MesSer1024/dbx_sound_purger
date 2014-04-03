using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml;

namespace SoundPurger
{
    class DiceAsset
    {
        public string FilePath { get; set; }
        public string Guid { get; set; }
        public string PrimaryInstance { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public List<string> Parents { get; set; }
        public List<string> Children { get; set; }

        public string VisibleName
        {
            get { return FilePath ?? Name ?? Guid; }
        }

        public DiceAsset()
        {
            Parents = new List<string>();
            Children = new List<string>();
        }

        public bool addChildUnique(DiceAsset asset)
        {
            if (Children.Contains(asset.Guid))
                return false;
            Children.Add(asset.Guid);
            return true;
        }

        public bool addParentUnique(DiceAsset asset)
        {
            if (Parents.Contains(asset.Guid))
                return false;
            Parents.Add(asset.Guid);
            return true;
        }
    }

    class AssetBuilder
    {
        public List<DiceAsset> Assets { get; set; }
        public static Dictionary<string, DiceAsset> AllAssets { get; set; }

        public AssetBuilder(FileInfo[] files)
        {
            Assets = new List<DiceAsset>(files.Length);
            AllAssets = new Dictionary<string, DiceAsset>();
            StringBuilder sb = new StringBuilder();

            foreach (var file in files)
            {
                Assets.Add(buildAsset(file));
            }
        }

        public static DiceAsset buildAsset(FileInfo file)
        {
            if (!file.Exists)
                throw new Exception();

            if (AllAssets.ContainsKey(file.FullName))
            {
                return AllAssets[file.FullName];
            }

            int lineNr = 0;
            StringBuilder sb = new StringBuilder();

            //create asset
            DiceAsset asset = null;

            using (var sr = new StreamReader(file.FullName))
            {
                while (lineNr++ < 10)
                {
                    var line = sr.ReadLine().Trim();
                    sb.AppendLine(line);
                    if (line.Contains("<partition"))
                    {
                        //create asset
                        asset = new DiceAsset();

                        //read entire xml
                        var stringReader = new StringReader(sb.ToString() + sr.ReadToEnd());
                        var xml = XmlReader.Create(stringReader);
                        xml.ReadToFollowing("partition");

                        //read xml
                        asset.Guid = xml.GetAttribute("guid");
                        asset.PrimaryInstance = xml.GetAttribute("primaryInstance");
                        asset.FilePath = file.FullName;

                        //verify data
                        if (string.IsNullOrEmpty(asset.Guid) || string.IsNullOrEmpty(asset.PrimaryInstance))
                            throw new Exception("foobard");

                        //add item to cache
                        AllAssets.Add(asset.Guid, asset);
                        AllAssets.Add(asset.FilePath, asset);

                        //setup xml to "primary instance"
                        while (xml.ReadToFollowing("instance"))
                        {
                            var instanceGuid = xml.GetAttribute("guid");
                            if (instanceGuid != asset.PrimaryInstance)
                                continue;

                            asset.Name = xml.GetAttribute("id");
                            asset.Type = xml.GetAttribute("type");
                            break;
                        }

                        if (string.IsNullOrEmpty(asset.Name) || string.IsNullOrEmpty(asset.Type))
                        {
                            Console.WriteLine("Error in file: {0}, parsing primaryInstance name={1}, type={2}", file.FullName, asset.Name, asset.Type);

                            if (string.IsNullOrEmpty(asset.Type))
                                throw new Exception("foobard");
                        }

                        //stop iterating on this file
                        break;
                    }
                }

                if (lineNr >= 9)
                    throw new Exception("Foobared!");
            }

            return asset;
        }
    }
}
