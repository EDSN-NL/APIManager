using System;
using System.Collections.Generic;
using Framework.Exceptions;
using Framework.View;

namespace Framework.Model
{
    /// <summary>
    /// Representation of an UML 'Package' artifact.
    /// </summary>
    internal class MEPackage : ModelElement
    {
        // Result of a lock test on the current package:
        // Unknown = no conclusive result;
        // Locked = locked by another user;
        // LockedByMe = locked by current user;
        // Unlocked = not locked;
        internal enum LockStatus {Unknown, Locked, LockedByMe, Unlocked }

        /// <summary>
        /// Returns the number of classes in the package.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal int ClassCount
        {
            get
            {
                if (this._imp != null) return ((MEIPackage)this._imp).GetClassCount();
                else throw new MissingImplementationException("MEIPackage");
            }
        }

        /// <summary>
        /// Getter for the 'parent' virtual property.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MEPackage Parent
        {
            get
            {
                if (this._imp != null)
                {
                    MEPackage parent = null;
                    MEIPackage parentImp = ((MEIPackage)this._imp).GetParent();
                    if (parentImp != null) parent = new MEPackage(parentImp);
                    return parent;
                }
                else throw new MissingImplementationException("MEIPackage");
            }
        }

        /// <summary>
        /// Construct a new UML 'Package' artifact by associating MEPackage with the appropriate implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="packageID">Unique instance identifier of the package artifact within the tool repository.</param>
        internal MEPackage(int packageID) : base(ModelElementType.Package, packageID) { }

        /// <summary>
        /// Construct a new UML 'Package' artifact by associating MEPackage with the appropriate implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="packageGUID">Globally unique instance identifier of the package artifact.</param>
        internal MEPackage(string packageGUID) : base(ModelElementType.Package, packageGUID) { }

        /// <summary>
        /// Construct a new UML 'Package' artifact by associating MEPackage with the given implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="imp">Package implementation object.</param>
        internal MEPackage(MEIPackage imp) : base(imp) { }

        /// <summary>
        /// Copy constructor creates a new Model Element package from another Model Element package.
        /// </summary>
        /// <param name="copy">Package to use as basis.</param>
        internal MEPackage(MEPackage copy) : base(copy) { }

        /// <summary>
        /// Default constructor creates an empty interface.
        /// </summary>
        internal MEPackage() : base() { }

        /// <summary>
        /// Create a new class instance within the current package with given name and stereotype.
        /// Optionally, a sorting ID can be provided, which is used to specify the order of the new element amidst other
        /// elements in the package. If omitted, the order is defined by the repository (typically, this will imply
        /// alphabetic ordering).
        /// </summary>
        /// <param name="name">The name of the new class.</param>
        /// <param name="stereotype">An optional stereotype, use only if you're creating something that is NOT a business
        /// component (which is the default class type created by this method).</param>
        /// <param name="sortID">Optional ordering ID, can be omitted if not required.</param>
        /// <returns>Newly created class.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MEClass CreateClass(string name, string stereotype, int sortID = -1)
        {
            if (this._imp != null) return ((MEIPackage)this._imp).CreateClass(name, stereotype, sortID);
            else throw new MissingImplementationException("MEIPackage");
        }

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
        /// <returns>Newly created data type interface.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MEDataType CreateDataType(string name, MEDataType.MetaDataType metaType, int sortID = -1)
        {
            if (this._imp != null) return ((MEIPackage)this._imp).CreateDataType(name, metaType, sortID);
            else throw new MissingImplementationException("MEIPackage");
        }

        /// <summary>
        /// Creates a new class diagram object in the current package using the provided name.
        /// </summary>
        /// <param name="name">Name of the new diagram. If omitted, the diagram is named after its parent package.</param>
        /// <returns>Diagram object.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal Diagram CreateDiagram(string name = null)
        {
            if (this._imp != null) return ((MEIPackage)this._imp).CreateDiagram(name);
            else throw new MissingImplementationException("MEIPackage");
        }

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
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MEObject CreateObject(string name, MEClass classifier, List<Tuple<string, string>> runtimeState, int sortID = -1)
        {
            if (this._imp != null) return ((MEIPackage)this._imp).CreateObject(name, classifier, runtimeState, sortID);
            else throw new MissingImplementationException("MEIPackage");
        }

        /// <summary>
        /// Create a new package instance as a child of the current parent and with given name and stereotype.
        /// Optionally, a sorting ID can be provided, which is used to specify the order of the new package amidst the
        /// children of the parent package. If omitted, the order is defined by the repository (typically, this will imply
        /// alphabetic ordering).
        /// Since there are no 'obvious' default stereotypes for packages, the stereotype should be specified at all times!
        /// </summary>
        /// <param name="name">The name of the new package</param>
        /// <param name="stereotype">A mandatory stereotype.</param>
        /// <param name="sortID">Optional ordering ID, can be omitted if not required.</param>
        /// <returns>Newly created package or NULL in case of errors.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MEPackage CreatePackage(string name, string stereotype, int sortID = -1)
        {
            if (this._imp != null) return ((MEIPackage)this._imp).CreatePackage(name, stereotype, sortID);
            else throw new MissingImplementationException("MEIPackage");
        }

        /// <summary>
        /// Delete the specified class from the package. If the class could not be found, the operation fails silently.
        /// </summary>
        /// <param name="thisOne">The class (or data type) to be deleted.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal void DeleteClass(MEClass thisOne)
        {
            if (this._imp != null) ((MEIPackage)this._imp).DeleteClass(thisOne);
            else throw new MissingImplementationException("MEIPackage");
        }

        /// <summary>
        /// Delete the specified object from the package. If the class could not be found, the operation fails silently.
        /// Since the model element is still around, some object properties (e.g. Name, Alias, ElementID, GlobalID) will remain
        /// to be available as long as the object is in scope. However, most other operations on the object will fail!
        /// </summary>
        /// <param name="thisOne">The object to be deleted.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal void DeleteObject(MEObject thisOne)
        {
            if (this._imp != null) ((MEIPackage)this._imp).DeleteObject(thisOne);
            else throw new MissingImplementationException("MEIPackage");
        }

        /// <summary>
        /// Delete specified package from the current package. The package to be deleted mu be a direct child of the
        /// specified package!
        /// </summary>
        /// <param name="package">Package to be deleted.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal void DeletePackage (MEPackage package)
        {
            if (this._imp != null) ((MEIPackage)this._imp).DeletePackage(package);
            else throw new MissingImplementationException("MEIPackage");
        }

        /// <summary>
        /// Export the package (and optional sub-packages) as an XMI file to the specified output path.
        /// </summary>
        /// <param name="fileName">Absolute pathname to output path and file.</param>
        /// <param name="recursive">When true (default) export the entire hierarchy. When false, export only the current package. </param>
        /// <returns>True when exported successfully, false on errors.</returns>
        internal bool ExportPackage(string fileName, bool recursive = true)
        {
            if (this._imp != null) return ((MEIPackage)this._imp).ExportPackage(fileName, recursive);
            else throw new MissingImplementationException("MEIPackage");
        }

        /// <summary>
        /// Searches the package for any class with given name and optional stereotype.
        /// </summary>
        /// <param name="className">Name of class (or class-derived) object to find.</param>
        /// <param name="classStereotype">Optional stereotype of object.</param>
        /// <returns>Class instance found or NULL when not found.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MEClass FindClass(string className, string classStereotype = null)
        {
            if (this._imp != null) return ((MEIPackage)this._imp).FindClass(className, classStereotype);
            else throw new MissingImplementationException("MEIPackage");
        }

        /// <summary>
        /// Searches the package for any class containing the specified name part and/or stereotype.
        /// One or both parameters must be specified. If we have only the name part, the function returns all classes
        /// that contain that name part. If only the stereotype is specified, we return all classes that contain the
        /// stereotype. If both are specified, we return all classes of the specified stereotype that match the name filter.
        /// The list is ordered ascending by class name.
        /// Note that both name- and stereotype are used as filters, not exact matches! If you need an exact match, specify
        /// the 'doExactMatch = true' parameter!
        /// </summary>
        /// <param name="nameFilter">Optional (part of) name to search for.</param>
        /// <param name="stereotype">Optional stereotype of class.</param>
        /// <returns>List of classes found (can be empty).</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal List<MEClass> FindClasses(string nameFilter, string stereotype)   
        {
            if (this._imp != null) return ((MEIPackage)this._imp).FindClasses(nameFilter, stereotype);
            else throw new MissingImplementationException("MEIPackage");
        }

        /// <summary>
        /// Searches the package for any data type with given name and optional stereotype.
        /// </summary>
        /// <param name="typeName">Name of type to find.</param>
        /// <param name="stereotype">Optional stereotype of data type.</param>
        /// <returns>Type instance found or NULL when not found.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MEDataType FindDataType(string typeName, string typeStereotype = null)
        {
            if (this._imp != null) return ((MEIPackage)this._imp).FindDataType(typeName, typeStereotype);
            else throw new MissingImplementationException("MEIPackage");
        }

        /// <summary>
        /// Searches the package for any diagram with given name. If no name is specified, the function returns the first diagram that is present
        /// in the package.
        /// </summary>
        /// <param name="diagramName">Optional name of diagram to find.</param>
        /// <returns>Diagram instance found or NULL when not found.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal Diagram FindDiagram(string diagramName = null)
        {
            if (this._imp != null) return ((MEIPackage)this._imp).FindDiagram(diagramName);
            else throw new MissingImplementationException("MEIPackage");
        }

        /// <summary>
        /// The method searches the current package for the first child with specified name and optional stereotype.
        /// You can have multiple child packages with the same name. In that case, differentiation by Stereotype makes sense.
        /// If stereotype is not specified, we return the first match found. In case of name + stereotype, we return the first
        /// match of both the name and the stereotype.
        /// </summary>
        /// <param name="packageName">Name of child to locate.</param>
        /// <returns>Child package or NULL when not found or on errors.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MEPackage FindPackage(string packageName, string packageStereotype = null)
        {
            if (this._imp != null) return ((MEIPackage)this._imp).FindPackage(packageName, packageStereotype);
            else throw new MissingImplementationException("MEIPackage");
        }

        /// <summary>
        /// Searches the package for any child packages containing the specified name part and/or stereotype.
        /// One or both parameters must be specified. If we have only the name part, the function returns all packages
        /// that contain that name part. If only the stereotype is specified, we return all packages that match the
        /// stereotype. If both are specified, we return all packages of the specified stereotype that match the name filter.
        /// This function ALSO considers the current package for a match.
        /// When parameter 'allLevels' is false, the function only searches the current package. When the parameter is 'true',
        /// the function searches all child packages up till 6 levels 'down'.
        /// The result set is ordered ascending by package name.
        /// </summary>
        /// <param name="nameFilter">Optional (part of) name to search for.</param>
        /// <param name="stereotype">Optional stereotype of class.</param>
        /// <param name="exactMatch">When 'true', the specified names must be matched exactly.</param>
        /// <param name="allLevels">When 'true', we search any levels down, not just direct decendants.</param>
        /// <returns>List of packages found (can be empty).</returns>
        internal List<MEPackage> FindPackages(string nameFilter, string stereotype, bool exactMatch, bool allLevels)
        {
            if (this._imp != null) return ((MEIPackage)this._imp).FindPackages(nameFilter, stereotype, exactMatch, allLevels);
            else throw new MissingImplementationException("MEIPackage");
        }

        /// <summary>
        /// Locate the first parent package of the current package that has the specified stereotype (which can be
        /// the current package itself).
        /// </summary>
        /// <param name="stereotype">Stereotype that we're looking for.</param>
        /// <returns>Parent package with specified stereotype or NULL when not found.</returns>
        internal MEPackage FindParentWithStereotype(string stereotype)
        {
            if (this._imp != null) return ((MEIPackage)this._imp).FindParentWithStereotype(stereotype);
            else throw new MissingImplementationException("MEIPackage");
        }

        /// <summary>
        /// Searches the current package for the existance of a Model Profiler element of which the name contains the specified fragment (case
        /// insensitive). The first matching component is returned (or NULL if nothing found).
        /// When no name is specified, the function returns the first Profiler found in the package.
        /// </summary>
        /// <param name="nameFragment">Optional string that must be part of the name (case insensitive).</param>
        /// <returns>First matching profiler element or NULL if not found.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MEProfiler FindProfiler(string nameFragment = null)
        {
            if (this._imp != null) return ((MEIPackage)this._imp).FindProfiler(nameFragment);
            else throw new MissingImplementationException("MEIPackage");
        }

        /// <summary>
        /// Iterator that returns each class in a package.
        /// </summary>
        /// <returns>Next class</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal IEnumerable<MEClass> GetClasses()
        {
            if (this._imp != null) return ((MEIPackage)this._imp).GetClasses();
            else throw new MissingImplementationException("MEIPackage");
        }

        /// <summary>
        /// Returns a list of all classes in the package that have the specified stereotype. If no stereotype is
        /// specified, the method searches for 'Business Component' classes as well as all data types.
        /// </summary>
        /// <returns>List of all classes in the package (empty list when nothing found)</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal List<MEClass> GetClasses(string stereotype)
        {
            if (this._imp != null) return ((MEIPackage)this._imp).GetClasses(stereotype);
            else throw new MissingImplementationException("MEIPackage");
        }

        /// <summary>
        /// Returns true if the package holds one or more Classes. If optional parameter 'hierarchy' is set to 'true', we check the 
        /// entire package hierarchy, otherwise, we check only the current package.
        /// </summary>
        /// <param name="hierarchy">Optional, default is false. True when entire hierarchy must be checked, starting at current package.</param>
        /// <returns>True when package(hierarchy) contains one or more classes.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal bool HasContents(bool hierarchy = false)
        {
            if (this._imp != null) return ((MEIPackage)this._imp).HasContents(hierarchy);
            else throw new MissingImplementationException("MEIPackage");
        }

        /// <summary>
        /// Import the package from the specified XML file into this package, overwriting all contents.
        /// </summary>
        /// <param name="fileName">Absolute pathname to input path and file.</param>
        /// <returns>True when imported successsfully, false when unable to import.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal bool ImportPackage(string fileName)
        {
            if (this._imp != null) return ((MEIPackage)this._imp).ImportPackage(fileName);
            else throw new MissingImplementationException("MEIPackage");
        }

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
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal bool ImportPackage(string fileName, string parentPackageName, string parentStereotype, string newImportedPackageName = null)
        {
            if (this._imp != null) return ((MEIPackage)this._imp).ImportPackage(fileName, parentPackageName, parentStereotype, newImportedPackageName);
            else throw new MissingImplementationException("MEIPackage");
        }

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
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal LockStatus IsLocked(out string lockedUser)
        {
            if (this._imp != null) return ((MEIPackage)this._imp).IsLocked(out lockedUser);
            else throw new MissingImplementationException("MEIPackage");
        }

        /// <summary>
        /// This method searches the list current package for any names that match the provided 'name'. This can be a package or a class, diagram,
        /// etc. The function supports 'dotted name notation', e.g. qualified names such as 'package.element' can be used.
        /// When any name is detected that matches the given name, the function returns 'false'.
        /// </summary>
        /// <param name="name">Name to be verified.</param>
        /// <returns>True if name is unique, false otherwise.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal bool IsUniqueName(string name)
        {
            if (this._imp != null) return ((MEIPackage)this._imp).IsUniqueName(name);
            else throw new MissingImplementationException("MEIPackage");
        }

        /// <summary>
        /// First check whether the current package is unlocked. If so, create a (recursive) lock for the package and all contents.
        /// If 'recursiveLock' is set to 'false', the function only locks the current package.
        /// </summary>
        /// <param name="recursiveLock">Optional indicator that, when set to 'false', will only lock the current package. Default is 'true', 
        /// which locks the entire package tree.</param>
        /// <returns>True when lock is successfull, false on errors (includes locked by somebody else).</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal bool Lock(bool recursiveLock = true)
        {
            if (this._imp != null) return ((MEIPackage)this._imp).Lock(recursiveLock);
            else throw new MissingImplementationException("MEIPackage");
        }

        /// <summary>
        /// Forces the repository implementation to refresh the current package and all children packages. This can be
        /// called after a number of model changes to assure that the model view is consistent with these changes.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal void RefreshPackage()
        {
            if (this._imp != null) ((MEIPackage)this._imp).RefreshPackage();
            else throw new MissingImplementationException("MEIPackage");
        }

        /// <summary>
        /// Function that repairs the stereotypes of a series of model elements. The function checks whether stereotypes without a profile
        /// name are present and adds the profile if required. Parameter is the fully-qualified stereotype.
        /// The function processes the entire package hierarchy from the current package downwards.
        /// </summary>
        /// <param name="stereotype">Fully qualified stereotype to check.</param>
        /// <param name="entireHierarchy">Optional parameter that enforces a check of current package and all child packages. 
        /// Default is current package only.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal void RepairStereotype(string stereotype, bool entireHierarchy = false)
        {
            if (this._imp != null) ((MEIPackage)this._imp).RepairStereotype(stereotype, entireHierarchy);
            else throw new MissingImplementationException("MEIPackage");
        }

        /// <summary>
        /// Selects the package in the package tree and show to user.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal void ShowInTree()
        {
            if (this._imp != null) ((MEIPackage)this._imp).ShowInTree();
            else throw new MissingImplementationException("MEIPackage");
        }

        /// <summary>
        /// Attempts to unlock the package and all included elements, diagrams and sub-packages. Errors are silently ignored.
        /// When parameter 'currentPackageOnly' is set to 'true', the function only unlocks the current package. When set to
        /// 'false', the package is unlocked recursively, including all sub-packages.
        /// </summary>
        /// <param name="recursiveUnlock">When set to 'true' (the default), unlocks the entire package tree. When set to 'false', we 
        /// only unlock the current package hierarchy.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal void Unlock(bool recursiveUnlock = true)
        {
            if (this._imp != null) ((MEIPackage)this._imp).Unlock(recursiveUnlock);
            else throw new MissingImplementationException("MEIPackage");
        }
    }
}
