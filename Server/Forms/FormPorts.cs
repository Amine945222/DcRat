using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using Server.Helper;

namespace Server.Forms
{
    public partial class FormPorts : Form
    {
        private static bool isOK;

        public FormPorts()
        {
            InitializeComponent();
            Opacity = 0;
        }

        private void PortsFrm_Load(object sender, EventArgs e)
        {
            _ = Methods.FadeIn(this, 5);

            if (Properties.Settings.Default.Ports.Length == 0)
                listBox1.Items.AddRange(new object[] { "8848" });
            else
                try
                {
                    var ports = Properties.Settings.Default.Ports.Split(new[] { "," }, StringSplitOptions.None);
                    foreach (var item in ports)
                        if (!string.IsNullOrWhiteSpace(item))
                            listBox1.Items.Add(item.Trim());
                }
                catch
                {
                }

            Text = $"{Settings.Version} | Welcome {Environment.UserName}";

            if (!File.Exists(Settings.CertificatePath))
                using (var formCertificate = new FormCertificate())
                {
                    formCertificate.ShowDialog();
                }
            else
                Settings.ServerCertificate = new X509Certificate2(Settings.CertificatePath);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count > 0)
            {
                var ports = "";
                foreach (string item in listBox1.Items) ports += item + ",";
                Properties.Settings.Default.Ports = ports.Remove(ports.Length - 1);
                Properties.Settings.Default.Save();
                isOK = true;
                Close();
            }
        }

        private void PortsFrm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!isOK)
            {
                Program.Form1.notifyIcon1.Dispose();
                Environment.Exit(0);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                Convert.ToInt32(textPorts.Text.Trim());
                listBox1.Items.Add(textPorts.Text.Trim());
                textPorts.Clear();
            }
            catch
            {
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            listBox1.Items.Remove(listBox1.SelectedItem);
        }
    }
}