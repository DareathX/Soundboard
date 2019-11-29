using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Soundboard.Settings
{
    /// <summary>
    /// Interaction logic for NewKey.xaml
    /// </summary>
    public partial class NewKey : Window
    {
        public NewKey()
        {
            InitializeComponent();
        }

        private void NewInputKey(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.None)
            {
                if (!e.Key.ToString().Contains("Shift") && !e.Key.ToString().Contains("Ctrl") && !e.Key.ToString().Contains("Alt") && e.Key != Key.System)
                {
                    showKey.Text = Keyboard.Modifiers + " + " + e.Key.ToString();
                }
            }
            else
            {
                showKey.Text = e.Key.ToString();
            }
        }

        private void SaveKeyEntry(object sender, RoutedEventArgs e)
        {
            Hide();
            Topmost = false;
            TableView.TableView.addWindow.SoundKey.Text = showKey.Text;
        }
    }
}
