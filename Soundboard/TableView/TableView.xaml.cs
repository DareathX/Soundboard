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

namespace Soundboard.TableView
{
    /// <summary>
    /// Interaction logic for TableView.xaml
    /// </summary>
    public partial class TableView : UserControl
    {
        private List<Entries> entries = new List<Entries>();
        public TableView()
        {
            InitializeComponent();
        }

        private void AddEntry(object sender, RoutedEventArgs e)
        {
            Entries newEntry = new Entries();
            entries.Add(newEntry);
            TableEntries.Items.Add(entries);
            TableEntries.UnselectAllCells();
        }
        private void EditEntry(object sender, RoutedEventArgs e)
        {
            Entries newEntry = new Entries();
            if (TableEntries.SelectedIndex != -1)
            {
                entries[TableEntries.SelectedIndex] = newEntry;
                int replaceAt = TableEntries.SelectedIndex;
                TableEntries.Items.RemoveAt(replaceAt);
                TableEntries.Items.Insert(replaceAt, newEntry);
            }
        }
        private void DeleteEntry(object sender, RoutedEventArgs e)
        {
            if (TableEntries.SelectedIndex != -1)
            {
                TableEntries.Items.RemoveAt(TableEntries.SelectedIndex);
            }
        }
    }
}
