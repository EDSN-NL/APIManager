﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Plugin.Application.Forms
{
    /// <summary>
    /// Dialog box that can be used to request confirmation from the user for a specified action. The dialogue includes a minor-version update toggle
    /// switch.
    /// </summary>
    internal partial class ConfirmOperationChanges : Form
    {
        private bool _newMinorVersion;

        internal bool MinorVersionIndicator { get { return this._newMinorVersion; } }
      
        /// <summary>
        /// Dialog constructor receives the text to be displayed in the warning box.
        /// </summary>
        /// <param name="text">Text to be displayed.</param>
        internal ConfirmOperationChanges(string text)
        {
            InitializeComponent();
            Label.Text = text;
            this._newMinorVersion = false;
        }

        /// <summary>
        /// Load resources in the dialog box.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void ConfirmOperationChanges_Load(object sender, EventArgs e)
        {
            Warning.Image = SystemIcons.Warning.ToBitmap();
            NewMinorVersion.Checked = false;
            NewMinorVersion.Focus();
        }

        /// <summary>
        /// Invoked whenever the user toggled the checkbox.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void NewMinorVersion_CheckedChanged(object sender, EventArgs e)
        {
            this._newMinorVersion = NewMinorVersion.Checked;
        }
    }
}
