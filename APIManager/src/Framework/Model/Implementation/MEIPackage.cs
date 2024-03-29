﻿using System;
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
        /// Creates an instance of a class (an object) within the current package with given name. Also, the class that acts
        /// as a classifier for the object must be provided and optionally, a run-time state. The latter is a list of one or more
        /// attribute/value tuples, where the attribute must be a valid attribute from the specified classifier class.
        /// Optionally, a sorting ID can be provided, which is used to specify the order of the new element amidst other
        /// elements in the package. If omitted, the order is defined by the repository (typically, this will imply
        /// alphabetic ordering).
        /// </summary>
        /// <param name="name">The name of the new object.</param>
        /// <param name="classifier">The class for which we must create an object.</param>
        /// <param name="runtimeState">A list of attribute/value tuples to be used to store state of the object. The attribute names MUST exist in classifier.</param>
        /// <param name="sortID">Optional ordering ID, can be omitted if not required.</param>
        /// <returns>Newly created object.</returns>
        /// <exception cref="ArgumentException">Illegal or missing name or classifier or error in run-time state.</exception>
        internal abstract MEObject CreateObject(string name, MEClass classifier, List<Tuple<string, string>> runtimeState, int sortID);

        /// <summary>
        /// Either creates a new class in the current package, or performs an update of an existing class. The function accepts a set of metadata
        /// for the new class: the name, alias name, stereotype, notes and a set of tags. On an existing class, we (re-) assign alias name, notes
        /// and the tag list (we do not check whether additional tags, outside the list, exist). If we don't specify a class to be updated, the
        /// function will search for the class with the specified 'name' (and 'stereotype') and if found, updates that class. When no class is 
        /// specified and we can't find an existing class with the specified name and stereotype, a new class will be created.
        /// </summary>
        /// <param name="name">The name of the class to either create or update.</param>
        /// <param name="aliasName">An optional alias name for the class, specify either null or empty string when not needed.</param>
        /// <param name="stereotype">Mandatory class primary stereotype.</param>
        /// <param name="notes">Optional notes for the class, specify either null or empty string when not needed.</param>
        /// <param name="tagList">List of tags to assign to the class. These are tuples in which the first item is the tag name, the second
        /// the tag value.</param>
        /// <param name="thisClass">When specified, we will force an update of that particular class instead of searching for the class by name.</param>
        /// <returns>Created or updated class reference. The first item in the returned tuple contains the class, the second item
        /// is an indicator that indicates whether we have updated an existing class (false) or created a new class (true).</returns>
        internal abstract Tuple<MEClass,bool> CreateOrUpdateClass(string name, string aliasName, string stereotype, string notes, List<Tuple<string, string>> tagList, MEClass thisClass);

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
        /// Delete the specified object from the package. If the class could not be found, the operation fails silently.
        /// Since the model element is still around, some object properties (e.g. Name, Alias, ElementID, GlobalID) will remain
        /// to be available as long as the object is in scope. However, most other operations on the object will fail!
        /// </summary>
        /// <param name="thisOne">The object to be deleted.</param>
        internal abstract void DeleteObject(MEObject thisOne);

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
        /// Searches the package for any child packages containing the specified name part and/or stereotype.
        /// One or both parameters must be specified. If we have only the name part, the function returns all packages
        /// that contain that name part. If only the stereotype is specified, we return all packages that match the
        /// stereotype. If both are specified, we return all packages of the specified stereotype that match the name filter.
        /// The search is only at the level of the current package, that is, we don't search multiple levels down!
        /// The result set is ordered ascending by package name.
        /// </summary>
        /// <param name="nameFilter">Optional (part of) name to search for.</param>
        /// <param name="stereotype">Optional stereotype of class.</param>
        /// <param name="exactMatch">When 'true', the specified names must be matched exactly.</param>
        /// <param name="allLevels">When 'true', we search any levels down, not just direct decendants.</param>
        /// <returns>List of packages found (can be empty).</returns>
        internal abstract List<MEPackage> FindPackages(string nameFilter, string stereotype, bool exactMatch, bool allLevels);

        /// <summary>
        /// Locate the first parent package of the current package that has the specified stereotype (which can be
        /// the current package itself).
        /// </summary>
        /// <param name="stereotype">Stereotype that we're looking for.</param>
        /// <returns>Parent package with specified stereotype or NULL when not found.</returns>
        internal abstract MEPackage FindParentWithStereotype(string stereotype);

        /// <summary>
        /// Searches the package for any class with given name and optional stereotype.
        /// </summary>
        /// <param name="className">Name of class (or class-derived) object to find.</param>
        /// <param name="classStereotype">Optional stereotype of object.</param>
        /// <returns>Class instance found or NULL when not found.</returns>
        internal abstract MEClass FindClass(string className, string classStereotype);

        /// <summary>
        /// Searches the package for any class containing the specified name part and/or stereotype.
        /// One or both parameters must be specified. If we have only the name part, the function returns all classes
        /// that contain that name part. If only the stereotype is specified, we return all classes that contain the
        /// stereotype. If both are specified, we return all classes of the specified stereotype that match the name filter.
        /// The list is ordered ascending by class name. The function uses an approximate search, returning all classes
        /// of which the name (or alias) matches either the entire name filter or contain the name filter.
        /// By setting 'useAlias' to 'true', the function uses the Alias Name of the class instead of the normal name.
        /// </summary>
        /// <param name="nameFilter">Optional (part of) name to search for.</param>
        /// <param name="stereotype">Optional stereotype of class.</param>
        /// <param name="useAliasName">When 'true' use the class Alias Name instead of 'ordinary' Name.</param>
        /// <returns>List of classes found (can be empty).</returns>
        internal abstract List<MEClass> FindClasses(string nameFilter, string stereotype, bool useAliasName);

        /// <summary>
        /// Searches the package for any class containing the specified tag name and (optional) tag value. If the value
        /// is not specified, we return all classes in the package that have the specified tag name, irrespective of contents.
        /// If the tag value is specified as well, we only return classes that contain the specified tag AND an exact
        /// match on the value of that tag.
        /// </summary>
        /// <param name="tagName">The tag name we're looking for.</param>
        /// <param name="tagValue">Optional value for the tag (exact match).</param>
        /// <returns>List of classes found (can be empty).</returns>
        internal abstract List<MEClass> FindClassesByTag(string tagName, string tagValue);

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
        /// Iterator that returns each class in a package.
        /// </summary>
        /// <returns>Next class</returns>
        internal abstract IEnumerable<MEClass> GetClasses();

        /// <summary>
        /// Returns a list of all classes in the package that have the specified stereotype. If no stereotype is
        /// specified, the method searches for 'Business Component' classes.
        /// </summary>
        /// <returns>List of all classes in the package (empty list when nothing found)</returns>
        internal abstract List<MEClass> GetClasses(string stereotype);

        /// <summary>
        /// Returns the number of classes in the package.
        /// </summary>
        /// <returns>Number of classes in the package.</returns>
        internal abstract int GetClassCount();

        /// <summary>
        /// Iterator that returns each child package of the current package.
        /// </summary>
        /// <returns>Next package</returns>
        internal abstract IEnumerable<MEPackage> GetPackages();

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
        /// First check whether the current package is unlocked. If so, create a (recursive) lock for the package and all contents.
        /// If 'recursiveLock' is set to 'false', the function only locks the current package.
        /// </summary>
        /// <param name="recursiveLock">Optional indicator that, when set to 'false', will only lock the current package. Default is 'true', 
        /// which locks the entire package tree.</param>
        /// <returns>True when lock is successfull, false on errors (includes locked by somebody else).</returns>
        internal abstract bool Lock(bool recursiveLock);

        /// <summary>
        /// Forces the repository implementation to refresh the current package and all children packages. This can be
        /// called after a number of model changes to assure that the model view is consistent with these changes.
        /// </summary>
        internal abstract void RefreshPackage();

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
        /// When parameter 'currentPackageOnly' is set to 'true', the function only unlocks the current package. When set to
        /// 'false', the package is unlocked recursively, including all sub-packages.
        /// </summary>
        /// <param name="recursiveUnlock">When set to 'true' (the default), unlocks the entire package tree. When set to 'false', we 
        /// only unlock the current package hierarchy.</param>
        internal abstract void Unlock(bool recursiveUnlock);

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