using System;
using System.Collections.Generic;
using Framework.Model;
using Framework.Logging;
using Framework.Context;
using Framework.Util;
using Framework.View;
using Plugin.Application.Events.API;

namespace Plugin.Application.CapabilityModel.API
{
    /// Represents a Resource Collection in a REST service. Resource collections can exist on root- or intermediate levels in a resource model
    /// and can contain operations as well as other resources as children.
    /// At root-level, a Resource Collection can be empty, which means that it does not appear as part of the URI. However, in the UML model,
    /// it must ALWAYS exist as a model element! Empty collections are identified by a reserved name "[Empty]". The use of angle brackets 
    /// facilitates introduction of multiple reserved names in the future.
    /// Example: "https://api.enexis.nl/customers/v1/{CustomerID} 
    /// In model: Interface = "Customers" (version = 1.0) --> Collection "[Empty]" --> Path "/{CustomerID}" --> ResourceRepresentation "CustomerDetails"
    /// Empty Resource Collections ONLY exist at root-level and there can be at most ONE per API!
    internal class RESTResourceCapabilityImp: CapabilityImp
    {
        // Names of attributes to be used for external documentation descriptor. These MUST be a case-insensitive match of the
        // associated OpenAPI property names!
        internal const string _DescriptionClassifier                = "TextType";       // Classifier for Terms of Service.
        internal const string _URLClassifier                        = "IdentifierType"; // Classifier for names.
        internal const string _DescriptionAttribute                 = "Description";    // Attribute name for License.Name
        internal const string _URLAttribute                         = "URL";            // Attribute name for License.URL

        // Configuration properties used by this module:
        private const string _BusinessComponentStereotype           = "BusinessComponentStereotype";
        private const string _ResourceCollectionPkgStereotype       = "ResourceCollectionPkgStereotype";
        private const string _ResourceClassStereotype               = "ResourceClassStereotype";
        private const string _RESTOperationClassStereotype          = "RESTOperationClassStereotype";
        private const string _RESTOperationResultStereotype         = "RESTOperationResultStereotype";
        private const string _RESTParameterStereotype               = "RESTParameterStereotype";
        private const string _RESTOperationPkgStereotype            = "RESTOperationPkgStereotype";
        private const string _DocumentationTypeClassName            = "DocumentationTypeClassName";
        private const string _CoreDataTypesPathName                 = "CoreDataTypesPathName";
        private const string _MessageAssemblyClassStereotype        = "MessageAssemblyClassStereotype";
        private const string _IdentifierResourceRoleName            = "IdentifierResourceRoleName";
        private const string _ArchetypeTag                          = "ArchetypeTag";
        private const string _IsRootLevelTag                        = "IsRootLevelTag";
        private const string _TagNamesTag                           = "TagNamesTag";

        private MEPackage _resourcePackage;                         // The package in which the resource lives.
        private Capability _myParent;                               // Either RESTInterface or other Resource.
        private RESTResourceCapability.ResourceArchetype _archetype;    // Specifies the archtetype of the resource.
        private bool _isCollection;                                 // Indicates whether the resource represents a resource collection.
        private bool _isRootLevel;                                  // Indicates whether the resource is a root-level resourcr ("sub-API").
        private List<string> _tagNames;                             // List of tag names associated with this resource.
        private RESTParameterDeclaration _parameter;                // In case of Identifier resources, this contains the parameter.
        private MEClass _componentClass;                            // In case of Document resources, this is the associated Business Component.

        /// <summary>
        /// Getters and setters of properties for this class:
        /// ResourcePackage = Retrieves the package that stores this resource.
        /// ArcheType = Returns the archetype of the resource.
        /// IsCollection = Returns true if this is a collection-type resource.
        /// IsRootLevel = Returns true if the resource represents a sub-API.
        /// TagNames = Returns list of tag names associated with this resource.
        /// Parameter = Returns the parameter in case of an Identifier resource.
        /// DocumentResources = Returns the list of Document Resources associated with this resource.
        /// BusinessComponent = Returns the associated Business Component in case of Document resource.
        /// </summary>
        internal MEPackage ResourcePackage                          { get { return this._resourcePackage; } }
        internal RESTResourceCapability.ResourceArchetype Archetype { get { return this._archetype; } }
        internal bool IsCollection                                  { get { return this._isCollection; } }
        internal bool IsRootLevel                                   { get { return this._isRootLevel; } }
        internal List<string> TagNames                              { get { return this._tagNames; } }
        internal RESTParameterDeclaration Parameter                 { get { return this._parameter; } }
        internal MEClass BusinessComponent                          { get { return this._componentClass; } }

        /// <summary>
        /// Creates a new resource based on a resource declaration object. This object contains all the information necessary to create 
        /// the associated model elements and attributes.
        /// </summary>
        /// <param name="parentInterface">Interface capability that acts as parent for the resource.</param>
        /// <param name="resource">Resource declaration object, created by user and containing all necessary information.</param>
        internal RESTResourceCapabilityImp(RESTInterfaceCapability parentInterface, RESTResourceDeclaration resource) : base(parentInterface.RootService)
        {
            try
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp (interface, declaration) >> Creating new resource '" + 
                                 parentInterface.Name + "." + resource.Name + "'...");
                this._myParent = parentInterface;
                this._archetype = resource.Archetype;
                this._isCollection = (this._archetype == RESTResourceCapability.ResourceArchetype.Collection ||
                                      this._archetype == RESTResourceCapability.ResourceArchetype.Store);
                this._isRootLevel = true;
                this._componentClass = null;

                ConstructCapability(parentInterface, resource); // Performs most of the work.
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp (interface, declaration) >> Error creating resource collection because: " + exc.Message);
                this._capabilityClass = null;   // Assures that instance is declared invalid.
            }
        }

        /// <summary>
        /// Creates a new resource based on an resource declaration object. This object contains all the information necessary to create 
        /// the associated model elements and attributes.
        /// </summary>
        /// <param name="parent">Path expression that acts as parent for the operation.</param>
        /// <param name="resource">Resource declaration object, created by user and containing all necessary information.</param>
        internal RESTResourceCapabilityImp(RESTResourceCapability parentResource, RESTResourceDeclaration resource) : base(parentResource.RootService)
        {
            try
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp (resource, declaration) >> Creating new resource '" +
                                 parentResource.Name + "." + resource.Name + "'...");
                this._myParent = parentResource;
                this._archetype = resource.Archetype;
                this._isCollection = (this._archetype == RESTResourceCapability.ResourceArchetype.Collection ||
                                      this._archetype == RESTResourceCapability.ResourceArchetype.Store);
                this._isRootLevel = false;
                this._componentClass = null;

                ConstructCapability(parentResource, resource); // Performs most of the work.
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp (resource, declaration) >> Error creating resource collection because: " + exc.Message);
                this._capabilityClass = null;   // Assures that instance is declared invalid.
            }
        }

        /// <summary>
        /// Generic constructor to be used for existing class models. The constructor initialises a root resource, that is, a resource that
        /// is directly linked to an Interface.
        /// </summary>
        /// <param name="myInterface">The interface for which we create the resource.</param>
        /// <param name="hierarchy">Class hierarchy consisting of Resource- and associated Path- and Operation objects.</param>
        internal RESTResourceCapabilityImp(RESTInterfaceCapability myInterface, TreeNode<MEClass> hierarchy): base(myInterface.RootService)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp (existing itf) >> Creating new instance '" +
                             myInterface.Name + "." + hierarchy.Data.Name + "'...");
            InitializeCapability(myInterface, hierarchy);
        }

        /// <summary>
        /// Generic constructor to be used for existing class models. The constructor initialises local context and creates the subordinate messages.
        /// </summary>
        /// <param name="parentResource">The PathExpression that is our parent.</param>
        /// <param name="hierarchy">Class hierarchy consisting of Path, Collection, Operation, etc. objects.</param>
        internal RESTResourceCapabilityImp(RESTResourceCapability parentResource, TreeNode<MEClass> hierarchy) : base(parentResource.RootService)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp (existing resource) >> Creating new instance '" +
                             parentResource.Name + "." + hierarchy.Data.Name + "'...");
            InitializeCapability(parentResource, hierarchy);
        }

        /// <summary>
        /// This method creates a new association between the provided parent capability and the current resource. The parent MUST be
        /// either a RESTInterface or another Resource. If a wrong parent is specified, the method does not perform any operations!
        /// The method is NOT directly provided by the Resource Capability interface and is only used for the implementation of 
        /// "shared resource" constructors.
        /// </summary>
        /// <param name="parent">New parent capability.</param>
        internal void AssignParent(Capability parent)
        {
            if (parent is RESTInterfaceCapability || parent is RESTResourceCapability)
            {
                var parentEndpoint = new EndpointDescriptor(parent.CapabilityClass, "1", parent.Name, null, false);
                var resourceEndpoint = new EndpointDescriptor(this._capabilityClass, "1", this._assignedRole, null, true);
                MEAssociation link = ModelSlt.GetModelSlt().CreateAssociation(parentEndpoint, resourceEndpoint, MEAssociation.AssociationType.MessageAssociation);
                InitialiseParent(parent);
            }
            else Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.AssignParent >> Provided parent '" + parent.Name + "' is of wrong type!");
        }

        /// <summary>
        /// Create- and add a new operation to the current resource. The operation is specified using its Operation Declaration object 
        /// and we do not check whether the operation is indeed unique, this should have been done during creation (e.g. in user interface).
        /// It is the responsibility of the operation to register with the parent capability.
        /// </summary>
        /// <param name="operation">New operation that must be added.</param>
        /// <param name="newMinorVersion">True when a new minor version must be created. Parameter is ignored when CM is active!</param>
        /// <returns>True if operation completed successfully, false on errors.</returns>
        internal bool AddOperation(RESTOperationDeclaration operation, bool newMinorVersion)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.AddOperation >> Adding operation '" + operation.Name + "'...");
            bool result = true;
            try
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                string newNames = string.Empty;
                var interfaceCapItf = new RESTResourceCapability(this);
                var operationCapability = new RESTOperationCapability(interfaceCapItf, operation);
                if (operationCapability.Valid)
                {
                    operationCapability.InitialiseParent(interfaceCapItf);
                    string roleName = RESTUtil.GetAssignedRoleName(operationCapability.Name);
                }
                else
                {
                    Logger.WriteWarning("Failed to create operation!");
                    result = false;
                }

                // This will update the service version, followed by all child capabilities!
                // But the operation is executed ONLY when configuration management is disabled (with CM enabled, versions are
                // managed differently).
                if (!this._rootService.UseConfigurationMgmt && newMinorVersion) RootService.IncrementVersion();

                string logMessage = "Added Operation: '" + operationCapability.Name + "'";
                RootService.CreateLogEntry(logMessage + " to parent Resource '" + Name + "'.");
                CreateLogEntry(logMessage + ".");
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.AddOperation >> Exception caught: " + exc.Message);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Add one or more child resources to the current resource. We do not check whether the names are indeed unique, 
        /// this should have been done during creation (e.g. in user interface).
        /// We don't attempt to filter on resource types since we might have valid reason to add each type of child resource
        /// to the parent resource (not just collections).
        /// It is the responsibility of the resource to register with the parent capability tree.
        /// </summary>
        /// <param name="resources">List of resources that must be added.</param>
        /// <param name="newMinorVersion">True when a new minor version must be created. Parameter is ignored when CM is active!</param>
        /// <returns>True if operation completed successfully, false on errors.</returns>
        internal bool AddResources(List<RESTResourceDeclaration> resources, bool newMinorVersion)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.AddResources >> Adding new resources...");
            bool result = true;
            try
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                string newNames = string.Empty;
                var parentResource = new RESTResourceCapability(this);
                bool isFirst = true;
                foreach (RESTResourceDeclaration resource in resources)
                {
                    RESTResourceCapability resourceCapability;
                    bool existingDocument = false;
                    if (resource.Archetype == RESTResourceCapability.ResourceArchetype.Document)
                    {
                        resourceCapability = ((RESTService)RootService).FindDocumentResource(resource.Name);
                        if (resourceCapability != null)
                        {
                            // Existing capability. In this case, we have to explicitly create an association with that resource 
                            // Normally, this is taken care of by the child resource constructor but since we're not going to invoke this,
                            // we have to create one manually.
                            // First of all, we check whether we already have an association with this resource...
                            foreach (Capability child in GetChildren()) if (child == resourceCapability) return true;
                            string roleName = RESTUtil.GetAssignedRoleName(resource.DocumentClass.Name);
                            var myEndpoint = new EndpointDescriptor(this._capabilityClass, "1", this._capabilityClass.Name, null, false);
                            var documentEndpoint = new EndpointDescriptor(resourceCapability.CapabilityClass, "1", roleName, null, true);
                            ModelSlt.GetModelSlt().CreateAssociation(myEndpoint, documentEndpoint, MEAssociation.AssociationType.MessageAssociation);
                            resourceCapability.InitialiseParent(parentResource);
                            newNames += (!isFirst) ? ", " + roleName : roleName;
                            isFirst = false;
                            existingDocument = true;
                        }
                    }

                    if (!existingDocument)    // This holds for non-document resources or for new document resources.
                    {
                        resourceCapability = new RESTResourceCapability(parentResource, resource);
                        if (resourceCapability.Valid)
                        {
                            resourceCapability.InitialiseParent(parentResource);
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
                }

                if (result)
                {
                    // This will update the service version, followed by all child capabilities!
                    // But the operation is executed ONLY when configuration management is disabled (with CM enabled, versions are
                    // managed differently).
                    if (!this._rootService.UseConfigurationMgmt && newMinorVersion) RootService.IncrementVersion();

                    string logMessage = "Added child Resource(s): '" + newNames + "'";
                    RootService.CreateLogEntry(logMessage + " to parent Resource '" + Name + "'.");
                    CreateLogEntry(logMessage + ".");
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.AddResources >> Exception caught: " + exc.Message);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Overrides the default Capability.delete in order to assure that subordinate capabilities are deleted as well as the collection package.
        /// On return, all resource collection resources, including the package tree, are deleted and the Capability is INVALID.
        /// </summary>
        internal override void Delete()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.Delete >> Deleting the resource collection and all associated resources...");

            if (this._archetype == RESTResourceCapability.ResourceArchetype.Document)
            {
                Logger.WriteWarning("Deleted Document Resource '" + this.Name + "', please verify integrity!");
            }

            this._myParent.RemoveChild(new RESTResourceCapability(this));     // Unlink this resource from my parent.
            base.Delete();                                                    // Deletes all resources owned by this capability (Operations, Child Resources).
            if (this._isCollection) this._resourcePackage.Parent.DeletePackage(this._resourcePackage); // Finally, remove my package.
        }

        /// <summary>
        /// Deletes the operation specified by the Operation Declaration from the Resource. The function silently fails if the operation
        /// could not be found.
        /// </summary>
        /// <param name="operation">Operation to be deleted.</param>
        /// <param name="newMinorVersion">True when a new minor version must be created. Parameter is ignored when CM is active!</param>
        internal void DeleteOperation(MEClass operationClass, bool newMinorVersion)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.DeleteOperation >> Going to delete operation '" + 
                             operationClass.Name + "' from interface '" + Name + "'...");
            bool foundIt = false;
            foreach (Capability cap in GetChildren())
            {
                if (cap is RESTOperationCapability && cap.CapabilityClass == operationClass)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.DeleteResource >> Found the resource!");
                    DeleteChild(cap.Implementation, true);
                    foundIt = true;
                    break;
                }
            }

            if (foundIt)
            {
                // This will update the service version, followed by all child capabilities!
                // But the operation is executed ONLY when configuration management is disabled (with CM enabled, versions are
                // managed differently).
                if (!this._rootService.UseConfigurationMgmt && newMinorVersion) RootService.IncrementVersion();

                string logMessage = "Deleted Operation: '" + operationClass.Name + "'";
                RootService.CreateLogEntry(logMessage + " from parent Resource '" + Name + "'.");
                CreateLogEntry(logMessage + ".");
            }
        }

        /// <summary>
        /// Deletes the resource identified by the specified resource-class object. This will delete the entire resource hierarchy.
        /// </summary>
        /// <param name="resourceClass">Identifies the resource to be deleted.</param>
        /// <param name="newMinorVersion">Set to 'true' when minor version must be updated, 'false' to keep existing version.
        /// Parameter is ignored when CM is active!</param>
        internal void DeleteResource(MEClass resourceClass, bool newMinorVersion)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.DeleteResource >> Going to delete resource '" + resourceClass.Name + "' from interface '" + Name + "'...");
            bool foundIt = false;
            foreach (Capability cap in GetChildren())
            {
                if (cap is RESTResourceCapability && cap.CapabilityClass == resourceClass)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.DeleteResource >> Found the resource!");
                    DeleteChild(cap.Implementation, true);
                    break;
                }
            }

            if (foundIt)
            {
                // This will update the service version, followed by all child capabilities!
                // But the operation is executed ONLY when configuration management is disabled (with CM enabled, versions are
                // managed differently).
                if (!this._rootService.UseConfigurationMgmt && newMinorVersion) RootService.IncrementVersion();

                string logMessage = "Deleted child Resource: '" + resourceClass.Name + "'";
                RootService.CreateLogEntry(logMessage + " from parent Resource '" + Name + "'.");
                CreateLogEntry(logMessage + ".");
            }
        }

        /// <summary>
        /// This method is invoked when the user has made one or more changes to a Resource Capability. The method receives an
        /// REsource Declaration object that contains the (updated) information for the Resource. The method updates metadata and
        /// associations where appropriate.
        /// </summary>
        /// <param name="resource">Updated Resource properties.</param>
        /// <param name="newMinorVersion">Set to true to force update of API minor version. Parameter is ignored when CM is active!</param>
        /// <returns>True on successfull completion, false on errors.</returns>
        internal bool Edit(RESTResourceDeclaration resource, bool newMinorVersion)
        {
            if (resource.Status == RESTResourceDeclaration.DeclarationStatus.Edited)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.Edit >> Editing '" + resource.Name + "'...");
                ContextSlt context = ContextSlt.GetContextSlt();
                ModelSlt model = ModelSlt.GetModelSlt();

                // Check whether our type has changed...
                // Since the dialog does not allow structural type changes (e.g. from Collection to Document), we only have to update the type
                // itself, no other changes are allowed.
                if (this._archetype != resource.Archetype)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.Edit >> Changed archetype from '" +
                                     this._archetype + "' to '" + resource.Archetype + "'!");
                    this._archetype = resource.Archetype;
                    this._capabilityClass.SetTag(context.GetConfigProperty(_ArchetypeTag), 
                                                 EnumConversions<RESTResourceCapability.ResourceArchetype>.EnumToString(resource.Archetype));
                }

                // Check whether our name has changed...
                if (this.Name != resource.Name)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTREsourceCapabilityImp.Edit >> Changed name from '" +
                                     this.Name + "' to '" + resource.Name + "'!");
                    if (this._archetype == RESTResourceCapability.ResourceArchetype.Collection ||
                        this._archetype == RESTResourceCapability.ResourceArchetype.Store)
                    {
                        // These resources have their own package, which now requires a name update as well...
                        if (this._capabilityClass.OwningPackage.Parent.FindPackage(resource.Name, context.GetConfigProperty(_ResourceCollectionPkgStereotype)) == null)
                        {
                            this._capabilityClass.OwningPackage.Name = resource.Name;
                        }
                        else
                        {
                            Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.Edit >> Resource rename from '" +
                                               this.Name + "' to '" + resource.Name + "' failed: name already in use!");
                            return false;
                        }
                    }
                    else if (this._archetype == RESTResourceCapability.ResourceArchetype.Document)
                    {
                        ((RESTService)this._rootService).DeleteDocumentResource(this.Name);
                    }

                    // Update class- and role names (but change role only if we're not an Identifier)...
                    if (this._archetype != RESTResourceCapability.ResourceArchetype.Identifier)
                    {
                        foreach (MEAssociation assoc in Parent.CapabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
                        {
                            if (assoc.Destination.EndPoint == this.CapabilityClass)
                            {
                                assoc.SetName(RESTUtil.GetAssignedRoleName(resource.Name), MEAssociation.AssociationEnd.Destination);
                                break;
                            }
                        }
                        this._assignedRole = resource.Name;
                    }
                    this._capabilityClass.Name = resource.Name;
                    if (this._archetype == RESTResourceCapability.ResourceArchetype.Document)
                        ((RESTService)this._rootService).RegisterDocument(new RESTResourceCapability(this));
                }

                // (Re-)Load documentation...
                if (!string.IsNullOrEmpty(resource.Description)) MEChangeLog.SetRTFDocumentation(this._capabilityClass, resource.Description);
                string extDocClassName = context.GetConfigProperty(_DocumentationTypeClassName);
                MEAssociation extDocAssoc = null;
                foreach (MEAssociation assoc in this._capabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
                {
                    if (assoc.Destination.EndPoint.Name == extDocClassName)
                    {
                        extDocAssoc = assoc;
                        break;
                    }
                }

                // If we have (new) external documentation, we use a brute-force mechanism for updates: simply delete the class if it
                // already exists and re-create from scratch. This is probably more efficient then spending a lot of time figuring out
                // what we already have and what we have to update.
                // If we dont't have external documentation but we have found a class, the class is deleted.
                if (!string.IsNullOrEmpty(resource.ExternalDocDescription ) || !string.IsNullOrEmpty(resource.ExternalDocURL))
                {
                    if (extDocAssoc != null) this._myParent.OwningPackage.DeleteClass(extDocAssoc.Destination.EndPoint);
                    CreateExternalDocumentation(this._myParent, resource);
                    
                }
                else if (extDocAssoc != null) this._myParent.OwningPackage.DeleteClass(extDocAssoc.Destination.EndPoint);

                // Update the Tag names (if any)...
                AssignTagNames(resource);

                // If I'm a Document check whether we must update the Document Class association...
                // And if I'm an Identifier, check whether we must update the Identifier properties...
                if (this._archetype == RESTResourceCapability.ResourceArchetype.Document) AssignBusinessDocument(resource);
                else if (this._archetype == RESTResourceCapability.ResourceArchetype.Identifier) AssignIdentifier(resource);

                // Next, we're going to check whether operations have changed. We can either add new operations, remove existing
                // ones or edit operation properties. 
                // When looking for associated operation objects, we check against the MEClass objects to make sure we have the
                // correct operation (names might have changed during edit). Since we are editing operations, the MEClass object
                // MUST be present (with the exception of Add Operation of course)...
                foreach (RESTOperationDeclaration operation in resource.Operations)
                {
                    if (operation.Status == RESTOperationDeclaration.DeclarationStatus.Deleted)
                    {
                        foreach (Capability child in GetChildren())
                        {
                            if (child is RESTOperationCapability && child.CapabilityClass == operation.OperationClass)
                            {
                                DeleteOperation(child.CapabilityClass, false);
                                break;
                            }
                        }
                    }
                    else if (operation.Status == RESTOperationDeclaration.DeclarationStatus.Edited)
                    {
                        foreach (Capability child in GetChildren())
                        {
                            if (child is RESTOperationCapability && child.CapabilityClass == operation.OperationClass)
                            {
                                ((RESTOperationCapability)child).Edit(operation, false);
                                break;
                            }
                        }
                    }
                    else if (operation.Status == RESTOperationDeclaration.DeclarationStatus.Created) AddOperation(operation, false);
                }

                // This will update the service version, followed by all child capabilities!
                // But the operation is executed ONLY when configuration management is disabled (with CM enabled, versions are
                // managed differently).
                if (!this._rootService.UseConfigurationMgmt && newMinorVersion) RootService.IncrementVersion();

                CreateLogEntry("Operation has been edited.");
            }
            return true;
        }

        /// <summary>
        /// Resource capabilities can not be saved in files, so this function returns an empty string.
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
            return "REST Resource [" + this._archetype.ToString() + "]";
        }

        /// <summary>
        /// Creates an Interface object that matches the current Implementation.
        /// </summary>
        /// <returns>Interface object.</returns>
        internal override Capability GetInterface() { return new RESTResourceCapability(this); }

        /// <summary>
        /// Process the capability (i.e. generate output according to provided processor.).
        /// </summary>
        /// <param name="stage">The processing stage we're currently in, passed verbatim to processor.</param>
        /// <returns>True when processing can commence, false on errors.</returns>
        internal override bool HandleCapability(CapabilityProcessor processor, ProcessingStage stage)
        {
            // Since all the actual work is being performed by the processor, simply pass information onwards...
            return processor.ProcessCapability(new RESTResourceCapability(this), stage);
        }

        /// <summary>
        /// This function checks whether this resource has at least one associated operation.
        /// </summary>
        /// <returns>True if the resource contains at least one operation.</returns>
        internal bool HasOperations()
        {
            foreach (Capability child in GetChildren()) if (child is RESTOperationCapability) return true;
            return false;
        }

        /// <summary>
        /// This method is called whenever a (new) parent of the Capability has taken ownership of the Capability. 
        /// If this parent is an Interface or a PathExpression, we have to register the current instance with the parent.
        /// In all other cases, nothing will happen here!
        /// </summary>
        /// <param name="parent">The parent Capability that has taken ownership of this Capability.</param>
        internal override void InitialiseParent(Capability parent)
        {
            if (parent is RESTInterfaceCapability || parent is RESTResourceCapability)
            {
                this._myParent = parent;
                parent.AddChild(new RESTResourceCapability(this));
            }
        }

        /// <summary>
        /// Renames the resource identified by the specified resource-class object.
        /// </summary>
        /// <param name="resourceClass">Resource to be renamed.</param>
        /// <param name="oldName">Original name of the Resource.</param>
        /// <param name="newName">New name for the Resource, in PascalCase.</param>
        /// <param name="newMinorVersion">Set to 'true' when minor version must be updated, 'false' to keep existing version.
        /// Parameter is ignored when CM is active!</param>
        internal void RenameResource(MEClass resourceClass, string oldName, string newName, bool newMinorVersion)
        {
            /**************************************
             * OBVIOUSLY, THIS CODE IS NOT YET CORRECT!!!
             * ***********************************/
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.renameResource >> Going to rename resource '" + resourceClass.Name + "' to: '" + newName + "'...");
            if (string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName) || oldName == newName) return;     // Nothing to rename!

            foreach (Capability cap in GetChildren())
            {
                if (cap.CapabilityClass == resourceClass)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.RenameResource >> Found the resource!");

                    // First of all, we attempt to locate the association between this capability and the resource to be renamed so we can rename the role...
                    foreach (MEAssociation association in this._capabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
                    {
                        if (association.Destination.EndPoint == cap.CapabilityClass)
                        {
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.RenameResource >> Found child association...");
                            association.SetName(RESTUtil.GetAssignedRoleName(Conversions.ToCamelCase(newName)), MEAssociation.AssociationEnd.Destination);
                            break;
                        }
                    }

                    if (cap.Name == oldName)    // Perform operation rename only if there is something to rename...
                    {
                        cap.Rename(newName);
                        if (!this._rootService.UseConfigurationMgmt && newMinorVersion) // If we have to increment the minor version, do so BEFORE creating the log message...
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

            string logMessage = "Renamed child Resource: '" + oldName + "' to: '" + resourceClass.Name + "'";
            RootService.CreateLogEntry(logMessage + " in parent Resource '" + Name + "'.");
            CreateLogEntry(logMessage + ".");
        }

        /// <summary>
        /// Facilitates iteration over the set of child resources associated with this parent resource. If the type
        /// is not equal to 'Unknown', the method only returns children of archetype identified by 'type'.
        /// </summary>
        /// <returns>Resource Capability enumerator.</returns>
        internal List<RESTResourceCapability> ResourceList(RESTResourceCapability.ResourceArchetype type)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.ResourceList >> Retrieving resources of type '" + type + "'...");
            List<RESTResourceCapability> children = new List<RESTResourceCapability>();
            foreach (Capability cap in GetChildren())
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.ResourceList >> Inspecting '" + cap.Name + "'...");
                if (cap is RESTResourceCapability &&
                    (type == RESTResourceCapability.ResourceArchetype.Unknown || ((RESTResourceCapability)cap).Archetype == type))
                    children.Add((RESTResourceCapability)cap);
            }
            return children;
        }

        /// <summary>
        /// Overrides the 'rename' operation for a Resource. In this particular case, we have to rename both
        /// the class (taken care of by invoking base.rename) as well as the collection package (when applicable).
        /// </summary>
        /// <param name="newName">New name to be assigned to the resource collection</param>
        internal override void Rename(string newName)
        {
            base.Rename(newName);                               // Takes care of renaming the class.
            if (this._isCollection) this._resourcePackage.Name = newName; // And this renames the package in case we are a collection-type resource.
        }

        /// <summary>
        // If we're a Document Resource, create the association with the Business Document and register this class as a document.
        // We use the resource name as basis for the role name. This assures that we get the proper role in case the Business Document
        // uses an Alias name (that would already have been incorporated in the resource name at moment of assignment in the user dialog).
        // If an association already exists, this is removed and replaced by a new association.
        /// </summary>
        /// <param name="properties">Resource properties.</param>
        private void AssignBusinessDocument(RESTResourceDeclaration properties)
        {
            ModelSlt model = ModelSlt.GetModelSlt();

            if (this._componentClass != null && properties.DocumentClass != null)
            {
                if (this._componentClass == properties.DocumentClass) return; // Business Component classes match, no action required.
                
                // No match, locate the Business Component association and remove it...
                foreach (MEAssociation assoc in this._capabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
                {
                    if (assoc.Destination.EndPoint == this._componentClass)
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.AssignBusinessDocument >> New association, deleting existing one...");
                        this._capabilityClass.DeleteAssociation(assoc);
                        this._componentClass = null;
                        break;
                    }
                }
            }

            if (properties.DocumentClass != null)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.AssignBusinessDocument >> Associate with Business Component '" + 
                                 properties.DocumentClass.Name + "'...");
                string role = properties.Name.EndsWith("Type") ? properties.Name.Substring(0, properties.Name.IndexOf("Type")) : properties.Name;
                var componentEndpoint = new EndpointDescriptor(properties.DocumentClass, "1", role, null, true);
                var resourceEndpoint = new EndpointDescriptor(this._capabilityClass, "1", this._assignedRole, null, true);
                model.CreateAssociation(resourceEndpoint, componentEndpoint, MEAssociation.AssociationType.MessageAssociation);
                this._componentClass = properties.DocumentClass;
            }
        }

        /// <summary>
        /// Helper method that updates the Identifier attribute in case of an Identifier resource.
        /// </summary>
        /// <param name="properties">Resource properties.</param>
        private void AssignIdentifier(RESTResourceDeclaration properties)
        {
            RESTParameterDeclaration param = properties.Parameter;
            bool validParam = (param != null && param.Status != RESTParameterDeclaration.DeclarationStatus.Deleted &&
                                                param.Status != RESTParameterDeclaration.DeclarationStatus.Invalid);
            bool changedParam = (param != null && (this._parameter.Name != param.Name || this._parameter.Classifier != param.Classifier));
            if (!validParam)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.AssignIdentifier >> Identifier Resource '" + Name + "' has no valid Identifier!");
            }
            if (this._parameter != null && (!validParam || changedParam))
            {
                // Parameter has changed or disappeared, remove existing attribute!
                // Note that the capability becomes invalid if there is no valid parameter association!
                MEAttribute oldAttrib = this._capabilityClass.FindAttribute(this._parameter.Name);
                if (oldAttrib != null) this._capabilityClass.DeleteAttribute(oldAttrib);
                this._parameter = null;
            }
            if (validParam && changedParam)
            {
                RESTParameterDeclaration.ConvertToAttribute(this._capabilityClass, param);
                this._parameter = param;
            }
        }

        /// <summary>
        /// Helper method that either creates or updates the set of tags assigned to this resource.
        /// </summary>
        /// <param name="properties">Resource properties.</param>
        private void AssignTagNames(RESTResourceDeclaration properties)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            if (properties.TagNames.Count > 0)
            {
                string tagList = string.Empty;
                bool isFirst = true;
                foreach (string tagName in properties.TagNames)
                {
                    tagList += isFirst ? tagName : "," + tagName;
                    isFirst = false;
                }
                this._capabilityClass.SetTag(context.GetConfigProperty(_TagNamesTag), tagList);
            }
            else this._capabilityClass.SetTag(context.GetConfigProperty(_TagNamesTag), string.Empty);
            this._tagNames = properties.TagNames;
        }

        /// <summary>
        /// Helper method that constructs a new Resource class from a Resource Declaration object.
        /// It creates the class (optionally in its own package), initialises class attributes and tags and creates a diagram for the class
        /// (if the resource is of the correct type). Next, it continues to create associated operations and child resources.
        /// </summary>
        /// <param name="parent">Parent capability that 'owns' this resource.</param>
        /// <param name="resource">Resource declaration object.</param>
        private void ConstructCapability(Capability parent, RESTResourceDeclaration resource)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();

            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.ConstructCapability >> Constructing '" + 
                             parent.Name + "." + resource.Name + "'..."); 

            // Collection resources live in their own packages...
            if (this._isCollection)
            {
                // Create a Resource Collection package underneath the Service Model package and create the Collection class as a member.
                this._resourcePackage = this._rootService.ModelPkg.CreatePackage(resource.Name, context.GetConfigProperty(_ResourceCollectionPkgStereotype));
                this._capabilityClass = this._resourcePackage.CreateClass(resource.Name, context.GetConfigProperty(_ResourceClassStereotype));
            }
            else
            {
                // Otherwise, the resource lives in the package of its parent.
                this._capabilityClass = parent.OwningPackage.CreateClass(resource.Name, context.GetConfigProperty(_ResourceClassStereotype));
            }

            if (this._capabilityClass != null)
            {
                this._capabilityClass.Version = new Tuple<int, int>(parent.RootService.MajorVersion, 0);
                if (this._archetype == RESTResourceCapability.ResourceArchetype.Identifier)
                    this._assignedRole = context.GetConfigProperty(_IdentifierResourceRoleName);
                else this._assignedRole = RESTUtil.GetAssignedRoleName(resource.Name);
                this._capabilityClass.SetTag(context.GetConfigProperty(_ArchetypeTag), this._archetype.ToString());
                if (!string.IsNullOrEmpty(resource.Description)) MEChangeLog.SetRTFDocumentation(this._capabilityClass, resource.Description);
                this._capabilityClass.SetTag(context.GetConfigProperty(_IsRootLevelTag), this._isRootLevel.ToString());

                // Assign tags (if any)...
                AssignTagNames(resource);

                // Establish link with our Parent...
                var parentEndpoint = new EndpointDescriptor(parent.CapabilityClass, "1", parent.Name, null, false);
                var resourceEndpoint = new EndpointDescriptor(this._capabilityClass, "1", this._assignedRole, null, true);
                MEAssociation link = model.CreateAssociation(parentEndpoint, resourceEndpoint, MEAssociation.AssociationType.MessageAssociation);
                InitialiseParent(parent);

                // Create the ExternalDocument class if we have defined entries for it...
                CreateExternalDocumentation(parent, resource);

                // Create the operations that are associated with this Resource.
                // If we create a Resource Collection as a child of another Resource that has Documents assigned, these Documents could have been
                // selected by the Operations of the new Resource. In that case, we have to associate with these Documents in order to keep
                // things consistent...
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.ConstructCapability >> Creating operations...");
                var resourceCapItf = new RESTResourceCapability(this);
                var documentList = new List<RESTResourceDeclaration>();
                foreach (RESTOperationDeclaration operation in resource.Operations)
                {
                    if (operation.Status != RESTOperationDeclaration.DeclarationStatus.Deleted &&
                        operation.Status != RESTOperationDeclaration.DeclarationStatus.Invalid)
                    {
                        RESTOperationCapability newOperation = new RESTOperationCapability(resourceCapItf, operation);
                        if (operation.RequestDocument != null) documentList.Add(new RESTResourceDeclaration(operation.RequestDocument));
                        if (operation.ResponseDocument != null) documentList.Add(new RESTResourceDeclaration(operation.ResponseDocument));
                    }
                }
                if (documentList.Count > 0) AddResources(documentList, false); // This will create association with existing Documents.

                // Check whether we must associate with a Business Document class...
                AssignBusinessDocument(resource);

                // And create the child resources that are associated with this Resource...
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.ConstructCapability >> Creating child resources...");
                foreach (RESTResourceDeclaration childResource in resource.ChildResources)
                {
                    if (childResource.Status != RESTResourceDeclaration.DeclarationStatus.Deleted &&
                        childResource.Status != RESTResourceDeclaration.DeclarationStatus.Invalid)
                    {
                        RESTResourceCapability newResource = new RESTResourceCapability(resourceCapItf, childResource);
                    }
                }

                if (this.IsCollection)
                {
                    // Collection resources also have their own diagram.
                    // Create the diagram, add the Interface and the collection to that diagram and collect all child objects that should
                    // also be shown.
                    Diagram myDiagram = this._resourcePackage.CreateDiagram();
                    DiagramItemsCollector collector = new DiagramItemsCollector(myDiagram);
                    this._capabilityClass.AssociatedDiagram = myDiagram;
                    collector.DiagramClassList.Add(parent.CapabilityClass);
                    collector.DiagramAssociationList.Add(link);
                    Traverse(collector.Collect);

                    myDiagram.AddDiagramProperties();
                    myDiagram.ShowConnectorStereotypes(false);
                    myDiagram.AddClassList(collector.DiagramClassList);
                    myDiagram.AddAssociationList(collector.DiagramAssociationList);
                    myDiagram.Show();
                }
                else if (this._archetype == RESTResourceCapability.ResourceArchetype.Identifier)
                {
                    // Convert the parameter to an attribute of the Resource class...
                    RESTParameterDeclaration param = resource.Parameter;
                    if (param.Classifier is MEDataType &&
                        param.Status != RESTParameterDeclaration.DeclarationStatus.Deleted &&
                        param.Status != RESTParameterDeclaration.DeclarationStatus.Invalid)
                    {
                        RESTParameterDeclaration.ConvertToAttribute(this._capabilityClass, param);
                    }
                }
                CreateLogEntry("Initial release.");
            }
            else Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.ConstructCapability >> Unable to create Capability Class '" + 
                                    resource.Name + "' in parent '" + parent.Name + "'!");
        }

        /// <summary>
        /// Helper method that creates the external documentation class for this resource and creates an association with it.
        /// </summary>
        /// <param name="parent">My resource parent.</param>
        /// <param name="resource">Resource declaration properties.</param>
        private void CreateExternalDocumentation(Capability parent, RESTResourceDeclaration resource)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();

            if (!string.IsNullOrEmpty(resource.ExternalDocDescription) || !string.IsNullOrEmpty(resource.ExternalDocURL))
            {
                string coreDataTypesPath = context.GetConfigProperty(_CoreDataTypesPathName);
                MEDataType descriptionType = model.FindDataType(coreDataTypesPath, _DescriptionClassifier);
                MEDataType identifierType = model.FindDataType(coreDataTypesPath, _URLClassifier);
                var mandatory = new Tuple<int, int>(1, 1);
                MEClass docClass = parent.OwningPackage.CreateClass(context.GetConfigProperty(_DocumentationTypeClassName),
                                                                    context.GetConfigProperty(_BusinessComponentStereotype));
                if (docClass != null)
                {
                    if (!string.IsNullOrEmpty(resource.ExternalDocDescription))
                        docClass.CreateAttribute(_DescriptionAttribute, descriptionType, AttributeType.Attribute, resource.ExternalDocDescription, mandatory, true);
                    if (!string.IsNullOrEmpty(resource.ExternalDocURL))
                        docClass.CreateAttribute(_URLAttribute, identifierType, AttributeType.Attribute, resource.ExternalDocURL, mandatory, true);

                    string role = docClass.Name.EndsWith("Type") ? docClass.Name.Substring(0, docClass.Name.IndexOf("Type")) : docClass.Name;
                    var docEndpoint = new EndpointDescriptor(docClass, "1", role, null, true);
                    var resourceEndpoint = new EndpointDescriptor(this._capabilityClass, "1", this._assignedRole, null, true);
                    model.CreateAssociation(resourceEndpoint, docEndpoint, MEAssociation.AssociationType.MessageAssociation);
                }
                else Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.CreateExternalDocumentation >> Unable to find Contact Class '" +
                                       context.GetConfigProperty(_DocumentationTypeClassName) + "/" + context.GetConfigProperty(_BusinessComponentStereotype));
            }
        }

        /// <summary>
        /// Initializes an existing Resource capability. 
        /// </summary>
        /// <param name="parent">Parent can be either an Interface or an Identifier.</param>
        /// <param name="hierarchy">Starting from our Capability, the hierarchy contains child Identifiers, Documents or Operations.</param>
        private void InitializeCapability(Capability parent, TreeNode<MEClass> hierarchy)
        {
            try
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.InitializeCapability >> Creating new instance '" +
                                 parent.Name + "." + hierarchy.Data.Name + "'...");

                ContextSlt context = ContextSlt.GetContextSlt();
                this._myParent = parent;
                this._capabilityClass = hierarchy.Data;
                string archetypeTagName = context.GetConfigProperty(_ArchetypeTag);
                string RESTParamStereotype = context.GetConfigProperty(_RESTParameterStereotype);
                string resourceClassStereotype = context.GetConfigProperty(_ResourceClassStereotype);
                string operationClassStereotype = context.GetConfigProperty(_RESTOperationClassStereotype);
                string businessComponentStereotype = context.GetConfigProperty(_BusinessComponentStereotype);
                var resourceCapItf = new RESTResourceCapability(this);
                this._archetype = (RESTResourceCapability.ResourceArchetype) Enum.Parse(typeof(RESTResourceCapability.ResourceArchetype), 
                                                                                        this._capabilityClass.GetTag(archetypeTagName), true);
                this._isCollection = (this._archetype == RESTResourceCapability.ResourceArchetype.Collection || 
                                      this._archetype == RESTResourceCapability.ResourceArchetype.Store);
                this._resourcePackage = this._isCollection? this.RootService.ModelPkg.FindPackage(this._capabilityClass.Name, context.GetConfigProperty(_ResourceCollectionPkgStereotype)):
                                                            parent.CapabilityClass.OwningPackage;
                this._assignedRole = parent.FindChildClassRole(this._capabilityClass.Name, context.GetConfigProperty(_ResourceClassStereotype));
                this._isRootLevel = (string.Compare(this._capabilityClass.GetTag(context.GetConfigProperty(_IsRootLevelTag)), "true", true) == 0);

                // Retrieve the list of tag names (if any)...
                string tagList = this._capabilityClass.GetTag(context.GetConfigProperty(_TagNamesTag));
                this._tagNames = new List<string>();
                if (!string.IsNullOrEmpty(tagList))
                {
                    string[] tagArray = tagList.Split(',');
                    foreach (string tagName in tagArray) this._tagNames.Add(tagName.Trim());
                    ((RESTService)RootService).RegisterTag(resourceCapItf);
                }

                // Check whether we have a parameter (in case of Identifier Resource)...
                // If the class has multiple RESTParameter attributes, we simply take the first one we encounter (and issue a warning 'cause this is illegal)...
                this._parameter = null;
                foreach (MEAttribute attrib in this._capabilityClass.Attributes)
                {
                    if (attrib.HasStereotype(RESTParamStereotype))
                    {
                        this._parameter = new RESTParameterDeclaration(attrib);
                        if (this._capabilityClass.Attributes.Count > 1)
                            Logger.WriteWarning("Resource '" + this.Name + "' has too many attributes, only '" + this._parameter.Name + "' is used!");
                        break;
                    }
                }

                // Creating all child resources and operations...
                // In order to guarantee that capability processing is handled correctly, we MUST register all operations BEFORE registering
                // child resources. Since the order in which we receive them from the repository is undetermined, we keep a separate list
                // of child resources at hand in which we store all encountered children. Since 'InitialiseParent' adds the resource to the
                // END of the children list, we can now safely initialise all operations first and postpone registration of child resources
                // until we processed all our children...
                // EXCEPTION: If the resource contains child DOCUMENT resources, these must be FIRST, since operations might depend on them.
                // That is why we must use two passes, first one to get all Document Resources, second pass to get all the others...
                // So, the order is: (1) Document resources, (2) Operations and (3) everything else.
                var childResources = new List<RESTResourceCapability>();
                foreach (TreeNode<MEClass> node in hierarchy.Children)
                {
                    if (node.Data.HasStereotype(resourceClassStereotype))
                    {
                        string typeTag = node.Data.GetTag(archetypeTagName);
                        if (!string.IsNullOrEmpty(typeTag) && 
                            EnumConversions<RESTResourceCapability.ResourceArchetype>.StringToEnum(typeTag) == RESTResourceCapability.ResourceArchetype.Document)
                        {
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.InitializeCapability >> Found Document Resource '" + node.Data.Name + "'...");
                            var resource = new RESTResourceCapability(resourceCapItf, node);
                            if (!resource.Valid)
                            {
                                Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.InitializeCapability >> Error creating Document '" + node.Data.Name + "'!");
                                this._capabilityClass = null;
                                return;
                            }
                            resource.InitialiseParent(resourceCapItf);
                        }
                    }
                }

                // Now that we have initialized our child Document Resources, look for all the others...
                foreach (TreeNode<MEClass> node in hierarchy.Children)
                {
                    if (node.Data.HasStereotype(operationClassStereotype))
                    {
                        var operation = new RESTOperationCapability(resourceCapItf, node);
                        operation.InitialiseParent(resourceCapItf);
                        if (!operation.Valid)
                        {
                            Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.InitializeCapability >> Error creating Operation '" + node.Data.Name + "'!");
                            this._capabilityClass = null;
                            return;
                        }
                    }
                    else if (node.Data.HasStereotype(resourceClassStereotype))
                    {
                        string typeTag = node.Data.GetTag(archetypeTagName);
                        if (!string.IsNullOrEmpty(typeTag) &&
                            EnumConversions<RESTResourceCapability.ResourceArchetype>.StringToEnum(typeTag) != RESTResourceCapability.ResourceArchetype.Document)
                        {
                            var resource = new RESTResourceCapability(resourceCapItf, node);
                            if (!resource.Valid)
                            {
                                Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.InitializeCapability >> Error creating Path '" + node.Data.Name + "'!");
                                this._capabilityClass = null;
                                return;
                            }
                            childResources.Add(resource);   // Deferring registration of parent until all child capabilities have been processed.
                        }
                    }
                    else
                    {
                        Logger.WriteWarning("Unknown child type '" + node.GetType() + "' with name '" + node.Data.Name + "'!");
                        //this._capabilityClass = null;
                        //return;
                    }
                }

                // Now that all children are known and all operations have been registered, register our child resources...
                foreach (RESTResourceCapability child in childResources) child.InitialiseParent(resourceCapItf);

                // If we're a document resource, locate the associated Business Component (if any)...
                this._componentClass = null;
                if (this._archetype == RESTResourceCapability.ResourceArchetype.Document)
                {
                    foreach (MEAssociation assoc in this._capabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
                    {
                        if (assoc.Destination.EndPoint.HasStereotype(businessComponentStereotype))
                        {
                            this._componentClass = assoc.Destination.EndPoint;
                            break;
                        }
                    }
                    ((RESTService)this._rootService).RegisterDocument(resourceCapItf);
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.InitializeCapability >> Error creating capability because: " + exc.Message);
                this._capabilityClass = null;   // Assures that instance is declared invalid.
            }
        }

        /************
        /// <summary>
        /// Helper function that is invoked by the capability hierarchy traversal for each node in the hierarchy, starting at the current resource
        /// and subsequently invoked for each subordinate capability (child resources and operations).
        /// The function collects items that must be displayed on the updated Resource diagram. It only selects capabilities that are defined
        /// within the resource package or other resources (irrespective of their package).
        /// </summary>
        /// <param name="svc">My parent service.</param>
        /// <param name="cap">The current Capability.</param>
        /// <returns>Always 'false', which indicates that traversal must continue until all nodes are processed.</returns>
        private bool DiagramItemsCollector(Service svc, Capability cap)
        {
            if (cap != null) // Safety catch, must not be NULL since we start at capability level.   
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                Logger.WriteInfo("Plugin.Application.Events.API.AddResourcesEvent.DiagramItemsCollector >> Traversing capability '" + cap.Name + "'...");
                if (cap is RESTResourceCapability || cap is RESTOperationCapability || cap.OwningPackage == this._resourcePackage)
                {
                    this._diagramClassList.Add(cap.CapabilityClass);
                    string msgAssemblyStereotype = context.GetConfigProperty(_MessageAssemblyClassStereotype);
                    string operationResultStereotype = context.GetConfigProperty(_RESTOperationResultStereotype);
                    bool mustShowMsgAssembly = context.GetBoolSetting(FrameworkSettings._SMAddMessageAssemblyToDiagram);
                    foreach (MEAssociation assoc in cap.CapabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
                    {
                        this._diagramAssocList.Add(assoc);
                        // If the endpoint of the association is a Message Assembly or an operation result, we MIGHT have to add it to the diagram manually...
                        if (assoc.Destination.EndPoint.HasStereotype(msgAssemblyStereotype) && mustShowMsgAssembly)
                        {
                            this._diagramClassList.Add(assoc.Destination.EndPoint);
                        }
                        else if (assoc.Destination.EndPoint.HasStereotype(operationResultStereotype))
                        {
                            this._diagramClassList.Add(assoc.Destination.EndPoint);
                        }
                    }
                }
            }
            return false;
        }
        *****************/
    }
}
