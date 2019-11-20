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
using Soundboard.Settings;

namespace Soundboard.TableView
{
    /// <summary>
    /// Interaction logic for TableView.xaml
    /// </summary>
    public partial class TableView : UserControl
    {
        public static List<Sound.Files> SoundFiles { get; set; } = new List<Sound.Files>();
        private AddWindow addWindow = new AddWindow();
        private EditWindow editWindow = new EditWindow();
        public string SelectedItem
        {
            get
            {
                return GetSelectedFile();
            }
        }
        public TableView()
        {
            InitializeComponent();
        }

        private void AddEntryWindow(object sender, RoutedEventArgs e)
        {
            
            addWindow.ItemAddedEvent += ItemAddedEventHandler;
            addWindow.Show();
        }

        private void ItemAddedEventHandler(object sender, Sound.Files e)
        {
            TableEntries.Items.Add(e);
            TableEntries.UnselectAllCells();
            SoundFiles.Add(e);
        }

        private void EditEntry(object sender, RoutedEventArgs e)
        {
            if (TableEntries.SelectedIndex != -1)
            {
                editWindow.ItemEditEvent += ItemEditEventHandler;
                Sound.Files toBeEdited = (Sound.Files)TableEntries.SelectedItem;
                editWindow.SoundName.Text = toBeEdited.NameSound;
                editWindow.SoundKey.Text = toBeEdited.InputKey;
                editWindow.SoundFile.Text = toBeEdited.FileLocation;
                editWindow.Show();
            }
        }

        private void ItemEditEventHandler(object sender, Sound.Files e)
        {
            SoundFiles[SoundFiles.FindIndex(replace => replace.Equals(TableEntries.Items[TableEntries.SelectedIndex]))] = e;
            TableEntries.Items[TableEntries.SelectedIndex] = e;
            TableEntries.UnselectAllCells();
        }

        private void DeleteEntry(object sender, RoutedEventArgs e)
        {
            if (TableEntries.SelectedIndex != -1)
            {
                SoundFiles.RemoveAt(TableEntries.SelectedIndex);
                TableEntries.Items.RemoveAt(TableEntries.SelectedIndex);
            }
        }

        private string GetSelectedFile()
        {
            if (TableEntries.SelectedIndex != -1)
            {
                Sound.Files selected = (Sound.Files)TableEntries.SelectedItem;
                return selected.FileLocation;
            }
            else
            {
                return "";
            }
        }
    }
}
