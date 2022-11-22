using System.Windows;

namespace AlwaysNote {
    public partial class InfoDialog : Window {
        public InfoDialog(string title, string label) {
            InitializeComponent();
            Title = title;
            Label.Text = label;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
