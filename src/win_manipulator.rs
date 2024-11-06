use crate::{
    skip_taskbar::{skip_taskbar, skip_taskbar_in_loop},
    ui::*,
};

pub trait WindowManipulator {
    fn ensure(&self);
    fn toggle(&self);
}

impl WindowManipulator for MainWindow {
    fn ensure(&self) {
        let _ = self.show();
        skip_taskbar();
    }

    fn toggle(&self) {
        if self.window().is_visible() {
            let _ = self.hide();
            return;
        }

        self.ensure();
    }
}

fn ensure_in_loop(win: &MainWindow) {
    win.show().unwrap();
    skip_taskbar_in_loop();
}

impl WindowManipulator for slint::Weak<MainWindow> {
    fn ensure(&self) {
        let _ = self.upgrade_in_event_loop(move |win| {
            ensure_in_loop(&win);
        });
    }

    fn toggle(&self) {
        let _ = self.upgrade_in_event_loop(move |win| {
            if win.window().is_visible() {
                win.hide().unwrap();
                return;
            }

            ensure_in_loop(&win);
        });
    }
}
