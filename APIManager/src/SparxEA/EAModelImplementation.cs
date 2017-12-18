using System;
using System.Collections.Generic;
using System.Xml;
using EA;
using Framework.Logging;
using Framework.Model;
using Framework.View;
using Framework.Context;
using SparxEA.View;

namespace SparxEA.Model
{
    internal sealed class EAModelImplementation: ModelImplementation
    {
        private Repository _repository;                                 // The actual Sparx EA Repository API.
        private ModelSlt.RepositoryType _repositoryType;           // Registers our repository implementation.
        private static SortedList<string, MEClass> _typeCache = null;   // Keeps track of searched-for classes/types.

        // Configuration properties used to distinguish between different classifier [meta-]types...
        private const string _MetaTypeComplexDataType       = "MetaTypeComplexDataType";
        private const string _MetaTypeSimpleDataType        = "MetaTypeSimpleDataType";
        private const string _MetaTypeEnumeration           = "MetaTypeEnumeration";
        private const string _MetaTypeUnion                 = "MetaTypeUnion";

        // Some of the default stereotypes required when creating components...
        private const string _GeneralizationStereotype      = "GeneralizationStereotype";
        private const string _AssociationStereotype         = "AssociationStereotype";
        private const string _MessageAssociationStereotype  = "MessageAssociationStereotype";

        /// <summary>
        /// Getter that returns the repository instance to interested parties (such as Model Element Implementations)
        /// </summary>
        internal Repository Repository {get { return this._repository; }}

        /// <summary>
        /// The internal constructor is called to initialize the repository.
        /// </summary>
        internal EAModelImplementation(Repository repository): base()
        {
            this._repository = repository;
            this._repositoryType = ModelSlt.RepositoryType.Unknown;
            if (_typeCache == null) _typeCache = new SortedList<string, MEClass>();
        }

        /// <summary>
        /// This method is called before the model implementation is released. It should cleanup context.
        /// </summary>
        internal override void ShutDown()
        {
            this._repository = null;
            _typeCache = null;
            base.ShutDown();
        }

        /// <summary>
        /// Finds a class by its name and a repository path.
        /// </summary>
        /// <param name="path">Path name, elements separated by ':' and max. 6 levels of depth.</param>
        /// <param name="className">Name of the class to find.</param>
        /// <returns>Retrieved class or NULL on errors / nothing found.</returns>
        internal override MEClass FindClass(string path, string className)
        {
            string key = path + ":" + className;
            MEClass foundClass = null;
            if (_typeCache.ContainsKey(key))
            {
                foundClass = _typeCache[key];
            }
            else
            {
                String query = @"SELECT o.Object_ID AS ElementID
                            FROM((((((t_object o INNER JOIN t_package p0 ON o.package_id = p0.package_id)
                            LEFT JOIN t_package p1 ON p1.package_id = p0.parent_id)
                            LEFT JOIN t_package p2 ON p2.package_id = p1.parent_id)
                            LEFT JOIN t_package p3 ON p3.package_id = p2.parent_id)
                            LEFT JOIN t_package p4 ON p4.package_id = p3.parent_id)
                            LEFT JOIN t_package p5 ON p5.package_id = p4.parent_id) 
                            WHERE o.Object_Type = 'Class' AND o.Name = '" + className + "'";

                string[] pathElements = path.Split(':');
                int classID = -1;

                if (pathElements.Length <= 6)
                {
                    int index = pathElements.Length - 1;        // We count backwards...
                    foreach (string packageName in pathElements)
                    {
                        query += " and p" + index-- + ".name = '" + packageName.Trim() + "'";
                    }
                    query += ";";

                    var queryResult = new XmlDocument();                    // Repository query will return an XML Document.
                    queryResult.LoadXml(Repository.SQLQuery(query));                // Execute query and store result in XML Document.
                    XmlNodeList elements = queryResult.GetElementsByTagName("Row"); // Retrieve parent class name and package in which class resides.

                    // There should be only one result, we take the first one....
                    if (elements.Count > 0)
                    {
                        classID = Convert.ToInt32(elements[0]["ElementID"].InnerText.Trim());
                        foundClass = new MEClass(classID);
                        _typeCache.Add(key, foundClass);              
                    }
                }
                else
                {
                    Logger.WriteError("SparxEA.Model.EAModelImplementation.findClass >> Path '" + path + "' contains more then 6 levels!");
                }
            }
            return foundClass;
        }

        /// <summary>
        /// Finds an object by its name and a repository path.
        /// </summary>
        /// <param name="path">Path name, elements separated by ':' and max. 6 levels of depth.</param>
        /// <param name="objectName">Name of the object to find.</param>
        /// <returns>Retrieved object or NULL on errors / nothing found.</returns>
        internal override MEObject FindObject(string path, string objectName)
        {
            string key = path + ":" + objectName;
            MEObject foundObject = null;
            if (_typeCache.ContainsKey(key))
            {
                foundObject = _typeCache[key] as MEObject;
            }
            else
            {
                String query = @"SELECT o.Object_ID AS ElementID
                            FROM((((((t_object o INNER JOIN t_package p0 ON o.package_id = p0.package_id)
                            LEFT JOIN t_package p1 ON p1.package_id = p0.parent_id)
                            LEFT JOIN t_package p2 ON p2.package_id = p1.parent_id)
                            LEFT JOIN t_package p3 ON p3.package_id = p2.parent_id)
                            LEFT JOIN t_package p4 ON p4.package_id = p3.parent_id)
                            LEFT JOIN t_package p5 ON p5.package_id = p4.parent_id) 
                            WHERE o.Object_Type = 'Object' AND o.Name = '" + objectName + "'";

                string[] pathElements = path.Split(':');
                int objectID = -1;

                if (pathElements.Length <= 6)
                {
                    int index = pathElements.Length - 1;        // We count backwards...
                    foreach (string packageName in pathElements)
                    {
                        query += " and p" + index-- + ".name = '" + packageName.Trim() + "'";
                    }
                    query += ";";

                    var queryResult = new XmlDocument();                    	 	// Repository query will return an XML Document.
                    queryResult.LoadXml(Repository.SQLQuery(query));                // Execute query and store result in XML Document.
                    XmlNodeList elements = queryResult.GetElementsByTagName("Row"); // Retrieve parent class name and package in which class resides.

                    // There should be only one result, we take the first one....
                    if (elements.Count > 0)
                    {
                        objectID = Convert.ToInt32(elements[0]["ElementID"].InnerText.Trim());
                        foundObject = new MEObject(objectID);
                        _typeCache.Add(key, foundObject);
                        Logger.WriteInfo("SparxEA.Model.EAModelImplementation.findObject >> Got element ID: " + objectID);
                    }
                }
                else
                {
                    Logger.WriteError("SparxEA.Model.EAModelImplementation.findObject >> Path '" + path + "' contains more then 6 levels!");
                }
            }
            return foundObject;
        }

        /// <summary>
        /// Retrieve a data type by name and path from the repository. The path parameter specifies the 
        /// package in which we have to locate the type. This also defines whether we return a BDT, CDT or 
        /// PRIM data type. The meta-type is defined by the name (name should be unique within the package).
        /// </summary>
        /// <param name="path">Full path towards the package in which we have to search for the data type.
        /// Path elements must be separated by ':' characters.</param>
        /// <param name="typeName">The name of the type to select.</param>
        /// <returns>Retrieve data type or NULL on errors / nothing found.</returns>
        internal override MEDataType FindDataType(string path, string typeName)
        {
            string key = path + ":" + typeName;
            MEDataType foundType = null;
            if (_typeCache.ContainsKey(key))
            {
                foundType = _typeCache[key] as MEDataType;
            }
            else
            {
                String query = @"SELECT o.Object_ID AS ElementID
                            FROM((((((t_object o INNER JOIN t_package p0 ON o.package_id = p0.package_id)
                            LEFT JOIN t_package p1 ON p1.package_id = p0.parent_id)
                            LEFT JOIN t_package p2 ON p2.package_id = p1.parent_id)
                            LEFT JOIN t_package p3 ON p3.package_id = p2.parent_id)
                            LEFT JOIN t_package p4 ON p4.package_id = p3.parent_id)
                            LEFT JOIN t_package p5 ON p5.package_id = p4.parent_id) 
                            WHERE o.Object_Type in ('DataType','Enumeration') AND o.Name = '" + typeName + "'";

                string[] pathElements = path.Split(':');
                int classID = -1;

                if (pathElements.Length <= 6)
                {
                    int index = pathElements.Length - 1;        // We count backwards...
                    foreach (string packageName in pathElements)
                    {
                        query += " and p" + index-- + ".name = '" + packageName.Trim() + "'";
                    }
                    query += ";";

                    XmlDocument queryResult = new XmlDocument();                    // Repository query will return an XML Document.
                    queryResult.LoadXml(Repository.SQLQuery(query));                // Execute query and store result in XML Document.
                    XmlNodeList elements = queryResult.GetElementsByTagName("Row"); // Retrieve parent class name and package in which class resides.

                    // There should be only one result, we take the first one....
                    if (elements.Count > 0)
                    {
                        classID = Convert.ToInt32(elements[0]["ElementID"].InnerText.Trim());
                        foundType = GetDataType(classID);
                        if (foundType != null) _typeCache.Add(key, foundType);
                    }
                }
                else
                {
                    Logger.WriteError("SparxEA.Model.EAModelImplementation.findDataType >> Path '" + path + "' contains more then 6 levels!");
                }
            }
            return foundType;
        }

        /// <summary>
        /// Finds a package by its name and a repository path.
        /// </summary>
        /// <param name="path">Path name, elements separated by ':' and max. 6 levels of depth.</param>
        /// <param name="packageName">Name of the package to find.</param>
        /// <returns>Retrieved package or NULL on errors / nothing found.</returns>
        internal override MEPackage FindPackage(string path, string packageName)
        {
            string query = @"SELECT p.Package_ID AS PackageID
                            FROM((((((t_package p
                            LEFT JOIN t_package p0 ON p0.package_id = p.parent_id)
                            LEFT JOIN t_package p1 ON p1.package_id = p0.parent_id)
                            LEFT JOIN t_package p2 ON p2.package_id = p1.parent_id)
                            LEFT JOIN t_package p3 ON p3.package_id = p2.parent_id)
                            LEFT JOIN t_package p4 ON p4.package_id = p3.parent_id)
                            LEFT JOIN t_package p5 ON p5.package_id = p4.parent_id)
                            WHERE p.Name = '" + packageName + "'";

            string[] pathElements = path.Split(':');
            int packageID = -1;

            if (pathElements.Length <= 6)
            {
                int index = pathElements.Length - 1;        // We count backwards...
                foreach (string pName in pathElements)
                {
                    query += " and p" + index-- + ".name = '" + pName.Trim() + "'";
                }
                query += ";";

                XmlDocument queryResult = new XmlDocument();                    // Repository query will return an XML Document.
                queryResult.LoadXml(Repository.SQLQuery(query));                // Execute query and store result in XML Document.
                XmlNodeList elements = queryResult.GetElementsByTagName("Row"); // Retrieve parent class name and package in which class resides.

                // There should be only one result, we take the first one....
                if (elements.Count > 0)
                {
                    packageID = Convert.ToInt32(elements[0]["PackageID"].InnerText.Trim());
                }
            }
            else
            {
                Logger.WriteError("SparxEA.Model.EAModelImplementation.findPackage >> Path '" + path + "' contains more then 6 levels!");
                return null;
            }
            return (packageID > 0) ? new MEPackage(packageID) : null;
        }

        /// <summary>
        /// This function provides an efficient mechanism to obtain classes that are associated with the provided class. The function returns a list
        /// of all MEClass objects that are referenced from the provided source class, i.e. are at the 'receiving end' of an association.
        /// </summary>
        /// <param name="source">Class for which we want to obtain associated classes.</param>
        /// <param name="stereotype">The stereotype of the association</param>
        /// <returns>List of associated classes (could be empty if none found).</returns>
        internal override List<MEClass> GetAssociatedClasses(MEClass source, string stereotype)
        {
            bool isLocalDB = ModelSlt.GetModelSlt().ModelRepositoryType == ModelSlt.RepositoryType.Local;
            string likeClause = isLocalDB ? "LIKE '*" : "LIKE '%";  // EAP files use different syntax for 'like'!
            string checkType = (stereotype.Contains("::")) ? stereotype.Substring(stereotype.IndexOf("::") + 2) : stereotype;
            String query = @"SELECT o2.Object_ID AS ElementID
                            FROM((t_connector c INNER JOIN t_object o ON c.Start_Object_ID = o.Object_ID)
                            LEFT JOIN t_object o2 ON c.End_Object_ID = o2.Object_ID) 
                            WHERE o.Object_ID = " + source.ElementID + 
                            " AND c.Connector_Type = 'Association' AND c.Stereotype " + likeClause + checkType + "'";

            var queryResult = new XmlDocument();                            // Repository query will return an XML Document.
            queryResult.LoadXml(Repository.SQLQuery(query));                // Execute query and store result in XML Document.
            XmlNodeList elements = queryResult.GetElementsByTagName("Row"); // Retrieve parent class name and package in which class resides.
            List<MEClass> classList = new List<MEClass>();

            foreach (XmlNode element in elements)
            {
                int elementID = Convert.ToInt32(element["ElementID"].InnerText.Trim());
                classList.Add(new MEClass(elementID));
            }
            return classList;
        }

        /// <summary>
        /// Converts the given type identifier to the proper Data Type object. Based on the meta-type of the retrieved object,
        /// the returned type is constructed as either an MEDataType, MEEnumeratedType or an MEUnion.
        /// </summary>
        /// <param name="typeID">Repository object identifier, must be of a data type!</param>
        /// <returns>Appropriate data type object.</returns>
        internal override MEDataType GetDataType(int typeID)
        {
            var element = Repository.GetElementByID(typeID) as EA.Element;
            MEDataType dataType = null;
            ContextSlt context = ContextSlt.GetContextSlt();

            if (String.Compare(element.MetaType, context.GetConfigProperty(_MetaTypeComplexDataType), StringComparison.OrdinalIgnoreCase) == 0 ||
			    String.Compare(element.MetaType, context.GetConfigProperty(_MetaTypeSimpleDataType), StringComparison.OrdinalIgnoreCase) == 0)
            {
                dataType = new MEDataType(element.ElementID);
            }
            else if (String.Compare(element.MetaType, context.GetConfigProperty(_MetaTypeEnumeration), StringComparison.OrdinalIgnoreCase) == 0)
            {
                dataType = new MEEnumeratedType(element.ElementID);
            }
            else if (String.Compare(element.MetaType, context.GetConfigProperty(_MetaTypeUnion), StringComparison.OrdinalIgnoreCase) == 0)
            {
                dataType = new MEUnionType(element.ElementID);
            }
            else
            {
                Logger.WriteError("SparxEA.Model.EAModelImplementation.getDataType >> Element '" + element.Name +
                                 "'is of illegal type '" + element.MetaType + "', giving up!");
            }
            return dataType;
        }

        /// <summary>
        /// Converts the given type identifier to the proper Data Type object. Based on the meta-type of the retrieved object,
        /// the returned type is constructed as either an MEDataType, MEEnumeratedType or an MEUnion.
        /// </summary>
        /// <param name="typeGUID">Globally unique object identifier, must be of a data type!</param>
        /// <returns>Appropriate data type object.</returns>
        internal override MEDataType GetDataType(string typeGUID)
        {
            var element = Repository.GetElementByGuid(typeGUID) as EA.Element;
            MEDataType dataType = null;
            ContextSlt context = ContextSlt.GetContextSlt();

            if (String.Compare(element.MetaType, context.GetConfigProperty(_MetaTypeComplexDataType), StringComparison.OrdinalIgnoreCase) == 0 ||
                String.Compare(element.MetaType, context.GetConfigProperty(_MetaTypeSimpleDataType), StringComparison.OrdinalIgnoreCase) == 0)
            {
                dataType = new MEDataType(element.ElementID);
            }
            else if (String.Compare(element.MetaType, context.GetConfigProperty(_MetaTypeEnumeration), StringComparison.OrdinalIgnoreCase) == 0)
            {
                dataType = new MEEnumeratedType(element.ElementID);
            }
            else if (String.Compare(element.MetaType, context.GetConfigProperty(_MetaTypeUnion), StringComparison.OrdinalIgnoreCase) == 0)
            {
                dataType = new MEUnionType(element.ElementID);
            }
            else
            {
                Logger.WriteError("SparxEA.Model.EAModelImplementation.getDataType >> Element '" + element.Name +
                                 "'is of illegal type '" + element.MetaType + "', giving up!");
            }
            return dataType;
        }

        /// <summary>
        /// Factory method that constructs a Diagram Implementation object according to the provided instance ID.
        /// The method searches the dictionary first in order to avoid having to create redundant objects. This also assures that
        /// multiple diagram interfaces all reference THE SAME implementation instance and thus maintain stable state.
        /// </summary>
        /// <param name="diagramID">Instance identifier within the tool-specific repository.</param>
        /// <returns>DiagramImplementation object or NULL in case of errors.</returns>
        internal override DiagramImplementation GetDiagramImplementation(int diagramID)
        {
            // First of all, we check if the implementation already exists in the diagram list. If so, we re-use this one...
            DiagramImplementation imp = FindRegisteredDiagramImp(diagramID);

            if (imp == null) imp = new EADiagramImplementation(this, diagramID);
            return imp;
        }

        /// <summary>
        /// Many model elements are specializations of another element type. In order to reduce the number of levels in the dictionary and
        /// to avoid redundant entries, we maintain the dictionary on 'root-types' only. This function receives a ModelElementType and
        /// returns the root type of that type in order to arrive at the correct dictionary entry.
        /// The method is implemented in this repository-specific module since we also have to assure that the root types exist in different
        /// tables within the repository in order to avoid ID overlap!
        /// </summary>
        /// <param name="type">Specialized element type.</param>
        /// <returns>Associated root element type or Unknown type if not found.</returns>
        internal override ModelElementType GetRootType(ModelElementType type)
        {
            ModelElementType[,] translateTable =
              { {ModelElementType.Association,      ModelElementType.Association },
                {ModelElementType.Attribute,        ModelElementType.Attribute },
                {ModelElementType.Class,            ModelElementType.Class },
                {ModelElementType.DataType,         ModelElementType.Class },
                {ModelElementType.Enumeration,      ModelElementType.Class },
                {ModelElementType.Facet,            ModelElementType.Attribute },
                {ModelElementType.Object,           ModelElementType.Class },
                {ModelElementType.Package,          ModelElementType.Package },
                {ModelElementType.Supplementary,    ModelElementType.Attribute },
                {ModelElementType.Union,            ModelElementType.Class } };

            for (int i = 0; i < translateTable.GetLength(0); i++)
            {
                if (translateTable[i, 0] == type)
                {
                    return translateTable[i, 1];
                }
            }
            return ModelElementType.Unknown;
        }

        /// <summary>
        /// Factory method that constructs the appropriate Model Element Implementation object according to the provided type and instance ID.
        /// The method searches the dictionary first in order to avoid having to create redundant objects. This also assures that
        /// multiple interfaces of a particular type/instance all reference THE SAME implementation instance and thus maintain
        /// stable state.
        /// </summary>
        /// <param name="type">The type of implementation object to create.</param>
        /// <param name="elementID">Instance identifier within the tool-specific repository.</param>
        /// <returns>ModelElementImplementation object or NULL in case of errors.</returns>
        internal override ModelElementImplementation GetModelElementImplementation(ModelElementType type, int elementID)
        {
            // First of all, we check if the implementation already exists in the model dictionary. If so, we re-use this one...
            ModelElementImplementation imp = FindRegisteredElementImp(type, elementID);

            if (imp == null)
            {
                // Not found in dictionary, or registered as another type. We have to create a new instance of the correct type in order to
                // avoid casting errors. Note that we DO NOT register the instance here.
                // Instead, registering / de-registering of implementation objects is the responsibility of those objects
                // themselves and is managed by monitoring the association count between implementation objects and their
                // corresponding interfaces.
                switch (type)
                {
                    case ModelElementType.Association:
                        imp = new EAMEIAssociation(this, elementID);
                        break;

                    case ModelElementType.Attribute:
                        imp = new EAMEIAttribute(this, elementID);
                        break;

                    case ModelElementType.Class:
                        imp = new EAMEIClass(this, elementID);
                        break;

                    case ModelElementType.DataType:
                        imp = new EAMEIDataType(this, elementID);
                        break;

                    case ModelElementType.Enumeration:
                        imp = new EAMEIEnumeratedType(this, elementID);
                        break;

                    case ModelElementType.Facet:
                        imp = new EAMEIFacet(this, elementID);
                        break;

                    case ModelElementType.Object:
                        imp = new EAMEIObject(this, elementID);
                        break;

                    case ModelElementType.Package:
                        imp = new EAMEIPackage(this, elementID);
                        break;

                    case ModelElementType.Supplementary:
                        imp = new EAMEISupplementary(this, elementID);
                        break;

                    case ModelElementType.Union:
                        imp = new EAMEIUnionType(this, elementID);
                        break;

                    default:
                        Logger.WriteError("SparxEA.Model.EAModelImplementation.getModelElementImplementation >> Illegal type '" + type + "' passed to method!");
                        break;
                }
            }
            return imp;
        }

        /// <summary>
        /// Factory method that constructs the appropriate Model Element Implementation object according to the provided type and instance ID.
        /// The method searches the dictionary first in order to avoid having to create redundant objects. This also assures that
        /// multiple interfaces of a particular type/instance all reference THE SAME implementation instance and thus maintain
        /// stable state.
        /// </summary>
        /// <param name="type">The type of implementation object to create.</param>
        /// <param name="elementID">Instance identifier within the tool-specific repository.</param>
        /// <returns>ModelElementImplementation object or NULL in case of errors.</returns>
        internal override ModelElementImplementation GetModelElementImplementation(ModelElementType type, string elementGUID)
        {
            ModelElementImplementation imp = null;
            switch (type)
            {
                case ModelElementType.Association:
                    imp = new EAMEIAssociation(this, elementGUID);
                    break;

                case ModelElementType.Attribute:
                    imp = new EAMEIAttribute(this, elementGUID);
                    break;

                case ModelElementType.Class:
                    imp = new EAMEIClass(this, elementGUID);
                    break;

                case ModelElementType.DataType:
                    imp = new EAMEIDataType(this, elementGUID);
                    break;

                case ModelElementType.Enumeration:
                    imp = new EAMEIEnumeratedType(this, elementGUID);
                    break;

                case ModelElementType.Facet:
                    imp = new EAMEIFacet(this, elementGUID);
                    break;

                case ModelElementType.Object:
                    imp = new EAMEIObject(this, elementGUID);
                    break;

                case ModelElementType.Package:
                    imp = new EAMEIPackage(this, elementGUID);
                    break;

                case ModelElementType.Supplementary:
                    imp = new EAMEISupplementary(this, elementGUID);
                    break;

                case ModelElementType.Union:
                    imp = new EAMEIUnionType(this, elementGUID);
                    break;

                default:
                    Logger.WriteError("SparxEA.Model.EAModelImplementation.getModelElementImplementation >> Illegal type '" + type + "' passed to method!");
                    break;
            }

            // If we have created a new object, we use this to check if we already have a similar one registered using its
            // database identifier (ElementID). If so, we return the existing object instead of the newly created one.
            // If not found, we return the newly created instance. Note that we DO NOT register the instance here.
            // Instead, registering / de-registering of implementation objects is the responsibility of those objects
            // themselves and is managed by monitoring the association count between implementation objects and their
            // corresponding interfaces.
            if (imp != null)
            {
                ModelElementImplementation altImp = FindRegisteredElementImp(type, imp.ElementID);
                return (altImp != null) ? altImp : imp;
            }
            return imp;
        }

        /// <summary>
        /// Returns the repository type (must have been set by SetRepositoryType before...
        /// </summary>
        /// <returns>Repository type.</returns>
        internal override ModelSlt.RepositoryType GetRepositoryType()
        {
            return this._repositoryType;
        }

        /// <summary>
        /// Forces the repository implementation to refresh the entire model tree. This can be
        /// called after a number of model changes to assure that the model view is consistent with these changes.
        /// </summary>
        internal override void Refresh()
        {
            this._repository.RefreshModelView(0);
        }

        /// <summary>
        /// Loads the repository type. Typically, this is determined elsewhere, e.g. when opening a new model. For Sparx EA implementations,
        /// the repository type is determined by the EA Controller.
        /// </summary>
        /// <param name="type">Repository type.</param>
        internal override void SetRepositoryType(ModelSlt.RepositoryType type)
        {
            this._repositoryType = type;
        }
    }
}
