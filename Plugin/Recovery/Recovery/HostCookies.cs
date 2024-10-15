using System;
using System.Collections.Generic;

namespace Plugin
{
    public class HostCookies
    {
        public Cookie[] Cookies { get; set; }

        public string HostName { get; set; }

        public void Print()
        {
            var user = Environment.GetEnvironmentVariable("USERNAME");
            Console.WriteLine("--- Cookie (User: {0}) ---", user);
            Console.WriteLine("Domain         : {0}", HostName);
            Console.WriteLine("Cookies (JSON) :\n{0}", ToJSON());
            Console.WriteLine();
        }


        /*
         * [X] Exception: Object reference not set to an instance of an object.

   at SharpChrome.Cookie.ToJSON()
   at SharpChrome.HostCookies.ToJSON()
   at SharpChrome.HistoricUrl.Print()
   at SharpChrome.Program.Main(String[] args)
[*] Assembly 'SharpChrome' with commands 'history' completed

         */
        public string ToJSON()
        {
            if (Cookies != null && Cookies.Length > 0)
            {
                var jsonCookies = new List<string>();
                //string[] jsonCookies = new string[this.Cookies.Length];
                var j = 0;
                //Console.WriteLine("Cookies length: {0}", this.Cookies.Length);
                for (var i = 0; i < Cookies.Length; i++)
                    //Console.WriteLine("Cookie {0}: {1}", i, this.Cookies[i]);
                    if (Cookies[i] != null)
                    {
                        //this.Cookies[i].Id = j + 1;
                        jsonCookies.Add(Cookies[i].ToJSON());
                        j++;
                    }

                return "[\n" + string.Join(",\n", jsonCookies.ToArray()) + "\n]";
            }

            return "";
        }

        public static HostCookies FilterHostCookies(HostCookies[] hostCookies, string url)
        {
            var results = new HostCookies();
            if (hostCookies == null)
                return results;
            if (url == "" || url == null || url == string.Empty)
                return results;
            var hostPermutations = new List<string>();
            // First retrieve the domain from the url
            var domain = url;
            // determine if url or raw domain name
            if (domain.IndexOf('/') != -1) domain = domain.Split('/')[2];
            results.HostName = domain;
            var domainParts = domain.Split('.');
            for (var i = 0; i < domainParts.Length; i++)
            {
                if (domainParts.Length - i < 2)
                    // We've reached the TLD. Break!
                    break;
                var subDomainParts = new string[domainParts.Length - i];
                Array.Copy(domainParts, i, subDomainParts, 0, subDomainParts.Length);
                var subDomain = string.Join(".", subDomainParts);
                hostPermutations.Add(subDomain);
                hostPermutations.Add("." + subDomain);
            }

            var cookies = new List<Cookie>();
            foreach (var sub in hostPermutations)
                // For each permutation
            foreach (var hostInstance in hostCookies)
                // Determine if the hostname matches the subdomain perm
                if (hostInstance.HostName.ToLower() == sub.ToLower())
                    // If it does, cycle through
                    foreach (var cookieInstance in hostInstance.Cookies)
                        // No dupes
                        if (!cookies.Contains(cookieInstance))
                            cookies.Add(cookieInstance);
            results.Cookies = cookies.ToArray();
            return results;
        }
    }
}