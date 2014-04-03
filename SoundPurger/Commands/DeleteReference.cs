using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundPurger.Commands
{
    class DeleteReference
    {
        private DiceAsset _asset;

        public DeleteReference(DiceAsset asset)
        {
            this._asset = asset;
            var parentAssets = getAssets(asset.Parents);
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
        }

        private void deleteGuidFromAssets(string guid, List<DiceAsset> assets)
        {
            foreach(var asset in assets)
            {
                var file = new FileInfo(asset.FilePath);

                if (!file.Exists || file.IsReadOnly)
                    throw new Exception();

                var lines = File.ReadAllLines(asset.FilePath).ToList();
                var indexesToRemove = new List<int>();

                //find all lines
                for (int i = 0; i < lines.Count; ++i)
                {
                    if (lines[i].Contains(guid))
                        indexesToRemove.Add(i);
                }

                //remove all tagged lines
                for (int i = indexesToRemove.Count - 1; i >= 0; --i)
                {
                    lines.RemoveAt(indexesToRemove[i]);
                }
                var sb = new StringBuilder();
                lines.ForEach(a => sb.AppendLine(a));
                using (var sw = new StreamWriter(asset.FilePath))
                {
                    sw.Write(sb.ToString());
                    sw.Flush();
                }
            }
        }
    }
}
