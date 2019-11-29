using System;
using System.Linq;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace Soundboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IntPtr handle;
        private VarispeedSampleProvider speedControlInput;
        private SmbPitchShiftingSampleProvider smbPitchInput;
        private Sound.SoundControl control = new Sound.SoundControl();
        private TableView.TableView tableView = new TableView.TableView();
        private float pitchFactor = 1;
        private float speedFactor = 1;
        private float volumeOutput = 0.5F;
        public MainWindow()
        {
            SourceInitialized += (s, e) =>
            {
                handle = (new WindowInteropHelper(this)).Handle;
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(Handler.Handler.WindowProc));
            };
            InitializeComponent();
            Setup();
            dataGrid.Children.Add(tableView);
        }

        private void Setup()
        {
            WhichAudioDevice();
            Settings.Config config = new Settings.Config();
            if (File.Exists("Config.xml") && !File.ReadAllText("Config.xml").Equals(""))
            {
                using (FileStream fileStream = new FileStream("Config.xml", FileMode.Open))
                {
                    XmlSerializer xml = new XmlSerializer(typeof(Settings.Config));
                    config = (Settings.Config)xml.Deserialize(fileStream);
                }
                //volumeInput = config.FirstVolume;
                //volumeOutput = config.SecondVolume;
                //(firstVolume.RenderTransform as RotateTransform).Angle = config.FirstVolumeAngle;
                //(secondVolume.RenderTransform as RotateTransform).Angle = config.SecondVolumeAngle;
                //firstVolumePercentage.Text = config.FirstVolumePercentage;
                //secondVolumePercentage.Text = config.SecondVolumePercentage;

                if (config.SavedSoundFiles.Count >= 1)
                {
                    foreach (Sound.Files file in config.SavedSoundFiles)
                    {
                        tableView.TableEntries.Items.Add(file);
                        TableView.TableView.SoundFiles.Add(file);
                    }
                }
            }
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            Sound.AudioPlaybackEngine.Instance.Dispose();
            Settings.Config config = new Settings.Config
            {
                //FirstVolume = inPlayer.Volume,
                //SecondVolume = outPlayer.Volume,
                //FirstVolumeAngle = (firstVolume.RenderTransform as RotateTransform).Angle,
                //SecondVolumeAngle = (secondVolume.RenderTransform as RotateTransform).Angle,
                //FirstVolumePercentage = firstVolumePercentage.Text,
                //SecondVolumePercentage = secondVolumePercentage.Text,
                SavedSoundFiles = TableView.TableView.SoundFiles,
            };
            config.Save();
            HwndSource.FromHwnd(handle).RemoveHook(Handler.Handler.WindowProc);
            foreach (int hotkey in TableView.TableView.Hotkeys)
            {
                Handler.Hotkey.UnregisterHotKey(handle, hotkey);
            }
        }


        private void ExitSoundBoard(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MaximizeSoundBoard(object sender, RoutedEventArgs e)
        {
            MaxHeight = SystemParameters.WorkArea.Height;
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
            if (WindowState == WindowState.Maximized)
            {
                maximizeButtonImage.Source = new BitmapImage(new Uri(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "/Resources/Maximized.png"));
                chrome.ResizeBorderThickness = new Thickness(0);
            }
            else
            {
                maximizeButtonImage.Source = new BitmapImage(new Uri(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "/Resources/Maximize.png"));
                chrome.ResizeBorderThickness = new Thickness(4);
            }
        }

        private void AeroSnapCheck(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                maximizeButtonImage.Source = new BitmapImage(new Uri(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "/Resources/Maximized.png"));
                chrome.ResizeBorderThickness = new Thickness(0);
            }
            else
            {
                maximizeButtonImage.Source = new BitmapImage(new Uri(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "/Resources/Maximize.png"));
                chrome.ResizeBorderThickness = new Thickness(4);
            }
        }

        private void MinimizeSoundBoard(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void WhichAudioDevice()
        {
            for (int n = -1; n < WaveOut.DeviceCount; n++)
            {
                var caps = WaveOut.GetCapabilities(n);
                Console.WriteLine($"{n}: {caps.ProductName}");
            }
        }

        private void ChangeVolume()
        {
            double output = 0.5 * (control.VolumePercentage / 100);
            volumeOutput = (float)output;
            Sound.AudioPlaybackEngine.Instance.Volume = volumeOutput;
        }


        private void ChangeAudioPitch(double percentage)
        {
            double output = 1 * (percentage / 100);
            if (smbPitchInput == null)
            {
                pitchFactor = (float)output;
            }
            else
            {
                smbPitchInput.PitchFactor = (float)output;
                pitchFactor = (float)output;
            }
        }

        private void ChangeAudioSpeed(double speed)
        {
            double output = 1 * (speed / 100);
            if (speedControlInput == null)
            {
                speedFactor = (float)output;
            }
            else
            {
                speedControlInput.PlaybackRate = (float)output;
                speedFactor = (float)output;
            }
        }

        private void PlaySound(object sender, RoutedEventArgs e)
        {
            string path = tableView.SelectedItem;
            if (path != "" && Sound.AudioPlaybackEngine.Instance.outputDevice.PlaybackState == PlaybackState.Stopped || path != "" && overlapEnabled.IsChecked == true)
            {
                AudioFileReader fileReaderInput = new AudioFileReader(path);
                smbPitchInput = new SmbPitchShiftingSampleProvider(fileReaderInput)
                {
                    PitchFactor = pitchFactor
                };
                speedControlInput = new VarispeedSampleProvider(smbPitchInput, 100, new SoundTouchProfile(true, false))
                {
                    PlaybackRate = speedFactor
                };
                Sound.AudioPlaybackEngine.Instance.Volume = volumeOutput;
                Sound.AudioPlaybackEngine.Instance.PlaySound(speedControlInput);
            }
        }

        private void StopSound(object sender, RoutedEventArgs e)
        {
            Sound.AudioPlaybackEngine.Instance.StopSound();
        }

        private void SoundControl(object sender, RoutedEventArgs e)
        {
            control.Show();
        }
    }
}
