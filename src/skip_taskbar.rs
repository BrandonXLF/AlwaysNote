use winsafe::{
    co::{CLSCTX, CLSID},
    prelude::{shell_ITaskbarList, user_Hwnd},
    CoCreateInstance, ITaskbarList, HWND,
};

pub fn skip_taskbar_in_loop() {
    if let Some(hwnd) = HWND::GetActiveWindow() {
        if let Ok(taskbar) =
            CoCreateInstance::<ITaskbarList>(&CLSID::TaskbarList, None, CLSCTX::INPROC_SERVER)
        {
            let _ = taskbar.HrInit().and_then(|_| taskbar.DeleteTab(&hwnd));
        }
    }
}

pub fn skip_taskbar() {
    #[cfg(target_os = "windows")]
    let _ = slint::invoke_from_event_loop(|| {
        let _ = skip_taskbar_in_loop();
    });
}
