using System.Collections.Generic;

namespace Plugin
{
    public class Cookie
    {
        //private int _id;

        // Getters and setters
        public string Domain { get; set; }

        public double ExpirationDate { get; set; }

        public bool HostOnly { get; set; }

        public bool HttpOnly { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public string SameSite { get; set; }

        public bool Secure { get; set; }

        public bool Session { get; set; }

        public string StoreId { get; set; }

        public string Value { get; set; }
        //public int Id
        //{
        //    get { return _id; }
        //    set { _id = value; }
        //}

        public void SetSameSiteCookie(string val)
        {
            switch (val)
            {
                case "-1":
                    SameSite = "unspecified";
                    break;
                case "0":
                    SameSite = "no_restriction";
                    break;
                case "1":
                    SameSite = "lax";
                    break;
                case "2":
                    SameSite = "strict";
                    break;

                default:
                    SameSite = "unspecified";
                    break;
            }
        }

        public string ToJSON()
        {
            var type = GetType();
            var properties = type.GetProperties();
            if ((properties == null) | (properties.Length == 0))
                return "";
            var jsonItems = new List<string>(); // Number of items in EditThisCookie
            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                object[] keyvalues =
                {
                    property.Name[0].ToString().ToLower() + property.Name.Substring(1, property.Name.Length - 1),
                    property.GetValue(this, null)
                };
                if (keyvalues[1] != null && keyvalues[1].ToString().Contains("\""))
                    keyvalues[1] = keyvalues[1].ToString().Replace("\"", "\\\"");
                var jsonString = "";
                if (keyvalues[0].ToString() == "expirationDate" && keyvalues[1].ToString() == "0")
                    continue;
                if (keyvalues[1] == null)
                {
                    jsonString = string.Format("    \"{0}\": null", keyvalues[0]);
                }
                else if (keyvalues[1].GetType() == typeof(string))
                {
                    jsonString = string.Format("    \"{0}\": \"{1}\"", keyvalues);
                }
                else if (keyvalues[1].GetType() == typeof(bool))
                {
                    keyvalues[1] = keyvalues[1].ToString().ToLower();
                    jsonString = string.Format("    \"{0}\": {1}", keyvalues);
                }
                else
                {
                    jsonString = string.Format("    \"{0}\": {1}", keyvalues);
                }

                if (jsonString != "")
                    jsonItems.Add(jsonString);
            }

            var results = "{\n" + string.Join(",\n", jsonItems.ToArray()) + "\n}";
            return results;
        }
    }
}