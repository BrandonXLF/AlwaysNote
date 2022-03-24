using System;
using System.Text.RegularExpressions;
using System.IO;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace AlwaysNote {
    class NoteStore : INotifyPropertyChanged {
        private readonly string dataFolder;
        private readonly string noteFolder;
        private readonly Regex matchUntitled = new(@"^Untitled Note (\d+)$");
        private string currentNote;

        public readonly ObservableCollection<string> NoteNames = new();
        public event PropertyChangedEventHandler PropertyChanged;

        public String CurrentNote {
            get {
                EnsureCurrentNote();

                return currentNote;
            }

            set {
                currentNote = value;

                NotifyPropertyChanged("CurrentNote");
                NotifyPropertyChanged("CurrentNoteText");

                if (!Directory.Exists(dataFolder)) {
                    Directory.CreateDirectory(dataFolder);
                }

                File.WriteAllTextAsync(Path.Combine(dataFolder, "CurrentNote"), value);
            }
        }

        public String CurrentNoteText {
            get {
                return GetNoteText(CurrentNote);
            }

            set {
                SetNoteText(CurrentNote, value);
                NotifyPropertyChanged("CurrentNoteText");
            }
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

        public void LoadNoteNames() {
            if (!Directory.Exists(noteFolder)) {
                return;
            }

            string[] filePaths = Directory.GetFiles(noteFolder);

            foreach (string filePath in filePaths) {
                if (Path.GetExtension(filePath) != ".txt") {
                    continue;
                }

                NoteNames.Add(Path.GetFileNameWithoutExtension(filePath));
            }
        }

        public void LoadCurrentNote() {
            if (!File.Exists(Path.Combine(dataFolder, "CurrentNote"))) {
                return;
            }

            currentNote = File.ReadAllText(Path.Combine(dataFolder, "CurrentNote"));
        }

        public int GetNextUntitledNum() {
            int nextUntitledNum = 1;

            foreach (string noteName in NoteNames) {
                Match untitledMatch = matchUntitled.Match(noteName);

                if (!untitledMatch.Success) {
                    continue;
                }

                int untitledNum = int.Parse(untitledMatch.Groups[1].Value, System.Globalization.NumberStyles.None);

                if (untitledNum + 1 > nextUntitledNum) {
                    nextUntitledNum = untitledNum + 1;
                }
            }

            return nextUntitledNum;
        }

        public string GetNotePath(string noteName) {
            return Path.Combine(noteFolder, noteName + ".txt"); ;
        }

        public string GetNoteText(string noteName) {
            string path = GetNotePath(noteName);

            return File.Exists(path) ? File.ReadAllText(path) : "";
        }

        public void SetNoteText(string noteName, string noteText) {
            if (!Directory.Exists(noteFolder)) {
                Directory.CreateDirectory(noteFolder);
            }

            File.WriteAllTextAsync(GetNotePath(noteName), noteText);
        }

        public void AddNote(string noteName) {
            if (noteName == "") {
                noteName = GetUnnamedNoteName();
            }

            NoteNames.Add(noteName);
            SetNoteText(noteName, "");
        }

        public void DeleteNote(string noteName) {
            NoteNames.Remove(noteName);

            EnsureCurrentNote();

            string path = GetNotePath(noteName);

            if (File.Exists(path)) {
                File.Delete(path);
            }
        }

        public void RenameNote(string oldNoteName, string newNoteName) {
            if (newNoteName == "") {
                newNoteName = GetUnnamedNoteName();
            }

            if (File.Exists(GetNotePath(oldNoteName))) {
                File.Move(GetNotePath(oldNoteName), GetNotePath(newNoteName));
            }

            NoteNames[NoteNames.IndexOf(oldNoteName)] = newNoteName;
            CurrentNote = newNoteName;
        }

        private void EnsureCurrentNote() {
            if (!NoteNames.Contains(currentNote)) {
                if (NoteNames.Count > 0) {
                    CurrentNote = NoteNames[0];
                } else {
                    AddNote("Default");
                    CurrentNote = "Default";
                }
            }
        }

        private string GetUnnamedNoteName() {
            int nextUntitledNum = 1;

            foreach (string name in NoteNames) {
                Match untitledMatch = matchUntitled.Match(name);

                if (!untitledMatch.Success) {
                    continue;
                }

                int untitledNum = int.Parse(untitledMatch.Groups[1].Value);

                if (untitledNum + 1 > nextUntitledNum) {
                    nextUntitledNum = untitledNum + 1;
                }
            }

            return "Untitled Note " + nextUntitledNum;
        }
    }
}
