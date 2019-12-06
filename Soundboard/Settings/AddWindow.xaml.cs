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
        private int hotkeyCounter = 0;
        private string key;
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
            key = SoundKey.Text.Split('+').Last();
            SoundName.BorderBrush = Brushes.Gray;
            SoundKey.BorderBrush = Brushes.Gray;
            SoundFile.BorderBrush = Brushes.Gray;
            foreach (Sound.Files file in TableView.TableView.SoundFiles)
            {
                if (SoundKey.Text == file.InputKey)
                {
                    SoundKey.BorderBrush = Brushes.Red;
                }
            }
            if (SoundName.Text == "")
            {
                SoundName.BorderBrush = Brushes.Red;
            }
            else if (SoundKey.Text == "")
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
                TableView.TableView.Hotkeys.Add(new KeyValuePair<int, string>(hotkeyCounter, key));
                if (key.Any(char.IsDigit))
                {
                    key = "D" + key;
                }
                TableView.TableView.SoundFiles.Add(new Sound.Files() { NameSound = SoundName.Text, InputKey = SoundKey.Text, HotkeyCode = key, HotkeyCounter = hotkeyCounter, FileLocation = SoundFile.Text });
                Handler.Hotkey.RegisterHotKey(Handler.Handler.Handle, hotkeyCounter++, 0, KeyInterop.VirtualKeyFromKey((Key)Enum.Parse(typeof(Key), key)));
            }
        }
        protected void NewEntryEvent(string name, string key, string path)
        {
            newEntry = new Sound.Files { NameSound = name, InputKey = key, FileLocation = path };
            ItemAddedEvent.Invoke(this, newEntry);
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
                e.Handled = true;
                SoundKey.Text = Keyboard.Modifiers.ToString();
                if (!e.Key.ToString().Contains("Ctrl") && !e.Key.ToString().Contains("Shift") && !e.Key.ToString().Contains("Alt") && !e.Key.ToString().Contains("System"))
                {
                    SoundKey.Text += " + " + e.Key.ToString();
                }
            }
            else
            {
                SoundKey.Text = e.Key.ToString();
            }
            SoundKey.Text = SoundKey.Text.Replace("Control", "Ctrl");
            if (e.Key.ToString().Any(char.IsDigit))
            {
                SoundKey.Text = SoundKey.Text.Replace("D", "");
            }
        }
    }
}
