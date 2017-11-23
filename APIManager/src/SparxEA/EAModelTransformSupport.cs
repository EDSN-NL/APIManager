using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Framework.Logging;
using Framework.Context;
using EA;

namespace SparxEA.ModelTransform
{
    /// <summary>
    /// This singleton contains a set of EA Support methods that are called from the model transformation scripts and aid in the transformation of 
    /// reference models to the Enexis CDM Framwork.
    /// </summary>
    internal sealed class EAModelTransformSlt
    {
        // This is the actual singleton. It is created automatically on first load.
        private static readonly EAModelTransformSlt _transformSlt = new EAModelTransformSlt();

        private static Regex oagisCDTRegex = new Regex(@"([a-z]+)type_([a-z0-9]{6})"); //Matches an OAGIS CDT, which ends with 'Type_', followed by a 6-character alphanum. suffix

        private Hashtable _classifiersCache;  // Used as a database buffer for classifiers lookup. Key = ClassifierName, Value = Package + Classifier GUID (sorted list).
        private SortedList<string, string> _EDSNMappingTable;     // Used to map from EDSN LDT to our CDT/BDT.

        private Repository _repository;                         // EA Repository to be used for the transformation.

        /// <summary>
        /// Public Context "factory" method. Simply returns the static instance.
        /// </summary>
        /// <returns>Context singleton object</returns>
        internal static EAModelTransformSlt GetModelTransformSlt() { return _transformSlt; }

        /// <summary>
        /// This operation must be called at the beginning of a new transformation session. It opens the logfile and clears caches and buffers.
        /// Parameters are:
        /// repository = the EA repository object.
        /// argList[0] = name of the logfile to be opened.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="argList"></param>
        /// <returns>ok</returns>
        internal string Initialize(EA.Repository repository, string fileName)
        {
            if (fileName != "EXISTING") ContextSlt.GetContextSlt().SetLogfile(fileName);
            Logger.WriteInfo("initialize >> Logfile " + fileName + " created and opened for write.");

            ClassifierManagerSlt.GetClassifierManagerSlt().Initialize(repository);
            this._classifiersCache = new Hashtable();                 // Again, this replaces any old tables still lingering around.
            this._EDSNMappingTable = null;                            // Created only when needed.
            this._repository = repository;
            return "ok";
        }

        /// <summary>
        /// This must (may) be called at the end of the transformation session and is used to disconnect from the caches.
        /// Parameters are required by the EA call interface, but are not used here.
        /// </summary>
        /// <returns>ok</returns>
        internal string CloseDown()
        {
            ClassifierManagerSlt.GetClassifierManagerSlt().CloseDown();
            this._classifiersCache = null;
            this._EDSNMappingTable = null;
            this._repository = null;
            return "ok";
        }

        /// <summary>
        /// This operation checks whether the provided class has a generalization and if so, whether the parent class has the provided stereotype.
        /// </summary>
        /// <param name="className">Name of the class to be examined (logging purposes only).</param>
        /// <param name="classGUID">GUID of the class.</param>
        /// <param name="parentStereotype">Stereotype to be checked.</param>
        /// <returns>'true' if parent has stereotype, 'false' of no parent or parent without specified stereotype.</returns>
        internal string CheckParentStereotype(string className, string classGUID, string parentStereotype)
        {
            string result = "false";
            Logger.WriteInfo("checkParentStereotype >> Check whether parent of '" + className + "' has stereotype '" + parentStereotype + "'.");

            try
            {
                string query = @"SELECT o2.ea_guid AS ParentGUID
                            FROM ((t_connector c INNER JOIN t_object o ON c.Start_Object_ID = o.Object_ID) 
                            LEFT JOIN t_object o2 ON c.End_Object_ID = o2.Object_ID)
                            WHERE o.ea_guid = '" + classGUID + "' AND c.Connector_Type = 'Generalization';";

                var queryResult = new XmlDocument();                    // Repository query will return an XML Document.
                queryResult.LoadXml(this._repository.SQLQuery(query));                // Execute query and store result in XML Document.
                XmlNodeList elements = queryResult.GetElementsByTagName("Row"); // Retrieve parent class name and package in which class resides.

                // Class could have multiple parents, check each in turn...
                for (int i = 0; i < elements.Count; i++)
                {
                    //We found a parent class, check whether this class has the specified stereotype...
                    string parentGUID = elements[i]["ParentGUID"].InnerText.Trim();
                    Logger.WriteInfo("checkParentStereotype >> Found parent class, now checking stereotype...");

                    EA.Element element = this._repository.GetElementByGuid(parentGUID);
                    Logger.WriteInfo("checkParentStereotype >> Parent has stereotypes list: " + element.StereotypeEx);
                    if (element.StereotypeEx != string.Empty && element.StereotypeEx.IndexOf(parentStereotype, StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        result = "true";
                        break;
                    }
                }
            }
            catch
            {
                Logger.WriteError("checkParentStereotype >> caught exception, asuming illegal- or no parent");
            }
            Logger.WriteError("checkParentStereotype >> Result of operation is " + result);
            return result;
        }

        /// <summary>
        /// Checks whether the given class, identified by GUID, contains the specified stereotype.
        /// </summary>
        /// <param name="classGUID">the class GUID.</param>
        /// <param name="stereotype">the stereotype (case insentitive).</param>
        /// <returns>"true" if stereotype is defined for the class, "false" otherwise.</returns>
        internal string ClassHasStereotype(string classGUID, string stereotype)
        {
            string result = "false";
            EA.Element element = null;

            try
            {
                element = this._repository.GetElementByGuid(classGUID);
                if (element.HasStereotype(stereotype)) result = "true";
            }
            catch
            {
                Logger.WriteInfo("classHasStereotype >> caught exception, asuming illegal GUID!");
            }
            Logger.WriteInfo("classHasStereotype >> checking class: " + element.Name + " for Stereotype: " + stereotype + ", --> " + result);
            return result;
        }

        /// <summary>
        /// This operation is called to create a template that is used by the EA transformation scripts to create a generalization association between the specifiec class
        /// and the specified CDT type. It can be used to force inheritance of reference classes that have been declared as 'stand alone' while in practice they should
        /// have a parent. Best example is Enumerations, which in most models are declared as stand-alone primitive types. In our model, however, an Enumeration must be
        /// a specialization of the EnumType, which in itself is a specialization of the Enum primitive. This construction is required for the creation of proper schemas.
        /// Parameters are:
        /// argList[0] = transformation namespace root package;
        /// argList[1] = Source class package path;
        /// argList[2] = Source class name;
        /// argList[3] = Source class GUID;
        /// argList[4] = Classifier name of target class, must be a CDT or 'reserved name' GUID;
        /// argList[5] = in case the classifier name is 'GUID', this parameter contains the GUID of the target instead of the name;
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="argList"></param>
        /// <returns>Connector intermediate code or empty string in case of errors.</returns>
        internal string CreateParentConnector(string namespaceName, string[] packagePath, string sourceClassName, string sourceGUID, string targetClassName, string targetGUID)
        {
            string packageRoot = packagePath[0];                                // Top of the package tree currently being transformed (direct child of namespaceRoot)
            string sourcePackage = packagePath[packagePath.Length - 1];         // Package name of source class.
            string connectorGUID = GetParentGUID(this._repository, packageRoot, sourcePackage, sourceClassName, targetClassName, targetGUID); // Locate existing connector GUID or create new one.

            Logger.WriteInfo("createParentConnector >> Going to build a Generalization connector template from source " + sourceClassName + " to target: " + targetClassName);

            // Absence of connector GUID indicates an existing association, no need to create a new one.
            if (connectorGUID == string.Empty)
            {
                Logger.WriteInfo("createParentConnector >> Looks like association already exists, no further action!");
                return string.Empty;
            }

            // Determine the target GUID..
            if (targetGUID == string.Empty)
            {
                ClassifierManagerSlt.ClassifierIdentity id = ClassifierManagerSlt.GetClassifierManagerSlt().FindClassifierByName(targetClassName);
                if (id != null)
                {
                    targetGUID = id.GUID;
                    Logger.WriteInfo("createParentConnector >> Found target GUID " + targetGUID);
                }
            }

            if (targetGUID == string.Empty)
            {
                // Oops, for some reason we could not find the proposed target. We return an empty string...
                Logger.WriteError("createParentConnector >> Target not found. Returning empty template!");
                return string.Empty;
            }

            // Create EA intermediate code fragment that describes the parent connector...
            const string tplHeader = "notes=\"\"\nalias=\"\"\nname=\"\"\nstereotype=\"DerivedFrom\"\ndirection=\"Source -> Destination\"\n";
            string connectorRef = "XRef{namespace=\"" + namespaceName + "\" name=\"Connector\" source=\"" + connectorGUID + "\"}\n";
            string sourceRef = "XRef{namespace=\"" + namespaceName + "\" name=\"Class\" source=\"" + sourceGUID + "\"}\n";
            string targetRef = "GUID=\"" + targetGUID + "\"\n";
            const string connProperties = "notes=\"\"\nqualifier=\"\"\nordered=\"0\"\nscope=\"\"\nmultiplicity=\"\"\nrole=\"\"\nalias=\"\"\ncontainment=\"\"\nstereotype=\"\"\nmembertype=\"\"\nallowduplicates=\"\"\nnavigability=\"\"\nchangeable=\"\"\nconstraint=\"\"\naccess=\"Public\"\naggregation=\"0\"\n";

            string connector = "Generalization\n{\n" + tplHeader + connectorRef + "Source\n{\n" + sourceRef + connProperties + "}\nTarget\n{\n" + targetRef + connProperties + "}\n}\n";

            Logger.WriteInfo("createParentConnector >> Created template: " + connector);
            return connector;
        }

        /// <summary>
        /// This operation receives a class identifier that we have classified as a BDT. Since a BDT must be derived from a CDT, we have to examine the class attributes to determine the value type
        /// and subsequently the type of CDT that we must assign the 'parent role'. A BDT must only have a single 'value attribute', since BDT's must not be constructed. On the other hand, a BDT 
        /// may have multiple supplementary attributes. 
        /// The operation is ONLY called on CIM data types and the chosen approach is to check for the data type of the 'value' attribute (all CIM data types have exactly one such attribute).
        /// The operation is NOT used for OAGIS transformations, since OAGIS BDT's already have the proper base class.
        /// Note that, although in theory we do not need the namespace, this assures that we're searching the correct part of the package tree in case of classes that have been transformed in 
        /// the past and thus exist in multiple namespaces!
        /// </summary>
        /// <param name="namespaceRoot">Namespace root name.</param>
        /// <param name="packagePath">A list of comma-separated package names all the way from transformation root to current package.</param>
        /// <param name="className">Name of the class.</param>
        /// <returns>Empty string in case no suitable translation is possible, otherwise a constructed string: TYPE+space+GUID in which TYPE is one of 'CDTSimpleType' or 'CDTComplexType'</returns>
        internal string DetermineBDTParent(string namespaceRoot, string[] packagePath, string className)
        {
            string targetGUID = string.Empty;                   // Will receive the result
            string targetName = string.Empty;                   // The classifier typename.
            string targetStereoType = string.Empty;             // Will receive the CDT stereotype.
            string resultstring = string.Empty;                 // Will receive overall result: <stereoType> <GUID>
            string packageRoot = packagePath[0];
            string packageName = packagePath[packagePath.Length - 1];

            Logger.WriteInfo("determineBDTParent >> Looking under: " + namespaceRoot + " for classifier: " + className);

            string query = @"SELECT a.Type AS AttribType
                             FROM ((((((((((t_object o INNER JOIN t_attribute a ON o.Object_ID = a.Object_ID) INNER JOIN t_package p ON o.package_id = p.package_id) 
                             LEFT JOIN t_package p1 ON p1.package_id = p.parent_id)
                             LEFT JOIN t_package p2 ON p2.package_id = p1.parent_id)
                             LEFT JOIN t_package p3 ON p3.package_id = p2.parent_id)
                             LEFT JOIN t_package p4 ON p4.package_id = p3.parent_id)
                             LEFT JOIN t_package p5 ON p5.package_id = p4.parent_id)
                             LEFT JOIN t_package p6 ON p6.package_id = p5.parent_id)
                             LEFT JOIN t_package p7 ON p7.package_id = p6.parent_id)
                             LEFT JOIN t_package p8 ON p8.package_id = p7.parent_id)
                             WHERE p.Name = '" + packageName + "' AND o.Name = '" + className + "' AND a.Name like 'value' AND ((p1.Name = '" + packageRoot + "' AND p2.Name = '" + namespaceRoot +
                          "') OR (p2.Name = '" + packageRoot + "' AND p3.Name = '" + namespaceRoot + "') OR (p3.Name = '" + packageRoot + "' AND p4.Name = '" + namespaceRoot +
                          "') OR (p4.Name = '" + packageRoot + "' AND p5.Name = '" + namespaceRoot + "') OR (p5.Name = '" + packageRoot + "' AND p6.Name = '" + namespaceRoot +
                          "') OR (p6.Name = '" + packageRoot + "' AND p7.Name = '" + namespaceRoot + "') OR (p7.Name = '" + packageRoot + "' AND p8.Name = '" + namespaceRoot + "'));";

            var queryResult = new XmlDocument();                    // Repository query will return an XML Document.
            queryResult.LoadXml(this._repository.SQLQuery(query));                // Execute query and store result in XML Document.
            XmlNodeList elements = queryResult.GetElementsByTagName("Row"); // Retrieve all the individual Classifier identifiers (should be only one).

            if (elements.Count >= 1)
            {
                //We found the attribute, must be on the first (and hopefully only) index!
                string attribType = elements[0]["AttribType"].InnerText.Trim();
                KeyValuePair<string, string> targetClass = TransformPRIMReference(attribType.ToLower());
                Logger.WriteInfo("DetermineBDTParent>> Going to determine target for type: " + attribType);
                if (targetClass.Key != string.Empty)
                {
                    targetName = targetClass.Key;
                    targetGUID = targetClass.Value;

                    // Now we have to figure-out whether this is a primitive- or a complex type...
                    ClassifierManagerSlt.ClassifierIdentity id = ClassifierManagerSlt.GetClassifierManagerSlt().FindClassifierByName(targetName);
                    if (id != null) targetStereoType = id.StereoType;
                }
            }
            Logger.WriteInfo("DetermineBDTParent >> Found target name: " + targetName + " with GUID: " + targetGUID + " and stereoType: " + targetStereoType);
            if (targetGUID != "") resultstring = targetStereoType + " " + targetGUID;
            Logger.WriteInfo("DetermineBDTParent>> Returning: " + resultstring);
            return resultstring;
        }

        /// <summary>
        /// This operation receives a class identifier that we have classified as an EDSN LDT. Since EDSN derives LDT's directly from a PRIM, we have to 
        /// replace this PRIM parent by the correct CDT parent. A helper function, transformEDSNLogicalDataType, is used to retrieve the correct parent type + GUID.
        /// </summary>
        /// <param name="className">Name of class to be evaluated.</param>
        /// <param name="classGUID">GUID of the class.</param>
        /// <returns>Empty string in case no suitable translation is possible, otherwise a constructed string: TYPE+space+GUID in which TYPE is one of 'CDTSimpleType' or 'CDTComplexType'</returns>
        internal string DetermineEDSNLDTParent(string className, string classGUID)
        {
            string targetGUID = string.Empty;                   // Will receive the result
            string targetName = string.Empty;                   // The classifier typename.
            string targetStereoType = string.Empty;             // Will receive the CDT stereotype.
            string resultstring = string.Empty;                 // Will receive overall result: <stereoType> <GUID>

            Logger.WriteInfo("determineEDSNLDTParent >> Going to determine target for type: " + className);
            KeyValuePair<string, string> CDT = TransformEDSNLogicalDataType(this._repository, className, classGUID);

            if (CDT.Key != string.Empty)
            {
                //We found a proper parent!
                targetName = CDT.Key;
                targetGUID = CDT.Value;

                // Now we have to figure-out whether this is a primitive- or a complex type...
                ClassifierManagerSlt.ClassifierIdentity id = ClassifierManagerSlt.GetClassifierManagerSlt().FindClassifierByName(targetName);
                if (id != null) targetStereoType = id.StereoType;
            }
            Logger.WriteInfo("determineEDSNLDTParent >> Found target name: " + targetName + " with GUID: " + targetGUID + " and stereoType: " + targetStereoType);
            if (targetGUID != "") resultstring = targetStereoType + " " + targetGUID;
            Logger.WriteInfo("determineEDSNLDTParent >> Returning: " + resultstring);
            return resultstring;
        }

        /// <summary>
        /// Receives a CDT GUID and returns the associated class name.
        /// If we have a match on the GUID, we always return the root-name in order to get rid of alias types...
        /// </summary>
        /// <param name="GUID">The GUID of the class we want to check.</param>
        /// <returns>Name of the class or empty string if nothing found.</returns>
        internal string GetCDTNameByGUID(string GUID)
        {
            ClassifierManagerSlt.ClassifierIdentity id = ClassifierManagerSlt.GetClassifierManagerSlt().FindClassifierByGUID(GUID);
            return (id != null) ? id.RootName : string.Empty;
        }

        /// <summary>
        /// The method checks whether the given class name is either a primitive type or a CDT. This is primarily used to check whether we receive enumeration declarations that already have
        /// a base class (which we don't allow since we explicitly add a base class to enumerations)!
        /// We also check whether the provided name exists as one of the HR-XML 'Qualified Base Classes', these are considered CDT as well.
        /// </summary>
        /// <param name="className">The name of the class we want to check.</param>
        /// <returns>"true" if this is a primitive or a CDT type.</returns>
        internal string IsPrimitiveOrCDT(string className)
        {
            string result = "false";
            Logger.WriteInfo("isPrimitiveOrCDT >> checking whether " + className + " is a PRIM or a CDT");

            // First of all, we check whether we're dealing with one of the HR-XML Qualified Data Types. Whe consider these to be 'CDT' as well...
            // We check this by attempting to translate the class to a CDT GUI. If successfull, the class was an HR-XML Qualified Data Type.
            string[] isHRXMLCDT = ReplaceQualifiedCDTParent(className);
            if (isHRXMLCDT != null && isHRXMLCDT[0] != string.Empty)
            {
                Logger.WriteInfo("isPrimitiveOrCDT >> This is an HR-XML CDT!");
                result = "true";
            }
            else
            {
                // We are checking the CDT/PRIM type-cache for occurence of the classname...     
                ClassifierManagerSlt.ClassifierIdentity id = ClassifierManagerSlt.GetClassifierManagerSlt().FindClassifierByName(className);
                if (id != null) result = "true";
            }
            Logger.WriteInfo("isPrimitiveOrCDT >> result of check: " + result);
            return result;
        }

        /// <summary>
        /// Explicitly called by the transformation scripts and used to load specified types. 
        /// The function is called thrice, once to load the Primitive types, once to load the CDT types based on those primitives and
        /// once to load the ECDM Data Types.
        /// This results in a classifier list that contains name/GUID tuples for the PRIM and CDT/BDT types.
        /// </summary>
        /// <param name="fullyQualifiedName">The Fully Qualified Name of the package to be loaded.</param>
        /// <param name="isPrim">Whether these are primitive types ('PRIM') or core data types ('CDT').</param>
        /// <returns>"Ok" or "Error"</returns>
        internal string LoadClassifierTypes(string fullyQualifiedName, bool isPrim)
        {
            return ClassifierManagerSlt.GetClassifierManagerSlt().LoadClassifierTypes(fullyQualifiedName, isPrim);
        }

        /// <summary>
        /// This operation is called when transforming the target of a class association. It verifies whether this target is a CDT and if this is the case,
        /// the operation returns the GUID of the 'proper' CDT. Note that this might leave a number of unreferenced, 'illegal' CDT declarations in the 
        /// reference model that are not referenced anymore.  
        /// The operation also checks whether the parent of the specified class exists in one of the specified 'suspect' non-CDT packages and if so, replaces
        /// this parent by the proper CDT. 
        /// </summary>
        /// <param name="sourceGUID">Originating class GUID.</param>
        /// <param name="targetClass">Name of target class.</param>
        /// <param name="forbiddenPackages">The list of packages in which the target must NOT reside.</param>
        /// <returns>new association GUID or empty string if nothing to replace.</returns>
        internal string TransformCDTReference(string sourceGUID, string targetClass, string forbiddenPackages)
        {
            string altTargetClass = string.Empty;                           // Used to check for OAGIS primitives
            bool endsWithType = targetClass.EndsWith("type");               // Check whether the name ends with 'type'...

            Logger.WriteInfo("transformCDTReference >> checking whether " + targetClass + " is a CDT (or a PRIM) and whether residing in correct class....");

            // First of all, we're going to check whether the parent is in the correct class...
            string[] newParent = SwapBDTParent(this._repository, sourceGUID, forbiddenPackages);
            if (newParent != null && newParent[0] != string.Empty)
            {
                // We have a new parent GUID, our work is done already!
                Logger.WriteInfo("transformCDTReference >> replaced " + targetClass + " by alternative CDT: " + newParent[0]);
                return newParent[0];
            }

            // OAGIS has defined a number of 'ValueType' variants that are declared as a string. Since this is not allowed (a value is, by definition, a number),
            // we replace these by our CodeType (closest match). If successfull, we are done right away...
            string result = ReplaceIllegalCDT(targetClass);
            if (result != string.Empty)
            {
                // We have replaced targetClass by an alternative CDT. Our work here is done now!
                Logger.WriteInfo("transformCDTReference >> replaced " + targetClass + " by alternative CDT: " + result);
                result = result.Substring(result.IndexOf(" ", StringComparison.Ordinal) + 1);  // Result is space-separated name and GUID. We only need the GUID part!
                return result;
            }

            // The classifier might be an OAGIS CDT declaration...
            if (oagisCDTRegex.Match(targetClass).Success)
            {
                string[] nameParts = targetClass.Split(new Char[] { '_' });  // Break-up in name-part and identifier part;
                targetClass = nameParts[0];                                  // ...and assign the classifier to the name-part, ignore identifier part!
                Logger.WriteInfo("transformCDTReference >> removed OAGIS CDT trailing part " + nameParts[1] + ", resulting in class: " + targetClass);
            }

            // OAGIS also adds the 'type' prefix to primitive types. Since we don't, this would imply that we would miss these. Therefore, we check whether
            // the className ends in 'Type' and if so, create a second match variable with this postfix removed and see if this gives us a 'primitive match'...          
            if (endsWithType)
            {
                altTargetClass = targetClass.Remove(targetClass.LastIndexOf("type", StringComparison.Ordinal));
                Logger.WriteInfo("transformCDTReference >> potential OAGIS primitive detected, created alternative match string: " + altTargetClass);
            }

            // We are checking the CDT type-cache for occurence of the classname...       
            ClassifierManagerSlt.ClassifierIdentity id = ClassifierManagerSlt.GetClassifierManagerSlt().FindClassifierByName(targetClass);
            if (id != null)
            {
                // If we have a match on the name, we return the associated GUID so we assure that the reference is made against the 'real' CDT...
                if (id.Primitive)
                {
                    // We have an association with a primitive type. We do not allow this, so replace with suitable CDT...
                    KeyValuePair<string, string> primResult = TransformPRIMReference(targetClass);
                    if (primResult.Key != string.Empty) result = primResult.Value;
                }
                else result = id.GUID;
            }
            else if (altTargetClass != string.Empty)
            {
                id = ClassifierManagerSlt.GetClassifierManagerSlt().FindClassifierByName(altTargetClass);
                if (id != null)
                {
                    // Found a match for our alternative. Of course, we could have turned a valid XXXType into XXX (e.g. DurationType to Duration).
                    // But since we test the full-name before the alternate name, this is not an issue.
                    KeyValuePair<string, string> primResult = TransformPRIMReference(altTargetClass);
                    if (primResult.Key != string.Empty) result = primResult.Value;
                }
            }
            Logger.WriteInfo((result != "") ? "transformCDTReference >> found: " + result : "transformCDTReference >> not a CDT!");
            return result;
        }

        /// <summary>
        /// This operation is invoked on [OAGIS] BDT checks. Some reference models use 'internal' OAGIS packages that contain references to different CDT/PRIM packages.
        /// We want to replace these by our own packages and thus must inspect these packages. This particular method is called from Class transformation to obtain
        /// the 'complexity stereotype' of the eventual parent class. The operation determince whether we have to switch classes and if so, whether the new parent is
        /// a simple- or a complex type. The actual parent switch is processed from the Connector transformation (TransformCDTReference).
        /// </summary>
        /// <param name="classGUID">GUID of class to be checked.</param>
        /// <param name="className">Name of class to be checked.</param>
        /// <param name="forbiddenPackages">The list of packages in which the target must NOT reside.</param>
        /// <returns>Empty string in case no suitable replacement can be found, or one of 'CDTSimpleType' or 'CDTComplexType'</returns>
        internal string DetermineBDTStereotype(string classGUID, string className, string forbiddenPackages)
        {
            Logger.WriteInfo("determineBDTStereotype >> Examining class '" + className + "' for presence of parent in '" + forbiddenPackages + "'.");

            string[] result = SwapBDTParent(this._repository, classGUID, forbiddenPackages);
            if (result != null && result[0] != string.Empty)
            {
                Logger.WriteInfo("determineBDTStereotype >> Found correct stereotype: " + result[1]);
                return result[1];
            }
            Logger.WriteInfo("determineBDTStereotype >> No replacement found.");
            return string.Empty;
        }

        /// <summary>
        /// Return the GUID of a Core Data Type classifier identified by args[0], if it can be found in the classifiers list...
        /// The operation also attempts to detect whether the associated type is an OAGIS CDT type or even a Primitive type and replaces these with the
        /// proper CDT type before returning the GUID. The operation is thus also used to transform 'illegal' primitive types to the correct types.
        /// </summary>
        /// <param name="attributeName">The name of the attribute for which we're trying to lookup the type.</param>
        /// <param name="classifier">Classifier of attribute type.</param>
        /// <param name="classMetaType">Attribute-owning class meta type: PRIM, CDT, BDT or ABIE/ACC.</param>
        /// <returns>CDT-name+space+CDT-GUID, or empty string is nothing to replace.</returns>
        internal string GetCDTClassifierGUID(string attributeName, string classifier, string classMetaType)
        {
            string result = string.Empty;
            string altClass = classifier + "Type";              // If we do't get an exact match on the provided name, we try with 'Type' attached to the name...
            string alt2Class = string.Empty;                    // We use this one the other way around (without 'Type' attached to the name)...
            bool endsWithType = classifier.EndsWith("type");    // Check whether the type name ends with 'type'...

            Logger.WriteInfo("getCDTClassifierGUID >> looking for attribute: " + attributeName + " with classifier: " + classifier);

            if (classifier == string.Empty)
            {
                Logger.WriteInfo("getCDTClassifierGUID >> Empty classifier, no action!");
                return result;
            }

            result = ReplaceIllegalCDT(classifier);
            if (result != string.Empty)
            {
                // The classifier happened to be one that needed replacement by an alternative type. In this case, we are done now.
                Logger.WriteInfo("getCDTClassifierGUID >> We replaced " + classifier + " by an alternative CDT. Returning GUID: " + result);
                return result;
            }

            // OAGIS also adds the 'type' prefix to primitive types. Since we don't, this would imply that we would miss these. Therefore, we check whether
            // the className ends in 'Type' and if so, create a second match variable with this postfix removed and see if this gives us a 'primitive match'...          
            if (endsWithType)
            {
                alt2Class = classifier.Remove(classifier.LastIndexOf("type"));
                Logger.WriteInfo("getCDTClassifierGUID >> potential OAGIS primitive detected, created alternative match string: " + alt2Class);
            }

            // The classifier might be an OAGIS CDT declaration...
            if (oagisCDTRegex.Match(classifier).Success)
            {
                string[] nameParts = classifier.Split(new Char[] { '_' });  // Break-up in name-part and identifier part;
                classifier = nameParts[0];                                // ...and assign the classifier to the name-part, ignore identifier part!
                Logger.WriteInfo("getCDTClassifierGUID >> removed OAGIS CDT trailing part " + nameParts[1] + ", resulting in classifier: " + classifier);
            }

            if (classMetaType == "ABIE")
            {
                // We're in a component (ABIE or ACC) that might contain one or more type classifiers that refer to primitive types. These must be replaced by CDT's...
                // Note that Date, Time and DateTime are automatically mapped against the CDT versions since we removed the PRIM versions from the classifiers list! 
                switch (classifier)
                {
                    // Booleans are mapped against the CDT 'Indicator'.
                    case "boolean":
                        classifier = "indicatortype";
                        break;

                    // Since floating point values do not exist as such in XSD's, we map these to the 'ValueType' CDT, which is the most generic decimal primitive.
                    // The result might require some adjustments in case we got the context wrong (which is unknown at this point).
                    case "float":
                        classifier = "valuetype";
                        break;

                    // Percents are mapped against the CDT 'Percent'.
                    case "percent":
                        classifier = "percenttype";
                        break;

                    // All integer values are mapped to the most generic numeric CDT, which is 'NumericType'
                    case "integer":
                        classifier = "numerictype";
                        break;

                    // We attempt to guess the context looking at the name of the attribute. If this contains the string 'name', we guess this is a name.
                    // In case it ends with the string 'id', we guess this is an identifier...
                    // Otherwise, we map to the most generic string type, which is 'TextType'...  
                    case "normalizedstring":
                    case "string":
                        if (attributeName.IndexOf("name") != -1) classifier = "nametype";
                        else if (attributeName.EndsWith("id")) classifier = "identifiertype";
                        else classifier = "texttype";
                        break;

                    default:
                        // Leave existing value alone.
                        break;
                }
            }

            // Going to look in the CDT/PRIM cache for a matching classifier...
            ClassifierManagerSlt.ClassifierIdentity id = ClassifierManagerSlt.GetClassifierManagerSlt().FindClassifierByName(classifier);
            if (id != null)
            {
                // If we have a match on the name, we always return the root-name in order to get rid of alias types...
                result = id.RootName + " " + id.GUID;
            }
            else
            {
                id = ClassifierManagerSlt.GetClassifierManagerSlt().FindClassifierByName(altClass);
                if (id != null)
                {
                    // If we have a match on the alternate name (ends with 'type'), we always return the root-name in order to get rid of alias types...
                    result = id.RootName + " " + id.GUID;
                }
                else if (alt2Class != string.Empty)
                {
                    id = ClassifierManagerSlt.GetClassifierManagerSlt().FindClassifierByName(alt2Class);
                    if (id != null)
                    {
                        // Found a match for our second alternative ('type' extension explicitly removed). Of course, we could have turned a valid XXXType 
                        // into XXX (e.g. DurationType to Duration). But since we test the full-name before the alternate name, this is not an issue.
                        // We only get a match here in case we received a classifier name that ends with 'type' and that matches an entry in the PRIM list...
                        KeyValuePair<string, string> altResult = TransformPRIMReference(alt2Class);
                        if (altResult.Key != string.Empty) result = altResult.Key + " " + altResult.Value;
                    }
                }
            }
            Logger.WriteInfo((result != string.Empty) ? "getCDTClassifierGUID >> found: " + result : "getCDTClassifierGUID >> classifier not found!");
            return result;
        }

        /// <summary>
        /// Return the GUID of a Primitive Data Type classifier identified by args[0], if it can be found in the classifiers list...
        /// The operation also attempts to detect whether the associated type is an OAGIS CDT type or even a Primitive type and replaces these with the
        /// proper PRIM type before returning the GUID. The operation is thus also used to transform 'illegal' primitive types to the correct types.
        /// This operation must ONLY be called on attributes that are destined to be XSD Atrributes!!
        /// </summary>
        /// <param name="attributeName">The name of the attribute for which we're trying to lookup the type.</param>
        /// <param name="classifier">Classifier of attribute type.</param>
        /// <param name="classMetaType">Attribute-owning class meta type: PRIM, CDT, BDT or ABIE/ACC.</param>
        /// <returns>PRIM-name+space+PRIM-GUID, or empty string is nothing to replace.</returns>
        internal string GetPRIMClassifierGUID(string attributeName, string classifier, string classMetaType)
        {
            string result = string.Empty;
            string altClass = string.Empty;
            string alt2Class = classifier + "type";             // We use this to catch CDT's that do not have a 'type' postfix.
            bool endsWithType = classifier.EndsWith("type");    // Check whether the type name ends with 'type'...
            bool replacedClassifier = false;

            // This table is used to translate from CDT to PRIM...
            // We added some 'alternative' CDT's used by OAGIS: NumberType, OrdinalType and IDType. These are non-standard and must be mapped to proper primitives.
            string[,] translateTable =
                {{"amounttype",             "decimal"},
                 {"anytype",                "anyType"},
                 {"binaryobjecttype",       "binary"},
                 {"codetype",               "normalizedstring"},
                 {"datetimetype",           "datetime"},
                 {"datetype",               "date"},
                 {"durationtype",           "duration"},
                 {"graphictype",            "binary"},
                 {"idtype",                 "normalizedstring"},
                 {"identifiertype",         "normalizedstring"},
                 {"indicatortype",          "boolean"},
                 {"measuretype",            "decimal"},
                 {"nametype",               "string"},
                 {"numerictype",            "decimal"},
                 {"numbertype",             "decimal"},
                 {"ordinaltype",            "decimal"},
                 {"percenttype",            "decimal"},
                 {"picturetype",            "binary"},
                 {"quantitytype",           "decimal"},
                 {"ratetype",               "decimal"},
                 {"ratiotype",              "decimal"},
                 {"soundtype",              "binary"},
                 {"texttype",               "string"},
                 {"timetype",               "time"},
                 {"valuetype",              "decimal"},
                 {"videotype",              "binary"} };

            Logger.WriteInfo("getPRIMClassifierGUID >> looking for attribute: " + attributeName + " with classifier: " + classifier);

            // Since PRIM classifier types are only allowed to be used in CDT, BDT or PRIM components, check the meta type for correctness...
            if (classifier == string.Empty)
            {
                Logger.WriteInfo("getPRIMClassifierGUID >> Empty classifier, give-up!");
                return result;
            }
            else if (classifier == "float")
            {
                //Float is sometimes used as a primitive type, we replace this by 'decimal'...
                Logger.WriteInfo("getPRIMClassifierGUID >> Replaced 'float' by 'decimal'");
                classifier = "decimal";
            }
            else if (oagisCDTRegex.Match(classifier).Success)
            {
                // The classifier might be an OAGIS CDT declaration...
                string[] nameParts = classifier.Split(new Char[] { '_' });  // Break-up in name-part and identifier part;
                classifier = nameParts[0];                                // ...and assign the classifier to the name-part, ignore identifier part!
                Logger.WriteInfo("getPRIMClassifierGUID >> removed OAGIS CDT trailing part " + nameParts[1] + ", resulting in classifier: " + classifier);
                replacedClassifier = true;
            }

            // First of all, we check whether the classifier is a CDT. If so, we replace it by the appropriate PRIM type...
            for (int i = 0; i < translateTable.GetLength(0); i++)
            {
                if (string.Compare(classifier, translateTable[i, 0], StringComparison.Ordinal) == 0 || 
            	    string.Compare(alt2Class, translateTable[i, 0], StringComparison.Ordinal) == 0)
                {
                    Logger.WriteInfo("getPrimClassifierGUID >> Found CDT, going to swap against replacement PRIM: " + translateTable[i, 1]);
                    classifier = translateTable[i, 1];
                    replacedClassifier = true;
                    break;
                }
            }

            // In case the original classifier name ends with 'type', we remove this; all valid 'types' have been checked above when trying to convert from CDT to PRIM so
            // the only alternatives would be PRIM names with 'type' attached to them...
            // If the classifier has been replaced by an alternative, don't bother with the alternative, we now look for a different classifier...
            if (!replacedClassifier && endsWithType)
            {
                altClass = classifier.Remove(classifier.LastIndexOf("type", StringComparison.Ordinal));
                Logger.WriteInfo("getPRIMClassifierGUID >> potential OAGIS primitive detected, created alternative match string: " + altClass);
            }

            // Going to look in the CDT/PRIM cache for a matching classifier...
            // Note that we do not check whether the match is a PRIM, since we got rid of all CDT's earlier on using the translate table...
            Logger.WriteInfo("getPRIMClassifierGUID >> Going to look for possible PRIM: " + classifier);
            ClassifierManagerSlt.ClassifierIdentity id = ClassifierManagerSlt.GetClassifierManagerSlt().FindClassifierByName(classifier);
            if (id != null) result = id.RootName + " " + id.GUID;
            else
            {
                id = ClassifierManagerSlt.GetClassifierManagerSlt().FindClassifierByName(altClass);
                if (id != null) result = id.RootName + " " + id.GUID;
            }
            Logger.WriteInfo((result != string.Empty) ? "getPRIMClassifierGUID >> found: " + result : "getPRIMClassifierGUID >> classifier not found!");
            return result;
        }

        /// <summary>
        /// Return the GUID of any existing classifier that is present 'somewhere' in the namespace identified by args[0]. The operation first attempts to find the
        /// classifier in the current package. If not found, the search will be widened to any package that is part of the provided namespace root. 
        /// The operation ALWAYS skips the CCLibrary and PRIMLibrary (the first because it is not referenced by any reference model and the second since no
        /// references to PRIM are allowed, other then via the CDT types.
        /// 
        /// Known limitations: If a classifier exists in the current package, but has not been transformed yet (e.g. does not yet exist in the new namespace), but it DOES 
        /// exist in another package in the new namespace, the function will select this classifier as the target, even though this might be completely wrong!
        /// Solution would be to copy the original FQN for the qualifier and look in the new namespace for a similar FQN. Not sure this is feasible, though.
        /// 
        /// </summary>
        /// <param name="namespaceRoot">Root of the namespace to be searched.</param>
        /// <param name="packagePath">Current package path (e.g. the dot-separated path from the namespace root to the package that is currently being transformed).</param>
        /// <param name="classifier">Classifier to be located (e.g. Type Name).</param>
        /// <returns>GUID of classifier or empty string if not found.</returns>
        internal object FindClassifierGUID(string namespaceRoot, string[] packagePath, string classifier)
        {
            string packageRoot = packagePath[0];
            string packageName = packagePath[packagePath.Length - 1];
            string classGUID = FindGUIDInPackage(this._repository, namespaceRoot, packageRoot, packageName, classifier);

            Logger.WriteInfo("findClassifierGUID >> searching namespace " + namespaceRoot + "." + " for classifier: " + classifier);
            if (classGUID != string.Empty)
            {
                // We have located the GUID in the current package. This has preference over a full namespace search!
                Logger.WriteInfo("findClassifierGUID >> Retrieved GUID: " + classGUID + " from current package: " + packageName);
                return classGUID;
            }
            else if (packageName.StartsWith("pain.")) // THIS IS A DIRTY HACK, REQUIRED BECAUSE OF FAILING TRANSFORM_CLASSIFIER FOR THE ISO PACKAGE. Sorry :-(
            {
                // Since ISO20022 replicates most declarations across the packages, search 'outside' the current package is a bad idea since this would return
                // Classifiers from other ISO packages with the same name, but different context. Bad design of the ISO20022 model!!!
                // This combined with a TRANSFORM_CLASSIFIER that fails to work properly on the ISO package, we simply reduce the search an run the transformation
                // twice: first time to create all classifiers, second time to complete the references.
                Logger.WriteInfo("findClassifierGUID >> Classifier not in current package and package is part of ISO20022, DO NOT SEARCH ELSEWHERE!");
                return string.Empty;
            }

            // We did not find the GUID in the package specified. It might still be in the cache for a different package. Let's check...
            if (this._classifiersCache.ContainsKey(classifier))
            {
                // The hashtable uses the classifier name as a key and the result is a sorted list of package/GUID combinations.
                var GUIDList = (SortedList)this._classifiersCache[classifier];
                foreach (DictionaryEntry de in GUIDList)
                {
                    // To be shure, we check the packageName. Since we did not return a result from findGUIDInPackage, this should not be neccessary 
                    // but better be safe then sorry...
                    if ((string)de.Key != packageName)
                    {
                        classGUID = (string)de.Value;
                        Logger.WriteInfo("findClassifierGUID >> found GUID " + classGUID + " in cache for package: " + (string)de.Key);
                        return classGUID;
                    }
                }
            }

            string query = @"SELECT o.ea_guid AS classGUID, p.Name as repoPackageName
                             FROM (((((((((t_object o INNER JOIN t_package p ON o.package_id = p.package_id) 
                             LEFT JOIN t_package p1 ON p1.package_id = p.parent_id)
                             LEFT JOIN t_package p2 ON p2.package_id = p1.parent_id)
                             LEFT JOIN t_package p3 ON p3.package_id = p2.parent_id)
                             LEFT JOIN t_package p4 ON p4.package_id = p3.parent_id)
                             LEFT JOIN t_package p5 ON p5.package_id = p4.parent_id)
                             LEFT JOIN t_package p6 ON p6.package_id = p5.parent_id)
                             LEFT JOIN t_package p7 ON p7.package_id = p6.parent_id)
                             LEFT JOIN t_package p8 ON p8.package_id = p7.parent_id)
                             WHERE p.Name <> '" + packageName + "' AND o.Name = '" + classifier + "' AND ((p1.Name = '" + packageRoot + "' AND p2.Name = '" + namespaceRoot +
            "') OR (p2.Name = '" + packageRoot + "' AND p3.Name = '" + namespaceRoot + "') OR (p3.Name = '" + packageRoot + "' AND p4.Name = '" + namespaceRoot +
            "') OR (p4.Name = '" + packageRoot + "' AND p5.Name = '" + namespaceRoot + "') OR (p5.Name = '" + packageRoot + "' AND p6.Name = '" + namespaceRoot +
            "') OR (p6.Name = '" + packageRoot + "' AND p7.Name = '" + namespaceRoot + "') OR (p7.Name = '" + packageRoot + "' AND p8.Name = '" + namespaceRoot + "'));";

            Logger.WriteInfo("findClassifierGUID >> Going to execute query: " + query);

            var queryResult = new XmlDocument();                    // Repository query will return an XML Document.
            queryResult.LoadXml(this._repository.SQLQuery(query));                // Execute query and store result in XML Document.
            XmlNodeList elements = queryResult.GetElementsByTagName("Row"); // Retrieve all the individual Classifier identifiers.

            Logger.WriteInfo("findClassifierGUID >> Result: " + queryResult.InnerText);

            if (elements.Count > 0)
            {
                classGUID = elements[0]["classGUID"].InnerText.Trim();                  // Classifier GUID.
                string repoPackage = elements[0]["repoPackageName"].InnerText.Trim();   // The first package in which we found the classifier.
                if (classGUID != "")
                {
                    if (this._classifiersCache.ContainsKey(classifier))
                    {
                        // We already have this in the cache, make sure that we do not have this combination of package and GUID before adding it to the cache....
                        // This should not be neccessary since we already searched the cache, but again, better be safe then sorry...
                        if (!((SortedList)this._classifiersCache[classifier]).ContainsKey(repoPackage))
                        {
                            ((SortedList)this._classifiersCache[classifier]).Add(repoPackage, classGUID);
                            Logger.WriteInfo("findClassifierGUID >> classifier found in repository with GUID " + classGUID + ". Also in cache! Added a new cache entry for package: " + repoPackage);
                        }
                    }
                    else
                    {
                        // Not in the cache as of yet, create a new list to keep track of package/classifier combinations...
                        var classifierList = new SortedList
                        {
                            { repoPackage, classGUID }
                        };
                        this._classifiersCache.Add(classifier, classifierList);
                        Logger.WriteInfo("findClassifierGUID >> classifier found in repository with GUID " + classGUID + ". Not yet in cache. Added a new cache entry for package: " + repoPackage);
                    }
                }
                if (elements.Count > 1)
                {
                    Logger.WriteInfo("findClassifierGUID >> **WARNING: multiple occurences found in repository, first one is used!");
                }
            }
            Logger.WriteInfo((classGUID != string.Empty) ? "findClassifierGUID >> found: " + classGUID : "findClassifierGUID >> classifier not found!");
            return classGUID;
        }

        /// ------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// Local helper functions...
        ///

        /// <summary>
        /// Constructor is declared private in order to avoid direct construction.
        /// </summary>
        private EAModelTransformSlt()
        {
            this._classifiersCache = null;
            this._EDSNMappingTable = null;
            this._repository = null;
        }

        /// <summary>
        /// This helper function attempts to locate the GUID of the given classifier in the given package
        /// This package must be present in the given namespace!
        /// Parameters are:
        /// repository      = The EA repository to be searched;
        /// namespaceRoot   = The namespace within the repository to be searched (e.g. the 'root' package);
        /// packageRoot     = The top of the package tree that is being processed (must be direct child of 'namespaceName')!
        /// packageName     = We would like the classifier to be located in this package if possible;
        /// classifier      = The classifier to be located.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="namespaceRoot"></param>
        /// <param name="packageRoot"></param>
        /// <param name="packageName"></param>
        /// <param name="classifier"></param>
        /// <returns></returns>
        private string FindGUIDInPackage(EA.Repository repository, string namespaceRoot, string packageRoot, string packageName, string classifier)
        {
            string classGUID = string.Empty;

            Logger.WriteInfo("findGUIDInPackage >> looking for classifier: " + classifier + " in package: " + namespaceRoot + "." + packageRoot + "..." + packageName);
            if (this._classifiersCache.ContainsKey(classifier))
            {
                // The hashtable uses the classifier name as a key and the result is a sorted list of package/GUID combinations.
                var GUIDList = (SortedList)this._classifiersCache[classifier];
                if (GUIDList.ContainsKey(packageName))
                {
                    classGUID = (string)GUIDList[packageName];
                    Logger.WriteInfo("findGUIDInPackage >> found GUID " + classGUID + " in cache for specified package!");
                }
            }
            else
            {
                // Search repository for the given classifier in the given package...
                string query = @"SELECT o.ea_guid AS classGUID
                             FROM (((((((((t_object o INNER JOIN t_package p ON o.package_id = p.package_id) 
                             LEFT JOIN t_package p1 ON p1.package_id = p.parent_id)
                             LEFT JOIN t_package p2 ON p2.package_id = p1.parent_id)
                             LEFT JOIN t_package p3 ON p3.package_id = p2.parent_id)
                             LEFT JOIN t_package p4 ON p4.package_id = p3.parent_id)
                             LEFT JOIN t_package p5 ON p5.package_id = p4.parent_id)
                             LEFT JOIN t_package p6 ON p6.package_id = p5.parent_id)
                             LEFT JOIN t_package p7 ON p7.package_id = p6.parent_id)
                             LEFT JOIN t_package p8 ON p8.package_id = p7.parent_id)
                             WHERE p.Name = '" + packageName + "' AND o.Name = '" + classifier + "' AND ((p1.Name = '" + packageRoot + "' AND p2.Name = '" + namespaceRoot +
                            "') OR (p2.Name = '" + packageRoot + "' AND p3.Name = '" + namespaceRoot + "') OR (p3.Name = '" + packageRoot + "' AND p4.Name = '" + namespaceRoot +
                            "') OR (p4.Name = '" + packageRoot + "' AND p5.Name = '" + namespaceRoot + "') OR (p5.Name = '" + packageRoot + "' AND p6.Name = '" + namespaceRoot +
                            "') OR (p6.Name = '" + packageRoot + "' AND p7.Name = '" + namespaceRoot + "') OR (p7.Name = '" + packageRoot + "' AND p8.Name = '" + namespaceRoot + "'));";

                Logger.WriteInfo("findGUIDInPackage >> Going to execute query: " + query);

                var queryResult = new XmlDocument();                    // Repository query will return an XML Document.
                queryResult.LoadXml(repository.SQLQuery(query));                // Execute query and store result in XML Document.
                XmlNodeList elements = queryResult.GetElementsByTagName("Row"); // Retrieve all the individual Classifier identifiers.

                Logger.WriteInfo("findGUIDInPackage >> Result: " + queryResult.InnerText);

                if (elements.Count > 0)
                {
                    classGUID = elements[0]["classGUID"].InnerText.Trim();
                    if (classGUID != string.Empty)
                    {
                        if (this._classifiersCache.ContainsKey(classifier))
                        {
                            // We already have this in the cache (but should be in a different package otherwise we would have found it earlier)....
                            ((SortedList)this._classifiersCache[classifier]).Add(packageName, classGUID);
                            Logger.WriteInfo("findGUIDInPackage >> classifier found in repository with GUID " + classGUID + ". Also in cache! Added a new cache entry for package: " + packageName);
                        }
                        else
                        {
                            // Not on the cache as of yet, create a new list to keep track of package/classifier combinations...
                            var classifierList = new SortedList
                            {
                                { packageName, classGUID }
                            };
                            this._classifiersCache.Add(classifier, classifierList);
                            Logger.WriteInfo("findGUIDInPackage >> classifier found in repository with GUID " + classGUID + ". Not yet in cache. Added a new cache entry for package: " + packageName);
                        }
                    }
                }
                if (elements.Count > 1) Logger.WriteInfo("findGUIDInPackage >> **WARNING: found multiple instances of same classifier in same package, only the first one is used!");
            }
            Logger.WriteInfo((classGUID != string.Empty) ? "findGUIDInPackage >> returns: " + classGUID : "findGUIDInPackage >> classifier not found!");
            return classGUID;
        }

        /// <summary>
        /// This operation checks whether an existing specialization connector exists between the 'to be copied version' of the given child class (identified by child package/name) 
        /// and a parent class (identified by either name or GUID). If no existing association exists, a new GUID is generated. If the connector exists, an empty string is returned.
        /// Parameters are:
        /// repository   = The EA repository to be searched;
        /// packageRoot  = The top of the package namespace that contains the specified childPackage
        /// childPackage = Package name of source class;
        /// childName    = Name of source class;
        /// parentName   = Name of parent class or 'GUID' if parent is identified by GUID;
        /// parentGUID   = GUID of parent class or empty string if identified by name;
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="packageRoot"></param>
        /// <param name="childPackage"></param>
        /// <param name="childName"></param>
        /// <param name="parentName"></param>
        /// <param name="parentGUID"></param>
        /// <returns>New GUID if association does not exist or empty string on existing association.</returns>
        private string GetParentGUID(EA.Repository repository, string packageRoot, string childPackage, string childName, string parentName, string parentGUID)
        {
            string connectorGUID = string.Empty;  // This becomes the GUID of the newly created connector. 

            Logger.WriteInfo("getParentGUID >> Going to find parent " + ((parentName != "GUID") ? parentName : parentGUID) + " for child: " + packageRoot + "..." + childPackage + "." + childName);

            // We receive either the name of the parent class, or its GUID. We build the correct query accordingly...
            string query;
            if (parentName != "GUID")
            {
                query = @"SELECT c.ea_guid AS connectorGUID
                          FROM (((((((((((t_connector c INNER JOIN t_object o ON c.Start_Object_ID = o.Object_ID) 
                                LEFT JOIN t_object o2 ON c.End_Object_ID = o2.Object_ID)
                                LEFT JOIN t_package p ON p.Package_ID = o.Package_ID)
                                LEFT JOIN t_package p1 ON p1.package_id = p.parent_id)
                                LEFT JOIN t_package p2 ON p2.package_id = p1.parent_id)
                                LEFT JOIN t_package p3 ON p3.package_id = p2.parent_id)
                                LEFT JOIN t_package p4 ON p4.package_id = p3.parent_id)
                                LEFT JOIN t_package p5 ON p5.package_id = p4.parent_id)
                                LEFT JOIN t_package p6 ON p6.package_id = p5.parent_id)
                                LEFT JOIN t_package p7 ON p7.package_id = p6.parent_id)
                                LEFT JOIN t_package p8 ON p8.package_id = p7.parent_id)
                          WHERE o.Name = '" + childName + "' AND c.Connector_Type = 'Generalization' AND p.Name = '" + childPackage + "' AND o2.Name = '" + parentName +
                          "' AND (p1.Name = '" + packageRoot + "' OR p2.Name = '" + packageRoot + "' OR p3.Name = '" + packageRoot + "' OR p4.Name = '" + packageRoot +
                          "' OR p5.Name = '" + packageRoot + "' OR p6.Name = '" + packageRoot + "' OR p7.Name = '" + packageRoot + "' OR p8.Name = '" + packageRoot + "');";
            }
            else
            {
                query = @"SELECT c.ea_guid AS connectorGUID
                          FROM (((((((((((t_connector c INNER JOIN t_object o ON c.Start_Object_ID = o.Object_ID) 
                                LEFT JOIN t_object o2 ON c.End_Object_ID = o2.Object_ID)
                                LEFT JOIN t_package p ON p.Package_ID = o.Package_ID)
                                LEFT JOIN t_package p1 ON p1.package_id = p.parent_id)
                                LEFT JOIN t_package p2 ON p2.package_id = p1.parent_id)
                                LEFT JOIN t_package p3 ON p3.package_id = p2.parent_id)
                                LEFT JOIN t_package p4 ON p4.package_id = p3.parent_id)
                                LEFT JOIN t_package p5 ON p5.package_id = p4.parent_id)
                                LEFT JOIN t_package p6 ON p6.package_id = p5.parent_id)
                                LEFT JOIN t_package p7 ON p7.package_id = p6.parent_id)
                                LEFT JOIN t_package p8 ON p8.package_id = p7.parent_id)
                          WHERE o.Name = '" + childName + "' AND c.Connector_Type = 'Generalization' AND p.Name = '" + childPackage + "' AND o2.ea_guid = '" + parentGUID +
                          "' AND (p1.Name = '" + packageRoot + "' OR p2.Name = '" + packageRoot + "' OR p3.Name = '" + packageRoot + "' OR p4.Name = '" + packageRoot +
                          "' OR p5.Name = '" + packageRoot + "' OR p6.Name = '" + packageRoot + "' OR p7.Name = '" + packageRoot + "' OR p8.Name = '" + packageRoot + "');";
            }

            Logger.WriteInfo("getParentGUID >> Going to execute query: " + query);

            var queryResult = new XmlDocument();                    // Repository query will return an XML Document.
            queryResult.LoadXml(repository.SQLQuery(query));                // Execute query and store result in XML Document.
            XmlNodeList elements = queryResult.GetElementsByTagName("Row"); // Retrieve result row(s)

            Logger.WriteInfo("getParentGUID >> Result: " + queryResult.InnerText);

            // We are only interested in the fact that the association exists. We are NOT interested in the actual result, since we're not going to use this anyway.
            // If the association exists, there is no need to create a connector. If the association does NOT exist, we need to create a GUID to identify the new
            // association. 
            if (elements.Count == 0)
            {
                Logger.WriteInfo("getParentGUID >> No existing/valid connector found, going to create a new GUID!");
                connectorGUID = "{" + Guid.NewGuid().ToString() + "}";
            }
            Logger.WriteInfo("getParentGUID >> Returning GUID: " + connectorGUID);
            return connectorGUID;
        }

        /// <summary>
        /// Helper operation that checks whether the parent of 'className' exists as a qualified data type in one of the 'forbiddenPackages'. 
        /// If so, the current parent is replaced by a suitable CDT replacement type and the operation returns both the complexity stereotype 
        /// as well as the GUID of the new parent. If the class does not occur in the forbiddenPackages, the operation returns null.
        /// </summary>
        /// <param name="repository">EA repository to be queried.</param>
        /// <param name="classGUID">GUID of the child class (to be inspected)</param>
        /// <param name="forbiddenPackages">List of packages that will be checked for occurence of parent class</param>
        /// <returns>NULL when nothing needs to be changed or [0] = GUID and [1] = Stereotype</returns>
        private string[] SwapBDTParent(EA.Repository repository, string classGUID, string forbiddenPackages)
        {
            Logger.WriteInfo("swapBDTParent >> Examining class '" + classGUID + "' for presence of parent in '" + forbiddenPackages + "'.");

            string query = @"SELECT o2.Name AS ParentName, p.Name AS ParentPackageName
                            FROM (((t_connector c INNER JOIN t_object o ON c.Start_Object_ID = o.Object_ID) 
                            LEFT JOIN t_object o2 ON c.End_Object_ID = o2.Object_ID)
                            LEFT JOIN t_package p ON p.Package_ID = o2.Package_ID)
                            WHERE o.ea_guid = '" + classGUID + "' AND c.Connector_Type = 'Generalization';";

            var queryResult = new XmlDocument();                    // Repository query will return an XML Document.
            queryResult.LoadXml(repository.SQLQuery(query));                // Execute query and store result in XML Document.
            XmlNodeList elements = queryResult.GetElementsByTagName("Row"); // Retrieve parent class name and package in which class resides.

            if (elements.Count >= 1)
            {
                //We found the attribute, must be on the first (and hopefully only) index!
                string parentClassName = elements[0]["ParentName"].InnerText.Trim();
                string parentPackage = elements[0]["ParentPackageName"].InnerText.Trim().ToLower();
                Logger.WriteInfo("determineBDTStereotype >> Retrieved parent type '" + parentClassName + "' in package '" + parentPackage + "'.");

                if (forbiddenPackages.Contains(parentPackage))
                {
                    Logger.WriteInfo("determineBDTStereotype >> Found parent in the wrong package, replacement required.....");
                    return ReplaceQualifiedCDTParent(parentClassName);
                }
            }
            Logger.WriteInfo("swapBDTParent >> Nothing to change!");
            return null;
        }

        /// <summary>
        /// Helper method that loads the EDSN LDT Mapping table from a system resource.
        /// </summary>
        /// <returns>True when loaded Ok, false on errors.</returns>
        private bool LoadEDSNMAppingTable()
        {

            Logger.WriteInfo("transformEDSNLogicalDataType >> Loading mapping table...");
            string EDSNMappingInput = ContextSlt.GetContextSlt().GetResourceString(FrameworkSettings._EDSNLDTMappingTable);
            if (string.IsNullOrEmpty(EDSNMappingInput))
            {
                Logger.WriteError("transformEDSNLogicalDataType >> Unable to load mapping table!");
                return false;
            }

            this._EDSNMappingTable = new SortedList<string, string>();
            char[] trimChars = { ' ', '\t', ';', '\r', '\n' };   // Gets rid of spaces, tabs, end-of-token, return and newline chars.
            while (EDSNMappingInput.Length > 0)
            {
                string token = EDSNMappingInput.Substring(0, EDSNMappingInput.IndexOf(';') + 1);  // Read including ';' (will be trimmed of)
                string[] tuple = token.Split(':');
                tuple[0] = tuple[0].Trim(trimChars);
                tuple[1] = tuple[1].Trim(trimChars);
                this._EDSNMappingTable.Add(tuple[0], tuple[1]);
                Logger.WriteInfo("transformEDSNLogicalDataType >> Added Tuple '" + tuple[0] + ":" + tuple[1] + "'.");
                EDSNMappingInput = EDSNMappingInput.Substring(token.Length);
                if (!EDSNMappingInput.Contains(";")) break;     // Quit parsing in case only 'garbage' is left in the string.
            }
            return true;
        }

        /// <summary>
        /// This helper function checks the received class name against a list of "illegal" names. If found, the class is replaced by an alternative CDT and the GUID of this CDT is returned.
        /// An example of use is a set of OAGIS ValueTypes that have been declared a "string" by OAGIS. Since our standards-compliant UN/CEFACT CCTS model does not allow ValueType to be of 
        /// string type, we replace these specific ValueType instances by our generic CodeType instance, since this is the closest match to the 'intent' of these values. Other examples might
        /// occur in the future, in which case we can simply extend the translation table.
        /// Parameters are:
        /// className = the full-name of the CDT class to be evaluated, must be in LOWERCASE!
        /// </summary>
        /// <param name="className"></param>
        /// <returns>Either replacement CDT-Name+space+CDT-GUID or empty string if nothing to replace.</returns>
        private string ReplaceIllegalCDT(string className)
        {
            // This table consists of tuples <CDT name> - <Replacement CDT name>
            // This setup allows extension in the future.
            string[,] translateTable =
                {{"valuetype_e7171e", "CodeType"},
                 {"valuetype_d19e7b", "CodeType"},
                 {"valuetype_55c009", "CodeType"}};

            Logger.WriteInfo("replaceIllegalCDT >> Going to check CDT: " + className);

            string result = string.Empty;
            for (int i = 0; i < translateTable.GetLength(0); i++)
            {
                if (string.Compare(className, translateTable[i, 0], StringComparison.Ordinal) == 0)
                {
                    Logger.WriteInfo("replaceIllegalCDT >> Found illegal CDT, going to swap against replacement CDT: " + translateTable[i, 1]);

                    // We are checking the CDT type-cache for occurence of the replacement classifier name...
                    ClassifierManagerSlt.ClassifierIdentity id = ClassifierManagerSlt.GetClassifierManagerSlt().FindClassifierByName(translateTable[i, 1]);
                    if (id != null)
                    {
                        Logger.WriteInfo("replaceIllegalCDT >> Returning GUID for classifier: " + translateTable[i, 1]);
                        result = id.Name + " " + id.GUID;
                        break;
                    }
                }
            }
            Logger.WriteInfo((result == string.Empty) ? "replaceIllegalCDT >> No replacement!" : "replaceIllegalCDT >> returning: " + result);
            return result;
        }

        /// <summary>
        /// This operation is called in case the specified 'className' resides in qualified datatype package that is not one of our CDT / BDT or PRIM packages.
        ///  In this case, we want to replace this by one of our CDT's.
        /// The operation uses a translation table to locate the qualified type and returns one of our corresponding CDT GUID's. In case of errors or 'not found', the operation
        /// returns an empty string (meaning: do not change current parent).
        /// </summary>
        /// <param name="className">Name of the class to be replaced.</param>
        /// <returns>A string list with [0] = GUID and [1] = Stereotype. Will be empty in case no replacement can be found.</returns>
        private string[] ReplaceQualifiedCDTParent(string className)
        {
            // This table consists of tuples <Type name> - <Replacement CDT name>
            string[,] translateTable =
                {{"daydatetype",                    "datetype"},
                 {"monthdatetype",                  "datetype"},
                 {"doublenumerictype",              "numerictype"},
                 {"durationmeasuretype",            "measuretype"},
                 {"floatnumerictype",               "numerictype"},
                 {"hexbinaryobjecttype",            "binaryobjecttype"},
                 {"integernumerictype",             "numerictype"},
                 {"languagecodetype",               "codetype"},
                 {"monthdaydatetype",               "datetype"},
                 {"negativeintegernumerictype",     "numerictype"},
                 {"nonnegativeintegernumerictype",  "numerictype"},
                 {"nonpositiveintegernumerictype",  "numerictype"},
                 {"normalizedstringtype",           "texttype"},
                 {"positiveintegernumerictype",     "numerictype"},
                 {"stringtype",                     "texttype"},
                 {"tokentype",                      "codetype"},
                 {"uritype",                        "identifiertype"},
                 {"yeardatetype",                   "datetype"},
                 {"yearmonthdatetype",              "datetype"}};

            Logger.WriteInfo("replaceWrongCDTParent >> Going to check CDT: " + className);

            string parentGUID = string.Empty;
            string parentStereotype = string.Empty;
            string replacementName = className;

            for (int i = 0; i < translateTable.GetLength(0); i++)
            {
                if (string.Compare(className, translateTable[i, 0], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    replacementName = translateTable[i, 1];
                    Logger.WriteInfo("replaceWrongCDTParent >> Found wrong parent, going to swap against replacement CDT: " + replacementName);
                    break;
                }
            }

            // We either have a replacement name, or replacementName still contains the original class (which MIGHT be a valid CDT class name already)...
            ClassifierManagerSlt.ClassifierIdentity id = ClassifierManagerSlt.GetClassifierManagerSlt().FindClassifierByName(replacementName);
            if (id != null)
            {
                Logger.WriteInfo("replaceWrongCDTParent >> Returning GUID for classifier: " + replacementName);
                parentGUID = id.GUID;
                parentStereotype = id.StereoType;
            }

            var result = new string[] { parentGUID, parentStereotype };
            Logger.WriteInfo((parentGUID == string.Empty) ? "replaceWrongCDTParent >> No replacement!" : "replaceIllegalCDTParent >> returning: " + parentGUID);
            return result;
        }

        /// <summary>
        /// Helper function that takes an EDSN LDT datatype name and links it to the correct ECDM CDT.
        /// </summary>
        /// <param name="repository">The current EA repository object.</param>
        /// <param name="className">The name of the EDSN LDT Datatype</param>
        /// <param name="classGUID">The GUID of the EDSN LDT Datatype</param>
        /// <returns>A key-value pair containing CDT classifier name as key and the GUID as value.</returns>
        private KeyValuePair<string, string> TransformEDSNLogicalDataType(EA.Repository repository, string className, string classGUID)
        {
            Logger.WriteInfo("transformEDSNLogicalDataType >> Going to transform type: '" + className + "'...");
            var result = new KeyValuePair<string, string>("", ""); // Initialise to empty result (just in case...);

            if ((this._EDSNMappingTable == null) && !LoadEDSNMAppingTable())
            {
                Logger.WriteError("transformEDSNLogicalDataType >> Unable to load mapping table!");
                return result;
            }

            // First of all, we're going to determine the current parent and check whether this is an enumeration....
            string query = @"SELECT o2.Name AS ParentName
                            FROM (((t_connector c INNER JOIN t_object o ON c.Start_Object_ID = o.Object_ID) 
                            LEFT JOIN t_object o2 ON c.End_Object_ID = o2.Object_ID)
                            LEFT JOIN t_package p ON p.Package_ID = o2.Package_ID)
                            WHERE o.ea_guid = '" + classGUID + "' AND c.Connector_Type = 'Generalization';";

            XmlDocument queryResult = new XmlDocument();                    // Repository query will return an XML Document.
            queryResult.LoadXml(repository.SQLQuery(query));                // Execute query and store result in XML Document.
            XmlNodeList elements = queryResult.GetElementsByTagName("Row"); // Retrieve parent class name and package in which class resides.

            if (elements.Count >= 1)
            {
                //We found the parent, must be on the first (and hopefully only) index!
                string parentClassName = elements[0]["ParentName"].InnerText.Trim();
                Logger.WriteInfo("transformEDSNLogicalDataType >> Retrieved parent type '" + parentClassName + "'.");
                if (parentClassName == "enumeration")
                {
                    Logger.WriteInfo("transformEDSNLogicalDataType >> This is an enumeration! Assign EnumType...");
                    // We are checking the CDT type-cache for occurence of the classifier name...
                    ClassifierManagerSlt.ClassifierIdentity id = ClassifierManagerSlt.GetClassifierManagerSlt().FindClassifierByName("EnumType");
                    if (id != null) return new KeyValuePair<string, string>("EnumType", id.GUID);
                }
            }

            // No enumeration, search table for replacement type...
            className = className.ToLower().Trim();
            if (this._EDSNMappingTable.ContainsKey(className))
            {
                string typeName = this._EDSNMappingTable[className];
                Logger.WriteInfo("transformEDSNLogicalDataType >> Found type, going to swap against CDT: " + typeName);

                // We are checking the CDT type-cache for occurence of the classifier name...
                ClassifierManagerSlt.ClassifierIdentity id = ClassifierManagerSlt.GetClassifierManagerSlt().FindClassifierByName(typeName);
                if (id != null) return new KeyValuePair<string, string>(typeName, id.GUID);
            }
            Logger.WriteWarning("transformEDSNLogicalDataType >> Nothing to transform!");
            return result;
        }

        /// <summary>
        /// Helper function that takes a PRIM classifier name, replaces this with a suitable CDT counterpart and returns the GUID of that CDT.
        /// Parameters are:
        /// className = the name of the PRIM class to be replaced, must be in LOWERCASE!
        /// </summary>
        /// <param name="className"></param>
        /// <returns>A key-value pair containing CDT classifier name as key and the GUID as value.</returns>
        private KeyValuePair<string, string> TransformPRIMReference(string className)
        {
            // This table consists of tuples <PRIM name> - <Corresponding CDT name>
            string[,] translateTable =
                {{"binary",             "BinaryObjectType"},
                 {"boolean",            "IndicatorType"},
                 {"date",               "DateType"},
                 {"datetime",           "DateTimeType"},
                 {"decimal",            "NumericType"},
                 {"duration",           "DurationType"},
                 {"float",              "NumericType"},
                 {"integer",            "NumericType"},
                 {"normalizedstring",   "CodeType"},
                 {"string",             "TextType"},
                 {"time",               "TimeType"}};

            Logger.WriteInfo("transformPRIMReference >> Going to transform primitive: " + className);
            var result = new KeyValuePair<string, string>(string.Empty, string.Empty);

            for (int i = 0; i < translateTable.GetLength(0); i++)
            {
                if (string.Compare(className, translateTable[i, 0], StringComparison.Ordinal) == 0)
                {
                    Logger.WriteInfo("transformPRIMReference >> Found primitive, going to swap against CDT: " + translateTable[i, 1]);

                    // We are checking the CDT type-cache for occurence of the classifier name...
                    ClassifierManagerSlt.ClassifierIdentity id = ClassifierManagerSlt.GetClassifierManagerSlt().FindClassifierByName(translateTable[i, 1]);
                    if (id != null) return new KeyValuePair<string, string>(translateTable[i, 1], id.GUID);
                }
            }
            Logger.WriteInfo("transformPRIMReference >> Nothing to transform!");
            return result;
        }
    }
}
