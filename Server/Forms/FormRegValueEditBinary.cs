using System;
using System.Windows.Forms;
using Server.Helper;
using static Server.Helper.RegistrySeeker;

namespace Server.Forms
{
    public partial class FormRegValueEditBinary : Form
    {
        private const string INVALID_BINARY_ERROR =
            "The binary value was invalid and could not be converted correctly.";

        private readonly RegValueData _value;

        public FormRegValueEditBinary(RegValueData value)
        {
            _value = value;

            InitializeComponent();

            valueNameTxtBox.Text = RegValueHelper.GetName(value.Name);
            hexEditor.HexTable = value.Data;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            var bytes = hexEditor.HexTable;
            if (bytes != null)
                try
                {
                    _value.Data = bytes;
                    DialogResult = DialogResult.OK;
                    Tag = _value;
                }
                catch
                {
                    ShowWarning(INVALID_BINARY_ERROR, "Warning");
                    DialogResult = DialogResult.None;
                }

            Close();
        }

        private void ShowWarning(string msg, string caption)
        {
            MessageBox.Show(msg, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}