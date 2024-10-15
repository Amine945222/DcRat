using System.Windows.Forms;
using Server.Connection;
using Server.Forms;
using Server.MessagePack;

namespace Server.Handle_Packet
{
    public class HandleFun
    {
        public void Fun(Clients client, MsgPack unpack_msgpack)
        {
            try
            {
                var fun = (FormFun)Application.OpenForms["fun:" + unpack_msgpack.ForcePathObject("Hwid").AsString];
                if (fun != null)
                    if (fun.Client == null)
                    {
                        fun.Client = client;
                        fun.timer1.Enabled = true;
                    }
            }
            catch
            {
            }
        }
    }
}