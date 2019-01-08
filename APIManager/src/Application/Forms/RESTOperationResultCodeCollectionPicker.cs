using System.Collections.Generic;
using System.Windows.Forms;

namespace Plugin.Application.Forms
{
    /// <summary>
    /// Form that facilitates selection of one REST Operation Result Code collection from a list of multiple collections.
    /// </summary>
    internal partial class RESTOperationResultCodeCollectionPicker : Form
    {
        internal string SelectedCollection { get { return CollectionList.SelectedItem as string; } }

        /// <summary>
        /// Dialog constructor, loads the list of collection names.
        /// </summary>
        internal RESTOperationResultCodeCollectionPicker(IList<string> collectionNames)
        {
            InitializeComponent();
            foreach (string name in collectionNames) CollectionList.Items.Add(name);
            CollectionList.SelectedItem = collectionNames[0];
        }
    }
}
