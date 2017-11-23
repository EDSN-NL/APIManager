using System;
using System.Windows.Forms;
using Framework.Logging;

namespace Plugin.Application.Forms
{
    /// <summary>
    /// A simple form that accepts digits between 1 and 999.
    /// </summary>
    internal partial class ChangeMajorVersion : Form
    {
        private int _lowTreshold;

        internal int MajorVersion { get { return Int32.Parse(InputField.Text); } }

        /// <summary>
        /// Default constructor. Receives the current version, which is used as the low threshold (user can not select a number
        /// lower than the current version). Current version + 1 is shown to the user as proposed version.
        /// Upper limit of the version is set to 9999.
        /// </summary>
        internal ChangeMajorVersion(int currentVersion)
        {
            InitializeComponent();
            toolTip.IsBalloon = true;
            Ok.Enabled = false;
            this._lowTreshold = currentVersion;
            InputField.Text = (currentVersion+1).ToString();
        }

        /// <summary>
        /// Called whenever the user enters another character. Check whether this is valid and if not, warn the user.
        /// Releases the OK butten the moment we got a valid version number.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InputField_TextChanged(object sender, EventArgs e)
        {
        	int value;
        	if (Int32.TryParse(InputField.Text, out value))
            {
                Logger.WriteInfo("Custom.Event.EventImp.ChangeMajorVersion.TextChanged >> Got value: " + value);
                if (value < this._lowTreshold || value > 9999)
                {
                    Logger.WriteInfo("Custom.Event.EventImp.ChangeMajorVersion.TextChanged >> Illegal value!");
                    toolTip.Show("Please enter a number between '" + _lowTreshold + "' and '9999'!", InputField, 5000);
                    Ok.Enabled = false;
                }
                else Ok.Enabled = true;
            }
            else
            {
                Logger.WriteInfo("Custom.Event.EventImp.ChangeMajorVersion.TextChanged >> Illegal value!");
                toolTip.Show("Please enter a number between '" + _lowTreshold + "' and '9999'!", InputField, 5000);
                //InputField.Select(0, InputField.Text.Length);
                Ok.Enabled = false;
            }
        }
    }
}
