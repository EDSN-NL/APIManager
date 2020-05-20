using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Collections.Generic;
using Framework.Logging;
using Framework.Context;

namespace Framework.Util.SchemaManagement.XML
{
    /// <summary>
    /// The schema is the foundation of the XML schema builder module. Instances of this class represent a single XSD schema document and the class contains 
    /// operations for the construction of:
    /// 1) CDT type objects that represent BBIE types. For each BBIE type, an appropriate type object is created (one of SimpleType, ComplexType or 
    ///    EnumeratedType; Unions are not yet supported).
    /// 2) Complex type objects that represent the different class objects (ABIE types). For each ABIE in the message, a corresponding ABIE type must be 
    ///    constructed, which is used as a type for references from other ABIE's (ASBIE's).
    /// 3) Generation of Message Assembly element, which is the root of the message.
    /// </summary>
    internal class XMLSchema: Schema
    {
        // Configuration properties used by this module:
        private const string _XMLSchemaStdNamespace        = "XMLSchemaStdNamespace";
        private const string _XMLSchemaStdNamespaceToken   = "XMLSchemaStdNamespaceToken";

        private XmlSchema _schema;                  // The current XML schema.
        private XmlSchemaSet _set;                  // Group of all schemas referenced from the current schema.
        private List<string> _foreignSchemaNames;   // Used to store pointers to external schemas that are imported by this particular schema.
        private List<string> _classifierList;       // List of all classifiers we have defined so far.

        /// <summary>
        /// Schema constructor.
        /// Since schemas are constructed dynamically, the constructor does not perform any operations. After construction, the schema MUST be explicitly
        /// initialized via a call to 'Initialize'. Until that has been done, object state is indeterminate.
        /// The constructor MUST be declared public since it is called by the .Net Invocation framework, which is in another assembly!
        /// </summary>
        public XMLSchema()
        {
            this._schema = null;
            this._set = null;
            this._foreignSchemaNames = null;
        }

        /// <summary>
        /// Creates a new ABIE type and adds the list of content- and supplementary attributes as well as a list of associated classes to this type. 
        /// An association (ASBIE) is constructed as a sub-element of the type of the associated class (e.g. the ABIE is considered the 'source' of the association).
        /// An ABIE thus consists of a complex type, containing a sequence of content attribute elements as well as a sequence of associated classes (ASBIE's).
        /// The ABIE supports three sets of attributes:
        /// - Content Attributes - will be translated to a sequence of xsd:elements.
        ///                        Each element must be of a CDT (or BDT) type that must have been created earlier using one of the 'addxxxBBIEType' operations.
        /// - Supplementary Attributes - will be translated to xsd:attribute components.
        /// Both sets of attributes are passed as a single list and correct processing is managed by the Schema.
        /// If an ABIE with the specified name already exists, no processing takes place and 'true' is returned.
        /// </summary>
        /// <param name="nodeID">Unique numeric identification of the ABIE (not used here).</param>
        /// <param name="className">The name of the business component (ABIE name).</param>
        /// <param name="annotation">Optional comment for this element (empty list in case of no comment).</param>
        /// <param name="attributeList">List of content- and supplementary attributes.</param>
        /// <param name="associationList">List of associated classes (ASBIE).</param>
        /// <returns>TRUE in case of success, FALSE on errors. If the type already exists, no action is performed and the operation returns TRUE.</returns>
        internal override bool AddABIEType(int nodeID, string className, List<MEDocumentation> annotation, 
                                           List<SchemaAttribute> attributeList, List<SchemaAssociation> associationList)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addABIEType >> Adding class " + className);

            try
            {
                var elementList = new SortedList<SortableSchemaElement, XmlSchemaParticle>();
                var choiceList = new SortedList<string, XMLChoice>();
                var newType = new XmlSchemaComplexType() { Name = className };

                if (!this._classifierList.Contains(className))
                {
                    this._schema.Items.Add(newType);
                    this._classifierList.Add(className);
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addABIEType " + this.NSToken + ">> Successfully added class '" + newType.Name + "'.");
                }
                else
                {
                    Logger.WriteWarning("Warning: duplicate class '" + newType.Name + "' in schema with namespace token '" + this.NSToken + "' skipped.");
                    SetLastError(string.Empty);
                    return true;
                }

                // Add (list of) annotation(s) to the element...
                if (annotation.Count > 0)
                {
                    Logger.WriteInfo("Framework.Util.XML.addABIEType >> Adding annotation to element...");
                    var annotationNode = new XmlSchemaAnnotation();
                    newType.Annotation = annotationNode;
                    foreach (MEDocumentation docNode in annotation)
                    {
                        annotationNode.Items.Add(docNode.GetXmlDocumentationNode());
                    }
                }

                // Iterate through list of attributes (simple elements) for this class...
                foreach (SchemaAttribute att in attributeList)
                {
                    if (att.IsValid && att.AttributeType == SchemaAttribute.AttributeTypeCode.Content)
                    {
                        var contentAttrib = att as XMLContentAttribute;     // Want to avoid a lot of explicit casts ;-)
                        // Check whether this attribute (simple element) is part of a choice group. If so, check whether we have created an instance of this
                        // choice object. If not, first create the new choice object. Finally, add the attribute to the choice.
                        if (contentAttrib.IsChoiceElement)
                        {
                            var choiceGroup = contentAttrib.ChoiceGroup;
                            if (!choiceList.ContainsKey(choiceGroup.GroupID))
                            {
                                Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addABIEType " + this.NSToken + ">> Creating new Choice with name: " + choiceGroup.GroupID);
                                var newChoice = new XMLChoice(this, choiceGroup);
                                choiceList.Add(choiceGroup.GroupID, newChoice);
                            }
                            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addABIEType " + this.NSToken + ">> Adding attribute '" + contentAttrib.SchemaElement.Name +
                                             "' to Choice Group '" + choiceGroup.GroupID + "'...");
                            choiceList[choiceGroup.GroupID].AddContentAttribute(contentAttrib);
                        }
                        else
                        {
                            XmlSchemaElement schemaElement = contentAttrib.SchemaElement;
                            var sortKey = new SortableSchemaElement(schemaElement, att.SequenceKey);
                            elementList.Add(sortKey, schemaElement);
                            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addABIEType " + this.NSToken + ">> Sorted content attribute '" + schemaElement.Name + "'.");
                        }
                    }
                    else if (att.IsValid && att.AttributeType == SchemaAttribute.AttributeTypeCode.Supplementary)
                    {
                        XmlSchemaAttribute schemaAttribute = ((XMLSupplementaryAttribute)att).XMLSchemaAttribute;
                        newType.Attributes.Add(schemaAttribute);
                        Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addABIEType " + this.NSToken + " >> Added supplementary attribute '" + schemaAttribute.Name + "'.");
                    }
                    else
                    {
                        Logger.WriteError("Framework.Util.SchemaManagement.XML.XMLSchema.addABIEType " + this.NSToken + " >> Illegal attribute type detected!");
                    }
                }

                // Iterate through list of associations (constructed elements) for this class...
                foreach (XMLAssociation associatedClass in associationList)
                {
                    // Check whether this association (constructed element) is part of a choice group. If so, check whether we have created an instance of this
                    // choice object. If not, first create the new choice object. Finally, add the association to the choice.
                    if (associatedClass.IsChoiceElement)
                    {
                        var choiceGroup = associatedClass.ChoiceGroup;
                        if (!choiceList.ContainsKey(choiceGroup.GroupID))
                        {
                            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addABIEType " + this.NSToken + ">> Creating new Choice with name: " + choiceGroup.GroupID);
                            var newChoice = new XMLChoice(this, choiceGroup);
                            choiceList.Add(choiceGroup.GroupID, newChoice);
                        }
                        Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addABIEType " + this.NSToken + ">> Adding association '" + associatedClass.SchemaElement.Name +
                                         "' to Choice Group '" + choiceGroup.GroupID + "'...");
                        choiceList[choiceGroup.GroupID].AddAssociation(associatedClass);
                    }
                    else
                    {
                        XmlSchemaElement schemaElement = associatedClass.SchemaElement;
                        var sortKey = new SortableSchemaElement(schemaElement, associatedClass.SequenceKey);
                        elementList.Add(sortKey, schemaElement);
                        Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addABIEType " + this.NSToken + " >> Sorted association '" + schemaElement.Name + "'.");
                    }
                }

                // Now that we have processed all attributes and associations, we can add the collected choices to our element list...
                foreach (KeyValuePair<string, XMLChoice> choice in choiceList)
                {
                    if (choice.Value.IsValid)
                    {
                        var sortKey = new SortableSchemaElement(choice.Value, choice.Value.SequenceKey);
                        elementList.Add(sortKey, choice.Value.SchemaObject);
                        Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addABIEType " + this.NSToken + " >> Sorted Choice '" + choice.Value.Name + "'.");
                    }
                    else
                    {
                        string lastError = "Choice Group '" + choice.Value.Name + "' in class '" + className + "' only has a single element!";
                        Logger.WriteError("Framework.Util.SchemaManagement.XML.XMLSchema.addABIEType " + this.NSToken + " >> " + lastError);
                        return false;
                    }
                }

                // Now we can move the contents of the sorted list to the item-list of this element....
                // If we have only a single element of type 'choice', we should not create the sequence...
                if (elementList.Count == 1 && elementList.Keys[0].IsChoice)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addABIEType " + this.NSToken + " >> Adding single element of type Choice...");
                    newType.Particle = elementList.Values[0];
                }
                // In ALL other cases, even with single elements, we need a sequence to order the element(s) within the parent...
                // EXCEPT in case of empty element (no attributes, no associations), in which case we don't want an empty sequence.
                else if (elementList.Count > 0)
                {
                    var typeBodySequence = new XmlSchemaSequence();
                    newType.Particle = typeBodySequence;
                    foreach (KeyValuePair<SortableSchemaElement, XmlSchemaParticle> sortedElement in elementList)
                    {
                        typeBodySequence.Items.Add(sortedElement.Value);
                    }
                }
                SetLastError(string.Empty);
                return true;
            }
            catch (System.SystemException exc)
            {
                Logger.WriteError("Framework.Util.SchemaManagement.XML.XMLSchema.addBBIEType " + this.NSToken + " >> adding element failed because: " + exc.ToString());
                SetLastError(exc.Message);
                return false;
            }
        }

        /// <summary>
        /// This operation is used to create a type definition for a complex-type Basic Business Information Entity (BBIE). These are always of type CDT (or BDT derived from CDT).
        /// The classifierName MUST be a PRIM type, so the caller must have parsed the entire class hierarchy until arrival at PRIM level.
        /// Note that in XML schema, a complex type that is derived from a primitive (as is the case here) can NOT have any facets directly! 
        /// Also, the only derivation mechanism allowed is 'extension'.
        /// Because of this, assigning facets implies that first a 'facetted primitive' is created which is subsequently used as the base of the complex type, e.g.:
        /// <![CDATA[
        /// <xs:simpleType name="MyClassifierBinaryObjectType">
        ///   <xs:restriction base="xs:base64Binary">
        ///     <xs:maxLength value = "255" />
        ///   </ xs:restriction>
        /// </xs:simpleType>
        ///
        /// <xs:complexType name = "MyClassifier" >
        ///   <xs:simpleContent>
        ///     <xs:extension base="cmn:MyClassifierBinaryObjectType">
        ///       <xs:attribute name = "MyAttribute" type="xs:string"/>
        ///     </xs:extension>
        ///   </xs:simpleContent>
        /// </xs:complexType>
        /// ]]>
        /// Note that we're in fact creating classifiers here (these are definitions used as classifiers in attributes of other elements). 
        /// To differentiate between the 'regular' primitive types and facetted types, we add the classifier name as a prefix to the primitive (since different classifiers
        /// might use different facets for the same primitive).
        /// </summary>
        /// <param name="classifierName">The name of the classifier to be constructed. The name must be unique within the current namespace. If not, it will be skipped!</param>
        /// <param name="primType">Associated PRIM type</param>
        /// <param name="annotation">Optional comment for the classifier (empty list in case of no comment).</param>
        /// <param name="attribList">Optional list of attributes to be assigned to this type</param>
        /// <param name="facetList">Optional list of facets that must be applied to the primitive type of the classifier.</param>
        /// <returns>TRUE in case of success, FALSE on errors. If the type already exists, no action is performed and the operation returns TRUE.</returns>
        internal override bool AddComplexClassifier(string classifierName, string primType, List<MEDocumentation> annotation, List<SupplementaryAttribute> attribList, List<Facet> facetList)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addComplexClassifier " + this.NSToken + " >> Adding classifier " + classifierName + " with type " + primType);

            try
            {
                var newClassifier = new XmlSchemaComplexType() { Name = classifierName };

                if (!this._classifierList.Contains(classifierName))
                {
                    this._schema.Items.Add(newClassifier);
                    this._classifierList.Add(classifierName);
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addComplexClassifier " + this.NSToken + " >> Successfully added classifier '" + newClassifier.Name + "' with type '" + primType + "'.");
                }
                else
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addComplexClassifier " + this.NSToken + " >> Duplicate type '" + newClassifier.Name + "' skipped!");
                    SetLastError(string.Empty);
                    return true;
                }

                // Adding (list of) annotation(s) to this classifier...
                if (annotation.Count > 0)
                {
                    Logger.WriteInfo("Framework.Util.XML.addComplexClassifier >> Adding annotation to classifier...");
                    var annotationNode = new XmlSchemaAnnotation();
                    newClassifier.Annotation = annotationNode;
                    foreach (MEDocumentation docNode in annotation)
                    {
                        annotationNode.Items.Add(docNode.GetXmlDocumentationNode());
                    }
                }

                // We have to check the facetList for a possible type replacement (might happen for e.g. decimals, which could be transformed into integers)...
                foreach (Facet facet in facetList)
                {
                    string replacementType = facet.CheckReplaceType(primType);
                    if (replacementType != string.Empty)
                    {
                        Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addSimpleClassifier >> Facet forced replacement from '" + primType + "' to '" + replacementType + "'!");
                        primType = replacementType;
                        facetList.Remove(facet);        // Remove from list, has fulfilled its purpose.
                        break;
                    }
                }

                XMLPrimitiveType simpleType = (facetList != null && facetList.Count > 0) ? new XMLPrimitiveType(this, classifierName, primType, facetList) : new XMLPrimitiveType(this, primType);

                // Any type is an exception here since this requires complex content...
                if (String.Compare(primType, "anytype", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addComplexClassifier >> Creating 'any' type...");
                    var complexContent = new XmlSchemaComplexContent();
                    newClassifier.ContentModel = complexContent;
                    var complexExtension = new XmlSchemaComplexContentExtension();
                    complexContent.Content = complexExtension;

                    complexExtension.BaseTypeName = simpleType.QualifiedType;
                    foreach (XMLSupplementaryAttribute attrib in attribList) complexExtension.Attributes.Add(attrib.XMLSchemaAttribute);
                }
                else
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addComplexClassifier >> Creating extended derivation for complex type...");
                    var simpleContent = new XmlSchemaSimpleContent();
                    newClassifier.ContentModel = simpleContent;

                    // A Complex Type can not have restrictions if it is derived from a primitive base type (as is the case here)! The only possible derivation method is extension.
                    var simpleExtension = new XmlSchemaSimpleContentExtension();
                    simpleContent.Content = simpleExtension;
                    simpleExtension.BaseTypeName = simpleType.QualifiedType;
                    foreach (XMLSupplementaryAttribute attrib in attribList) simpleExtension.Attributes.Add(attrib.XMLSchemaAttribute);
                }
                SetLastError(string.Empty);
                return true;
            }
            catch (System.SystemException exc)
            {
                Logger.WriteError("Framework.Util.SchemaManagement.XML.XMLSchema.addComplexClassifier " + this.NSToken + " >> adding complex type failed because: " + exc.ToString());
                SetLastError(exc.Message);
                return false;
            }
        }

        /// <summary>
        /// Helper function that directly adds a schemaObject to the internal XML schema. Must be used with caution!
        /// The method checks whether the type to be added is indeed a new one. If it already exists, the method returns false. 
        /// </summary>
        /// <param name="schemaObject">The schema object to be added.</param>
        /// <returns>True on successfull completion, false if object already exists.</returns>
        internal bool AddToSchema(XmlSchemaObject schemaObject)
        {
            bool success = true;
            if (!this._schema.Items.Contains(schemaObject)) this._schema.Items.Add(schemaObject);
            else success = false;
            return success;
        }

        /// <summary>
        /// This operation is used to create a type definition for an enumerated-type Basic Business Information Entity (BBIE). Enumerations can have attributes but no facets 
        /// (other then the enumeration values). If the enumeration has attributes (attribList not empty), it is created as a complex type, derived from a simple type where the
        /// simple type contains the enumeration facets and the complex type contains the attributes. If the attribute list is empty, only a simple type will be created.
        /// </summary>
        /// <param name="classifierName">BBIE classifier name. The name must be unique within the current namespace. If not, it will be skipped!</param>
        /// <param name="annotation">Optional classifier documentation, empty list in case of no comments.</param>
        /// <param name="attribList">Optional list of attributes to be assigned to this type</param>
        /// <param name="enumList">The list of enumeration values.</param>
        /// <returns>TRUE in case of success, FALSE on errors. If the type already exists, no action is performed and the operation returns TRUE.</returns>
        internal override bool AddEnumClassifier(string classifierName, List<MEDocumentation> annotation, List<SupplementaryAttribute> attribList, List<EnumerationItem> enumList)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addEnumClassifier " + this.NSToken + " >> Adding enumeration classifier " + classifierName);

            try
            {
                // If we have an enumeration with supplementary attributes, we need two types: one simple retriction of string to hold the enumeration facets and one complex extension of this
                // simple type to hold the attributes. The latter (complex type) is the one that must actually be used as classifier.
                if (attribList.Count > 0)
                {
                    string baseTypeName = (classifierName.EndsWith("Type")) ? classifierName.Substring(0, classifierName.Length - 4) : classifierName;
                    baseTypeName += "BaseType";
                    var baseType = new XmlSchemaSimpleType() { Name = baseTypeName };
                    if (!this._classifierList.Contains(baseTypeName))
                    {
                        if (annotation.Count > 0)
                        {
                            Logger.WriteInfo("Framework.Util.XML.addEnumClassifier >> Adding annotation to element...");
                            var annotationNode = new XmlSchemaAnnotation();
                            baseType.Annotation = annotationNode;
                            foreach (MEDocumentation docNode in annotation)
                            {
                                annotationNode.Items.Add(docNode.GetXmlDocumentationNode());
                            }
                        }

                        // First, build the simple-type base type and add the enumeration facets...
                        var restriction = new XmlSchemaSimpleTypeRestriction();
                        baseType.Content = restriction;
                        var simpleType = new XMLPrimitiveType(this, "string");
                        restriction.BaseTypeName = simpleType.QualifiedType;
                        foreach (EnumerationItem enumItem in enumList)
                        {
                            var enumFacet = new XMLFacet("enumeration", enumItem.Name);
                            XmlSchemaFacet xmlEnumFacet = enumFacet.GetXmlFacet();

                            if (enumItem.Annotation.Count > 0)
                            {
                                Logger.WriteInfo("Framework.Util.XML.addEnumClassifier >> Adding annotation to enumeration item...");
                                var annotationNode = new XmlSchemaAnnotation();
                                xmlEnumFacet.Annotation = annotationNode;
                                foreach (MEDocumentation docNode in enumItem.Annotation)
                                {
                                    annotationNode.Items.Add(docNode.GetXmlDocumentationNode());
                                }
                            }
                            restriction.Facets.Add(xmlEnumFacet);
                        }

                        // Next, build the derived complex type and add the attributes...
                        var derivedType = new XmlSchemaComplexType() { Name = classifierName };
                        if (!this._classifierList.Contains(classifierName))
                        {
                            this._schema.Items.Add(baseType);     // Add the base type.
                            this._schema.Items.Add(derivedType);  // And add the derived type.
                            this._classifierList.Add(baseTypeName);
                            this._classifierList.Add(classifierName);
                            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addEnumClassifier " + this.NSToken + " >> Successfully added enumeration '" + derivedType.Name + "'.");

                            var simpleContent = new XmlSchemaSimpleContent();
                            derivedType.ContentModel = simpleContent;

                            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addEnumClassifier " + this.NSToken + " >> Creating extended derivation for attribute storage...");
                            var extension = new XmlSchemaSimpleContentExtension();
                            simpleContent.Content = extension;
                            extension.BaseTypeName = new XmlQualifiedName(baseTypeName, this.NSToken);
                            foreach (XMLSupplementaryAttribute attrib in attribList) extension.Attributes.Add(attrib.XMLSchemaAttribute);
                        }
                        else
                        {
                            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addEnumClassifier " + this.NSToken + " >> Duplicate enumeration '" + derivedType.Name + "' skipped!");
                        }
                    }
                    else
                    {
                        Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addEnumClassifier " + this.NSToken + " >> Duplicate enumeration base '" + baseTypeName + "' Skipped!");
                    }
                    SetLastError(string.Empty);
                    return true;
                }
                else
                {
                    // Enumeration without attributes can be a simple string restriction.
                    var newType = new XmlSchemaSimpleType() { Name = classifierName };
                    if (!this._classifierList.Contains(classifierName))
                    {
                        this._schema.Items.Add(newType);
                        this._classifierList.Add(classifierName);
                        Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addEnumClassifier " + this.NSToken + " >> Successfully added enumeration '" + newType.Name + "'.");
                    }
                    else
                    {
                        Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addEnumClassifier " + this.NSToken + " >> Duplicate enumeration '" + newType.Name + "' skipped!");
                        SetLastError(string.Empty);
                        return true;
                    }

                    if (annotation.Count > 0)
                    {
                        Logger.WriteInfo("Framework.Util.XML.addEnumClassifier >> Adding annotation to element...");
                        var annotationNode = new XmlSchemaAnnotation();
                        newType.Annotation = annotationNode;
                        foreach (MEDocumentation docNode in annotation)
                        {
                            annotationNode.Items.Add(docNode.GetXmlDocumentationNode());
                        }
                    }

                    // The derivation method for these simple types is always a restriction!
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addEnumClassifier " + this.NSToken + " >> Creating restricted derivation for simple type...");
                    var restriction = new XmlSchemaSimpleTypeRestriction();
                    newType.Content = restriction;
                    var simpleType = new XMLPrimitiveType(this, "string");
                    restriction.BaseTypeName = simpleType.QualifiedType;
                    foreach (EnumerationItem enumItem in enumList)
                    {
                        var enumFacet = new XMLFacet("enumeration", enumItem.Name);
                        XmlSchemaFacet xmlEnumFacet = enumFacet.GetXmlFacet();

                        if (enumItem.Annotation.Count > 0)
                        {
                            Logger.WriteInfo("Framework.Util.XML.Association >> Adding annotation to enumeration item...");
                            var annotationNode = new XmlSchemaAnnotation();
                            xmlEnumFacet.Annotation = annotationNode;
                            foreach (MEDocumentation docNode in enumItem.Annotation)
                            {
                                annotationNode.Items.Add(docNode.GetXmlDocumentationNode());
                            }
                        }
                        restriction.Facets.Add(xmlEnumFacet);
                    }
                    SetLastError(string.Empty);
                    return true;
                }
            }
            catch (System.SystemException exc)
            {
                Logger.WriteError("Framework.Util.SchemaManagement.XML.XMLSchema.addEnumClassifier " + this.NSToken + " >> adding enumeration failed because: " + exc.ToString());
                SetLastError(exc.Message);
                return false;
            }
        }

        /// <summary>
        /// Add a typedef that contains elements from an external schema. The schema details are passed as arguments to the call. The type might either contain
        /// elements of a specified base-type, or, if the name of the base-type is "Any", the type can contain any elements defined by the external schema.
        /// </summary>
        /// <param name="classifierName">Name of the classifier.</param>
        /// <param name="annotation">Any comment from the UML model or empty list in case of no comments.</param>
        /// <param name="ns">Namespace string.</param>
        /// <param name="nsToken">Namespace token to be used when referring to 'ns'.</param>
        /// <param name="schema">Fully qualified schema filename.</param>
        /// <param name="baseType">Name of base type, or 'Any' in case of any type.</param>
        /// <param name="cardinality">Classifier cardinality, an upper boundary of '0' is interpreted as 'unbounded'.</param>
        /// <returns>True when defined Ok, false on errors.</returns>
        internal override bool AddExternalClassifier(string classifierName, List<MEDocumentation> annotation,
                                                   string ns, string nsToken,
                                                   string schema, string baseType,
                                                   Tuple<int, int> cardinality)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addExternalClassifier " + this.NSToken + " >> Adding external classifier " + classifierName);

            try
            {
                var newClassifier = new XmlSchemaComplexType() { Name = classifierName };
                if (!this._classifierList.Contains(classifierName))
                {
                    if (!this._foreignSchemaNames.Contains(schema))
                    {
                        // First time we meet this external schema; make sure to add a namespace- and import declaration...
                        AddNamespace(nsToken, ns, schema);
                        this._foreignSchemaNames.Add(schema);   // Store filename for later access by schema validator and to keep track of detected schemas.
                        Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addExternalClassifier " + this.NSToken + " >> Added nsToken: " + nsToken + " with ns: '" + ns + "' and schema: '" + schema + "'!");
                    }
                    this._schema.Items.Add(newClassifier);
                    this._classifierList.Add(classifierName);
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addExternalClassifier " + this.NSToken + " >> Successfully added classifier '" + newClassifier.Name + "' in schema '" + schema + "'.");
                }
                else
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addExternalClassifier " + this.NSToken + " >> Duplicate type '" + newClassifier.Name + "' skipped!");
                    SetLastError(string.Empty);
                    return true;
                }

                if (annotation.Count > 0)
                {
                    Logger.WriteInfo("Framework.Util.XML.addExternalClassifier >> Adding annotation to external classifier...");
                    var annotationNode = new XmlSchemaAnnotation();
                    newClassifier.Annotation = annotationNode;
                    foreach (MEDocumentation docNode in annotation)
                    {
                        annotationNode.Items.Add(docNode.GetXmlDocumentationNode());
                    }
                }

                var typeBodySequence = new XmlSchemaSequence();
                newClassifier.Particle = typeBodySequence;

                if (baseType.Equals("Any", StringComparison.OrdinalIgnoreCase))
                {
                    var anyElement = new XmlSchemaAny()
                    {
                        Namespace = ns,
                        ProcessContents = XmlSchemaContentProcessing.Strict,     // Elements must adhere to specified schema!
                        MinOccurs = cardinality.Item1
                    };
                    if (cardinality.Item2 == 0) anyElement.MaxOccursString = "unbounded";
                    else anyElement.MaxOccurs = cardinality.Item2;
                    typeBodySequence.Items.Add(anyElement);
                }
                else
                {
                    // Classifier is regular type, create element of that type...
                    var externalElement = new XmlSchemaElement()
                    {
                        SchemaTypeName = new XmlQualifiedName(baseType, ns),
                        Name = (classifierName.EndsWith("Type", StringComparison.Ordinal) ? 
                                classifierName.Substring(0, classifierName.LastIndexOf("Type", StringComparison.Ordinal)) : classifierName) + "Element"
                    };

                    Logger.WriteInfo("Framework.Util.XML.addExternalClassifier >> Set Element name to: " + externalElement.Name);
                    Logger.WriteInfo("Framework.Util.XML.addExternalClassifier >> Set SchemaTypeName to: " + externalElement.SchemaTypeName);
                    externalElement.MinOccurs = cardinality.Item1;
                    if (cardinality.Item2 == 0) externalElement.MaxOccursString = "unbounded";
                    else externalElement.MaxOccurs = cardinality.Item2;
                    typeBodySequence.Items.Add(externalElement);
                }
                SetLastError(string.Empty);
                return true;
            }
            catch (System.SystemException exc)
            {
                Logger.WriteError("Framework.Util.SchemaManagement.XML.XMLSchema.addExternalClassifier " + this.NSToken + " >> adding complex type failed because: " + exc.ToString());
                SetLastError(exc.Message);
                return false;
            }
        }

        /// <summary>
        /// Adds an element to the schema. The element name must be unique within the schema and the classifier must have been defined earlier, wither in the
        /// current schema or in the schema identified by the specified namespace.
        /// </summary>
        /// <param name="elementName">Name of the element.</param>
        /// <param name="classifierNamespace">Namespace of the element classifier.</param>
        /// <param name="classifierName">Name of the element classifier (type of the element).</param>
        /// <param name="annotation">Optional comment for the element (empty list in case of no comment).</param>
        /// <returns>True on success or false on errors.</returns>
        internal override bool AddElement(string elementName, string classifierNamespace, string classifierName, List<MEDocumentation> annotation)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addElement " + this.NSToken + " >> Adding element '" + elementName + "' with classifier '" + classifierNamespace + ":" + classifierName + "'...");
            bool success;
            try
            {
                var newElement = new XmlSchemaElement()
                {
                    Name = elementName,
                    SchemaTypeName = new XmlQualifiedName(classifierName, classifierNamespace)
                };
                if (!this._schema.Items.Contains(newElement))
                {
                    if (annotation.Count > 0)
                    {
                        Logger.WriteInfo("CDMMgr.XMLSchema.addElement >> Adding annotation to element...");
                        var annotationNode = new XmlSchemaAnnotation();
                        newElement.Annotation = annotationNode;
                        foreach (MEDocumentation docNode in annotation)
                        {
                            annotationNode.Items.Add(docNode.GetXmlDocumentationNode());
                        }
                    }
                    this._schema.Items.Add(newElement);
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addElement " + this.NSToken + " >> Successfully added element '" + newElement.Name + "'.");
                    success = true;
                }
                else
                {
                    SetLastError("Error: duplicate element '" + newElement.Name + "' in schema with namespace token '" + this.NSToken + "' not allowed!");
                    Logger.WriteError("Framework.Util.SchemaManagement.XML.XMLSchema.addMessage " + this.NSToken + " >> " + this.LastError);
                    success = false;
                }
            }
            catch (System.SystemException exc)
            {
                Logger.WriteError("Framework.Util.SchemaManagement.XML.XMLSchema.addMessageAssembly " + this.NSToken + " >> adding element failed because: " + exc.ToString());
                SetLastError(exc.Message);
                success = false;
            }
            if (success) SetLastError(string.Empty);
            return success;
        }

        /// <summary>
        /// This method adds an external namespace to the schema. Regarding the location: it is most convenient to keep all schemas together, in which case this
        /// argument will look something like: './schemaname.xsd'. Location could by NULL if no explicit import is required.
        /// The method will silently fail (with a logged error) in case of exceptions.
        /// </summary>
        /// <param name="nsToken">Namespace token as used throughout the schema to reference this external namespace.</param>
        /// <param name="nsName">The fully qualified namespace name, typically an URI or URL.</param>
        /// <param name="nsLocation">The location of the namespace as used during import of the namespace. Must refer to a valid location. NULL if no explicit import required.</param>
        internal override void AddNamespace(string nsToken, string nsName, string nsLocation)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addNamespace " + this.NSToken + " >> Adding namespace: " + nsToken + ":" + nsName);
            try
            {
                this._schema.Namespaces.Add(nsToken, nsName);

                if (nsLocation != null)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addNamespace " + this.NSToken + " >> creating import declaration for location: " + nsLocation);
                    var import = new XmlSchemaImport()
                    {
                        Namespace = nsName,
                        SchemaLocation = nsLocation
                    };
                    this._schema.Includes.Add(import);
                }
                SetLastError(string.Empty);
            }
            catch (System.SystemException exc)
            {
                Logger.WriteError("Framework.Util.SchemaManagement.XML.XMLSchema.addNamespace " + this.NSToken + " >> adding namespace failed because: " + exc.ToString());
                SetLastError(exc.Message);
            }
        }

        /// <summary>
        /// Adds an external schema to the set of schemas referenced by the current schema.
        /// </summary>
        /// <param name="referencedSchema">The external schema that is referenced by the current schema.</param>
        internal override void AddSchemaReference(Schema referencedSchema)
        {
            if (referencedSchema != null && referencedSchema is XMLSchema)
            {
                var xmlSchema = referencedSchema as XMLSchema;
                Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.AddSchemaReference " + this.NSToken + " >> Adding schema '" + 
                                  referencedSchema.SchemaNamespace + "' to my schema...");
                if (!this._set.Contains(xmlSchema._schema)) this._set.Add(xmlSchema._schema);
            }
        }

        /// <summary>
        /// This operation is used to create a type definition for a simple-type Basic Business Information Entity (BBIE). These are always of type CDT (or derived from CDT).
        /// The typeName MUST be a PRIM type, so the caller must have parsed the entire class hierarchy until arrival at PRIM level.
        /// Simple types are allowed to have facets and the derivation method of a simple type that is derived from a primitive is always a restriction, a list or a union!
        /// Enumerations and Unions must NOT be added using this method, but must use methods 'addEnumClassifier' and 'addUnionClassifier' respectively.
        /// If an attribute list is specified, the simple type becomes a complex type (holding the attributes) around a simple type (optionally holding the facets). This
        /// is implemented by simply invoking the 'addComplexClassifier' method.
        /// </summary>
        /// <param name="classifierName">BBIE classifier name</param>
        /// <param name="typeName">Associated PRIM type</param>
        /// <param name="annotation">Optional comment for the classifier (empty list in case of no comment).</param>
        /// <param name="attribList">Optional list of attributes to be assigned tot the classifier. Since a simple classifier can not directly contain attributes,
        /// the type is converted into a complex type instead!</param>
        /// <param name="facetList">Optional list of facets to be applied to this type</param>
        /// <returns>TRUE in case of success, FALSE on errors. If the type already exists, no action is performed and the operation returns TRUE.</returns>
        internal override bool AddSimpleClassifier(string classifierName, string typeName, List<MEDocumentation> annotation, List<SupplementaryAttribute> attribList, List<Facet> facetList)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addSimpleClassifier " + this.NSToken + " >> Adding classifier " + classifierName + " with type " + typeName);

            try
            {
                // If we have attributes, the simple classifier becomes a complex classifier instead!
                if (attribList != null && attribList.Count > 0) return AddComplexClassifier(classifierName, typeName, annotation, attribList, facetList);
                else
                {
                    var newType = new XmlSchemaSimpleType() { Name = classifierName };
                    if (!this._classifierList.Contains(classifierName))
                    {
                        if (annotation.Count > 0)
                        {
                            Logger.WriteInfo("Framework.Util.XML.addSimpleClassifier >> Adding annotation to simple classifier...");
                            var annotationNode = new XmlSchemaAnnotation();
                            newType.Annotation = annotationNode;
                            foreach (MEDocumentation docNode in annotation)
                            {
                                annotationNode.Items.Add(docNode.GetXmlDocumentationNode());
                            }
                        }
                        this._schema.Items.Add(newType);
                        this._classifierList.Add(classifierName);
                        Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addSimpleClassifier " + this.NSToken + " >> Successfully added classifier '" + newType.Name + "' with type '" + typeName + "'.");
                    }
                    else
                    {
                        Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addSimpleClassifier " + this.NSToken + " >> Duplicate type '" + newType.Name + "' skipped!");
                        SetLastError(string.Empty);
                        return true;
                    }

                    // The derivation method for these simple types is always a restriction!
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addSimpleClassifier >> Creating restricted derivation for simple type...");
                    var restriction = new XmlSchemaSimpleTypeRestriction();
                    newType.Content = restriction;

                    // We have to check the facetList for a possible type replacement (might happen for e.g. decimals, which could be transformed into integers)...
                    foreach (XMLFacet facet in facetList)
                    {
                        string replacementType = facet.CheckReplaceType(typeName);
                        if (replacementType != string.Empty)
                        {
                            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.addSimpleClassifier >> Facet forced replacement from '" + typeName + "' to '" + replacementType + "'!");
                            typeName = replacementType;
                            facetList.Remove(facet);        // Remove from list, has fulfilled its purpose.
                            break;
                        }
                    }

                    var simpleType = new XMLPrimitiveType(this, typeName);
                    restriction.BaseTypeName = simpleType.QualifiedType;
                    foreach (XMLFacet facet in facetList)
                    {
                        if (facet.IsValid(simpleType)) restriction.Facets.Add(facet.GetXmlFacet());
                    }
                    SetLastError(string.Empty);
                    return true;
                }
            }
            catch (System.SystemException exc)
            {
                Logger.WriteError("Framework.Util.SchemaManagement.XML.XMLSchema.addSimpleClassifier " + this.NSToken + " >> adding simple type failed because: " + exc.ToString());
                SetLastError(exc.Message);
                return false;
            }
        }

        /// <summary>
        /// Returns a schema-specific file extension.
        /// </summary>
        /// <returns>Schema-specific file extension, including separator character (e.g. ".xsd").</returns>
        internal override string GetSchemaFileExtension()
        {
            return ".xsd";
        }

        /// <summary>
        /// Schema initializer.
        /// The XMLSchema class is responsible for the construction of an XML schemafile for a Service model (XSD). One class instance corresponds to 
        /// one service model. We recognize three different types of schemas:
        /// 1) Collection: has no explicit type or title, but consists merely of a list of type definitions that can be used by other schemas.
        /// 2) Operation: corresponds to an operation that typically contains a request- and response message element.
        /// 3) Message: the schema represents one single message, typically represented by a single 'Message' element.
        /// The initializer accepts the schema type and name, a namespaceToken (mnemonic name that guarantees that elements within the schema are unique), 
        /// the namespace to be used for the schema and an optional schema version. When omitted, the version is set to '1.0.0'.
        /// </summary>
        /// <param name="type">Identifies the type of schema that we're building.</param>
        /// <param name="name">A meaningfull name, with which we can identify the schema.</param>
        /// <param name="namespaceToken">Namespace token, not used for JSON schemas.</param>
        /// <param name="schemaNamespace">Schema namespace, must adhere to JSON Schema 'Id' specifications.</param>
        /// <param name="version">Major, minor and build number of the schema. When omitted, the version defaults to '1.0.0'</param>
        internal override void Initialize(SchemaType type, string name, string namespaceToken, string schemaNamespace, string version = "1.0.0")
        {
            if (!IsInitialized)
            {
                base.Initialize(type, name, namespaceToken, schemaNamespace, version);

                this._schema = new XmlSchema()
                {
                    AttributeFormDefault = XmlSchemaForm.Unqualified,
                    ElementFormDefault = XmlSchemaForm.Qualified,
                    Version = version,
                    TargetNamespace = schemaNamespace
                };
                this._schema.Namespaces.Add(namespaceToken, schemaNamespace);
                this._foreignSchemaNames = new List<string>();
                this._set = new XmlSchemaSet();
                this._set.Add(this._schema);
                this._classifierList = new List<string>();

                Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema >> instantiated new schema with token: '" + namespaceToken + "', namespace: '" +
                                 schemaNamespace + "' and version: '" + version + "'.");
            }
        }

        /// <summary>
        /// Merges all the objects in the provided schema (other) with our current schema.
        /// </summary>
        /// <param name="other">The schema to copy from.</param>
        internal override void Merge(Schema other)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.merge " + this.NSToken + " >> Going to merge other schema with ours...");
            XMLSchema otherSchema = other as XMLSchema;
            foreach (XmlSchemaObject o in otherSchema._schema.Items)
            {
                if (!this._schema.Items.Contains(o))
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.merge " + this.NSToken + " >> Merging object: " + o.ToString());
                    this._schema.Items.Add(o);
                }
                else
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.merge " + this.NSToken + " >> Skipping duplicate item.");
                }
            }

            // Merge any external schema names (plus all related namespace- and import declarations) as well, these are referenced by the merged schema...
            if (otherSchema._foreignSchemaNames.Count > 0)
            {
                Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.merge " + this.NSToken + " >> Merging schema names: " + otherSchema._foreignSchemaNames);
                this._foreignSchemaNames.AddRange(otherSchema._foreignSchemaNames);
                XmlQualifiedName[] qNames = otherSchema._schema.Namespaces.ToArray();
                foreach (XmlQualifiedName qName in qNames)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.merge " + this.NSToken + " >> Merging namespace: " + qName.ToString());
                    this._schema.Namespaces.Add(qName.Name, qName.Namespace);
                }
                foreach (XmlSchemaObject obj in otherSchema._schema.Includes)
                {
                    this._schema.Includes.Add(obj);
                }
            }
        }

        /// <summary>
        /// This method first sorts the schema contents, then adds the provided header commentblock and finally writes the schema to the provided stream
        /// as an XML schema file.
        /// The header comment must be provided as ASCII text, without XML comment tokens!
        /// In case of compile- or save errors, an exception is thrown.
        /// </summary>
        /// <param name="stream">The stream to which to write the schema.</param>
        /// <param name="header">Header text that will be placed at the top of the schema document.</param>
        /// <exception cref="SystemException">Compiling of Schema object failed.</exception>
        internal override void Save(Stream stream, string header)
        {
            try
            {
                var doc = new XmlDocument();

                Sort();     // First of all, make sure that the schema is in proper order.

                this._set.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);
                SetLastError(string.Empty);

                // We have to explicitly collect all external schemas in order for the "Compile" to work. XmlSchemaSet must be able to validate the entire
                // schema set in order to be able to generate any output. 
                // Note that the Common Schema, if used, must have been added explicitly via a call to 'AddSchemaReference'.
                foreach (string schemaName in this._foreignSchemaNames)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.Save " + this.NSToken + " >> Adding external schema: " + schemaName);
                    this._set.Add("", new XmlTextReader(schemaName));
                }

                this._set.Compile();
                if (this.LastError != string.Empty)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.Save " + this.NSToken + " >> Validation errors occurred:" + Environment.NewLine + this.LastError);
                    throw new SystemException("Validation errors occurred:" + Environment.NewLine + this.LastError);
                }

                Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.Save " + this.NSToken + " >> Schema has been compiled Ok!");

                // The easiest way to get the compiled schema out of the schemas collection.
                XmlSchema compiledSchema = null;
                foreach (XmlSchema outSchema in this._set.Schemas())
                {
                    if (outSchema.TargetNamespace == this.SchemaNamespace) compiledSchema = outSchema;
                }

                var nsManager = new XmlNamespaceManager(new NameTable());
                ContextSlt context = ContextSlt.GetContextSlt();
                nsManager.AddNamespace(context.GetConfigProperty(_XMLSchemaStdNamespaceToken), context.GetConfigProperty(_XMLSchemaStdNamespace));
                using (XmlWriter writer = doc.CreateNavigator().AppendChild())
                {
                    compiledSchema.Write(writer, nsManager);
                }
                doc.InsertBefore(doc.CreateComment(header), doc.DocumentElement);  // Insert header in front of root element.
                doc.InsertBefore(doc.CreateXmlDeclaration("1.0", "UTF-8", ""), doc.FirstChild);
                doc.Save(stream);
                SetLastError(string.Empty);
            }
            catch (SystemException exc)
            {
                Logger.WriteError("Framework.Util.SchemaManagement.XML.XMLSchema.Save " + this.NSToken + " >> schema conversion failed because: " + exc.ToString());
                SetLastError(exc.Message);
                throw;
            }
        }

        /// <summary>
        /// Sorts the current schema according to the following rules:
        /// 1) Elements before types;
        /// 2) Lower sequence key before higher sequence key (only for key values != 0);
        /// 3) Names in alphabetical order (if no key is defined).
        /// </summary>
        internal override void Sort()
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.sort " + this.NSToken + " >> Going to sort this schema...");

            var sortedList = new SortedList<SortableSchemaElement, XmlSchemaObject>();
            foreach (XmlSchemaObject schemaObject in this._schema.Items)
            {
                if (schemaObject is XmlSchemaElement)
                {
                    var schemaElement = schemaObject as XmlSchemaElement;
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.sort " + this.NSToken + " >> Sorting: " + schemaElement.Name);
                    sortedList.Add(new SortableSchemaElement(schemaElement, 0), schemaObject);
                }
                else
                {
                    var schemaType = schemaObject as XmlSchemaType;
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.sort " + this.NSToken + " >> Sorting: " + schemaType.Name);
                    sortedList.Add(new SortableSchemaElement(schemaType, 0), schemaObject);
                }
            }

            // Now we have a sorted list of schema objects in sortedList. Move back to our Item list...
            this._schema.Items.Clear();
            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLSchema.merge " + this.NSToken + " >> Move sorted list back to XML Schema...");
            foreach (XmlSchemaObject listObject in sortedList.Values)
            {
                this._schema.Items.Add(listObject);
            }
        }

        /// <summary>
        /// Helper method that is attached to the schema set for validation purposes.
        /// </summary>
        /// <param name="sender">Originating caller (ignored)</param>
        /// <param name="args">Error message.</param>
        private void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            Logger.WriteError("Framework.Util.SchemaManagement.XML.XMLSchema.SchemaValidation >> Validation error: " + args.Message);
            string newError = this.LastError + "**ERROR: Validation error: " + args.Message + Environment.NewLine;
            SetLastError(newError);
        }
    }
}
