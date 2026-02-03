using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace AlwaysNote {
    internal class Hotkey {
        private static readonly int WM_HOTKEY = 0x0312;
        private readonly NoteWindow window;

        enum KeyModifier {
            Alt = 1,
            Ctrl = 2,
            Shift = 4,
            Win = 8
        }

        [DllImport("user32.dll")]
        private static extern int RegisterHotKey(IntPtr hwnd, int id, int fsModifiers, int vk);

        public Hotkey(NoteWindow window) {
            this.window = window;

            var helper = new WindowInteropHelper(window);
            HwndSource source = HwndSource.FromHwnd(helper.EnsureHandle());
            source.AddHook(WndProc);
        }

        private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled) {
            if (msg == WM_HOTKEY)  {
                window.Toggle();
            }

            return IntPtr.Zero;
        }

        public void Register() {
            var wih = new WindowInteropHelper(window);
            _ = RegisterHotKey(wih.Handle, 0, (int)(KeyModifier.Win | KeyModifier.Ctrl), 'A');
        }
    }
}
