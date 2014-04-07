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

            using (var sr = new StreamReader(file.FullName))
            {
                string s = sr.ReadToEnd();
                var load = JsonConvert.DeserializeObject<SavedData>(s);
                AppSettings.FilesToRemove = load.RemovableFiles.ToArray();
                AssetBuilder.AllAssets = load.AllAssets;
                sr.Close();
            }

            MessageManager.sendMessage(new FilesFoundMessage());
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
