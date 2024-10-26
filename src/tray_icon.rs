use std::{process, thread};

use trayicon::{Icon, MenuBuilder, TrayIconBuilder};

use crate::notepad::NotepadManager;

#[derive(Clone, Eq, PartialEq, Debug)]
enum TrayIconEvent {
    Toggle,
    Open,
    Exit,
    Menu,
}

pub fn show(notepad_manager: NotepadManager) {
    let icon_bytes = include_bytes!("../images/icon.ico");
    let icon = Icon::from_buffer(icon_bytes, None, None).unwrap();
    let (s, r) = crossbeam_channel::unbounded();

    let mut tray_icon = TrayIconBuilder::new()
        .sender(move |e: &TrayIconEvent| {
            let _ = s.send(e.clone());
        })
        .icon(icon)
        .tooltip("AlwaysNote\nWin + Shift + A")
        .on_click(TrayIconEvent::Toggle)
        .on_right_click(TrayIconEvent::Menu)
        .menu(
            MenuBuilder::new()
                .item("Open", TrayIconEvent::Open)
                .item("Exit", TrayIconEvent::Exit),
        )
        .build()
        .unwrap();

    thread::spawn(move || {
        loop {
            if let Ok(event) = r.recv() {
                match event {
                    TrayIconEvent::Exit => process::exit(0),
                    TrayIconEvent::Open => notepad_manager.ensure(),
                    TrayIconEvent::Toggle => notepad_manager.toggle(),
                    // TODO: Handle, user cannot close the program otherwise
                    TrayIconEvent::Menu => { let _ = tray_icon.show_menu(); }
                }
            }
        }
    });
}
