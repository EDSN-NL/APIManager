using System;
using System.Collections.Generic;
using Framework.Model;
using Framework.Logging;
using Framework.Context;
using Framework.Util;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// Implementation class for REST Interfaces. A REST Interface contains a collection of ResourceCollections, each of which implementing
    /// a specific REST Flow.
    /// </summary>
    internal class RESTInterfaceCapabilityImp: InterfaceCapabilityImp
    {
        // Configuration properties used by this module.
        private const string _InterfaceContractClassStereotype      = "InterfaceContractClassStereotype";
        private const string _InterfaceContractTypeTag              = "InterfaceContractTypeTag";
        private const string _InterfaceDefaultRESTContract          = "InterfaceDefaultRESTContract";
        private const string _ResourceClassStereotype               = "ResourceClassStereotype";
        private const string _ContactTypeClassName                  = "ContactTypeClassName";
        private const string _LicenseTypeClassName                  = "LicenseTypeClassName";
        private const string _TermsOfServiceAttributeName           = "TermsOfServiceAttributeName";
        private const string _APISupportModelPathName               = "APISupportModelPathName";
        private const string _CoreDataTypesPathName                 = "CoreDataTypesPathName";
        private const string _BusinessComponentStereotype           = "BusinessComponentStereotype";

        // Names of attributes to be used for Contact- and License descriptor. These MUST be a case-insensitive match of the
        // associated OpenAPI property names!
        private const string _TermsOfServiceClassifier              = "TextType";   // Classifier for Terms of Service.
        private const string _NameClassifier                        = "NameType";   // Classifier for names.
        private const string _IDClassifier                          = "IdentifierType"; // Classifier for identities.
        private const string _LicenseNameAttribute                  = "Name";       // Attribute name for License.Name
        private const string _LicenseURLAttribute                   = "URL";        // Attribute name for License.URL
        private const string _ContactNameAttribute                  = "Name";       // Attribute name for Contact.Name
        private const string _ContactURLAttribute                   = "URL";        // Attribute name for Contact.URL
        private const string _ContactEMailAttribute                 = "EMail";      // Attribute name for Contact.EMail

        /// <summary>
        /// Create constructor, used to create a new instance of an Interface. The constructor assumes that the package structure
        /// exists and that there exists a service to which we can connect the new capability. The constructor creates the
        /// appropriate model elements in the correct packages and links stuff together. If no resource collections are specified, the
        /// constructor only creates the Interface, Common Schema (if required to do so) and association with the service. 
        /// Collections can be added seperately.
        /// By default, the constructor creates resource COLLECTIONS at this level, since this is the most common use case.
        /// When the constructor is invoked without any resource names, it creates an EMPTY Resource Collection by default.
        /// <param name="myService">All capabilities are, directly or indirectly, always associated with a single Service.</param>
        /// <param name="metaData">Additional user-provided metadata for this API.</param>
        /// <param name="resources">Optional: the resource collections that this initial version must support. An empty
        /// resource collection is created in case the list is omitted.</param>
        /// </summary>
        internal RESTInterfaceCapabilityImp(Service myService, RESTInterfaceCapability.MetaData metaData, List<RESTResourceDeclaration> resources): base(myService)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTInterfaceCapabilityImp (create) >> Creating new Interface capability for service '" +
                              myService.Name + "', with name: '" + metaData.qualifiedName + "'...");
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();

            try
            {
                // Create capability class in same package as service.
                MEPackage owner = myService.ModelPkg;
                string interfaceName = metaData.qualifiedName.Substring(0, metaData.qualifiedName.IndexOf("_V"));
                string myStereotype = context.GetConfigProperty(_InterfaceContractClassStereotype);
                string coreDataTypesPath = context.GetConfigProperty(_CoreDataTypesPathName);
                this._capabilityClass = owner.CreateClass(interfaceName, myStereotype);
                this._capabilityClass.Version = myService.Version;
                this._capabilityClass.SetTag(context.GetConfigProperty(_InterfaceContractTypeTag), context.GetConfigProperty(_InterfaceDefaultRESTContract));
                this._assignedRole = RESTUtil.GetAssignedRoleName(interfaceName);
                var interfaceCapItf = new RESTInterfaceCapability(this);

                // Establish link with the service...
                var source = new EndpointDescriptor(myService.ServiceClass, "1", Name, null, false);
                var target = new EndpointDescriptor(this._capabilityClass, "1", this._assignedRole, null, true);
                model.CreateAssociation(source, target, MEAssociation.AssociationType.MessageAssociation);

                // Process the metadata...
                if (!string.IsNullOrEmpty(metaData.termsOfService))
                {
                    MEDataType classifier = model.FindDataType(coreDataTypesPath, _TermsOfServiceClassifier);
                    if (classifier != null)
                    {
                        this._capabilityClass.CreateAttribute(context.GetConfigProperty(_TermsOfServiceAttributeName), classifier,
                                                              AttributeType.Attribute, metaData.termsOfService, new Tuple<int, int>(1, 1), true);
                    }
                    else Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTInterfaceCapabilityImp (create) >> Unable to find Classifier '" +
                                            coreDataTypesPath + "/" + _TermsOfServiceClassifier);
                }

                // Add license details (if specified)...
                MEDataType nameType = model.FindDataType(coreDataTypesPath, _NameClassifier);
                MEDataType identifierType = model.FindDataType(coreDataTypesPath, _IDClassifier);
                var mandatory = new Tuple<int, int>(1, 1);
                if (nameType != null && identifierType != null)
                {
                    if (!string.IsNullOrEmpty(metaData.licenseName))
                    {
                        MEClass licenseClass = owner.CreateClass(context.GetConfigProperty(_LicenseTypeClassName), context.GetConfigProperty(_BusinessComponentStereotype));
                        if (licenseClass != null)
                        {
                            licenseClass.CreateAttribute(_LicenseNameAttribute, nameType, AttributeType.Attribute, metaData.licenseName, mandatory, true);
                            if (!string.IsNullOrEmpty(metaData.licenseURL))
                                licenseClass.CreateAttribute(_LicenseURLAttribute, identifierType, AttributeType.Attribute, metaData.licenseURL, mandatory, true);

                            string role = licenseClass.Name.EndsWith("Type") ? licenseClass.Name.Substring(0, licenseClass.Name.IndexOf("Type")) : licenseClass.Name;
                            var licenseEndpoint = new EndpointDescriptor(licenseClass, "1", role, null, true);
                            model.CreateAssociation(target, licenseEndpoint, MEAssociation.AssociationType.MessageAssociation);
                        }
                        else Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTInterfaceCapabilityImp (create) >> Unable to find License Class '" +
                            context.GetConfigProperty(_APISupportModelPathName) + "/" + context.GetConfigProperty(_LicenseTypeClassName));
                    }

                    // Add contact details (is specified)...
                    if (!string.IsNullOrEmpty(metaData.contactName) ||
                        !string.IsNullOrEmpty(metaData.contactURL) ||
                        !string.IsNullOrEmpty(metaData.contactEMail))
                    {
                        MEClass contactClass = owner.CreateClass(context.GetConfigProperty(_ContactTypeClassName), context.GetConfigProperty(_BusinessComponentStereotype));
                        if (contactClass != null)
                        {
                            if (!string.IsNullOrEmpty(metaData.contactName))
                                contactClass.CreateAttribute(_ContactNameAttribute, nameType, AttributeType.Attribute, metaData.contactName, mandatory, true);
                            if (!string.IsNullOrEmpty(metaData.contactURL))
                                contactClass.CreateAttribute(_ContactURLAttribute, identifierType, AttributeType.Attribute, metaData.contactURL, mandatory, true);
                            if (!string.IsNullOrEmpty(metaData.contactEMail))
                                contactClass.CreateAttribute(_ContactEMailAttribute, identifierType, AttributeType.Attribute, metaData.contactEMail, mandatory, true);

                            string role = contactClass.Name.EndsWith("Type") ? contactClass.Name.Substring(0, contactClass.Name.IndexOf("Type")) : contactClass.Name;
                            var contactEndpoint = new EndpointDescriptor(contactClass, "1", role, null, true);
                            model.CreateAssociation(target, contactEndpoint, MEAssociation.AssociationType.MessageAssociation);
                        }
                        else Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTInterfaceCapabilityImp (create) >> Unable to find Contact Class '" +
                                               context.GetConfigProperty(_APISupportModelPathName) + "/" + context.GetConfigProperty(_ContactTypeClassName));
                    }
                }
                else Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTInterfaceCapabilityImp (create) >> Unable to retrieve required classifiers from '" + coreDataTypesPath + "'!");

                // If we didn't specify any root resources, we create an empty resource collection by default.
                if (resources == null || resources.Count == 0)
                {
                    // If no resources have been specified, we implicitly create an empty resource instead...
                    if (resources == null) resources = new List<RESTResourceDeclaration>();
                    resources.Add(new RESTResourceDeclaration());
                }

                string newNames = string.Empty;
                bool isFirst = true;                // Little trick to get the right amount of ',' separators.
                foreach (RESTResourceDeclaration resource in resources)
                {
                    // Like all Capabilities, Resource Collections are implemented as a Bridge and we have to explicitly
                    // register my Interface with the Collection...
                    var resourceCapability = new RESTResourceCapability(interfaceCapItf, resource);
                    if (resourceCapability.Valid)
                    {
                        resourceCapability.InitialiseParent(interfaceCapItf);
                        newNames += (!isFirst) ? ", " + resource.Name : resource.Name;
                        isFirst = false;
                    }
                    else
                    {
                        // Oops, something went terribly wrong during construction of the collection. Invalidate and exit!
                        Logger.WriteWarning("Failed to create resource '" + resource.Name + "'!");
                        this._capabilityClass = null;       // Invalidates the capability!
                    }
                }

                if (this._capabilityClass != null)
                {
                    myService.AddCapability(interfaceCapItf);
                    CreateLogEntry("Initial release with collection(s): " + newNames);
                    if (!string.IsNullOrEmpty(metaData.description)) MEChangeLog.SetRTFDocumentation(this._capabilityClass, metaData.description);
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTInterfaceCapabilityImp (create) >> Error creating capability because: " + exc.ToString());
                this._capabilityClass = null;   // Assures that instance is declared invalid.
            }
        }

        /// <summary>
        /// Generic constructor to be used for existing class models. The constructor creates all of the subordinate objects (common schema and 
        /// all resource collections) and builds the complete object hierarchy, including registration with the parent Service.
        /// Please note that it is the responsibility of the CHILD to register with the PARENT and not the other way around. In other words: the
        /// Resource Collections will add themselves to the capability tree.
        /// The constructor receives a 'ready-built' hierarchy of all Collections, Path Expressions and Operations through the 'hierarchy' property. 
        /// The root-node represents the Interface.
        /// </summary>
        /// <param name="myService">Associated service instance.</param>
        /// <param name="hierarchy">Capability class hierarchy for this Interface.</param>
        internal RESTInterfaceCapabilityImp(Service myService, TreeNode<MEClass> hierarchy): base(myService)
        {
            try
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTInterfaceCapabilityImp (existing) >> Creating new instance '" +
                                  myService.Name + "." + hierarchy.Data.Name + "'...");
                ContextSlt context = ContextSlt.GetContextSlt();
                this._capabilityClass = hierarchy.Data;
                this._assignedRole = this._capabilityClass.Name;
                var interfaceCapItf = new RESTInterfaceCapability(this);
                this.CommonSchema = null;

                foreach (TreeNode<MEClass> node in hierarchy.Children)
                {
                    // Since we use a bridge pattern for the Capabilities, it might be that the Resource is not actually constructed, but an existing
                    // object is returned. That's why the constructor does not rely on a particular parent and parent initialisation must be 
                    // performed explicitly by invocation of 'initialiseParent'.....
                    var resource = new RESTResourceCapability(interfaceCapItf, node);
                    resource.InitialiseParent(interfaceCapItf);
                    if (!resource.Valid)
                    {
                        Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTInterfaceCapabilityImp (existing) >> Error creating resource '" + node.Data.Name + "'!");
                        this._capabilityClass = null;
                        return;

                    }
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTInterfaceCapabilityImp (existing) >> Error creating capability because: " + exc.ToString());
                this._capabilityClass = null;   // Assures that instance is declared invalid.
            }
        }

        /// <summary>
        /// Add one or more resources to the current interface. We do not check whether the names are indeed unique, 
        /// this should have been done during creation (e.g. in user interface).
        /// We don't attempt to filter on resource types since we might have valid reason to add each type of resource
        /// to the Interface (not just collections).
        /// It is the responsibility of the resource to register with the parent capability tree.
        /// </summary>
        /// <param name="resources">List of resources that must be added.</param>
        /// <param name="newMinorVersion">True when a new minor version must be created. Parameter is ignored when CM is active!</param>
        /// <returns>True if operation completed successfully, false on errors.</returns>
        internal bool AddResources(List<RESTResourceDeclaration> resources, bool newMinorVersion)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTInterfaceCapabilityImp.AddResources >> Adding new resources...");
            bool result = true;
            try
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                string newNames = string.Empty;
                var parentInterface = new RESTInterfaceCapability(this);
                bool isFirst = true;
                foreach (RESTResourceDeclaration resource in resources)
                {
                    var resourceCapability = new RESTResourceCapability(parentInterface, resource);
                    if (resourceCapability.Valid)
                    {
                        resourceCapability.InitialiseParent(parentInterface);
                        string roleName = RESTUtil.GetAssignedRoleName(resource.Name);
                        newNames += (!isFirst) ? ", " + roleName : roleName;
                        isFirst = false;
                    }
                    else
                    {
                        Logger.WriteWarning("Failed to create resource '" + resource.Name + "'!");
                        result = false;
                    }
                }

                // This will update the service version, followed by all child capabilities!
                // But the operation is executed ONLY when configuration management is disabled (with CM enabled, versions are
                // managed differently).
                if (!this._rootService.UseConfigurationMgmt && newMinorVersion) RootService.IncrementVersion();

                string logMessage = "Added Resource Collection(s): '" + newNames + "'";
                RootService.CreateLogEntry(logMessage + " to Interface '" + Name + "'.");
                CreateLogEntry(logMessage + ".");
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTInterfaceCapabilityImp.AddResources >> Exception caught: " + exc.ToString());
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Deletes the resource identified by the specified resource-class object. This will delete the entire resource hierarchy.
        /// </summary>
        /// <param name="resourceClass">Identifies the resource to be deleted.</param>
        /// <param name="newMinorVersion">Set to 'true' when minor version must be updated, 'false' to keep existing version. Parameter is 
        /// ignored when CM is active!</param>
        internal void DeleteResource(MEClass resourceClass, bool newMinorVersion)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp.DeleteResource >> Going to delete resource '" + resourceClass.Name + "' from interface '" + Name + "'...");
            foreach (Capability cap in GetChildren())
            {
                if (cap.CapabilityClass == resourceClass)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp.DeleteResource >> Found the resource!");
                    DeleteChild(cap.Implementation, true);
                    break;
                }
            }

            // This will update the service version, followed by all child capabilities!
            // But the operation is executed ONLY when configuration management is disabled (with CM enabled, versions are
            // managed differently).
            if (!this._rootService.UseConfigurationMgmt && newMinorVersion) RootService.IncrementVersion();

            string logMessage = "Deleted Resource: '" + resourceClass.Name + "'";
            RootService.CreateLogEntry(logMessage + " from Interface '" + Name + "'.");
            CreateLogEntry(logMessage + ".");
        }

        /// <summary>
        /// Renames the resource identified by the specified resource-class object.
        /// </summary>
        /// <param name="resourceClass">Collection to be renamed.</param>
        /// <param name="oldName">Original name of the collection.</param>
        /// <param name="newName">New name for the collection, in PascalCase.</param>
        /// <param name="newMinorVersion">Set to 'true' when minor version must be updated, 'false' to keep existing version.
        /// Parameter is ignored when CM is active!</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal void RenameResource(MEClass resourceClass, string oldName, string newName, bool newMinorVersion)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTInterfaceCapabilityImp.renameResource >> Going to rename resource '" + resourceClass.Name + "' to: '" + newName + "'...");
            if (string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName) || oldName == newName) return;     // Nothing to rename!

            foreach (Capability cap in GetChildren())
            {
                if (cap.CapabilityClass == resourceClass)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTInterfaceCapabilityImp.RenameResource >> Found the resource!");

                    // First of all, we attempt to locate the association between this capability and the resource to be renamed so we can rename the role...
                    foreach (MEAssociation association in this._capabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
                    {
                        if (association.Destination.EndPoint == cap.CapabilityClass)
                        {
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTInterfaceCapabilityImp.RenameResource >> Found child association...");
                            association.SetName(RESTUtil.GetAssignedRoleName(Conversions.ToCamelCase(newName)), MEAssociation.AssociationEnd.Destination);
                            break;
                        }
                    }

                    if (cap.Name == oldName)    // Perform operation rename only if there is something to rename...
                    {
                        cap.Rename(newName);
                        if (newMinorVersion) // If we have to increment the minor version, do so BEFORE creating the log message...
                        {
                            var newVersion = new Tuple<int, int>(cap.CapabilityClass.Version.Item1, cap.CapabilityClass.Version.Item2 + 1);
                            cap.CapabilityClass.Version = newVersion;
                        }
                        cap.CreateLogEntry("Renamed from: '" + oldName + "' to: '" + newName + "'.");
                    }
                    break;
                }
            }

            // This will update the service version, followed by all child capabilities!
            // But the operation is executed ONLY when configuration management is disabled (with CM enabled, versions are
            // managed differently).
            if (!this._rootService.UseConfigurationMgmt && newMinorVersion) RootService.IncrementVersion();

            string logMessage = "Renamed resource: '" + oldName + "' to: '" + resourceClass.Name + "'.";
            RootService.CreateLogEntry(logMessage + " in Interface '" + Name + "'.");
            CreateLogEntry(logMessage + ".");
        }

        /// <summary>
        /// Facilitates iteration over the set of resources associated with this interface.
        /// </summary>
        /// <returns>Resource Capability enumerator.</returns>
        internal IEnumerable<RESTResourceCapability> ResourceList()
        {
            foreach (Capability cap in GetChildren())
            {
                if (cap is RESTResourceCapability) yield return cap as RESTResourceCapability;
            }
        }

        /// <summary>
        /// Returns the list of all resource capabilities associated with this Interface.
        /// </summary>
        /// <returns>List of REST-Resource capabilities.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal List<RESTResourceCapability> GetResources()
        {
            var collectionList = new List<RESTResourceCapability>();
            foreach (Capability cap in GetChildren())
            {
                if (cap is RESTResourceCapability)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.InterfaceCapabilityImp.GetResourceCollections >> Found resource '" + cap.Name + "'!");
                    collectionList.Add(cap as RESTResourceCapability);
                }
            }
            return collectionList;
        }

        /// <summary>
        /// Deletes the current Interface from the model. Collections associated with the Interface are deleted as well. 
        /// On return, the Interface object is INVALID and should NOT be used anymore!
        /// This method overrides the default 'delete' operation from the base class and thus assures that resources are deleted selectively.
        /// </summary>
        internal override void Delete()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTInterfaceCapabilityImp.delete >> Deleting Interface '" + Name + "'...");
            var interfaceCapItf = new RESTInterfaceCapability(this);

            // We use 'GetResources' here since this returns a stable collection (otherwise, we're deleting entries for the same collection that is
            // being iterated, which could result in unexpected behavior).
            foreach (RESTResourceCapability child in GetResources())
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTInterfaceCapabilityImp.delete >> Deleting resource '" + child.Name + "'...");
                child.Delete();
            }
            base.Delete();  // Finally, finish off by deleting the parent resources (this will delete the Common Schema and de-registers with Service).
        }

        /// <summary>
        /// Returns a short textual identification of the capability type.
        /// </summary>
        /// <returns>Capability type name.</returns>
        internal override string GetCapabilityType()
        {
            return "REST Interface";
        }

        /// <summary>
        /// Creates an Interface object that matches the current Implementation.
        /// </summary>
        /// <returns>Interface object.</returns>
        internal override Capability GetInterface() { return new RESTInterfaceCapability(this); }

        /// <summary>
        /// This method is called whenever a (new) parent of the Capability has taken ownership of the Capability.
        /// </summary>
        /// <param name="parent">The parent Capability that has taken ownership of this Capability.</param>
        internal override void InitialiseParent(Capability parent)
        {
            // No operation: An Interface Capability is the 'top' of the Capability tree and will have no meaningful parent Capabilities.
            // It will thus no be used in practice.
            string parentMsg = (parent != null) ? parent.Name : "-Null Parent-";
            Logger.WriteWarning("Interface should not have parent Capability '" + parentMsg + "'!");
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
            return processor.ProcessCapability(new RESTInterfaceCapability(this), stage);
        }
    }
}
