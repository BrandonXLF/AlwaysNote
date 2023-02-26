using System.Windows;

namespace AlwaysNote {
    public partial class App : Application {
        public App() {
            new NoteWindow().Show();
        }
    }
}
