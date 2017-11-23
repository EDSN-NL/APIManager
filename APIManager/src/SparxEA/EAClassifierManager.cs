using System;
using System.Xml;
using System.Collections.Generic;
using Framework.Logging;
using EA;

namespace SparxEA.ModelTransform
{
    /// <summary>
    /// Factory for production of PRIM, CDT or BDT classifier information. It is designed as a singleton object that maintains a table of a selected
    /// set of PRIM, CDT and/or BDT classifiers, loaded using the 'loadClassifiers' method.
    /// </summary>
    internal sealed class ClassifierManagerSlt
    {
        /// <summary>
        /// This local class is used to keep type information of CDT/BDT and BIN data types, used for lookup operations on core types (getCDTClassifierGUID)
        /// </summary>
        internal class ClassifierIdentity
        {
            private string _name;               // The classifier name or alias name;
            private string _rootName;           // Original name in case of alias;
            private string _GUID;               // The associated GUID;
            private bool _isPrim;               // TRUE when this is a primitive type, false in case of CDT.
            private string _stereoType;         // Class stereotype.

            /// <summary>
            /// Simple getters:
            /// name = returns (alias) name;
            /// rootName = always returns the formal (official) type name.
            /// GUID = return associated GUID.
            /// primitive = returns 'true' if this is a PRIM or 'false' if this is a CDT.
            /// stereotype = classifier stereotype or NULL if not defined.
            /// </summary>
            /// <returns></returns>
            internal string Name          { get { return this._name; } }
            internal string RootName      { get { return this._rootName; } }
            internal string GUID          { get { return this._GUID; } }
            internal bool Primitive       { get { return this._isPrim; } }
            internal string StereoType    { get { return this._stereoType; } }

            /// <summary>
            /// This constructor creates a standard entry (tuple of name / guid
            /// Parameters are:
            /// cName = classifier name (typename).
            /// cGUID = classifier GUID.
            /// cStereoType = classifier Stereotype.
            /// cIsPrim = indicates whether this is a PRIM or a CDT/BDT type.
            /// </summary>
            /// <param name="cName"></param>
            /// <param name="cGUID"></param>
            /// <param name="cIsPrim"></param>
            internal ClassifierIdentity(string cName, string cGUID, string cStereoType, bool cIsPrim = false)
            {
                this._name = cName;
                this._rootName = cName;
                this._GUID = cGUID;
                this._isPrim = cIsPrim;
                this._stereoType = cStereoType;
            }

            /// <summary>
            /// This constructor creates an alias entry, consisting of a formal CDT/BDT or PRIM name and an alias name that can be used instead of the
            /// formal name. This facilitates support for reference models that do not adhere to the UN/Cefact naming conventions and/or use
            /// deprecated names.
            /// Parameters are:
            /// cName = alias name (typename).
            /// cRootName = formal name that this alias refers to.
            /// cGUID = classifier GUID.
            /// cStereoType = classifier Stereotype.
            /// cIsPrim = indicates whether this is a PRIM or a CDT type.
            /// </summary>
            /// <param name="cName"></param>
            /// <param name="cGUID"></param>
            /// <param name="cIsPrim"></param>           
            internal ClassifierIdentity(string cName, string cRootName, string cGUID, string cStereoType, bool cIsPrim = false)
            {
                this._name = cName;
                this._rootName = cRootName;
                this._GUID = cGUID;
                this._isPrim = cIsPrim;
                this._stereoType = cStereoType;
            }
        }

        // This is the actual singleton. It is created automatically on first load.
        private static readonly ClassifierManagerSlt _classifierMgrSlt = new ClassifierManagerSlt();

        private SortedList<string, ClassifierIdentity> _classifiers;    // Used to keep a list of CDT and PRIM declarations in memory for future references.
        private SortedList<string, ClassifierIdentity> _dateTimePRIM;   // We use this for PRIM date/time types only (these are treated a little different).
        private Repository _repository;                                 // EA Repository to be used for the transformation.

        /// <summary>
        /// Public Context "factory" method. Simply returns the static instance.
        /// </summary>
        /// <returns>Context singleton object</returns>
        internal static ClassifierManagerSlt GetClassifierManagerSlt() { return _classifierMgrSlt; }

        /// <summary>
        /// Must be called to remove old context and release resources.
        /// </summary>
        internal void CloseDown()
        {
            this._classifiers = null;
            this._dateTimePRIM = null;
            this._repository = null;
        }

        /// <summary>
        /// Searches the classifier collection for any classifier that has the given name and returns the associated ClassifierIdentity object.
        /// Date/Time classifiers reside in a different list and thus must be treated differently.
        /// </summary>
        /// <param name="classifierName">Classifier name to locate.</param>
        /// <returns>ClassifierIdentity object or null if not found.</returns>
        internal ClassifierIdentity FindClassifierByName(string classifierName)
        {
            classifierName = classifierName.ToLower();
            if (classifierName == "date" || classifierName == "time" || classifierName == "datetime")
            {
                return (this._dateTimePRIM.ContainsKey(classifierName)) ? this._dateTimePRIM[classifierName] : null;
            }
            else return (this._classifiers.ContainsKey(classifierName)) ? this._classifiers[classifierName] : null;
        }

        /// <summary>
        /// Searches the classifier collection for any classifier that has the given GUID and returns the associated ClassifierIdentity object.
        /// </summary>
        /// <param name="GUID">GUID to locate.</param>
        /// <returns>ClassifierIdentity object or null if not found.</returns>
        internal ClassifierIdentity FindClassifierByGUID(string GUID)
        {
            foreach (ClassifierIdentity id in this._classifiers.Values) if (id.GUID == GUID) return id; 
            return null;
        }

        /// <summary>
        /// Must be called to initialize the object before first use, allocates appropriate resources and removes old contents.
        /// </summary>
        internal void Initialize(Repository repository)
        {
            this._classifiers = new SortedList<string, ClassifierIdentity>();
            this._dateTimePRIM = new SortedList<string, ClassifierIdentity>();
            this._repository = repository;
        }

        /// <summary>
        /// EMust be called with the pathname of the PRIM/CDT/BDT library that must be loaded. 
        /// </summary>
        /// <param name="fullyQualifiedName">The Fully Qualified Name of the package to be loaded.</param>
        /// <param name="isPrim">Whether these are primitive types ('PRIM'), core data types ('CDT') or business data types ('BDT').</param>
        /// <returns>"Ok" or "Error"</returns>
        internal string LoadClassifierTypes(string fullyQualifiedName, bool isPrim)
        {
            string[] nameParts = fullyQualifiedName.Split(new Char[] { '.' });  // Break-up in separate naming components.
            try
            {
                string query;

                switch (nameParts.Length)
                {
                    case 1:
                        query = BuildQueryPartL1(nameParts);
                        break;

                    case 2:
                        query = BuildQueryPartL2(nameParts);
                        break;

                    case 3:
                        query = BuildQueryPartL3(nameParts);
                        break;

                    case 4:
                        query = BuildQueryPartL4(nameParts);
                        break;

                    case 5:
                        query = BuildQueryPartL5(nameParts);
                        break;

                    default:
                        Logger.WriteError("loadClassifierTypes >> Qualified name can not be more then 5 levels deep:" + fullyQualifiedName);
                        return "Error";
                }

                var queryResult = new XmlDocument();                    // Repository query will return an XML Document.
                queryResult.LoadXml(this._repository.SQLQuery(query));          // Execute query and store result in XML Document.
                XmlNodeList elements = queryResult.GetElementsByTagName("Row"); // Retrieve all the individual Classifier identifiers.
                Logger.WriteInfo("SparxEA.ModelTransform.EAClassifierManager.loadClassifierTypes >> Query result: " + queryResult.InnerText);

                // Build a list of classifier name/GUID tuples...
                for (int i = 0; i < elements.Count; i++)
                {
                    // We do NOT want the PRIM types 'Date', 'Time' and 'DateTime' in our list since we want these to be matched against the CDT types only!
                    Logger.WriteInfo("SparxEA.ModelTransform.EAClassifierManager.loadClassifierTypes >> Processing: '" + elements[i]["className"].InnerText + "'...");
                    string classifierName = elements[i]["className"].InnerText.ToLower().Trim();
                    if (isPrim && classifierName == "anytype") continue;        // We skip this one as a primitive!
                    if (classifierName != "date" && classifierName != "time" && classifierName != "datetime")
                    {
                        // Although we use a lowercase, trimmed, version for comparisons, we must store the original name to use in the models!
                        string classifierGUID = elements[i]["classGUID"].InnerText;
                        string classifierStereoType = elements[i]["stereoType"].InnerText;
                        var classifier = new ClassifierIdentity(elements[i]["className"].InnerText, classifierGUID, classifierStereoType, isPrim);
                        this._classifiers.Add(classifierName, classifier);
                        Logger.WriteInfo("SparxEA.ModelTransform.EAClassifierManager.loadClassifierTypes >> Added classifier: " + elements[i]["className"].InnerText + " with GUID: " + classifierGUID + " and stereoType: " + classifierStereoType);

                        // Two 'alias' type names exist: IDType is similar to IdentifierType and NumberType is similar to NumericType.
                        // Also, we map the deprecated OrdinalType to NumericType.
                        if (classifierName == "identifiertype")
                        {
                            classifier = new ClassifierIdentity("IDType", "IdentifierType", classifierGUID, classifierStereoType, false);
                            this._classifiers.Add("idtype", classifier);
                            Logger.WriteInfo("SparxEA.ModelTransform.EAClassifierManager.loadClassifierTypes >> Added classifier: IDType with GUID: " + classifierGUID);
                        }
                        else if (classifierName == "numerictype")
                        {
                            classifier = new ClassifierIdentity("NumberType", "NumericType", classifierGUID, classifierStereoType, false);
                            this._classifiers.Add("numbertype", classifier);
                            Logger.WriteInfo("SparxEA.ModelTransform.EAClassifierManager.loadClassifierTypes >> Added classifier: NumberType with GUID: " + classifierGUID);

                            classifier = new ClassifierIdentity("OrdinalType", "NumericType", classifierGUID, classifierStereoType, false);
                            this._classifiers.Add("ordinaltype", classifier);
                            Logger.WriteInfo("SparxEA.ModelTransform.EAClassifierManager.loadClassifierTypes >> Added classifier: OrdinalType with GUID: " + classifierGUID);
                        }
                        else if (classifierName == "decimal")
                        {
                            // In case the reference models use 'float' as a data type, this is mapped onto the primitive 'Decimal'
                            classifier = new ClassifierIdentity("Float", "Decimal", classifierGUID, classifierStereoType, true);
                            this._classifiers.Add("float", classifier);
                            Logger.WriteInfo("SparxEA.ModelTransform.EAClassifierManager.loadClassifierTypes >> Added primitive classifier: Float with GUID: " + classifierGUID);
                        }
                    }
                    else
                    {
                        // Extract and store date/time classifiers in a separate list...
                        string classifierGUID = elements[i]["classGUID"].InnerText;
                        string classifierStereoType = elements[i]["stereoType"].InnerText;
                        var classifier = new ClassifierIdentity(elements[i]["className"].InnerText, classifierGUID, classifierStereoType, isPrim);
                        this._dateTimePRIM.Add(classifierName, classifier);
                        Logger.WriteInfo("SparxEA.ModelTransform.EAClassifierManager.loadClassifierTypes >> Added date/time classifier: " + elements[i]["className"].InnerText + " with GUID: " + classifierGUID + " and stereoType: " + classifierStereoType);
                    }
                }
                Logger.WriteInfo("SparxEA.ModelTransform.EAClassifierManager.loadClassifierTypes >> Classifiers loaded for FQN: " + fullyQualifiedName);
                return "Ok";
            }
            catch (Exception exc)
            {
                Logger.WriteError("SparxEA.ModelTransform.EAClassifierManager.loadClassifierTypes >> Exception retrieving Classifiers for FQN: " + fullyQualifiedName + "! Cause: " + Environment.NewLine + exc.Message);
                return "Error";
            }
        }

        /// <summary>
        /// Private constructor prevents direct instantiation.
        /// </summary>
        private ClassifierManagerSlt()
        {
            this._classifiers = null;
            this._dateTimePRIM = null;
            this._repository = null;
        }

        /// <summary>
        /// Helper function that creates a query that queries a single-level FQN (e.g. Package.Element)
        /// Parameters are:
        /// qualifiedNameParts = an array of strings that represent the FQN (package names only, no element name!)
        /// </summary>
        /// <param name="qualifiedNameParts"></param>
        /// <returns></returns>
        private static string BuildQueryPartL1(string[] qualifiedNameParts)
        {
            string query = @"SELECT o.name AS className, o.Stereotype AS stereoType, o.ea_guid AS classGUID 
                FROM t_object o INNER JOIN t_package p ON o.package_id = p.package_id
                WHERE o.Object_Type in ('DataType', 'Enumeration') AND p.Name = '" + qualifiedNameParts[0] + "\';";
            return query;
        }

        /// <summary>
        /// Helper function that creates a query that queries a two-level FQN (e.g. Package.Package.Element)
        /// Parameters are:
        /// qualifiedNameParts = an array of strings that represent the FQN (package names only, no element name!)
        /// </summary>
        /// <param name="qualifiedNameParts"></param>
        /// <returns></returns>
        private static string BuildQueryPartL2(string[] qualifiedNameParts)
        {
            string query = @"SELECT o.name AS className, o.Stereotype AS stereoType, o.ea_guid AS classGUID 
                FROM ((t_object o INNER JOIN t_package p ON o.package_id = p.package_id) LEFT JOIN t_package p1 ON p1.package_id = p.parent_id)
                WHERE o.Object_Type in ('DataType', 'Enumeration') 
                AND p.Name = '" + qualifiedNameParts[1] + "' and p1.Name = '"
                                + qualifiedNameParts[0] + "\';";
            return query;
        }

        /// <summary>
        /// Helper function that creates a query that queries a three-level FQN (e.g. Package.Package.Package.Element)
        /// Parameters are:
        /// qualifiedNameParts = an array of strings that represent the FQN (package names only, no element name!)
        /// </summary>
        /// <param name="qualifiedNameParts"></param>
        /// <returns></returns>
        private static string BuildQueryPartL3(string[] qualifiedNameParts)
        {
            string query = @"SELECT o.name AS className, o.Stereotype AS stereoType, o.ea_guid AS classGUID 
                FROM (((t_object o INNER JOIN t_package p ON o.package_id = p.package_id) 
                LEFT JOIN t_package p1 ON p1.package_id = p.parent_id)
                LEFT JOIN t_package p2 ON p2.package_id = p1.parent_id)
                WHERE o.Object_Type in ('DataType', 'Enumeration') 
                AND p.Name = '" + qualifiedNameParts[2] + "' AND p1.Name = '"
                                + qualifiedNameParts[1] + "' AND p2.Name = '"
                                + qualifiedNameParts[0] + "\';";
            return query;
        }

        /// <summary>
        /// Helper function that creates a query that queries a four-level FQN (e.g. Package.Package.Package.Package.Element)
        /// Parameters are:
        /// qualifiedNameParts = an array of strings that represent the FQN (package names only, no element name!)
        /// </summary>
        /// <param name="qualifiedNameParts"></param>
        /// <returns></returns>
        private static string BuildQueryPartL4(string[] qualifiedNameParts)
        {
            string query = @"SELECT o.name AS className, o.Stereotype AS stereoType, o.ea_guid AS classGUID 
                FROM ((((t_object o INNER JOIN t_package p ON o.package_id = p.package_id) 
                LEFT JOIN t_package p1 ON p1.package_id = p.parent_id)
                LEFT JOIN t_package p2 ON p2.package_id = p1.parent_id)
                LEFT JOIN t_package p3 ON p3.package_id = p2.parent_id)
                WHERE o.Object_Type in ('DataType', 'Enumeration') 
                AND p.Name = '" + qualifiedNameParts[3] + "' AND p1.Name = '"
                                + qualifiedNameParts[2] + "' AND p2.Name = '"
                                + qualifiedNameParts[1] + "' AND p3.Name = '"
                                + qualifiedNameParts[0] + "\';";
            return query;
        }

        /// <summary>
        /// Helper function that creates a query that queries a five-level FQN (e.g. Package.Package.Package.Package.Package.Element)
        /// Parameters are:
        /// qualifiedNameParts = an array of strings that represent the FQN (package names only, no element name!)
        /// </summary>
        /// <param name="qualifiedNameParts"></param>
        /// <returns></returns>
        private static string BuildQueryPartL5(string[] qualifiedNameParts)
        {
            string query = @"SELECT o.name AS className, o.Stereotype AS stereoType, o.ea_guid AS classGUID 
                FROM (((((t_object o INNER JOIN t_package p ON o.package_id = p.package_id) 
                LEFT JOIN t_package p1 ON p1.package_id = p.parent_id)
                LEFT JOIN t_package p2 ON p2.package_id = p1.parent_id)
                LEFT JOIN t_package p3 ON p3.package_id = p2.parent_id)
                LEFT JOIN t_package p4 ON p4.package_id = p3.parent_id)
                WHERE o.Object_Type in ('DataType', 'Enumeration') 
                AND p.Name = '" + qualifiedNameParts[4] + "' AND p1.Name = '"
                                + qualifiedNameParts[3] + "' AND p2.Name = '"
                                + qualifiedNameParts[2] + "' AND p3.Name = '"
                                + qualifiedNameParts[1] + "' AND p4.Name = '"
                                + qualifiedNameParts[0] + "\';";
            return query;
        }
    }
}
