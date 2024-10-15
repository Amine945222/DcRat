using System;
using System.Windows.Forms;

namespace Server.Forms
{
    public partial class FormSetting : Form
    {
        public FormSetting()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                if (textBox1.Text == null || textBox2.Text == null)
                    MessageBox.Show("Input the WebHook and secret");
            Properties.Settings.Default.DingDing = checkBox1.Checked;

            Properties.Settings.Default.WebHook = textBox1.Text;

            Properties.Settings.Default.Secret = textBox2.Text;

            Properties.Settings.Default.Save();

            Close();
        }

        private void FormSetting_Load(object sender, EventArgs e)
        {
            checkBox1.Checked = Properties.Settings.Default.DingDing;

            textBox1.Text = Properties.Settings.Default.WebHook;

            textBox2.Text = Properties.Settings.Default.Secret;
        }
    }
}