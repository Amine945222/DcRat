using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using MessagePackLib.MessagePack;

namespace Plugin
{
    public partial class FormChat : Form
    {
        public FormChat()
        {
            InitializeComponent();
        }

        private void FormChat_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (!Connection.IsConnected)
            {
                Packet.GetFormChat.Invoke((MethodInvoker)(() =>
                {
                    Packet.GetFormChat?.Close();
                    Packet.GetFormChat?.Dispose();
                }));
                Connection.Disconnected();
                GC.Collect();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                richTextBox1.SelectionColor = Color.Green;
                richTextBox1.AppendText("Me: \n");
                richTextBox1.SelectionColor = Color.Black;
                richTextBox1.AppendText(textBox1.Text + Environment.NewLine);
                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "chat";
                msgpack.ForcePathObject("Hwid").AsString = Connection.Hwid;
                msgpack.ForcePathObject("WriteInput").AsString = Environment.UserName + ": \n";
                msgpack.ForcePathObject("WriteInput2").AsString = textBox1.Text + Environment.NewLine;
                Connection.Send(msgpack.Encode2Bytes());
                textBox1.Clear();
            }
        }

        private void FormChat_Load(object sender, EventArgs e)
        {
            TopMost = true;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/qwqdanchun/DcRat");
        }
    }
}