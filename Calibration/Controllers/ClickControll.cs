using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Calibration.Controllers
{
    public class ClickControll
    {
        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        public static void LeftClick(int x, int y)
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, x, y,0, 0);
        }

        public static void RightClick(int x, int y)
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN| MOUSEEVENTF_RIGHTUP, x, y, 0, 0);
        }



    }
}
