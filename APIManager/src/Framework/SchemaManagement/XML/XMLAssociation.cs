using System;
using System.Xml;
using System.Xml.Schema;
using System.Collections.Generic;
using Framework.Logging;

namespace Framework.Util.SchemaManagement.XML
{
    /// <summary>
    /// An Association class represents the "target end" of an ASBIE (an ASsociated Business Information Element). In the chosen model, each ABIE is 
    /// represented by a separate complex type and instances (elements) are only created for association elements. The only exception is the "root" 
    /// Message Assembly element. This method keeps the number of elements in a schema as low as possible, while still facilitating efficient mapping 
    /// since all elements share a common type model.
    /// The association is thus implemented as an XmlSchemaElement, which must be added to the parent ABIE using the Schema.addABIEType method.
    /// 
    /// Note: in case the cardinality of the association has an upper boundary > 1, a separate List element is created.
    /// If the cardinality lower boundary is 0, this list becomes an optional element. List contents always have a lower boundary of 1!
    /// </summary>
    internal class XMLAssociation : SchemaAssociation
    {
        private XmlSchemaElement _classifier;

        /// <summary>
        /// Getters for Association properties:
        /// SchemaElement = Returns the schema element declaration contained in this association.
        /// </summary>
        internal XmlSchemaElement SchemaElement   { get { return this._classifier; } }

        /// <summary>
        /// Creates a new Association object (ASBIE instance) within the given schema. Other arguments are the name of the target ABIE as well as the cardinality of the association (target end).
        /// If the target of the association (the actual classifier definition) resides in another schema, the namespace of the external schema MUST be passed through 'namespaceRef'.
        /// The resulting Association object is represented as an XSD Schema element with name 'associationName' and referencing type 'classifier'.
        /// </summary>
        /// <param name="schema">The schema in which the ASBIE is being created.</param>
        /// <param name="associationName">The name that will be assigned to the element.</param>
        /// <param name="classifier">The name of the target ABIE type.</param>
        /// <param name="sequenceKey">The value of sequenceKey defines the order within a list of associations. Value 0 means 'inactive'</param>
        /// <param name="choiceGroup">Optional identifier of choicegroup that this association should go to (NULL is not defined).</param>
        /// <param name="cardinality">Association cardinality. An upper boundary of '0' is interpreted as 'unbounded'.</param>
        /// <param name="annotation">Optional annotation for the association (empty list in case of no annotation).</param>
        /// <param name="namespaceRef">Optional reference to external namespace. If NULL, the classifier is referenced through the specified schema.</param>
        internal XMLAssociation(XMLSchema schema, string associationName,
                              string classifier,
                              int sequenceKey,
                              ChoiceGroup choiceGroup,
                              Tuple<int, int> cardinality,
                              List<MEDocumentation> annotation,
                              string namespaceRef = null): base(associationName, classifier, sequenceKey, cardinality, choiceGroup)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLAssociation >> Creating association for '" + associationName + "' with classifier: " + classifier + 
                             " and cardinality: " + cardinality.Item1 + ".." + cardinality.Item2 + "...");
            this.IsValid = false;
            if (cardinality.Item1 < 0 || cardinality.Item2 < 0 || (cardinality.Item2 != 0 && (cardinality.Item1 > cardinality.Item2)))
            {
                Logger.WriteError("Framework.Util.SchemaManagement.XML.XMLAssociation >> Cardinality out of bounds for target '" + classifier + "'!");
                return;
            }

            try
            {
                string classifierNamespace = namespaceRef ?? schema.SchemaNamespace;
                Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLAssociation >> Collected namespace '" + classifierNamespace + "', for classifier '" + classifier + "'.");

                bool listRequired = (cardinality.Item2 != 1) ? true : false;
                this._classifier = new XmlSchemaElement()
                {
                    Name = associationName,
                    SchemaTypeName = new XmlQualifiedName(classifier, classifierNamespace)
                };
                if (cardinality.Item2 == 0)
                {
                    this._classifier.MaxOccursString = "unbounded";
                }
                else
                {
                    this._classifier.MaxOccurs = cardinality.Item2;
                }
                this._classifier.MinOccurs = (listRequired && cardinality.Item1 == 0) ? 1 : cardinality.Item1;     // If we're in a list, there must be at least one element.

                // Add (list of) annotation(s) to the association...
                if (annotation.Count > 0)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLAssociation >> Adding annotation to element...");
                    var annotationNode = new XmlSchemaAnnotation();
                    this._classifier.Annotation = annotationNode;
                    foreach (MEDocumentation docNode in annotation)
                    {
                        annotationNode.Items.Add(docNode.GetXmlDocumentationNode());
                    }
                }

                if (listRequired)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLAssociation >> ASBIE cardinality > 1, creating an intermediate List element....");
                    var listElement = new XmlSchemaElement()
                    {
                        Name = associationName + "List",
                        MinOccurs = (cardinality.Item1 == 0) ? 0 : 1,
                        MaxOccurs = 1
                    };
                    var listBody = new XmlSchemaComplexType();
                    var listBodySequence = new XmlSchemaSequence();
                    listElement.SchemaType = listBody;
                    listBody.Particle = listBodySequence;
                    listBodySequence.Items.Add(this._classifier);
                    this._classifier = listElement;                     // Replace original ASBIE by the list (containing the ASBIE)...
                    this.ASBIEName = listElement.Name;                  // We also have to update the name to reflect the list. 
                }
                this.IsValid = true;
                Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLAssociation >> Successfully created ASBIE element.");
            }
            catch (System.SystemException exc)
            {
                Logger.WriteError("Framework.Util.SchemaManagement.XML.XMLAssociation >> construction failed because: " + exc.Message);
            }
        }

        /// <summary>
        /// Is used to insert the association element into the specified element collection.
        /// </summary>
        /// <returns>Content attribute or NULL in case of illegal attribute.</returns>
        internal void AddToCollection(XmlSchemaObjectCollection collection)
        {
            if (this.IsValid) collection.Add(this._classifier);
        }
    }
}
