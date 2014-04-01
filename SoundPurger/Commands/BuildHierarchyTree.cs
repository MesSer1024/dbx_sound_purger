using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundPurger.Commands
{
    class BuildHierarchyTree
    {
        public Dictionary<string, DiceAsset> _hierarchyTree;

        public BuildHierarchyTree(List<DiceAsset> assets, FileInfo[] files)
        {
            _hierarchyTree = new Dictionary<string, DiceAsset>(assets.Count);

            //add them to dictionary based on their guid
            foreach (var asset in assets)
            {
                _hierarchyTree.Add(asset.Guid, asset);
            }

            //link them
            foreach (var asset in assets)
            {
                
            }
        }
    }
}
