using System;
using System.Runtime.InteropServices;

namespace AlwaysNote {
    internal class ExistingWindow {
        private const int SW_RESTORE = 9;
        private const uint WM_SHOWWINDOW = 0x0018;
        private const uint SW_PARENTOPENING = 3;

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static void Show() {
            IntPtr activeWindowHwnd = FindWindow(null, "AlwaysNote");

            if (activeWindowHwnd != IntPtr.Zero) {
                ShowWindow(activeWindowHwnd, SW_RESTORE);
                SendMessage(activeWindowHwnd, WM_SHOWWINDOW, new IntPtr(1), new IntPtr(SW_PARENTOPENING));
            }
        }
    }
}
