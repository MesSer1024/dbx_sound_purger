using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SoundPurger.Commands
{
    class DeleteReference
    {
        private DiceAsset _asset;

        private string Replacement_SoundPatchConfigurationAsset = "<field name=\"Sound\" ref=\"sound/nosound_dummypatch/ff792238-fa86-a510-4fda-7418acc7cd0a\" partitionGuid=\"97aeebf4-be33-11e3-b671-a60d21d3217d\" />";

        public DeleteReference(DiceAsset asset)
        {
            this._asset = asset;
            var parentAssets = getAssets(asset.Parents);
            checkoutFile(asset.FilePath);
            checkoutPerforceFiles(parentAssets);
            deleteGuidFromAssets(asset.Guid, parentAssets);
        }

        private List<DiceAsset> getAssets(List<string> list)
        {
            var foo = new List<DiceAsset>();
            list.ForEach(a => foo.Add(AssetBuilder.AllAssets[a]));
            return foo;
        }

        private void checkoutPerforceFiles(List<DiceAsset> assets)
        {
            foreach (var asset in assets)
            {
                checkoutFile(asset.FilePath);
            }
        }

        private void checkoutFile(string p)
        {
            var file = new FileInfo(p);
            if (!file.Exists)
                throw new Exception();

            Console.WriteLine("Attempting to checkout {0}", file.FullName);

            //basically, what we do is to remove write protection and then later do a p4 reconcile manually...
            file.IsReadOnly = false;
            AppSettings.addModifiedFile(file);
        }

        /// <summary>
        /// Not thread safe... need heavy refactoring....
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="assets"></param>
        private void deleteGuidFromAssets(string guid, List<DiceAsset> assets)
        {
            var originalAsset = AssetBuilder.AllAssets[guid];
            var isSoundPatchConfigurationAsset = originalAsset.Type == "Audio.SoundPatchConfigurationAsset";



            foreach(var asset in assets)
            {
                var file = new FileInfo(asset.FilePath);

                if (!file.Exists || file.IsReadOnly)
                    throw new Exception();

                var lines = File.ReadAllLines(asset.FilePath).ToList().AsReadOnly();

                switch(asset.Type)
                {
                    case "Entity.EffectBlueprint":
                        modifyEffectBlueprint(ref lines, guid, asset);
                        break;
                    default:
                        if (isSoundPatchConfigurationAsset)
                            modifySPC_asset(ref lines, guid, asset);
                        else
                            modifyAsset(ref lines, guid, asset);
                        break;
                }
            }
        }

        private void modifySPC_asset(ref ReadOnlyCollection<string> lines, string guid, DiceAsset asset)
        {
            var indexesToRemove = new List<int>();

            //find all lines                
            for (int i = 0; i < lines.Count; ++i)
            {
                var line = lines[i];
                if (line.Contains(guid))
                {
                    string str = "";
                    bool parentIsSpecialCaseInstance = false;
                    int j = -1;
                    for (j = i - 1; j >= 0; --j)
                    {
                        str = lines[j].ToLower();
                        if (str.Contains("<instance"))
                        {
                            parentIsSpecialCaseInstance = str.Contains("audio.soundentitydata");
                            break;
                        }
                    }

                    if (parentIsSpecialCaseInstance)
                    {
                        int firstNonWhitespaceIndex = line.IndexOf<char>(c => !char.IsWhiteSpace(c));
                        asset.replaceLine(i, line.Substring(0, firstNonWhitespaceIndex) + Replacement_SoundPatchConfigurationAsset);
                    }
                    else
                        asset.removeLine(i);
                }
            }
        }

        private void modifyAsset(ref ReadOnlyCollection<string> lines, string guid, DiceAsset asset)
        {
            throw new NotImplementedException();
        }

        private void modifyEffectBlueprint(ref ReadOnlyCollection<string> lines, string guid, DiceAsset asset)
        {
            var linesToRemove = new List<int>();
            
            //1: Locate the line containing the guid
            for (int i = 0; i < lines.Count; ++i)
            {
                var line = lines[i];
                if (line.Contains(guid))
                {
                    linesToRemove.Add(i);
                }
            }

            List<string> extraRemovableGuids = new List<string>();
            //2: Wherever the guid exists, locate if it only element inside an <instance type=Audio.SoundEffectEntityData> ... </instance>
            {
                for (int i = linesToRemove.Count - 1; i >= 0; --i )
                {
                    int nxtCounter, prvCounter, idx;
                    nxtCounter = prvCounter = idx = linesToRemove[i];
                    string prevLine, nextLine;

                    //verify that our assumptions are correct in regards to item
                    while (true)
                    {
                        nextLine = lines[++nxtCounter];
                        if(nextLine.Contains("/instance>"))
                            break;
                    }

                    while (true)
                    {
                        prevLine = lines[--prvCounter];
                        if(prevLine.Contains("Audio.SoundEffectEntityData"))
                            break;
                    }


                    var regex = new Regex("guid=\"([0-9a-f-]+)\"", RegexOptions.Singleline);
                    var match = regex.Match(prevLine);
                    if (!match.Success)
                        throw new Exception();
                    var foo = match.Groups[1];

                    AppSettings.writeModification(String.Format("Removed line {0} from file {1} due to asset[guid:{2}, name:{3}", lines[idx], asset.FilePath, asset.Guid, asset.VisibleName));
                    asset.removeLines(prvCounter, nxtCounter);

                    AppSettings.writeModification(String.Format("Also tagging following guid={0} as removable due to it being only child...", foo.Value));
                    extraRemovableGuids.Add(foo.Value);
                }
            }

            //3: remove any parent of previously removed item...
            for (int i = lines.Count -1; i >= 0; --i)
            {
                var line = lines[i];
                for (int j=0; j < extraRemovableGuids.Count; ++j)
                {
                    var extraGuid = extraRemovableGuids[j];
                    if (line.Contains(extraGuid))
                    {
                        asset.removeLine(i);
                        break;
                    }
                }
            }


        }
    }
}
