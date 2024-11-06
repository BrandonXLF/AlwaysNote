use i_slint_core::items::{TextHorizontalAlignment, TextVerticalAlignment};

use crate::{
    color_palette::{self, ColorValueEventHolder},
    find_replace,
    note_model::NoteModel,
    note_store::NoteStore,
    store_adapter,
    ui::*,
};

pub struct Notepad {
    pub win: MainWindow,
    _color_holder: ColorValueEventHolder,
}

impl Notepad {
    pub fn new() -> Notepad {
        let win = MainWindow::new().unwrap();

        win.on_move_window({
            let win_weak = win.as_weak();

            move |hoz_pos, mut dx, vert_pos, mut dy| {
                let win = win_weak.unwrap();
                let sys_win = win.window();

                if hoz_pos == TextHorizontalAlignment::Left
                    || vert_pos == TextVerticalAlignment::Top
                {
                    let logical_pos = sys_win
                        .position()
                        .to_logical(win_weak.unwrap().window().scale_factor());

                    let mut pos_x = logical_pos.x;
                    let mut pos_y = logical_pos.y;

                    if hoz_pos == TextHorizontalAlignment::Left {
                        pos_x += dx;
                        dx = -dx;
                    }

                    if vert_pos == TextVerticalAlignment::Top {
                        pos_y += dy;
                        dy = -dy;
                    }

                    sys_win.set_position(slint::LogicalPosition::new(pos_x, pos_y));
                }

                let logical_size = sys_win
                    .size()
                    .to_logical(win_weak.unwrap().window().scale_factor());

                sys_win.set_size(slint::LogicalSize::new(
                    logical_size.width + dx,
                    logical_size.height + dy,
                ));
            }
        });

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

        let color_holder = color_palette::init(&win);
        find_replace::init(&win);
        store_adapter::init(&win);

        win.set_note_names(NoteModel::rc_from_saved(&win).into());
        win.invoke_set_current_note(NoteStore::get_current_note().into());

        Notepad {
            win,
            _color_holder: color_holder,
        }
    }
}
