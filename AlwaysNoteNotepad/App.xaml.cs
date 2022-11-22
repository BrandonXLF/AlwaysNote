using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Linq;
using System.Windows.Data;

namespace AlwaysNote {
    public partial class App : Application {
        public App() {
            new NoteWindow().Show();
        }
    }
}
