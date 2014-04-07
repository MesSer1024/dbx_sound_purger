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

namespace SoundPurger
{
    /// <summary>
    /// Interaction logic for HierarchyPresenter.xaml
    /// </summary>
    public partial class HierarchyPresenter : UserControl
    {
        private List<DiceAsset> _removableFiles;
        private List<DiceAsset> _subset;

        public HierarchyPresenter()
        {
            InitializeComponent();
            _removableFiles = new List<DiceAsset>();
            _items.SelectionChanged += _items_SelectionChanged;
            _items.KeyDown += _items_KeyDown;
            _items.SelectionMode = SelectionMode.Extended;
        }

        void _items_KeyDown(object sender, KeyEventArgs e)
        {
            if(_items.SelectedItem != null)
            {
                if (e.IsToggled && e.IsDown && e.Key == Key.Delete)
                {
                    foreach (var item in _items.SelectedItems)
                    {
                        if (item is DiceAsset)
                        {
                            var asset = item as DiceAsset;
                            var del = new DeleteReference(asset);
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }

                    AppSettings.writeLogToFileAndClear(new System.IO.FileInfo("./output/foobar.txt"));
                }

            }
        }



        internal void populate(System.IO.FileInfo[] removableFiles)
        {
            _removableFiles.Clear();

            foreach (var file in removableFiles)
            {
                _removableFiles.Add(AssetBuilder.AllAssets[file.FullName]);
            }

            //identifiersList.ItemsSource = _items.FindAll(a => a.Guid.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
            //_items.ItemsSource = _removableFiles;
            _subset = _removableFiles.FindAll(a => a.Type == "Audio.SoundPatchConfigurationAsset" || a.Type == "Audio.SoundPatchAsset" || a.Type == "Audio.SoundWaveAsset");
            _items.ItemsSource = _subset;
        }

        void updateSelectedData()
        {
            bool valid = _items.SelectedItem != null;

            if(valid)
            {
                var children = new List<DiceAsset>();
                var parents = new List<DiceAsset>();
                _subset[_items.SelectedIndex].Children.ForEach((a) => children.Add(AssetBuilder.AllAssets[a]));
                _subset[_items.SelectedIndex].Parents.ForEach((a) => parents.Add(AssetBuilder.AllAssets[a]));

                _children.ItemsSource = children;
                _parents.ItemsSource = parents;
            }
            else
            {
                _children.ItemsSource = null;
                _parents.ItemsSource = null;
            }
        }

        void _items_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            updateSelectedData();
        }

        //private void onFilterChanged(object sender, RoutedEventArgs e)
        //{
        //    if (_items == null)
        //    {
        //        return;
        //    }
        //    var filter = idFilter.Text;
        //    if (filter != "")
        //    {
        //        int foo = 0;
        //        if (int.TryParse(filter, out foo))
        //        {
        //            identifiersList.ItemsSource = _items.FindAll(a => a.Guid.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
        //        }
        //        else
        //        {
        //            identifiersList.ItemsSource = _items.FindAll(a => a.VisibleName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
        //        }
        //    }
        //    else
        //    {
        //        identifiersList.ItemsSource = _items;
        //    }
        //    identifiersList.Items.Refresh();
        //}
    }
}
