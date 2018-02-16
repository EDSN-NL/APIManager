using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Framework.Model;
using Framework.Logging;
using Framework.Context;
using Framework.Util;
using Plugin.Application.Forms;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// A simple helper class that bundles the components that make up a REST parameter (attributes of a Path Expression or operation).
    /// </summary>
    internal sealed class RESTResourceDeclaration
    {
        // The status is used to track operations on the declaration.
        internal enum DeclarationStatus { Created, Edited, Deleted, Invalid }

        // Configuration properties used by this module:
        private const string _ArchetypeTag                  = "ArchetypeTag";
        private const string _ServiceContainerPkgStereotype = "ServiceContainerPkgStereotype";
        private const string _RootPkgName                   = "RootPkgName";
        private const string _EmptyResourceName             = "EmptyResourceName";
        private const string _ResourceClassStereotype       = "ResourceClassStereotype";
        private const string _RESTParameterStereotype       = "RESTParameterStereotype";
        private const string _ParameterScopeTag             = "ParameterScopeTag";
        private const string _CollectionFormatTag           = "CollectionFormatTag";
        private const string _BusinessComponentStereotype   = "BusinessComponentStereotype";
        private const string _TagNamesTag                   = "TagNamesTag";

        private Capability _parent;                                     // The capability that acts as parent for the resource. Either an Interface or a Resource.
        private MEClass _existingResource;                              // Contains associated class in case of existing resource.
        private string _name;                                           // Resource name.
        private RESTResourceCapability.ResourceArchetype _archetype;    // Associated resource archetype.
        private SortedList<string, RESTOperationDeclaration> _operationList;    // Operations associated with this resource.
        private SortedList<string, RESTResourceDeclaration> _children;  // List of child resource descriptors.
        private RESTParameterDeclaration _parameter;                    // Parameter specification in case of 'Identifier' resource.
        private MEClass _documentClass;                                 // Either a Business Component or an existing Document Resource class.
        private DeclarationStatus _status;                              // Status of this declaration record.
        private DeclarationStatus _initialStatus;                       // Used to keep track of status change from empty to created.
        private string _description;                                    // Description text entered by user.
        private string _externalDocDescription;                         // External documentation description entered by user.
        private string _externalDocURL;                                 // URL to be used for external documentation.
        private List<string> _tagNames;                                 // List of tags assigned to this resource.

        /// <summary>
        /// Get or set the archetype of this resource.
        /// </summary>
        internal RESTResourceCapability.ResourceArchetype Archetype
        {
            get { return this._archetype; }
            set { SetType(value); }
        }

        /// <summary>
        /// Get or set the resource description text.
        /// </summary>
        internal string Description
        {
            get { return this._description; }
            set { this._description = value; }
        }

        /// <summary>
        /// Returns the associated Business Component in case of Document resources. If the user has associated the declaration
        /// with an existing Document Resource, the property returns the Document Resource class instead!
        /// Returns NULL when neither component is defined.
        /// </summary>
        internal MEClass DocumentClass { get { return this._documentClass; } }

        /// <summary>
        /// Get or set the description for external documentation.
        /// </summary>
        internal string ExternalDocDescription
        {
            get { return this._externalDocDescription; }
            set { this._externalDocDescription = value; }
        }

        /// <summary>
        /// Get or set the URL for external documentation.
        /// </summary>
        internal string ExternalDocURL
        {
            get { return this._externalDocURL; }
            set { this._externalDocURL = value; }
        }

        /// <summary>
        /// Get or set the list of tag names assigned to this resource.
        /// </summary>
        internal List<string> TagNames
        {
            get { return this._tagNames; }
            set { this._tagNames = value; }
        }

        /// <summary>
        /// Resource name.
        /// </summary>
        internal string Name
        {
            get { return GetName(); }
            set { SetName(value); }
        }

        /// <summary>
        /// Returns the list of associated operation declarations...
        /// </summary>
        internal List<RESTOperationDeclaration> Operations
        {
            get { return new List<RESTOperationDeclaration>(this._operationList.Values); }
        }

        /// <summary>
        /// Get the resource parameter in case of Identifier Resource...
        /// </summary>
        internal RESTParameterDeclaration Parameter { get { return this._parameter; } }

        /// <summary>
        /// Returns the capability that 'owns' this resource declaration.
        /// </summary>
        internal Capability Parent { get { return this._parent; } }

        /// <summary>
        /// Returns the list of child resources...
        /// </summary>
        internal List<RESTResourceDeclaration> ChildResources { get { return new List<RESTResourceDeclaration>(this._children.Values); } }

        /// <summary>
        /// If the resource declaration is based on an existing resource, this property returns the associated resource class.
        /// </summary>
        internal MEClass ResourceClass { get { return this._existingResource; } }

        /// <summary>
        /// Get or set the status of this declaration record.
        /// </summary>
        internal DeclarationStatus Status
        {
            get { return this._status; }
            set { this._status = value; }
        }

        /// <summary>
        /// Checks whether the declaration record is in a valid state...
        /// </summary>
        internal bool Valid { get { return this._status == DeclarationStatus.Invalid; } }

        /// <summary>
        /// Default constructor creates an empty, illegal, operation declaration that has no parent capability. 
        /// This is typically used for new API declaration where we have no structure yet.
        /// We create an empty resource collection (name = 'empty-resource-name', type = Collection).
        /// </summary>
        /// <param name="parent">The capability that acts as parent for the operation.</param>
        internal RESTResourceDeclaration()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceDeclaration (default) >> Creating empty declaration.");
            this._name = ContextSlt.GetContextSlt().GetConfigProperty(_EmptyResourceName);
            this._archetype = RESTResourceCapability.ResourceArchetype.Collection;
            this._existingResource = null;
            this._status = this._initialStatus = DeclarationStatus.Invalid;
            this._parent = null;
            this._parameter = new RESTParameterDeclaration();
            this._operationList = new SortedList<string, RESTOperationDeclaration>();
            this._children = new SortedList<string, RESTResourceDeclaration>();
            this._description = string.Empty;
            this._externalDocDescription = string.Empty;
            this._externalDocURL = string.Empty;
            this._documentClass = null;
            this._tagNames = new List<string>();
        }

        /// <summary>
        /// This constructor creates an empty, illegal, operation declaration that is linked to its parent Capability.
        /// Since we did not specify a name, we implicitly create an empty collection.
        /// </summary>
        /// <param name="parent">The capability that acts as parent for the operation.</param>
        internal RESTResourceDeclaration(Capability parent)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceDeclaration >> Creating empty declaration.");
            this._name = this._name = ContextSlt.GetContextSlt().GetConfigProperty(_EmptyResourceName);
            this._archetype = RESTResourceCapability.ResourceArchetype.Collection;
            this._existingResource = null;
            this._status = this._initialStatus = DeclarationStatus.Invalid;
            this._parent = parent;
            this._parameter = new RESTParameterDeclaration();
            this._operationList = new SortedList<string, RESTOperationDeclaration>();
            this._children = new SortedList<string, RESTResourceDeclaration>();
            this._description = string.Empty;
            this._externalDocDescription = string.Empty;
            this._externalDocURL = string.Empty;
            this._documentClass = null;
            this._tagNames = new List<string>();
        }

        /// <summary>
        /// Creates a new declaration descriptor using the provided parameters.
        /// </summary>
        /// <param name="name">The name of the resource (unique within the API).</param>
        /// <param name="archetype">The archetype of the resource.</param>
        internal RESTResourceDeclaration(string name, RESTResourceCapability.ResourceArchetype archetype, Capability parent)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceDeclaration >> Creating new declaration with name '" + name + "' and type '" + archetype + "'...");
            this._name = name;
            this._archetype = archetype;
            this._parent = parent;
            this._existingResource = null;
            this._status = DeclarationStatus.Invalid;
            if (this._name != string.Empty && this._archetype != RESTResourceCapability.ResourceArchetype.Unknown) this._status = DeclarationStatus.Created;
            this._initialStatus = this._status;
            this._parameter = new RESTParameterDeclaration();
            this._operationList = new SortedList<string, RESTOperationDeclaration>();
            this._children = new SortedList<string, RESTResourceDeclaration>();
            this._description = string.Empty;
            this._externalDocDescription = string.Empty;
            this._externalDocURL = string.Empty;
            this._documentClass = null;
            this._tagNames = new List<string>();
        }

        /// <summary>
        /// Constructor that creates a resource declaration based on an existing resource class. We check whether the provided class is indeed a child of the
        /// specified parent. If this is not the case, the status is set to 'Invalid'.
        /// </summary>
        /// <param name="resourceClass">Associated resource class.</param>
        /// <param name="parent">Pa</param>
        internal RESTResourceDeclaration(MEClass resourceClass, Capability parent)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceDeclaration >> Creating declaration for existing class '" + resourceClass.Name + "'...");

            ContextSlt context = ContextSlt.GetContextSlt();;
            string repositoryRootName = context.GetConfigProperty(_RootPkgName);
            string containerStereotype = context.GetConfigProperty(_ServiceContainerPkgStereotype);
            string RESTParameterStereotype = context.GetConfigProperty(_RESTParameterStereotype);
            string businessComponentStereotype = context.GetConfigProperty(_BusinessComponentStereotype);

            // The resource must be a child of the current service declaration...
            // We search upwards until we either find our parent, reacht container level or, as a last safety catch, the top of the repository...
            MEPackage rootPkg = parent.RootService.DeclarationPkg;
            MEPackage currentPkg = resourceClass.OwningPackage;
            while (currentPkg != rootPkg && !currentPkg.HasStereotype(containerStereotype) && currentPkg.Name != repositoryRootName)
            {
                currentPkg = currentPkg.Parent;
            }

            this._name = resourceClass.Name;
            this._existingResource = resourceClass;
            this._parent = parent;
            string archetypeTag = resourceClass.GetTag(context.GetConfigProperty(_ArchetypeTag));
            this._archetype = EnumConversions<RESTResourceCapability.ResourceArchetype>.StringToEnum(archetypeTag);
            this._parameter = null;
            this._operationList = new SortedList<string, RESTOperationDeclaration>();
            this._children = new SortedList<string, RESTResourceDeclaration>();

            // Fetch the optional list of tag names from the resource...
            string tagList = resourceClass.GetTag(context.GetConfigProperty(_TagNamesTag));
            this._tagNames = new List<string>();
            if (!string.IsNullOrEmpty(tagList))
            {
                string[] tagArray = tagList.Split(',');
                foreach (string tagName in tagArray) this._tagNames.Add(tagName.Trim());
            }

            // Check whether we have a parameter (in case of Identifier Resource)...
            // If the class has multiple RESTParameter attributes, we simply take the first one we encounter (and issue a warning 'cause this is illegal)...
            foreach (MEAttribute attrib in resourceClass.Attributes)
            {
                if (attrib.HasStereotype(RESTParameterStereotype))
                {
                    this._parameter = new RESTParameterDeclaration(attrib);
                    if (resourceClass.Attributes.Count > 1)
                        Logger.WriteWarning("Plugin.Application.CapabilityModel.API.RESTResourceDeclaration >> Resource '" +
                                            this._name + "' has too many attributes, only '" + this._parameter.Name + "' is used!");
                    break;
                }
            }

            // If we are a 'document' resource, locate the associated Business Component...
            // In theory, the Document Resource class name MUST be identical to the associated Business Component. However, it is
            // possible to rename the resource class so we still look for the stereotype to be sure we have the correct component.
            foreach (MEAssociation association in resourceClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
            {
                if (association.Destination.EndPoint.HasStereotype(businessComponentStereotype))
                {
                    this._documentClass = association.Destination.EndPoint;
                    if (this._documentClass.Name != this._name)
                    {
                        Logger.WriteWarning("Plugin.Application.CapabilityModel.API.RESTResourceDeclaration >> Document Resource '" +
                                            this._name + "' does not match associated Business Document '" + this._documentClass.Name + "'!");
                    }
                    break;
                }
            }

            // Check all preconditions for the status to be valid...
            this._status = (currentPkg == rootPkg && this._name != string.Empty && 
                            this._archetype != RESTResourceCapability.ResourceArchetype.Unknown) ? DeclarationStatus.Created : DeclarationStatus.Invalid;
            this._initialStatus = this._status;
        }

        /// <summary>
        /// Add a new operation to the resource. The method displays the 'add operation' dialog to the user in order to create 
        /// and initialize the operation. On exit, if the operation does not exist, it is added to the list. If the operation exists, 
        /// but has a status of 'Deleted', it is replaced and the status is set to 'Edited'.
        /// Otherwise (duplicate operation), the method shows an error and ignores the operation.
        /// If the user decided to cancel the action, an empty operation is returned.
        /// </summary>
        /// <returns>The created operation or NULL on errors.</returns>
        internal RESTOperationDeclaration AddOperation()
        {
            if (this._archetype == RESTResourceCapability.ResourceArchetype.Document ||
                this._archetype == RESTResourceCapability.ResourceArchetype.Unknown)
            {
                MessageBox.Show("Currently selected resource type does not support operations!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            RESTResourceCapability parentResource = null;
            if (this._parent is RESTResourceCapability) parentResource = this._parent as RESTResourceCapability;

            var newOperation = new RESTOperationDeclaration(parentResource, string.Empty, RESTOperationCapability.OperationType.Unknown);
            using (var dialog = new RESTOperationDialog(newOperation))
            {
                dialog.DisableMinorVersion();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (!this._operationList.ContainsKey(dialog.Operation.Name))
                    {
                        dialog.Operation.Status = RESTOperationDeclaration.DeclarationStatus.Created;
                        this._operationList.Add(dialog.Operation.Name, dialog.Operation);
                        if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                        newOperation = dialog.Operation;
                    }
                    else if (this._operationList[dialog.Operation.Name].Status == RESTOperationDeclaration.DeclarationStatus.Deleted)
                    {
                        dialog.Operation.Status = RESTOperationDeclaration.DeclarationStatus.Edited;
                        this._operationList[dialog.Operation.Name] = dialog.Operation;
                        if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                        newOperation = dialog.Operation;
                    }
                    else
                    {
                        MessageBox.Show("Duplicate operation name, please try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        newOperation = null;
                    }
                }
            }
            return newOperation;
        }

        /// <summary>
        /// Add a new child resource to the current resource. The method displays the 'add resource' dialog to the user in order to create 
        /// and initialize the resource. On exit, if the resource does not exist, it is added to the list. If the resource exists, but
        /// has a status of 'Deleted', it is replaced and the status is set to 'Edited'.
        /// Otherwise (duplicate resource), the method shows an error and ignores the resource.
        /// If the user decided to cancel the action, an empty resource is returned..
        /// </summary>
        /// <returns>Created resource or NULL on errors.</returns>
        internal RESTResourceDeclaration AddResource()
        {
            RESTResourceDeclaration newResource = new RESTResourceDeclaration(this._parent);
            using (var dialog = new RESTResourceDialog(newResource))
            {
                dialog.DisableMinorVersion();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    newResource = dialog.Resource;
                    if (!this._children.ContainsKey(newResource.Name))
                    {
                        newResource.Status = RESTResourceDeclaration.DeclarationStatus.Created;
                        this._children.Add(newResource.Name, newResource);
                        if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                    }
                    else if (this._children[newResource.Name].Status == RESTResourceDeclaration.DeclarationStatus.Deleted)
                    {
                        newResource.Status = RESTResourceDeclaration.DeclarationStatus.Edited;
                        this._children[newResource.Name] = newResource;
                        if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                    }
                    else
                    {
                        MessageBox.Show("Duplicate resource name, please try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        newResource = null;
                    }
                }
            }
            return newResource;
        }

        /// <summary>
        /// Can be used to reset the Document Class property. This also clears the Resource Declaration name!
        /// If no Document Class property has been set, the method performs no actions.
        /// </summary>
        internal void ClearDocumentClass()
        {
            if (this._documentClass != null)
            {
                this._documentClass = null;
                this._name = string.Empty;
            }
        }

        /// <summary>
        /// Removes the parameter definition from the current Resource Declaration;
        /// </summary>
        internal void ClearParameter()
        {
            this._parameter = null;
        }

        /// <summary>
        /// This method assigns an existing Document Resource class to the Document Class property. It checks the list of registered Document
        /// Resources that is maintained at Service level. If there is only one registered Document, it is selected automatically. If there are
        /// multiple, the method presents the Capability Picker in order to let the user select the desired Document.
        /// </summary>
        /// <returns>Name of selected component or empty string in case of errors/cancel.</returns>
        internal string LinkDocumentClass()
        {
            var documentList = new List<Capability>();
            string documentName = string.Empty;
            if (((RESTService)this._parent.RootService).DocumentList.Count > 0)
            {
                // If we only have a single defined Document Resource, this is selected automatically. When there are multiple,
                // we ask the user which one to use...
                if (((RESTService)this._parent.RootService).DocumentList.Count == 1)
                {
                    this._documentClass = ((RESTService)this._parent.RootService).DocumentList[0].CapabilityClass;
                    documentName = this._documentClass.Name;
                }
                else
                {
                    foreach (Capability cap in ((RESTService)this._parent.RootService).DocumentList) documentList.Add(cap);
                    using (CapabilityPicker dialog = new CapabilityPicker("Select Document resource", documentList, false, false))
                    {
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            List<Capability> checkedCapabilities = dialog.GetCheckedCapabilities();
                            if (checkedCapabilities.Count > 0)
                            {
                                // If the user selected multiple, we take the first one.
                                this._documentClass = checkedCapabilities[0].CapabilityClass;
                                documentName = this._documentClass.Name;
                            }
                        }
                    }
                }
            }
            else MessageBox.Show("No suitable Document Resources to select, add one first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return documentName;
        }

        /// <summary>
        /// If the resource is an Identifier Resource, the method call will display the 'Edit Parameter' dialog so that the user can create or
        /// update a parameter declaration object. On return from the dialog, if the parameter settings are Ok, the parameter declaration is
        /// initialized and the created / modified parameter declaration object is returned to the caller.
        /// </summary>
        /// <returns>Created parameter declaration or NULL on cancel/errors.</returns>
        internal RESTParameterDeclaration SetParameter()
        {
            if (this._archetype == RESTResourceCapability.ResourceArchetype.Identifier)
            {
                bool isCreate = (this._parameter == null);
                if (isCreate) this._parameter = new RESTParameterDeclaration();
                using (var dialog = new RESTParameterDialog(this._parameter))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        if (dialog.Parameter.Default != string.Empty)
                        {
                            MessageBox.Show("Default value not allowed, please try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }
                        if (!(dialog.Parameter.Classifier is MEDataType) && !(dialog.Parameter.Classifier is MEEnumeratedType))
                        {
                            MessageBox.Show("Parameter must be Data Type or Enumeration, please try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }
                        if (dialog.Parameter.Cardinality.Item1 != 1)
                        {
                            MessageBox.Show("Identifier is mandatory, lower bound adjusted!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            dialog.Parameter.Cardinality = new Tuple<int, int>(1, dialog.Parameter.Cardinality.Item2);
                        }
                        if (dialog.Parameter.Cardinality.Item2 == 0 || dialog.Parameter.Cardinality.Item2 > 1)
                        {
                            if (dialog.Parameter.CollectionFormat == RESTParameterDeclaration.QueryCollectionFormat.Multi ||
                                dialog.Parameter.CollectionFormat == RESTParameterDeclaration.QueryCollectionFormat.NA ||
                                dialog.Parameter.CollectionFormat == RESTParameterDeclaration.QueryCollectionFormat.Unknown)
                            {
                                MessageBox.Show("Collection Format must be one of CSV, SSV, TSV or Pipes, please try again!", 
                                                "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return null;
                            }
                        }
                        else dialog.Parameter.CollectionFormat = RESTParameterDeclaration.QueryCollectionFormat.NA;
                        this._parameter = dialog.Parameter;
                        this._parameter.Status = isCreate ? RESTParameterDeclaration.DeclarationStatus.Created :
                                                            RESTParameterDeclaration.DeclarationStatus.Edited;
                        this._parameter.Scope = RESTParameterDeclaration.ParameterScope.Path;
                        if (this._initialStatus != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                    }
                }
            }
            return this._parameter;
        }

        /// <summary>
        /// This method assigns a (new) class to the Document Class property. It presents a class selector to the user, which facilitates
        /// browsing of the component tree and selecting the appropriate Business Document. When we have a valid parent Capability, the
        /// method verifies whether the component selected by the user is indeed part of our API.
        /// </summary>
        /// <returns>Name of selected component or empty string in case of errors/cancel.</returns>
        internal string SetDocumentClass()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string selectedName = string.Empty;
            MEClass selectedClass = context.SelectClass(new List<string> { context.GetConfigProperty(_BusinessComponentStereotype) });
            if (selectedClass != null)
            {
                if (this._parent != null)
                {
                    // If we have a parent, we can validate that the selected class is indeed part of my API...
                    // We also have to check whether the selected class has an Alias name defined. If so, we use that name instead of the class name.
                    MEPackage declPackage = this._parent.RootService.DeclarationPkg;
                    if (selectedClass.OwningPackage.Parent == declPackage || selectedClass.OwningPackage.Parent.Parent == declPackage)
                    {
                        selectedName = !string.IsNullOrEmpty(selectedClass.AliasName) ? selectedClass.AliasName : selectedClass.Name;
                        this._documentClass = selectedClass;
                        this._name = selectedName;
                    }
                    else MessageBox.Show("Selected component is not part of current API, please try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
                else
                {
                    // If we don't have a (valid) parent, we simply assume that this is a valid selection.
                    // We also have to check whether the selected class has an Alias name defined. If so, we use that name instead of the class name.
                    selectedName = !string.IsNullOrEmpty(selectedClass.AliasName) ? selectedClass.AliasName : selectedClass.Name;
                    this._documentClass = selectedClass;
                    this._name = selectedName;
                }
            }
            return selectedName;
        }

        /// <summary>
        /// Deletes an operation from the list. In fact, the record is not actually removed but marked as 'deleted'.
        /// </summary>
        /// <param name="operationName">Name of the Operation to be deleted.</param>
        internal void DeleteOperation(string operationName)
        {
            if (this._operationList.ContainsKey(operationName))
            {
                this._operationList[operationName].Status = RESTOperationDeclaration.DeclarationStatus.Deleted;
                if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
            }
        }

        /// <summary>
        /// Deletes a resource from the list. In fact, the record is not actually removed but marked as 'deleted'.
        /// </summary>
        /// <param name="resourceName">Name of the Operation to be deleted.</param>
        internal void DeleteResource(string resourceName)
        {
            if (this._children.ContainsKey(resourceName))
            {
                this._children[resourceName].Status = RESTResourceDeclaration.DeclarationStatus.Deleted;
                if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
            }
        }

        /// <summary>
        /// This function is called to facilitate editing of the operation with the provided name. The method displays
        /// an edit dialog that the user then uses to make changes to the operation. On return, the operation is updated
        /// and the updated object is returned to the caller (could be unchanged).
        /// On errors, the function returns NULL.
        /// </summary>
        /// <param name="operationName">Name of operation that has to be edited.</param>
        /// <returns>The edited operation.</returns>
        internal RESTOperationDeclaration EditOperation(string operationName)
        {
            string originalKey = operationName;
            RESTOperationDeclaration operation = GetOperation(operationName);
            if (operation != null)
            {
                using (var dialog = new RESTOperationDialog(operation))
                {
                    dialog.DisableMinorVersion();
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        if (dialog.Operation.Name == originalKey || !this._operationList.ContainsKey(dialog.Operation.Name))
                        {
                            operation = dialog.Operation;
                            if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;

                            if (operation.Name != originalKey)
                            {
                                this._operationList.Remove(originalKey);
                                this._operationList.Add(operation.Name, operation);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Renaming operation resulted in duplicate name, please try again!",
                                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            operation = null;
                        }
                    }
                }
            }
            return operation;
        }

        /// <summary>
        /// This function is called to facilitate editing of the resource with the provided name. The method displays
        /// an edit dialog that the user then uses to make changes to the resource. On return, the resource is updated
        /// and the updated object is returned to the caller (could be unchanged).
        /// On errors, the function returns NULL.
        /// </summary>
        /// <param name="resourceName">Name of resource that has to be edited.</param>
        /// <returns>The edited resaource.</returns>
        internal RESTResourceDeclaration EditResource(string resourceName)
        {
            string originalKey = resourceName;
            RESTResourceDeclaration resource = GetResource(resourceName);
            if (resource != null)
            {
                using (var dialog = new RESTResourceDialog(resource))
                {
                    dialog.DisableMinorVersion();
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        if (dialog.Resource.Name == originalKey || !this._children.ContainsKey(dialog.Resource.Name))
                        {
                            resource = dialog.Resource;
                            resource.Status = RESTResourceDeclaration.DeclarationStatus.Edited;

                            if (resource.Name != originalKey)
                            {
                                this._children.Remove(originalKey);
                                this._children.Add(resource.Name, resource);
                            }
                            if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                        }
                        else
                        {
                            MessageBox.Show("Renaming resource resulted in duplicate name, please try again!",
                                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            resource = null;
                        }
                    }
                }
            }
            return resource;
        }

        /// <summary>
        /// Returns the operation with the provided name. If no operation could be found, the function returns NULL.
        /// </summary>
        /// <param name="operationName">Name of operation to retrieve.</param>
        /// <returns>Operation declaration or NULL if not found.</returns>
        internal RESTOperationDeclaration GetOperation(string operationName)
        {
            return (this._operationList.ContainsKey(operationName)) ? this._operationList[operationName] : null;
        }

        /// <summary>
        /// Returns the resource with the provided name. If no resource could be found, the function returns NULL.
        /// </summary>
        /// <param name="resourceName">Name of resource to retrieve.</param>
        /// <returns>Resource declaration or NULL if not found.</returns>
        internal RESTResourceDeclaration GetResource(string resourceName)
        {
            return (this._children.ContainsKey(resourceName)) ? this._children[resourceName] : null;
        }

        /// <summary>
        /// This method displays a Class Picker dialog that the user can use to select an existing resource class. This MUST be a resource
        /// that is part of the current API!
        /// </summary>
        /// <returns>Created resource or NULL on errors.</returns>
        internal RESTResourceDeclaration SelectResource()
        {
            // Ask the user to select a class that has the 'Resource' stereotype...
            var resourceStereotype = new List<string>
            {
                ContextSlt.GetContextSlt().GetConfigProperty(_ResourceClassStereotype)
            };
            MEClass resourceClass = ContextSlt.GetContextSlt().SelectClass(resourceStereotype);

            if (resourceClass != null)
            {
                RESTResourceDeclaration newResource = new RESTResourceDeclaration(resourceClass, this._parent);
                if (newResource.Status != RESTResourceDeclaration.DeclarationStatus.Invalid)
                {
                    if (!this._children.ContainsKey(newResource.Name))
                    {
                        newResource.Status = RESTResourceDeclaration.DeclarationStatus.Created;
                        this._children.Add(newResource.Name, newResource);
                        if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                    }
                    else if (this._children[newResource.Name].Status == RESTResourceDeclaration.DeclarationStatus.Deleted)
                    {
                        newResource.Status = RESTResourceDeclaration.DeclarationStatus.Edited;
                        this._children[newResource.Name] = newResource;
                        if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                    }
                    else
                    {
                        MessageBox.Show("Duplicate resource name, please try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        newResource = null;
                    }
                }
                else
                {
                    MessageBox.Show("Invalid resource selected, please try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    newResource = null;
                }
                return newResource;
            }
            return null;
        }

        /// <summary>
        /// Constructs a name for Resource based on archetype and configured parameters.
        /// </summary>
        /// <returns>Dynamically constructed name.</returns>
        private string GetName()
        {
            string name = string.Empty;
            if (this._archetype == RESTResourceCapability.ResourceArchetype.Identifier)
            {
                if (this._parameter != null)
                {
                    name = "{" + this._parameter.Name + "(" + this._parameter.Classifier.Name +
                           (this._parameter.Default != string.Empty ? (")[" + this._parameter.Default + "]") : ")") + "}";
                }
                else name = "{}";
                this._name = name;
            }
            else if (string.IsNullOrEmpty(this._name)) this._name = ContextSlt.GetContextSlt().GetConfigProperty(_EmptyResourceName);
            return this._name;
        }

        /// <summary>
        /// Helper function which loads a (new) resource name. If both the name and archetype are valid, the status is set to Created.
        /// If the initial status was not invalid, the status is set to edited.
        /// If we have a archetype of 'Identifier', the name MUST NOT be set directly, so the method ignores the received name and invokes
        /// 'GetName' instead in order to construct an Identifier-based name.
        /// </summary>
        /// <param name="newName">Name to be assigned to resource.</param>
        private void SetName(string newName)
        {
            if (this._name != newName) // We only do something in case the name is indeed different from the current name!
            {
                if (this._archetype != RESTResourceCapability.ResourceArchetype.Identifier)
                {
                    this._name = newName;
                    if (this._initialStatus == DeclarationStatus.Invalid && this._archetype != RESTResourceCapability.ResourceArchetype.Unknown) this._status = DeclarationStatus.Created;
                    else if (this._initialStatus != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                }
                else GetName();
            }
        }

        /// <summary>
        /// Helper function which loads a (new) resource archetype. If both the name and archtetype are valid, the status is set to Created.
        /// If the initial status was not invalid, the status is set to edited.
        /// </summary>
        /// <param name="newType">Archetype to be assigned to resource.</param>
        private void SetType(RESTResourceCapability.ResourceArchetype newType)
        {
            if (this._archetype != newType) // We only perform an action when the type actually changes.
            {
                this._archetype = newType;
                if (this._archetype == RESTResourceCapability.ResourceArchetype.Document ||
                    this._archetype == RESTResourceCapability.ResourceArchetype.Unknown)
                {
                    this._operationList.Clear();    // Documents or Unknown resources must not have operations!
                }
                if (this._initialStatus == DeclarationStatus.Invalid && this._name != string.Empty) this._status = DeclarationStatus.Created;
                else if (this._initialStatus != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
            }
        }
    }
}
