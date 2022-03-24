using System;
using System.Windows;
using System.Windows.Input;
using Windows.UI.ViewManagement;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Data;

namespace AlwaysNote {
    [StructLayout(LayoutKind.Sequential)]
    internal struct Win32Point {
        public int X;
        public int Y;
    };

    public partial class NoteWindow : Window {
        private readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");
        private readonly uint ID_EXIT_PROGRAM = 40059;
        private readonly int HOTKEY_ID = 5643242;

        private readonly System.Threading.Mutex mutex = new(true, "{24e44fcf-b450-4128-863f-96007d8ea21f}");
        private readonly string[] args = Environment.GetCommandLineArgs();
        private readonly NoteStore noteStore = new();
        private IntPtr windowHandle;
        private UISettings uiSettings = new();

        [DllImport("user32")]
        internal static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32")]
        internal static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

        [DllImport("user32", CharSet = CharSet.Unicode)]
        internal static extern int RegisterWindowMessage(string message);

        [DllImport("user32")]
        internal static extern IntPtr CreatePopupMenu();

        [DllImport("user32", CharSet = CharSet.Unicode)]
        internal static extern bool AppendMenu(IntPtr hMenu, uint uFlags, uint uIDNewItem, string lpNewItem);

        [DllImport("user32")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32")]
        internal static extern bool DestroyMenu(IntPtr hMenu);

        [DllImport("user32")]
        internal static extern bool TrackPopupMenuEx(IntPtr hMenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(out Win32Point lpPoint);

        private static Color ToColor(Windows.UI.Color color) {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public NoteWindow() {
            if (!mutex.WaitOne(TimeSpan.Zero, true)) {
                PostMessage((IntPtr)0xffff, WM_SHOWME, IntPtr.Zero, IntPtr.Zero);
                Application.Current.Shutdown();
            }

            InitializeComponent();

            DataContext = noteStore;
            NoteList.ItemsSource = noteStore.NoteNames;

            ApplyTheme(uiSettings, new object());
            uiSettings.ColorValuesChanged += ApplyTheme;
        }

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);

            windowHandle = new WindowInteropHelper(this).Handle;
            HwndSource source = HwndSource.FromHwnd(windowHandle);

            source.AddHook(new HwndSourceHook(WindowHook));
            RegisterHotKey(windowHandle, HOTKEY_ID, 0x0008 /* MOD_WIN */ | 0x0004 /* MOD_SHIFT */, 0x41); // Win + Shift + A

            if (args.Length > 1 && args[1] == "--minimized") {
                window.Hide();
            }

            System.Windows.Forms.NotifyIcon notifyIcon = new() {
                Icon = Resource.icon,
                Text = "AlwaysNote\nWin + Shift + A",
                Visible = true
            };

            notifyIcon.MouseClick += NotifyIcon_MouseClick;
        }

        private IntPtr WindowHook(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool Handled) {
            if (msg == 0x0111 /* WM_COMMAND */ && wParam == (IntPtr)ID_EXIT_PROGRAM) {
                Application.Current.Shutdown();
            } else if (msg == WM_SHOWME) {
                window.Show();
            } else if (msg == 0x0312 /* WM_HOTKEY */) {
                if (window.IsVisible) {
                    window.Hide();
                } else {
                    window.Show();
                }
            }

            return IntPtr.Zero;
        }

        public void ApplyTheme(UISettings uiSettings, object args) {
            Color accent = ToColor(uiSettings.GetColorValue(UIColorType.Accent));
            Color background = ToColor(uiSettings.GetColorValue(UIColorType.Background));

            bool isDark = (background.R + background.G + background.B) < (255 * 3 - background.R - background.G - background.B);
            bool useDarkText = (accent.R * 0.299 + accent.G * 0.587 + accent.B * 0.114) > 150;

            window.Dispatcher.Invoke(() => {
                (Resources["Accent"] as SolidColorBrush).Color = accent;
                (Resources["AccentOpposite"] as SolidColorBrush).Color = useDarkText ? Color.FromRgb(0, 0, 0) : Color.FromRgb(255, 255, 255);

                if (isDark) {
                    (Resources["Theme"] as SolidColorBrush).Color = Color.FromArgb(225, 25, 25, 25);
                    (Resources["Theme2"] as SolidColorBrush).Color = Color.FromRgb(50, 50, 50);
                    (Resources["Theme3"] as SolidColorBrush).Color = Color.FromRgb(75, 75, 75);
                    (Resources["Opposite"] as SolidColorBrush).Color = Color.FromRgb(255, 255, 255);
                } else {
                    (Resources["Theme"] as SolidColorBrush).Color = Color.FromArgb(225, 255, 255, 255);
                    (Resources["Theme2"] as SolidColorBrush).Color = Color.FromRgb(225, 225, 225);
                    (Resources["Theme3"] as SolidColorBrush).Color = Color.FromRgb(200, 200, 200);
                    (Resources["Opposite"] as SolidColorBrush).Color = Color.FromRgb(0, 0, 0);
                }
            });
        }

        private void NotifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (e.Button == System.Windows.Forms.MouseButtons.Left) {
                if (window.IsVisible) {
                    window.Hide();
                } else {
                    window.Show();
                }
            } else if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                GetCursorPos(out Win32Point curPos);
                IntPtr hMenu = CreatePopupMenu();

                AppendMenu(hMenu, 0x0 /* MF_STRING */, ID_EXIT_PROGRAM, "Exit");
                SetForegroundWindow(windowHandle);
                TrackPopupMenuEx(hMenu, 0x0 /* TPM_LEFTALIGN */ | 0x0 /* TPM_TOPALIGN */, curPos.X, curPos.Y, windowHandle, IntPtr.Zero);
                DestroyMenu(hMenu);
            }
        }

        private void Top_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e) {
            e.Handled = true;
            window.Hide();
        }

        private void NoteTitle_Click(object sender, RoutedEventArgs e) {
            ListPopup.IsOpen = !ListPopup.IsOpen;
        }

        private void NoteTitle_MouseEnter(object sender, MouseEventArgs e) {
            e.Handled = true;
            ListPopup.StaysOpen = true;
        }

        private void NoteTitle_MouseLeave(object sender, MouseEventArgs e) {
            e.Handled = true;
            ListPopup.StaysOpen = false;
        }

        private void NoteListEntry_MouseLeftButtonUp(object sender, RoutedEventArgs e) {
            noteStore.CurrentNote = (sender as TextBlock).Text;
        }

        private void Menu_RenameNote(object sender, RoutedEventArgs es) {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            TextBlock item = contextMenu.PlacementTarget as TextBlock;
            TextDialog renameDialog = new("Rename Note", "Enter new name", item.Text);

            if (renameDialog.ShowDialog() == true) {
                if (noteStore.NoteNames.Contains(renameDialog.NewValue)) {
                    new InfoDialog("Error", "A note with that name already exists!").ShowDialog();
                    return;
                }

                noteStore.RenameNote(item.Text, renameDialog.NewValue);
            }
        }

        private void Menu_DeleteNote(object sender, RoutedEventArgs e) {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            TextBlock item = contextMenu.PlacementTarget as TextBlock;
            ConfirmDialog deleteDialog = new("Delete Note", "Are you sure you want to delete the note \"" + item.Text + "\"?");

            if (deleteDialog.ShowDialog() == true) {
                noteStore.DeleteNote(item.Text);
            }
        }

        private void AddNote_Click(object sender, RoutedEventArgs e) {
            TextDialog nameDialog = new("New Note", "Enter note name");

            if (nameDialog.ShowDialog() == true) {
                if (noteStore.NoteNames.Contains(nameDialog.NewValue)) {
                    new InfoDialog("Error", "A note with that name already exists!").ShowDialog();
                    return;
                }

                noteStore.AddNote(nameDialog.NewValue);
                noteStore.CurrentNote = nameDialog.NewValue;
            }
        }
    }
}
