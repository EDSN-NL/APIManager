using System.Collections.Generic;
using Framework.Util;
using Framework.View;

namespace Framework.Model
{
    /// <summary>
    /// Model Element Implementation Package adds another layer of abstraction between the generic Model Element Implementation
    /// and the tool-specific implementation. This facilitates implementation of Model Element type-specific methods at this layer
    /// without the bridge interface needing tool-specific implementation logic.
    /// For packages, this layer is used to implement the package tree, which is generic in nature.
    /// </summary>
    internal abstract class MEIPackage : ModelElementImplementation
    {
        private TreeNode<MEIPackage> _packageTreeNode;   // Implements the package tree.

        // The orphan list is used to keep children waiting for their parent package to appear so they can be linked...
        private static SortedList<int, List<int>> _orphanList = new SortedList<int, List<int>>();

        /// <summary>
        /// Create a new class instance within the current package with given name and stereotype.
        /// Optionally, a sorting ID can be provided, which is used to specify the order of the new element amidst other
        /// elements in the package. If omitted, the order is defined by the repository (typically, this will imply
        /// alphabetic ordering).
        /// </summary>
        /// <param name="name">The name of the new class.</param>
        /// <param name="stereotype">An optional stereotype, pass NULL or an empty string if none is required.</param>
        /// <param name="sortID">Optional ordering ID, can be set to -1 if not required.</param>
        /// <returns>Newly created class.</returns>
        internal abstract MEClass CreateClass(string name, string stereotype, int sortID);

        /// <summary>
        /// Create a new data type instance within the current package with given name.
        /// Optionally, a sorting ID can be provided, which is used to specify the order of the new element amidst other
        /// elements in the package. If omitted, the order is defined by the repository (typically, this will imply
        /// alphabetic ordering).
        /// The method will assign the correct primary stereotype according to the provided meta type. If additional 
        /// stereotypes are required, these have to be assigned in subsequent calls!
        /// </summary>
        /// <param name="name">The name of the new data type.</param>
        /// <param name="metaType">Defines the exact data type that must be created.</param>
        /// <param name="sortID">Optional ordering ID, can be omitted if not required.</param>
        /// <returns>Newly created class.</returns>
        internal abstract MEDataType CreateDataType(string name, MEDataType.MetaDataType metaType, int sortID);

        /// <summary>
        /// Creates a new diagram object in the current package using the provided name.
        /// </summary>
        /// <param name="name">Name of the new diagram.</param>
        /// <returns>Diagram object or NULL in case of errors.</returns>
        internal abstract Diagram CreateDiagram(string name);

        /// <summary>
        /// Create a new package instance as a child of the current package and with given name and stereotype.
        /// Optionally, a sorting ID can be provided, which is used to specify the order of the new package amidst the
        /// children of the parent package. If omitted, the order is defined by the repository (typically, this will imply
        /// alphabetic ordering).
        /// </summary>
        /// <param name="name">The name of the new package</param>
        /// <param name="stereotype">An optional stereotype, pass NULL or an empty string if none is required.</param>
        /// <param name="sortID">Optional ordering ID, can be set to -1 if not required.</param>
        /// <returns>Newly created package.</returns>
        internal abstract MEPackage CreatePackage(string name, string stereotype, int sortID);

        /// <summary>
        /// Delete the specified class from the package. If the class could not be found, the operation fails silently.
        /// </summary>
        /// <param name="thisOne">The class (or data type) to be deleted.</param>
        internal abstract void DeleteClass(MEClass thisOne);

        /// <summary>
        /// Delete specified package from the current package. The package to be deleted mu be a direct child of the
        /// specified package!
        /// </summary>
        /// <param name="package">Package to be deleted.</param>
        internal abstract void DeletePackage(MEPackage package);

        /// <summary>
        /// Export the package (and optional sub-packages) as an XMI file to the specified output path.
        /// </summary>
        /// <param name="fileName">Absolute pathname to output path and file.</param>
        /// <returns>True when exported successfully, false on errors.</returns>
        internal abstract bool ExportPackage(string fileName, bool recursive);

        /// <summary>
        /// The method searches the current package for the first child with specified name and optional stereotype.
        /// You can have multiple child packages with the same name. In that case, differentiation by Stereotype makes sense.
        /// If stereotype is not specified, we return the first match found. In case of name + stereotype, we return the first
        /// match of both the name and the stereotype.
        /// </summary>
        /// <param name="childName">Name of child to locate.</param>
        /// <param name="packageStereotype">Optional stereotype of package.</param>
        /// <returns>Child package implementation or NULL when not found or on errors.</returns>
        internal abstract MEPackage FindPackage(string childName, string packageStereotype);

        /// <summary>
        /// Searches the package for any class with given name and optional stereotype.
        /// </summary>
        /// <param name="className">Name of class (or class-derived) object to find.</param>
        /// <param name="classStereotype">Optional stereotype of object.</param>
        /// <returns>Class instance found or NULL when not found.</returns>
        internal abstract MEClass FindClass(string className, string classStereotype);

        /// <summary>
        /// Searches the package for any diagram with given name.
        /// </summary>
        /// <param name="diagramName">Name of type to find.</param>
        /// <returns>Diagram instance found or NULL when not found.</returns>
        internal abstract Diagram FindDiagram(string diagramName);

        /// <summary>
        /// Searches the package for any data type with given name and optional stereotype.
        /// </summary>
        /// <param name="typeName">Name of type to find.</param>
        /// <param name="stereotype">Optional stereotype of data type.</param>
        /// <returns>Type instance found or NULL when not found.</returns>
        internal abstract MEDataType FindDataType(string typeName, string stereotype);

        /// <summary>
        /// Searches the current package for the existance of a Model Profiler element of which the name contains the specified fragment (case
        /// insensitive). The first matching component is returned (or NULL if nothing found).
        /// When no name is specified, the function returns the first Profiler found in the package.
        /// </summary>
        /// <param name="nameFragment">Optional string that must be part of the name (case insensitive).</param>
        /// <returns>First matching profiler element or NULL if not found.</returns>
        internal abstract MEProfiler FindProfiler(string nameFragment);

        /// <summary>
        /// Returns a list of all classes in the package that have the specified stereotype. If no stereotype is
        /// specified, the method searches for 'Business Component' classes.
        /// </summary>
        /// <returns>List of all classes in the package (empty list when nothing found)</returns>
        internal abstract List<MEClass> GetClasses(string stereotype = null);

        /// <summary>
        /// Returns the number of classes in the package.
        /// </summary>
        /// <returns>Number of classes in the package.</returns>
        internal abstract int GetClassCount();

        /// <summary>
        /// Returns the parent MEIPackage instance of this package, or NULL if no parent known.
        /// </summary>
        /// <returns>Parent package object or NULL if no parent (yet) known.</returns>
        internal MEIPackage GetParent()
        {
            return ((this._packageTreeNode != null) && (this._packageTreeNode.Parent != null)) ? this._packageTreeNode.Parent.Data : GetParentExplicit();
        }

        /// <summary>
        /// Returns true if the package holds one or more Classes. If 'hierarchy' is set to 'true', we check the entire package
        /// hierarchy, otherwise, we check only the current package.
        /// </summary>
        /// <param name="hierarchy">True when entire hierarchy must be checked, starting at current package.</param>
        /// <returns>True when package(hierarchy) contains one or more classes.</returns>
        internal abstract bool HasContents(bool hierarchy);

        /// <summary>
        /// Import the package from the specified XML file into this package, overwriting all contents.
        /// </summary>
        /// <param name="fileName">Absolute pathname to input path and file.</param>
        /// <returns>True when imported successsfully, false when unable to import.</returns>
        internal abstract bool ImportPackage(string fileName);

        /// <summary>
        /// Import the package from the specified XML file into a new package underneath the current package. The new parent package
        /// for the import is specified by 'parentPackageName'. If the container package does not yet exist, it is created first.
        /// The imported package has all it's GUID's replaced by new instances so that the import does not conflict with possible
        /// existing packages. Also, if the imported package contains any Profilers, these will be reloaded (as long as we can locate a
        /// donor package with the same name as the newly imported package. If not, the Profilers are left alone for the user to fix).
        /// For te Profile reload to work, the original package (which has been exported to XMI), MUST be a direct child package of 
        /// the current package. Also, we copy Profilers FROM the existing package (if found) TO the newly imported package. So, if
        /// the package structures have changed, not all Profilers might have been correctly copied over so this method works best
        /// for a full package copy (export to XMI - import from XMI at other location).
        /// Unlike the 'other' ImportPackage, which overwrites the current package, this import variant thus imports 'underneath' the
        /// current package. Therefore, we don't make assumptions on the name of the imported package.
        /// </summary>
        /// <param name="fileName">Absolute pathname to input path and file.</param>
        /// <param name="parentPackageName">Package that will act as the parent for the imported package. Will be created if not already
        /// present.</param>
        /// <param name="parentStereotype">The stereotype to be assigned to the parent package.</param>
        /// <param name="newImportedPackageName">Optionally, one can specify a new name for the imported package.</param>
        /// <returns>True when imported successsfully, false when unable to import.</returns>
        internal abstract bool ImportPackage(string fileName, string parentPackageName, string parentStereotype, string newImportedPackageName);

        /// <summary>
        /// Test whether the package is currently locked. We do this by checking for the presence of a lock for the current user.
        /// When no user is found, we assume that security is not enabled and the package is thus unlocked.
        /// When a user is found, we check for a lock for the current package. This can have three different results:
        /// 1) No entries are found at all, package is unlocked;
        /// 2) Lock is found for current user;
        /// 3) Lock is found for another user;
        /// Note that we ONLY check whether the package is locked and NOT the contents of the package!
        /// </summary>
        /// <param name="lockedUser">Will receive the name of the user who has locked the package.</param>
        /// <returns>Lock status, one of Unlocked, LockedByMe or Locked.</returns>
        internal abstract MEPackage.LockStatus IsLocked(out string lockedUser);

        /// <summary>
        /// This method searches the list current package for any names that match the provided 'name'. This can be a package or a class, diagram,
        /// etc. The function supports 'dotted name notation', e.g. qualified names such as 'package.element' can be used.
        /// When any name is detected that matches the given name, the function returns 'false'.
        /// </summary>
        /// <param name="name">Name to be verified.</param>
        /// <returns>True if name is unique, false otherwise.</returns>
        internal abstract bool IsUniqueName(string name);

        /// <summary>
        /// Attempts to lock the package and all included elements, diagrams and sub-packages.
        /// </summary>
        /// <returns>True when lock is successfull, false on errors (includes locked by somebody else).</returns>
        internal abstract bool Lock();

        /// <summary>
        /// Forces the repository implementation to refresh the current package and all children packages. This can be
        /// called after a number of model changes to assure that the model view is consistent with these changes.
        /// </summary>
        internal abstract void Refresh();

        /// <summary>
        /// Function that repairs the stereotypes of a series of model elements. The function checks whether stereotypes without a profile
        /// name are present and adds the profile if required. Parameter is the fully-qualified stereotype.
        /// The function processes the entire package hierarchy from the current package downwards.
        /// </summary>
        /// <param name="stereotype">Fully qualified stereotype to check.</param>
        /// <param name="entireHierarchy">Optional parameter that enforces a check of current package and all child packages. 
        /// Default is current package only.</param>
        internal abstract void RepairStereotype(string stereotype, bool entireHierarchy);

        /// <summary>
        /// Selects the package in the package tree and show to user.
        /// </summary>
        internal abstract void ShowInTree();

        /// <summary>
        /// Attempts to unlock the package and all included elements, diagrams and sub-packages. Errors are silently ignored.
        /// </summary>
        internal abstract void Unlock();

        /// <summary>
        /// Method that explicitly searches the model repository for the parent implementation of the current package.
        /// We use this method when we attempt to find a parent package that has not (yet) been registered.
        /// </summary>
        /// <returns>Parent implementation package or NULL when non-existing or on errors.</returns>
        protected abstract MEIPackage GetParentExplicit();

        /// <summary>
        /// This method creates an entry in the package tree for the current package. If a parent is specified as well,
        /// the method attempts to link this package to the parent package. If the parent not (yet) exists, the request is kept
        /// in the orphan list until the parent is registered. This might never happen, but the orphan list is pretty small,
        /// only a set of identifiers.
        /// </summary>
        /// <param name="packageID">Current package that must be registered.</param>
        /// <param name="parentID">Optional identifier of the parent package. Set to -1 to indicate that it's not used.</param>
        protected void RegisterPackage(int packageID, int parentID)
        {
            if (parentID != -1)
            {
                // Package has a known parent, let's see if we can find it...
                var parent = (MEIPackage)_model.FindRegisteredElementImp(ModelElementType.Package, parentID);
                if (parent != null)
                {
                    TreeNode<MEIPackage> parentNode = parent._packageTreeNode;
                    this._packageTreeNode = new TreeNode<MEIPackage>(this, parentNode);
                    parentNode.AddChild(this);
                }
                else
                {
                    // Parent is specified, but not yet registered (so no interfaces using it).
                    // This COULD mean that nobody is interested in this parent, or it has simply not yet instantiated.
                    // We add our package to the orphan list for this parent until the parent appears, in which case all
                    // orphans will be properly linked (see further).
                    this._packageTreeNode = new TreeNode<MEIPackage>(this);     // For the time being, our parent is NULL.
                    if (_orphanList.ContainsKey(parentID))
                    {
                        if (!_orphanList[parentID].Contains(packageID)) _orphanList[parentID].Add(packageID);
                    }
                    else
                    {
                        var orphans = new List<int>
                        {
                            packageID
                        };
                        _orphanList.Add(parentID, orphans);
                    }
                }
            }
            else this._packageTreeNode = new TreeNode<MEIPackage>(this);

            // Check is we happen to be a parent that is present in the orphan list, i.e. children are waiting for me...
            if (_orphanList.ContainsKey(packageID))
            {
                foreach(int childID in _orphanList[packageID])
                {
                    var child = (MEIPackage)_model.FindRegisteredElementImp(ModelElementType.Package, childID);
                    if (child != null)  // Could be absent since it went out of scope since last registration. Silently ignore.
                    {
                        this._packageTreeNode.AddChild(child);
                        child._packageTreeNode.Parent = this._packageTreeNode;
                    }
                }
                _orphanList.Remove(packageID);  // Get rid of this list of waiting children.
            }
        }

        /// <summary>
        /// Default constructor, mainly used to pass the model instance to the base constructor and set the correct type.
        /// </summary>
        /// <param name="model">Reference to the associated model implementation object.</param>
        protected MEIPackage (ModelImplementation model): base(model)
        {
            this._type = ModelElementType.Package;
        }
    }
}