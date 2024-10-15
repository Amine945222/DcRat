using System;
using System.Diagnostics;
using System.Text;

namespace Plugin.Handler
{
    public class HandleTaskbar
    {
        private const string VistaStartMenuCaption = "Start";
        private static IntPtr vistaStartMenuWnd = IntPtr.Zero;


        public void Show()
        {
            try
            {
                SetVisibility(true);
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }
        }

        public void Hide()
        {
            try
            {
                SetVisibility(false);
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }
        }

        private static void SetVisibility(bool show)
        {
            var taskBarWnd = Native.FindWindow("Shell_TrayWnd", null);

            var startWnd = Native.FindWindowEx(IntPtr.Zero, IntPtr.Zero, (IntPtr)0xC017, "Start");


            if (startWnd == IntPtr.Zero)
            {
                startWnd = Native.FindWindow("Button", null);

                if (startWnd == IntPtr.Zero) startWnd = GetVistaStartMenuWnd(taskBarWnd);
            }

            Native.ShowWindow(taskBarWnd, show ? ShowWindowCommands.Show : ShowWindowCommands.Hide);
            Native.ShowWindow(startWnd, show ? ShowWindowCommands.Show : ShowWindowCommands.Hide);
        }

        private static IntPtr GetVistaStartMenuWnd(IntPtr taskBarWnd)
        {
            uint procId;
            Native.GetWindowThreadProcessId(taskBarWnd, out procId);

            var p = Process.GetProcessById((int)procId);

            foreach (ProcessThread t in p.Threads) Native.EnumThreadWindows(t.Id, MyEnumThreadWindowsProc, IntPtr.Zero);

            return vistaStartMenuWnd;
        }

        private static bool MyEnumThreadWindowsProc(IntPtr hWnd, IntPtr lParam)
        {
            var buffer = new StringBuilder(256);
            if (Native.GetWindowText(hWnd, buffer, buffer.Capacity) > 0)
                if (buffer.ToString() == VistaStartMenuCaption)
                {
                    vistaStartMenuWnd = hWnd;
                    return false;
                }

            return true;
        }
    }
}