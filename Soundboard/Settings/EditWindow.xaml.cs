using Microsoft.Win32;
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
using System.Windows.Shapes;

namespace Soundboard.Settings
{
    /// <summary>
    /// Interaction logic for EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        public event EventHandler<Sound.Files> ItemEditEvent;
        public EditWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Closes the settings window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitEditEntry(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void SaveEditEntry(object sender, RoutedEventArgs e)
        {
            List<TextBox> borderColor = new List<TextBox> { SoundName, SoundKey, SoundFile };
            if (SoundName.Text == "" || SoundKey.Text == "" || SoundFile.Text == "")
            {
                foreach (var empty in borderColor)
                {
                    if (empty.Text == "")
                    {
                        empty.BorderBrush = Brushes.Red;
                    }
                    else
                    {
                        empty.BorderBrush = Brushes.Transparent;
                    }
                }
            }
            else
            {
                EditEntryEvent(SoundName.Text, SoundKey.Text, SoundFile.Text);
                Hide();
            }
        }
        protected void EditEntryEvent(string name, string key, string path)
        {
            Sound.Files editEntry = new Sound.Files { NameSound = name, InputKey = key, FileLocation = path };
            ItemEditEvent.Invoke(this, editEntry);
        }

        private void FindFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog searchFile = new OpenFileDialog
            {
                DefaultExt = ".mp3",
                Filter = "MP3 Files (*.mp3)|*.mp3"
            };
            searchFile.ShowDialog();
            SoundFile.Text = searchFile.FileName;
        }
    }
}
