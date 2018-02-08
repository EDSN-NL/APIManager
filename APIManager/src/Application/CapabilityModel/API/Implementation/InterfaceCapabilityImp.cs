using System;
using System.Collections.Generic;
using Framework.Model;
using Framework.Logging;
using Framework.Context;
using Framework.Util;

namespace Plugin.Application.CapabilityModel.API
{
    internal class InterfaceCapabilityImp: CapabilityImp, IDisposable
    {
        // Configuration properties used by this module.
        private const string _InterfaceContractClassStereotype      = "InterfaceContractClassStereotype";
        private const string _CommonSchemaClassStereotype           = "CommonSchemaClassStereotype";
        private const string _OperationClassStereotype              = "OperationClassStereotype";

        private bool _disposed;    // Mark myself as invalid after call to dispose!

        // Although this capability is registered as the first child in my capability-list, we keep this additional property
        // for efficiency reasons (in order to to search the list everytime a reference to the common schema is required).
        private CommonSchemaCapability _commonSchema;

        /// <summary>
        /// Get- and Set the CommonSchema object.
        /// </summary>
        internal CommonSchemaCapability CommonSchema
        {
            get { return this._commonSchema; }
            set { this._commonSchema = value; }
        }

        /// <summary>
        /// Create constructor, used to create a new instance of an Interface. The constructor assumes that the package structure
        /// exists and that there exists a service to which we can connect the new capability. The constructor creates the
        /// appropriate model elements in the correct packages and links stuff together. If no operationNames are specified, the
        /// constructor only creates the Interface, Common Schema (if required to do so) and association with the service. 
        /// Operations can be added seperately.
        /// <param name="myService">All capabilities are, directly or indirectly, always associated with a single Service.</param>
        /// <param name="capabilityName">Name of the interface to be created.</param>
        /// <param name="operationNames">Optional: the operations that this initial version must support.</param>
        /// </summary>
        internal InterfaceCapabilityImp(Service myService, string capabilityName, List<string> operationNames): base(myService)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp >> Creating new Interface capability for service '" +
                              myService.Name + "', with name: '" + capabilityName + "'...");
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();

            try
            {
                // Create capability class in same package as service.
                MEPackage owner = myService.ModelPkg;                                               
                string myStereotype = context.GetConfigProperty(_InterfaceContractClassStereotype);
                this._capabilityClass = owner.CreateClass(capabilityName, myStereotype);
                this._capabilityClass.Version = new Tuple<int, int>(myService.MajorVersion, 0);
                var interfaceCapItf = new InterfaceCapability(this);

                // Establish link with the service...
                var source = new EndpointDescriptor(myService.ServiceClass, "1", Name, null, false);
                var target = new EndpointDescriptor(this._capabilityClass, "1", capabilityName, null, true);
                model.CreateAssociation(source, target, MEAssociation.AssociationType.MessageAssociation);
                this._assignedRole = capabilityName;

                // When instructed to do so, create the Common Schema and make sure to properly initialize 
                // my Interface with the Common Schema...
                if (context.GetBoolSetting(FrameworkSettings._SMCreateCommonSchema))
                {
                    this._commonSchema = new CommonSchemaCapability(interfaceCapItf);
                    this._commonSchema.InitialiseParent(interfaceCapItf);
                }
                else this._commonSchema = null;

                // Create the operations if any are specified (when we create additional Interfaces, these will NOT have operation names)...
                if (operationNames != null)
                {
                    string newNames = string.Empty;
                    bool isFirst = true;                // Little trick to get the right amount of ',' separators.
                    foreach (string operationName in operationNames)
                    {
                        // Since we use a bridge pattern, the OperationCapability MIGHT not get constructed from scratch. That's why
                        // we have to call 'initialiseParent' separately.
                        var operationCapability = new OperationCapability(interfaceCapItf, operationName);
                        if (operationCapability.Valid)
                        {
                            operationCapability.InitialiseParent(interfaceCapItf);
                            string roleName = Conversions.ToCamelCase(operationName);
                            newNames += (!isFirst) ? ", " + roleName : roleName;
                            isFirst = false;
                        }
                        else
                        {
                            // Oops, something went terribly wrong during construction of the operation. Invalidate and exit!
                            Logger.WriteWarning("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp >> Failed to create operation '" + operationName + "'!");
                            this._capabilityClass = null;       // Invalidates the capability!
                        }
                    }

                    if (this._capabilityClass != null)
                    {
                        myService.AddCapability(interfaceCapItf);
                        CreateLogEntry("Initial release with operation(s): " + newNames);
                    }
                }
                else
                {
                    myService.AddCapability(interfaceCapItf);
                    CreateLogEntry("Initial release without operations.");
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp (new) >> Error creating capability because: " + exc.Message);
                this._capabilityClass = null;   // Assures that instance is declared invalid.
            }
        }

        /// <summary>
        /// Generic constructor to be used for existing class models. The constructor creates all of the subordinate objects (common schema and 
        /// all operations) and builds the complete object hierarchy, including registration with the parent Service.
        /// Please note that it is the responsibility of the CHILD to register with the PARENT and not the other way around. In other words: the
        /// operation capabilities will add themselves to the capability tree.
        /// The constructor receives a 'ready-built' hierarchy of all operations and messages through the 'hierarchy' property. The
        /// root-node represents the Interface.
        /// </summary>
        /// <param name="myService">Associated service instance.</param>
        /// <param name="hierarchy">Capability class hierarchy for this Interface.</param>
        internal InterfaceCapabilityImp(Service myService, TreeNode<MEClass> hierarchy): base(myService)
        {
            try
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp (existing) >> Creating new instance '" +
                                  myService.Name + "." + hierarchy.Data.Name + "'...");
                ContextSlt context = ContextSlt.GetContextSlt();
                this._capabilityClass = hierarchy.Data;
                this._assignedRole = this._capabilityClass.Name;
                var interfaceCapItf = new InterfaceCapability(this);
                this._commonSchema = null;

                foreach (TreeNode<MEClass> node in hierarchy.Children)
                {
                    if (node.Data.HasStereotype(context.GetConfigProperty(_CommonSchemaClassStereotype)))
                    {
                        // Constructor registers the new object with it's parent...
                        this._commonSchema = new CommonSchemaCapability(interfaceCapItf, node.Data);
                        this._commonSchema.InitialiseParent(interfaceCapItf);
                        if (!this._commonSchema.Valid)
                        {
                            Logger.WriteError("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp (existing) >> Error creating Common Schema '" + node.Data.Name + "'!");
                            this._capabilityClass = null;
                            return;
                        }
                    }
                    else
                    {
                        // Since we use a bridge pattern for the Capabilities, it might be that the Operation is not actually constructed, but an existing
                        // object is returned. That's why the constructor does not rely on a particular parent and parent initialisation must be 
                        // performed explicitly by invocation of 'initialiseParent'.....
                        var oper = new OperationCapability(interfaceCapItf, node);
                        oper.InitialiseParent(interfaceCapItf);
                        if (!oper.Valid)
                        {
                            Logger.WriteError("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp (existing) >> Error creating operation '" + node.Data.Name + "'!");
                            this._capabilityClass = null;
                            return;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp (existing) >> Error creating capability because: " + exc.Message);
                this._capabilityClass = null;   // Assures that instance is declared invalid.
            }
        }

        /// <summary>
        /// Add one or more operations to the current interface. We do not check whether the operation names are indeed unique, this should have
        /// been done before! 
        /// It is the responsibility of the operation to register with the parent capability tree.
        /// </summary>
        /// <param name="operationNames">List of operations that must be added.</param>
        /// <param name="newMinorVersion">True when a new minor version must be created.</param>
        /// <returns>True if operation completed successfully, false on errors.</returns>
        internal bool AddOperations (List<string> operationNames, bool newMinorVersion)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp.addOperations >> Adding new operations...");
            bool result = true;
            try
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                string newNames = string.Empty;
                var interfaceCapItf = new InterfaceCapability(this);
                bool isFirst = true; 
                foreach (string operationName in operationNames)
                {
                    var operationCapability = new OperationCapability(interfaceCapItf, operationName);
                    if (operationCapability.Valid)
                    {
                        operationCapability.InitialiseParent(interfaceCapItf);
                        string roleName = Conversions.ToCamelCase(operationName);
                        newNames += (!isFirst) ? ", " + roleName : roleName;
                        isFirst = false;
                    }
                    else
                    {
                        Logger.WriteWarning("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp >> Failed to create operation '" + operationName + "'!");
                        result = false;
                    }
                }

                if (newMinorVersion)
                {
                    var newVersion = new Tuple<int, int>(RootService.Version.Item1, RootService.Version.Item2 + 1);
                    RootService.UpdateVersion(newVersion);
                    newVersion = new Tuple<int, int>(this._capabilityClass.Version.Item1, this._capabilityClass.Version.Item2 + 1);
                    if (this._commonSchema != null) this._commonSchema.CapabilityClass.Version = newVersion;
                    this._capabilityClass.Version = newVersion;
                }
                string logMessage = "Added Operation(s): '" + newNames + "'";
                RootService.CreateLogEntry(logMessage + " to Interface '" + Name + "'.");
                if (this._commonSchema != null) this._commonSchema.CreateLogEntry(logMessage + " to Interface '" + Name + "'.");
                CreateLogEntry(logMessage + ".");
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp.addOperations >> Exception caught: " + exc.Message);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// This method is called in order to link (associate) one or more Operation capabilities with the current Interface.
        /// Association implies registering the Operation as a child and creating an association between the Interface- and
        /// Operation classes. When 'newMinorVersion' is set to 'true', the version of the Interface, associated Common Schema 
        /// and the Service are all incremented.
        /// Finally, an appropriate log message is generated for the Interface, associated Common Schema and Service.
        /// </summary>
        /// <param name="operationList">One or more operations to associate with this Interface.</param>
        /// <param name="newMinorVersion">Set to 'true' if the minor version of the Interface must be bumped.</param>
        internal void AssociateOperations (List<OperationCapability> operationList, bool newMinorVersion)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp.associateOperations >> Going to associate operations with interface '" + Name + "'...");
            string newNames = string.Empty;
            bool isFirst = true;

            if (operationList.Count == 0) return;   // Nothing to associate.

            foreach (OperationCapability operation in operationList)
            {
                operation.AssociateInterface(new InterfaceCapability(this), newMinorVersion);
                string roleName = Conversions.ToCamelCase(operation.Name);
                newNames += (!isFirst) ? ", " + roleName : roleName;
                isFirst = false;
            }

            if (newMinorVersion)
            {
                var newVersion = new Tuple<int, int>(RootService.Version.Item1, RootService.Version.Item2 + 1);
                RootService.UpdateVersion(newVersion);
                newVersion = new Tuple<int, int>(this._capabilityClass.Version.Item1, this._capabilityClass.Version.Item2 + 1);
                if (this._commonSchema != null) this._commonSchema.CapabilityClass.Version = newVersion;
                this._capabilityClass.Version = newVersion;
            }
            string logMessage = "Associated Operation(s): '" + newNames + "'";
            RootService.CreateLogEntry(logMessage + " with Interface '" + Name + "'.");
            if (this._commonSchema != null) this._commonSchema.CreateLogEntry(logMessage + " with Interface '" + Name + "'.");
            CreateLogEntry(logMessage + ".");
        }

        /// <summary>
        /// This method is called in order to link (associate) one or more Operation capability classes with the current interface.
        /// Association implies registering the Operation as a child and creating an association between the Interface- and
        /// Operation classes. When 'newMinorVersion' is set to 'true', the version of the Interface, associated Common Schema 
        /// and the Service are all incremented.
        /// Finally, an appropriate log message is generated for the Interface, associated Common Schema and Service.
        /// </summary>
        /// <param name="operationList">One or more operations to associate with this Interface.</param>
        /// <param name="newMinorVersion">Set to 'true' if the minor version of the Interface must be bumped.</param>
        internal void AssociateOperations(List<MEClass> operationList, bool newMinorVersion)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp.associateOperations (class) >> Going to associate operations with interface '" + Name + "'...");
            string newNames = string.Empty;
            bool isFirst = true;
            var interfaceCapItf = new InterfaceCapability(this);

            if (operationList.Count == 0) return;   // Nothing to associate.

            foreach (MEClass operationClass in operationList)
            {
                var operCapability = new OperationCapability(operationClass);
                operCapability.AssociateInterface(interfaceCapItf, newMinorVersion);    // Will register with Interface, so no need to call 'initialiseParent' here.
                string roleName = Conversions.ToCamelCase(operCapability.Name);
                newNames += (!isFirst) ? ", " + roleName : roleName;
                isFirst = false;
            }

            if (newMinorVersion)
            {
                var newVersion = new Tuple<int, int>(RootService.Version.Item1, RootService.Version.Item2 + 1);
                RootService.UpdateVersion(newVersion);
                newVersion = new Tuple<int, int>(this._capabilityClass.Version.Item1, this._capabilityClass.Version.Item2 + 1);
                if (this._commonSchema != null) this._commonSchema.CapabilityClass.Version = newVersion;
                this._capabilityClass.Version = newVersion;
            }
            string logMessage = "Associated Operation(s): '" + newNames + "'";
            RootService.CreateLogEntry(logMessage + " with Interface '" + Name + "'.");
            if (this._commonSchema != null) this._commonSchema.CreateLogEntry(logMessage + " with Interface '" + Name + "'.");
            CreateLogEntry(logMessage + ".");
        }

        /// <summary>
        /// Deletes the current Interface from the model. Operations associated with the Interface are dissociated (and if they become an orphan as a result,
        /// the Operation is deleted as well). On return, the Interface object is INVALID and should NOT be used anymore!
        /// This method overrides the default 'delete' operation from the base class and thus assures that resources are deleted selectively.
        /// </summary>
        internal override void Delete()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp.delete >> Deleting Interface '" + Name + "'...");
            var interfaceCapItf = new InterfaceCapability(this);

            foreach (OperationCapability child in GetOperations())
            {
                // Dissociate child Operations. If the child becomes an orphan (this is only Interface for the child), the Child 'self-destructs'.
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp.delete >> Dissociating Operation '" + child.Name + "'...");
                child.DissociateInterface(interfaceCapItf);
            }

            // Perform the actual delete operation by removing the Interface classes from the Service Model Package...
            if (this._commonSchema != null) this._commonSchema.Delete();
            this._rootService.ModelPkg.DeleteClass(this._capabilityClass);
            this._commonSchema = null;
            base.Delete();                  // Finally, finish off by deleting the parent resources.
        }

        /// <summary>
        /// Deletes the operation identified by the specified operation-class object. When the deleteResources indicator is set to 'true', the
        /// method physically deletes all resources (elements, packages, associations, etc.) that are used by the operation. When the indicator
        /// is set to 'false' (the default), the operation is just unlinked from the interface, but no resources are deleted.
        /// We do NOT update our service annotation and/or version info in this method since we do not know whether the delete has been executed
        /// one one or many interfaces. This could result in confusing log messages and/or incorrect versions so we leave the service update
        /// to the event code, which knows the context.
        /// </summary>
        /// <param name="operationClass">Identifies the operation to be deleted.</param>
        /// <param name="newMinorVersion">Set to 'true' when operation minor version must be updated, 'false' to keep existing version.</param>
        /// <param name="deleteResources">When set to 'true', the associated operation class with all related packages, elements, etc. are deleted 
        /// from the repository. When set to 'false' (default), the operation association is removed but all resources remain intact.</param>
        internal void DeleteOperation (MEClass operationClass, bool newMinorVersion, bool deleteResources = false)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp.deleteOperation >> Going to delete operation '" + operationClass.Name + "' from interface '" + Name + "'...");
            foreach (Capability cap in GetChildren())
            {
                if (cap.CapabilityClass == operationClass)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp.deleteOperation >> Found the operation!");
                    DeleteChild(cap.Implementation, deleteResources);
                    break;
                }
            }

            if (newMinorVersion)
            {
                var newVersion = new Tuple<int, int>(this._capabilityClass.Version.Item1, this._capabilityClass.Version.Item2 + 1);
                if (this._commonSchema != null) this._commonSchema.CapabilityClass.Version = newVersion;
                this._capabilityClass.Version = newVersion;
            }
            CreateLogEntry("Deleted Operation: '" + operationClass.Name + "'.");
            if (this._commonSchema != null) this._commonSchema.CreateLogEntry("Deleted Operation: '" + operationClass.Name + "'.");
        }

        /// <summary>
        /// Invoke in order to explicitly release resources. Don't use the object after calling Dispose!
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Returns the file name (without extension) for this Capability. The extension is left out since this typically depends on the
        /// chosen serialization mechanism. The filename returned by this method only provides a generic name to be used for further, serialization
        /// dependent, processing.
        /// If the OperationalStatus of the service is not equal to the default status, the filename also receives the OperationalStatus.
        /// </summary>
        internal override string GetBaseFileName()
        {
            Tuple<int, int> version = this.CapabilityClass.Version;
            string postfix = Conversions.ToPascalCase(RootService.IsDefaultOperationalStatus ? string.Empty : "_" + RootService.OperationalStatus);
            return this.Name + "_v" + version.Item1 + "p" + version.Item2 + postfix;
        }

        /// <summary>
        /// Returns a short textual identification of the capability type.
        /// </summary>
        /// <returns>Capability type name.</returns>
        internal override string GetCapabilityType()
        {
            return "Interface";
        }

        /// <summary>
        /// Creates an Interface object that matches the current Implementation.
        /// </summary>
        /// <returns>Interface object.</returns>
        internal override Capability GetInterface() { return new InterfaceCapability(this); }

        /// <summary>
        /// Returns the list of all Operation capabilities associated to this Interface Capability.
        /// </summary>
        /// <returns>List of Operation capabilities.</returns>
        internal List<OperationCapability> GetOperations()
        {
            string operationStereotype = ContextSlt.GetContextSlt().GetConfigProperty(_OperationClassStereotype);
            var operationList = new List<OperationCapability>();
            foreach (Capability cap in GetChildren())
            {
                if (cap is OperationCapability)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp.getOperations >> Found operation '" + cap.Name + "'!");
                    operationList.Add(cap as OperationCapability);
                }
            }
            return operationList;
        }

        /// <summary>
        /// This method is called whenever a (new) parent of the Capability has taken ownership of the Capability.
        /// </summary>
        /// <param name="parent">The parent Capability that has taken ownership of this Capability.</param>
        internal override void InitialiseParent(Capability parent)
        {
            // No operation: An Interface Capability is the 'top' of the Capability tree and will have no meaningful parent Capabilities.
            // It will thus no be used in practice.
            string parentMsg = (parent != null) ? parent.Name : "-Null Parent-";
            Logger.WriteWarning("Plugin.Application.CapabilityModel.APIManager.InterfaceCapabilityImp.initialiseParent >> Interface should not have parent Capability '" + parentMsg + "'!");
        }

        /// <summary>
        /// Renames the operation identified by the specified operation-class object.
        /// We do NOT update our service annotation and/or version info in this method since we do not know whether the rename has been executed
        /// one one or many interfaces. This could result in confusing log messages and/or incorrect versions so we leave the service update
        /// to the event code, which knows the context.
        /// </summary>
        /// <param name="operationClass">Operation to be renamed.</param>
        /// <param name="oldName">Original name of the operation.</param>
        /// <param name="newName">New name for the operation, in PascalCase.</param>
        /// <param name="newMinorVersion">Set to 'true' when operation minor version must be updated, 'false' to keep existing version.</param>
        internal void RenameOperation(MEClass operationClass, string oldName, string newName, bool newMinorVersion)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp.renameOperation >> Going to rename operation '" + operationClass.Name + "' to: '" + newName + "'...");
            if (string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName) || oldName == newName) return;     // Nothing to rename!

            foreach (Capability cap in GetChildren())
            {
                if (cap.CapabilityClass == operationClass)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp.renameOperation >> Found the operation!");
                    
                    // First of all, we attempt to locate the association between this capability and the operation to be renamed so we can rename the role...
                    foreach (MEAssociation association in this._capabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
                    {
                        if (association.Destination.EndPoint == cap.CapabilityClass)
                        {
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp.renameOperation >> Found child association...");
                            association.SetName(Conversions.ToCamelCase(newName), MEAssociation.AssociationEnd.Destination);
                            break;
                        }
                    }

                    if (cap.Name == oldName)    // Perform operation rename only if there is something to rename...
                    {
                        cap.Rename(newName);
                        if (newMinorVersion) // If we have to increment the minor version of the operation, do so BEFORE creating the log message...
                        {
                            var newVersion = new Tuple<int, int>(cap.CapabilityClass.Version.Item1, cap.CapabilityClass.Version.Item2 + 1);
                            cap.CapabilityClass.Version = newVersion;
                        }
                        cap.CreateLogEntry("Renamed from: '" + oldName + "' to: '" + newName + "'.");
                    }
                    break;
                }
            }

            // If we have to increment the minor version of the interface (and common schema), do so BEFORE creating the log message...
            // We create the new version and log message irrespective of a performed rename operation since, in case of identical old- and new names,
            // we can safely assume that this is caused by the same operation, being associated with multiple interfaces and only the first interface will
            // perform the actual rename.
            if (newMinorVersion) 
            {
                var newVersion = new Tuple<int, int>(this._capabilityClass.Version.Item1, this._capabilityClass.Version.Item2 + 1);
                if (this._commonSchema != null) this._commonSchema.CapabilityClass.Version = newVersion;
                this._capabilityClass.Version = newVersion;
            }
            string logMessage = "Renamed operation: '" + oldName + "' to: '" + operationClass.Name + "'.";
            CreateLogEntry(logMessage);
            if (this._commonSchema != null) this._commonSchema.CreateLogEntry(logMessage);
        }

        /// <summary>
        /// Process the current capability (i.e. generate output according to provided processor).
        /// Note that children of this capability are processed separately from 'handleCapabilties' in the parent class. There is thus no
        /// need to explicitly invoke child capability methods in here!
        /// </summary>
        /// <param name="processor">The capability processor to use for processing.</param>
        /// <param name="stage">The processing stage we're currently in, passed verbatim to processor.</param>
        /// <returns>True when processing can commence, false on errors.</returns>
        internal override bool HandleCapability(CapabilityProcessor processor, ProcessingStage stage)
        {
            // Since all the actual work is being performed by the processor, simply pass information onwards...
            return processor.ProcessCapability(new InterfaceCapability(this), stage);
        }

        /// <summary>
        /// Constructor that is used by specialized Interface Implementations. It ONLY passes the Service object to the base class, but does
        /// not attempt to create a valid Interface class (set all attributes to suitable defaults). Proper initialization is left to the
        /// specialized Interface instead!
        /// <param name="myService">All capabilities are, directly or indirectly, always associated with a single Service.</param>
        /// </summary>
        protected InterfaceCapabilityImp(Service myService) : base(myService)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp (for specialization) >> Creating new Interface capability for service '" +
                              myService.Name + "'...");

            this._disposed = false;
            this._commonSchema = null;
        }

        /// <summary>
        /// This is the actual disposing interface, which takes case of structural removal of the implementation type when no longer
        /// needed.
        /// </summary>
        /// <param name="disposing">Set to 'true' when called directly. Set to 'false' when called from the finalizer.</param>
        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                try
                {
                    if (this._commonSchema != null) this._commonSchema.Dispose();
                    this._disposed = true;
                }
                catch { };   // Ignore any exceptions, no use in processing them here.
            }
        }
    }
}
