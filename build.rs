fn main() {
    slint_build::compile("ui/main_window.slint").unwrap();
    windres::Build::new().compile("src/resources.rc").unwrap();
}
