using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace CursorSwitcher
{
    public partial class MainWindow : Window
    {
        private string _selectedStyle1Path = string.Empty;
        private string _selectedStyle2Path = string.Empty;
        private string _selectedHotkey = "F9";
        private bool _isWaitingForHotkey = false;
        
        private List<string> _style1CursorList = new List<string>();
        private List<string> _style2CursorList = new List<string>();
        private int _style1CurrentIndex = 0;
        private int _style2CurrentIndex = 0;
        
        private GlobalMouseHook _mouseHook;
        private GlobalKeyboardHook _keyboardHook;

        public MainWindow()
        {
            InitializeComponent();
            HotkeyBox.Text = _selectedHotkey;
            
            _mouseHook = new GlobalMouseHook();
            _mouseHook.MouseButtonPressed += MouseHook_MouseButtonPressed;

            _keyboardHook = new GlobalKeyboardHook();
            _keyboardHook.KeyPressed += KeyboardHook_KeyPressed;
        }

        private void KeyboardHook_KeyPressed(object sender, GlobalKeyboardHook.KeyPressedEventArgs e)
        {
            if (e.KeyName == _selectedHotkey && _style1CursorList.Count > 0)
            {
                _style1CurrentIndex = (_style1CurrentIndex + 1) % _style1CursorList.Count;
                ApplyCursor(_style1CursorList[_style1CurrentIndex]);
            }
        }

        private void MouseHook_MouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            if (_isWaitingForHotkey)
            {
                string buttonName = GetMouseButtonName(e.Button);
                if (!string.IsNullOrEmpty(buttonName))
                {
                    _selectedHotkey = buttonName;
                    HotkeyBox.Text = buttonName;
                    _isWaitingForHotkey = false;
                }
            }
        }

        private string GetMouseButtonName(int button)
        {
            return button switch
            {
                1 => "MouseLeft",
                2 => "MouseRight",
                3 => "MouseMiddle",
                4 => "Mouse4",
                5 => "Mouse5",
                6 => "Mouse6",
                7 => "Mouse7",
                8 => "Mouse8",
                _ => null
            };
        }

        private void ApplyCursor(string cursorPath)
        {
            try
            {
                if (!File.Exists(cursorPath))
                {
                    MessageBox.Show($"Файл курсора не найден: {cursorPath}", "Ошибка");
                    return;
                }

                CursorManager.SetAllCursorTypes(cursorPath);
                Title = $"CursorSwitcher - {Path.GetFileNameWithoutExtension(cursorPath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при применении курсора: {ex.Message}", "Ошибка");
            }
        }

        private void BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Выберите папку со стилями курсора (.ani/.cur файлы)";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadCursorsFromFolder(dialog.SelectedPath, 1);
            }
        }

        private void BrowseFolder2_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Выберите папку со стилями курсора (.ani/.cur файлы)";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadCursorsFromFolder(dialog.SelectedPath, 2);
            }
        }

        private void LoadCursorsFromFolder(string folderPath, int styleNumber)
        {
            try
            {
                var cursorFiles = Directory.GetFiles(folderPath, "*.*")
                    .Where(f => f.EndsWith(".ani", StringComparison.OrdinalIgnoreCase) || 
                                f.EndsWith(".cur", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(f => Path.GetFileNameWithoutExtension(f))
                    .ToList();

                if (cursorFiles.Count == 0)
                {
                    MessageBox.Show("В папке не найдено .ani или .cur файлов!", "Ошибка");
                    return;
                }

                if (styleNumber == 1)
                {
                    _style1CursorList = cursorFiles;
                    _style1CurrentIndex = 0;
                    _selectedStyle1Path = folderPath;
                    Style1PathBox.Text = $"Папка: {Path.GetFileName(folderPath)} ({cursorFiles.Count} файлов)";
                    MessageBox.Show($"Загружено {cursorFiles.Count} курсоров для стиля 1", "OK");
                }
                else if (styleNumber == 2)
                {
                    _style2CursorList = cursorFiles;
                    _style2CurrentIndex = 0;
                    _selectedStyle2Path = folderPath;
                    Style2PathBox.Text = $"Папка: {Path.GetFileName(folderPath)} ({cursorFiles.Count} файлов)";
                    MessageBox.Show($"Загружено {cursorFiles.Count} курсоров для стиля 2", "OK");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке папки: {ex.Message}", "Ошибка");
            }
        }

        private void BrowseStyle1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Выберите курсор для стиля 1",
                Filter = "Курсоры (*.ani;*.cur)|*.ani;*.cur|Анимированные курсоры (*.ani)|*.ani|Статические курсоры (*.cur)|*.cur|Все файлы (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedStyle1Path = dialog.FileName;
                _style1CursorList = new List<string> { dialog.FileName };
                _style1CurrentIndex = 0;
                Style1PathBox.Text = dialog.FileName;
                MessageBox.Show("Стиль 1 выбран: " + dialog.FileName, "OK");
            }
        }

        private void BrowseStyle2_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Выберите курсор для стиля 2",
                Filter = "Курсоры (*.ani;*.cur)|*.ani;*.cur|Анимированные курсоры (*.ani)|*.ani|Ст��тические курсоры (*.cur)|*.cur|Все файлы (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedStyle2Path = dialog.FileName;
                _style2CursorList = new List<string> { dialog.FileName };
                _style2CurrentIndex = 0;
                Style2PathBox.Text = dialog.FileName;
                MessageBox.Show("Стиль 2 выбран: " + dialog.FileName, "OK");
            }
        }

        private void BindHotkey_Click(object sender, RoutedEventArgs e)
        {
            _isWaitingForHotkey = true;
            HotkeyBox.Text = "Нажмите клавишу или кнопку мыши...";
            HotkeyBox.Focus();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (_isWaitingForHotkey)
            {
                e.Handled = true;
                
                string keyName = GetKeyName(e.Key, e.SystemKey);
                
                _selectedHotkey = keyName;
                HotkeyBox.Text = keyName;
                _isWaitingForHotkey = false;
                
                return;
            }

            base.OnKeyDown(e);
        }

        private string GetKeyName(Key key, Key systemKey)
        {
            switch (key)
            {
                case Key.F1: return "F1";
                case Key.F2: return "F2";
                case Key.F3: return "F3";
                case Key.F4: return "F4";
                case Key.F5: return "F5";
                case Key.F6: return "F6";
                case Key.F7: return "F7";
                case Key.F8: return "F8";
                case Key.F9: return "F9";
                case Key.F10: return "F10";
                case Key.F11: return "F11";
                case Key.F12: return "F12";
                case Key.Space: return "Space";
                case Key.Return: return "Return";
                case Key.Tab: return "Tab";
                case Key.LeftShift: return "LShift";
                case Key.RightShift: return "RShift";
                case Key.LeftCtrl: return "LCtrl";
                case Key.RightCtrl: return "RCtrl";
                case Key.LeftAlt: return "LAlt";
                case Key.RightAlt: return "RAlt";
                default: return key.ToString();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_style1CursorList.Count == 0)
            {
                MessageBox.Show("Ошибка! Сначала выберите курсор или папку для стиля 1!", "Ошибка");
                return;
            }

            string style2Info = _style2CursorList.Count > 0 
                ? $"{_style2CursorList.Count} файлов" 
                : "Windows 11 default";

            MessageBox.Show(
                $"Настройки сохранены:\n\n" +
                $"Стиль 1: {_style1CursorList.Count} файлов\n" +
                $"Стиль 2: {style2Info}\n" +
                $"Горячая клавиша: {_selectedHotkey}",
                "Сохранено");
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            _selectedStyle1Path = string.Empty;
            _selectedStyle2Path = string.Empty;
            _selectedHotkey = "F9";
            _style1CursorList.Clear();
            _style2CursorList.Clear();
            _style1CurrentIndex = 0;
            _style2CurrentIndex = 0;

            Style1PathBox.Text = string.Empty;
            Style2PathBox.Text = "Windows 11 default";
            HotkeyBox.Text = "F9";

            MessageBox.Show("Все настройки сброшены на значения по умолчанию.", "Сброс");
        }

        private void ApplyStyle1_Click(object sender, RoutedEventArgs e)
        {
            if (_style1CursorList.Count == 0)
            {
                MessageBox.Show("Ошибка! Сначала выберите курсор для стиля 1!", "Ошибка");
                return;
            }

            try
            {
                string currentCursor = _style1CursorList[_style1CurrentIndex];
                CursorManager.SetAllCursorTypes(currentCursor);
                Title = $"CursorSwitcher - {Path.GetFileNameWithoutExtension(currentCursor)}";
                MessageBox.Show(
                    $"Применен курсор:\n{Path.GetFileName(currentCursor)}",
                    "Применено");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _mouseHook?.Dispose();
            _keyboardHook?.Dispose();
        }
    }

    // Класс для применения курсора
    public static class CursorManager
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetSystemCursor(IntPtr hcur, uint id);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr LoadCursorFromFile(string lpFileName);

        public static void SetAllCursorTypes(string cursorPath)
        {
            try
            {
                if (!File.Exists(cursorPath))
                {
                    throw new Exception("Файл курсора не найден");
                }

                IntPtr handle = LoadCursorFromFile(cursorPath);
                if (handle == IntPtr.Zero)
                {
                    throw new Exception("Не удалось загрузить курсор из файла");
                }

                uint[] cursorTypes = new uint[]
                {
                    32512, // OCR_NORMAL - Обычный выбор
                    32513, // OCR_IBEAM - Текстовый ввод
                    32514, // OCR_WAIT - Занято
                    32515, // OCR_CROSS - Точный выбор
                    32516, // OCR_UP - Перемещение
                    32640, // OCR_SIZENWSE - Изменение размера NW-SE
                    32641, // OCR_SIZENS - Изменение размера N-S
                    32642, // OCR_SIZEWE - Изменение размера W-E
                    32643, // OCR_SIZENESW - Изменение размера NE-SW
                    32644, // OCR_SIZEALL - Перемещение
                    32645, // OCR_NO - Недоступно
                    32646, // OCR_HAND - Рука/Ссылка
                    32648  // OCR_APPSTARTING - Работаю
                };

                foreach (uint cursorType in cursorTypes)
                {
                    SetSystemCursor(handle, cursorType);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при установке курсора: {ex.Message}");
            }
        }
    }

    public class GlobalKeyboardHook : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private IntPtr _hookHandle = IntPtr.Zero;
        private HookProc _hookProc;

        public class KeyPressedEventArgs : EventArgs
        {
            public string KeyName { get; set; }
        }

        public event EventHandler<KeyPressedEventArgs> KeyPressed;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        private const int WM_KEYDOWN = 0x0100;

        public GlobalKeyboardHook()
        {
            _hookProc = HookCallback;
            _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, IntPtr.Zero, 0);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (int)wParam == WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Key key = KeyInterop.KeyFromVirtualKey(vkCode);
                string keyName = key.ToString();

                KeyPressed?.Invoke(this, new KeyPressedEventArgs { KeyName = keyName });
            }

            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            if (_hookHandle != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookHandle);
                _hookHandle = IntPtr.Zero;
            }
        }
    }

    public class GlobalMouseHook : IDisposable
    {
        private const int WH_MOUSE_LL = 14;
        private IntPtr _hookHandle = IntPtr.Zero;
        private HookProc _hookProc;

        public event EventHandler<MouseButtonEventArgs> MouseButtonPressed;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_XBUTTONDOWN = 0x020B;

        public GlobalMouseHook()
        {
            _hookProc = HookCallback;
            _hookHandle = SetWindowsHookEx(WH_MOUSE_LL, _hookProc, IntPtr.Zero, 0);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int msg = (int)wParam;
                int button = 0;

                switch (msg)
                {
                    case WM_LBUTTONDOWN:
                        button = 1;
                        break;
                    case WM_RBUTTONDOWN:
                        button = 2;
                        break;
                    case WM_MBUTTONDOWN:
                        button = 3;
                        break;
                    case WM_XBUTTONDOWN:
                    {
                        MSLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                        int xButton = (int)((hookStruct.mouseData >> 16) & 0xFFFF);
                        button = xButton == 1 ? 4 : (xButton == 2 ? 5 : 0);
                        break;
                    }
                }

                if (button > 0)
                {
                    MouseButtonPressed?.Invoke(this, new MouseButtonEventArgs { Button = button });
                }
            }

            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            if (_hookHandle != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookHandle);
                _hookHandle = IntPtr.Zero;
            }
        }
    }

    public class MouseButtonEventArgs : EventArgs
    {
        public int Button { get; set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }
}
