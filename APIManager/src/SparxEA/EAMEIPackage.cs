using System;
using System.Xml;
using System.Collections.Generic;
using EA;
using Framework.Context;
using Framework.Logging;
using Framework.Model;
using Framework.Controller;

namespace SparxEA.Model
{
    /// <summary>
    /// Represents a 'package' within Sparx EA. A package can contain Classes, other Packages and/or Diagrams.
    /// </summary>
    internal sealed class EAMEIPackage: MEIPackage
    {
        // Configuration properties used to differentiate between different (business-) data types...
        private const string _SimpleBDTStereotype           = "SimpleBDTStereotype";
        private const string _ComplexBDTStereotype          = "ComplexBDTStereotype";
        private const string _BDTUnionStereotype            = "BDTUnionStereotype";
        private const string _BDTEnumStereotype             = "BDTEnumStereotype";
        private const string _LogicalDataTypeStereotype     = "LogicalDataTypeStereotype";
        private const string _PrimitiveDataTypeStereotype   = "PrimitiveDataTypeStereotype";
        private const string _BusinessComponentStereotype   = "BusinessComponentStereotype";

        // Configuration properties used to distinguish between different classifier [meta-]types...
        private const string _MetaTypeComplexDataType       = "MetaTypeComplexDataType";
        private const string _MetaTypeSimpleDataType        = "MetaTypeSimpleDataType";
        private const string _MetaTypeEnumeration           = "MetaTypeEnumeration";
        private const string _MetaTypeUnion                 = "MetaTypeUnion";

        private EA.Package _package;                        // EA Package representation.
        private bool _isLocked;                             // Keep track of successfull locking.

        // Provide access to the EA instance for use by partner entities...
        internal EA.Package PackageInstance           { get { return this._package; } }

        /// <summary>
        /// Creates a new EA package implementation from a repository identifier.
        /// </summary>
        /// <param name="model">The associated model implementation.</param>
        /// <param name="packageID">The EA Package identifier on which the implementation is based.</param>
        internal EAMEIPackage(EAModelImplementation model, int packageID): base(model)
        {
            this._package = model.Repository.GetPackageByID(packageID);
            if (this._package != null)
            {
                this._name = this._package.Name;
                this._elementID = packageID;
                this._globalID = this._package.PackageGUID;
				this._aliasName = this._package.Alias ?? string.Empty;
                this._isLocked = false;

                // Register this package in the package tree. If the package has a parent, we attempt to link to it...
                // In EA an empty package ID is represented by value '0', we use '-1' instead.
                int parentID = this._package.ParentID;
                RegisterPackage(packageID, parentID == 0 ? -1 : parentID);
            }
            else
            {
                Logger.WriteError("SparxEA.Model.EAMEIPackage >> Failed to retrieve EA Package with ID: " + packageID);
            }
        }

        /// <summary>
        /// Creates a new EA package implementation from a repository global identifier (GUID).
        /// </summary>
        /// <param name="model">The associated model implementation.</param>
        /// <param name="packageGUID">The EA Package global identifier (GUID) on which the implementation is based.</param>
        internal EAMEIPackage(EAModelImplementation model, string packageGUID) : base(model)
        {
            this._package = model.Repository.GetPackageByGuid(packageGUID);
            if (this._package != null)
            {
                this._name = this._package.Name;
                this._elementID = this._package.PackageID;
                this._globalID = packageGUID;
                this._aliasName = this._package.Alias ?? string.Empty;
                this._isLocked = false;

                // Register this package in the package tree. If the package has a parent, we attempt to link to it...
                // In EA an empty package ID is represented by value '0', we use '-1' instead.
                int parentID = this._package.ParentID;
                RegisterPackage(this._elementID, parentID == 0 ? -1 : parentID);
            }
            else
            {
                Logger.WriteError("SparxEA.Model.EAMEIPackage >> Failed to retrieve EA Package with GUID: " + packageGUID);
            }
        }

        /// <summary>
        /// Constructor that creates a new implementation instance based on a provided EA package instance.
        /// </summary>
        /// <param name="model">The associated model implementation.</param>
        /// <param name="package">The EA Package on which the implementation is based.</param>
        internal EAMEIPackage(EAModelImplementation model, EA.Package package): base(model)
        {
            this._package = package;
            if (this._package != null)
            {
                this._name = this._package.Name;
                this._elementID = this._package.PackageID;
                this._globalID = this._package.PackageGUID;
				this._aliasName = this._package.Alias ?? string.Empty;
                this._isLocked = false;

                // Register this package in the package tree. If the package has a parent, we attempt to link to it...
                // In EA an empty package ID is represented by value '0', we use '-1' instead.
                int parentID = this._package.ParentID;
                RegisterPackage(this._elementID, parentID == 0 ? -1 : parentID);
            }
            else
            {
                Logger.WriteError("SparxEA.Model.EAMEIPackage >> Constructor based on empty package!");
            }
        }

        /// <summary>
        /// Assignes the specified stereotype to this package.
        /// </summary>
        /// <param name="stereotype">Stereotype to be assigned.</param>
        internal override void AddStereotype(string stereotype)
        {
            if (!string.IsNullOrEmpty(stereotype))
            {
                if (this._package.Element == null)
                {
                    Logger.WriteError("SparxEA.Model.EAMEIPackage.AddStereotype >> Package '" + this._package.Name + "' not yet fully initialized!");
                    return;
                }
                string stereoTypes = this._package.Element.StereotypeEx;
                if (!this._package.Element.HasStereotype(stereotype))
                {
                    stereoTypes += (stereoTypes.Length > 0) ? "," + stereotype : stereotype;
                    this._package.Element.StereotypeEx = stereoTypes;
                    this._package.Element.Update();
                }
            }
        }

        /// <summary>
        /// Create a new class instance within the current package with given name and stereotype.
        /// Optionally, a sorting ID can be provided, which is used to specify the order of the new element amidst other
        /// elements in the package. If omitted, the order is defined by the repository (typically, this will imply
        /// alphabetic ordering).
        /// </summary>
        /// <param name="name">The name of the new class.</param>
        /// <param name="stereotype">An optional stereotype, pass NULL or an empty string if none is required.</param>
        /// <param name="sortID">Optional ordering ID, can be omitted if not required.</param>
        /// <returns>Newly created class.</returns>
        /// <exception cref="ArgumentException">Illegal or missing name.</exception>
        internal override MEClass CreateClass(string name, string stereotype, int sortID)
        {
            if (string.IsNullOrEmpty(name))
            {
                string message = "SparxEA.Model.EAMEIPackage.createClass >> Attempt to create a class without name in package '" + this.Name + "'!";
                Logger.WriteError(message);
                throw new ArgumentException(message);
            }

            // Prevent the 'AddNew' + 'Update' to generate scope switches until we're ready for them...
            ControllerSlt.GetControllerSlt().EnableScopeSwitch = false;

            var newElement = this._package.Elements.AddNew(name, "Class") as EA.Element;
            newElement.Update();        // Update immediately to properly finish the create. Note that this triggers a Scope Switch to incomplete object!
            this._package.Elements.Refresh();
            bool needUpdate = false;

            if (!string.IsNullOrEmpty(stereotype))
            {
                newElement.StereotypeEx = stereotype;
                needUpdate = true;
            }

            if (sortID != -1)
            {
                newElement.TreePos = sortID;
                needUpdate = true;
            }

            if (needUpdate) newElement.Update();
            ControllerSlt.GetControllerSlt().EnableScopeSwitch = true;
            return new MEClass(newElement.ElementID);
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
        /// <returns>Newly created data type.</returns>
        /// <exception cref="ArgumentException">Illegal meta type or missing name.</exception>
        internal override MEDataType CreateDataType(string name, MEDataType.MetaDataType metaType, int sortID)
        {
            if (string.IsNullOrEmpty(name))
            {
                string message = "SparxEA.Model.EAMEIPackage.createDataType >> Attempt to create a data type without name in package '" + this.Name + "'!";
                Logger.WriteError(message);
                throw new ArgumentException(message);
            }
            
            // Prevent the 'AddNew' + 'Update' to generate scope switches until we're ready for them...
            ControllerSlt.GetControllerSlt().EnableScopeSwitch = false;

            var newElement = this._package.Elements.AddNew(name, "DataType") as EA.Element;
            newElement.Update();        // Update immediately to properly finish the create.
            this._package.Elements.Refresh();

            ContextSlt context = ContextSlt.GetContextSlt();
            string stereotype;
            MEDataType returnType;
            switch (metaType)
            {
                case MEDataType.MetaDataType.ComplexType:
                case MEDataType.MetaDataType.ExtSchemaType:
                    stereotype = context.GetConfigProperty(_ComplexBDTStereotype);
                    var complexImp = new EAMEIDataType((EAModelImplementation)this._model, newElement, metaType);
                    returnType = new MEDataType(complexImp);
                    break;

                case MEDataType.MetaDataType.SimpleType:
                    stereotype = context.GetConfigProperty(_SimpleBDTStereotype);
                    var simpleImp = new EAMEIDataType((EAModelImplementation)this._model, newElement, metaType);
                    returnType = new MEDataType(simpleImp);
                    break;

                case MEDataType.MetaDataType.Enumeration:
                    stereotype = context.GetConfigProperty(_BDTEnumStereotype);
                    returnType = new MEEnumeratedType(newElement.ElementID);
                    break;

                case MEDataType.MetaDataType.Union:
                    stereotype = context.GetConfigProperty(_BDTUnionStereotype);
                    returnType = new MEUnionType(newElement.ElementID);
                    break;

                default:
                    string msg = "SparxEA.Model.EAMEIPackage.createDataType >> Illegal meta type '" + metaType + "' passed to package '" +
                                 this._name + " when creating data type '" + name + "'!";
                    Logger.WriteError(msg);
                    ControllerSlt.GetControllerSlt().EnableScopeSwitch = true;
                    throw new ArgumentException(msg);
            }
            newElement.StereotypeEx = stereotype;
            if (sortID != -1) newElement.TreePos = sortID;
            newElement.Update();
            ControllerSlt.GetControllerSlt().EnableScopeSwitch = true;
            return returnType;
        }

        /// <summary>
        /// Creates a new logical diagram object in the current class using the provided name.
        /// </summary>
        /// <param name="diagramName">Name of the new diagram. If omitted, the diagram is named after its parent.</param>
        /// <returns>Diagram object.</returns>
        internal override Framework.View.Diagram CreateDiagram(string diagramName)
        {
            if (string.IsNullOrEmpty(diagramName)) diagramName = this.Name;
           
            // Prevent the 'AddNew' + 'Update' to generate scope switches until we're ready for them...
            ControllerSlt.GetControllerSlt().EnableScopeSwitch = false;

            var diagram = this._package.Diagrams.AddNew(diagramName, "Logical") as EA.Diagram;
            diagram.Update();
            this._package.Diagrams.Refresh();
            ControllerSlt.GetControllerSlt().EnableScopeSwitch = true;
            return new Framework.View.Diagram(diagram.DiagramID);
        }

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
        /// <exception cref="ArgumentException">Empty name passed.</exception>
        internal override MEPackage CreatePackage(string name, string stereotype, int sortID)
        {
            if (string.IsNullOrEmpty(name))
            {
                string message = "SparxEA.Model.EAMEIPackage.createPackage >> Attempt to create a package without name in package '" + this.Name + "'!";
                Logger.WriteError(message);
                throw new ArgumentException(message);
            }

            // Prevent the 'AddNew' + 'Update' to generate scope switches until we're ready for them...
            ControllerSlt.GetControllerSlt().EnableScopeSwitch = false;

            var newPackage = this._package.Packages.AddNew(name, "Package") as EA.Package;
            newPackage.Update();    // Update immediately to properly finish the creation!
            newPackage.Element.Update();
            this._package.Packages.Refresh();
            bool needUpdate = false;

            if (!string.IsNullOrEmpty(stereotype))
            {
                newPackage.Element.StereotypeEx = stereotype;
                newPackage.Element.Update();
            }

            if (sortID != -1)
            {
                newPackage.TreePos = sortID;
                needUpdate = true;
            }

            if (needUpdate) newPackage.Update();
            newPackage.Update();
            newPackage.Element.Update();
            ControllerSlt.GetControllerSlt().EnableScopeSwitch = true;
            return new MEPackage(newPackage.PackageID);
        }

        /// <summary>
        /// Delete the specified class from the package. If the class could not be found, the operation fails silently.
        /// Since the model element is still around, some class properties (e.g. Name, Alias, ElementID, GlobalID) will remain
        /// to be available as long as the class is in scope. However, most other operations on the class will fail!
        /// </summary>
        /// <param name="thisOne">The class (or data type) to be deleted.</param>
        internal override void DeleteClass(MEClass thisOne)
        {
            Logger.WriteInfo("SparxEA.Model.EAMEIPackage.deleteClass >> Deleting class '" + thisOne.Name + "' from package '" + this._package.Name + "' with '" + this._package.Elements.Count + "' elements...");

            this._package.Elements.Refresh();   // Make sure that we're looking at the most up-to-date state.
            for (short i = 0; i < this._package.Elements.Count; i++)
            {
                Logger.WriteInfo("SparxEA.Model.EAMEIPackage.deleteClass >> Examining index: " + i);
                var currElement = this._package.Elements.GetAt(i) as EA.Element;
                Logger.WriteInfo("SparxEA.Model.EAMEIPackage.deleteClass >> Found element: '" + currElement.Name + "'...");
                if (currElement.ElementID == thisOne.ElementID)
                {
                    this._package.Elements.DeleteAt(i, true); // Refresh options currently does not work.
                    this._package.Elements.Refresh();
                    thisOne.InValidate();                     // Make sure to mark the associated implementation as invalid!
                    return;
                }
            }
            Logger.WriteWarning("SparxEA.Model.EAMEIPackage.deleteClass >> Attempt to delete Class '" + thisOne.Name +
                                "' from Package: '" + this._package.Name + "' failed; Class not found!");
        }

        /// <summary>
        /// Delete specified package from the current package. The package to be deleted mu be a direct child of the
        /// specified package! The method fails silently (with log warning) when the package could not be found.
        /// </summary>
        /// <param name="child">Package to be deleted.</param>
        internal override void DeletePackage(MEPackage child)
        {
            this._package.Packages.Refresh();   // Make sure that we're looking at the most up-to-date state.
            for (short i = 0; i < this._package.Packages.Count; i++)
            {
                var childPackage = this._package.Packages.GetAt(i) as EA.Package;
                if (childPackage.PackageID == child.ElementID)
                {
                    this._package.Packages.DeleteAt(i, true);   // Refresh options currently does not work.
                    this._package.Packages.Refresh();
                    child.InValidate();                         // Make sure to mark the associated implementation as invalid!
                    return;
                }
            }
            Logger.WriteWarning("SparxEA.Model.EAMEIPackage.deletePackage >> Attempt to delete '" + child.Name +
                                "' from package: '" + this._package.Name + "' failed; child package not found!");
        }

        /// <summary>
        /// Export the package (and optional sub-packages) as an XMI file to the specified output path.
        /// </summary>
        /// <param name="fileName">Absolute pathname to output path and file.</param>
        /// <param name="recursive">When true (default) export the entire hierarchy. When false, export only the current package. </param>
        internal override bool ExportPackage(string fileName, bool recursive)
        {
            int _NoFormatXML        = Convert.ToInt32(false);
            int _UseDTD             = Convert.ToInt32(true);
            int _NoPictures         = 0;
            int _NoPictureFormats   = -1;

            bool result = true;
            try
            {
                fileName = fileName.Replace('/', '\\');    
                EA.Project project = ((EAModelImplementation)this._model).Repository.GetProjectInterface();
                project.ExportPackageXMIEx(this._package.PackageGUID,
                                           EnumXMIType.xmiEADefault,    // Of xmiEA251?
                                           _NoPictures, _NoPictureFormats, _NoFormatXML, _UseDTD,
                                           fileName,
                                           !recursive? Convert.ToInt32(EA.ExportPackageXMIFlag.epSaveToStub): 0);
            }
            catch (Exception exc)
            {
                Logger.WriteError("SparxEA.Model.EAMEIPackage.ExportPackage >> Error exporting package '" + this._name + 
                                  "' to XMI file '" + fileName + "':" + Environment.NewLine + exc.Message);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Searches the package for any class with given name and optional stereotype.
        /// </summary>
        /// <param name="className">Name of class to find.</param>
        /// <param name="stereotype">Optional stereotype of class.</param>
        /// <returns>Class instance found or NULL when not found.</returns>
        internal override MEClass FindClass(string className, string stereotype)
        {
            this._package.Elements.Refresh();   // Make sure that we're looking at the most up-to-date state.
            foreach (EA.Element element in this._package.Elements)
            {
                if (element.Name == className)
                {
                    if (!string.IsNullOrEmpty(stereotype) && element.HasStereotype(stereotype))
                    {
                        MEIClass classImp = new EAMEIClass((EAModelImplementation)this._model, element.ElementID);
                        return new MEClass(classImp);
                    }
                    else if (string.IsNullOrEmpty(stereotype))  // We found a match on name and stereotype is not specified.
                    {
                        MEIClass classImp = new EAMEIClass((EAModelImplementation)this._model, element.ElementID);
                        return new MEClass(classImp);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Searches the package for any data type with given name and optional stereotype.
        /// </summary>
        /// <param name="typeName">Name of type to find.</param>
        /// <param name="stereotype">Optional stereotype of data type.</param>
        /// <returns>Type instance found or NULL when not found.</returns>
        internal override MEDataType FindDataType(string typeName, string stereotype)
        {
            ModelSlt model = ModelSlt.GetModelSlt();
            this._package.Elements.Refresh();   // Make sure that we're looking at the most up-to-date state.
            foreach (EA.Element element in this._package.Elements)
            {
                if (element.Name == typeName)
                {
                    if (string.IsNullOrEmpty(stereotype)) return model.GetDataType(element.ElementID);
                    else if (element.HasStereotype(stereotype)) return model.GetDataType(element.ElementID);
                }
            }
            return null;
        }

        /// <summary>
        /// Searches the package for any diagram with given name. If the name is null or empty, the function returns the first diagram 
        /// that is present in the package.
        /// </summary>
        /// <param name="diagramName">Name of type to find.</param>
        /// <returns>Diagram instance found or NULL when not found.</returns>
        internal override Framework.View.Diagram FindDiagram(string diagramName)
        {
            this._package.Diagrams.Refresh();   // Make sure that we're looking at the most up-to-date state.
            foreach (EA.Diagram diagram in this._package.Diagrams)
            {
                if (string.IsNullOrEmpty(diagramName) || (diagram.Name == diagramName)) return new Framework.View.Diagram(diagram.DiagramID);
            }
            return null;
        }

        /// <summary>
        /// The method searches the current package for the first child with specified name and optional stereotype.
        /// You can have multiple child packages with the same name. In that case, differentiation by Stereotype makes sense.
        /// If stereotype is not specified, we return the first match found. In case of name + stereotype, we return the first
        /// match of both the name and the stereotype.
        /// </summary>
        /// <param name="childName">Package name to search for.</param>
        /// <param name="stereotype">Optional stereotype.</param>
        /// <returns>Child class or NULL when not found or on errors.</returns>
        internal override MEPackage FindPackage(string childName, string stereotype)
        {
            EA.Repository repository = ((EAModelImplementation)this._model).Repository;
            string query = string.Empty;
            bool isLocalDB = ModelSlt.GetModelSlt().ModelRepositoryType == ModelSlt.RepositoryType.Local;

            // We MUST specify either a package name and/or a stereotype!
            if (childName == string.Empty && string.IsNullOrEmpty(stereotype)) return null;

            if (!string.IsNullOrEmpty(stereotype))
            {
                string checkType = (stereotype.Contains("::")) ? stereotype.Substring(stereotype.IndexOf("::") + 2) : stereotype;
                if (childName == string.Empty)
                {
                    // In case of empty package name, we return the first package that has the specified stereotype.
                    if (isLocalDB)
                    {
                        // Access uses different syntax from 'real' SQL databases. LIKE clause looks different and we can NOT compare
                        // indices with different data types. Therefor, we explcitly convert PDATA1 to an integer (avoids syntax errors).
                        query = @"SELECT p.Package_ID AS PackageID FROM 
                             t_package p INNER JOIN t_object o ON p.Package_ID = Val(o.PDATA1)
                             WHERE p.Parent_ID = " + this._package.PackageID + " AND o.Stereotype LIKE '*" + checkType + "'";
                    }
                    else
                    {
                        query = @"SELECT p.Package_ID AS PackageID FROM 
                             t_package p INNER JOIN t_object o ON p.Package_ID = o.PDATA1
                             WHERE p.Parent_ID = " + this._package.PackageID + " AND o.Stereotype LIKE '%" + checkType + "'";
                    }
                }
                else
                {
                    if (isLocalDB)
                    {
                        // Access uses different syntax from 'real' SQL databases. LIKE clause looks different and we can NOT compare
                        // indices with different data types. Therefor, we explcitly convert PDATA1 to an integer (avoids syntax errors).
                        query = @"SELECT p.Package_ID AS PackageID FROM 
                             t_package p INNER JOIN t_object o ON p.Package_ID = Val(o.PDATA1)
                             WHERE p.Parent_ID = " + this._package.PackageID + " AND p.Name = '" + childName +
                                 "' AND o.Stereotype LIKE '*" + checkType + "'";
                    }
                    else
                    {
                        query = @"SELECT p.Package_ID AS PackageID FROM 
                             t_package p INNER JOIN t_object o ON p.Package_ID = o.PDATA1
                             WHERE p.Parent_ID = " + this._package.PackageID + " AND p.Name = '" + childName +
                                 "' AND o.Stereotype LIKE '%" + checkType + "'";
                    }
                }
            }
            else
            {
                query = @"SELECT p.Package_ID AS PackageID FROM t_package p
                         WHERE p.Parent_ID = " + this._package.PackageID + " AND p.Name = '" + childName + "'";
            }

            var queryResult = new XmlDocument();                            // Repository query will return an XML Document.
            queryResult.LoadXml(repository.SQLQuery(query));                // Execute query and store result in XML Document.
            XmlNodeList elements = queryResult.GetElementsByTagName("Row"); // Retrieve parent class name and package in which class resides.
            var parentList = new List<MEClass>();

            // We could get multiple results, just return the first one from the list...
            if (elements.Count > 0)
            {
                int packageID = Convert.ToInt32(elements[0]["PackageID"].InnerText.Trim());
                return new MEPackage(packageID);
            }
            return null;
        }

        /// <summary>
        /// Returns element annotation (if present).
        /// </summary>
        /// <returns>Element annotation or empty string if nothing present.</returns>
        internal override string GetAnnotation()
        {
            string notes = string.Empty;
            if (!string.IsNullOrEmpty(this._package.Notes)) notes = this._package.Notes.Trim();
            return notes;
        }

        /// <summary>
        /// Returns a list of all classes in the package that contain the specified stereotype. If no stereotype is specified,
        /// the method returns all Business Components and recognized Data Types.
        /// </summary>
        /// <returns>List of all classes in the package (empty list when nothing found)</returns>
        internal override List<MEClass> GetClasses(string stereotype)
        {
            var classList = new List<MEClass>();
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            string dataTypeStereotype = context.GetConfigProperty(_LogicalDataTypeStereotype);
            string componentStereotype = context.GetConfigProperty(_BusinessComponentStereotype);
            string enumStereotype = context.GetConfigProperty(_BDTEnumStereotype);

            this._package.Elements.Refresh();   // Assures that we're looking at the most up-to-date contents.
            foreach (EA.Element element in this._package.Elements)
            {
                if (!string.IsNullOrEmpty(stereotype))
                {
                    if (element.HasStereotype(stereotype)) classList.Add(new MEClass(element.ElementID));
                }
                else
                {
                    if (element.HasStereotype(componentStereotype))
                    {
                        // 'Standard' component (class).
                        classList.Add(new MEClass(element.ElementID));
                    }
                    else
                    {
                        // Check if this is a recognized data type...
                        if (String.Compare(element.MetaType, context.GetConfigProperty(_MetaTypeComplexDataType), StringComparison.OrdinalIgnoreCase) == 0 ||
						                      String.Compare(element.MetaType, context.GetConfigProperty(_MetaTypeSimpleDataType), StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            classList.Add(new MEDataType(element.ElementID));
                        }
                        else if (String.Compare(element.MetaType, context.GetConfigProperty(_MetaTypeEnumeration), StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            classList.Add(new MEEnumeratedType(element.ElementID));
                        }
                        else if (String.Compare(element.MetaType, context.GetConfigProperty(_MetaTypeUnion), StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            classList.Add(new MEUnionType(element.ElementID));
                        }
                    }
                }
            }
            return classList;
        }

        /// <summary>
        /// Returns the number of classes in the package.
        /// </summary>
        /// <returns>Number of classes in the package.</returns>
        internal override int GetClassCount()
        {
            return this._package.Elements.Count;
        }

        /// <summary>
        /// Simple method that searches a Package implementation for the occurence of a tag with specified name. 
        /// If found, the value of the tag is returned.
        /// </summary>
        /// <param name="tagName">The name of the tag.</param>
        /// <returns>Value of specified tag or empty string if nothing could be found.</returns>
        internal override string GetTag(string tagName)
        {
            try
            {
                if (this._package.Element == null)
                {
                    Logger.WriteError("SparxEA.Model.EAMEIPackage.GetTag >> Package '" + this._package.Name + "' not yet fully initialized!");
                    return string.Empty;
                }
                foreach (TaggedValue t in this._package.Element.TaggedValues)
                {
                    if (String.Compare(t.Name, tagName, StringComparison.OrdinalIgnoreCase) == 0) return t.Value;
                }
            }
            catch { /* & ignore all errors */ }
            return string.Empty;
        }

        /// <summary>
        /// Returns true if the package holds one or more Classes. If parameter 'hierarchy' is set to 'true', we check the 
        /// entire package hierarchy, otherwise, we check only the current package.
        /// </summary>
        /// <param name="hierarchy">True when entire hierarchy must be checked, starting at current package.</param>
        /// <returns>True when package(hierarchy) contains one or more classes.</returns>
        internal override bool HasContents(bool hierarchy)
        {
            bool foundContent = this._package.Elements.Count > 0;
            if (!foundContent && hierarchy)
            {
                foreach(EA.Package package in this._package.Packages)
                {
                    foundContent = new MEPackage(package.PackageID).HasContents(hierarchy);
                    if (foundContent) break;
                }
            }
            return foundContent;
        }

        /// <summary>
        /// The method checks whether one or more stereotypes from the given list of stereotypes are owned by the Package.
        /// Since HasStereotype supports fully-qualified stereotype names, we don't have to strip the (optional) profile names in this case.
        /// </summary>
        /// <param name="stereotypes">List of stereotypes to check.</param>
        /// <returns>True if at least one match is found, false otherwise.</returns>
        internal override bool HasStereotype(List<string> stereotypes)
        {
            if (this._package.Element == null)
            {
                Logger.WriteError("SparxEA.Model.EAMEIPackage.HasStereotype (list) >> Package '" + this._package.Name + "' not yet fully initialized!");
                return false;
            }
            foreach (string stereotype in stereotypes)
            {
                if (this._package.Element.HasStereotype(stereotype)) return true;
            }
            return false;
        }

        /// <summary>
        /// The method checks whether the given stereotype is owned by the Package. Since HasStereotype supports fully-qualified stereotype
        /// names, we don't have to strip the (optional) profile names in this case.
        /// </summary>
        /// <param name="stereotype">Stereotype to check.</param>
        /// <returns>True if a match is found, false otherwise.</returns>
        internal override bool HasStereotype(string stereotype)
        {
            if (this._package.Element == null)
            {
                Logger.WriteError("SparxEA.Model.EAMEIPackage.HasStereotype >> Package '" + this._package.Name + "' not yet fully initialized!");
                return false;
            }
            return this._package.Element.HasStereotype(stereotype);
        }

        /// <summary>
        /// Import the package from the specified XML file into this package, overwriting all contents.
        /// </summary>
        /// <param name="fileName">Absolute pathname to input path and file.</param>
        /// <returns>True when imported successsfully, false when unable to import.</returns>
        internal override bool ImportPackage(string fileName)
        {
            int _ImportDiagrams = Convert.ToInt32(true);
            int _NoStripGUID    = Convert.ToInt32(false);

            bool result = true;
            try
            {
                // Delete all sub-packages, elements and diagrams in this package as a preparation for the import...
                this._package.Packages.Refresh();
                for (short i = (short)(this._package.Packages.Count - 1); i >= 0; this._package.Packages.DeleteAt(i--, true));
                for (short i = (short)(this._package.Elements.Count - 1); i >= 0; this._package.Elements.DeleteAt(i--, true));
                for (short i = (short)(this._package.Diagrams.Count - 1); i >= 0; this._package.Diagrams.DeleteAt(i--, true));
                this._package.Packages.Refresh();

                fileName = fileName.Replace('/', '\\');
                EA.Project project = ((EAModelImplementation)this._model).Repository.GetProjectInterface();
                string resultMsg = project.ImportPackageXMI(this._package.PackageGUID,
                                                            fileName,
                                                            _ImportDiagrams, _NoStripGUID);
                if (resultMsg != this._package.PackageGUID)
                {
                    Logger.WriteError("SparxEA.Model.EAMEIPackage.ImportPackage >> Error importing package '" + this._name +
                                      "' from XMI file '" + fileName + "':" + Environment.NewLine + resultMsg);
                    result = false;
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("SparxEA.Model.EAMEIPackage.ImportPackage >> Error importing package '" + this._name +
                                  "' from XMI file '" + fileName + "':" + Environment.NewLine + exc.Message);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Test whether the package is currently locked. We do this by checking the database for the presence of a lock for the current user.
        /// When no user is found (or we get an exception), we assume that security is not enabled and the package is thus unlocked.
        /// When a user is found, we check for a lock for the current package. This can have three different results:
        /// 1) No entries are found at all, package is unlocked;
        /// 2) Lock is found for current user;
        /// 3) Lock is found for another user;
        /// Note that we ONLY check whether the package itself is locked and NOT the contents of the package, nor any sub-packages!
        /// </summary>
        /// <param name="lockedUser">Output parameter that will receive the formatted name of the user who has locked the package.</param>
        /// <returns>Lock status, one of Unlocked, LockedByMe or Locked.</returns>
        internal override MEPackage.LockStatus IsLocked(out string lockedUser)
        {
            MEPackage.LockStatus status = MEPackage.LockStatus.Unlocked;
            lockedUser = string.Empty;
            if (this._isLocked) return MEPackage.LockStatus.LockedByMe;             // This particular package has been locked earlier.
            try
            {
                EA.Repository repository = ((EAModelImplementation)this._model).Repository;
                if (!repository.IsSecurityEnabled) return MEPackage.LockStatus.Unlocked;    // When security is disabled, treat as always unlocked.
                string userGUID = repository.GetCurrentLoginUser(true);             // Returns the GUID of the current user (or exception when no security).
                if (!string.IsNullOrEmpty(userGUID))                                // Just to be sure...
                {
                    string lockedUserID = string.Empty;
                    string query = "SELECT UserID AS UserGUID FROM t_seclocks WHERE EntityID='" + this._package.PackageGUID + "';";
                    var queryResult = new XmlDocument();                            // Repository query will return an XML Document.
                    queryResult.LoadXml(repository.SQLQuery(query));                // Execute query and store result in XML Document.
                    XmlNodeList elements = queryResult.GetElementsByTagName("Row");

                    // We should have only a single result or no results at all (when no lock is found).
                    if (elements.Count > 0)
                    {
                        lockedUserID = elements[0]["UserGUID"].InnerText.Trim();
                        status = (lockedUserID == userGUID) ? MEPackage.LockStatus.LockedByMe : MEPackage.LockStatus.Locked;
                        Logger.WriteInfo("SparxEA.Model.EAMEIPackage.IsLocked >> Locked: " + status);

                        // Determine who has locked the package...
                        query = "SELECT FirstName, Surname FROM t_secuser WHERE UserID='" + lockedUserID + "';";
                        queryResult.LoadXml(repository.SQLQuery(query));            // Execute query and store result in XML Document.
                        elements = queryResult.GetElementsByTagName("Row");

                        // We should have only a single result.
                        if (elements.Count > 0)
                        {
                            lockedUser = elements[0]["FirstName"].InnerText.Trim() + " " + elements[0]["Surname"].InnerText.Trim();
                            Logger.WriteInfo("SparxEA.Model.EAMEIPackage.IsLocked >> Locked by: " + lockedUser);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.WriteInfo("SparxEA.Model.EAMEIPackage.IsLocked >> Exception, assume not locked: " + exc.Message);
            }
            Logger.WriteInfo("SparxEA.Model.EAMEIPackage.IsLocked >> Result of check: '" + status + "'.");
            return status;
        }

        /// <summary>
        /// This method searches the list current package for any names that match the provided 'name'. This can be a package or a class, diagram,
        /// etc. The function supports 'dotted name notation', e.g. qualified names such as 'package.element' can be used.
        /// When any name is detected that matches the given name, the function returns 'false'.
        /// </summary>
        /// <param name="name">Name to be verified.</param>
        /// <returns>True if name is unique, false otherwise.</returns>
        internal override bool IsUniqueName(string name)
        {
            try
            {
                if (this._package.FindObject(name) != null) return false;
            }
            catch { }
            return true;
        }

        /// <summary>
        /// First check whether the current package is unlocked. If so, create a recursive lock for the package and all contents (including contained 
        /// packages).
        /// </summary>
        /// <returns>True when lock is successfull, false on errors (includes locked by somebody else).</returns>
        internal override bool Lock()
        {
            try
            {
                string lockedUser;
                MEPackage.LockStatus status = IsLocked(out lockedUser);
                if (status == MEPackage.LockStatus.Unlocked)
                {
                    bool result = this._package.ApplyUserLockRecursive(true, true, true);
                    if (!result)
                    {
                        this._isLocked = false;
                        Logger.WriteWarning("SparxEA.Model.EAMEIPackage.Lock >> Failed to lock package structure because: '" + this._package.GetLastError() + "'!");
                    }
                    else this._isLocked = true;
                    return result;
                }
                else if (status == MEPackage.LockStatus.LockedByMe)
                {
                    // Package is already locked by the current user. However, since we did not perform the lock ourselves, the 'isLocked' status is set to 
                    // false. This assures that a subsequent call to unlock will not remove the lock from the package and thus we do not alter the precondition.
                    this._isLocked = false;
                    return true;
                }
                else
                {
                    // Locked by somebody else. Nothing to do here.
                    this._isLocked = false;
                    return false;
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("SparxEA.Model.EAMEIPackage.Lock >> Failed to lock package structure because: '" + exc.Message + "'!");
            }
            this._isLocked = false;
            return false;
        }

        /// <summary>
        /// Forces the repository implementation to refresh the current package and all children packages. This can be
        /// called after a number of model changes to assure that the model view is consistent with these changes.
        /// </summary>
        internal override void Refresh()
        {
            ((EAModelImplementation)this._model).Repository.RefreshModelView(this._package.PackageID);
        }
        
        /// <summary>
        /// Updates Package annotation.
        /// </summary>
        /// <param name="text">The annotation text to write.</param>
        internal override void SetAnnotation(string text)
        {
            this._package.Notes = text;
            this._package.Update();
        }

        /// <summary>
        /// Update the alias name of the Package.
        /// </summary>
        /// <param name="newAliasName">New name to be assigned.</param>
        internal override void SetAliasName(string newAliasName)
        {
            this._package.Alias = newAliasName;
            this._aliasName = newAliasName;
            this._package.Update();
        }

        /// <summary>
        /// Update the name of the Package.
        /// </summary>
        /// <param name="newName">New name to be assigned.</param>
        internal override void SetName(string newName)
        {
            this._package.Name = newName;
            this._name = newName;
            this._package.Update();
        }

        /// <summary>
        /// Set the tag with specified name to the specified value for the current package implementation. 
        /// If the tag could not be found, nothing will happen, unless the 'createIfNotExists' flag is set to true, in which case the tag will
        /// be created.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="tagValue">Value of the tag.</param>
        /// <param name="createIfNotExist">When set to true, create the tag if it's not there.</param>
        internal override void SetTag(string tagName, string tagValue, bool createIfNotExist = false)
        {
            try
            {
                if (this._package.Element == null)
                {
                    Logger.WriteError("SparxEA.Model.EAMEIPackage.SetTag >> Package '" + this._package.Name + "' not yet fully initialized!");
                    return;
                }
                foreach (TaggedValue t in this._package.Element.TaggedValues)
                {
                    if (String.Compare(t.Name, tagName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        t.Value = tagValue;
                        t.Update();
                        return;
                    }
                }

                // Element tag not found, create new one if instructed to do so...
                if (createIfNotExist)
                {
                    var newTag = this._package.Element.TaggedValues.AddNew(tagName, "TaggedValue") as TaggedValue;
                    newTag.Value = tagValue;
                    newTag.Update();
                    this._package.Element.TaggedValues.Refresh();
                }
            }
            catch { /* & ignore all errors */ }
        }

        /// <summary>
        /// Selects the package in the package tree and show to user.
        /// </summary>
        internal override void ShowInTree()
        {
            ((EAModelImplementation)this._model).Repository.ShowInProjectView(this._package);
        }

        /// <summary>
        /// Attempts to unlock the package and all included elements, diagrams and sub-packages. Errors are silently ignored.
        /// </summary>
        internal override void Unlock()
        {
            try
            {
                if (this._isLocked)
                {
                    bool result = this._package.ReleaseUserLockRecursive(true, true, true);
                    if (!result)
                    {
                        Logger.WriteWarning("SparxEA.Model.EAMEIPackage.Unlock >> Failed to unlock package structure because: '" + this._package.GetLastError() + "'!");
                    }
                    this._isLocked = false;
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("SparxEA.Model.EAMEIPackage.Unlock >> Failed to unlock package structure because: '" + exc.Message + "'!");
            }
        }

        /// <summary>
        /// Is called when looking for a parent that has not been registered (yet). The method retrieves the parent ID of
        /// the current package and creates a new implementation.
        /// This method is useful when browsing through the package tree from a given package upwards where parents
        /// have not yet been registered (e.g. parent of parent of parent....)
        /// </summary>
        /// <returns>Parent implementation or NULL when no parent present.</returns>
        protected override MEIPackage GetParentExplicit()
        {
            int parentID = this._package.ParentID;
            return (parentID != 0)? new EAMEIPackage((EAModelImplementation)this._model, parentID): null;
        }
    }
}
