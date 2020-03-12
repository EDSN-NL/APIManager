using System;
using System.IO;
using System.Runtime.Remoting;
using System.Collections.Generic;
using Framework.Logging;
using Framework.Util;
using Framework.Util.SchemaManagement;
using Framework.Util.SchemaManagement.XML;
using Framework.Util.SchemaManagement.JSON;
using Framework.Model;
using Framework.Context;
using Plugin.Application.CapabilityModel.ASCIIDoc;

namespace Plugin.Application.CapabilityModel.SchemaGeneration
{
    /// <summary>
    /// This is the 'third part' of the SchemaProcessor Class definition, which contains all the supporting functions required for processing.
    /// </summary>
    internal partial class SchemaProcessor : CapabilityProcessor
    {
        // Configuration properties used by this module...
        private const string _ExternalSchemaCDTTypeName             = "ExternalSchemaCDTTypeName";
        private const string _ExternalSchemaNSAttribute             = "ExternalSchemaNSAttribute";
        private const string _ExternalSchemaNSTokenAttribute        = "ExternalSchemaNSTokenAttribute";
        private const string _ExternalSchemaAttribute               = "ExternalSchemaAttribute";
        private const string _ExternalSchemaBaseTypeAttribute       = "ExternalSchemaBaseTypeAttribute";
        private const string _ExternalSchemaMinOccAttribute         = "ExternalSchemaMinOccAttribute";
        private const string _ExternalSchemaMaxOccAttribute         = "ExternalSchemaMaxOccAttribute";
        private const string _SupplementaryAttStereotype            = "SupplementaryAttStereotype";
        private const string _FacetAttStereotype                    = "FacetAttStereotype";
        private const string _ServiceModelPkgName                   = "ServiceModelPkgName";
        private const string _SuppressEnumClassifier                = "SuppressEnumClassifier";
        private const string _CoreDataTypesPathName                 = "CoreDataTypesPathName";
        private const string _BasicProfileName                      = "BasicProfileName";
        private const string _ProfileStereotype                     = "ProfileStereotype";

        /// <summary>
        /// This method is invoked whenever we want to save the processed Capability to an output file. The method assures that the schema is sorted
        /// properly, converts it to an XML document and subsequently writes that document to the provided output stream. The method does not perform
        /// any control operations on the stream (e.g. does not attempt to close the stream).
        /// </summary>
        /// <param name="stream">Stream that must receive processed Capability contents.</param>
        protected override void SaveContents(FileStream stream)
        {
            // The method requires a reference to the common schema since the schema compiler must have access to ALL imported/included schema sets!!
            // It might have already been there, but that's no problem (method is guarded against multiple references).
            if (this._commonSchema != null) this._schema.AddSchemaReference(this._commonSchema);
            this._schema.Save(stream, BuildHeader());
        }

        /// <summary>
        /// Helper method that formats a schema header for use in schema export. The method replaces the header placeholders by the actual property
        /// contents retrieved from our current Capability.
        /// </summary>
        /// <returns>Header as a formatted string</returns>
        private string BuildHeader()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.BuildHeader >> Going to format header for capability: " + this._currentCapability.Name);

            ContextSlt context = ContextSlt.GetContextSlt();
            MEChangeLog.MigrateLog(this._currentCapability.CapabilityClass);
            string header = context.GetResourceString(FrameworkSettings._SOAPSchemaHeader);

            header = header.Replace("@SERVICE@", this._currentService.Name);
            header = header.Replace("@CAPABILITY@", Conversions.ToPascalCase(this._currentCapability.AssignedRole));
            header = header.Replace("@CAPABILITYTYPE@", this._currentCapability.CapabilityType);
            header = header.Replace("@AUTHOR@", this._currentCapability.Author);
            header = header.Replace("@TIMESTAMP@", DateTime.Now.ToString());
            header = header.Replace("@YEAR@", DateTime.Now.Year.ToString());
            header = header.Replace("@VERSION@", this._currentCapability.VersionString + " Build: " + this._currentService.BuildNumber);
            header = header.Replace("@ACCESSLEVEL@", "Not Applicable");

            string annotation = this._currentCapability.CapabilityClass.Annotation;
            if (!string.IsNullOrEmpty(annotation))
            {
                var newLog = new MEChangeLog(context.TransformRTF(annotation, RTFDirection.ToRTF));
                header = header.Replace("@CHANGELOG@", newLog.GetLogAsText());
            }
            else header = header.Replace("@CHANGELOG@", string.Empty);

            Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.BuildHeader >> Created header:\n" + header);
            return header;
        }

        /// <summary>
        /// This helper function receives a classifier data type as an argument. It determines the exact type of the classifier and creates a 
        /// suitable type definition, either in the common schema (if not NULL) or in the local schema if common schema is not provided. 
        /// </summary>
        /// <param name="classifier">The classifier element to be parsed.</param>
        /// <param name="scope">The Schema- and Documentation scopes of the classifier in the current processing context (documentation scope
        /// can be different from schema scope since classifiers are always defined in a common documentation chapter, even if we don't have 
        /// a common schema).</param>
        /// <returns>Classifier Context or NULL in case of (fatal) errors.</returns>
        private ClassifierContext DefineClassifier(MEDataType classifier, Tuple<ClassifierContext.ScopeCode, ClassifierContext.DocScopeCode> scope)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.DefineClassifier >> Processing classifier: " + 
                             scope + ":" + classifier.Name + "...");

            Tuple<MEDataType.MetaDataType, string> typeDescriptor = classifier.GetPrimitiveTypeName();
            if (typeDescriptor == null)
            {
                string message = "Can not obtain a [primitive] type for classifier '" + classifier.Name + "; type hierarchy might be incomplete or corrupted!";
                Logger.WriteError("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.DefineClassifier >> " + message);
                if (this._panel != null) this._panel.WriteError(this._panelIndex + 1, message);
                this._lastError = "Could not determine classifier type for classifier: " + classifier.Name;
                return null;
            }

            string typeName = typeDescriptor.Item2;
            var hierarchy = new SortedList<uint, MEClass>();
            ContextSlt context = ContextSlt.GetContextSlt();
            ClassifierContext classifierCtx = null;
            List<MEDataType.MetaDataDescriptor> classifierMetadata = classifier.GetMetaData();  // All Facets and/or Supplementary attributes.

            // Since classifiers are typically defined in the common schema and common doc. context. We use these as default, unless the received 
            // scope argument says otherwise...
            DocContext targetDocCtx = this._commonDocContext;
            Schema targetSchema = this._commonSchema;
            if (scope.Item2 == ClassifierContext.DocScopeCode.Local) targetDocCtx = this._currentOperationDocContext;
            if (targetSchema == null || scope.Item1 == ClassifierContext.ScopeCode.Operation || scope.Item1 == ClassifierContext.ScopeCode.Message) targetSchema = this._schema;

            if (typeName != string.Empty)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.DefineClassifier >> Classifier of type: " + typeName);
                switch (typeDescriptor.Item1)
                {
                    case MEDataType.MetaDataType.SimpleType:
                        {
                            // Scope of Simple classifier is ALWAYS Common or Operation.
                            List<Facet> facetList = GetFacets(classifierMetadata);
                            List<SupplementaryAttribute> attribList = GetSupplementaries(classifierMetadata);
                            if (this._useDocContext) targetDocCtx.AddClassifier(classifier, facetList, attribList, typeName);
                            targetSchema.AddSimpleClassifier(classifier.Name, typeName, classifier.GetDocumentation(), attribList, facetList);
                            classifierCtx = new ClassifierContext(ClassifierContext.ContentTypeCode.Simple, classifier.Name, scope, this._commonSchema != null);
                        }
                        break;

                    case MEDataType.MetaDataType.ComplexType:
                        {
                            // Scope of Complex classifier is ALWAYS Common or Operation.
                            List<Facet> facetList = GetFacets(classifierMetadata);
                            List<SupplementaryAttribute> attribList = GetSupplementaries(classifierMetadata);
                            // Create documentation first, facetList might be modified by the AddComplexClassifier method!!
                            if (this._useDocContext) targetDocCtx.AddClassifier(classifier, facetList, attribList, typeName); 
                            targetSchema.AddComplexClassifier(classifier.Name, typeName, classifier.GetDocumentation(), attribList, facetList);
                            classifierCtx = new ClassifierContext(ClassifierContext.ContentTypeCode.Complex, classifier.Name, scope, this._commonSchema != null);
                        }
                        break;

                    case MEDataType.MetaDataType.Enumeration:
                        {
                            MEEnumeratedType myEnum = classifier as MEEnumeratedType;
                            if (myEnum != null && myEnum.MustSuppressEnumeration)
                            {
                                var replacementClassifier = ModelSlt.GetModelSlt().FindDataType(context.GetConfigProperty(_CoreDataTypesPathName),
                                                                                                context.GetConfigProperty(_SuppressEnumClassifier));
                                if (replacementClassifier != null) return DefineClassifier(replacementClassifier, scope);
                                else
                                {
                                    Logger.WriteError("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.DefineClassifier >> Enumeration '" + 
                                                      myEnum.Name + "' has 'suppress-definitions' defined but we can't find replacement classifier '" + 
                                                      context.GetConfigProperty(_SuppressEnumClassifier) + "'!");
                                    return null;
                                }
                            }
                            else
                            {
                                List<SupplementaryAttribute> attribList = GetSupplementaries(classifierMetadata);
                                List<EnumerationItem> enumList = GetEnumerations(classifier);

                                // For enumerations, we have to check the scope of the enumeration, since they can be restricted at message-, 
                                // operation- or interface level, just like Classes!
                                classifierCtx = new ClassifierContext(ClassifierContext.ContentTypeCode.Enum, classifier.Name, scope, this._commonSchema != null);
                                if (this._useDocContext) targetDocCtx.AddClassifier(classifier, null, attribList, typeName, classifierCtx.Name);
                                if (classifierCtx.IsInCommonSchema)
                                {
                                    this._commonSchema.AddEnumClassifier(classifierCtx.Name, classifier.GetDocumentation(), attribList, enumList);
                                }
                                else
                                {
                                    // If we're NOT in the common schema, but have message scope, we have to make the name unique by prefixing
                                    // it with the assigned role...
                                    if (classifierCtx.SchemaScope == ClassifierContext.ScopeCode.Message)
                                    {
                                        classifierCtx.Name = this._currentCapability.AssignedRole + classifier.Name;
                                    }
                                    this._schema.AddEnumClassifier(classifierCtx.Name, classifier.GetDocumentation(), attribList, enumList);
                                }
                            }
                        }
                        break;

                    case MEDataType.MetaDataType.ExtSchemaType:       // Data type that accepts 1-n items that are defined by an external XML schema.
                        {
                            string ExtSchemaNSAttribute = context.GetConfigProperty(_ExternalSchemaNSAttribute);
                            string ExtSchemaNSTokenAttribute = context.GetConfigProperty(_ExternalSchemaNSTokenAttribute);
                            string ExtSchemaAttribute = context.GetConfigProperty(_ExternalSchemaAttribute);
                            string ExtSchemaBaseTypeAttribute = context.GetConfigProperty(_ExternalSchemaBaseTypeAttribute);
                            string ExtSchemaMinOccAttribute = context.GetConfigProperty(_ExternalSchemaMinOccAttribute);
                            string ExtSchemaMaxOccAttribute = context.GetConfigProperty(_ExternalSchemaMaxOccAttribute);
                            string nameSpace = string.Empty;
                            string nameSpaceToken = string.Empty;
                            string schemaName = string.Empty;
                            string baseType = string.Empty;
                            string minOccStr = "1";
                            string maxOccStr = "1";
                            var cardinality = new Tuple<int, int>(1, 1);

                            foreach (MEAttribute attribute in classifier.Attributes)
                            {
                                Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.DefineClassifier >> Inspecting attribute: " + attribute.Name);
                                string value = !string.IsNullOrEmpty(attribute.FixedValue) ? attribute.FixedValue : attribute.DefaultValue;
                                if (attribute.Name.Equals(ExtSchemaNSAttribute, StringComparison.OrdinalIgnoreCase)) nameSpace = value;
                                else if (attribute.Name.Equals(ExtSchemaNSTokenAttribute, StringComparison.OrdinalIgnoreCase)) nameSpaceToken = value;
                                else if (attribute.Name.Equals(ExtSchemaAttribute, StringComparison.OrdinalIgnoreCase)) schemaName = value;
                                else if (attribute.Name.Equals(ExtSchemaBaseTypeAttribute, StringComparison.OrdinalIgnoreCase)) baseType = value;
                                else if (attribute.Name.Equals(ExtSchemaMinOccAttribute, StringComparison.OrdinalIgnoreCase)) minOccStr = value;
                                else if (attribute.Name.Equals(ExtSchemaMaxOccAttribute, StringComparison.OrdinalIgnoreCase)) maxOccStr = value;
                            }
                            try // On any parse errors, we use the default values instead!
                            {
                                int minOcc = Int16.Parse(minOccStr);
                                int maxOcc = (maxOccStr == "*") ? 0 : Int16.Parse(maxOccStr);
                                cardinality = new Tuple<int, int>(minOcc, maxOcc);
                            }
                            catch
                            {
                                Logger.WriteWarning("External reference '" + classifier.Name +
                                                    "' has illegal minOcc and/or maxOcc declarations, default is used instead!");
                                if (this._panel != null) this._panel.WriteWarning(this._panelIndex + 1, "External reference '" + classifier.Name +
                                                                                  "' has illegal minOcc and/or maxOcc declarations, default is used instead!");
                            }
                            targetSchema.AddExternalClassifier(classifier.Name, classifier.GetDocumentation(), nameSpace,
                                                               nameSpaceToken, schemaName, baseType, cardinality);
                            classifierCtx = new ClassifierContext(ClassifierContext.ContentTypeCode.Complex, classifier.Name, scope, this._commonSchema != null);
                            if (this._useDocContext) targetDocCtx.AddClassifier(classifier, null, null, typeName);
                        }
                        break;

                    default:
                        // All others are not supported at this time.
                        this._lastError = "Classifier '" + classifier.Name + "' is of unsupported type: " + typeName;
                        Logger.WriteError("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.DefineClassifier >> " + this._lastError);
                        if (this._panel != null) this._panel.WriteError(this._panelIndex + 3, this._lastError);
                        classifierCtx = new ClassifierContext(ClassifierContext.ContentTypeCode.Unknown, classifier.Name, scope, this._commonSchema != null);
                        break;
                }
                Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.DefineClassifier >> Returning PRIM type: " + typeName + " with designator: " + classifier.Type);
                return classifierCtx;
            }
            Logger.WriteError("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.DefineClassifier >> Could not determine type!");
            this._lastError = "Could not determine classifier type for classifier: " + classifier.Name;
            return null;
        }

        /// <summary>
        /// Retrieves all Enumeration attributes from the current classifier metadata and convert these to a list of XML Facet definitions.
        /// </summary>
        /// <param name="classifier">Enumeration classifier of which we want the attributes.</param>
        /// <returns>List of facets that are defined for this classifier (could be empty).</returns>
        private List<EnumerationItem> GetEnumerations(MEDataType classifier)
        {
            var itemList = new List<EnumerationItem>();
            ContextSlt context = ContextSlt.GetContextSlt();
            string supplementary = context.GetConfigProperty(_SupplementaryAttStereotype);

            if (classifier != null)
            {
                foreach (MEAttribute attrib in classifier.Attributes)
                {
                    // We copy all attributes that are NOT supplementaries (an enumeration can have 'enum' and/or 'facet' stereotypes,
                    // but sometimes has no stereotype at all.
                    if (!attrib.HasStereotype(supplementary))
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.GetEnumerations >> Adding '" + attrib.Name + "'...");
                        itemList.Add(new EnumerationItem(attrib.Name, attrib.GetDocumentation()));
                    }
                }
            }
            return itemList;
        }

        /// <summary>
        /// Retrieves a schema implementation according to current interface type settings. When retrieved Ok, the instance is explicitly initialized
        /// and returned to the caller.
        /// </summary>
        /// <param name="type">Identifies the type of schema that we're building.</param>
        /// <param name="name">A meaningfull name, with which we can identify the schema.</param>
        /// <param name="namespaceToken">Namespace token.</param>
        /// <param name="schemaNamespace">Schema namespace, preferably an URI.</param>
        /// <param name="version">Major, minor and build number of the schema. When omitted, the version defaults to '1.0.0'</param>
        /// <returns>Appropriate Schema implementation.</returns>
        /// <exception cref="MissingFieldException">No schema implementation has been defined for the current interface type.</exception>
        private Schema GetSchema(Schema.SchemaType type, string name, string namespaceToken, string schemaNamespace, string version = "1.0.0")
        {
            string itfTypeKey = string.Empty;
            try
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                itfTypeKey = "InterfaceType:" + context.GetStringSetting(FrameworkSettings._InterfaceContractType);
                Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.GetSchema >> Retrieving schema implementation for '" + itfTypeKey + "'...");
                ObjectHandle handle = Activator.CreateInstance(null, context.GetConfigProperty(itfTypeKey));
                var proc = handle.Unwrap() as Schema;
                if (proc != null)
                {
                    proc.Initialize(type, name, namespaceToken, schemaNamespace, version);
                    if (!(proc is XMLSchema) && !(proc is JSONSchema))
                    {
                        string message = "Unknown schema implementation '" + proc.GetType() + "'  has been defined for key '" + itfTypeKey + "'!";
                        Logger.WriteError("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.GetSchema >> " + message);
                        throw new MissingFieldException(message);
                    }
                    return proc;
                }
                else
                {
                    string message = "No (valid) schema implementation has been defined for key '" + itfTypeKey + "'!";
                    Logger.WriteError("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.GetSchema >> " + message);
                    throw new MissingFieldException(message);
                }
            }
            catch (Exception exc)
            {
                string message = "Caught exception when retrieving schema for key '" + itfTypeKey + "'!" + Environment.NewLine + exc.Message;
                Logger.WriteError("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.GetSchema >> " + message);
                throw new MissingFieldException(message, exc);
            }
        }

        /// <summary>
        /// Retrieves all Facet attributes from the current classifier metadata and convert these to a list of Schema Facet definitions.
        /// </summary>
        /// <param name="metaData">Classifier metadata to parse.</param>
        /// <returns>List of facets that are defined for this classifier (could be empty).</returns>
        private List<Facet> GetFacets(List<MEDataType.MetaDataDescriptor> metaData)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.GetFacets >> Converting metadata list to Facet list...");
            var facetList = new List<Facet>();
            foreach (MEDataType.MetaDataDescriptor desc in metaData)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.GetFacets >> Checking '" + desc.Name + "' of type '" + desc.Type + "'...");
                if (desc.Type == AttributeType.Facet)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.GetFacets >> Adding facet '" + desc.Name + "' with value '" +
                                     (!string.IsNullOrEmpty(desc.FixedValue) ? desc.FixedValue : desc.DefaultValue) + "'...");
                    if (this._schema is XMLSchema)
                    {
                        facetList.Add(new XMLFacet(desc.Name, !string.IsNullOrEmpty(desc.FixedValue) ? desc.FixedValue : desc.DefaultValue));
                    }
                    else
                    {
                        facetList.Add(new JSONFacet(desc.Name, !string.IsNullOrEmpty(desc.FixedValue) ? desc.FixedValue : desc.DefaultValue));
                    }
                }
            }
            return facetList;
        }

        /// <summary>
        /// The method is called during construction of CDT or BDT classifier definitions, in which case the supplementary attributes are used to pass
        /// classifier meta-data. For CDT's. the type of these attributes is always a 'String' type and the attributes are of relevance only if a value 
        /// is assigned to them. The method searches 'up' the hierarchy and remembers processed attributes. This implies that one can define the same 
        /// meta-data at multiple levels, but only the 'most specialized' of each attribute is copied. And since CDT and BDT types are considered 'common', 
        /// the namespace is NOT evaluated.
        /// The method checks whether the Classifier is a CDT or BDT. In case of BDT and valid request for supplementary attribute expansion, ALL 
        /// supplementary attributes are accepted. If the classifier is a CDT, the method searches the class hierarchy for any supplementary attributes 
        /// that have a default value. If found, these are returned in the list.
        /// </summary>
        /// <param name="metaData">The class hierarchy to parse.</param>
        /// <returns>List of supplementary attributes retrieved from hierarchy. Empty list if none could be found.</returns>
        private List<SupplementaryAttribute> GetSupplementaries(List<MEDataType.MetaDataDescriptor> metaData)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.GetSupplementaries >> Looking for supplementary values in hierarchy...");
            var attribList = new List<SupplementaryAttribute>();

            foreach (MEDataType.MetaDataDescriptor desc in metaData)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.GetSupplementaries >> Checking '" + desc.Name + "' of type '" + desc.Type + "'...");
                if (desc.Type == AttributeType.Supplementary)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.GetSupplementaries >> Adding Supplementary '" + desc.Name + "'...");
                    string classifierName = desc.Classifier.Name;
                    bool classifierInSchemaNS = false;
                    if (!string.IsNullOrEmpty(desc.FixedValue) && desc.Classifier.MetaType == MEDataType.MetaDataType.Enumeration)
                    {
                        // In case of an enumeration with a fixed value, keeping the original enumeration around does not make sense (since you can't
                        // change the value anyway). In this case, we replace the original enumerated-type classifier by a primitive string.
                        classifierName = "String";
                    }
                    else
                    {
                        // Supplementary attributes MUST be EITHER a primitive type OR an Enumeration. In the latter case, it MIGHT be defined in
                        // the Schema namespace and we must take this into account when creating the supplementary attribute...
                        // By default, supplementaries have a primitive-type classifier that is defined in the XSD (remote) namespace.
                        if (ProcessClassifier(desc.Classifier, true).SchemaScope != ClassifierContext.ScopeCode.Remote) classifierInSchemaNS = true;
                    }
                    Schema targetSchema = this._commonSchema ?? this._schema;
                    if (targetSchema is XMLSchema)
                    {

                        attribList.Add(new XMLSupplementaryAttribute((XMLSchema)targetSchema, desc.Name, classifierName, classifierInSchemaNS, desc.IsOptional,
                                                                     desc.Documentation, desc.DefaultValue, desc.FixedValue));
                    }
                    else
                    {
                        attribList.Add(new JSONSupplementaryAttribute((JSONSchema)targetSchema, desc.Name, classifierName, desc.IsOptional,
                                                                      desc.Documentation, desc.DefaultValue, desc.FixedValue));
                    }
                }
            }
            return attribList;
        }

        /// <summary>
        /// This function is invoked when processing a class hierarchy in order to retrieve a class name that is properly formatted for the current 
        /// classifier scope. The scope value that is passed is directly retrieved from the Scope tag in the class that is
        /// currently being processed. If no scope is provided, processClass will pass the 'Message Scope' configuration value.
        /// If the class has an 'Interface' scope, but the common schema has not been specified, the scope is reduced to 'Operation'.
        ///         
        /// A qualified classname consists of:
        /// 1) The namespace token of the namespace in which the class exists;
        /// 2) The character ":"
        /// 4) The original class name (or alias name if this has been passed instead).
        /// </summary>
        /// <param name="thisClass">The class for which we have to create the qualified name.</param>
        /// <param name="scope">Scope tag from the class that is currently being processed.</param>
        /// <returns>Qualified Class Name.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        private string GetQualifiedClassName(MEClass thisClass, ClassifierContext.ScopeCode scope)
        {
            scope = (scope == ClassifierContext.ScopeCode.Remote) ? ClassifierContext.ScopeCode.Interface : scope;  // We treat Remote equal to Interface!
            string classNs = (this._commonSchema != null && 
                              (scope == ClassifierContext.ScopeCode.Interface || scope == ClassifierContext.ScopeCode.Profile)) ? this._commonSchema.NSToken : this._schema.NSToken;
            string qualifiedName = thisClass.AliasName != string.Empty ? thisClass.AliasName : thisClass.Name;
            string role = this._currentCapability.AssignedRole;

            if (scope == ClassifierContext.ScopeCode.Message)
            {
                // In case of Message Scope, which is the most restrictive scope, we prefix the class name with the name of the role in which the
                // class is used. Also, if the role name is different from the package name in which the class resides, we also add the package
                // name. All in all, the resulting name might be: <role><package><className> (or <role><className> if package name == role name).
                // If the role is already part of the original class name, we will not add it again. So, worst-case the class name is not changed
                // at all (if package name == role name and role name is already part of class name).
                // The package name is added ONLY if this is an operation name (as is used in case of REST Operations). For SOAP, the
                // package name is equal to the Service Model package name and we do NOT add it since it has no added value!
                if (thisClass.OwningPackage.Name != ContextSlt.GetContextSlt().GetConfigProperty(_ServiceModelPkgName) && 
                    string.Compare(role, thisClass.OwningPackage.Name, true) != 0)
                    qualifiedName = thisClass.OwningPackage.Name + qualifiedName;
                if (!(qualifiedName.StartsWith(role) || qualifiedName.EndsWith(role))) qualifiedName = role + qualifiedName;
            }
            else if (scope == ClassifierContext.ScopeCode.Profile)
            {
                // In case of Profile scope, we must add the name of the profile as a prefix to the class, to assure uniqueness across all possible
                // scopes. Note that the "Basic" scope does not yield any name changes (Basic scope is assumed for all packages that do not have the
                // 'profile' stereotype OR that are named after the Basic profile).
                // In all other cases, the name of the profile is equal to the name of the package that 'owns' this class.
                ContextSlt context = ContextSlt.GetContextSlt();
                if (thisClass.OwningPackage.HasStereotype(context.GetConfigProperty(_ProfileStereotype)) && 
                    thisClass.OwningPackage.Name != context.GetConfigProperty(_BasicProfileName))
                    qualifiedName = thisClass.OwningPackage.Name + qualifiedName;
            }

            Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.GetQualifiedClassName >> Returning: " + 
                             classNs + ":" + qualifiedName);
            return classNs + ":" + qualifiedName;
        }
    }
}
