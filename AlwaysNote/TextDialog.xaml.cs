using System.Windows;

namespace AlwaysNote {

    public partial class TextDialog : Window {
        public string NewValue { get; internal set; }

        public TextDialog(string title, string label, string startingValue = "") {
            InitializeComponent();
            Title = title;
            Label.Text = label + ":";
            Input.Text = startingValue;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
            NewValue = Input.Text;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
