using System;
using System.Windows.Forms;

namespace Plugin.Application.Forms
{
    internal partial class AccessLevelForm : Form
    {
        private int _level;     // Numeric from 0 .. n (0 = least restrictive).

        internal int level { get { return this._level; } }

        internal AccessLevelForm()
        {
            InitializeComponent();
            this._level = 0;
            RBPublic.Checked = true;
            RBInternalUse.Checked = false;
            RBConfidential.Checked = false;
            RBSecret.Checked = false;
        }

        private void RBPublic_CheckedChanged(object sender, EventArgs e)
        {
            this._level = 0;
        }

        private void RBInternalUse_CheckedChanged(object sender, EventArgs e)
        {
            this._level = 1;
        }

        private void RBConfidential_CheckedChanged(object sender, EventArgs e)
        {
            this._level = 2;
        }

        private void RBSecret_CheckedChanged(object sender, EventArgs e)
        {
            this._level = 3;
        }

        private void Done_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
