using System;
using System.Collections.Generic;
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
        private const string _ResultCodeAttributeName           = "ResultCodeAttributeName";
        private const string _ResultCodeAttributeClassifier     = "ResultCodeAttributeClassifier";
        private const string _CoreDataTypesPathName             = "CoreDataTypesPathName";
        private const string _RESTOperationResultStereotype     = "RESTOperationResultStereotype";
        private const string _DefaultResponseCode               = "DefaultResponseCode";

        private RESTOperationCapability _parent;                            // Parent operation capability that owns this result.
        private RESTOperationResultCapability.ResponseCategory _category;   // The result category code
        private string _resultCode;                                         // Operation result code (must match category).

        /// <summary>
        /// Returns the HTTP response code as a string. 
        /// </summary>
        internal string ResultCode  { get { return this._resultCode; } }
        
        /// <summary>
        /// Creates a new operation result capability based on a declaration object. This object contains all the information necessary to create 
        /// the associated model elements and attributes.
        /// </summary>
        /// <param name="myCollection">Resource collection that acts as parent for the operation.</param>
        /// <param name="operation">Operation declaration object, created by user and containing all necessary information.</param>
        internal RESTOperationResultCapabilityImp(RESTOperationCapability parentOperation, RESTOperationResultDeclaration result): base(parentOperation.RootService)
        {
            try
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationResultCapabilityImp (declaration) >> Creating operation result'" +
                                 parentOperation.Name + "." + result.ResultCode + "'...");
                ContextSlt context = ContextSlt.GetContextSlt();
                ModelSlt model = ModelSlt.GetModelSlt();
                this._parent = parentOperation;
                this._category = result.Category;
                this._resultCode = result.ResultCode;
                this._assignedRole = context.GetConfigProperty(_OperationResultPrefix) + Conversions.ToPascalCase(this._resultCode);
                this._capabilityClass = parentOperation.OperationPackage.CreateClass(this._assignedRole + "Type", context.GetConfigProperty(_RESTOperationResultStereotype));
                var myEndpoint = new EndpointDescriptor(this._capabilityClass, "1", this.AssignedRole, null, false);

                MEDataType classifier = model.FindDataType(context.GetConfigProperty(_CoreDataTypesPathName), context.GetConfigProperty(_ResultCodeAttributeClassifier));
                if (classifier != null)
                {
                    this._capabilityClass.CreateAttribute(context.GetConfigProperty(_ResultCodeAttributeName), classifier,
                                                          AttributeType.Attribute, this._resultCode, new Tuple<int, int>(1, 1), true);
                    MEChangeLog.SetRTFDocumentation(this._capabilityClass, result.Description);
                    if (result.Parameters != null)
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationResultCapabilityImp (declaration) >> Associating with response type '" + result.Parameters.Name + "'...");
                        string roleName = Conversions.ToPascalCase(result.Parameters.Name);
                        if (roleName.EndsWith("Type")) roleName = roleName.Substring(0, roleName.IndexOf("Type"));
                        var typeEndpoint = new EndpointDescriptor(result.Parameters, "1", roleName, null, true);
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
        /// <param name="myCollection">The resource collection for which we create the operation.</param>
        /// <param name="operationClass">Class associated with the operation.</param>
        internal RESTOperationResultCapabilityImp(RESTOperationCapability parentOperation, MEClass resultClass): base(parentOperation.RootService)
        {
            try
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationResultCapabilityImp (existing) >> Creating new instance '" + 
                                 parentOperation.Name + "." + resultClass.Name + "'...");
                ContextSlt context = ContextSlt.GetContextSlt();
                this._parent = parentOperation;
                this._capabilityClass = resultClass;
                this._assignedRole = parentOperation.FindChildClassRole(this._capabilityClass.Name, context.GetConfigProperty(_RESTOperationResultStereotype));
                this._resultCode = string.Empty;
                this._category = RESTOperationResultCapability.ResponseCategory.Unknown;
                string resultCodeAttibName = context.GetConfigProperty(_ResultCodeAttributeName);
                string defaultResponse = context.GetConfigProperty(_DefaultResponseCode);

                foreach (MEAttribute attrib in resultClass.Attributes)
                {
                    if (attrib.Name == resultCodeAttibName)
                    {
                        this._resultCode = attrib.FixedValue;
                        this._category = (this._resultCode == defaultResponse)? RESTOperationResultCapability.ResponseCategory.Default:
                                         (RESTOperationResultCapability.ResponseCategory)(int.Parse(this._resultCode[0].ToString()));
                        break;
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.OperationResultCapabilityImp (existing) >> Error creating capability because: " + exc.Message);
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

           //NOT YET IMPLEMENTED
            base.Delete();
        }

        /// <summary>
        /// This method is invoked when the user has made one or more changes to a Result Capability. The method receives an
        /// Operation Result Declaration object that contains the (updated) information for the Result.
        /// </summary>
        /// <param name="result">Updated Operation Result properties.</param>
        internal void EditOperation(RESTOperationResultDeclaration result)
        {
            // NOT YET IMPLEMENTED
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
            if (parent is RESTOperationCapability)
            {
                this._parent = parent as RESTOperationCapability;
                parent.AddChild(new RESTOperationResultCapability(this));
            }
        }
    }
}
