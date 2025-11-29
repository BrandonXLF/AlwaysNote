using System;
using System.ComponentModel;
using System.Windows;

namespace AlwaysNote {
    internal class NotifyIcon {
        private readonly NoteWindow window;
        private readonly System.Windows.Forms.NotifyIcon icon;

        public NotifyIcon(NoteWindow window) {
            this.window = window;

            System.Windows.Forms.ContextMenuStrip contextMenu = new();

            System.Windows.Forms.ToolStripMenuItem openItem = new() {
                Text = "Open"
            };
            openItem.Click += new EventHandler(OpenClicked);
            contextMenu.Items.Add(openItem);

            System.Windows.Forms.ToolStripMenuItem exitItem = new() {
                Text = "Exit"
            };
            exitItem.Click += new EventHandler(ExitClicked);
            contextMenu.Items.Add(exitItem);

            icon = new(new Container()) {
                Icon = Resources.AppIcon,
                Text = "AlwaysNote\nWin + Ctrl + A",
                ContextMenuStrip = contextMenu
            };
            icon.Click += new EventHandler(IconClicked);
        }

        public void Show() {
            icon.Visible = true;
        }

        private void IconClicked(object Sender, EventArgs e) {
            if ((e as System.Windows.Forms.MouseEventArgs).Button
                != System.Windows.Forms.MouseButtons.Left) return;

            window.Toggle();
        }

        private void OpenClicked(object Sender, EventArgs e) {
            window.Show();
        }

        private void ExitClicked(object Sender, EventArgs e) {
            Application.Current.Shutdown();
        }
    }
}
