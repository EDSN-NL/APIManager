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
    internal sealed class RESTResourceDeclaration : IEquatable<RESTResourceDeclaration>, IDisposable
    {
        // The status is used to track operations on the declaration.
        internal enum DeclarationStatus { Created, Deleted, Edited, Invalid, Stable }

        // Configuration properties used by this module:
        private const string _EmptyResourceName                         = "EmptyResourceName";
        private const string _ResourceClassStereotype                   = "ResourceClassStereotype";
        private const string _BusinessComponentStereotype               = "BusinessComponentStereotype";
        private const string _GenericMessageClassStereotype             = "GenericMessageClassStereotype";
        private const string _DocumentationTypeClassName                = "DocumentationTypeClassName";
        private const string _ProfileStereotype                         = "ProfileStereotype";

        private Capability _parent;                                     // The capability that acts as parent for the resource. Either an Interface or a Resource.
        private MEClass _existingResource;                              // Contains associated class in case of existing resource.
        private string _name;                                           // Resource name.
        private RESTResourceCapability.ResourceArchetype _archetype;    // Associated resource archetype.
        private List<HTTPOperation> _availableOperationsList;           // The list of HTTP operations that are available to be assigned to the resource.
        private SortedList<string, RESTOperationDeclaration> _operationList;    // Operations associated with this resource.
        private SortedList<string, RESTResourceDeclaration> _children;  // List of child resource descriptors.
        private RESTParameterDeclaration _parameter;                    // Parameter specification in case of 'Identifier' resource.
        private SortedList<string, MEClass> _businessDocumentClassList; // The list of actual message-model documents, in case of Document resource, the list contains one entry!
        private MEClass _documentResourceClass;                         // When we link against an existing Document Class or ProfileSet class, this contains the associated UML class.
        private DeclarationStatus _status;                              // Status of this declaration record.
        private DeclarationStatus _initialStatus;                       // Used to keep track of status change from empty to created.
        private string _description;                                    // Description text entered by user.
        private string _externalDocDescription;                         // External documentation description entered by user.
        private string _externalDocURL;                                 // URL to be used for external documentation.
        private List<string> _tagNames;                                 // List of tags assigned to this resource.
        private bool _disposed;                                         // Mark myself as invalid after call to dispose!

        /// <summary>
        /// This is the normal entry for all users of the object that want to indicate that the resource declaration is not required anymore.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Get or set the archetype of this resource.
        /// </summary>
        internal RESTResourceCapability.ResourceArchetype Archetype
        {
            get { return this._archetype; }
            set { SetType(value); }
        }

        /// <summary>
        /// Get the list of available operations for this resource.
        /// </summary>
        internal List<HTTPOperation> AvailableOperationsList
        {
            get { return this._availableOperationsList; }
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
        /// Returns the linked Document- or ProfileSet Resource class in case of linked resources. Returns NULL when we have an
        /// 'actual' resource, in which case the associated message business documents are in BusinessDocumentClassList.
        /// Returns NULL when neither component is defined.
        /// </summary>
        internal MEClass DocumentResourceClass { get { return this._documentResourceClass; } }

        /// <summary>
        /// Only when we're constructing/editing a Document- or ProfileSet resource, this returns the list of profiles and associated root classes.
        /// When we're a simple Document resource, this list will only contain the 'Basic' profile at entry 0.
        /// On non-Document= / ProfileSet resources, this property should be ignored.
        /// </summary>
        internal SortedList<string, MEClass> ProfileSet { get { return this._businessDocumentClassList; } }

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
        /// Compares the Resource Declaration against another object. If the other object is also a Resource Declaration, the 
        /// function returns true if both Declarations are of the same archetype and have the same name. In all other cases, the
        /// function returns false.
        /// </summary>
        /// <param name="obj">The thing to compare against.</param>
        /// <returns>True if same object, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var objElement = obj as RESTResourceDeclaration;
            return (objElement != null) && Equals(objElement);
        }

        /// <summary>
        /// Compares the Resource Declaration against another Resource Declaration. The function returns true if both 
        /// Declarations are of the same archetype and have the same name. In all other cases, the function returns false.
        /// </summary>
        /// <param name="other">The Resource Declaration to compare against.</param>
        /// <returns>True if same object, false otherwise.</returns>
        public bool Equals(RESTResourceDeclaration other)
        {
            return other != null && other._archetype == this._archetype && other._name == this._name;
        }

        /// <summary>
        /// Returns a hashcode that is associated with the Resource Declaration. The hash code
        /// is derived from the resource name and archetype.
        /// </summary>
        /// <returns>Hashcode according to Resource Declaration.</returns>
        public override int GetHashCode()
        {
            return this._name.GetHashCode() ^ this._archetype.GetHashCode();
        }

        /// <summary>
        /// Override of compare operator. Two Resource Declaration objects are equal if they are of the same archetype,
        /// have the same name or if they are both NULL.
        /// </summary>
        /// <param name="elementa">First Resource Declaration to compare.</param>
        /// <param name="elementb">Second Resource Declaration to compare.</param>
        /// <returns>True if the Resource Declarations are equal.</returns>
        public static bool operator ==(RESTResourceDeclaration elementa, RESTResourceDeclaration elementb)
        {
            // Tricky to implement correctly. These first statements make sure that we check whether we are actually
            // dealing with identical objects and/or whether one or both are NULL.
            if (ReferenceEquals(elementa, elementb)) return true;
            if (ReferenceEquals(elementa, null)) return false;
            if (ReferenceEquals(elementb, null)) return false;
            return elementa.Equals(elementb);
        }

        /// <summary>
        /// Override of compare operator. Two Resource Declaration objects are different if they have different archetypes,
        /// different names or one of them is NULL..
        /// </summary>
        /// <param name="elementa">First Resource Declaration to compare.</param>
        /// <param name="elementb">Second Resource Declaration to compare.</param>
        /// <returns>True if the Resource Declarations are different.</returns>
        public static bool operator !=(RESTResourceDeclaration elementa, RESTResourceDeclaration elementb)
        {
            return !(elementa == elementb);
        }

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
            this._availableOperationsList = HTTPOperation.GetOperationList("Unknown");
            this._children = new SortedList<string, RESTResourceDeclaration>();
            this._description = string.Empty;
            this._externalDocDescription = string.Empty;
            this._externalDocURL = string.Empty;
            this._documentResourceClass = null;
            this._businessDocumentClassList = new SortedList<string, MEClass>();
            this._tagNames = new List<string>();
            this._disposed = false;
        }

        /// <summary>
        /// This constructor creates an empty, illegal, operation declaration that is linked to its parent Capability.
        /// Since we did not specify a name, we implicitly create an empty collection.
        /// </summary>
        /// <param name="parent">The capability that acts as parent for the operation.</param>
        internal RESTResourceDeclaration(Capability parent)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceDeclaration >> Creating empty declaration.");
            this._name = ContextSlt.GetContextSlt().GetConfigProperty(_EmptyResourceName);
            this._archetype = RESTResourceCapability.ResourceArchetype.Collection;
            this._existingResource = null;
            this._status = this._initialStatus = DeclarationStatus.Invalid;
            this._parent = parent;
            this._parameter = new RESTParameterDeclaration();
            this._operationList = new SortedList<string, RESTOperationDeclaration>();
            this._availableOperationsList = HTTPOperation.GetOperationList("Unknown");
            this._children = new SortedList<string, RESTResourceDeclaration>();
            this._description = string.Empty;
            this._externalDocDescription = string.Empty;
            this._externalDocURL = string.Empty;
            this._documentResourceClass = null;
            this._businessDocumentClassList = new SortedList<string, MEClass>();
            this._tagNames = new List<string>();
            this._disposed = false;
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
            this._availableOperationsList = HTTPOperation.GetOperationList(EnumConversions< RESTResourceCapability.ResourceArchetype>.EnumToString(archetype));
            this._children = new SortedList<string, RESTResourceDeclaration>();
            this._description = string.Empty;
            this._externalDocDescription = string.Empty;
            this._externalDocURL = string.Empty;
            this._documentResourceClass = null;
            this._businessDocumentClassList = new SortedList<string, MEClass>();
            this._tagNames = new List<string>();
            this._disposed = false;
        }

        /// <summary>
        /// This constructor creates a new resource declaration descriptor using an existing Resource Capability.
        /// </summary>
        /// <param name="operation">Operation Capability to use for initialisation.</param>
        internal RESTResourceDeclaration(RESTResourceCapability resource)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResourceDeclaration >> Creating declaration from Capability '" + resource.Name + "'...");
            this._name = resource.Name;
            this._archetype = resource.Archetype;
            this._existingResource = resource.CapabilityClass;
            this._status = this._initialStatus = DeclarationStatus.Stable;
            this._disposed = false;

            // Unfortunately, my parent can be either an Interface or another Resource.
            this._parent = null;
            if (resource.Parent is RESTResourceCapabilityImp) this._parent = new RESTResourceCapability((RESTResourceCapabilityImp)resource.Parent);
            else if (resource.Parent is RESTInterfaceCapabilityImp) this._parent = new RESTInterfaceCapability((RESTInterfaceCapabilityImp)resource.Parent);

            this._parameter = resource.Parameter != null ? new RESTParameterDeclaration(resource.Parameter) : null; // Make sure to create a deep copy in order to avoid conflicts on edit.
            this._operationList = new SortedList<string, RESTOperationDeclaration>();
            this._availableOperationsList = HTTPOperation.GetOperationList(EnumConversions<RESTResourceCapability.ResourceArchetype>.EnumToString(resource.Archetype));
            this._children = new SortedList<string, RESTResourceDeclaration>();
            this._description = MEChangeLog.GetDocumentationAsText(resource.CapabilityClass);
            this._documentResourceClass = null;

            // Make very sure to perform an actual COPY here!
            this._businessDocumentClassList = new SortedList<string, MEClass>();
            foreach (KeyValuePair<string, MEClass> entry in resource.ProfileSet) this._businessDocumentClassList.Add(entry.Key, entry.Value);

            this._tagNames = resource.TagNames;

            // Check whether we have external documentation...
            this._externalDocDescription = string.Empty;
            this._externalDocURL = string.Empty;
            string docClassName = ContextSlt.GetContextSlt().GetConfigProperty(_DocumentationTypeClassName);
            foreach (MEAssociation assoc in resource.CapabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
            {
                if (assoc.Destination.EndPoint.Name == docClassName)
                {
                    foreach (MEAttribute attrib in assoc.Destination.EndPoint.Attributes)
                    {
                        if (attrib.Name == RESTResourceCapabilityImp._DescriptionAttribute) this._externalDocDescription = attrib.FixedValue;
                        else if (attrib.Name == RESTResourceCapabilityImp._URLAttribute) this._externalDocURL = attrib.FixedValue;
                    }
                    break;
                }
            }

            // Retrieve the Operations and update the 'available operations' list as we go...
            foreach (Capability child in resource.Children)
            {
                if (child is RESTOperationCapability)
                {
                    this._operationList.Add(child.Name, new RESTOperationDeclaration(child as RESTOperationCapability));
                    HTTPOperation.RemoveFromOperationList(ref this._availableOperationsList, ((RESTOperationCapability)child).HTTPOperationType);
                }
            }
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

            var newOperation = new RESTOperationDeclaration(parentResource, string.Empty, new HTTPOperation(HTTPOperation.Type.Unknown));
            using (var dialog = new RESTOperationDialog(this._parent.RootService, newOperation, this))
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
                        HTTPOperation.RemoveFromOperationList(ref this._availableOperationsList, newOperation.OperationType);
                    }
                    else if (this._operationList[dialog.Operation.Name].Status == RESTOperationDeclaration.DeclarationStatus.Deleted)
                    {
                        dialog.Operation.Status = RESTOperationDeclaration.DeclarationStatus.Edited;
                        this._operationList[dialog.Operation.Name] = dialog.Operation;
                        if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                        newOperation = dialog.Operation;
                        HTTPOperation.RemoveFromOperationList(ref this._availableOperationsList, newOperation.OperationType);
                    }
                    else
                    {
                        MessageBox.Show("Duplicate operation name, please try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        newOperation = null;
                    }
                }
                else newOperation = null;
            }
            return newOperation;
        }

        /// <summary>
        /// Adds a new profile to the current profile set resource. The function shows the class-picker, which facilitates selection of the
        /// profile 'root' class by the user. The selected class is verified against the context: it must not exist in a profile that is already
        /// registered for this profile set and we allow only a single entry in the 'Basic' profile. Also, the selected profile must be part of
        /// our API scope.
        /// </summary>
        /// <returns>A tuple consisting of the selected profile name (1) and class-name (2). Or NULL on error/cancel.</returns>
        internal Tuple<string, string> AddProfile()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            MEClass selectedClass = context.SelectClass(new List<string> {context.GetConfigProperty(_BusinessComponentStereotype),
                                                                          context.GetConfigProperty(_GenericMessageClassStereotype)});
            string qualifiedClassName = string.Empty;
            string profileStereotype = context.GetConfigProperty(_ProfileStereotype);
            Tuple<string, string> result = null;

            if (selectedClass != null)
            {
                if (this._parent != null)
                {
                    // If we have a parent, we can validate that the selected class is indeed part of my API...
                    // We also have to check whether the selected class has an Alias name defined. If so, we use that name instead of the class name.
                    MEPackage declPackage = this._parent.RootService.DeclarationPkg;
                    if (selectedClass.OwningPackage.Parent == declPackage || selectedClass.OwningPackage.Parent.Parent == declPackage)
                    {
                        qualifiedClassName = string.IsNullOrEmpty(selectedClass.AliasName) ? selectedClass.Name: selectedClass.AliasName;
                    }
                    else MessageBox.Show("Selected component is not part of current API, please try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else qualifiedClassName = string.IsNullOrEmpty(selectedClass.AliasName) ? selectedClass.Name: selectedClass.AliasName;

                // Now that we have a class name (and instance), we check the 'owning' package of the class in order to determine the profile name. 
                // Any owning package that does not have the 'Profile' stereotype is considered to be part of the 'Basic' profile, of which we support 
                // only one single member per profile set!
                if (qualifiedClassName != string.Empty)
                {
                    string profileName = selectedClass.OwningPackage.HasStereotype(profileStereotype) ? selectedClass.OwningPackage.Name : RESTResourceCapability._BasicProfile;

                    if (this._businessDocumentClassList.ContainsKey(profileName))
                    {
                        MessageBox.Show("Selected profile '" + profileName + "' already exists in this set, please try again!",
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        this._businessDocumentClassList.Add(profileName, selectedClass);
                        result = new Tuple<string, string>(profileName, selectedClass.Name);
                        if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                    }
                }
            }
            return result;
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
                else newResource = null;
            }
            return newResource;
        }

        /// <summary>
        /// A helper function that is used by the user resource dialog to clear (linked) contents. When called, we remove all associated 
        /// document classes and/or linked resources (if any are defined) and also clear the class name.
        /// </summary>
        internal void ClearDocuments()
        {
            if (this._businessDocumentClassList != null && this._businessDocumentClassList.Count > 0)
            {
                this._businessDocumentClassList.Clear();
                this._name = string.Empty;
                if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
            }
            if (this._documentResourceClass != null)
            {
                this._documentResourceClass = null;
                this._name = string.Empty;
                if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
            }
        }

        /// <summary>
        /// Removes the parameter definition from the current Resource Declaration;
        /// </summary>
        internal void ClearParameter()
        {
            this._parameter = new RESTParameterDeclaration();
        }

        /// <summary>
        /// Deletes an operation from the list. In fact, the record is not actually removed but marked as 'deleted'.
        /// Since the operation now becomes available as an operation that can be (again) added to the resource, we also update the
        /// 'AvailableOperations' list with the HTTP Operation.
        /// </summary>
        /// <param name="operationName">Name of the Operation to be deleted.</param>
        internal void DeleteOperation(string operationName)
        {
            if (this._operationList.ContainsKey(operationName))
            {
                HTTPOperation operation = this._operationList[operationName].OperationType;
                this._operationList[operationName].Status = RESTOperationDeclaration.DeclarationStatus.Deleted;
                if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                HTTPOperation.AddToOperationList(ref this._availableOperationsList, operation);
            }
        }

        /// <summary>
        /// Removes the named profile from the profile list. When the profile could not be found, the function fails silently and no processing takes place.
        /// </summary>
        /// <param name="profileName">Name of the profile that must be deleted.</param>
        internal void DeleteProfile(string profileName)
        {
            if (this._businessDocumentClassList.ContainsKey(profileName))
            {
                this._businessDocumentClassList.Remove(profileName);
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
            HTTPOperation originalOperation = this._operationList[originalKey].OperationType;
            if (operation != null)
            {
                using (var dialog = new RESTOperationDialog(this._parent.RootService, operation, this))
                {
                    dialog.DisableMinorVersion();
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        if (dialog.Operation.Name == originalKey || !this._operationList.ContainsKey(dialog.Operation.Name))
                        {
                            operation = dialog.Operation;
                            if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;

                            // If we changed the operation type, we have to update our 'available operations' list to reflect this...
                            HTTPOperation newOperation = operation.OperationType;
                            if (originalOperation != newOperation)
                            {
                                HTTPOperation.RemoveFromOperationList(ref this._availableOperationsList, newOperation);
                                HTTPOperation.AddToOperationList(ref this._availableOperationsList, originalOperation);
                            }

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
        /// <returns>The edited resource.</returns>
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
        /// This method assigns an existing Document- or ProfileSet Resource class to the DocumentResourceClass property. It checks the list 
        /// of registered resources of the appropriate type that are maintained at Service level. If there is only one registered resource, it 
        /// is selected automatically. If there are multiple, the method presents the Capability Picker in order to let the user select the 
        /// desired resource.
        /// </summary>
        /// <returns>Name of selected component or empty string in case of errors/cancel.</returns>
        internal string LinkDocumentClass()
        {
            var documentList = new List<Capability>();
            string documentName = string.Empty;
            if (this._archetype == RESTResourceCapability.ResourceArchetype.Document)
            {
                if (((RESTService)this._parent.RootService).DocumentList.Count > 0)
                {
                    // If we only have a single defined Document Resource, this is selected automatically. When there are multiple,
                    // we ask the user which one to use...
                    if (((RESTService)this._parent.RootService).DocumentList.Count == 1)
                    {
                        this._documentResourceClass = ((RESTService)this._parent.RootService).DocumentList[0].CapabilityClass;
                        documentName = this._documentResourceClass.Name;
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
                                    this._documentResourceClass = checkedCapabilities[0].CapabilityClass;
                                    documentName = this._documentResourceClass.Name;
                                    if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                                }
                            }
                            else documentName = string.Empty;
                        }
                    }
                }
                else MessageBox.Show("No suitable Document Resources to select, create one first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (this._archetype == RESTResourceCapability.ResourceArchetype.ProfileSet)
            {
                if (((RESTService)this._parent.RootService).ProfileSetList.Count > 0)
                {
                    // If we only have a single defined ProfileSet Resource, this is selected automatically. When there are multiple,
                    // we ask the user which one to use...
                    if (((RESTService)this._parent.RootService).ProfileSetList.Count == 1)
                    {
                        this._documentResourceClass = ((RESTService)this._parent.RootService).ProfileSetList[0].CapabilityClass;
                        documentName = this._documentResourceClass.Name;
                    }
                    else
                    {
                        foreach (Capability cap in ((RESTService)this._parent.RootService).ProfileSetList) documentList.Add(cap);
                        using (CapabilityPicker dialog = new CapabilityPicker("Select Profile Set resource", documentList, false, false))
                        {
                            if (dialog.ShowDialog() == DialogResult.OK)
                            {
                                List<Capability> checkedCapabilities = dialog.GetCheckedCapabilities();
                                if (checkedCapabilities.Count > 0)
                                {
                                    // If the user selected multiple, we take the first one.
                                    this._documentResourceClass = checkedCapabilities[0].CapabilityClass;
                                    documentName = this._documentResourceClass.Name;
                                }
                            }
                            else documentName = string.Empty;
                        }
                    }
                }
                else MessageBox.Show("No suitable Profile Set Resources to select, create one first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (documentName != string.Empty && this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
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
            RESTParameterDeclaration newParam = this._parameter;
            if (this._archetype == RESTResourceCapability.ResourceArchetype.Identifier)
            {
                bool isCreate = (this._parameter.Status == RESTParameterDeclaration.DeclarationStatus.Invalid);
                if (isCreate) newParam = new RESTParameterDeclaration();
                using (var dialog = new RESTParameterDialog(newParam))
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
                        if (dialog.Parameter.Cardinality.LowerBoundary != 1)
                        {
                            MessageBox.Show("Identifier is mandatory, lower bound adjusted!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            dialog.Parameter.Cardinality = new Cardinality(1, dialog.Parameter.Cardinality.UpperBoundary);
                        }
                        if (dialog.Parameter.Cardinality.IsList)
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
                        newParam = this._parameter;
                    }
                    else newParam = null;
                }
            }
            return newParam;
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
                // We need a resource declaration but these can not be created using an MEClass. However, since we MUST have parsed the
                // entire hierarchy before, we can easily create a Resource Capability (based on the class) and subsequently create the
                // resource declaration from the capability. 
                var newResourceDecl = new RESTResourceDeclaration(new RESTResourceCapability(resourceClass));
                if (newResourceDecl.Status != RESTResourceDeclaration.DeclarationStatus.Invalid)
                {
                    if (!this._children.ContainsKey(newResourceDecl.Name))
                    {
                        newResourceDecl.Status = RESTResourceDeclaration.DeclarationStatus.Created;
                        this._children.Add(newResourceDecl.Name, newResourceDecl);
                    }
                    else if (this._children[newResourceDecl.Name].Status == RESTResourceDeclaration.DeclarationStatus.Deleted)
                    {
                        newResourceDecl.Status = RESTResourceDeclaration.DeclarationStatus.Edited;
                        this._children[newResourceDecl.Name] = newResourceDecl;
                    }
                    else
                    {
                        MessageBox.Show("Duplicate resource name, please try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        newResourceDecl = null;
                    }
                }
                else
                {
                    MessageBox.Show("Invalid resource selected, please try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    newResourceDecl = null;
                }
                if (newResourceDecl != null && this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                return newResourceDecl;
            }
            return null;
        }

        /// <summary>
        /// This method is invoked from the Resource Dialog when the user wants to assign a business component to a Document resource, or
        /// a series of business components to a ProfileSet. The method facilitates selecting of business components and/or creation of new
        /// Profile Sets and applies the necessary checks to assure that input is valid given the current context. In case of Profile Set,
        /// when the set already contains profiles, the function acts as an 'edit' instead of a 'create'.
        /// </summary>
        /// <returns>Name of selected component or empty string in case of errors/cancel. In case of ProfileSet, the returned name is the
        /// name of the created set and not the name of the business component!</returns>
        internal string SetDocumentClass()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string selectedName = string.Empty;
            if (this._businessDocumentClassList == null) this._businessDocumentClassList = new SortedList<string, MEClass>();
            if (this._archetype == RESTResourceCapability.ResourceArchetype.Document)
            {
                const string _Type = "Type";
                string emptyResourceName = context.GetConfigProperty(_EmptyResourceName);
                string profileStereotype = context.GetConfigProperty(_ProfileStereotype);
                MEClass selectedClass = context.SelectClass(new List<string> {context.GetConfigProperty(_BusinessComponentStereotype),
                                                                              context.GetConfigProperty(_GenericMessageClassStereotype)});
                if (selectedClass != null)
                {
                    if (this._parent != null)
                    {
                        // If we have a parent, we can validate that the selected class is indeed part of my API...
                        // We also have to check whether the selected class has an Alias name defined. If so, we use that name instead of the class name.
                        // Last but not least...Document resources MUST be from an explicit- or implicit Basic Profile!
                        MEPackage declPackage = this._parent.RootService.DeclarationPkg;
                        if (selectedClass.OwningPackage.Parent == declPackage || selectedClass.OwningPackage.Parent.Parent == declPackage)
                        {
                            if (!selectedClass.OwningPackage.HasStereotype(profileStereotype) || 
                                selectedClass.OwningPackage.Name == RESTResourceCapability._BasicProfile)
                            {
                                selectedName = !string.IsNullOrEmpty(selectedClass.AliasName) ? selectedClass.AliasName : selectedClass.Name;
                                if (this._businessDocumentClassList.ContainsKey(RESTResourceCapability._BasicProfile))
                                    this._businessDocumentClassList[RESTResourceCapability._BasicProfile] = selectedClass;
                                else this._businessDocumentClassList.Add(RESTResourceCapability._BasicProfile, selectedClass);
                            }
                            else MessageBox.Show("Document resources MUST be part of the Basic Profile, please try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else MessageBox.Show("Selected component is not part of current API, please try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        // If we don't have a (valid) parent, we simply assume that this is a valid selection.
                        // We also have to check whether the selected class has an Alias name defined. If so, we use that name instead of the class name.
                        selectedName = !string.IsNullOrEmpty(selectedClass.AliasName) ? selectedClass.AliasName : selectedClass.Name;
                        if (this._businessDocumentClassList.ContainsKey(RESTResourceCapability._BasicProfile))
                            this._businessDocumentClassList[RESTResourceCapability._BasicProfile] = selectedClass;
                        else this._businessDocumentClassList.Add(RESTResourceCapability._BasicProfile, selectedClass);
                    }

                    if (selectedName != string.Empty)
                    {
                        // Finally, we check whether the name ends with 'Type', if so, we remove this for the name of the resource class...
                        if (selectedName.EndsWith(_Type)) selectedName = selectedName.Substring(0, selectedName.Length - _Type.Length);
                        if (this._name != string.Empty && this._name != selectedName && this._name != emptyResourceName)
                            MessageBox.Show("Document resource '" + this._name + "' receives new name '" +
                                            selectedName + "', please update operation roles accordingly!",
                                            "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        this._name = selectedName;
                        if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                    }
                }
            }
            else if (this._archetype == RESTResourceCapability.ResourceArchetype.ProfileSet)
            {
                // Collect some state in case of user 'cancel'...
                string originalName = this._name;
                var originalStatus = this._status;
                SortedList<string, MEClass> originalList = new SortedList<string, MEClass>();
                foreach (var entry in this._businessDocumentClassList) originalList.Add(entry.Key, entry.Value);

                using (var dialog = new RESTProfileSetDialog(this))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        // At this place, we only have to check changes to the name. All profile set changes are handled by direct 
                        // method invocations (AddProfile, DeleteProfile) from within the dialog.
                        if (dialog.SetName != originalName)
                        {
                            this._name = dialog.SetName;
                            if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                        }
                        selectedName = this._name;
                    }
                    else
                    {
                        // Since the dialog might have added / deleted one or more Profile entries, we must roll-back changes here!
                        this._businessDocumentClassList = new SortedList<string, MEClass>();
                        foreach (var entry in originalList) this._businessDocumentClassList.Add(entry.Key, entry.Value);
                        this._status = originalStatus;
                        this._name = originalName;
                        selectedName = this._name;
                    }
                }
            }
            return selectedName;
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
                if (this._parameter != null && this._parameter.Status != RESTParameterDeclaration.DeclarationStatus.Invalid)
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
        /// Check whether the resource contains an operation with the specified name.
        /// </summary>
        /// <param name="operationName">Operation name to check.</param>
        /// <returns>True if we have this operation, false if not.</returns>
        internal bool HasOperation(string operationName)
        {
            return (this._operationList.ContainsKey(operationName));
        }

        /// <summary>
        /// This is the actual disposing interface, which takes case of structural removal of the implementation type when no longer
        /// needed. Please note that one should invoke explicit 'dispose' calls only on resources that are actually 'owned' by this
        /// object and not just 'referenced'!
        /// </summary>
        /// <param name="disposing">Set to 'true' when called directly. Set to 'false' when called from the finalizer.</param>
        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                try
                {
                    this._parent = null;
                    this._existingResource = null;
                    this._availableOperationsList = null;
                    this._operationList = null;
                    this._children = null;
                    this._parameter = null;
                    this._documentResourceClass = null;
                    this._businessDocumentClassList = null;
                    this._tagNames = null;
                    this._disposed = true;
                }
                catch { };   // Ignore any exceptions, no use in processing them here.
            }
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
        /// We also prune the list of current operations to match the allowed operations for the new type.
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
                else
                {
                    // This is a bit of a hack since it is not possible to delete entries from an array while iterating over it.
                    // So...we first collect a list of all operations that must be deleted and perform the actual delete from another list.
                    // We first loop over all currently defined operations for the resource. If we have an operation that does not match
                    // our new resource type, the operation is removed. And the other way around, if we have an allowed operation, we
                    // remove this from the list of allowed operations so we can use this list to load the set of remaining, valid, operations
                    // for the resource.
                    List<HTTPOperation> allowedOperations = HTTPOperation.GetOperationList(EnumConversions< RESTResourceCapability.ResourceArchetype>.EnumToString(newType));
                    List<RESTOperationDeclaration> illegalList = new List<RESTOperationDeclaration>();
                    foreach (RESTOperationDeclaration operation in this._operationList.Values)
                    {
                        if (!allowedOperations.Contains(operation.OperationType)) illegalList.Add(operation);
                        else allowedOperations.Remove(operation.OperationType);
                    }
                        
                    foreach (RESTOperationDeclaration illegalOperation in illegalList) this._operationList.Remove(illegalOperation.Name);
                    this._availableOperationsList = allowedOperations;
                }

                if (this._initialStatus == DeclarationStatus.Invalid && this._name != string.Empty) this._status = DeclarationStatus.Created;
                else if (this._initialStatus != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
            }
        }
    }
}
