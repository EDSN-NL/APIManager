﻿using System;
using System.Windows.Forms;

namespace Plugin.Application.Forms
{
    internal partial class CMReleaseMessage : Form
    {
        /// <summary>
        /// Returns the annotation text entered by the user.
        /// </summary>
        internal string Annotation { get { return AnnotationFld.Text; } } 

        /// <summary>
        /// Dialog constructor.
        /// </summary>
        internal CMReleaseMessage()
        {
            InitializeComponent();
            AnnotationFld.Select();
            Ok.Enabled = false;
        }

        /// <summary>
        /// Invoked when the user leaves the annotation field. We check whether there are an arbitrary number of characters in the field
        /// and if so, release the 'ok' button.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void AnnotationFld_Leave(object sender, EventArgs e)
        {
            Ok.Enabled = AnnotationFld.Text.Length >= 8;
            ErrorText.Text = Ok.Enabled ? string.Empty : "Please enter at least an 8-character description!";
        }
    }
}
