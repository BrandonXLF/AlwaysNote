use crate::ui::*;

pub fn init(win: &MainWindow) {
    let adapter = win.global::<MainInputAdapter>();

    adapter.on_tab_pressed({
        let win_weak = win.as_weak();

        move || {
            let win = win_weak.unwrap();
            let mut text: String = win.get_text().into();

            let anchor = win.get_main_anchor_byte_pos() as usize;
            let cursor = win.get_main_cursor_byte_pos() as usize;

            let (start, end) = if anchor < cursor {
                (anchor, cursor)
            } else {
                (cursor, anchor)
            };

            text.replace_range(start.min(end)..start.max(end), "\t");
            win.invoke_set_text(text.into());

            let new_start = if start == end { start + 1 } else { start };
            win.invoke_set_selection_offsets(new_start as i32, (start + 1) as i32);
        }
    });
}
