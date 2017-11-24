using System;
using System.Collections.Generic;
using Framework.Model;
using Framework.Logging;
using Framework.Context;
using Framework.Util;
using Framework.View;

namespace Plugin.Application.CapabilityModel.API
{
    /// Represents a Resource Collection in a REST service. Resource collections can exist on root- or intermediate levels in a resource model
    /// and can contain operations as well as Path Expressions as children.
    /// At root-level, a Resource Collection can be empty, which means that it does not appear as part of the URI. However, in the UML model,
    /// it must ALWAYS exist as a model element! Empty collections are identified by a reserved name "[Empty]". The use of angle brackets 
    /// facilitates introduction of multiple reserved names in the future.
    /// Example: "https://api.enexis.nl/customers/v1/{CustomerID} 
    /// In model: Interface = "Customers" (version = 1.0) --> Collection "[Empty]" --> Path "/{CustomerID}" --> ResourceRepresentation "CustomerDetails"
    /// Empty Resource Collections ONLY exist at root-level and there can be at most ONE per API!
    internal class RESTResourceCapabilityImp: CapabilityImp
    {
        // Configuration properties used by this module:
        private const string _BusinessComponentStereotype           = "BusinessComponentStereotype";
        private const string _ResourceCollectionPkgStereotype       = "ResourceCollectionPkgStereotype";
        private const string _ResourceClassStereotype               = "ResourceClassStereotype";
        private const string _RESTOperationClassStereotype          = "RESTOperationClassStereotype";
        private const string _RESTOperationResultStereotype         = "RESTOperationResultStereotype";
        private const string _DocumentationTypeClassName            = "DocumentationTypeClassName";
        private const string _CoreDataTypesPathName                 = "CoreDataTypesPathName";
        private const string _MessageAssemblyClassStereotype        = "MessageAssemblyClassStereotype";
        private const string _IdentifierResourceRoleName            = "IdentifierResourceRoleName";
        private const string _ArchetypeTag                          = "ArchetypeTag";
        private const string _IsRootLevelTag                        = "IsRootLevelTag";
        private const string _IsTag                                 = "IsTag";

        // Names of attributes to be used for external documentation descriptor. These MUST be a case-insensitive match of the
        // associated OpenAPI property names!
        private const string _DescriptionClassifier                 = "TextType";           // Classifier for Terms of Service.
        private const string _URLClassifier                         = "IdentifierType";     // Classifier for names.
        private const string _DescriptionAttribute                  = "Description";        // Attribute name for License.Name
        private const string _URLAttribute                          = "URL";                // Attribute name for License.URL

        private MEPackage _resourcePackage;                         // The package in which the resource lives.
        private Capability _myParent;                               // Either RESTInterface or other Resource.
        private RESTResourceCapability.ResourceArchetype _archetype;    // Specifies the archtetype of the resource.
        private bool _isCollection;                                 // Indicates whether the resource represents a resource collection.
        private bool _isRootLevel;                                  // Indicates whether the resource is a root-level resourcr ("sub-API").
        private bool _isTag;                                        // Indicates whether the resource is marked as a tag.

        // Keep track of (extra) classes and associations to show in the resource diagram...
        private List<MEClass> _diagramClassList = new List<MEClass>();
        private List<MEAssociation> _diagramAssocList = new List<MEAssociation>();

        /// <summary>
        /// Getters and setters of properties for this class:
        /// ResourcePackage = Retrieves the package that stores this resource.
        /// ArcheType = Returns the archetype of the resource.
        /// IsCollection = Returns true if this is a collection-type resource.
        /// IsRootLevel = Returns true if the resource represents a sub-API.
        /// IsTag = Returns true of the resource is marked as a tag.
        /// </summary>
        internal MEPackage ResourcePackage                          { get { return this._resourcePackage; } }
        internal RESTResourceCapability.ResourceArchetype Archetype { get { return this._archetype; } }
        internal bool IsCollection                                  { get { return this._isCollection; } }
        internal bool IsRootLevel                                   { get { return this._isRootLevel; } }
        internal bool IsTag                                         { get { return this._isTag; } }

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
        /// <param name="newMinorVersion">True when a new minor version must be created.</param>
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
                    Logger.WriteWarning("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.AddOperation >> Failed to create operation!");
                    result = false;
                }

                if (newMinorVersion)
                {
                    var newVersion = new Tuple<int, int>(RootService.Version.Item1, RootService.Version.Item2 + 1);
                    RootService.UpdateVersion(newVersion);
                    newVersion = new Tuple<int, int>(this._capabilityClass.Version.Item1, this._capabilityClass.Version.Item2 + 1);
                    this._capabilityClass.Version = newVersion;
                }
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
        /// <param name="newMinorVersion">True when a new minor version must be created.</param>
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
                    var resourceCapability = new RESTResourceCapability(parentResource, resource);
                    if (resourceCapability.Valid)
                    {
                        resourceCapability.InitialiseParent(parentResource);
                        string roleName = RESTUtil.GetAssignedRoleName(resource.Name);
                        newNames += (!isFirst) ? ", " + roleName : roleName;
                        isFirst = false;
                    }
                    else
                    {
                        Logger.WriteWarning("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.AddResources >> Failed to create resource '" + resource.Name + "'!");
                        result = false;
                    }
                }

                if (newMinorVersion)
                {
                    var newVersion = new Tuple<int, int>(RootService.Version.Item1, RootService.Version.Item2 + 1);
                    RootService.UpdateVersion(newVersion);
                    newVersion = new Tuple<int, int>(this._capabilityClass.Version.Item1, this._capabilityClass.Version.Item2 + 1);
                    this._capabilityClass.Version = newVersion;
                }
                string logMessage = "Added child Resource(s): '" + newNames + "'";
                RootService.CreateLogEntry(logMessage + " to parent Resource '" + Name + "'.");
                CreateLogEntry(logMessage + ".");
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

            this._myParent.RemoveChild(new RESTResourceCapability(this));     // Unlink this resource from my parent.
            base.Delete();                                                    // Deletes all resources owned by this capability (Operations, PathExpressions).
            if (this._isCollection) this._resourcePackage.Parent.DeletePackage(this._resourcePackage);    // Delete the package as last step (if we are a collection).
        }

        /// <summary>
        /// Deletes the resource identified by the specified resource-class object. This will delete the entire resource hierarchy.
        /// </summary>
        /// <param name="resourceClass">Identifies the resource to be deleted.</param>
        /// <param name="newMinorVersion">Set to 'true' when minor version must be updated, 'false' to keep existing version.</param>
        internal void DeleteResource(MEClass resourceClass, bool newMinorVersion)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.DeleteResource >> Going to delete resource '" + resourceClass.Name + "' from interface '" + Name + "'...");
            foreach (Capability cap in GetChildren())
            {
                if (cap.CapabilityClass == resourceClass)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.DeleteResource >> Found the resource!");
                    DeleteChild(cap.Implementation, true);
                    break;
                }
            }

            if (newMinorVersion)
            {
                var newVersion = new Tuple<int, int>(RootService.Version.Item1, RootService.Version.Item2 + 1);
                RootService.UpdateVersion(newVersion);
                newVersion = new Tuple<int, int>(this._capabilityClass.Version.Item1, this._capabilityClass.Version.Item2 + 1);
                this._capabilityClass.Version = newVersion;
            }
            string logMessage = "Deleted child Resource: '" + resourceClass.Name + "'";
            RootService.CreateLogEntry(logMessage + " from parent Resource '" + Name + "'.");
            CreateLogEntry(logMessage + ".");
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
        /// Renames the resource identified by the specified resource-class object.
        /// </summary>
        /// <param name="resourceClass">Resource to be renamed.</param>
        /// <param name="oldName">Original name of the Resource.</param>
        /// <param name="newName">New name for the Resource, in PascalCase.</param>
        /// <param name="newMinorVersion">Set to 'true' when minor version must be updated, 'false' to keep existing version.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal void RenameResource(MEClass resourceClass, string oldName, string newName, bool newMinorVersion)
        {
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

            // If we have to increment the minor version, do so BEFORE creating the log message...
            if (newMinorVersion)
            {
                var newVersion = new Tuple<int, int>(this._capabilityClass.Version.Item1, this._capabilityClass.Version.Item2 + 1);
                this._capabilityClass.Version = newVersion;
            }
            string logMessage = "Renamed child Resource: '" + oldName + "' to: '" + resourceClass.Name + "'";
            RootService.CreateLogEntry(logMessage + " in parent Resource '" + Name + "'.");
            CreateLogEntry(logMessage + ".");
        }

        /// <summary>
        /// Facilitates iteration over the set of child resources associated with this parent resource.
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
        /// Returns the list of all child resource capabilities associated with this parent resource.
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
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.GetResourceCollections >> Found resource '" + cap.Name + "'!");
                    collectionList.Add(cap as RESTResourceCapability);
                }
            }
            return collectionList;
        }

        /// <summary>
        /// Returns the file name (without extension) for this Capability. The extension is left out since this typically depends on the
        /// chosen serialization mechanism. The filename returned by this method only provides a generic name to be used for further, serialization
        /// dependent, processing.
        /// </summary>
        internal override string GetBaseFileName()
        {
            Tuple<int, int> version = this.CapabilityClass.Version;
            return this._rootService.Name + "_" + this.Name + "_v" + version.Item1 + "p" + version.Item2;
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
        /// Helper function that collects items that have been added to the specified diagram, relative to the specified parent capability.
        /// After collecting items, the diagram is updated and refreshed.
        /// </summary>
        /// <param name="diagram">Diagram to be updated.</param>
        /// <param name="parent">Parent class used as starting point for collecting updates.</param>
        /// <param name="parentLink">Association from my capability to this parent.</param>
        internal void UpdateResourceDiagram(Diagram diagram, MEClass parent, MEAssociation parentLink)
        {
            this._diagramClassList = new List<MEClass> { parent };
            this._diagramAssocList = new List<MEAssociation> { parentLink };
            Traverse(DiagramItemsCollector);

            diagram.AddClassList(this._diagramClassList);
            diagram.AddAssociationList(this._diagramAssocList);
            diagram.Show();
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
                this._capabilityClass.SetTag(context.GetConfigProperty(_IsTag), resource.IsTag.ToString());
                if (!string.IsNullOrEmpty(resource.Description)) MEChangeLog.SetRTFDocumentation(this._capabilityClass, resource.Description);
                this._capabilityClass.SetTag(context.GetConfigProperty(_IsRootLevelTag), this._isRootLevel.ToString());
                this._isTag = resource.IsTag;

                // Establish link with our Parent...
                var parentEndpoint = new EndpointDescriptor(parent.CapabilityClass, "1", parent.Name, null, false);
                var resourceEndpoint = new EndpointDescriptor(this._capabilityClass, "1", this._assignedRole, null, true);
                MEAssociation link = model.CreateAssociation(parentEndpoint, resourceEndpoint, MEAssociation.AssociationType.MessageAssociation);
                InitialiseParent(parent);

                // Create the ExternalDocument class if we have defined entries for it...
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
                        model.CreateAssociation(resourceEndpoint, docEndpoint, MEAssociation.AssociationType.MessageAssociation);
                    }
                    else Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTInterfaceCapabilityImp (create) >> Unable to find Contact Class '" +
                                           context.GetConfigProperty(_DocumentationTypeClassName) + "/" + context.GetConfigProperty(_BusinessComponentStereotype));
                }

                // Create the operations that are associated with this Resource...
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.ConstructCapability >> Creating operations...");
                var resourceCapItf = new RESTResourceCapability(this);
                foreach (RESTOperationDeclaration operation in resource.Operations)
                {
                    if (operation.Status != RESTOperationDeclaration.DeclarationStatus.Deleted &&
                        operation.Status != RESTOperationDeclaration.DeclarationStatus.Invalid)
                    {
                        RESTOperationCapability newOperation = new RESTOperationCapability(resourceCapItf, operation);
                    }
                }

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
                    myDiagram.AddDiagramProperties();
                    myDiagram.ShowConnectorStereotypes(false);
                    this._capabilityClass.AssociatedDiagram = myDiagram;
                    UpdateResourceDiagram(myDiagram, parent.CapabilityClass, link);
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
        /// Initializes an existing Resource capability. 
        /// </summary>
        /// <param name="parent">Parent can be either an Interface or a PathExpression.</param>
        /// <param name="hierarchy">Starting from our Capability, the hierarchy contains child PathExpressions or Operations.</param>
        private void InitializeCapability(Capability parent, TreeNode<MEClass> hierarchy)
        {
            try
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.InitializeCapability >> Creating new instance '" +
                                 parent.Name + "." + hierarchy.Data.Name + "'...");

                ContextSlt context = ContextSlt.GetContextSlt();
                this._myParent = parent;
                this._capabilityClass = hierarchy.Data;
                this._isTag = false;
                string archetypeTagName = context.GetConfigProperty(_ArchetypeTag);
                var resourceCapItf = new RESTResourceCapability(this);
                this._archetype = (RESTResourceCapability.ResourceArchetype) Enum.Parse(typeof(RESTResourceCapability.ResourceArchetype), 
                                                                                        this._capabilityClass.GetTag(archetypeTagName), true);
                this._isCollection = (this._archetype == RESTResourceCapability.ResourceArchetype.Collection || 
                                      this._archetype == RESTResourceCapability.ResourceArchetype.Store);
                this._resourcePackage = this._isCollection? this.RootService.ModelPkg.FindPackage(this._capabilityClass.Name, context.GetConfigProperty(_ResourceCollectionPkgStereotype)):
                                                            parent.CapabilityClass.OwningPackage;
                this._assignedRole = parent.FindChildClassRole(this._capabilityClass.Name, context.GetConfigProperty(_ResourceClassStereotype));
                if (string.Compare(this._capabilityClass.GetTag(context.GetConfigProperty(_IsTag)), "true", true) == 0)
                {
                    ((RESTService)RootService).RegisterTag(resourceCapItf);
                    this._isTag = true;
                }
                this._isRootLevel = (string.Compare(this._capabilityClass.GetTag(context.GetConfigProperty(_IsRootLevelTag)), "true", true) == 0);

                foreach (TreeNode<MEClass> node in hierarchy.Children)
                {
                    if (node.Data.HasStereotype(context.GetConfigProperty(_ResourceClassStereotype)))
                    {
                        var resource = new RESTResourceCapability(resourceCapItf, node);
                        resource.InitialiseParent(resourceCapItf);
                        if (!resource.Valid)
                        {
                            Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.InitializeCapability >> Error creating Path '" + node.Data.Name + "'!");
                            this._capabilityClass = null;
                            return;
                        }
                    }
                    else if (node.Data.HasStereotype(context.GetConfigProperty(_RESTOperationClassStereotype)))
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
                    else
                    {
                        Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.InitializeCapability >> Unknown child type '" + 
                                          node.GetType() + "' with name '" + node.Data.Name + "'!");
                        this._capabilityClass = null;
                        return;
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTResourceCapabilityImp.InitializeCapability >> Error creating capability because: " + exc.Message);
                this._capabilityClass = null;   // Assures that instance is declared invalid.
            }
        }

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
    }
}
