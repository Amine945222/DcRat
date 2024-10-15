using System;
using System.IO;
using System.Security.Cryptography;

namespace Server.Algorithm
{
    public static class GetHash
    {
        public static string GetChecksum(string file)
        {
            using (var stream = File.OpenRead(file))
            {
                var sha = new SHA256Managed();
                var checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", string.Empty);
            }
        }
    }
}