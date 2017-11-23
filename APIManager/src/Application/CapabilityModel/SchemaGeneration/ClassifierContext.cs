using System;
using Framework.Logging;
using Framework.Model;
using Framework.Context;

namespace Plugin.Application.CapabilityModel.SchemaGeneration
{
    /// <summary>
    /// Helper class for administration of a set of classifier context meta data.
    /// </summary>
    internal class ClassifierContext
    {
        // Configuration properties used by this module...
        private const string _ClassScopeTag                     = "ClassScopeTag";
        private const string _ClassScopeInterface               = "ClassScopeInterface";
        private const string _ClassScopeOperation               = "ClassScopeOperation";
        private const string _ClassScopeMessage                 = "ClassScopeMessage";
        private const string _RequestPkgName                    = "RequestPkgName";
        private const string _ResponsePkgName                   = "ResponsePkgName";
        private const string _CommonPkgName                     = "CommonPkgName";
        private const string _MessageAssemblyPkgStereotype      = "MessageAssemblyPkgStereotype";
        private const string _ServiceDeclPkgStereotype          = "ServiceDeclPkgStereotype";
        private const string _ServiceOperationPkgStereotype     = "ServiceOperationPkgStereotype";

        internal enum ContentTypeCode { Simple, Complex, Enum, Union, Unknown }   // Defines the schema content type of a classifier definition.
        internal enum ScopeCode       { Interface,                // Classifier is defined at Interface level (Common Schema);
                                        Operation,                // Classifier is defined at Operation level (unique for request/response);
                                        Message,                  // Classifier is Message-specific;
                                        Remote }                  // Classifier is defined outside the scope of the Interface (remote schema). 
        internal enum DocScopeCode    { Common, Local }           // For documentation purposes, the classifier can have either common- or local scope.
        
        private ContentTypeCode _contentType;
        private string _name;
        private ScopeCode _schemaScope;
        private DocScopeCode _docScope;

        /// <summary>
        /// Creates a new classifier context using the provided arguments.
        /// </summary>
        /// <param name="cType">The content type code of the classifier.</param>
        /// <param name="name">Classifier name.</param>
        /// <param name="scope">Classifier schema- and documentation scope.</param>
        internal ClassifierContext(ContentTypeCode cType, string name, Tuple<ScopeCode, DocScopeCode> scope)
        {
            this._contentType = cType;
            this._name = name;
            this._schemaScope = scope.Item1;
            this._docScope = scope.Item2;
        }

        /// <summary>
        /// Returns 'true' if the classifier is defined in the common schema.
        /// </summary>
        internal bool IsInCommonSchema
        {
            get { return ((this._schemaScope == ScopeCode.Interface) || (this._schemaScope == ScopeCode.Remote)); }
        }

        /// <summary>
        /// Returns 'true' if the classifier is defined in the common documentation context.
        /// </summary>
        internal bool IsInCommonDocContext
        {
            get { return (this._docScope == DocScopeCode.Common); }
        }

        /// <summary>
        /// Get- or set the name of the classifier.
        /// </summary>
        internal string Name
        {
            get { return this._name; }
            set { this._name = value; }
        }

        /// <summary>
        /// Get- or set the content type of the classifier.
        /// </summary>
        internal ContentTypeCode ContentType
        {
            get { return this._contentType; }
            set { this._contentType = value; }
        }

        /// <summary>
        /// Get- or set the schema scope of the classifier.
        /// </summary>
        internal ScopeCode SchemaScope
        {
            get { return this._schemaScope; }
            set { this._schemaScope = value; }
        }

        /// <summary>
        /// Get- or set the documentation scope of the classifier.
        /// </summary>
        internal DocScopeCode DocScope
        {
            get { return this._docScope; }
            set { this._docScope = value; }
        }

        /// <summary>
        /// This function determines the [namespace] scope for a class. The scope defines the location of the class (e.g. whether
        /// it resides in a common schema) as well as the naming of the class (in case of message-scope, the name of the class must be
        /// transformed in order to avoid name clashes).
        /// The method first checks whether the associated class has a 'scope' tag defined. If so, the tag value has precedence. If
        /// no tag could be found, the method inspects the owning package and defines the scope based on this package.
        /// If the scope is 'Interface' but no common schema has been defined, the scope is reduced to 'Operation'.
        /// The function also determines the Documentation Scope, which can be different from the Schema Scope and depends only on
        /// the presence of a common documentation context in combination with the classifier context (common schema is ignored here).
        /// </summary>
        /// <param name="currentClass">The class to check.</param>
        /// <param name="commonSchemaDefined">Set to TRUE in case a Common Schema is in effect for this build.</param>
        /// <param name="commonDocCtxDefined">Set to TRUE if we have a common Documentation context.</param>
        /// <returns>Defined scope (schema-scope as well as documentation scope) for this element.</returns>
        internal static Tuple<ScopeCode, DocScopeCode> GetScope(MEClass currentClass, bool commonSchemaDefined, bool commonDocCtxDefined)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.ClassifierContext.GetScope >> Determine scope for class: " + currentClass.Name);
            ContextSlt context = ContextSlt.GetContextSlt();

            ScopeCode schemaScope = ScopeCode.Message;
            DocScopeCode docScope = DocScopeCode.Local;

            string scopeTag = currentClass.GetTag(context.GetConfigProperty(_ClassScopeTag)).ToLower();
            if (scopeTag != string.Empty)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.ClassifierContext.GetScope >> Use Tagged Value: " + scopeTag);
                if (scopeTag == context.GetConfigProperty(_ClassScopeInterface).ToLower())
                {
                    schemaScope = commonSchemaDefined ? ScopeCode.Interface : ScopeCode.Operation;
                    docScope = commonDocCtxDefined ? DocScopeCode.Common : DocScopeCode.Local;
                }
                else if (scopeTag == context.GetConfigProperty(_ClassScopeOperation).ToLower()) schemaScope = ScopeCode.Operation;
            }
            else
            {
                MEPackage parent = currentClass.OwningPackage;
                Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.ClassifierContext.GetScope >> No tag, check owning package '" + parent.Name + "'...");
                if (parent.HasStereotype(context.GetConfigProperty(_MessageAssemblyPkgStereotype)))
                {
                    // Parent is either Request-, Response- or Common package, we have valid context.
                    if ((string.Compare(parent.Name, context.GetConfigProperty(_RequestPkgName), StringComparison.OrdinalIgnoreCase) == 0) ||
                        (string.Compare(parent.Name, context.GetConfigProperty(_ResponsePkgName), StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.ClassifierContext.GetScope >> Request/Response package == message scope!");
                        schemaScope = ScopeCode.Message;
                    }
                    else if (string.Compare(parent.Name, context.GetConfigProperty(_CommonPkgName), StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        // Found in common package, which has two flavors: operation- or interface level. We need another parent package...
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.ClassifierContext.GetScope >> It's a common package, check parent again...");
                        MEPackage parentOfParent = parent.Parent;

                        if (parentOfParent.HasStereotype(context.GetConfigProperty(_ServiceDeclPkgStereotype)))
                        {
                            schemaScope = commonSchemaDefined ? ScopeCode.Interface : ScopeCode.Operation;
                            docScope = commonDocCtxDefined ? DocScopeCode.Common : DocScopeCode.Local;
                        }
                        else if (parentOfParent.HasStereotype(context.GetConfigProperty(_ServiceOperationPkgStereotype)))
                            schemaScope = ScopeCode.Operation;
                        else schemaScope = ScopeCode.Message;
                    }
                    else
                    {
                        // We have a generic CCMA package, defined outside the operation packages, but assumed to be within the scope of the service.
                        // Since these can have any structure, we simply consider them to have 'Interface' scope (unless of course, we don't have
                        // a common schema, in which case this is demoted to 'Operation' scope). The same holds for the documentation scope.
                        schemaScope = commonSchemaDefined ? ScopeCode.Interface : ScopeCode.Operation;
                        docScope = commonDocCtxDefined ? DocScopeCode.Common : DocScopeCode.Local;
                    }
                }
                // Package is not within Service context, assume remote definition, unless we don't have a common schema, in which case it will be 'Operation' scope.
                else
                {
                    schemaScope = commonSchemaDefined ? ScopeCode.Remote : ScopeCode.Operation;
                    docScope = commonDocCtxDefined ? DocScopeCode.Common : DocScopeCode.Local;
                }
            }
            Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.ClassifierContext.GetScope >> Returning schema scope: '" + 
                             schemaScope + "' and doc. scope: '" + docScope + "'.");
            return new Tuple<ScopeCode, DocScopeCode>(schemaScope, docScope);
        }
    }
}
