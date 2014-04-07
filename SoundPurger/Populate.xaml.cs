using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SoundPurger.Commands;
using System.IO;
using SoundPurger.Messages;
using Newtonsoft.Json;

namespace SoundPurger
{
    /// <summary>
    /// Interaction logic for Populate.xaml
    /// </summary>
    public partial class Populate : UserControl
    {
        private System.Windows.Threading.DispatcherTimer _timer;
        private BuildHierarchyTree _tree;
        private DateTime _start;

        public Populate()
        {
            InitializeComponent();
            _folder.Text = AppSettings.RootFolder;
            _folder.IsReadOnly = true;
            _button.IsEnabled = true;
        }

        private void onLoadFile(object sender, RoutedEventArgs e)
        {
            var file = new FileInfo("./output/_lastsave.dat");
            if (!file.Exists)
                throw new Exception("Invalid file!");

            var load = JsonConvert.DeserializeObject<SavedData>(File.ReadAllText(file.FullName));
            AppSettings.FilesToRemove = load.RemovableFiles.ToArray();
            //all files loaded have TWO instances of each DiceAsset
            var dic = new Dictionary<string, DiceAsset>();
            foreach (var pair in load.AllAssets)
            {
                bool containsGuid = dic.ContainsKey(pair.Value.Guid);
                bool containsPath = dic.ContainsKey(pair.Value.FilePath);

                if (containsPath != containsGuid)
                {
                    throw new Exception();
                }
                if (containsGuid == false && containsPath == false)
                {
                    dic.Add(pair.Value.Guid, pair.Value);
                    dic.Add(pair.Value.FilePath, pair.Value);
                }
            }

            AssetBuilder.AllAssets = dic;
            MessageManager.sendMessage(new FilesFoundMessage());
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var cmd = new PopulateSoundDbxFiles();
            AppSettings.FilesToRemove = cmd.Files.ToArray();

            if (AppSettings.FilesToRemove.Length > 0)
            {
                var assets = new AssetBuilder(AppSettings.FilesToRemove);

                _button.IsEnabled = false;

                _timer = new System.Windows.Threading.DispatcherTimer();
                _timer.Tick += new EventHandler(dispatcherTimer_Tick);
                _timer.Interval = new TimeSpan(0, 0, 1);
                _timer.Start();

                _start = DateTime.Now;
                
                _tree = new BuildHierarchyTree();
                _tree.onComplete = () => { WpfUtils.toMainThread(onAllAssetsDone); };
                WpfUtils.createBgThread(() => { _tree.start(assets.Assets, new PopulateDbxFiles(new DirectoryInfo(AppSettings.RootFolder)).Files); });
            }
            else
            {
                throw new Exception("Unable to find any files");
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.AppendLine(String.Format("Finding references {0}s", (int)(DateTime.Now - _start).TotalSeconds));
            sb.AppendLine(String.Format("Data Searched: {0}mb/{1}mb", _tree.BytesCompleted / 1000000, _tree.TotalBytes / 1000000));
            _infoField.Content = sb.ToString();
        }

        private void onAllAssetsDone()
        {
            _timer.Stop();
            _timer.Tick -= dispatcherTimer_Tick;
            _button.IsEnabled = true;
            _infoField.Content = String.Format("Finding references {2}s \nFile status: {0} / {1}", _tree.TotalFiles - _tree.UnfinishedFiles, _tree.TotalFiles, (int)(DateTime.Now - _start).TotalSeconds);
            MessageManager.sendMessage(new FilesFoundMessage());
        }
    }
}
