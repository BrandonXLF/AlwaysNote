using System;
using System.Windows;

namespace AlwaysNote {
    public partial class App : Application {
        private readonly NoteWindow noteWindow = new();

        public App() {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length < 2 || args[1] != "--minimized") {
                noteWindow.Show();
            }

            new NotifyIcon(noteWindow).Show();
            new Hotkey(noteWindow).Register();
        }
    }
}
