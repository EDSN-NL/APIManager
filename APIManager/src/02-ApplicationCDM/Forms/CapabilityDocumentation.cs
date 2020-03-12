using System;
using System.Drawing;
using System.Windows.Forms;
using Framework.Logging;

namespace Plugin.Application.Forms
{
    /// <summary>
    /// A very basic text editor for the creation of RTF-formatted documentation blocks.
    /// Formatting is limited to Bold, Italic, Underline and Bullet lists, since our current repository (Enterprise Architect) does not
    /// support much else.
    /// </summary>
    internal partial class CapabilityDocumentation : Form
    {
        /// <summary>
        /// Returns the annotation text entered by the user.
        /// </summary>
        internal string Documentation { get { return DocumentationFld.Rtf; } }

        /// <summary>
        /// Dialog constructor.
        /// </summary>
        internal CapabilityDocumentation(string labelText, string documentation)
        {
            InitializeComponent();
            this.Text = labelText;
            DocumentationFld.Rtf = documentation;
            DocumentationFld.BulletIndent = 15;
        }

        /// <summary>
        /// Shows the 'File Open' dialog to the user and facilitates selection of existing files. The contents of the selected file will 
        /// be stored in the Documentation area.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void FileOpen_Click(object sender, EventArgs e)
        {
            if (OpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    DocumentationFld.LoadFile(OpenFileDialog.FileName, 
                                              OpenFileDialog.FilterIndex == 1? RichTextBoxStreamType.PlainText: RichTextBoxStreamType.RichText);
                }
                catch (Exception exc)
                {
                    Logger.WriteError("Plugin.Application.Forms.CapabilityDocumentation.FileOpen_Click >> Error opening file because: " + Environment.NewLine + exc.ToString());
                }
            }
        }

        /// <summary>
        /// Moves the selected text to the clipboard.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void EditCut_Click(object sender, EventArgs e)
        {
            DocumentationFld.Cut();
        }

        /// <summary>
        /// Copies the selected text to the clipboard.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void EditCopy_Click(object sender, EventArgs e)
        {
            DocumentationFld.Copy();
        }

        /// <summary>
        /// Pastes text from the clipboard to the local cursor position.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void EditPaste_Click(object sender, EventArgs e)
        {
            DocumentationFld.Paste();
        }

        /// <summary>
        /// Removes the selected text.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void EditDelete_Click(object sender, EventArgs e)
        {
            DocumentationFld.SelectedRtf = string.Empty;
        }

        /// <summary>
        /// Change selected text to Bold format.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void FormatBold_Click(object sender, EventArgs e)
        {
            DocumentationFld.SelectionFont = new Font(DocumentationFld.Font, FontStyle.Bold);
        }

        /// <summary>
        /// Change selected text to Italic format.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param
        private void FormatItalic_Click(object sender, EventArgs e)
        {
            DocumentationFld.SelectionFont = new Font(DocumentationFld.Font, FontStyle.Italic);
        }

        /// <summary>
        /// Change selected text to regular format.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param
        private void FormatRegular_Click(object sender, EventArgs e)
        {
            DocumentationFld.SelectionFont = new Font(DocumentationFld.Font, FontStyle.Regular);
        }

        /// <summary>
        /// Change selected text to Strikeout format.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param
        private void FormatStrikeout_Click(object sender, EventArgs e)
        {
            DocumentationFld.SelectionFont = new Font(DocumentationFld.Font, FontStyle.Strikeout);
        }

        /// <summary>
        /// Change selected text to Underline format.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param
        private void FormatUnderline_Click(object sender, EventArgs e)
        {
            DocumentationFld.SelectionFont = new Font(DocumentationFld.Font, FontStyle.Underline);
        }

        /// <summary>
        /// Add bullets to selected lines and indent text.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param
        private void FormatBullet_Click(object sender, EventArgs e)
        {
            DocumentationFld.SelectionBullet = true;
        }
    }
}
