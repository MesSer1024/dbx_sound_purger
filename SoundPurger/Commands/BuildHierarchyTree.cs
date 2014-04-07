using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SoundPurger.Commands
{
    class BuildHierarchyTree
    {
        public Action onComplete;

        public int TotalFiles { get; private set; }
        public long TotalBytes { get; private set; }
        public long BytesCompleted { get; private set; }
        public int UnfinishedFiles { get { return _unfinishedFiles; } }

        private object _writeLock = new Object();
        private DictionaryList<FileInfo> _guidsUsedInFiles;
        private ReadOnlyCollection<string> _identifiers;
        
        private DateTime _start;
        private int _unfinishedFiles = 0;
        private class ThreadState
        {
            public FileInfo File { get; set; }
            public StreamReader Content { get; set; }
            public const string STRING_FILTER = "uid=";
        }

        public BuildHierarchyTree()
        {
        }

        public void start(List<DiceAsset> assets, List<FileInfo> files)
        {
            _guidsUsedInFiles = new DictionaryList<FileInfo>();
            _start = DateTime.Now;
            TotalFiles = _unfinishedFiles = files.Count;
            TotalBytes = 0;
            files.ForEach(a => TotalBytes += a.Length);

            //add assets to dictionary based on their guid
            var foobar = new List<string>();
            foreach (var asset in assets)
            {
                foobar.Add(asset.Guid);
            }
            //make a threadsafe way to access each key
            _identifiers = foobar.AsReadOnly();

            //sort files based on filesize, biggest first
            files.Sort((a, b) =>
            {
                if (a.Length < b.Length)
                    return 1;
                if (a.Length > b.Length)
                    return -1;
                return 0;
            });

            //setup work
            int n = files.Count;
            for (int i = 0; i < n; ++i )
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(checkFileUsingAssets), new ThreadState { File = files[i] });
            }
            Console.WriteLine("foreach done in {0}s", (DateTime.Now - _start).TotalSeconds);
        }

        private void checkFileUsingAssets(Object o)
        {
            //find out which files that contains a link to a specific guid
            var GuidUsers = new DictionaryList<FileInfo>();
            var GuidLineUsed = new DictionaryList<string>();

            var file = ((ThreadState)o).File;
            findGuidsUsedInFile(file, ref GuidUsers, ref GuidLineUsed);
            lock (_writeLock)
            {
                BytesCompleted += file.Length;
                if (GuidUsers.Count > 0)
                    _guidsUsedInFiles.Add(GuidUsers);
                if (--_unfinishedFiles == 0)
                    allFileParsingDone();
            }
        }

        private void findGuidsUsedInFile(FileInfo file, ref DictionaryList<FileInfo> GuidUsers, ref DictionaryList<string> GuidLineUsed)
        {
            if (!file.Exists)
                throw new Exception("Foobared");

            int lineNumber = 0;
            string filter = ThreadState.STRING_FILTER;

            var sr = new StreamReader(file.FullName);

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                lineNumber++;

                if (line.Contains(filter))
                {
                    int n = _identifiers.Count;
                    for (int i = 0; i < n; ++i)
                    {
                        var guid = _identifiers[i];
                        if (line.Contains(guid))
                        {
                            GuidUsers.Add(guid, file);
                            GuidLineUsed.Add(guid, line);
                        }
                    }
                }
            }
        }

        private void allFileParsingDone()
        {
            Console.WriteLine("Finished parsing files in {0}s", (DateTime.Now - _start).TotalSeconds);

            WpfUtils.delayCall(() => { handleFileReferences(); });
        }

        private void handleFileReferences()
        {
            //make sure all files exist in database
            buildAssets(_guidsUsedInFiles);
            
            //get all files and update references to everything
            var allAssets = AssetBuilder.AllAssets;
            foreach (var pair in _guidsUsedInFiles)
            {
                var child = allAssets[pair.Key];

                foreach (var parentId in pair.Value)
                {
                    var parent = allAssets[parentId.FullName];
                    if (child != parent)
                    {
                        child.addParentUnique(parent);
                        parent.addChildUnique(child);
                    }
                }
            }

            saveAsJson();

            Console.WriteLine("Json files outputted and new assets built after {0}s", (DateTime.Now - _start).TotalSeconds);
            if (onComplete != null)
                WpfUtils.delayCall(() => { onComplete(); });
        }

        private void saveAsJson()
        {
            var save = new SavedData();
            save.AllAssets = AssetBuilder.AllAssets;
            save.RemovableFiles = AppSettings.FilesToRemove.ToList();

            var output = JsonConvert.SerializeObject(save);

            var file = new FileInfo(String.Format("./output/save_{0}.dat", DateTime.Now.Ticks));
            if (!file.Directory.Exists)
                file.Directory.Create();
            using (var sw = new StreamWriter(file.FullName, false))
            {
                sw.Write(output);
                sw.Flush();
                sw.Close();
            }

            using (var sw = new StreamWriter("./output/_lastsave.dat", false))
            {
                sw.Write(output);
                sw.Flush();
                sw.Close();
            }
        }

        private List<DiceAsset> buildAssets(DictionaryList<FileInfo> AssetUsedInFiles)
        {
            var val = new List<DiceAsset>(AssetUsedInFiles.Count);

            foreach (var item in AssetUsedInFiles)
            {
                foreach (var file in item.Value)
                {
                    var asset = AssetBuilder.buildAsset(file);
                    val.Add(asset);
                }
            }
            return val;
        }
    }
}
