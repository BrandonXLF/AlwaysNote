use std::{cell::RefCell, rc::Rc};

use slint::{Model, ModelNotify, ModelTracker, SharedString};

use crate::{
    note_store::{NoteStore, DEFAULT_NOTE_NAME},
    ui::*,
};

pub struct NoteModel {
    win_weak: slint::Weak<MainWindow>,
    notes: RefCell<Vec<SharedString>>,
    notify: ModelNotify,
}

impl NoteModel {
    pub fn rc_from_saved(win: &MainWindow) -> Rc<Self> {
        let model = Rc::from(NoteModel {
            win_weak: win.as_weak(),
            notes: RefCell::from(
                NoteStore::get_note_list()
                    .iter()
                    .map(SharedString::from)
                    .collect::<Vec<SharedString>>(),
            ),
            notify: Default::default(),
        });

        win.on_rename({
            let model = model.clone();

            move |old_name, new_name| {
                model.rename(old_name, new_name);
            }
        });

        win.on_delete({
            let model = model.clone();

            move |delete_name| {
                model.delete(delete_name);
            }
        });

        model
    }

    fn insert(&self, index: usize, value: SharedString) {
        self.notes.borrow_mut().insert(index, value);
        self.notify.row_added(index, 1);
    }

    fn remove(&self, index: usize) {
        self.notes.borrow_mut().remove(index);
        self.notify.row_removed(index, 1);
    }

    pub fn rename(&self, old_name: SharedString, new_name: SharedString) {
        if new_name == "" {
            return;
        }

        let win = self.win_weak.unwrap();

        let new_index = self
            .iter()
            .position(|name| name >= &new_name)
            .unwrap_or(self.row_count());

        if let Some(current_at_index) = self.row_data(new_index) {
            if current_at_index == new_name {
                win.invoke_set_current_note(new_name);
                return;
            }
        }

        self.insert(new_index, new_name.clone());

        if old_name == "" {
            NoteStore::set_note_text(new_name.as_str(), "");
        } else {
            if let Some(old_index) = self.iter().position(|name| name == &old_name) {
                self.remove(old_index);
            }

            NoteStore::rename_note(old_name.as_str(), new_name.as_str());
        }

        win.invoke_set_current_note(new_name);
    }

    pub fn delete(&self, delete_name: SharedString) {
        let win = self.win_weak.unwrap();

        if let Some(old_index) = self.iter().position(|name| name == &delete_name) {
            self.remove(old_index);

            let row_count = self.row_count();

            let new_current = if old_index < row_count {
                self.row_data(old_index).unwrap()
            } else if row_count > 0 {
                self.row_data(row_count - 1).unwrap()
            } else {
                let new_note: SharedString = DEFAULT_NOTE_NAME.into();
                self.insert(0, new_note.clone());
                new_note
            };

            win.invoke_set_current_note(new_current);
        }

        NoteStore::delete_note(delete_name.as_str());
    }
}

impl Model for NoteModel {
    type Data = SharedString;

    fn row_count(&self) -> usize {
        return self.notes.borrow().len();
    }

    fn row_data(&self, row: usize) -> Option<Self::Data> {
        return self.notes.borrow().get(row).cloned();
    }

    fn model_tracker(&self) -> &dyn ModelTracker {
        &self.notify
    }
}
