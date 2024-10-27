#![windows_subsystem = "windows"]

mod color_palette;
mod find_replace;
mod hotkey;
mod note_model;
mod note_store;
mod notepad;
mod store_adapter;
mod tray_icon;
mod ui;

use std::env;

use notepad::NotepadManager;

fn main() {
    let (notepad_manager, _win, _color_holder) = NotepadManager::new();

    hotkey::add(notepad_manager.clone());
    tray_icon::show(notepad_manager.clone());

    {
        let args: Vec<String> = env::args().collect();
        let switch: &str = args.get(1).map_or("", |x| x.as_str());

        if switch != "--minimized" {
            notepad_manager.ensure();
        }
    }

    slint::run_event_loop_until_quit().unwrap();
}
