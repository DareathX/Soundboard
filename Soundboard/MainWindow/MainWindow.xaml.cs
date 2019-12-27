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
using System.Windows.Input;
using System.Collections.Generic;

namespace Soundboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Sound.SoundControl control = new Sound.SoundControl();
        private TableView.TableView tableView = new TableView.TableView();
        public MainWindow()
        {
            SourceInitialized += (s, e) =>
            {
                Handler.Handler.Handle = (new WindowInteropHelper(this)).Handle;
                HwndSource.FromHwnd(Handler.Handler.Handle).AddHook(new HwndSourceHook(Handler.Handler.WindowProc));
                Setup();
            };
            InitializeComponent();
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
                Sound.SoundControl.VolumeOutput = config.FirstVolume;
                control.volumeFirstOutput.Value = config.FirstVolumeSlider;
                Sound.SoundControl.VolumeSecondOutput = config.SecondVolume;
                control.volumeSecondOutput.Value = config.SecondVolumeSlider;
                //volumeOutput = config.SecondVolume;
                //firstVolumePercentage.Text = config.FirstVolumePercentage;
                //secondVolumePercentage.Text = config.SecondVolumePercentage;

                if (config.SavedSoundFiles.Count >= 1)
                {
                    foreach (Sound.Files file in config.SavedSoundFiles)
                    {
                        tableView.TableEntries.Items.Add(file);
                        TableView.TableView.SoundFiles.Add(file);
                        TableView.TableView.Hotkeys.Add(new KeyValuePair<int, string>(file.HotkeyCounter, file.InputKey));
                        Handler.Hotkey.RegisterHotKey(Handler.Handler.Handle, file.HotkeyCounter, 0, KeyInterop.VirtualKeyFromKey((Key)Enum.Parse(typeof(Key), file.HotkeyCode)));
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
                FirstVolume = Sound.SoundControl.VolumeOutput,
                FirstVolumeSlider = control.volumeFirstOutput.Value,
                SecondVolume = Sound.SoundControl.VolumeSecondOutput,
                SecondVolumeSlider = control.volumeSecondOutput.Value,
                //SecondVolume = outPlayer.Volume,
                //FirstVolumePercentage = firstVolumePercentage.Text,
                //SecondVolumePercentage = secondVolumePercentage.Text,
                SavedSoundFiles = TableView.TableView.SoundFiles,
            };
            config.Save();
            HwndSource.FromHwnd(Handler.Handler.Handle).RemoveHook(Handler.Handler.WindowProc);
            foreach (var hotkey in TableView.TableView.Hotkeys)
            {
                Handler.Hotkey.UnregisterHotKey(Handler.Handler.Handle, hotkey.Key);
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

        private void PlayButton(object sender, RoutedEventArgs e)
        {
            PlaySound();
        }

        public void PlaySound()
        {
            string path = tableView.SelectedItem;
            foreach (Sound.Files file in TableView.TableView.SoundFiles)
            {
                if (Handler.Handler.VKey == (Key)Enum.Parse(typeof(Key), file.HotkeyCode))
                {
                    path = file.FileLocation;
                }
            }
            if (TableView.TableView.editing)
            {
                path = "";
            }
            else if (overlapEnabled.IsChecked == false)
            {
                Sound.AudioPlaybackEngine.Instance.StopSound();
            }

            if (path != "" && Sound.AudioPlaybackEngine.Instance.outputDevice.PlaybackState == PlaybackState.Stopped || path != "" && overlapEnabled.IsChecked == true)
            {
                Sound.AudioPlaybackEngine.fileReaderInput = new AudioFileReader(path);
                Sound.SoundControl.smbPitchInput = new SmbPitchShiftingSampleProvider(Sound.AudioPlaybackEngine.fileReaderInput)
                {
                    PitchFactor = control.PitchFactor
                };
                control.SpeedControlInput = new VarispeedSampleProvider(Sound.SoundControl.smbPitchInput, 100, new SoundTouchProfile(true, false))
                {
                    PlaybackRate = control.SpeedFactor
                };
                Sound.AudioPlaybackEngine.Instance.Volume = Sound.SoundControl.VolumeOutput;
                Sound.AudioPlaybackEngine.Instance.PlaySound(control.SpeedControlInput);
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
