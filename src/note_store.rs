use std::{collections::BinaryHeap, fs, path::PathBuf};

pub const DEFAULT_NOTE_NAME: &str = "Default";

pub struct NoteStore {}

impl NoteStore {
    fn get_path(parts: &[&str]) -> Option<PathBuf> {
        let mut path = dirs::config_dir()?.to_owned();

        path.push("AlwaysNote");

        for part in parts {
            path.push(part);
        }

        return Some(path);
    }

    fn get_notes_from_dir() -> Vec<String> {
        let notes_dir = Self::get_path(&["notes"]);

        if notes_dir.is_none() {
            return vec![];
        }

        let contents = fs::read_dir(notes_dir.unwrap());

        match contents {
            Ok(files) => {
                let mut out = BinaryHeap::new();

                for file in files {
                    if let Ok(file) = file {
                        let is_file = file.file_type().map(|file_type| file_type.is_file());
                        let raw_name = file.file_name();
                        let name = raw_name.to_string_lossy();

                        if is_file.is_ok() && is_file.unwrap() && name.ends_with(".txt") {
                            out.push(name[0..name.len() - 4].into());
                        }
                    }
                }

                out.into_sorted_vec()
            }
            Err(_) => vec![],
        }
    }

    pub fn get_note_list() -> Vec<String> {
        let mut list = Self::get_notes_from_dir();
        let current = Self::get_current_note();

        match list.binary_search(&current) {
            Ok(_) => {}
            Err(pos) => list.insert(pos, current),
        }

        list
    }

    fn get_note_path(name: &str) -> Option<PathBuf> {
        let file_name = name.to_owned() + ".txt";
        Self::get_path(&["notes", &file_name])
    }

    fn read_rel_path(parts: &[&str], default: &str) -> String {
        let info_path = Self::get_path(parts);

        if info_path.is_none() {
            return default.to_owned();
        }

        let maybe_bytes = fs::read(info_path.unwrap());

        match maybe_bytes {
            Ok(bytes) => String::from_utf8_lossy(&bytes).into_owned(),
            Err(_) => default.to_owned(),
        }
    }

    fn write_rel_path(parts: &[&str], content: &str) {
        let path = Self::get_path(parts);

        if let Some(path) = path {
            if let Some(parent) = path.parent() {
                let _ = fs::create_dir_all(parent);
            }

            let _ = fs::write(path, content);
        }
    }

    pub fn get_current_note() -> String {
        Self::read_rel_path(&["CurrentNote"], DEFAULT_NOTE_NAME)
    }

    pub fn set_current_note(name: &str) {
        Self::write_rel_path(&["CurrentNote"], name);
    }

    pub fn get_note_text(name: &str) -> String {
        let file_name = name.to_owned() + ".txt";
        Self::read_rel_path(&["notes", &file_name], "")
    }

    pub fn set_note_text(name: &str, content: &str) {
        let file_name = name.to_owned() + ".txt";
        Self::write_rel_path(&["notes", &file_name], content);
    }

    pub fn rename_note(old_name: &str, new_name: &str) -> Option<()> {
        let _ = std::fs::rename(
            Self::get_note_path(old_name)?,
            Self::get_note_path(new_name)?,
        );

        Some(())
    }

    pub fn delete_note(name: &str) -> Option<()> {
        let _ = std::fs::remove_file(Self::get_note_path(name)?);

        Some(())
    }
}
