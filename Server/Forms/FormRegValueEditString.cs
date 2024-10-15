using System;
using System.Windows.Forms;
using Server.Helper;
using static Server.Helper.RegistrySeeker;

namespace Server.Forms
{
    public partial class FormRegValueEditString : Form
    {
        private readonly RegValueData _value;

        public FormRegValueEditString(RegValueData value)
        {
            _value = value;

            InitializeComponent();

            valueNameTxtBox.Text = RegValueHelper.GetName(value.Name);
            valueDataTxtBox.Text = ByteConverter.ToString(value.Data);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            _value.Data = ByteConverter.GetBytes(valueDataTxtBox.Text);
            Tag = _value;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}