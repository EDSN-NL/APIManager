using System;
using System.Collections.Generic;
using Framework.Logging;
using Framework.Model;
using Framework.Exceptions;
using Framework.Util;

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
    internal abstract class Capability : IDisposable, IEquatable<Capability>
    {
        protected CapabilityImp _imp;   // Associated implementation object.

        private static SortedList<int, CapabilityImp> _capabilityList = new SortedList<int, CapabilityImp>(); // Capability registry.
        private bool _disposed;         // Mark myself as invalid after call to dispose!

        /// <summary>
        /// Returns the assigned role of this Capability, which is defined as the role name of the association between the FIRST parent 
        /// capability and the current Capability. It returns an empty string if no role is assigned at all (or no implementation exists). 
        /// </summary>
        internal string AssignedRole { get { return (this._imp != null) ? this._imp.AssignedRole : string.Empty; } }

        /// <summary>
        /// Returns the 'author' property of the assigned capability class (if present) or an empty string if Author could not be determined.
        /// </summary>
        internal string Author { get { return (this._imp != null) ? this._imp.Author : string.Empty; } }

        /// <summary>
        /// This returns the unique identification of the Capability. It is derived from the elementID of the associated Capability Class.
        /// A value of '-1' indicates an invalid ID.
        /// </summary>
        internal int CapabilityID { get { return (this._imp != null) ? this._imp.CapabilityID : -1; } }

        /// <summary>
        /// Returns the associated capability class.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal MEClass CapabilityClass
        {
            get
            {
                if (this._imp != null) return this._imp.CapabilityClass;
                else throw new MissingImplementationException("CapabilityImp");
            }
        }

        /// <summary>
        /// Returns the package in which the capability is defined.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal MEPackage OwningPackage
        {
            get
            {
                if (this._imp != null) return this._imp.OwningPackage;
                else throw new MissingImplementationException("CapabilityImp");
            }
        }

        /// <summary>
        /// Returns the hierarchy of Capability Implementations of which this Capability is root.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal TreeNode<CapabilityImp> CapabilityTree
        {
            get
            {
                if (this._imp != null) return this._imp.CapabilityTree;
                else throw new MissingImplementationException("CapabilityImp");
            }
        }

        /// <summary>
        /// Returns a textual description of the Capability for reporting purposes.
        /// </summary>
        /// <returns>Capability type name.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal string CapabilityType
        {
            get
            {
                if (this._imp != null) return this._imp.GetCapabilityType();
                else throw new MissingImplementationException("CapabilityImp");
            }
        }

        /// <summary>
        /// Retrieve a list of all Capability implementations that are registered as child of the current capability.
        /// </summary>
        /// <returns>List of child Capability Implementations</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal List<Capability> Children
        {
            get
            {
                if (this._imp != null) return this._imp.GetChildren();
                else throw new MissingImplementationException("CapabilityImp");
            }
        }

        /// <summary>
        /// Returns the number of child Capabilities that are registered for this Capability.
        /// </summary>
        /// <returns>Total number of registered Child Capabilities.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal int ChildrenCount
        {
            get
            {
                if (this._imp != null) return this._imp.ChildrenCount;
                else throw new MissingImplementationException("CapabilityImp");
            }
        }

        /// <summary>
        /// Returns the file name (without extension) for this Capability. The extension is left out since this typically depends on the
        /// chosen serialization mechanism. The filename returned by this method only provides a generic name to be used for further, serialization
        /// dependent, processing.
        /// </summary>
        /// <returns>Total number of registered Child Capabilities.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal string BaseFileName
        {
            get
            {
                if (this._imp != null) return this._imp.GetBaseFileName();
                else throw new MissingImplementationException("CapabilityImp");
            }
        }

        /// <summary>
        /// Returns the associated capability implementation object.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal CapabilityImp Implementation
        {
            get
            {
                if (this._imp != null) return this._imp;
                else throw new MissingImplementationException("CapabilityImp");
            }
        }

        /// <summary>
        /// Returns a list of all attributes of the class. It ONLY returns the attributes for the current class, NOT for any
        /// specialized (root) classes!
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal string Name
        {
            get
            {
                if (this._imp != null) return this._imp.Name;
                else throw new MissingImplementationException("CapabilityImp");
            }
        }

        /// <summary>
        /// Returns the parent Capability implementation of this Capability (if defined). Null indicates no parent.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal CapabilityImp Parent
        {
            get
            {
                if (this._imp != null) return this._imp.Parent;
                else throw new MissingImplementationException("CapabilityImp");
            }
        }

        /// <summary>
        /// Returns the root Service object associated with this capability (top of the hierarchy).
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal Service RootService
        {
            get
            {
                if (this._imp != null) return this._imp.RootService;
                else throw new MissingImplementationException("CapabilityImp");
            }
        }

        /// <summary>
        /// Returns the number of child Capabilities that are currently selected for processing by this Capability.
        /// </summary>
        /// <returns>Number of selected Child Capabilities.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal int SelectedChildrenCount
        {
            get
            {
                if (this._imp != null) return this._imp.SelectedChildrenCount;
                else throw new MissingImplementationException("CapabilityImp");
            }
        }

        /// <summary>
        /// The capability is considered 'valid' when it is not disposed, is associated with a capability class and that class reports valid state.
        /// </summary>
        internal bool Valid
        {
            get
            {
                return !this._disposed && this._imp != null && this._imp.Valid;
            }
        }

        /// <summary>
        /// Returns the version of the associated Capability class as a string 'majorVs.minorVs'
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal string VersionString
        {
            get
            {
                if (this._imp != null) return this._imp.VersionString;
                else throw new MissingImplementationException("CapabilityImp");
            }
        }

        /// <summary>
        /// Method must be called to attach a child capability to this capability. When no TreeNode yet exists, a new one is created. 
        /// Next, the provided capability is registered as child. 
        /// </summary>
        /// <param name="child">The child that must be registered.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal void AddChild(Capability child)
        {
            if (this._imp != null) this._imp.AddChild(child._imp);
            else throw new MissingImplementationException("CapabilityImp");
        }

        /// <summary>
        /// This method creates a new audit-log entry for the Capability Class. It retrieves the current log, adds an
        /// entry to it and writes the log back to the annotation field of the class.
        /// The method adds the version as a prefix to the log entry.
        /// </summary>
        /// <param name="text">Text to be added to the log.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal void CreateLogEntry(string text)
        {
            if (this._imp != null) this._imp.CreateLogEntry(text);
            else throw new MissingImplementationException("CapabilityImp");
        }

        /// <summary>
        /// Deletes all resources in use by the current capability and all sub-capabilities, elements, associations, packages, etc.
        /// The default implementation deletes the capabilityClass and all child classes present in the capability tree. This eventually deletes the
        /// entire capability hierarchy. The method is declared virtual in order for derived classes to choose a different implementation.
        /// Since we have deleted ALL associated model resources, the Capability is INVALID on return from this method!
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal void Delete()
        {
            if (this._imp != null)
            {
                this._imp.Delete();
                Dispose();
            }
            else throw new MissingImplementationException("CapabilityImp");
        }

        /// <summary>
        /// This method searches the current level of the capability tree for the specified child capability. If found, the node is removed from
        /// the tree and the association with the child is removed as well. If either the child could not be found, or the child association could
        /// not be found, the method fails silently. If the 'deleteResources' indicator is set, the method also deletes the child capability resources
        /// (capability + all sub-capabilities + associated packages, elements, associations, etc.).
        /// </summary>
        /// <param name="child">Child capability to be unlinked.</param>
        /// <param name="deleteResources">Set to true to delete not only the association, but all resources as well. Default value is false.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal void DeleteChild(Capability child, bool deleteResources = false)
        {
            if (this._imp != null) this._imp.DeleteChild(child._imp, deleteResources);
            else throw new MissingImplementationException("CapabilityImp");
        }

        /// <summary>
        /// This is the normal entry for all users of the object that want to indicate that the interface is not required anymore.
        /// Use with extreme caution since references to the interface might still be around, which will not work anymore after calling
        /// Dispose!
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Override method that compares a Capability with another Object..
        /// </summary>
        /// <param name="obj">The thing to compare against.</param>
        /// <returns>True if same object, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var objElement = obj as Capability;
			return objElement != null && Equals(objElement);
        }

        /// <summary>
        /// Compares one Capability with another, with returns true if both have identical capability classes.
        /// </summary>
        /// <param name="other">Capability to compare against.</param>
        /// <returns>TRUE is same object, false otherwise.</returns>
        public bool Equals(Capability other)
        {
            if (other != null && this._imp != null && other._imp != null)
            {
                return (this.CapabilityID == other.CapabilityID);
            }
            else return false;
        }

        /// <summary>
        /// Searches the list of composite associations for a child Capability that has the specified stereotype and optionally, the specified name.
        /// It returns the associated class for the first match found.
        /// </summary>
        /// <param name="stereoType">Capability must have this stereotype.</param>
        /// <param name="name">Capability optionally must have this name.</param>
        /// <returns>First matching child class or NULL when nothing found.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal MEClass FindChildClass(string stereoType, string name = null)
        {
            if (this._imp != null) return this._imp.FindChildClass(stereoType, name);
            else throw new MissingImplementationException("CapabilityImp");
        }

        /// <summary>
        /// Searches the list of composite associations for a child Capability that has the specified name and primary stereotype.
        /// It returns the role that is associated with the association between current Capability and the found child class.
        /// </summary>
        /// <param name="name">Capability must have this name.</param>
        /// <param name="stereoType">Capability must have this stereotype.</param>
        /// <returns>Role name or empty string is not found.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal string FindChildClassRole(string name, string stereoType)
        {
            if (this._imp != null) return this._imp.FindChildClassRole(name, stereoType);
            else throw new MissingImplementationException("CapabilityImp");
        }

        /// <summary>
        /// Flushes the contents of the Capability Registry. Note that this has NO effect on existing implementations!
        /// </summary>
        internal static void FlushRegistry() { _capabilityList.Clear(); }

        /// <summary>
        /// Determine a hash of the Capability, which uses the hash function of the capability class.
        /// Missing implementations always return a hash value 0.
        /// </summary>
        /// <returns>Hash of Capability</returns>
        public override int GetHashCode()
        {
            if (this._imp != null) return this._imp.GetHashCode();
            else return 0;
        }

        /// <summary>
        /// Process the capability (i.e. generate output according to provided processor.).
        /// </summary>
        /// <param name="processor">The Capability processor to use.</param>
        /// <param name="stage">The processing stage we're currently in, passed verbatim to processor.</param>
        /// <returns>True when processing can commence, false on errors.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal bool HandleCapability(CapabilityProcessor processor, ProcessingStage stage)
        {
            if (this._imp != null) return (this._imp.HandleCapability(processor, stage));
            else throw new MissingImplementationException("CapabilityImp");
        }

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
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal virtual bool HandleCapabilities(CapabilityProcessor processor)
        {
            if (this._imp != null) return this._imp.HandleCapabilities(processor);
            else throw new MissingImplementationException("CapabilityImp");
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
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal bool HandleCapabilities(CapabilityProcessor processor, ProcessingStage stage)
        {
            if (this._imp != null) return this._imp.HandleCapabilities(processor, stage);
            else throw new MissingImplementationException("CapabilityImp");
        }

        /// <summary>
        /// The method searches the list of child capabilities for any child capability based on the specified class. 
        /// If found, the method returns true, else false.
        /// </summary>
        /// <param name="thisClass">Capability class to locate.</param>
        /// <returns>True if specified class is a child, false otherwise.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal bool HasChildClass(MEClass thisClass)
        {
            if (this._imp != null) return this._imp.HasChildClass(thisClass);
            else throw new MissingImplementationException("CapabilityImp");
        }

        /// <summary>
        /// This method is called whenever a (new) parent of the Capability has taken ownership of this Capability. It can be implemented by specialized
        /// Capability Implementations that require parent-specific initialization.
        /// </summary>
        /// <param name="parent">The parent Capability that has taken ownership of this Capability.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal void InitialiseParent(Capability parent)
        {
            if (this._imp != null) this._imp.InitialiseParent(parent);
            else throw new MissingImplementationException("CapabilityImp");
        }

        /// <summary>
        /// This method is used to load a series of capabilities in the 'selected capabilities' list of the current
        /// capability. This list is subsequently used for the 'handleCapability' function. 
        /// Note that the method does NOT verify whether the selected capabilities originated from the original 
        /// capability tree of this capability.
        /// Any original set of capabilities in the 'selected capabilties' list is destructed.
        /// </summary>
        /// <param name="selectedChildren">List of capabilities to load.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal void LoadSelectedCapabilities(List<Capability> selectedChildren)
        {
            if (this._imp != null)
            {
                var impList = new List<CapabilityImp>();
                foreach (Capability cap in selectedChildren) if (cap != null && cap._imp != null) impList.Add(cap._imp);
                this._imp.LoadSelectedCapabilities(impList);
            }
            else throw new MissingImplementationException("CapabilityImp");
        }

        /// <summary>
        /// Override of compare operator. Two Capability objects are equal if they share the same capability class.
        /// </summary>
        /// <param name="elementa">First Capability to compare.</param>
        /// <param name="elementb">Second Capability to compare.</param>
        /// <returns>True if both elements share the same capability class, false otherwise.
        /// </returns>
        public static bool operator ==(Capability elementa, Capability elementb)
        {
            // Tricky to implement correctly. These first statements make sure that we check whether we are actually
            // dealing with identical objects and/or whether one or both are NULL.
            if (ReferenceEquals(elementa, elementb)) return true;
            if (ReferenceEquals(elementa, null)) return false;
            if (ReferenceEquals(elementb, null)) return false;

            // We have two different interface instances, now check whether they share the same Capability class....
            return elementa.Equals(elementb);
        }

        /// <summary>
        /// Override of compare operator. Two ModelElement objects are different if they have different Capability
        /// classes or if one of them is passed as NULL.
        /// </summary>
        /// <param name="elementa">First ModelElement to compare.</param>
        /// <param name="elementb">Second ModelElement to compare.</param>
        /// <returns>True if both elements have different implementation objects, (or one is missing an implementation 
        /// object), false otherwise.</returns>
        public static bool operator !=(Capability elementa, Capability elementb)
        {
            return !(elementa == elementb);
        }

        /// <summary>
        /// This method is the counterpart of 'addChild': it unlinks the specified child Capability from the capability tree. The child object
        /// itself is NOT affected by the operation (other then the parent link in the capability-tree record is removed).
        /// If the child could not be found, the method fails silently.
        /// </summary>
        /// <param name="child">The child that must be registered.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal void RemoveChild(Capability child)
        {
            if (this._imp != null) this._imp.RemoveChild(child._imp);
            else throw new MissingImplementationException("CapabilityImp");
        }

        /// <summary>
        /// Renames the class that is associated with this capability. The method is declared virtual in order to facilitate
        /// implementation of capability-specific rename operations.
        /// </summary>
        /// <param name="newName">New name to be assigned to the associated capability class.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal void Rename (string newName)
        {
            if (this._imp != null) this._imp.Rename(newName);
            else throw new MissingImplementationException("CapabilityImp");
        }

        /// <summary>
        /// Override method, returns a string representation of the Capability, which is the name of the capability.
        /// In case of missing implementation, the function returns the text 'NO IMPLEMENTATION'.
        /// </summary>
        /// <returns>Capability name.</returns>
        public override string ToString()
        {
            if (this._imp != null) return this._imp.ToString();
            else return "NO IMPLEMENTATION";
        }

        /// <summary>
        /// Recursively traverses the entire capability implementation hierarchy. For each node in this hierarchy, the provided function delegate
        /// is invoked, which receives both the service as well as the current capability as a parameter.
        /// As long as the delegate returns 'false', the traversal continues (until all nodes have been processed). It is therefor
        /// possible to abort traversal by letting the delegate return a 'true' value (as in 'done').
        /// </summary>
        /// <param name="visitor">Action that must be performed on each node.</param>
        /// <returns>True when 'done' with traversal, 'false' if we have to continue.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal bool Traverse(Func<Service, Capability, bool> visitor)
        {
            if (this._imp != null) return this._imp.Traverse(visitor);
            else throw new MissingImplementationException("CapabilityImp");
        }

        /// <summary>
        /// This static method can be used to remove the specified Capability Implementation from the registry, which will enforce a re-create of 
        /// the associated resource structure when reference next time. Please note that this not affect current Interfaces that still use the
        /// specified Implementation object!
        /// </summary>
        /// <param name="thisImp">Implementation to be removed.</param>
        internal static void UnregisterCapabilityImp(CapabilityImp thisImp)
        {
            if (_capabilityList.ContainsKey(thisImp.CapabilityID)) _capabilityList.Remove(thisImp.CapabilityID);
        }

        /// <summary>
        /// This method is used to synchronize the major version of the capability with its parent service in case that version has changed.
        /// If we detect a major update, the minor version is reset to '0'! 
        /// The method ONLY considers the service major version, minor version of the capability is independent of the Service!
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal void VersionSync()
        {
            if (this._imp != null) this._imp.VersionSync();
            else throw new MissingImplementationException("CapabilityImp");
        }

        /// <summary>
        /// Default constructor, used by derived classes for initialization on new instances. This constructor simply sets all local properties
        /// to an initialized state!
        /// </summary>
        protected Capability()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.Capability >> Initializing Capability");
            this._disposed = false;
            this._imp = null;
        }

        /// <summary>
        /// Constructor that creates a new Interface based on the specified implementation.
        /// </summary>
        /// <param name="thisImp">Implementation to be used for this Interface.</param>
        protected Capability (CapabilityImp thisImp)
        {
            this._disposed = false;
            this._imp = thisImp;
        }

        /// <summary>
        /// Copy constructor, creates an Interface that uses the properties of the specified Interface (same dispose status and implementation object).
        /// </summary>
        /// <param name="other">Interface to copy.</param>
        protected Capability (Capability other)
        {
            this._disposed = other._disposed;
            this._imp = other._imp;
        }

        /// <summary>
        /// Constructor used for initialization of existing Capabilities (might not be registered yet). The constructor attempts to load
        /// an implementation with given ID. If it's not in the registry yet, all properties are set to default state and the base class remains
        /// invalid. It's the task of the derived class to create the proper implementation and register this in the registry.
        /// </summary>
        protected Capability(int capabilityID)
        {
            this._disposed = false;
            if (!_capabilityList.TryGetValue(capabilityID, out this._imp)) this._imp = null;
        }

        /// <summary>
        /// Constructor that creates an interface based on an existing implementation. The implementation is identified by its Capability Class.
        /// The implementation must exist in the registry, otherwise the constructor fails and throws a 'MissingImplementation' Exception.
        /// If we specify a NULL object, the constructor behaves like the default constructor.
        /// </summary>
        /// <param name="capabilityClass">Capability class of the implementation we're looking for.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        protected Capability(MEClass capabilityClass)
        {
            this._disposed = false;
            this._imp = null;
            if (capabilityClass != null)
            {
                if (_capabilityList.ContainsKey(capabilityClass.ElementID))
                {
                    this._imp = _capabilityList[capabilityClass.ElementID];
                }
                else
                {
                    Logger.WriteError("Plugin.Application.CapabilityModel.Capability (MEClass) >> Capability implementation not found!");
                    throw new MissingImplementationException("CapabilityImp");
                }
            }
        }


        /// <summary>
        /// The destructor is declared as a safeguard to assure that the reference counter is decreased when the object is
        /// garbage collected. It's not failsafe since the moment of invocation is not guaranteed.
        /// </summary>
        ~Capability()
        {
            Dispose(false);
        }

        /// <summary>
        /// This is the actual disposing interface, which takes case of structural removal of the implementation type when no longer
        /// needed.
        /// </summary>
        /// <param name="disposing">Set to 'true' when called directly. Set to 'false' when called from the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                try
                {
                    if (this._imp != null)
                    {
                        _capabilityList.Remove(this._imp.CapabilityClass.ElementID);
                        this._imp = null;
                    }
                    this._disposed = true;
                }
                catch { };   // Ignore any exceptions, no use in processing them here.
            }
        }

        /// <summary>
        /// The method is invoked by specialized classes after creating a new Capability implementation. The method registers the implementation in the
        /// Capability registry and initializes the implementation reference.
        /// Please note that it's the reponsibility of the derived class to assure that no duplicate implementations are created. If an attempt is made
        /// to register an implementation that already exists with the assigned capabilityID, a duplicate keyan argument exception will be thrown!
        /// </summary>
        /// <param name="thisImp">Newly created Capability implementation object.</param>
        /// <exception cref="ArgumentException">Capability implementation with given identifier already exists.</exception>
        protected void RegisterCapabilityImp (CapabilityImp thisImp)
        {
            _capabilityList.Add(thisImp.CapabilityClass.ElementID, thisImp);
            this._imp = thisImp;
        }

        /// <summary>
        /// This method is used by specialized classes that want to 'clone' an existing interface. The method copies registration from the provided
        /// interface and thus effectively creates a second Capability Interface associated with the same implementation as the provided interface.
        /// </summary>
        /// <param name="itf">Interface to 'clone'</param>
        protected void RegisterCapabilityItf(Capability itf)
        {
            if (_capabilityList.ContainsKey(itf.CapabilityID))
            {
                this._imp = _capabilityList[itf.CapabilityID];
            }
            else
            {
                this._imp = null;
                Logger.WriteError("Plugin.Application.CapabilityModel.Capability (Itf) >> Capability implementation not found!");
                throw new MissingImplementationException("CapabilityImp");
            }
        }
    }
}
