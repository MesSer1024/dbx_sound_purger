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
using SoundPurger.Messages;
using System.IO;
using Newtonsoft.Json;

namespace SoundPurger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMessageListener
    {
        public MainWindow()
        {
            InitializeComponent();
            MessageManager.addListener(this);
            AppSettings.RootFolder = @"E:\repositories\Tunguska\Data";
            WpfUtils.MainDispatcher = this.Dispatcher;
            _content.Children.Add(new Populate());
        }

        #region IMessageListener Members

        public void onMessage(IMessage msg)
        {
            if (msg is FilesFoundMessage)
            {
                var foo = msg as FilesFoundMessage;
                var ui = new HierarchyPresenter();
                _content.Children.Clear();
                _content.Children.Add(ui);

                ui.populate(AppSettings.FilesToRemove);
            }
        }

        #endregion
    }
}
