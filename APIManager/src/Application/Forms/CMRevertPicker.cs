using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Plugin.Application.Forms
{
    internal partial class CMRevertPicker : Form
    {
        /// <summary>
        /// Returns the tag thas has been selected by the user.
        /// </summary>
        internal string SelectedTag { get { return (string)TagList.SelectedItem; } }

        /// <summary>
        /// Returns the user-assigned version associated with the selected tag as a triplet (major, minor, build).
        /// </summary>
        internal Tuple<int,int,int> AssignedVersion
        {
            get
            {
                int majorNr = Int32.Parse(MajorVersion.Text);
                int minorNr = Int32.Parse(MinorVersion.Text);
                int buildNr = Int32.Parse(BuildNumber.Text);
                return new Tuple<int, int, int>(majorNr, minorNr, buildNr);
            }
        }

        /// <summary>
        /// Dialog constructor.
        /// </summary>
        internal CMRevertPicker(List<string> tagList)
        {
            InitializeComponent();

            foreach (string s in tagList) TagList.Items.Add(s);
            TagList.SelectedIndex = 0;
        }

        /// <summary>
        /// Returns the version number (major, minor, build) corresponding to the currently selected tag.
        /// </summary>
        /// <returns>Triplet major, minor, build.</returns>
        private Tuple<int,int,int> GetVersionFromTag()
        {
            string selectedTag = SelectedTag;

            int indexMajor = selectedTag.IndexOf("_V");
            int indexMinor = selectedTag.IndexOf('P');
            int indexBuild = selectedTag.IndexOf('B');
            string major = selectedTag.Substring(indexMajor + 2, indexMinor - indexMajor - 2);
            string minor = selectedTag.Substring(indexMinor + 1, indexBuild - indexMinor - 1);
            string build = selectedTag.Substring(indexBuild + 1);

            int majorNr = Int32.Parse(major);
            int minorNr = Int32.Parse(minor);
            int buildNr = Int32.Parse(build);
            return new Tuple<int, int, int>(majorNr, minorNr, buildNr);
        }

        /// <summary>
        /// This event is raised when the user selects a (new) item in the tag list. It retrieves the version components from
        /// the tag and uses these to initialise the version boxes. The user can subsequently overwrite these.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TagList_SelectedValueChanged(object sender, EventArgs e)
        {
            Tuple<int, int, int> version = GetVersionFromTag();
            MajorVersion.Text = version.Item1.ToString();
            MinorVersion.Text = version.Item2.ToString();
            BuildNumber.Text = version.Item3.ToString();
        }
    }
}
