use crate::{note_store::NoteStore, ui::*};

pub fn init(win: &MainWindow) {
    let store_adapter = win.global::<NoteStoreAdapter>();

    store_adapter.on_get_text(|name| NoteStore::get_note_text(name.as_str()).into());

    store_adapter.on_set_text(|name, content| {
        NoteStore::set_note_text(name.as_str(), content.as_str());
    });

    store_adapter.on_set_current(|name| {
        NoteStore::set_current_note(name.as_str());
    });
}
