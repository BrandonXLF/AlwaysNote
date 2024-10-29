use crate::ui::*;

pub trait WindowManipulator {
    fn ensure(&self);
    fn toggle(&self);
}

impl WindowManipulator for MainWindow {
    fn ensure(&self) {
        let _ = self.show();
    }

    fn toggle(&self) {
        let _ = if self.window().is_visible() {
            self.hide()
        } else {
            self.show()
        };
    }
}

impl WindowManipulator for slint::Weak<MainWindow> {
    fn ensure(&self) {
        let _ = self.upgrade_in_event_loop(move |win| {
            win.show().unwrap();
        });
    }

    fn toggle(&self) {
        let _ = self.upgrade_in_event_loop(move |win| {
            if win.window().is_visible() {
                win.hide().unwrap();
            } else {
                win.show().unwrap();
            }
        });
    }
}
