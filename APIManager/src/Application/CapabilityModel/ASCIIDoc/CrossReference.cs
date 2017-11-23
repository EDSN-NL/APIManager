using System;
using System.Collections.Generic;
using Framework.Context;

namespace Plugin.Application.CapabilityModel.ASCIIDoc
{
    internal class CrossReference
    {
        // Configuration properties used by this module:
        private const string _ASCIIDocXREF = "ASCIIDocXREF";

        private string _itemName;                           // Name of the referenced item.
        private SortedList<string, Tuple<string, string>> _references;    // All anchor/title tuples that reference the destination.

        internal String ReferenceText { get { return GetReferenceText(); } }

        /// <summary>
        /// Creates a new cross-reference target.
        /// </summary>
        /// <param name="itemName">Name of the target.</param>
        internal CrossReference(string itemName)
        {
            this._itemName = itemName;
            this._references = new SortedList<string, Tuple<string, string>>();
        }

        /// <summary>
        /// Method adds a new source item to this cross-reference. We avoid adding the same source reference twice.
        /// </summary>
        /// <param name="anchor">Anchor for the source.</param>
        /// <param name="name">Human-readable name for the source.</param>
        internal void AddReference(string anchor, string name)
        {
            if (!this._references.ContainsKey(anchor)) this._references.Add(anchor, new Tuple<string, string>(anchor, name));
        }

        /// <summary>
        /// Helper function that creates a list of anchor points in ASCIIDoc format according to the collected information.
        /// </summary>
        /// <returns>ASCIIDoc formatted list of 'used by' pointers.</returns>
        private string GetReferenceText()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string xrefLine = context.GetConfigProperty(_ASCIIDocXREF);
            xrefLine = xrefLine.Replace("@NAME@", this._itemName);
            string xrefItems = string.Empty;
            bool firstOne = true;
            foreach (Tuple<string, string> src in this._references.Values)
            {
                xrefItems += (firstOne ? "<<" : ", <<") + src.Item1 + "," + src.Item2 + ">> ";
                firstOne = false;
            }
            xrefLine = xrefLine.Replace("@XREF@", xrefItems);
            return xrefLine;
        }
    }
}
