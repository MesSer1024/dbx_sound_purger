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

        private List<int> _removedLines;
        private Dictionary<int, string> _changedLines;

        public string VisibleName
        {
            get { return FilePath ?? Name ?? Guid; }
        }

        public DiceAsset()
        {
            Parents = new List<string>();
            Children = new List<string>();
            _removedLines = new List<int>();
            _changedLines = new Dictionary<int, string>();
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

        public void removeLine(int line)
        {
            if (!_removedLines.Contains(line))
            {
                _removedLines.Add(line);
            }
        }

        internal void removeLines(int startIndex, int endIndex)
        {
            for (int i = startIndex; i <= endIndex; ++i)
            {
                removeLine(i);
            }
        }

        public void replaceLine(int line, string content)
        {
            _changedLines.Add(line, content);
        }

        public void writeChanges()
        {
            if (_removedLines.Count == 0 && _changedLines.Count == 0)
                return;

            //assert that we are not about to change and modify same line
            _removedLines.ForEach(a =>
                { if (_changedLines.ContainsKey(a)) throw new Exception(); }
            );

            var lines = File.ReadAllLines(FilePath).ToList();

            foreach (var pair in _changedLines)
            {
                lines[pair.Key] = pair.Value;
            }

            _removedLines = _removedLines.OrderByDescending(a => a).ToList();
            for (int i = 0; i < _removedLines.Count; ++i)
                lines.RemoveAt(_removedLines[i]);

            //output modified version to file
            var sb = new StringBuilder();
            lines.ForEach(a => sb.AppendLine(a));
            using (var sw = new StreamWriter(FilePath, false, Encoding.UTF8))
            {
                sw.Write(sb.ToString());
                sw.Flush();
                sw.Close();
            }

            _removedLines.Clear();
            _changedLines.Clear();
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
                        try {
                            AllAssets.Add(asset.Guid, asset);
                            AllAssets.Add(asset.FilePath, asset);
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine("Error when trying to add item to Dictionary, this should not happen, why? {2}\n\trelated to guid={0}, file={1}", asset.Guid, asset.FilePath, e.Message);
                            try { 
                                //try to overwrite what is there?
                                AllAssets.Add(asset.FilePath, asset);
                                AllAssets[asset.Guid] = asset;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }

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
                sr.Close();
            }

            return asset;
        }
    }
}
