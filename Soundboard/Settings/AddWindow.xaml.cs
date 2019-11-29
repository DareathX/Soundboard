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
using Microsoft.Win32;

namespace Soundboard.Settings
{
    /// <summary>
    /// Interaction logic for AddWindow.xaml
    /// </summary>
    public partial class AddWindow : Window
    {
        public event EventHandler<Sound.Files> ItemAddedEvent;
        private Sound.Files newEntry;
        private NewKey newKey = new NewKey();
        public AddWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Closes the settings window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitAddEntry(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void SaveAddEntry(object sender, RoutedEventArgs e)
        {
            if (TableView.TableView.SoundFiles.Count >= 1)
            {
                foreach (Sound.Files file in TableView.TableView.SoundFiles)
                {
                    if (SoundName.Text == "" || SoundName.Text == file.NameSound)
                    {
                        SoundName.BorderBrush = Brushes.Red;
                    }
                    else if (SoundKey.Text == "" || SoundKey.Text == file.InputKey)
                    {
                        SoundKey.BorderBrush = Brushes.Red;
                    }
                    else if (SoundFile.Text == "")
                    {
                        SoundFile.BorderBrush = Brushes.Red;
                    }
                    else
                    {

                        NewEntryEvent(SoundName.Text, SoundKey.Text, SoundFile.Text);
                        Hide();
                    }
                }
            }
            else
            {
                NewEntryEvent(SoundName.Text, SoundKey.Text, SoundFile.Text);
                Hide();
            }
            TableView.TableView.SoundFiles.Add(new Sound.Files() { NameSound = SoundName.Text, InputKey = SoundKey.Text, FileLocation = SoundFile.Text });
        }
        protected void NewEntryEvent(string name, string key, string path)
        {
            newEntry = new Sound.Files { NameSound = name, InputKey = key, FileLocation = path };
            ItemAddedEvent.Invoke(this, newEntry);
        }

        private void EnterKey(object sender, RoutedEventArgs e)
        {
            newKey.showKey.Text = "";
            newKey.Show();
            newKey.Topmost = true;
            newKey.showKey.Focus();
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

        private void GotFocusSoundKey(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.None)
            {
                if (!e.Key.ToString().Contains("Shift") && !e.Key.ToString().Contains("Ctrl") && !e.Key.ToString().Contains("Alt") && e.Key != Key.System)
                {
                    if (Keyboard.Modifiers != ModifierKeys.Control)
                    {
                        SoundKey.Text = Keyboard.Modifiers + " + " + e.Key.ToString();
                    }
                    else
                    {
                        SoundKey.Text = "Ctrl + " + e.Key.ToString();
                    }
                }
            }
            else
            {
                SoundKey.Text = e.Key.ToString();
            }
        }
    }
}
