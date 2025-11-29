using System;
using System.Runtime.InteropServices;

namespace AlwaysNote {
    internal class Hotkey : System.Windows.Forms.NativeWindow {
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
            CreateHandle(new System.Windows.Forms.CreateParams());
        }

        public void Register() {
            _ = RegisterHotKey(Handle, 0, (int)(KeyModifier.Win | KeyModifier.Ctrl), 'A');
        }

        protected override void WndProc(ref System.Windows.Forms.Message m) {
            base.WndProc(ref m);

            if (m.Msg == WM_HOTKEY) {
                window.Toggle();
            }
        }
    }
}
