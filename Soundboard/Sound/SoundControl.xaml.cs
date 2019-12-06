using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Soundboard.Sound
{
    /// <summary>
    /// Interaction logic for SoundControl.xaml
    /// </summary>
    public partial class SoundControl : Window
    {
        public double VolumePercentage { get; set; }
        public SoundControl()
        {
            InitializeComponent();
        }

        private void ExitSoundControl(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void SliderPercentage(object sender, RoutedEventArgs e)
        {
            VolumePercentage = volumeFirstOutput.Value;
        }
    }
}
