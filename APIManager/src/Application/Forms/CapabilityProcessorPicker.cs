using System.Collections.Generic;
using System.Windows.Forms;
using Plugin.Application.CapabilityModel;

namespace Plugin.Application.Forms
{
    internal partial class CapabilityProcessorPicker : Form
    {
        private string _capabilityClass;

        /// <summary>
        /// The selectedProcessor property returns the CapabilityProcessor associated with the user selection.
        /// </summary>
        internal CapabilityProcessor SelectedProcessor
        {
            get
            {
                string selectedID = processorList.Text;
                ProcessorManagerSlt mgr = ProcessorManagerSlt.GetProcessorManagerSlt();
                return mgr.GetProcessorByID(this._capabilityClass, selectedID);
            }
        }

        /// <summary>
        /// Constructor prepares the picker by loading all capability processors that could be found for the specified 
        /// capability class.
        /// </summary>
        /// <param name="capabilityClass">Capability class to use as search basis...</param>
        internal CapabilityProcessorPicker(string capabilityClass)
        {
            InitializeComponent();

            ProcessorManagerSlt mgr = ProcessorManagerSlt.GetProcessorManagerSlt();
            List<string> processorIDList = mgr.GetProcessorIDList(capabilityClass);
            processorList.SelectionMode = SelectionMode.One;
            this._capabilityClass = capabilityClass;

            processorList.BeginUpdate();
            foreach (string proc in processorIDList) processorList.Items.Add(proc);
            processorList.EndUpdate();   
        }
    }
}
