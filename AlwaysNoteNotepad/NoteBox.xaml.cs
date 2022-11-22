using System;
using System.Windows;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows.Media.Animation;
using System.Windows.Controls;

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
            DoubleAnimation animation = new DoubleAnimation();
            animation.Duration = TimeSpan.FromMilliseconds(50);

            if (FindReplace.Height == 0) {
                animation.From = 0;
                animation.To = FindReplace.ActualHeight;

                FindReplace.BeginAnimation(HeightProperty, animation);
            } else {
                animation.From = FindReplace.ActualHeight;
                animation.To = 0;

                FindReplace.BeginAnimation(HeightProperty, animation);
            }
        }

        private void FindPrev_Click(object sender, RoutedEventArgs e) {
            RegexOptions flags = RegexOptions.Multiline;

            if (!(MatchCaseCheckbox.IsChecked ?? false)) flags |= RegexOptions.IgnoreCase;

            MatchCollection matches = Regex.Matches(textBox.Text, find.Text, flags);
            if (matches.Count == 0) return;

            matchCase--;

            if (matchCase < 0 || matchCase > matches.Count - 1) {
                matchCase = matches.Count - 1;
            }

            textBox.Focus();
            textBox.Select(matches[matchCase].Index, matches[matchCase].Length);
        }

        private void FindNext_Click(object sender, RoutedEventArgs e) {
            RegexOptions flags = RegexOptions.Multiline;

            if (!(MatchCaseCheckbox.IsChecked ?? false)) flags |= RegexOptions.IgnoreCase;

            MatchCollection matches = Regex.Matches(textBox.Text, find.Text, flags);
            if (matches.Count == 0) return;

            matchCase++;

            if (matchCase < 0 || matchCase > matches.Count - 1) {
                matchCase = 0;
            }

            textBox.Focus();
            textBox.Select(matches[matchCase].Index, matches[matchCase].Length);
        }

        private void ReplaceAll_Click(object sender, RoutedEventArgs e) {
            string oldStr = (useRegex.IsChecked ?? false) ? find.Text : Regex.Escape(find.Text);
            string newStr = (useRegex.IsChecked ?? false) ? replace.Text : replace.Text.Replace("$", "$$");
            RegexOptions flags = RegexOptions.Multiline;

            if (!(MatchCaseCheckbox.IsChecked ?? false)) flags |= RegexOptions.IgnoreCase;

            textBox.Text = Regex.Replace(textBox.Text, oldStr, newStr, flags);
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
