using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudio.CoreAudioApi;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using System.Xml;

namespace Soundboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isCollapsed = true;
        private bool knobIsPressed = false;
        VarispeedSampleProvider speedControlInput;
        VarispeedSampleProvider speedControlOutput;
        private SmbPitchShiftingSampleProvider smbPitchInput;
        private SmbPitchShiftingSampleProvider smbPitchOutput;
        private float pitchFactor = 1;
        private float speedFactor = 1;
        private float volumeInput = 1;
        private float volumeOutput = 1;
        private WaveOutEvent inPlayer;
        private WaveOutEvent outPlayer;
        private DispatcherTimer timer = new DispatcherTimer();
        private TableView.TableView tableView = new TableView.TableView();
        public MainWindow()
        {
            SourceInitialized += (s, e) =>
            {
                IntPtr handle = (new WindowInteropHelper(this)).Handle;
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
            };
            InitializeComponent();
            Setup();
            timer.Tick += Timer_Tick;
            dataGrid.Children.Add(tableView);
        }

        private void Setup()
        {
            //RotateTransform rotate;
            //rotate = new RotateTransform(Properties.Settings.Default.FirstVolume);
            //firstVolume.RenderTransform = rotate;
            //rotate = new RotateTransform(Properties.Settings.Default.SecondVolume);
            //secondVolume.RenderTransform = rotate;
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
        }

        private void MinimizeSoundBoard(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void MenuToggleButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Interval = TimeSpan.FromMilliseconds(15);
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (isCollapsed)
            {
                ButtonMenu.Height += 10;
                if (ButtonMenu.Height == ButtonMenu.MaxHeight)
                {
                    timer.Stop();
                    isCollapsed = false;
                }
            }
            else
            {
                ButtonMenu.Height -= 10;
                if (ButtonMenu.Height == ButtonMenu.MinHeight)
                {
                    timer.Stop();
                    isCollapsed = true;
                }
            }
        }

        private void KnobMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            knobIsPressed = true;
            FrameworkElement turningKnob = e.Source as FrameworkElement;
            Point mousePosition = new Point();
            Image whichAmount = sender as Image;
            var parent = VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(whichAmount));
            Image spaceOfWhichAmount = VisualTreeHelper.GetChild(parent, 0) as Image;
            if (turningKnob.Name.Equals(whichAmount.Name))
            {
                mousePosition = e.GetPosition(spaceOfWhichAmount);
            }
            double angle = AngleBoundaries(mousePosition);
            RotateTransform rotation = new RotateTransform(angle);
            if (turningKnob.Name.Equals(whichAmount.Name))
            {
                whichAmount.RenderTransform = rotation;
            }
        }
        private void KnobMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            knobIsPressed = false;
        }

        private void KnobMouseMove(object sender, MouseEventArgs e)
        {
            FrameworkElement turningKnob = e.Source as FrameworkElement;
            Point mousePosition = new Point();
            Point popupPosition = e.GetPosition(main);
            Image whichAmount = sender as Image;
            var parent = VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(whichAmount));
            Image spaceOfWhichAmount = VisualTreeHelper.GetChild(parent, 0) as Image;
            Popup popupPercentage = VisualTreeHelper.GetChild(parent, 2) as Popup;
            if (turningKnob.Name.Equals(whichAmount.Name))
            {
                mousePosition = e.GetPosition(spaceOfWhichAmount);
                popupPercentage.HorizontalOffset = popupPosition.X - 10;
                popupPercentage.VerticalOffset = popupPosition.Y + 25;
            }
            double angle = AngleBoundaries(mousePosition);
            RotateTransform rotation = new RotateTransform(angle);
            KnobPercentageAmount(angle, turningKnob, popupPercentage, whichAmount);
            if (knobIsPressed)
            {
                if (turningKnob.Name.Equals(whichAmount.Name))
                {
                    whichAmount.RenderTransform = rotation;
                }
            }
            WhichAudioDevice();
        }

        private double AngleBoundaries(Point mousePosition)
        {
            double slope = (50 - mousePosition.Y) / (50 - mousePosition.X);
            double angle = Math.Atan(slope) * 180 / Math.PI;
            if (mousePosition.X > 50)
            {
                angle = angle + 90;
            }
            else
            {
                angle = angle - 90;
            }
            if (angle >= 130)
            {
                angle = 130;
            }
            else if (angle <= -130)
            {
                angle = -130;
            }
            else if (double.IsNaN(angle))
            {
                angle = 0;
            }
            return angle;
        }

        private new void MouseLeave(object sender, MouseEventArgs e)
        {
            Image whichAmount = sender as Image;
            var parent = VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(whichAmount));
            Popup popupPercentage = VisualTreeHelper.GetChild(parent, 2) as Popup;
            popupPercentage.IsOpen = false;
            knobIsPressed = false;
        }

        private void KnobPercentageAmount(double angle, FrameworkElement turningKnob, Popup popupPercentage, Image whichAmount)
        {
            TextBlock popupText = popupPercentage.Child as TextBlock;
            double percentageCalc = 100f * (angle / 260f) * 2 + 100;
            if (turningKnob.Name.Equals(whichAmount.Name))
            {
                popupPercentage.IsOpen = true;
                if (knobIsPressed)
                {
                    switch (turningKnob.Name)
                    {
                        case "firstVolume":
                            ChangeInputVolume(percentageCalc);
                            break;
                        case "secondVolume":
                            ChangeOutputVolume(percentageCalc);
                            break;
                        case "pitch":
                            ChangeAudioPitch(percentageCalc);
                            break;
                        case "speed":
                            ChangeAudioSpeed(percentageCalc);
                            break;
                    }
                    popupText.Text = (int)percentageCalc + "%";
                }
                else
                {
                    if (popupText.Text.Equals(""))
                    {
                        popupText.Text = "100%";
                    }
                }
            }
        }

        private void WhichAudioDevice()
        {

        }

        private void ChangeInputVolume(double percentage)
        {
            double output = 0.5 * (percentage / 100);
            if (inPlayer == null)
            {
                volumeInput = (float)output;
            }
            else
            {
                inPlayer.Volume = (float)output;
                volumeInput = (float)output;
            }
        }

        private void ChangeOutputVolume(double percentage)
        {
            double output = 0.5 * (percentage / 100);
            if (inPlayer == null)
            {
                volumeOutput = (float)output;
            }
            else
            {
                outPlayer.Volume = (float)output;
                volumeOutput = (float)output;
            }
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
                smbPitchOutput.PitchFactor = (float)output;
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
                speedControlOutput.PlaybackRate = (float)output;
                speedFactor = (float)output;
            }
        }
        public void PlaySound()
        {
            inPlayer = new WaveOutEvent()
            {
                DeviceNumber = -1
            };
            outPlayer = new WaveOutEvent()
            {
                DeviceNumber = 1
            };
            string path = @"C:\Users\Wilco\Music\Giga Pudding.mp3";
            AudioFileReader fileReaderInput = new AudioFileReader(path);
            AudioFileReader fileReaderOutput = new AudioFileReader(path);
            smbPitchInput = new SmbPitchShiftingSampleProvider(fileReaderInput);
            smbPitchOutput = new SmbPitchShiftingSampleProvider(fileReaderOutput);
            smbPitchInput.PitchFactor = pitchFactor;
            smbPitchOutput.PitchFactor = pitchFactor;
            speedControlInput = new VarispeedSampleProvider(smbPitchInput, 100, new SoundTouchProfile(true, false));
            speedControlOutput = new VarispeedSampleProvider(smbPitchOutput, 100, new SoundTouchProfile(true, false));
            speedControlInput.PlaybackRate = speedFactor;
            speedControlOutput.PlaybackRate = speedFactor;
            inPlayer.Volume = volumeInput;
            outPlayer.Volume = volumeOutput;
            inPlayer.Init(speedControlInput);
            outPlayer.Init(speedControlOutput);
            inPlayer.Play();
            outPlayer.Play();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PlaySound();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            inPlayer.Stop();
            outPlayer.Stop();
        }

        private void DialResetButton(object sender, RoutedEventArgs e)
        {
            RotateTransform resetRotation = new RotateTransform(0);
            pitch.RenderTransform = resetRotation;
            speed.RenderTransform = resetRotation;
            ChangeAudioPitch(100);
            ChangeAudioSpeed(100);
            pitchPercentage.Text = "100%";
            speedPercentage.Text = "100%";
        }
    }
}
