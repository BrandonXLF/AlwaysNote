use crate::ui::*;

fn change_index(win: &MainWindow, change: i32) {
    let adapter = win.global::<FindReplaceAdapter>();

    let i = adapter.get_current_index();
    let count = adapter.get_match_count();

    let new_i = if count == 0 {
        0
    } else {
        (i + change).rem_euclid(count)
    };

    adapter.set_current_index(new_i);

    let find = adapter.get_find_text();
    let haystack: String = win.get_text().into();
    let matches: Vec<(usize, &str)> = haystack.match_indices(find.as_str()).collect();
    let start = matches[new_i as usize];

    println!("{} {} {}", start.0, start.1, start.1.len());
}

pub fn init(win: &MainWindow) {
    let adapter = win.global::<FindReplaceAdapter>();

    adapter.on_prev_pressed({
        let win_weak = win.as_weak();

        move || {
            change_index(&win_weak.unwrap(), -1);
        }
    });

    adapter.on_next_pressed({
        let win_weak = win.as_weak();

        move || {
            change_index(&win_weak.unwrap(), 1);
        }
    });

    adapter.on_one_pressed({
        let win_weak = win.as_weak();

        move || {
            let win = win_weak.unwrap();
            let adapter = win.global::<FindReplaceAdapter>();

            let find = adapter.get_find_text();
            let replace = adapter.get_replace_text();

            let mut haystack: String = win.get_text().into();
            let matches: Vec<(usize, &str)> = haystack.match_indices(find.as_str()).collect();
            let start = matches[adapter.get_current_index() as usize];

            haystack.replace_range(start.0..start.0 + start.1.len(), replace.as_str());
            win.invoke_set_text(haystack.into());
        }
    });

    adapter.on_all_pressed({
        let win_weak = win.as_weak();

        move || {
            let win = win_weak.unwrap();
            let adapter = win.global::<FindReplaceAdapter>();

            let haystack = win.get_text();
            let find = adapter.get_find_text();
            let replace = adapter.get_replace_text();

            win.invoke_set_text(haystack.replace(find.as_str(), replace.as_str()).into());
        }
    });

    adapter.on_text_edited({
        let win_weak = win.as_weak();

        move || {
            let win = win_weak.unwrap();
            let adapter = win.global::<FindReplaceAdapter>();
            let find = adapter.get_find_text();

            if find.len() > 0 {
                adapter.invoke_find_edited(find);
            }
        }
    });

    adapter.on_find_edited({
        let win_weak = win.as_weak();

        move |needle| {
            let win = win_weak.unwrap();
            let adapter = win.global::<FindReplaceAdapter>();

            let haystack = win.get_text();
            let matches: Vec<(usize, &str)> = haystack.match_indices(needle.as_str()).collect();

            adapter.set_current_index(0);
            adapter.set_match_count(matches.len() as i32);
        }
    });
}