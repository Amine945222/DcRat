using System;
using System.IO;
using System.Runtime.InteropServices;
using Plugin;

namespace Utils
{
    internal class FileUtils
    {
        public static string CreateTempDuplicateFile(string filePath)
        {
            var localAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");
            var newFile = "";
            newFile = Path.GetRandomFileName();
            var tempFileName = localAppData + "\\Temp\\" + newFile;
            File.Copy(filePath, tempFileName);
            return tempFileName;
        }
    }

    internal class MiscUtils
    {
        public static void BCRYPT_INIT_AUTH_MODE_INFO(
            out BCrypt.BCRYPT_AUTHENTICATED_CIPHER_MODE_INFO _AUTH_INFO_STRUCT_)
        {
            _AUTH_INFO_STRUCT_ = new BCrypt.BCRYPT_AUTHENTICATED_CIPHER_MODE_INFO();
            _AUTH_INFO_STRUCT_.cbSize = Marshal.SizeOf(typeof(BCrypt.BCRYPT_AUTHENTICATED_CIPHER_MODE_INFO));
            _AUTH_INFO_STRUCT_.dwInfoVersion = 1;
        }
    }
}