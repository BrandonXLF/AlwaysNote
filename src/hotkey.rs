use std::thread;

use windows_hotkeys::{
    keys::{ModKey, VKey},
    HotkeyManager, HotkeyManagerImpl,
};

use crate::notepad::NotepadManager;

pub fn add(notepad_manager: NotepadManager) {
    thread::spawn(|| {
        let mut hkm = HotkeyManager::new();

        hkm.register(VKey::A, &[ModKey::Win, ModKey::Shift], move || {
            notepad_manager.toggle();
        })
        .unwrap();

        hkm.event_loop();
    });
}
