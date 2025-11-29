#![windows_subsystem = "windows"]

mod color_palette;
mod find_replace;
mod hotkey;
mod main_input;
mod note_model;
mod note_store;
mod notepad;
mod skip_taskbar;
mod store_adapter;
mod tray_icon;
mod ui;
mod win_manipulator;

use std::env;

use notepad::Notepad;
use slint::ComponentHandle;
use win_manipulator::WindowManipulator;

fn main() {
    let notepad = Notepad::new();

    hotkey::add(notepad.win.as_weak());
    tray_icon::show(notepad.win.as_weak());

    {
        let switch = env::args().nth(1).unwrap_or_else(|| "".into());

        if switch != "--minimized" {
            notepad.win.ensure();
        }
    }

    slint::run_event_loop_until_quit().unwrap();
}
