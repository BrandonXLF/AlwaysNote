using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace AlwaysNote {
    public partial class App : Application {
        // [DllImport("../AlwaysNoteIcon")]
       // private static extern IntPtr createIcon(IntPtr hwnd, IntPtr icon);

        public App() {
            // InitializeComponent();
            Window noteWindow = new NoteWindow();
            noteWindow.Show();
            // Window mw = new NoteWindow();
            // IntPtr wh = new WindowInteropHelper(mw).Handle;
            // createIcon(wh, Resource.icon.Handle);
        }
    }
}
