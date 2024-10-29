use std::thread;

use windows_hotkeys::{
    keys::{ModKey, VKey},
    HotkeyManager, HotkeyManagerImpl,
};

use crate::{win_manipulator::WindowManipulator, ui::*};

pub fn add(win_weak: slint::Weak<MainWindow>) {
    thread::spawn(|| {
        let mut hkm = HotkeyManager::new();

        hkm.register(VKey::A, &[ModKey::Win, ModKey::Shift], move || {
            win_weak.toggle();
        })
        .unwrap();

        hkm.event_loop();
    });
}
