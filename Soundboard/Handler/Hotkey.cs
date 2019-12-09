using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Soundboard.Handler
{
    public class Hotkey
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, int vk);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public enum KeyModifier
        {
            None = 0,
            Alt = 1,
            System = 1,
            Ctrl = 2,
            Shift = 4,
            WinKey = 8
        }
    }
}
