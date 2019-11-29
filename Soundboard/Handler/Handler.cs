using System;
using System.Linq;
using System.Windows.Input;

namespace Soundboard.Handler
{
    public class Handler
    {
        public static Key VKey;
        public static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    AeroSnap.WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
                case 0x0312:
                    int key = (((int)lParam >> 16) & 0xFFFF);
                    VKey = KeyInterop.KeyFromVirtualKey(key);
                    Console.WriteLine("yup");
                    handled = true;
                    break;
            }
            return (IntPtr)0;
        }

        
    }
}
