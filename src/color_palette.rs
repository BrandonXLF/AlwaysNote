use crate::ui::*;
use slint::{Brush, Color, RgbaColor};
use windows::{
    Foundation::{EventRegistrationToken, TypedEventHandler},
    UI::ViewManagement::*,
};

macro_rules! brush {
    ($a:expr, $r:expr, $g:expr, $b:expr) => {
        Brush::SolidColor(Color::from(RgbaColor {
            alpha: $a,
            red: $r,
            green: $g,
            blue: $b,
        }))
    };
}

pub struct ColorValueEventHolder {
    ui_settings: UISettings,
    token: EventRegistrationToken,
}

impl Drop for ColorValueEventHolder {
    fn drop(&mut self) {
        let _ = self.ui_settings.RemoveColorValuesChanged(self.token);
    }
}

fn apply_theme(ui_settings: &UISettings, win: &MainWindow) {
    let color_palette = win.global::<ColorPalette>();

    let accent = ui_settings.GetColorValue(UIColorType::Accent).unwrap();
    let use_dark_text =
        (accent.R as f32 * 0.299 + accent.G as f32 * 0.587 + accent.B as f32 * 0.114) < 150.0;

    color_palette.set_accent(brush!(accent.A, accent.R, accent.G, accent.B));
    color_palette.set_contrast(if use_dark_text {
        brush!(255, 255, 255, 255)
    } else {
        brush!(255, 0, 0, 0)
    });

    let background: windows::UI::Color =
        ui_settings.GetColorValue(UIColorType::Background).unwrap();
    let is_dark = (background.R as u16 + background.G as u16 + background.B as u16)
        < (255 as u16 * 3 - background.R as u16 - background.G as u16 - background.B as u16);

    if is_dark {
        color_palette.set_theme(brush!(255, 25, 25, 25));
        color_palette.set_opposite(brush!(255, 255, 255, 255));
    } else {
        color_palette.set_theme(brush!(255, 250, 255, 255));
        color_palette.set_opposite(brush!(255, 0, 0, 0));
    }
}

pub fn init(win: &MainWindow) -> ColorValueEventHolder {
    let ui_settings = UISettings::new().unwrap();

    apply_theme(&ui_settings.clone(), win);

    let token = ui_settings
        .ColorValuesChanged(&TypedEventHandler::new({
            let win_weak = win.as_weak();

            move |sender: &Option<UISettings>, _| {
                if let Some(ui_settings) = &sender {
                    let ui_settings = ui_settings.clone();
                    let _ =
                        &win_weak.upgrade_in_event_loop(move |win| apply_theme(&ui_settings, &win));
                }

                Ok(())
            }
        }))
        .unwrap();

    ColorValueEventHolder { ui_settings, token }
}
