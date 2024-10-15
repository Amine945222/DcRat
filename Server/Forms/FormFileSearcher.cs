using System;
using System.Windows.Forms;

namespace Server.Forms
{
    public partial class FormFileSearcher : Form
    {
        public FormFileSearcher()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtExtnsions.Text) && numericUpDown1.Value > 0)
                DialogResult = DialogResult.OK;
        }
    }
}