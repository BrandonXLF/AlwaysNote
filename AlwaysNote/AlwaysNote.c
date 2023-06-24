#include <windows.h>
#include <tchar.h>

#define ID_EMPTY_BIN 40057
#define ID_EXIT_PROGRAM 40058
#define ID_OPEN_BIN 40059
#define ID_BIN_PROPERTIES 40060
#define ID_ENABLE_HIDE 40061
#define ID_DISABLE_HIDE 40062
#define ID_HOTKEY 5643242
#define WM_ICON_NOTIFY 34592

const wchar_t CLASS_NAME[] = L"AlwaysNoteIconWindow";
const wchar_t TOOLTIP[] = L"AlwaysNote\nWin + Shift + A";

NOTIFYICONDATA iconData;
HANDLE alwaysNoteProcess;

void CreateNotepad(BOOL toggle) {
    if (alwaysNoteProcess && WaitForSingleObject(alwaysNoteProcess, 0) != WAIT_OBJECT_0) {
        if (!toggle) return;

        TerminateProcess(alwaysNoteProcess, 0);
        return;
    }

    STARTUPINFO si;
    ZeroMemory(&si, sizeof(si));
    si.cb = sizeof(si);

    PROCESS_INFORMATION pi;
    ZeroMemory(&pi, sizeof(pi));

    CreateProcess(NULL, _tcsdup(L"AlwaysNoteNotepad.exe"), NULL, NULL, TRUE, 0, NULL, NULL, &si, &pi);

    alwaysNoteProcess = pi.hProcess;
    CloseHandle(pi.hThread);
}

LRESULT CALLBACK WindowProc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam) {
    switch (msg) {
        case WM_DESTROY:
            Shell_NotifyIcon(NIM_DELETE, &iconData);
            PostQuitMessage(0);
            break;
        case WM_CLOSE:
            DestroyWindow(hwnd);
            break;
        case WM_COMMAND:
            switch (wparam) {
                case ID_EXIT_PROGRAM:
                    DestroyWindow(hwnd);
                    break;
                case ID_OPEN_BIN:
                    CreateNotepad(FALSE);
                    break;
            }

            break;
        case WM_HOTKEY:
            CreateNotepad(TRUE);

            break;
        case WM_ICON_NOTIFY:
            switch (lparam) {
                case WM_LBUTTONUP:
                    CreateNotepad(TRUE);
                    break;
                case WM_RBUTTONUP:
                case WM_CONTEXTMENU: {
                    POINT lpClickPoint;
                    GetCursorPos(&lpClickPoint);

                    HMENU hMenu = CreatePopupMenu();

                    AppendMenu(hMenu, MF_STRING, ID_OPEN_BIN, L"Open");
                    AppendMenu(hMenu, MF_STRING, ID_EXIT_PROGRAM, L"Exit");

                    SetMenuDefaultItem(hMenu, ID_OPEN_BIN, FALSE);
                    SetForegroundWindow(hwnd);
                    TrackPopupMenu(hMenu, TPM_LEFTALIGN | TPM_TOPALIGN, lpClickPoint.x, lpClickPoint.y, 0, hwnd, NULL);
                    DestroyMenu(hMenu);

                    break;
                }
            }

            break;
    }

    return DefWindowProc(hwnd, msg, wparam, lparam);
}

int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPWSTR lpCmdLine, int nCmdShow) {
    CreateMutex(NULL, TRUE, L"eb9bed52-161c-4a2f-bc0f-e50da0a7aab6");

    if (GetLastError() == ERROR_ALREADY_EXISTS)
        return 0;
    
    HICON icon = (HICON)LoadImage(hInstance, MAKEINTRESOURCE(101), IMAGE_ICON, 24, 24, NULL);
    
    WNDCLASS winClass;
    ZeroMemory(&winClass, sizeof(winClass));
    winClass.hInstance = hInstance;
    winClass.lpszClassName = CLASS_NAME;
    winClass.lpfnWndProc = WindowProc;
    winClass.hIcon = icon;
    RegisterClass(&winClass);

    HWND hwnd = CreateWindowEx(NULL, CLASS_NAME, NULL, CW_USEDEFAULT, 0, 0, 0, 0, HWND_MESSAGE, NULL, hInstance, NULL);

    ZeroMemory(&iconData, sizeof(iconData));
    iconData.cbSize = sizeof(iconData);
    iconData.hIcon = icon;
    iconData.hWnd = hwnd;
    iconData.uID = WM_ICON_NOTIFY;
    iconData.uCallbackMessage = WM_ICON_NOTIFY;
    iconData.uFlags = NIF_ICON | NIF_TIP | NIF_MESSAGE;
    wcscpy_s(iconData.szTip, sizeof(iconData.szTip) / sizeof(WCHAR), TOOLTIP);

    Shell_NotifyIcon(NIM_ADD, &iconData);

    if (wcspbrk(lpCmdLine, L"--minimized") == NULL)
        CreateNotepad(FALSE);

    RegisterHotKey(hwnd, ID_HOTKEY, MOD_WIN | MOD_SHIFT, 'A'); // Win + Shift + A

    MSG msg;

    while (GetMessage(&msg, NULL, NULL, NULL)) {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }

    if (alwaysNoteProcess) {
        TerminateProcess(alwaysNoteProcess, 0);
        CloseHandle(alwaysNoteProcess);
    }

    return 0;
}