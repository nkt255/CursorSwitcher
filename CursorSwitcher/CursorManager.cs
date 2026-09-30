using System;
using System.Runtime.InteropServices;

namespace CursorSwitcher
{
    public class CursorManager
    {
        public bool ApplyCustomCursor(string cursorPath)
        {
            if (!System.IO.File.Exists(cursorPath) || !cursorPath.EndsWith(".ani", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var hCursor = NativeMethods.LoadCursorFromFile(cursorPath);
            if (hCursor == IntPtr.Zero)
            {
                return false;
            }

            NativeMethods.SetSystemCursor(hCursor, NativeMethods.OCR_NORMAL);
            return true;
        }

        public void ApplyWindowsDefault()
        {
            var defaultCursor = NativeMethods.LoadCursor(IntPtr.Zero, (int)NativeMethods.IDC_ARROW);
            if (defaultCursor != IntPtr.Zero)
            {
                NativeMethods.SetSystemCursor(defaultCursor, NativeMethods.OCR_NORMAL);
            }
        }
    }

    public static class NativeMethods
    {
        public const uint OCR_NORMAL = 32512;
        public const int IDC_ARROW = 32512;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadCursorFromFile(string lpFileName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetSystemCursor(IntPtr hcur, uint id);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
    }
}
