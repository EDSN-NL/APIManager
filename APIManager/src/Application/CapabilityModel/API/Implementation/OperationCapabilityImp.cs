using System;
using System.Collections.Generic;
using Framework.Model;
using Framework.Logging;
using Framework.Context;
using Framework.Util;

namespace Plugin.Application.CapabilityModel.API
{
    internal class OperationCapabilityImp: CapabilityImp
    {
        // Configuration properties used by this module:
        private const string _OperationClassStereotype          = "OperationClassStereotype";
        private const string _ServiceOperationPkgStereotype     = "ServiceOperationPkgStereotype";
        private const string _CommonPkgName                     = "CommonPkgName";
        private const string _CommonPkgStereotype               = "CommonPkgStereotype";
        private const string _BusinessMessageClassStereotype    = "BusinessMessageClassStereotype";
        private const string _NSTokenTag                        = "NSTokenTag";
        private const string _SOAPDefaultNSToken                = "SOAPDefaultNSToken";
        private const string _OperationIDTag                    = "OperationIDTag";

        private List<InterfaceCapability> _myInterfaces;        // The set of interfaces that share this operation.
        private MEPackage _operationPackage;                    // The package in which the operation messages live.
        private int _operationID;                               // The unique Operation identifier assigned to this Operation in the context of its Service.
        private string _NSToken;                                // Namespace token.

        /// <summary>
        /// Getters for the OperationCapabilityImp:
        /// OperationPackage = Read/Write the Package in which the Operation resides.
        /// NSToken = Namespace token to be used for Operation-dependent schemas.
        /// FQName = Fully qualified namespace URI for the Operation.
        /// </summary>
        internal MEPackage OperationPackage
        {
            get { return this._operationPackage; }
            set { this._operationPackage = value; }
        }
        internal string NSToken               { get { return this._NSToken; } }
        internal string FQName                { get { return this._rootService.GetFQN("SOAPOperation", this._capabilityClass.Name, -1); } }
        internal int OperationID              { get { return this._operationID; } }

        /// <summary>
        /// Creates a new instance of an operation. The constructor creates the operation package and request- and response messages underneath this
        /// package. Corresponding classes are created in the Service Model.
        /// </summary>
        /// <param name="myInterface">The interface for which we're creating the operation.</param>
        /// <param name="operationName">The name of the operation.</param>
        internal OperationCapabilityImp(InterfaceCapability myInterface, string operationName): base(myInterface.RootService)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();

            try
            {
                // Create operation class in same package as service.
                MEPackage modelPkg = myInterface.RootService.ModelPkg;
                string myStereotype = context.GetConfigProperty(_OperationClassStereotype);
                this._myInterfaces = new List<InterfaceCapability>();
                this._capabilityClass = modelPkg.CreateClass(operationName, myStereotype);
                this._capabilityClass.Version = new Tuple<int, int>(myInterface.RootService.MajorVersion, 0);
                var operationCapItf = new OperationCapability(this);

                // Create the operation package and common sub-package...
                MEPackage declPackage = myInterface.RootService.DeclarationPkg;
                this._operationPackage = declPackage.CreatePackage(operationName, context.GetConfigProperty(_ServiceOperationPkgStereotype), 50);
                MEPackage commonPackage = OperationPackage.CreatePackage(context.GetConfigProperty(_CommonPkgName), context.GetConfigProperty(_CommonPkgStereotype));

                // Request an OperationID...
                this._operationID = this._rootService.GetNewOperationID();
                this._capabilityClass.SetTag(context.GetConfigProperty(_OperationIDTag), this._operationID.ToString(), true);
                this._capabilityClass.SetTag(context.GetConfigProperty(_NSTokenTag), "ns" + this._operationID);
                this._NSToken = "ns" + this._operationID;

                // Establish link with the interface...
                this._assignedRole = Conversions.ToCamelCase(operationName);
                var interfaceEndpoint = new EndpointDescriptor(myInterface.CapabilityClass, "1", myInterface.Name, null, false);
                var operationEndpoint = new EndpointDescriptor(this._capabilityClass, "1", this._assignedRole, null, true);
                model.CreateAssociation(interfaceEndpoint, operationEndpoint, MEAssociation.AssociationType.MessageAssociation);

                // Create the Message Capability classes for Request and Response...
                var requestCap = new MessageCapability(operationCapItf, operationName, MessageCapability.MessageType.Request);
                var responseCap = new MessageCapability(operationCapItf, operationName, MessageCapability.MessageType.Response);
                requestCap.InitialiseParent(operationCapItf);
                responseCap.InitialiseParent(operationCapItf);
                if (!requestCap.Valid || !responseCap.Valid)
                {
                    // Oops, something went terribly wrong during construction of the messages. Invalidate and exit!
                    Logger.WriteWarning("Plugin.Application.CapabilityModel.API.OperationCapabilityImp >> Failed to create operation '" + operationName + "'!");
                    this._capabilityClass = null;       // Invalidates the capability!
                }
                if (this._capabilityClass != null) CreateLogEntry("Initial release.");
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.OperationCapabilityImp (new) >> Error creating capability because: " + exc.Message);
                this._capabilityClass = null;   // Assures that instance is declared invalid.
            }
        }

        /// <summary>
        /// Generic constructor to be used for existing class models. Th constructor initialises local context and creates the subordinate messages.
        /// </summary>
        /// <param name="myInterface">The interface for which we create the operation.</param>
        /// <param name="hierarchy">Class hierarchy consisting of Operation- and associated Message objects.</param>
        internal OperationCapabilityImp(InterfaceCapability myInterface, TreeNode<MEClass> hierarchy): base(myInterface.RootService)
        {
            try
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OperationCapabilityImp (existing) >> Creating new instance '" + 
                                 myInterface.Name + "." + hierarchy.Data.Name + "'...");
                ContextSlt context = ContextSlt.GetContextSlt();
                this._myInterfaces = new List<InterfaceCapability>();
                this._capabilityClass = hierarchy.Data;
                var operationCapItf = new OperationCapability(this);

                MEPackage declPackage = myInterface.RootService.DeclarationPkg;
                this._operationPackage = declPackage.FindPackage(hierarchy.Data.Name, context.GetConfigProperty(_ServiceOperationPkgStereotype));
                this._assignedRole = myInterface.FindChildClassRole(hierarchy.Data.Name, context.GetConfigProperty(_OperationClassStereotype));

                // Try to obtain a valid OperationID...
                this._operationID = -1;
                string operationID = this._capabilityClass.GetTag(context.GetConfigProperty(_OperationIDTag));
                if (string.IsNullOrEmpty(operationID))
                {
                    // We don't have a valid OperationID; probably old-style operation, try to convert namespace token...
                    string nsToken = this._capabilityClass.GetTag(context.GetConfigProperty(_NSTokenTag));
                    if (!string.IsNullOrEmpty(nsToken) && nsToken.StartsWith("ns"))
                    {
                        // We're in luck...
                        operationID = nsToken.Substring(2);
                        if (int.TryParse(operationID, out this._operationID))
                        {
                            // If Ok, we KEEP this ID. Tell the root service that this ID is in use.
                            this._rootService.SyncOperationID(this._operationID);
                            this._capabilityClass.SetTag(context.GetConfigProperty(_OperationIDTag), this._operationID.ToString(), true);
                        }
                        else this._operationID = -1;
                    }
                }
                else if (!int.TryParse(operationID, out this._operationID)) this._operationID = -1;
                if (this._operationID < 0)
                {
                    this._operationID = this._rootService.GetNewOperationID();    // No luck with existing ID, get a brand new one instead.
                    this._capabilityClass.SetTag(context.GetConfigProperty(_OperationIDTag), this._operationID.ToString());
                    this._capabilityClass.SetTag(context.GetConfigProperty(_NSTokenTag), "ns" + this._operationID);
                }
                this._NSToken = "ns" + this._operationID;

                foreach (TreeNode<MEClass> node in hierarchy.Children)
                {
                    var msg = new MessageCapability(operationCapItf, node.Data);
                    msg.InitialiseParent(operationCapItf);
                    if (!msg.Valid)
                    {
                        Logger.WriteError("Plugin.Application.CapabilityModel.API.OperationCapabilityImp (existing) >> Error creating Message '" + node.Data.Name + "'!");
                        this._capabilityClass = null;
                        return;
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.OperationCapabilityImp (existing) >> Error creating capability because: " + exc.Message);
                this._capabilityClass = null;   // Assures that instance is declared invalid.
            }
        }

        /// <summary>
        /// Associate the operation with the given Interface. This implies registering as a child of the interface and creating
        /// an association between the two classes. If 'newMinorVersion' is set to 'true', the minor version of the operation
        /// is incremented (we do NOT touch the versions of our parent interface and/or service since we do not know for sure
        /// what the scope of the call is (multiple operations can be involved).
        /// Finally, a log message is created for the operation.
        /// </summary>
        /// <param name="thisInterface">Interface with which we want to be associated.</param>
        /// <param name="newMinorVersion">Set to 'true' if the minor version must be incremented.</param>
        internal void AssociateInterface(InterfaceCapability thisInterface, bool newMinorVersion)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp.associateInterface >> Associating operation '" +
                             this.Name + "' with interface '" + thisInterface.Name + "'...");
            // If myInterfaces is NULL, we don't support these operation since we don't want to track interfaces (specialized operations do this).
            if (this._myInterfaces != null)
            {
                thisInterface.AddChild(new OperationCapability(this));
                this._myInterfaces.Add(thisInterface);

                // Establish link with the interface...
                string roleName = Conversions.ToCamelCase(this.Name);
                var interfaceEndpoint = new EndpointDescriptor(thisInterface.CapabilityClass, "1", thisInterface.Name, null, false);
                var operationEndpoint = new EndpointDescriptor(this._capabilityClass, "1", roleName, null, true);
                ModelSlt.GetModelSlt().CreateAssociation(interfaceEndpoint, operationEndpoint, MEAssociation.AssociationType.MessageAssociation);

                if (newMinorVersion) UpdateMinorVersion();
                CreateLogEntry("Associated Operation with Interface: '" + thisInterface.Name + "'.");
            }
        }

        /// <summary>
        /// Overrides the default Capability.delete in order to assure that the message capabilities are deleted as well as the operation package.
        /// On return, all operation resources, including the package tree, are deleted and the Capability is INVALID.
        /// </summary>
        internal override void Delete()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OperationCapabilityImp.delete >> Deleting the operation package and all sub-packages...");

            // Remove this entry from the list child entries of all Interfaces...
            if (this._myInterfaces != null) foreach (InterfaceCapability itf in this._myInterfaces) itf.RemoveChild(new OperationCapability(this));
            base.Delete();                                                          // Deletes the children (messages) as well as the capability class.
            if (this._operationPackage != null) this._operationPackage.Parent.DeletePackage(this._operationPackage);    // Delete the package as last step.
        }

        /// <summary>
        /// Dissociate the Operation from the specified Interface. We do NOT check whether the Interface is indeed associates (thus, an exception will be
        /// thrown when the specified Interface is not in the list of associated Interfaces). If this was the ONLY interface associated with the Operation,
        /// the method returns FALSE. Otherwise, if Interfaces remain associated, we create a log message for the Operation.
        /// Note that the association between Interface and Operation is NOT touched by this method. It is therefore the responsibility of the Interface
        /// to delete the association if required.
        /// </summary>
        /// <param name="thisInterface">Interface to dissociate.</param>
        /// <returns>True when other Interfaces remain associated with the Operation, False when the Operation has been turned into an orphan.</returns>
        internal void DissociateInterface(InterfaceCapability thisInterface)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp.dissociateInterface >> Dissociating operation '" +
                             this.Name + "' from interface '" + thisInterface.Name + "'...");
            // If myInterfaces is NULL, we don't support this operation (specialized operations can do stuff differently).
            if (this._myInterfaces != null)
            {
                if (this._myInterfaces.Count > 1)
                {
                    // We are associated with multiple Interfaces, just unlink...
                    thisInterface.DeleteChild(new OperationCapability(this), false);    // Unlink from parent and delete association.
                    this._myInterfaces.Remove(thisInterface);
                    CreateLogEntry("Dissociated Operation from Interface: '" + thisInterface.Name + "'.");

                }
                else
                {
                    // Dissociate from only Interface, delete all child resources as well.
                    thisInterface.DeleteChild(new OperationCapability(this), true);
                }
            }
        }

        /// <summary>
        /// Returns the file name (without extension) for this Capability. The extension is left out since this typically depends on the
        /// chosen serialization mechanism. The filename returned by this method only provides a generic name to be used for further, serialization
        /// dependent, processing.
        /// If the OperationalStatus of the service is not equal to the default, we also include the OperationalStatus in the filename.
        /// </summary>
        internal override string GetBaseFileName()
        {
            Tuple<int, int> version = this.CapabilityClass.Version;
            string postfix = Conversions.ToPascalCase(RootService.IsDefaultOperationalStatus ? string.Empty : "_" + RootService.OperationalStatus);
            return this._rootService.Name + "_" + this.Name + "_v" + version.Item1 + "p" + version.Item2 + postfix;
        }

        /// <summary>
        /// Returns a short textual identification of the capability type.
        /// </summary>
        /// <returns>Capability type name.</returns>
        internal override string GetCapabilityType()
        {
            return "Operation";
        }

        /// <summary>
        /// Creates an Interface object that matches the current Implementation.
        /// </summary>
        /// <returns>Interface object.</returns>
        internal override Capability GetInterface() { return new OperationCapability(this); }

        /// <summary>
        /// Process the capability (i.e. generate output according to provided processor.).
        /// </summary>
        /// <param name="stage">The processing stage we're currently in, passed verbatim to processor.</param>
        /// <returns>True when processing can commence, false on errors.</returns>
        internal override bool HandleCapability(CapabilityProcessor processor, ProcessingStage stage)
        {
            // Since all the actual work is being performed by the processor, simply pass information onwards...
            return processor.ProcessCapability(new OperationCapability(this), stage);
        }

        /// <summary>
        /// This method is called whenever a (new) parent of the Capability has taken ownership of the Capability. If this parent is an Interface,
        /// we have to register the current instance with that Interface.
        /// </summary>
        /// <param name="parent">The parent Capability that has taken ownership of this Capability.</param>
        internal override void InitialiseParent(Capability parent)
        {
            if (parent is InterfaceCapability && this._myInterfaces != null)
            {
                this._myInterfaces.Add(parent as InterfaceCapability);
                parent.AddChild(new OperationCapability(this));
            }
        }

        /// <summary>
        /// Overrides the 'rename' operation for SOAP Operation capabilities. In this particular case, we have to rename both
        /// the class (taken care of by invoking base.rename) as well as the operation package.
        /// </summary>
        /// <param name="newName">New name to be assigned to the operation</param>
        internal override void Rename(string newName)
        {
            base.Rename(newName);                                                       // Takes care of renaming the class.
            if (this._operationPackage != null) this._operationPackage.Name = newName;  // And this renames the package.
            foreach (Capability cap in GetChildren()) cap.Rename(newName);              // And this renames the child capabilities.
        }

        /// <summary>
        /// Constructor for use by specialised Operation types. Since the constructor does not receive any class details of the specialized
        /// operation class, the actions performed by this constructor are rather minimal. It just initialises the parent and creates a 
        /// package for the operation.
        /// </summary>
        /// <param name="parent">Capability that acts as parent for the specialised operation.</param>
        /// <param name="packageName">Name of package to construct.</param>
        /// <param name="packageStereotype">Stereotype of package to construct.</param>
        protected OperationCapabilityImp(Capability parent, string packageName, string packageStereotype) : base(parent.RootService)
        {
            // Create a package for the derived operation. We don't create any packages below this operation package, since requirements for 
            // extra packages depend on the functionality of the specialized operation type...
            MEPackage declPackage = parent.RootService.DeclarationPkg;
            this._operationPackage = declPackage.CreatePackage(packageName, packageStereotype, 50);
            this._myInterfaces = new List<InterfaceCapability>();
            this._operationID = -1;
            this._NSToken = string.Empty;
        }

        /// <summary>
        /// Constructor that is used by specialized Operation Implementations. The constructor receives an existing operation class and creates
        /// the basic structure for this level of Operation Capability. It does not attempt to create any children (this is left to the
        /// specialised operation).
        /// <param name="myService">All capabilities are, directly or indirectly, always associated with a single Service.</param>
        /// </summary>
        protected OperationCapabilityImp(Capability parent, MEClass operationClass, string operationPackageStereotype, string operationClassStereotype) : base(parent.RootService)
        {
            try
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OperationCapabilityImp (existing, specialized) >> Creating new instance '" +
                                 parent.Name + "." + operationClass.Name + "'...");
                ContextSlt context = ContextSlt.GetContextSlt();
                this._myInterfaces = new List<InterfaceCapability>();
                this._capabilityClass = operationClass;

                MEPackage declPackage = parent.RootService.DeclarationPkg;
                this._operationPackage = declPackage.FindPackage(operationClass.Name, operationPackageStereotype);
                this._assignedRole = parent.FindChildClassRole(operationClass.Name, operationClassStereotype);

                // Try to obtain a valid OperationID...
                this._operationID = -1;
                string operationID = this._capabilityClass.GetTag(context.GetConfigProperty(_OperationIDTag));
                if (string.IsNullOrEmpty(operationID))
                {
                    // We don't have a valid OperationID; probably old-style operation, try to convert namespace token...
                    string nsToken = this._capabilityClass.GetTag(context.GetConfigProperty(_NSTokenTag));
                    if (!string.IsNullOrEmpty(nsToken) && nsToken.StartsWith("ns"))
                    {
                        // We're in luck...
                        operationID = nsToken.Substring(2);
                        if (int.TryParse(operationID, out this._operationID))
                        {
                            // If Ok, we KEEP this ID. Tell the root service that this ID is in use.
                            this._rootService.SyncOperationID(this._operationID);
                            this._capabilityClass.SetTag(context.GetConfigProperty(_OperationIDTag), this._operationID.ToString(), true);
                        }
                        else this._operationID = -1;
                    }
                }
                else if (!int.TryParse(operationID, out this._operationID)) this._operationID = -1;
                if (this._operationID < 0) AssignNewOperationID();
                this._NSToken = "ns" + this._operationID;
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.OperationCapabilityImp (existing, specialized) >> Error creating capability because: " + exc.Message);
                this._capabilityClass = null;   // Assures that instance is declared invalid.
            }
        }

        /// <summary>
        /// Helper function that assignes a unique operation ID to this class. It must be called for those cases that the class is either constructed
        /// as a new class, or if we find out the an existing class does not yet possess a proper ID.
        /// </summary>
        protected void AssignNewOperationID()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            this._operationID = this._rootService.GetNewOperationID();
            this._capabilityClass.SetTag(context.GetConfigProperty(_OperationIDTag), this._operationID.ToString(), true);
            this._capabilityClass.SetTag(context.GetConfigProperty(_NSTokenTag), "ns" + this._operationID);
            this._NSToken = "ns" + this._operationID;
        }
    }
}
