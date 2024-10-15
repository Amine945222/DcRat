using System;
using System.Windows.Forms;
using Server.Helper;
using static Server.Helper.RegistrySeeker;

namespace Server.Forms
{
    public partial class FormRegValueEditMultiString : Form
    {
        private readonly RegValueData _value;

        public FormRegValueEditMultiString(RegValueData value)
        {
            _value = value;

            InitializeComponent();

            valueNameTxtBox.Text = value.Name;
            valueDataTxtBox.Text = string.Join("\r\n", ByteConverter.ToStringArray(value.Data));
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            _value.Data =
                ByteConverter.GetBytes(valueDataTxtBox.Text.Split(new[] { "\r\n" },
                    StringSplitOptions.RemoveEmptyEntries));
            Tag = _value;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}