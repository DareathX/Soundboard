using NAudio.Wave.SampleProviders;
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
        public float PitchFactor { get; set; } = 1;
        public float SpeedFactor { get; set; } = 1;
        public static float VolumeOutput { get; set; } = 0.5F;
        public static float VolumeSecondOutput { get; set; } = 0.5F;
        internal VarispeedSampleProvider SpeedControlInput { get; set; }

        public static SmbPitchShiftingSampleProvider smbPitchInput;
        public SoundControl()
        {
            InitializeComponent();
        }

        private void ExitSoundControl(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void FirstSliderPercentage(object sender, RoutedEventArgs e)
        {
            ChangeVolume(volumeFirstOutput.Value);
        }

        private void ChangeVolume(double volume)
        {
            double output = 0.5 * (volume / 100);
            if (volume != 100)
            {
                VolumeOutput = (float)output;
                AudioPlaybackEngine.Instance.Volume = VolumeOutput;
            }
            else
            {
                AudioPlaybackEngine.Instance.Volume = VolumeOutput;
            }
        }

        private void SecondSliderPercentage(object sender, RoutedEventArgs e)
        {
            ChangeSecondVolume(volumeSecondOutput.Value);
        }

        private void ChangeSecondVolume(double volume)
        {
            double output = 0.5 * (volume / 100);
            if (volume != 100)
            {
                VolumeOutput = (float)output;
                AudioPlaybackEngine.Instance.Volume = VolumeOutput;
            }
            else
            {
                AudioPlaybackEngine.Instance.Volume = VolumeOutput;
            }
        }

        private void PitchSliderPercentage(object sender, RoutedEventArgs e)
        {
            ChangeAudioPitch(pitchOutput.Value);
        }

        private void ChangeAudioPitch(double pitch)
        {
            double output = 1 * (pitch / 100);
            if (smbPitchInput == null)
            {
                PitchFactor = (float)output;
            }
            else
            {
                smbPitchInput.PitchFactor = (float)output;
                PitchFactor = (float)output;
            }
        }

        private void SpeedSliderPercentage(object sender, RoutedEventArgs e)
        {
            ChangeAudioSpeed(speedOutput.Value);
        }

        private void ChangeAudioSpeed(double speed)
        {
            double output = 1 * (speed / 100);
            if (SpeedControlInput == null)
            {
                SpeedFactor = (float)output;
            }
            else
            {
                SpeedControlInput.PlaybackRate = (float)output;
                SpeedFactor = (float)output;
            }
        }
    }
}
