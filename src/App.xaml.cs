using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace AlwaysNote {
    public partial class App : Application {
        private readonly NoteWindow noteWindow;
        private readonly Mutex mutex;

        [DllImport("psapi.dll")]
        static extern int EmptyWorkingSet(IntPtr hwProc);

        private static void ReduceMemory() {
            _ = EmptyWorkingSet(Process.GetCurrentProcess().Handle);
        }

        public App() {
            bool createdNew;
            mutex = new(true, "eb9bed52-161c-4a2f-bc0f-e50da0a7aab6", out createdNew);

            if (!createdNew) {
                ExistingWindow.Show();
                Shutdown();
                return;
            }

            AppDomain.CurrentDomain.UnhandledException += (s, e) => {
                Exception ex = (Exception) e.ExceptionObject;

                MessageBox.Show(
                    "An unhandled exception occurred.\n\n" + ex.Message,
                    "AlwaysNote Fatal Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            };

            noteWindow = new();
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length < 2 || args[1] != "--minimized") {
                noteWindow.Visibility = Visibility.Visible;
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

        ~App() {
            mutex.ReleaseMutex();
            mutex.Dispose();
        }
    }
}
