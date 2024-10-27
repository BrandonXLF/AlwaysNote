use std::{process, thread};

use trayicon::{Icon, MenuBuilder, TrayIconBuilder};

use crate::{win_manipulator::WindowManipulator, ui::*};

#[derive(Clone, Eq, PartialEq, Debug)]
enum TrayIconEvent {
    Toggle,
    Open,
    Exit,
    Menu,
}

pub fn show(win_weak: slint::Weak<MainWindow>) {
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
                    TrayIconEvent::Open => win_weak.ensure(),
                    TrayIconEvent::Toggle => win_weak.toggle(),
                    // TODO: Handle, user cannot close the program otherwise
                    TrayIconEvent::Menu => { let _ = tray_icon.show_menu(); }
                }
            }
        }
    });
}
