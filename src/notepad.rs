use crate::{
    color_palette::{self, ColorValueEventHolder},
    find_replace,
    note_model::NoteModel,
    note_store::NoteStore,
    store_adapter,
    ui::*,
};
use i_slint_backend_winit::{winit::platform::windows::WindowExtWindows, WinitWindowAccessor};

#[derive(Clone)]
pub struct NotepadManager {
    pub win_weak: slint::Weak<MainWindow>,
}

impl NotepadManager {
    pub fn new() -> (NotepadManager, MainWindow, ColorValueEventHolder) {
        let win = MainWindow::new().unwrap();

        win.on_close({
            let win_weak = win.as_weak();
            move || win_weak.unwrap().window().hide().unwrap()
        });

        win.on_mouse_move({
            let win_weak = win.as_weak();

            move |dx, dy| {
                let win = win_weak.unwrap();
                let sys_win = win.window();

                let logical_pos = sys_win
                    .position()
                    .to_logical(win_weak.unwrap().window().scale_factor());

                sys_win.set_position(slint::LogicalPosition::new(
                    logical_pos.x + dx,
                    logical_pos.y + dy,
                ));
            }
        });

        // TODO: workaround for https://github.com/slint-ui/slint/issues/4341
        win.window().with_winit_window(|winit_win| {
            winit_win.set_skip_taskbar(true);
        });

        let color_holder = color_palette::init(&win);
        find_replace::init(&win);
        store_adapter::init(&win);

        win.set_note_names(NoteModel::rc_from_saved(&win).into());
        win.invoke_set_current_note(NoteStore::get_current_note().into());

        (
            NotepadManager {
                win_weak: win.as_weak(),
            },
            win,
            color_holder,
        )
    }

    pub fn ensure(&self) {
        let _ = self.win_weak.upgrade_in_event_loop(move |win| {
            win.show().unwrap();
        });
    }

    pub fn toggle(&self) {
        let _ = self.win_weak.upgrade_in_event_loop(move |win| {
            if win.window().is_visible() {
                win.hide().unwrap();
            } else {
                win.show().unwrap();
            }
        });
    }
}
