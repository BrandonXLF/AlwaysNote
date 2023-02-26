using System;
using System.Windows;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Windows.Input;

namespace AlwaysNote {
    public partial class NoteBox : UserControl {
        private int matchCase;

        public NoteBox() {
            InitializeComponent();
        }

        public static readonly DependencyProperty FindReplaceBackgroundProperty = DependencyProperty.Register(
            "FindReplaceBackground", typeof(Brush), typeof(NoteBox)
        );

        public static readonly DependencyProperty InputColorProperty = DependencyProperty.Register(
            "InputColor", typeof(Brush), typeof(NoteBox)
        );

        public static readonly DependencyProperty ButtonStyleProperty = DependencyProperty.Register(
            "ButtonStyle", typeof(Style), typeof(NoteBox)
        );

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(NoteBox)
        );

        public Brush FindReplaceBackground {
            get => (Brush)GetValue(FindReplaceBackgroundProperty);
            set => SetValue(FindReplaceBackgroundProperty, value);
        }

        public Brush InputColor {
            get => (Brush)GetValue(InputColorProperty);
            set => SetValue(InputColorProperty, value);
        }

        public Style ButtonStyle {
            get => (Style)GetValue(ButtonStyleProperty);
            set => SetValue(ButtonStyleProperty, value);
        }

        public string Text {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private void FindReplace_Toggle(object sender, RoutedEventArgs e) {
            DoubleAnimation animation = new() {
                Duration = TimeSpan.FromMilliseconds(50)
            };

            animation.From = FindReplace.Height == 0 ? 0 : FindReplace.ActualHeight;
            animation.To = FindReplace.Height == 0 ? FindReplace.ActualHeight : 0;

            FindReplace.BeginAnimation(HeightProperty, animation);
        }

        private string FindText {
            get => useRegex.IsChecked == true ? find.Text : Regex.Escape(find.Text);
        }

        private string ReplaceText {
            get => useRegex.IsChecked == true ? replace.Text : replace.Text.Replace("$", "$$");
        }

        private RegexOptions RegexFlags {
            get {
                RegexOptions flags = RegexOptions.Multiline;

                if (!(MatchCaseCheckbox.IsChecked ?? false))
                    flags |= RegexOptions.IgnoreCase;

                return flags;
            }
        }

        private void HighlightMatch(int relIndex) {
            try {
                MatchCollection matches = Regex.Matches(textBox.Text, FindText, RegexFlags);

                if (matches.Count == 0) {
                    textBox.SelectionLength = 0;
                    matchesStatus.Text = "";

                    return;
                }

                matchCase += relIndex;

                if (matchCase < 0)
                    matchCase = matches.Count - 1;

                if (matchCase > matches.Count - 1)
                    matchCase = 0;

                IInputElement focused = FocusManager.GetFocusedElement(FocusManager.GetFocusScope(this));

                textBox.Focus();
                textBox.Select(matches[matchCase].Index, matches[matchCase].Length);
                focused.Focus();

                matchesStatus.Text = (matchCase + 1) + "/" + matches.Count;
            } catch { }
        }

        private void Find_TextChanged(object sender, RoutedEventArgs e) {
            HighlightMatch(-matchCase);
        }

        private void FindPrev_Click(object sender, RoutedEventArgs e) {
            HighlightMatch(-1);
        }

        private void FindNext_Click(object sender, RoutedEventArgs e) {
            HighlightMatch(1);
        }

        private void ReplaceOne_Click(object sender, RoutedEventArgs e) {
            int counter = 0;

            string eval(Match match) =>
                counter++ == matchCase ? match.Result(ReplaceText) : match.Value;

            textBox.Text = Regex.Replace(textBox.Text, FindText, eval, RegexFlags);
            HighlightMatch(0);
        }

        private void ReplaceAll_Click(object sender, RoutedEventArgs e) {
            textBox.Text = Regex.Replace(textBox.Text, FindText, ReplaceText, RegexFlags);
        }

        private void FindBox_GotFocus(object sender, RoutedEventArgs e) {
            TextBox source = e.Source as TextBox;
            TextBlock text = FindName(source.Name + "_placeholder") as TextBlock;

            text.Visibility = Visibility.Hidden;
        }

        private void FindBox_LostFocus(object sender, RoutedEventArgs e) {
            TextBox source = e.Source as TextBox;
            TextBlock text = FindName(source.Name + "_placeholder") as TextBlock;

            text.Visibility = source.Text == "" ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
