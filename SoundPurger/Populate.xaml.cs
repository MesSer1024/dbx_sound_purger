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

namespace SoundPurger
{
    /// <summary>
    /// Interaction logic for Populate.xaml
    /// </summary>
    public partial class Populate : UserControl
    {
        public Populate()
        {
            InitializeComponent();
            _folder.Text = AppSettings.Folder;
            _folder.IsReadOnly = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var cmd = new PopulateDbxFiles();
            AppSettings.Files = cmd.Files.ToArray();

            if (AppSettings.Files.Length > 0)
            {
                var assets = new BuildAssets();
                

                //MessageManager.sendMessage(new FilesFoundMessage(AppSettings.Files));
            }
            else
            {
                throw new Exception("Unable to find any files");
            }
        }
    }
}
