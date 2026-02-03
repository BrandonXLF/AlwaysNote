using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace AlwaysNote {
    partial class NoteStore : INotifyPropertyChanged {
        private readonly string dataFolder;
        private readonly string noteFolder;
        private readonly Regex matchUntitled = GetUntitledRegex();
        private string currentNote;
        private string currentNoteText;

        public readonly ObservableCollection<string> NoteNames = [];
        public event PropertyChangedEventHandler PropertyChanged;

        /// <remarks>Get: Throws on failure, Set: Alerts on failure</remarks>
        public string CurrentNote {
            get {
                EnsureCurrentNote();
                return currentNote;
            }

            set {
                string newNoteText;

                try {
                    newNoteText = GetNoteText(value);
                    EnsureFolderExists(dataFolder);
                    File.WriteAllText(Path.Combine(dataFolder, "CurrentNote"), value);
                } catch (IOException e) {
                    ShowError("Could not set current note to \"" + value + "\".", "Open Note", e);
                    return;
                }

                // Switch once committed to file system
                currentNote = value;
                currentNoteText = newNoteText;
                NotifyPropertyChanged("CurrentNote");
                NotifyPropertyChanged("CurrentNoteText");
            }
        }

        /// <remarks>Get: Throws on failure, Set: Alerts on failure</remarks>
        public string CurrentNoteText {
            get {
                EnsureCurrentNote();
                return currentNoteText;
            }

            set {
                // Record text immediately (before saving file)
                currentNoteText = value;
                NotifyPropertyChanged("CurrentNoteText");

                try {
                    EnsureFolderExists(noteFolder);
                    File.WriteAllText(GetNotePath(CurrentNote), value);
                } catch (IOException e) {
                    ShowError("Could not save note \"" + CurrentNote + "\".", "Save Note", e);
                }
            }
        }

        /// <remarks>Throws on failure</remarks>
        private static void EnsureFolderExists(string folder) {
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        }

        private static void ShowError(string message, string titleAction, Exception e) {
            MessageBox.Show(message + "\n\n" + e.Message, titleAction + " Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public NoteStore() {
            dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AlwaysNote");
            noteFolder = Path.Combine(dataFolder, "Notes");

            NoteNames.CollectionChanged += NoteNames_CollectionChanged;

            LoadNoteNames();
            LoadCurrentNote();
        }

        private void NotifyPropertyChanged(string prop) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private void NoteNames_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            NotifyPropertyChanged("NoteNames");
        }

        /// <remarks>Throws on failure</remarks>
        private void LoadNoteNames() {
            if (!Directory.Exists(noteFolder)) return;

            string[] filePaths = Directory.GetFiles(noteFolder);

            foreach (string filePath in filePaths) {
                if (Path.GetExtension(filePath) != ".txt") continue;
                NoteNames.Add(Path.GetFileNameWithoutExtension(filePath));
            }
        }

        /// <remarks>Throws on failure</remarks>
        private void LoadCurrentNote() {
            if (!File.Exists(Path.Combine(dataFolder, "CurrentNote"))) return;

            currentNote = File.ReadAllText(Path.Combine(dataFolder, "CurrentNote"));
            currentNoteText = GetNoteText(CurrentNote);
        }

        private string GetNotePath(string noteName) {
            return Path.Combine(noteFolder, noteName + ".txt");
        }

        /// <remarks>Throws on failure</remarks>
        private string GetNoteText(string noteName) {
            string path = GetNotePath(noteName);
            return File.Exists(path) ? File.ReadAllText(path) : "";
        }

        /// <remarks>Throws on failure</remarks>
        private void AddNoteThrows(string noteName) {
            if (noteName == "") noteName = GetUnnamedNoteName();

            EnsureFolderExists(noteFolder);
            File.WriteAllText(GetNotePath(noteName), "");

            NoteNames.Add(noteName);
            CurrentNote = noteName;
        }

        /// <remarks>Alerts on failure</remarks>
        public void AddNote(string noteName) {
            if (noteName == "") noteName = GetUnnamedNoteName();

            try {
                AddNoteThrows(noteName);
            } catch (IOException e) {
                ShowError("Could not create note \"" + noteName + "\".", "Create Note", e);
                return;
            }
        }

        /// <remarks>Alerts on IO failures, throws if current note cannot be ensured</remarks>
        public void DeleteNote(string noteName) {
            string path = GetNotePath(noteName);
            if (File.Exists(path)) {
                try {
                    File.Delete(path);
                } catch (IOException e) {
                    ShowError("Could not delete note \"" + noteName + "\".", "Delete Note", e);
                    return;
                }
            }

            NoteNames.Remove(noteName);
            EnsureCurrentNote();
        }

        /// <remarks>Alerts on failure</remarks>
        public void RenameNote(string oldNoteName, string newNoteName) {
            if (newNoteName == "") {
                newNoteName = GetUnnamedNoteName();
            }

            string oldPath = GetNotePath(oldNoteName);
            if (File.Exists(oldPath)) {
                try {
                    File.Move(oldPath, GetNotePath(newNoteName));
                } catch (IOException e) {
                    ShowError("Could not rename note \"" + oldNoteName + "\" to \"" + newNoteName + "\".", "Rename Note", e);
                    return;
                }
            }

            NoteNames[NoteNames.IndexOf(oldNoteName)] = newNoteName;
            CurrentNote = newNoteName;
        }

        /// <remarks>Throws on failure</remarks>
        private void EnsureCurrentNote() {
            if (!NoteNames.Contains(currentNote)) {
                if (NoteNames.Count > 0) {
                    CurrentNote = NoteNames[0];
                } else {
                    AddNoteThrows("Default");
                }
            }
        }

        private string GetUnnamedNoteName() {
            int nextUntitledNum = 1;

            foreach (string name in NoteNames) {
                Match untitledMatch = matchUntitled.Match(name);

                if (!untitledMatch.Success) continue;

                int untitledNum = int.Parse(untitledMatch.Groups[1].Value);

                if (untitledNum + 1 > nextUntitledNum) {
                    nextUntitledNum = untitledNum + 1;
                }
            }

            return "Untitled Note " + nextUntitledNum;
        }

        [GeneratedRegex(@"^Untitled Note (\d+)$")]
        private static partial Regex GetUntitledRegex();
    }
}
