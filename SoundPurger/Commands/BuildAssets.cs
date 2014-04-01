using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml;

namespace SoundPurger.Commands
{
    class DiceAsset
    {
        public string Guid { get; set; }
        public string PrimaryInstance { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }

    class BuildAssets
    {
        public List<DiceAsset> Assets { get; set; }

        public BuildAssets()
        {
            Assets = new List<DiceAsset>(AppSettings.Files.Length);
            StringBuilder sb = new StringBuilder();

            foreach (var file in AppSettings.Files)
            {
                if (!file.Exists)
                    throw new Exception();

                int lineNr = 0;
                bool success = false;
                sb.Clear();

                using (var sr = new StreamReader(file.FullName))
                {
                    while(lineNr++ < 10)
                    {
                        var line = sr.ReadLine().Trim();
                        sb.AppendLine(line);
                        if (line.Contains("<partition"))
                        {
                            //create asset
                            var asset = new DiceAsset();
                            Assets.Add(asset);

                            //read entire xml
                            var stringReader = new StringReader(sb.ToString() + sr.ReadToEnd());
                            var xml = XmlReader.Create(stringReader);
                            xml.ReadToFollowing("partition");

                            //read xml
                            asset.Guid = xml.GetAttribute("guid");
                            asset.PrimaryInstance = xml.GetAttribute("primaryInstance");

                            //verify data
                            if (string.IsNullOrEmpty(asset.Guid) || string.IsNullOrEmpty(asset.PrimaryInstance))
                                throw new Exception("foobard");

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

                                if(string.IsNullOrEmpty(asset.Type))
                                    throw new Exception("foobard");
                            }

                            //stop iterating on this file
                            break;
                        }
                    }

                    if (lineNr >= 9)
                        throw new Exception("Foobared!");
                }
            }
        }
    }
}
