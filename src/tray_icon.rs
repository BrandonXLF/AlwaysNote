use std::{process, thread};

use trayicon::{Icon, MenuBuilder, TrayIconBuilder};

use crate::notepad::NotepadManager;

#[derive(Clone, Eq, PartialEq, Debug)]
enum TrayIconEvent {
    Toggle,
    Open,
    Exit,
}

pub fn show(notepad_manager: NotepadManager) {
    let icon_bytes = include_bytes!("../images/icon.ico");
    let icon = Icon::from_buffer(icon_bytes, None, None).unwrap();
    let (s, r) = crossbeam_channel::unbounded();

    let tray_icon = TrayIconBuilder::new()
        .sender_crossbeam(s)
        .icon(icon)
        .tooltip("AlwaysNote\nWin + Shift + A")
        .on_click(TrayIconEvent::Toggle)
        .menu(
            MenuBuilder::new()
                .item("Open", TrayIconEvent::Open)
                .item("Exit", TrayIconEvent::Exit),
        )
        .build()
        .unwrap();

    thread::spawn(move || {
        let _tray_icon = tray_icon;

        loop {
            if let Ok(event) = r.recv() {
                match event {
                    TrayIconEvent::Exit => process::exit(0),
                    TrayIconEvent::Open => notepad_manager.ensure(),
                    TrayIconEvent::Toggle => notepad_manager.toggle(),
                }
            }
        }
    });
}
