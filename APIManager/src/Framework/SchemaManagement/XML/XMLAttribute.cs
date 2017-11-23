using System;
using System.Xml;
using System.Xml.Schema;
using System.Collections.Generic;
using Framework.Logging;
using Framework.Context;

namespace Framework.Util.SchemaManagement.XML
{
    /// <summary>
    /// Specialization of 'Attribute' for Content-type attributes. These are translated to elements that are associated with the type-object of the specified 
    /// classifier. These elements in turn must be included as a Sequence within the associated ABIE.
    /// The constructor performs most of the work in creating the appropriate element(s). In case of a cardinality >1, a list-element is constructed to contain 
    /// the actual content elements.
    /// The class contains an overloaded helper operation that is used to add the element to the appropriate container of the owning ABIE.
    /// </summary>
    internal class XMLContentAttribute : ContentAttribute
    {
        private XmlSchemaElement _attribute;    // The actual attribute in case of type = 'Content'.

        /// <summary>
        /// Getters for properties of Content Attribute:
        /// SchemaElement = Returns the schema element declaration contained in this content attribute.
        /// </summary>
        internal XmlSchemaElement SchemaElement   { get { return this._attribute; } }

        /// <summary>
        /// Construct a new content attribute (xsd:element). These attributes MUST be used in the context of the associated ABIE of which they are declared as attributes.
        /// Content attributes have a name (the attribute name as defined in the containing ABIE) and a classifier that must be a CDT (or derived BDT).
        /// The attribute is created in the specified schema and will use the namespace of that schema. In a way, we can state that the attribute element is
        /// created in the same schema as the classifier. However, they are not explicitly written to that schema but simply 'stored' there, together with
        /// their classifier. Eventually, the attribute element will be used as a sub-element of the associated ABIE and thus physically end-up in the same
        /// schema as the ABIE.
        /// Attributes have a cardinality specified by minOccurs and maxOccurs, both must be specified. If maxOccurs is '0', this is interpreted as 'unbounded'. 
        /// In case the cardinality of the attribute has an upper boundary > 1, a separate List element is created.
        /// If the cardinality lower boundary is 0, this list becomes an optional element. List contents always have a lower boundary of 1!
        /// The attribute can also have EITHER a default- or a fixed value, if both are specified, only the default value will be used.
        /// Note that the constructor creates a 'dangling' attribute, e.g. it is not assigned to an ABIE yet. It is the responsibility of the caller to pass this attribute to the associated
        /// ABIE element in due time.
        /// </summary>
        /// <param name="schema">The schema in which the attribute is created.</param>
        /// <param name="name">Name of the attribute as defined in the ABIE.</param>
        /// <param name="classifier">Classifier name as defined in the ABIE.</param>
        /// <param name="sequenceKey">When specified (not 0), this indicates the sorting order within a list of attributes and associations.</param>
        /// <param name="choiceGroup">Optional identifier for the choice group that this attribute should go to or NULL if not defined.</param>
        /// <param name="cardinality">Attribute cardinality. Upper boundary 0 is interpreted as 'unbounded'.</param>
        /// <param name="annotation">Optional comment for the content element. Empty list in case no comment is present.</param>
        /// <param name="defaultValue">Optional default value.</param>
        /// <param name="fixedValue">Optional fixed value.</param>
        internal XMLContentAttribute(XMLSchema schema,
                                   string name,
                                   string classifier,
                                   int sequenceKey,
                                   ChoiceGroup choiceGroup,
                                   Tuple<int, int> cardinality,
                                   List<MEDocumentation> annotation,
                                   string defaultValue, string fixedValue): 
            base(schema, name, classifier, sequenceKey, choiceGroup, cardinality, annotation, defaultValue, fixedValue)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLContentAttribute >> Creating attribute '" + name + "' with classifier '" + classifier + 
                             "' and cardinality '" + cardinality.Item1 + "-" + cardinality.Item2 + "'.");

            try
            {
                this._attribute = new XmlSchemaElement()
                {
                    Name = name
                };
                this._attribute.SchemaTypeName = new XmlQualifiedName(this.Classifier, this.ClassifierNS);
                if (cardinality.Item2 == 0)
                {
                    this._attribute.MaxOccursString = "unbounded";
                }
                else
                {
                    this._attribute.MaxOccurs = cardinality.Item2;
                }
                this._attribute.MinOccurs = (this.IsListRequired && cardinality.Item1 == 0) ? 1 : cardinality.Item1;     // If we're in a list, there must be at least one element.

                // Add (list of) annotation(s) to the attribute...
                if (annotation.Count > 0)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLContentAttribute >> Adding annotation to attribute...");
                    var annotationNode = new XmlSchemaAnnotation();
                    this._attribute.Annotation = annotationNode;
                    foreach (MEDocumentation docNode in annotation)
                    {
                        annotationNode.Items.Add(docNode.GetXmlDocumentationNode());
                    }
                }

                if (defaultValue != string.Empty)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLContentAttribute >> Attribute has default value: " + defaultValue);
                    this._attribute.DefaultValue = defaultValue;
                    this._attribute.MinOccurs = 0;                // Default value implies optional use of attribute!
                    if (fixedValue != string.Empty)
                    {
                        Logger.WriteWarning("Framework.Util.SchemaManagement.XML.XMLContentAttribute >> Attribute can not have both default- and fixed values, fixed value '" + 
                                            fixedValue + "' has been ignored!");
                    }
                }
                else if (fixedValue != string.Empty)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLContentAttribute >> Attribute has fixed value: " + fixedValue);
                    this._attribute.FixedValue = fixedValue;
                }

                if (this.IsListRequired)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLContentAttribute >> Attribute cardinality > 1, creating an intermediate List element....");
                    var listElement = new XmlSchemaElement()
                    {
                        Name = this.Name + "List",
                        MinOccurs = (cardinality.Item1 == 0) ? 0 : 1,
                        MaxOccurs = 1
                    };
                    var listBody = new XmlSchemaComplexType();
                    var listBodySequence = new XmlSchemaSequence();
                    listElement.SchemaType = listBody;
                    listBody.Particle = listBodySequence;
                    listBodySequence.Items.Add(this._attribute);
                    this._attribute = listElement;                // Replace original element by the created list (containing the element).
                    SetAttributeName(listElement.Name);           // We also have to update the name to reflect the list.
                }
                this.IsValid = true;
                Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLContentAttribute >> Successfully created attribute.");
            }
            catch (System.SystemException exc)
            {
                Logger.WriteError("Framework.Util.SchemaManagement.XML.XMLContentAttribute >> construction failed because: " + exc.Message);
            }
        }
    }

    /// <summary>
    /// Specialization of 'Attribute' for Supplementary-type attributes. These are translated to schema attributes of the owning ABIE.
    /// The constructor performs most of the work in creating the appropriate schema attributes. 
    /// The class contains an overloaded helper operation that is used to add the element to the appropriate container of the owning ABIE.
    /// </summary>
    internal class XMLSupplementaryAttribute : SupplementaryAttribute
    {
        // Configuration properties used by this module:
        private const string _XMLSchemaStdNamespace = "XMLSchemaStdNamespace";

        private XmlSchemaAttribute _attribute;      // The actual attribute in case of type = 'Supplementary'.

        /// <summary>
        /// Getters for properties of SupplementaryAttribute:
        /// SchemaAttribute = Returns the XSD schema object for this attribute.
        /// </summary>
        internal XmlSchemaAttribute XMLSchemaAttribute   { get { return this._attribute; } }

        /// <summary>
        /// Construct a new supplementary attribute (xsd:attribute). The attribute must have a name, a type and a usage flag (true in case of optional attribute, false in case of mandatory attribute).
        /// The attribute can also have EITHER a default- or a fixed value, if both are specified, only the default value will be used.
        /// The attribute classifier must be CCTS PRIM datatype name. UNION is NOT supported for attributes and will lead to an invalid object.
        /// </summary>
        /// <param name="schema">The schema in which the attribute is defined.</param>
        /// <param name="name">Name of the attribute</param>
        /// <param name="classifier">Type of the attribute, MUST be a PRIM type, CAN be an enumeration!</param>
        /// <param name="classifierInSchemaNS">When TRUE, the classifier is defined in the schema namespace instead of standard XSD namespace.</param>
        /// <param name="isOptional">TRUE if this is an optional attribute, FALSE if mandatory</param>
        /// <param name="annotation">Optional comment for the attribute (empty list in case of no comment).</param>
        /// <param name="defaultValue">Default value of the attribute</param>
        /// <param name="fixedValue">Fixed value of the attribute</param>
        internal XMLSupplementaryAttribute(XMLSchema schema,
                                           string name,
                                           string classifier,
                                           bool classifierInSchemaNS,
                                           bool isOptional,
                                           List<MEDocumentation> annotation,
                                           string defaultValue, string fixedValue): 
            base(schema, name, classifier, isOptional, annotation, defaultValue, fixedValue)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSupplementaryAttribute >> Creating attribute '" + name + "' of type '" + classifier + "'.");
            this.IsValid = false;

            try
            {
                this._attribute = new XmlSchemaAttribute()
                {
                    Name = name,
                    SchemaTypeName = new XMLPrimitiveType(schema, classifier).QualifiedType,
                    Use = isOptional ? XmlSchemaUse.Optional : XmlSchemaUse.Required
                };
                SetClassifierNS(classifierInSchemaNS ? schema.SchemaNamespace : ContextSlt.GetContextSlt().GetConfigProperty(_XMLSchemaStdNamespace));

                if (defaultValue != string.Empty)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSupplementaryAttribute >> Attribute has default value: " + defaultValue);
                    this._attribute.DefaultValue = defaultValue;
                    this._attribute.Use = XmlSchemaUse.Optional;        // Default value implies optional!
                }
                else if (fixedValue != string.Empty)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSupplementaryAttribute >> Attribute has fixed value: " + fixedValue);
                    this._attribute.FixedValue = fixedValue;
                }

                // Add (list of) annotation(s) to the attribute...
                if (annotation.Count > 0)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSupplementaryAttribute >> Adding annotation to element...");
                    var annotationNode = new XmlSchemaAnnotation();
                    this._attribute.Annotation = annotationNode;
                    foreach (MEDocumentation docNode in annotation)
                    {
                        annotationNode.Items.Add(docNode.GetXmlDocumentationNode());
                    }
                }

                if (this._attribute.SchemaTypeName != null)
                {
                    this.IsValid = true;
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSupplementaryAttribute >> Successfully created attribute.");
                }
                else
                {
                    Logger.WriteError("Framework.Util.SchemaManagement.XML.XMLSupplementaryAttribute >> construction failed because of illegal attribute type!");
                }
            }
            catch (System.SystemException exc)
            {
                Logger.WriteError("Framework.Util.SchemaManagement.XML.XMLSupplementaryAttribute >> construction failed because: " + exc.Message);
            }
        }
    }
}
