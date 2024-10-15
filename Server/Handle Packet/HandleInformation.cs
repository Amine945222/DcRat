using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Server.Connection;
using Server.MessagePack;

namespace Server.Handle_Packet
{
    public class HandleInformation
    {
        public void AddToInformationList(Clients client, MsgPack unpack_msgpack)
        {
            try
            {
                var tempPath = Path.Combine(Application.StartupPath,
                    "ClientsFolder\\" + unpack_msgpack.ForcePathObject("ID").AsString + "\\Information");
                var fullPath = tempPath + "\\Information.txt";
                if (!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);
                File.WriteAllText(fullPath, unpack_msgpack.ForcePathObject("InforMation").AsString);
                Process.Start("explorer.exe", fullPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}