using System;
using System.Collections.Generic;
using Framework.Model;
using Framework.Context;
using Framework.Logging;
using Framework.Util;

namespace Plugin.Application.CapabilityModel.ASCIIDoc
{
    /// <summary>
    /// A ClassDocNode contains documentation of a single Class in ASCIIDoc format.
    /// </summary>
    internal sealed class ClassDocNode
    {
        // Configuration properties used by this module:
        private const string _ASCIIDocAttribute = "ASCIIDocAttribute";

        private string _ASCIIDoc;       // ASCIIDoc-formatted class documentation.
        private bool _firstAttrib;      // Used for correct formatting of attribute table.
        private string _contextID;      // By definition, this is equal to the namespace of the associated context.
        private string _nodeName;       // Copied from the class name.
        private string _notes;          // Class annotation.
        private bool _isEmpty;          // Class is a placeholder without any attributes.
        private List<string> _attribList;   // List of all assigned attributes (to guard against duplicates).

        /// <summary>
        /// Getters for private properties:
        /// ASCIIDoc = Terminates documentation collection and returns ASCIIDoc formatted contents.
        /// NodeName = The name of this documentation node (typically, this is equal to the class name).
        /// Notes = Class Annotation.
        /// Empty = Returns true if the class is a placeholder without any contents.
        /// </summary>
        internal string ASCIIDoc      { get { CloseNode(); return this._ASCIIDoc; } }
        internal string Name          { get { return this._nodeName; } }
        internal string Notes         { get { return this._notes; } }
        internal bool Empty           { get { return this._isEmpty; } }

        /// <summary>
        /// Open a new Class node for documentation. The constructor creates the general template. Separate calls are used
        /// to add attribute- and associations to the class.
        /// </summary>
        /// <param name="contextID">By definition, this is the namespace token of the namespace in which the class is defined.</param>
        /// <param name="name">The name of the class we're documenting.</param>
        /// <param name="notes">Class annotation.</param>
        /// <param name="level">Defines the indentation level (chapter numbers) of this node within the complete document.</param>
        /// <param name="isEmpty">When TRUE, this class has no attributes and we must suppress some output.</param>
        internal ClassDocNode(string contextID, string name, string notes, int level, bool isEmpty)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.ASCIIDoc.ClassDocNode >> Creating new node for '" + contextID + ":" + name + "'...");
            ContextSlt context = ContextSlt.GetContextSlt();
            this._contextID = contextID;
            string indent = string.Empty;
            for (int i = 0; i < level; indent += "=", i++) ;

            this._ASCIIDoc = context.GetResourceString(isEmpty? FrameworkSettings._ASCIIDocEmptyClassTemplate: FrameworkSettings._ASCIIDocClassTemplate);
            this._ASCIIDoc = this._ASCIIDoc.Replace("@CLASSNAME@", name);
            this._ASCIIDoc = this._ASCIIDoc.Replace("@CLASSANCHOR@", contextID.ToLower() + "_" + name.ToLower());
            this._ASCIIDoc = this._ASCIIDoc.Replace("@LVL@", indent);
            this._ASCIIDoc = this._ASCIIDoc.Replace("@NOTES@", notes);
            this._firstAttrib = true;
            this._nodeName = name;
            this._notes = notes;
            this._isEmpty = isEmpty;
            this._attribList = new List<string>();
        }

        /// <summary>
        /// This method adds an association with another class as an ASCIIDoc attribute description (since associations can be considered
        /// structured attributes). The classifier is formatted as an anchor since we want to be able to drill-down.
        /// </summary>
        /// <param name="name">Role name of the association, defines the 'attribute name' in the source class.</param>
        /// <param name="classifierName">Name of the target class, which acts as a classifier for the attribute.</param>
        /// <param name="classifierContextID">The context in which this class is defined (by definition, namespace token of target context).</param>
        /// <param name="cardinality">Cardinality of the association.</param>
        /// <param name="notes">Notes associated with the role.</param>
        internal void AddAssociation(string name, string classifierName, string classifierContextID, Cardinality cardinality, string notes)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.ASCIIDoc.ClassDocNode.AddAssociation >> Link to '" + classifierContextID + ":" + classifierName + 
                             "' with role '" + name + "' and cardinality '" + cardinality + "'...");

            if (this._attribList.Contains(name))
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.ASCIIDoc.ClassDocNode.AddAssociation >> Duplicate attribute definition, ignore!");
                return;
            }
            else this._attribList.Add(name);

            string attribDoc = ContextSlt.GetContextSlt().GetConfigProperty(_ASCIIDocAttribute);
            string cardString = cardinality.ToString();

            attribDoc = attribDoc.Replace("@NAME@", name);
            attribDoc = attribDoc.Replace("@CLASSIFIERANCHOR@", classifierContextID.ToLower() + "_" + classifierName.ToLower());
            attribDoc = attribDoc.Replace("@CLASSIFIER@", classifierName);
            attribDoc = attribDoc.Replace("@CARDINALITY@", cardString);
            attribDoc = attribDoc.Replace("@PRESENCE@", cardinality.IsOptional ? "Optional" : "Required");
            attribDoc = attribDoc.Replace("@NOTES@", notes);
            this._ASCIIDoc = this._ASCIIDoc.Replace("@ATTRIBUTES@", this._firstAttrib ? attribDoc + "@ATTRIBUTES@" :
                                                                                       Environment.NewLine + attribDoc + "@ATTRIBUTES@");
            this._firstAttrib = false;
            this._isEmpty = false;
        }

        /// <summary>
        /// This method adds a simple attribute as an ASCIIDoc attribute description. The classifier is formatted as an anchor 
        /// since we want to be able to drill-down.
        /// </summary>
        /// <param name="name">Attribute name.</param>
        /// <param name="type">Metatype of attribute.</param>
        /// <param name="classifierName">Attribute classifier name.</param>
        /// <param name="classifierContextID">The context in which the classifier is defined (by definition, namespace token of target context).</param>
        /// <param name="cardinality">Cardinality of the attribute.</param>
        /// <param name="notes">Notes associated with the attribute.</param>
        internal void AddAttribute (string name, AttributeType type,  string classifierName, string classifierContextID, Cardinality cardinality, string notes)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.ASCIIDoc.ClassDocNode.AddAttribute >> Attribute '" + name + "' of type '" + classifierContextID + 
                             ":" + classifierName + "' with cardinality '" + cardinality + "'...");

            if (this._attribList.Contains(name))
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.ASCIIDoc.ClassDocNode.AddAssociation >> Duplicate attribute definition, ignore!");
                return;
            }
            else this._attribList.Add(name);

            string attribDoc = ContextSlt.GetContextSlt().GetConfigProperty(_ASCIIDocAttribute);
            if (type == AttributeType.Facet) return;    // We don't support Facets at this level!

            string cardString = cardinality.ToString();

            attribDoc = attribDoc.Replace("@NAME@", (type == AttributeType.Supplementary) ? "Attribute: " + name : name);
            attribDoc = attribDoc.Replace("@CLASSIFIERANCHOR@", classifierContextID.ToLower() + "_" + classifierName.ToLower());
            attribDoc = attribDoc.Replace("@CLASSIFIER@", classifierName);
            attribDoc = attribDoc.Replace("@CARDINALITY@", cardString);
            attribDoc = attribDoc.Replace("@PRESENCE@", cardinality.IsOptional ? "Optional" : "Required");
            attribDoc = attribDoc.Replace("@NOTES@", notes);
            this._ASCIIDoc = this._ASCIIDoc.Replace("@ATTRIBUTES@", this._firstAttrib ? attribDoc + "@ATTRIBUTES@" :
                                                                                       Environment.NewLine + attribDoc + "@ATTRIBUTES@");
            this._firstAttrib = false;
            this._isEmpty = false;
        }

        /// <summary>
        /// This method reads the list of references from the provided CrossReference object and merges this into the class documentation.
        /// </summary>
        /// <param name="references">List of cross-references for this class document node.</param>
        internal void AddXREF(CrossReference references)
        {
            this._ASCIIDoc = this._ASCIIDoc.Replace("@XREF@", references.ReferenceText);
            Logger.WriteInfo("Plugin.Application.CapabilityModel.ASCIIDoc.ClassDocNode.AddXREF >> Added list: '" + references.ReferenceText + "'.");
        }

        /// <summary>
        /// Must be called when done with adding attributes and associations. After this call, the class can not be extended anymore.
        /// </summary>
        private void CloseNode()
        {
            this._ASCIIDoc = this._ASCIIDoc.Replace("@ATTRIBUTES@", string.Empty);
            this._ASCIIDoc = this._ASCIIDoc.Replace("@XREF@", string.Empty);
            Logger.WriteInfo("Plugin.Application.CapabilityModel.ASCIIDoc.ClassDocNode.CloseNode >> Closed document!");
        }
    }
}
