using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Windows.UI.ViewManagement;

namespace AlwaysNote {
    public partial class NoteWindow : Window {
        private readonly NoteStore noteStore = new();
        private readonly UISettings uiSettings = new();

        private static Color ToColor(Windows.UI.Color color) {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public void Toggle() {
            if (IsVisible) {
                Hide();
            } else {
                Show();
            }
        }

        public NoteWindow() {
            InitializeComponent();

            DataContext = noteStore;
            NoteList.ItemsSource = noteStore.NoteNames;

            ApplyTheme(uiSettings, new object());
            uiSettings.ColorValuesChanged += ApplyTheme;
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

        private void Top_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e) {
            Hide();
        }

        private void NoteTitle_Click(object sender, RoutedEventArgs e) {
            ListPopup.IsOpen = !ListPopup.IsOpen;
        }

        private void NoteTitle_MouseEnter(object sender, MouseEventArgs e) {
            ListPopup.StaysOpen = true;
        }

        private void NoteTitle_MouseLeave(object sender, MouseEventArgs e) {
            ListPopup.StaysOpen = false;
        }

        private void NoteListEntry_Click(object sender, RoutedEventArgs e) {
            noteStore.CurrentNote = (string)(sender as Button).Tag;
            ListPopup.IsOpen = false;
        }

        private void Menu_RenameNote(object sender, RoutedEventArgs es) {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            string currentName = (string)(contextMenu.PlacementTarget as Button).Tag;
            TextDialog renameDialog = new("Rename Note", "Enter new name", currentName);

            if (renameDialog.ShowDialog() == true) {
                if (noteStore.NoteNames.Contains(renameDialog.NewValue)) {
                    new InfoDialog("Error", "A note with that name already exists!").ShowDialog();
                    return;
                }

                noteStore.RenameNote(currentName, renameDialog.NewValue);
            }
        }

        private void Menu_DeleteNote(object sender, RoutedEventArgs e) {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            string name = (string)(contextMenu.PlacementTarget as Button).Tag;
            ConfirmDialog deleteDialog = new("Delete Note", "Are you sure you want to delete the note \"" + name + "\"?");

            if (deleteDialog.ShowDialog() == true) {
                noteStore.DeleteNote(name);
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

        private System.Windows.Controls.Primitives.CustomPopupPlacement[] PlaceListPopup(Size popupSize, Size targetSize, Point offset) {
            return [
                new(new Point(0, targetSize.Height), System.Windows.Controls.Primitives.PopupPrimaryAxis.Horizontal)
            ];
        }
    }
}
