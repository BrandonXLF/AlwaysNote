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
        let empty_string = "".into();
        let switch = args.get(1).unwrap_or(&empty_string);

        if switch != "--minimized" {
            notepad_manager.ensure();
        }
    }

    // TODO: workaround for https://github.com/slint-ui/slint/issues/1499
    i_slint_backend_selector::with_platform(|b| {
        b.set_event_loop_quit_on_last_window_closed(false);
        Ok(())
    })
    .unwrap();

    slint::run_event_loop().unwrap();
}
