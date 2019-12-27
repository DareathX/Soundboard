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
            TableView.TableView.editing = false;
        }

        private void SaveEditEntry(object sender, RoutedEventArgs e)
        {
            SoundName.BorderBrush = Brushes.Gray;
            SoundKey.BorderBrush = Brushes.Gray;
            SoundFile.BorderBrush = Brushes.Gray;
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
                        empty.BorderBrush = Brushes.Gray;
                    }
                }
            }
            else
            {
                string key = SoundKey.Text.Split('+').Last();
                string modif = SoundKey.Text.Split('+').First();
                bool hasMods = false;
                if (key.Any(char.IsDigit))
                {
                    key = "D" + key.Replace(" ", "");
                }
                if (!SoundKey.Text.Equals(TableView.TableView.keyEdit))
                {
                    System.Data.DataTable allMods = new System.Data.DataTable();
                    var replaceHotkey = new KeyValuePair<int, string>(TableView.TableView.Hotkeys[TableView.TableView.Hotkeys.FindIndex(f => f.Value.Equals(TableView.TableView.keyEdit))].Key, SoundKey.Text);
                    TableView.TableView.Hotkeys[TableView.TableView.Hotkeys.FindIndex(f => f.Value.Equals(TableView.TableView.keyEdit))] = replaceHotkey;
                    foreach (string mods in Enum.GetNames(typeof(Handler.Hotkey.KeyModifier)))
                    {
                        if (modif.Contains(mods))
                        {
                            modif = modif.Replace(mods, ((int)(Handler.Hotkey.KeyModifier)Enum.Parse(typeof(Handler.Hotkey.KeyModifier), mods)).ToString());
                            hasMods = true;
                        }
                    }
                    if (!hasMods)
                    {
                        modif = "0";
                    }
                    else
                    {
                        modif = allMods.Compute(modif, "").ToString();
                    }
                    Handler.Hotkey.RegisterHotKey(Handler.Handler.Handle, TableView.TableView.Hotkeys[TableView.TableView.Hotkeys.FindIndex(f => f.Value.Equals(SoundKey.Text))].Key, uint.Parse(modif), KeyInterop.VirtualKeyFromKey((Key)Enum.Parse(typeof(Key), key)));
                    allMods.Dispose();
                }
                EditEntryEvent(SoundName.Text, SoundKey.Text, key, TableView.TableView.SoundFiles[TableView.TableView.SoundFiles.FindIndex(f => f.InputKey.Equals(TableView.TableView.keyEdit))].HotkeyCounter, SoundFile.Text);
                Hide();
                TableView.TableView.editing = false;
            }
        }
        protected void EditEntryEvent(string name, string key, string keyCode, int keyCounter, string path)
        {
            Sound.Files editEntry = new Sound.Files { NameSound = name, InputKey = key, HotkeyCode = keyCode, HotkeyCounter = keyCounter, FileLocation = path };
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
