using System.Windows;

namespace AlwaysNote {
    public partial class ConfirmDialog : Window {
        public ConfirmDialog(string title, string label) {
            InitializeComponent();
            Title = title;
            Label.Text = label;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void NoButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
