using System;
using System.Collections.Generic;
using Framework.Logging;
using Framework.Util;
using Framework.Model;
using Framework.Context;

namespace Plugin.Application.CapabilityModel
{
    /// <summary>
    /// A 'Capability' class represents a feature of a service, something that can be created as part of that
    /// service. Capabilities include model schema's, interface constracts, business message definitions, code lists,
    /// etc. Which capability is valid for which service depends on configuration constraints. The model does not
    /// impose any real restrictions.
    /// Capabilities can contain other capabilities, creating a hierarchical tree which can be processed in order,
    /// starting at the service, then to the 'root' capability and subsequently down to all children.
    /// </summary>
    internal abstract class CapabilityImp : IEquatable<CapabilityImp>
    {
        // Configuration properties related to Capability in general...
        internal static string ServiceCapabilityClassBaseStereotype = "ServiceCapabilityClassBaseStereotype";
        private const string _FileNameTag = "FileNameTag";
        private const string _PathNameTag = "PathNameTag";

        protected Service _rootService;                             // All capabilities 'belong' to a service instance.
        protected MEClass _capabilityClass;                         // Capabilities are always associated with a model element.
        protected string _assignedRole;                             // Contains the role name of the association between the FIRST parent capability and this capability.

        private TreeNode<CapabilityImp> _capabilityTree;            // My node within the hierarchy of capabilities.
        private TreeNode<CapabilityImp> _selectedCapabilityTree;    // Used to build sub-sets for processing.

        /// <summary>
        /// Returns the assigned role of this Capability, which is defined as the role name of the association between the FIRST parent 
        /// capability and the current Capability. It returns an empty string if no role is assigned at all. 
        /// </summary>
        internal string AssignedRole { get { return this._assignedRole; } }

        /// <summary>
        /// Returns the 'author' property of the assigned capability class (if present) or an empty string if Author could not be determined.
        /// </summary>
        internal string Author { get { return (this._capabilityClass != null) ? this._capabilityClass.Author : string.Empty; } }

        /// <summary>
        /// Returns the hierarchy of Capability Implementations of which this Capability is root.
        /// </summary>
        internal TreeNode<CapabilityImp> CapabilityTree { get { return this._capabilityTree; } }

        /// <summary>
        /// The name of the capability is defined by the name of the capability class.
        /// </summary>
        internal string Name { get { return this._capabilityClass.Name; } }

        /// <summary>
        /// Returns the parent Capability of this Capability (if defined). Null indicates no parent.
        /// </summary>
        internal CapabilityImp Parent { get { return (this._capabilityTree.Parent != null) ? this._capabilityTree.Parent.Data : null; } }

        /// <summary>
        /// Returns the associated capability class.
        /// </summary>
        internal MEClass CapabilityClass { get { return this._capabilityClass; } }

        /// <summary>
        /// Returns the unique identifies of the Capability, which is defined as the identifier of the associated capability class.
        /// If the class is not (yet) assigned, the function returns -1, which indicates an illegal identifier.
        /// </summary>
        internal int CapabilityID { get { return (this._capabilityClass != null) ? this._capabilityClass.ElementID : -1; } }

        /// <summary>
        /// Returns the number of child Capabilities that are registered for this Capability.
        /// </summary>
        internal int ChildrenCount { get { return this._capabilityTree.Children.Count; } }

        /// <summary>
        /// Returns the package in which the capability is defined.
        /// </summary>
        internal MEPackage OwningPackage { get { return this._capabilityClass.OwningPackage; } }

        /// <summary>
        /// Returns the root Service object associated with this capability (top of the hierarchy).
        /// </summary>
        internal Service RootService { get { return this._rootService; } }

        /// <summary>
        /// Returns the number of Child Capabilities in the 'selected Capabilities' list.
        /// </summary>
        internal int SelectedChildrenCount { get { return this._selectedCapabilityTree.Children.Count; } }

        /// <summary>
        /// The capability is considered 'valid' when it is associated with a valid capability class.
        /// </summary>
        internal bool Valid { get { return this._capabilityClass != null && this._capabilityClass.Valid; } }

        /// <summary>
        /// Returns the version of the associated Capability class as a string 'majorVs.minorVs'
        /// </summary>
        internal string VersionString { get { return this._capabilityClass.Version.Item1 + "." + this._capabilityClass.Version.Item2; } }

        /// <summary>
        /// Method must be called to attach a child capability to this capability. When no TreeNode yet exists, a new one is created. 
        /// Next, the provided capability is registered as child. 
        /// If the child is already present, the method does not perform any operation (considered Ok).
        /// NOTE: If a Capability has MULTIPLE PARENTS, the parent reference in the child Capability will refer to the LAST parent that
        /// has added the Capability as a child!!! Be aware of side-effects!
        /// </summary>
        /// <param name="child">The child that must be registered.</param>
        internal void AddChild(CapabilityImp child)
        {
            if (!this._capabilityTree.HasChild(child)) this._capabilityTree.AddChild(child._capabilityTree);
        }

        /// <summary>
        /// This method creates a new audit-log entry for the Capability Class. It retrieves the current log, adds an
        /// entry to it and writes the log back to the annotation field of the class.
        /// The method adds the version as a prefix to the log entry.
        /// </summary>
        /// <param name="text">Text to be added to the log.</param>
        internal void CreateLogEntry(string text)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CapabilityImp.createLogEntry >> Adding log entry: '" + text + "'...");
            MEChangeLog.MigrateLog(this.CapabilityClass);
            string annotation = this._capabilityClass.Annotation;

            MEChangeLog newLog = (!string.IsNullOrEmpty(annotation)) ? new MEChangeLog(context.TransformRTF(annotation, RTFDirection.ToRTF)) : new MEChangeLog();
            Tuple<int, int> myVersion = this._capabilityClass.Version;
            string logText = "[" + myVersion.Item1 + "." + myVersion.Item2 + "]: " + text;
            newLog.AddEntry(this._capabilityClass.Author, logText);
            string log = newLog.GetLog();
            this._capabilityClass.Annotation = context.TransformRTF(log, RTFDirection.FromRTF);
        }

        /// <summary>
        /// Deletes all resources in use by the current capability and all sub-capabilities, elements, associations, packages, etc.
        /// The default implementation deletes the capabilityClass and all child classes present in the capability tree. This eventually deletes the
        /// entire capability hierarchy. The method is declared virtual in order for derived classes to choose a different implementation.
        /// Since we have deleted ALL associated model resources, the Capability is INVALID on return from this method!
        /// </summary>
        internal virtual void Delete()
        {
            // First of all, we delete my child capabilities. Not that this will eventually delete the association with my capability, so we do not
            // have to explicitly remove these ourselves.
            if (!Valid) return; // If the associated class has already been deleted we have nothing to do.
            // We must store the list in an intermediate var. since the collection is modified by the delete!
            var childList = new List<TreeNode<CapabilityImp>>(this._capabilityTree.Children);
            foreach (TreeNode<CapabilityImp> cap in childList)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.CapabilityImp.delete >> Recursively deleting child '" + Name + "." + cap.Data.Name + "'...");
                DeleteChild(cap.Data, true);
            }
            this._capabilityClass.OwningPackage.DeleteClass(this._capabilityClass);
            this._capabilityClass = null;
            this._capabilityTree = null;
            this._selectedCapabilityTree = null;
            Capability.UnregisterCapabilityImp(this);   // Remove myself from the registry since the object is now dirty.
        }

        /// <summary>
        /// This method searches the current level of the capability tree for the specified child capability. If found, the node is removed from
        /// the tree and the association with the child is removed as well. If either the child could not be found, or the child association could
        /// not be found, the method fails silently. If the 'deleteResources' indicator is set, the method also deletes the child capability resources
        /// (capability + all sub-capabilities + associated packages, elements, associations, etc.).
        /// </summary>
        /// <param name="child">Child capability to be unlinked.</param>
        /// <param name="deleteResources">Set to true to delete not only the association, but all resources as well.</param>
        internal void DeleteChild(CapabilityImp child, bool deleteResources)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CapabilityImp.deleteChild >> Attempting to remove child '" + child.Name + "'...");
            if (!Valid || child == null) return;        // Ignore operation on invalid context!

            RemoveChild(child);                         // Unlink the capability from our child list.
            if (child.CapabilityClass.Valid)            // Invalid class indicates that is has been deleted already!
            {
                // First of all, we attempt to locate the association between this capability and the child to be deleted...
                MEAssociation childAssoc = null;
                foreach (MEAssociation association in this._capabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
                {
                    if (association.Destination.EndPoint == child.CapabilityClass)
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.CapabilityImp.deleteChild >> Found child association...");
                        childAssoc = association;
                        break;
                    }
                }

                if (!deleteResources)
                {
                    // Just unlink from capability tree and delete the association. Nothing else is touched!
                    if (childAssoc != null)
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.CapabilityImp.deleteChild >> Deleting association...");
                        this._capabilityClass.DeleteAssociation(childAssoc);
                    }
                }
                else if (childAssoc != null)
                {
                    // We only have to delete the child if the association is still there. Otherwise, somebody else probably has already
                    // deleted it...
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.CapabilityImp.deleteChild >> Deleting child resources...");
                    child.Delete();    // This physically deletes all resources recursively.
                }
            }
        }

        /// <summary>
        /// Override method that compares a Capability implementation with another Object..
        /// </summary>
        /// <param name="obj">The thing to compare against.</param>
        /// <returns>True if same object, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var objElement = obj as CapabilityImp;
            return objElement != null && Equals(objElement);
        }

        /// <summary>
        /// Compares one Capability implementation with another, returns true if both have identical capability classes.
        /// </summary>
        /// <param name="other">Capability implementation to compare against.</param>
        /// <returns>TRUE is same object, false otherwise.</returns>
        public bool Equals(CapabilityImp other)
        {
            return (other != null) && (this._capabilityClass == other._capabilityClass);
        }

        /// <summary>
        /// Searches the list of composite associations for a child Capability that has the specified stereotype and optionally, the specified name.
        /// It returns the first match found.
        /// </summary>
        /// <param name="stereoType">Capability must have this stereotype.</param>
        /// <param name="name">Capability optionally must have this name.</param>
        /// <returns></returns>
        internal MEClass FindChildClass(string stereoType, string name)
        {
            try
            {
                foreach (MEAssociation association in this._capabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
                {
                    MEClass targetClass = association.Destination.EndPoint;
                    if (targetClass.HasStereotype(stereoType))
                    {
                        if (!string.IsNullOrEmpty(name))
                        {
                            if (targetClass.Name == name) return targetClass;
                        }
                        else return targetClass;
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.CapabilityImp.findChildClass >> Caught exception: " + exc.Message);
            }
            return null;
        }

        /// <summary>
        /// Searches the list of associations for a child Capability that has the specified name and stereotype.
        /// It returns the role that is associated with the association between current Capability and the found child class.
        /// </summary>
        /// <param name="name">Capability name.</param>
        /// <param name="stereoType">Capability primary stereotype.</param>
        /// <returns>Role name or empty string is not found.</returns>
        internal string FindChildClassRole(string name, string stereoType)
        {
            return this._capabilityClass.FindAssociatedClassRole(name, stereoType);
        }

        /// <summary>
        /// Default implementation that returns the file name (without extension) for this Capability. The extension is left out since this 
        /// typically depends on the chosen serialization mechanism. The filename returned by this method only provides a generic name to 
        /// be used for further, serialization dependent, processing.
        /// If the OperationalStatus of the service is not equal to the default, we also include the OperationalStatus in the filename.
        /// Capabilities that need to create a specialized file name can override the method.
        /// </summary>
        internal virtual string GetBaseFileName()
        {
            Tuple<int, int> version = this.CapabilityClass.Version;
            string postfix = Conversions.ToPascalCase(RootService.IsDefaultOperationalStatus ? string.Empty : "_" + RootService.OperationalStatus); 
            return (this._rootService.UseConfigurationMgmt)? this.Name + postfix:
                                                             this.Name + "_v" + version.Item1 + "p" + version.Item2 + postfix;
        }

        /// <summary>
        /// Derived classes must implement this method, which returns a textual representation of the type of capability (for reporting purposes).
        /// </summary>
        /// <returns>Capability type name.</returns>
        internal abstract string GetCapabilityType();

        /// <summary>
        /// Returns the list of all child capabilities for the current capability. 
        /// </summary>
        /// <returns>List of children of NULL when no children present.</returns>
        internal List<Capability> GetChildren()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CapabilityImp.getChildren >> Retrieving list of child capabilites...");
            var childList = new List<Capability>();
            foreach (TreeNode<CapabilityImp> child in this._capabilityTree.Children)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.CapabilityImp.getChildren >> Adding: " + child.Data.Name);
                childList.Add(child.Data.GetInterface());
            }
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CapabilityImp.getChildren >> No children!");
            return childList;
        }

        /// <summary>
        /// The enumerator system method facilitates direct enumeration through all child capabilities using:
        /// foreach(CapabilityImp child in MyCapabilityImp)...
        /// </summary>
        /// <returns>Next child capability.</returns>
        public IEnumerator<CapabilityImp> GetEnumerator()
        {
            if (this._capabilityTree != null)
            {
                foreach (TreeNode<CapabilityImp> child in this._capabilityTree.Children)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.Capability.capabilityEnumerator >> Returning: " + child.Data.Name);
                    yield return child.Data;
                }
            }
        }

        /// <summary>
        /// Determine a hash of the Capability, which uses the hash function of the capability class.
        /// </summary>
        /// <returns>Hash of Capability</returns>
        public override int GetHashCode()
        {
            return (this._capabilityClass != null) ? this._capabilityClass.GetHashCode() : 0;
        }

        /// <summary>
        /// Returns a Capability Interface object of the appropriate type.
        /// </summary>
        /// <returns>Interface object that is of correct type for the current Implementation.</returns>
        internal abstract Capability GetInterface();

        /// <summary>
        /// This method iterates through the current capability and all children and executes the 
        /// 'handleCapabilities' method for each of them. If one of the calls returns a 'false' indicator, processing 
        /// is aborted. It is the capability-specific counterpart of the Service.handleCapabilities and intented to be used
        /// for those cases where one wants to process only a specific capability set instead of ALL capabilities for a
        /// service.
        /// Capabilities are processed three times, once for pre-processing, once for processing and once for post-processing.
        /// If pre-processing or processing stage returns an error, the entire tree is processed again with 'Cancel' stage
        /// in order to facilitate proper cleanup.
        /// Errors during post-processing are ignored, all capabilities must be processed, no matter whether errors occur.
        /// If an exception is thrown from one of the stages, all capabilities are called again with the 'cancel' stage.
        /// Since this might also happen during cancel processing, a capability must be prepared to process multiple
        /// cancel stages!
        /// This mechanism allows all Capabilities to properly cleanup. It also means that each capability must keep track of 
        /// which stage it already has processed!
        /// </summary>
        /// <returns>Result of all processing, false when a child returns an error or when there are no children.</returns>
        internal virtual bool HandleCapabilities(CapabilityProcessor processor)
        {
            ProcessingStage stage = ProcessingStage.PreProcess;
            if (!Valid) return false;   // Class can not handle any requests.
            bool result = false;
            try
            {
                if (result = HandleCapabilities(processor, stage))      // Pre-processing.
                {
                    stage = ProcessingStage.Process;
                    if (result = HandleCapabilities(processor, stage))  // Processing.
                    {
                        stage = ProcessingStage.PostProcess;
                        return HandleCapabilities(processor, stage);    // Post-processing (not cancelled on errors).
                    }
                }
                //We only end up here on errors!
                stage = ProcessingStage.Cancel;
                HandleCapabilities(processor, stage);
                result = false;
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.CapabilityImp.handleCapabilities >> Exception caught during pre-processing: " + exc.Message);
                HandleCapabilities(processor, ProcessingStage.Cancel);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// This method iterates through all capabilities present in the 'selected capabilities' tree and executes the 'handleCapability'
        /// method for each of them. If one of the calls returns a 'false' indicator, processing is aborted.
        /// The 'stage' parameter indicates which of the stages to process, one of pre-processing, processing or
        /// post-processing. A special cancel stage is used to force cleanup in case of errors.
        /// Normally, this method is invoked from the associated Service object and this also assures that stages
        /// are called in the correct order. It is not advised to invoke the handleCapabilities method directly!
        /// Processing of the chain is aborted on errors, but ONLY during pre- or processing stages. This allows all
        /// child Capabilities to properly cleanup on errors. It also means that each capability must keep track of 
        /// which stage it already has processed!
        /// </summary>
        /// <returns>Result of all processing, false when a child returns an error or when there are no children.</returns>
        internal bool HandleCapabilities(CapabilityProcessor processor, ProcessingStage stage)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CapabilityImp.handleCapabilities >> Processing capabilities one at a time for stage '" + stage + "'...");
            bool result = Valid && HandleCapability(processor, stage);      // Process my own capability first.
            if (result || stage == ProcessingStage.PostProcess || stage == ProcessingStage.Cancel)
            {
                if (this._selectedCapabilityTree != null)
                {
                    foreach (TreeNode<CapabilityImp> child in this._selectedCapabilityTree.Children)
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.CapabilityImp.handleCapabilities >> Processing: " + child.Data.Name);
                        result = child.Data.HandleCapabilities(processor, stage);
                        if (!result && (stage == ProcessingStage.PreProcess || stage == ProcessingStage.Process)) break;
                    }
                }
                else
                {
                    // In this case, we were invoked from a capability that was in the SelectedCapability set, but child capabilities were not.
                    // We now have to swich to the 'ordinary' capability tree to locate children to be processed...
                    foreach (TreeNode<CapabilityImp> child in this._capabilityTree.Children)
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.CapabilityImp.handleCapabilities >> Processing: " + child.Data.Name);
                        result = child.Data.HandleCapabilities(processor, stage);
                        if (!result && (stage == ProcessingStage.PreProcess || stage == ProcessingStage.Process)) break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Perform capability processing for a specified stage.
        /// </summary>
        /// <param name="processor">The Capability processor to use.</param>
        /// <param name="stage">The processing stage that must be executed. Typical processing uses pre-processing,
        /// followed by processing, followed by post-processing. If any capability issued an error during pre- or
        /// processing stages, all capabilities are processed again using a cancel stage.</param>
        /// <returns>True when processing can commence, false on errors (forces abort).</returns>
        internal abstract bool HandleCapability(CapabilityProcessor processor, ProcessingStage stage);

        /// <summary>
        /// The method searches the list of child capabilities for any child capability based on the specified class. 
        /// If found, the method returns true, else false.
        /// </summary>
        /// <param name="thisClass">Capability class to locate.</param>
        /// <returns>True if specified class is a child, false otherwise.</returns>
        internal bool HasChildClass(MEClass thisClass)
        {
            if (thisClass == null) return false;
            foreach (TreeNode<CapabilityImp> child in this._capabilityTree.Children) if (child.Data.CapabilityClass == thisClass) return true;
            return false;
        }

        /// <summary>
        /// This method is called whenever a (new) parent of the Capability has taken ownership of the Capability. The method must be implemented
        /// by derived Capability implementations to perform the necessary registration activities.
        /// </summary>
        /// <param name="parent">The parent Capability that has taken ownership of this Capability.</param>
        internal abstract void InitialiseParent(Capability parent);

        /// <summary>
        /// This method is used to load a series of capabilities in the 'selected capabilities' list of the current
        /// capability. This list is subsequently used for the 'handleCapability' function. 
        /// Note that the method does NOT verify whether the selected capabilties originated from the original 
        /// capability tree of this capability.
        /// Any original set of capabilities in the 'selected capabilties' list is destructed.
        /// </summary>
        /// <param name="selectedChildren">List of capabilities to load.</param>
        internal void LoadSelectedCapabilities(List<CapabilityImp> selectedChildren)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CapabilityImp.loadSelectedCapabilities >> Loading capabilities...");
            this._selectedCapabilityTree = new TreeNode<CapabilityImp>(this);
            foreach (CapabilityImp child in selectedChildren)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.CapabilityImp.loadSelectedCapabilities >> Loading '" + child.Name + "'...");
                this._selectedCapabilityTree.AddChild(child);
            }
        }

        /// <summary>
        /// Override of compare operator. Two Capability Implementation objects are equal if they share the same capability class.
        /// </summary>
        /// <param name="elementa">First Capability to compare.</param>
        /// <param name="elementb">Second Capability to compare.</param>
        /// <returns>True if both elements share the same capability class, false otherwise.
        /// </returns>
        public static bool operator ==(CapabilityImp elementa, CapabilityImp elementb)
        {
            // Tricky to implement correctly. These first statements make sure that we check whether we are actually
            // dealing with identical objects and/or whether one or both are NULL.
            if (ReferenceEquals(elementa, elementb)) return true;
            if (ReferenceEquals(elementa, null)) return false;
            if (ReferenceEquals(elementb, null)) return false;

            // We have two different interface instances, now check whether they share the same capability class....
            return elementa.Equals(elementb);
        }

        /// <summary>
        /// Override of compare operator. Two Capability Implementation objects are different if they have different capability
        /// classes or if one of them is passed as NULL.
        /// </summary>
        /// <param name="elementa">First Capability Implementation to compare.</param>
        /// <param name="elementb">Second Capability Implementation to compare.</param>
        /// <returns>True if both elements have different implementation objects, (or one is missing an implementation 
        /// object), false otherwise.</returns>
        public static bool operator !=(CapabilityImp elementa, CapabilityImp elementb)
        {
            return !(elementa == elementb);
        }

        /// <summary>
        /// This method is the counterpart of 'addChild': it unlinks the specified child Capability from the capability tree. The child object
        /// itself is NOT affected by the operation (other then the parent link in the capability-tree record is removed).
        /// If the child could not be found, the method fails silently.
        /// </summary>
        /// <param name="child">Child capability to be unlinked.</param>
        internal void RemoveChild(CapabilityImp child)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CapabilityImp.removeChild >> Attempting to remove child '" + child.Name + "'...");
            if (!Valid || child == null) return;        // Ignore operation on invalid context!

            if (this._capabilityTree != null)
            {
                if (this._capabilityTree.HasChild(child))
                {
                    this._capabilityTree.DeleteChild(child);
                    child._capabilityTree.Parent = null;
                }
            }
        }

        /// <summary>
        /// Renames the class that is associated with this Capability Implementation. The method is declared virtual in order to facilitate
        /// implementation of capability-specific rename operations.
        /// </summary>
        /// <param name="newName">New name to be assigned to the associated capability class.</param>
        internal virtual void Rename(string newName)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CapabilityImp.rename >> Renaming '" + this._capabilityClass.Name + "' to: " + newName + "'...");
            this._capabilityClass.Name = newName;
        }

        /// <summary>
        /// Override method, returns a string representation of the Capability, which is the name of the capability. On illegal Capabilities (not
        /// having a valid Class element), the method returns "UNKNOWN CAPABILITY".
        /// </summary>
        /// <returns>Capability name.</returns>
        public override string ToString()
        {
            return (this._capabilityClass != null) ? this._capabilityClass.Name : "UNKNOWN CAPABILITY";
        }

        /// <summary>
        /// Recursively traverses the entire capability hierarchy. For each node in this hierarchy, the provided function delegate
        /// is invoked, which receives both the service as well as the current capability as a parameter.
        /// As long as the delegate returns 'false', the traversal continues (until all nodes have been processed). It is therefor
        /// possible to abort traversal by letting the delegate return a 'true' value (as in 'done').
        /// </summary>
        /// <param name="visitor">Action that must be performed on each node.</param>
        /// <returns>True when 'done' with traversal, 'false' if we have to continue.</returns>
        internal virtual bool Traverse(Func<Service, Capability, bool> visitor)
        {
            if (visitor(this._rootService, this.GetInterface())) return true;
            if (this._capabilityTree != null)
            {
                foreach (TreeNode<CapabilityImp> node in this._capabilityTree.Children)
                {
                    if (node.Data.Traverse(visitor)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Functionality is identical to the 'Traverse' function. However, this function starts in the SelectedCapabilities tree and
        /// thus will traverse only a sub-set of the entire hierarchy. The function ONLY works reliably when started at the 
        /// appropriate level in the hierarchy that actually HAS an initialized 'Selected Capabilities' tree!
        /// For each node in the hierarchy, the provided function delegate is invoked, which receives both the service as well as 
        /// the current capability as a parameter.
        /// As long as the delegate returns 'false', the traversal continues (until all nodes have been processed). It is therefor
        /// possible to abort traversal by letting the delegate return a 'true' value (as in 'done').
        /// </summary>
        /// <param name="visitor">Action that must be performed on each node.</param>
        /// <returns>True when 'done' with traversal, 'false' if we have to continue.</returns>
        internal virtual bool TraverseSelected(Func<Service, Capability, bool> visitor)
        {
            if (visitor(this._rootService, this.GetInterface())) return true;
            if (this._selectedCapabilityTree != null)
            {
                foreach (TreeNode<CapabilityImp> node in this._selectedCapabilityTree.Children)
                {
                    if (node.Data.Traverse(visitor)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// This method must be implemented by derived Capability implementations in order to synchronize all child classes with the
        /// major version of the service. It is invoked whenever the major version of the service has been updated.
        /// It can be overloaded by capabilities that require special processing.
        /// </summary>
        internal virtual void VersionSync()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            Tuple<int, int> myVersion = this._capabilityClass.Version;
            int majorVersion = this._rootService.MajorVersion;

            if (myVersion.Item1 < majorVersion)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.CapabilityImp.versionSync >> Updating major version to: " + majorVersion);
                this._capabilityClass.Version = new Tuple<int, int>(majorVersion, 0);
            }
            CreateLogEntry("Version changed to: '" + majorVersion + ".0'.");

            // Synchronize my children (if any)...
            foreach (CapabilityImp child in this) child.VersionSync();
        }

        /// <summary>
        /// Default constructor, used by derived classes for initialization. Note that the capability class itself is not
        /// set by this constructor, we leave this to specialized classes!
        /// <param name="parentService">All capabilities are, directly or indirectly, always associated with a single Service.</param>
        /// </summary>
        protected CapabilityImp(Service parentService)
        {
            this._capabilityTree = new TreeNode<CapabilityImp>(this);
            this._selectedCapabilityTree = null;
            this._assignedRole = string.Empty;
            this._rootService = parentService;
            this._capabilityClass = null;           // As long as this is not set, the object is in invalid state!
        }

        /// <summary>
        /// Helper function that increments the minor version of the Capability (including the associated Service).
        /// </summary>
        protected void UpdateMinorVersion()
        {
            var newVersion = new Tuple<int, int>(RootService.Version.Item1, RootService.Version.Item2 + 1);
            RootService.UpdateVersion(newVersion);
            newVersion = new Tuple<int, int>(this._capabilityClass.Version.Item1, this._capabilityClass.Version.Item2 + 1);
            this._capabilityClass.Version = newVersion;
        }
    }
}
