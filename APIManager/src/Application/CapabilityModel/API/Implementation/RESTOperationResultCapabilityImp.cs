using System;
using Framework.Model;
using Framework.Logging;
using Framework.Context;
using Framework.Util;

namespace Plugin.Application.CapabilityModel.API
{
    internal class RESTOperationResultCapabilityImp: CapabilityImp
    {
        // Configuration properties used by this module:
        private const string _OperationResultPrefix             = "OperationResultPrefix";
        private const string _OperationResultClassName          = "OperationResultClassName";
        private const string _ResultCodeAttributeName           = "ResultCodeAttributeName";
        private const string _ResultCodeAttributeClassifier     = "ResultCodeAttributeClassifier";
        private const string _CoreDataTypesPathName             = "CoreDataTypesPathName";
        private const string _RESTOperationResultStereotype     = "RESTOperationResultStereotype";
        private const string _ResourceClassStereotype           = "ResourceClassStereotype";
        private const string _DefaultResponseCode               = "DefaultResponseCode";

        private RESTOperationResultCapability.ResponseCategory _category;   // The result category code
        private string _resultCode;                                         // Operation result code (must match category).
        private MEClass _responseBodyClass;                                 // Response body class in case result has a body.
        private Cardinality _responseCardinality;                           // Cardinality of response body class.

        /// <summary>
        /// Returns cardinality of the response body class (only valid if such a body has been defined).
        /// </summary>
        internal Cardinality ResponseCardinality { get { return this._responseCardinality; } }

        /// <summary>
        /// Returns the HTTP response code as a string. 
        /// </summary>
        internal string ResultCode  { get { return this._resultCode; } }

        /// <summary>
        /// Returns the response category code (100, 200, 300, etc.)
        /// </summary>
        internal RESTOperationResultCapability.ResponseCategory Category { get { return this._category; } }

        /// <summary>
        /// Returns the response body class (if present, otherwise the Property is NULL).
        /// </summary>
        internal MEClass ResponseBodyClass { get { return this._responseBodyClass; } }
        
        /// <summary>
        /// Creates a new operation result capability based on a declaration object. This object contains all the information necessary to create 
        /// the associated model elements and attributes.
        /// </summary>
        /// <param name="myCollection">Resource collection that acts as parent for the operation.</param>
        /// <param name="operation">Operation declaration object, created by user and containing all necessary information.</param>
        internal RESTOperationResultCapabilityImp(RESTOperationCapability parentOperation, RESTOperationResultDescriptor result): base(parentOperation.RootService)
        {
            try
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationResultCapabilityImp (declaration) >> Creating operation result'" +
                                 parentOperation.Name + "." + result.ResultCode + "'...");
                ContextSlt context = ContextSlt.GetContextSlt();
                ModelSlt model = ModelSlt.GetModelSlt();
                this._category = result.Category;
                this._resultCode = result.ResultCode;
                this._responseBodyClass = result.ResponsePayloadClass;
                this._responseCardinality = result.ResponseCardinality;
                this._assignedRole = context.GetConfigProperty(_OperationResultPrefix) + Conversions.ToPascalCase(this._resultCode);
                this._capabilityClass = parentOperation.OperationPackage.CreateClass(this._assignedRole + "Type", context.GetConfigProperty(_RESTOperationResultStereotype));
                var myEndpoint = new EndpointDescriptor(this._capabilityClass, "1", this.AssignedRole, null, false);

                MEDataType classifier = model.FindDataType(context.GetConfigProperty(_CoreDataTypesPathName), context.GetConfigProperty(_ResultCodeAttributeClassifier));
                if (classifier != null)
                {
                    this._capabilityClass.CreateAttribute(context.GetConfigProperty(_ResultCodeAttributeName), classifier,
                                                          AttributeType.Attribute, this._resultCode, new Cardinality(Cardinality._Mandatory), true);
                    MEChangeLog.SetRTFDocumentation(this._capabilityClass, result.Description);

                    if (result.ResponsePayloadClass != null)
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationResultCapabilityImp (declaration) >> Associating with response type '" + result.ResponsePayloadClass.Name + "'...");
                        string roleName = RESTUtil.GetAssignedRoleName(result.ResponsePayloadClass.Name);
                        if (roleName.EndsWith("Type")) roleName = roleName.Substring(0, roleName.IndexOf("Type"));
                        string cardinality = result.ResponseCardinality.ToString();
                        var typeEndpoint = new EndpointDescriptor(result.ResponsePayloadClass, cardinality, roleName, null, true);
                        model.CreateAssociation(myEndpoint, typeEndpoint, MEAssociation.AssociationType.MessageAssociation);
                    }
                }
                else Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTOperationResultCapabilityImp (declaration) >> Error retrieving result classifier '" +
                                       context.GetConfigProperty(_ResultCodeAttributeClassifier) + "' from path '" + context.GetConfigProperty(_CoreDataTypesPathName) + "'!");

                // Establish link with our Parent...
                var operationEndpoint = new EndpointDescriptor(parentOperation.CapabilityClass, "1", parentOperation.AssignedRole, null, true);
                model.CreateAssociation(operationEndpoint, myEndpoint, MEAssociation.AssociationType.MessageAssociation);
                InitialiseParent(parentOperation);

                CreateLogEntry("Initial release.");
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTOperationResultCapabilityImp (declaration) >> Error creating result: " + exc.Message);
                this._capabilityClass = null;   // Assures that instance is declared invalid.
            }
        }

        /// <summary>
        /// Generic constructor to be used for existing class models. The constructor initialises local context.
        /// </summary>
        /// <param name="hierarchy">The resource collection nodefor which we create the operation.</param>
        /// <param name="operationClass">Class associated with the operation.</param>
        internal RESTOperationResultCapabilityImp(RESTOperationCapability parentOperation, TreeNode<MEClass> hierarchy): base(parentOperation.RootService)
        {
            try
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationResultCapabilityImp (existing) >> Creating new instance '" + 
                                 parentOperation.Name + "." + hierarchy.Data.Name + "'...");
                ContextSlt context = ContextSlt.GetContextSlt();
                this._capabilityClass = hierarchy.Data;
                this._assignedRole = parentOperation.FindChildClassRole(this._capabilityClass.Name, context.GetConfigProperty(_RESTOperationResultStereotype));
                this._resultCode = string.Empty;
                this._responseBodyClass = null;
                this._responseCardinality = new Cardinality();
                this._category = RESTOperationResultCapability.ResponseCategory.Unknown;
                string resultCodeAttribName = context.GetConfigProperty(_ResultCodeAttributeName);
                string defaultResponse = context.GetConfigProperty(_DefaultResponseCode);

                foreach (MEAttribute attrib in hierarchy.Data.Attributes)
                {
                    if (attrib.Name == resultCodeAttribName)
                    {
                        this._resultCode = attrib.FixedValue;
                        this._category = (this._resultCode == defaultResponse)? RESTOperationResultCapability.ResponseCategory.Default:
                                         (RESTOperationResultCapability.ResponseCategory)(int.Parse(this._resultCode[0].ToString()));
                        break;
                    }
                }

                // Look for a Document Resource, which in turn has an association with the Business Component that we must use as the 
                // root of our response schema. We link to the document resource since that is how the model is constructed and which 
                // is expected by the schema generator.
                // There can be at most ONE such associated Document, so we quit after finding the first one.
                string resourceStereotype = context.GetConfigProperty(_ResourceClassStereotype);
                foreach (TreeNode<MEClass> node in hierarchy.Children)
                {
                    if (node.Data.HasStereotype(resourceStereotype))
                    {
                        this._responseBodyClass = node.Data;
                        parentOperation.ResponseBodyDocument = new RESTResourceCapability(node.Data);
                        // Now we have to figure out what the cardinality with the Document Resource is like...
                        foreach (MEAssociation association in this._capabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
                        {
                            if (association.Destination.EndPoint == this._responseBodyClass)
                            {
                                this._responseCardinality = association.GetCardinality(MEAssociation.AssociationEnd.Destination);
                                break;
                            }
                        }
                        break;
                    }
                    else
                    {
                        Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTOperationResultCapabilityImp >> Unknown child type '" +
                                          node.GetType() + "' with name '" + node.Data.Name + "'!");
                        this._capabilityClass = null;
                        return;
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.OperationResultCapabilityImp (existing) >> Error creating capability because: " + exc.ToString());
                this._capabilityClass = null;   // Assures that instance is declared invalid.
            }
        }

        /// <summary>
        /// Overrides the default Capability.delete in order to assure that the operation is deleted as well as the operation package.
        /// On return, all operation resources, including the package tree, are deleted and the Capability is INVALID.
        /// </summary>
        internal override void Delete()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationResultCapabilityImp.delete >> Deleting the operation result and all associated resources...");
            base.Delete();
        }

        /// <summary>
        /// This method is invoked when the user has made one or more changes to a Result Capability. The method receives an
        /// Operation Result Declaration object that contains the (updated) information for the Result.
        /// </summary>
        /// <param name="result">Updated Operation Result properties.</param>
        internal void Edit(RESTOperationResultDescriptor result)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string newRoleName = context.GetConfigProperty(_OperationResultPrefix) + Conversions.ToPascalCase(result.ResultCode);
            string newName = newRoleName + "Type";

            if (this._capabilityClass.Name != newName)
            {
                this._resultCode = result.ResultCode;
                this._category = (RESTOperationResultCapability.ResponseCategory)(int.Parse(this._resultCode[0].ToString()));
                string resultCodeAttribName = context.GetConfigProperty(_ResultCodeAttributeName);
                Rename(newName);

                foreach (MEAttribute attrib in this._capabilityClass.Attributes)
                {
                    if (attrib.Name == resultCodeAttribName)
                    {
                        attrib.FixedValue = this._resultCode;
                        break;
                    }
                }
            }

            UpdateResponseDocument(result);
            MEChangeLog.SetRTFDocumentation(this._capabilityClass, result.Description);
        }

        /// <summary>
        /// Operation Result capabilities can not be saved in files, so this function returns an empty string.
        /// </summary>
        internal override string GetBaseFileName()
        {
            return string.Empty;
        }

        /// <summary>
        /// Returns a short textual identification of the capability type.
        /// </summary>
        /// <returns>Capability type name.</returns>
        internal override string GetCapabilityType()
        {
            return "REST Operation Result";
        }

        /// <summary>
        /// Creates an Interface object that matches the current Implementation.
        /// </summary>
        /// <returns>Interface object.</returns>
        internal override Capability GetInterface() { return new RESTOperationResultCapability(this); }

        /// <summary>
        /// Process the capability (i.e. generate output according to provided processor.).
        /// </summary>
        /// <param name="stage">The processing stage we're currently in, passed verbatim to processor.</param>
        /// <returns>True when processing can commence, false on errors.</returns>
        internal override bool HandleCapability(CapabilityProcessor processor, ProcessingStage stage)
        {
            // Since all the actual work is being performed by the processor, simply pass information onwards...
            return processor.ProcessCapability(new RESTOperationResultCapability(this), stage);
        }

        /// <summary>
        /// This method is called whenever a (new) parent of the Capability has taken ownership of this Capability. 
        /// If this parent is an Operation, we have to register the current instance with that Interface.
        /// </summary>
        /// <param name="parent">The parent Capability that has taken ownership of this Capability.</param>
        internal override void InitialiseParent(Capability parent)
        {
            if (parent is RESTOperationCapability) parent.AddChild(new RESTOperationResultCapability(this));
        }

        /// <summary>
        /// Overrides the default parent 'rename' method. This method replaces the name of the class as well as the role of the association
        /// between parent and child. In this case, the role name is defines as the class name, minus 'Type' extension.
        /// </summary>
        /// <param name="newName">New name for the class.</param>
        internal override void Rename(string newName)
        {
            this._capabilityClass.Name = newName;
            this._assignedRole = newName.Substring(0, newName.IndexOf("Type"));
            foreach (MEAssociation association in Parent.CapabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
            {
                if (association.Destination.EndPoint == this._capabilityClass)
                {
                    association.SetName(this._assignedRole, MEAssociation.AssociationEnd.Destination);
                    break;
                }
            }
        }

        /// <summary>
        /// The method checks that we have indeed received a new response document. If so, it removes the association with the existing
        /// document (if one is present) and creates a new association with the provided Document Resource class.
        /// If the provided document is NULL, the existing association is removed.
        /// The cardinality of the new association is defined by 'hasMultipleResponses'.
        /// We can also have a situation where the document itself has not been changed, but the cardinality has!
        /// Finally, in case of error responses, we have an association with an OperationResult class. In this case, we don't make
        /// any changes!
        /// </summary>
        /// <param name="newDocument">Optional new Document Resource to be used as response.</param>
        /// <param name="hasMultipleResponses">When true, the cardinality of the new association will be '1..n' instead of '1'.</param>
        private void UpdateResponseDocument(RESTOperationResultDescriptor result)
        {
            // First of all, check whether result.ResponseDocumentClass is associated with an error response. In this case,
            // we should ignore this update request!
            if (result.ResponsePayloadClass != null && 
                result.ResponsePayloadClass.Name == ContextSlt.GetContextSlt().GetConfigProperty(_OperationResultClassName))
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationResultCapabilityImp.UpdateResponseDocument >> Ignored error response!");
                return;
            }

            // Locate the association with the Document resource (if we have one)...
            MEAssociation resourceAssoc = null;
            if (this._responseBodyClass != null)
            {
                foreach (MEAssociation association in this._capabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
                {
                    if (association.Destination.EndPoint == this._responseBodyClass)
                    {
                        resourceAssoc = association;
                        break;
                    }
                }
            }

            // If document changed, remove the existing association...
            bool responseDocChanged = result.ResponsePayloadClass != this._responseBodyClass;
            if (responseDocChanged && this._responseBodyClass != null && resourceAssoc != null)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationResultCapabilityImp.UpdateResponseDocument >> Removing existing association with '" + this._responseBodyClass.Name + "'...");
                this._capabilityClass.DeleteAssociation(resourceAssoc);
                this._responseBodyClass = null;
                this._responseCardinality = new Cardinality();
            }

            if (this._responseCardinality != result.ResponseCardinality && resourceAssoc != null)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationResultCapabilityImp.UpdateResponseDocument >> Cardinality has changed, update...");
                this._responseCardinality = result.ResponseCardinality;
                resourceAssoc.SetCardinality(this._responseCardinality, MEAssociation.AssociationEnd.Destination);
            }

            if (responseDocChanged && result.ResponsePayloadClass != null)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationResultCapabilityImp.UpdateResponseDocument >> Associating with new response type '" + result.ResponsePayloadClass.Name + "'...");
                string roleName = RESTUtil.GetAssignedRoleName(result.ResponsePayloadClass.Name);
                if (roleName.EndsWith("Type")) roleName = roleName.Substring(0, roleName.IndexOf("Type"));
                string cardinality = result.ResponseCardinality.ToString();
                var typeEndpoint = new EndpointDescriptor(result.ResponsePayloadClass, cardinality, roleName, null, true);
                var myEndpoint = new EndpointDescriptor(this.CapabilityClass, "1", Name, null, false);
                ModelSlt.GetModelSlt().CreateAssociation(myEndpoint, typeEndpoint, MEAssociation.AssociationType.MessageAssociation);
                this._responseBodyClass = result.ResponsePayloadClass;
                this._responseCardinality = result.ResponseCardinality;
            }
        }
    }
}
