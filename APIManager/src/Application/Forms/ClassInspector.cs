using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Plugin.Application.CapabilityModel;
using Plugin.Application.CapabilityModel.API;
using Framework.Model;

namespace Plugin.Application.Forms
{
    internal partial class ClassInspector : Form
    {
        /// <summary>
        /// Creates a new dialog that facilitates creation of a series of resources. Each resource is represented in the dialog
        /// as a tuple of resource name and archetype.
        /// </summary>
        /// <param name="parent">The capability that will act as the parent for the new resource(s).</param>
        internal ClassInspector(MEClassMetaData metaData)
        {
            InitializeComponent();

            ClassName.Text = metaData.Name;
            AliasName.Text = metaData.Alias;
            StereoTypes.Text = metaData.Stereotypes;

            foreach (MEAttributeMetaData att in metaData.Attributes)
            {
                ListViewItem attribItem = new ListViewItem(att.Identifier.ToString());
                attribItem.SubItems.Add(att.Name);
                attribItem.SubItems.Add(att.Alias);
                attribItem.SubItems.Add(att.Classifier);
                attribItem.SubItems.Add(att.Cardinality);
                attribItem.SubItems.Add(att.Type.ToString());
                ResourceList.Items.Add(attribItem);
            }

        }
    }
}
