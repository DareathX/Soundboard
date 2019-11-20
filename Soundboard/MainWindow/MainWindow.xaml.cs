using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.IO;
using System.Xml.Serialization;
using System.Net;

namespace Soundboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VarispeedSampleProvider speedControlInput;
        private SmbPitchShiftingSampleProvider smbPitchInput;
        private Sound.SoundControl control = new Sound.SoundControl();
        private TableView.TableView tableView = new TableView.TableView();
        WebClient web = new WebClient();
        private float pitchFactor = 1;
        private float speedFactor = 1;
        private float volumeOutput = 0.5F;
        public MainWindow()
        {
            SourceInitialized += (s, e) =>
            {
                IntPtr handle = (new WindowInteropHelper(this)).Handle;
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
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
        }

        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }
            return (IntPtr)0;
        }

        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
            }
            Marshal.StructureToPtr(mmi, lParam, true);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>x coordinate of point.</summary>
            public int x;
            /// <summary>y coordinate of point.</summary>
            public int y;
            /// <summary>Construct a point of coordinates (x,y).</summary>
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public int dwFlags = 0;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public static readonly RECT Empty = new RECT();
            public int Width { get { return Math.Abs(right - left); } }
            public int Height { get { return bottom - top; } }
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }
            public RECT(RECT rcSrc)
            {
                left = rcSrc.left;
                top = rcSrc.top;
                right = rcSrc.right;
                bottom = rcSrc.bottom;
            }
            public bool IsEmpty { get { return left >= right || top >= bottom; } }
            public override string ToString()
            {
                if (this == Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }
            public override bool Equals(object obj)
            {
                if (!(obj is Rect)) { return false; }
                return (this == (RECT)obj);
            }
            /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
            public override int GetHashCode() => left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
            public static bool operator ==(RECT rect1, RECT rect2) { return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom); }
            /// <summary> Determine if 2 RECT are different(deep compare)</summary>
            public static bool operator !=(RECT rect1, RECT rect2) { return !(rect1 == rect2); }
        }

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);


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
                //web.Headers.Add("user-agent", "Anything");
                //web.DownloadFile("https://github.com/DareathX/Soundboard/archive/master.zip", "Soundboard.zip");
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
