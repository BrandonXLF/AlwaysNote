using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace AlwaysNote {
    public partial class App : Application {
        private readonly NoteWindow noteWindow = new();

        public App() {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length < 2 || args[1] != "--minimized") {
                noteWindow.Show();
            } else {
                ReduceMemory();
            }

            new NotifyIcon(noteWindow).Show();
            new Hotkey(noteWindow).Register();

            noteWindow.IsVisibleChanged += (sender, e) => {
                if (!noteWindow.IsVisible) {
                    ReduceMemory();
                }
            };
        }

        [DllImport("psapi.dll")]
        static extern int EmptyWorkingSet(IntPtr hwProc);

        private void ReduceMemory() {
            _ = EmptyWorkingSet(Process.GetCurrentProcess().Handle);
        }
    }
}
