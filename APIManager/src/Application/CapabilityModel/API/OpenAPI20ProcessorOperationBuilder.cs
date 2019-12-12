using System;
using System.Collections.Generic;
using Framework.Logging;
using Framework.Model;
using Framework.Util;
using Framework.Util.SchemaManagement;
using Framework.Util.SchemaManagement.JSON;
using Framework.Context;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// The OpenAPI 2.0 Processor is a Capability Processor that takes an Interface Capability node and creates an OpenAPI 2.0 definition file, 
    /// otherwise known as 'Swagger file'. It only supports processing from the Interface downwards.
    /// </summary>
    internal partial class OpenAPI20Processor : CapabilityProcessor
    {
        // Configuration properties used by this module...
        private const string _ConsumesMIMEListTag               = "ConsumesMIMEListTag";
        private const string _ProducesMIMEListTag               = "ProducesMIMEListTag";
        private const string _RequestPaginationClassName        = "RequestPaginationClassName";
        private const string _ResponsePaginationClassName       = "ResponsePaginationClassName";
        private const string _PaginationClassStereotype         = "PaginationClassStereotype";
        private const string _OperationResultClassName          = "OperationResultClassName";
        private const string _RequestHdrParamClassName          = "RequestHdrParamClassName";
        private const string _ResponseHdrParamClassName         = "ResponseHdrParamClassName";
        private const string _ResourceClassStereotype           = "ResourceClassStereotype";
        private const string _BusinessComponentStereotype       = "BusinessComponentStereotype";
        private const string _APISupportModelPathName           = "APISupportModelPathName";
        private const string _CollectionFormatTag               = "CollectionFormatTag";
        private const string _ResponsePkgName                   = "ResponsePkgName";
        private const string _ResponseMessageSuffix             = "ResponseMessageSuffix";
        private const string _GenericMessageClassStereotype     = "GenericMessageClassStereotype";
        private const string _ProfileStereotype                 = "ProfileStereotype";
        private const string _BasicProfileName                  = "BasicProfileName";
        private const string _ProfileSetPrefix                  = "ProfileSetPrefix";
        private const string _DataModelPkgName                  = "DataModelPkgName";
        private const string _DataModelPkgStereotype            = "DataModelPkgStereotype";

        private const string _Summary                           = "summary: ";  // Separator between summary text and description text

        /// <summary>
        /// Main entry point for Operation processing. The method generates the generic Operation metadata such as name, tags and description.
        /// Next, we check whether MIME-type overrides have been defined for the operation and if so, these are written to the specification.
        /// Finally, we create the Parameter object for the operation.
        /// </summary>
        /// <param name="operation">Operation that is being processed.</param>
        /// <returns>True on successfull completion, false on errors.</returns>
        private bool BuildOperation(RESTOperationCapability operation)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildOperation >> Building operation '" + operation.Name + "'...");
            bool result = false;
            this._schema.CurrentCapability = operation;
            this._JSONWriter.WritePropertyName(operation.HTTPOperationType.TypeName.ToLower());
            this._JSONWriter.WriteStartObject();
            {
                if (this._currentResource.TagNames.Count > 0)
                {
                    this._JSONWriter.WritePropertyName("tags");
                    this._JSONWriter.WriteStartArray();
                    foreach (string tagName in this._currentResource.TagNames) this._JSONWriter.WriteValue(tagName);
                    this._JSONWriter.WriteEndArray();
                }

                // Extract documentation from the class. This can be a multi-line object in which the first line might be the 'summary' description...
                List<string> documentation = MEChangeLog.GetDocumentationAsTextLines(operation.CapabilityClass);
                if (documentation.Count > 0)
                {
                    if (documentation[0].StartsWith(_Summary, StringComparison.OrdinalIgnoreCase))
                    {
                        string summary = documentation[0].Substring(_Summary.Length);
                        this._JSONWriter.WritePropertyName("summary"); this._JSONWriter.WriteValue(summary);
                    }
                    if (documentation.Count > 1)
                    {
                        string description = string.Empty;
                        // We already copied the first line, so now append all additional lines...
                        // Since newlines don't really work in JSON descriptions, we replace them by some spaces.
                        bool firstLine = true;
                        for (int i = 1; i < documentation.Count; i++)
                        {
                            description += firstLine ? documentation[i] : "  " + documentation[i];
                            firstLine = false;
                        }
                        this._JSONWriter.WritePropertyName("description"); this._JSONWriter.WriteValue(description);
                    }
                }
                this._JSONWriter.WritePropertyName("operationId"); this._JSONWriter.WriteValue(Conversions.ToCamelCase(operation.Name));

                // Build the MIME list for this operation, if alternative MIME types have been defined...
                if (operation.ProducedMIMEList.Count > 0)
                {
                    this._JSONWriter.WritePropertyName("produces"); this._JSONWriter.WriteStartArray();
                    {
                        bool isFirst = true;
                        foreach (string MIMEstring in operation.ProducedMIMEList)
                        {
                            this._JSONWriter.WriteValue(isFirst ? MIMEstring : "," + MIMEstring);
                            isFirst = false;
                        }
                    }
                    this._JSONWriter.WriteEndArray();
                }
                if (operation.ConsumedMIMEList.Count > 0)
                {
                    this._JSONWriter.WritePropertyName("consumes"); this._JSONWriter.WriteStartArray();
                    {
                        bool isFirst = true;
                        foreach (string MIMEstring in operation.ConsumedMIMEList)
                        {
                            this._JSONWriter.WriteValue(isFirst ? MIMEstring : "," + MIMEstring);
                            isFirst = false;
                        }
                    }
                    this._JSONWriter.WriteEndArray();
                }

                this._JSONWriter.WritePropertyName("parameters"); this._JSONWriter.WriteStartArray();
                {
                    result = BuildParameters(operation);
                }
                this._JSONWriter.WriteEndArray();

                // After constructing the body of the operation, generate a list of all result codes plus associated data...
                if (result) result = BuildOperationResultList(operation);
            }
            this._JSONWriter.WriteEndObject();          // Close the operation block.
            return result;
        }

        /// <summary>
        /// Entry point for Operation Result processing. The method locates the Response Collection in the provided Operation capability and generates 
        /// the associated Response Objects. It writes all result codes and descriptions and, if specified, generates the appropriate response schemas.
        /// Note: since we 'stay' in the Operation Capability, the processing context remains the current Operation. Big advantage is that all 
        /// operation-specific data types (present in the Operation.Request/Response packages), are now automatically prefixed with the Operation 
        /// Name. This does NOT apply to the root-class, the one connected to the Document Resource.
        /// It is the responsibility of the API designer to assure that no name clashes exist (e.g. by assigning Alias names if required).
        /// </summary>
        /// <param name="operation">The Operation capability to process.</param>
        /// <returns>True when successfully completed, false on errors.</returns>
        private bool BuildOperationResultList(RESTOperationCapability operation)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildOperationResultList >> Building result for'" + operation.Name + "'...");
            ContextSlt context = ContextSlt.GetContextSlt();
            this._JSONWriter.WritePropertyName("responses"); this._JSONWriter.WriteStartObject();
            {
                foreach (RESTOperationResultDescriptor operationResult in operation.ResponseCodeCollection.Collection)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildOperationResultList >> Building result '" +
                                     operationResult.ResultCode + "'...");
                    bool result = false;
                    this._JSONWriter.WritePropertyName(operationResult.ResultCode); this._JSONWriter.WriteStartObject();
                    {
                        if (!string.IsNullOrEmpty(operationResult.Description))
                        {
                            this._JSONWriter.WritePropertyName("description");
                            this._JSONWriter.WriteValue(operationResult.Description);
                        }

                        if (operationResult.PayloadClass != null)    // Check whether we have a payload to be processed...
                        {
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildOperationResultList >> Found a payload class...");
                            result = WriteResponsePayload(operationResult.PayloadClass, operationResult.ResponseCardinality);
                        }
                        else if (operationResult.Document != null)   // Check whether we have a response Document or Profile Set to be processed...
                        {
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildOperationResultList >> Found a body parameter...");
                            result = WriteResponseBodyParameter(operationResult.Document.CapabilityClass, operationResult.ResponseCardinality);
                        }
                        else result = true;                         // No payload at all!

                        if (!result)
                        {
                            this._JSONWriter.WriteEndObject();      // Close result on errors.
                            this._JSONWriter.WriteEndObject();      // Close 'responses' section on errors.
                            return false;                           // Abort on errors!
                        }

                        // Finally, check whether we have to process header parameters and/or links...
                        if (this._currentOperation.UseHeaderParameters || this._currentOperation.UseLinkHeaders)
                            WriteResponseHeaderParameters(operationResult.Category);
                    }
                    this._JSONWriter.WriteEndObject();              // Close this result.
                }
            }
            this._JSONWriter.WriteEndObject();                      // Close the 'responses' section.
            return true;
        }

        /// <summary>
        /// Is called to construct the Parameter Object section of an Operation. If we're processing an Identifier Resource, we first of all will create the
        /// Identifier Path parameter. Next, we're scanning for Pagination (set of Query Parameters) and a Request Body.
        /// </summary>
        /// <param name="operation">The operation that is currently being processed.</param>
        /// <returns>True on successfull completion, false on errors.</returns>
        private bool BuildParameters(RESTOperationCapability operation)
        {
            bool result = true;
            this._schema.CurrentCapability = operation;
            ContextSlt context = ContextSlt.GetContextSlt();

            // The operation can be assigned directly to an Identifier Resource, or it can be further "down the line" from earlier Identifier Resources.
            // In all cases, we have collected the applicable Identifier Resources in the 'identifierList' property. We have to generate Identifier
            // definitions for ALL entries in that list...
            foreach (RESTResourceCapability identifierResource in this._identifierList)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildParameters >> Processing Identifier resource '" + identifierResource.Name + "', build path parameter...");
                RESTParameterDeclaration resourceParam = identifierResource.Parameter;
                if (resourceParam == null)
                {
                    Logger.WriteError("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildParameters >> Required ID parameter in Identifier Resource '" +
                                      identifierResource.Name + "' is missing!");
                }
                else result = WriteIdentifierParameter(identifierResource);   // Locate and write the Identifier Parameter.
                if (!result) Logger.WriteError("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildParameters >> Failed to write ID parameter in Identifier Resource '" +
                                                identifierResource.Name + "'!");
            }

            if (result)
            {
                string paginationClassName = context.GetConfigProperty(_RequestPaginationClassName);
                string resourceClassStereotype = context.GetConfigProperty(_ResourceClassStereotype);

                // Check if we have any query parameters...
                WriteQueryParameters(operation);

                // Go over each association, looking for stuff to process. At this point, these can be Pagination classes or [request] Document-/ProfileSet Resources.
                foreach (MEAssociation assoc in operation.CapabilityClass.AssociationList)
                {
                    if (assoc.Destination.EndPoint.Name == paginationClassName)
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildParameters >> Found Pagination class! Processing query parameters...");
                        result = WritePagination(assoc.Destination.EndPoint);
                    }
                    else if (assoc.Destination.EndPoint.HasStereotype(resourceClassStereotype))
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildParameters >> Found a body parameter...");
                        result = WriteRequestBodyParameter(assoc.Destination.EndPoint, assoc.GetCardinality(MEAssociation.AssociationEnd.Destination));
                    }
                }

                // Finally, check if whe have any header parameters...
                if (operation.UseHeaderParameters) WriteRequestHeaderParameters();
            }
            return result;
        }

        /// <summary>
        /// Helper function that searches the current Operation for an association with a Response Pagination class and, when found, creates
        /// a copy of this class in the current Response package of the Operation Package. We do this since we must create a 'composite response', 
        /// consisting of the pagination parameters, combined with an association with the actual response object. This combination is then
        /// referenced from the Response Parameter in the Swagger definition. The reason for this construction is that not all frameworks
        /// support definition of constructed response parameters ("response object").
        /// When a pagination response class has been created earlier, we compare versions with the template class and if we have an older version,
        /// we delete the existing class and create a new one.
        /// In all cases, we delete any associations between the response parameter class and payload. This will be re-established lateron.
        /// </summary>
        /// <returns>The pagination response class if one is needed or NULL when pagination is not defined.</returns>
        private MEClass GetResponsePaginationClass()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string paginationClassName = context.GetConfigProperty(_ResponsePaginationClassName);
            string paginationClassStereotype = context.GetConfigProperty(_PaginationClassStereotype);
            string responseSuffix = context.GetConfigProperty(_ResponseMessageSuffix);
            MEClass responseClass = null;
            MEClass templateClass = null;
            try
            {
                // First of all, check whether pagination is defined for the current operation...
                foreach (MEAssociation assoc in this._currentOperation.CapabilityClass.AssociationList)
                {
                    if (assoc.Destination.EndPoint.Name == paginationClassName)
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.GetResponsePaginationClass >> Found Pagination class, processing...");
                        templateClass = assoc.Destination.EndPoint;
                        break;
                    }
                }

                // Next, we check whether an earlier pagination class already exists. If so, check whether we're lagging in version...
                // When we have an older version, or pagination is not needed anymore, delete the existing class...
                // The newly created class receives a name that is a combination of operation + standard suffix...
                string myPaginationClassName = this._currentOperation.Name + responseSuffix;
                MEPackage paramPackage = this._currentOperation.OwningPackage.FindPackage(context.GetConfigProperty(_ResponsePkgName));
                responseClass = paramPackage.FindClass(myPaginationClassName, paginationClassStereotype);
                if (responseClass != null && (templateClass == null ||
                    responseClass.Version.Item1 < templateClass.Version.Item1 || responseClass.Version.Item2 < templateClass.Version.Item2))
                {
                    paramPackage.DeleteClass(responseClass);
                    responseClass = null;
                }

                if (responseClass != null)
                {
                    // If we did not delete the class, we WILL delete the association with the 'old' payload. This way, we can simply
                    // create a (new) association without spending time checking what has changed.
                    foreach (MEAssociation association in responseClass.AssociationList) responseClass.DeleteAssociation(association);
                }

                // Finally, IF we must support pagination, (and we have deleted the original class), we create a new class and copy all properties...
                if (templateClass != null && responseClass == null)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.GetResponsePaginationClass >> Creating copy of pagination class...");
                    responseClass = paramPackage.CreateClass(myPaginationClassName, paginationClassStereotype);
                    foreach (MEAttribute attrib in templateClass.Attributes) responseClass.CreateAttribute(attrib);
                    responseClass.Version = templateClass.Version;
                }
            }
            catch (Exception exc)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.GetResponsePaginationClass >> Caught an exception: '" + exc.ToString());
                responseClass = null;
            }
            return responseClass;
        }

        /// <summary>
        /// Searches the provided Identifier Resource capability for an Identifier Parameter and if found, writes the associated OpenAPI contents.
        /// This method should only be called in case of 'Identifier' resources and there should only be a single matching parameter.
        /// The parameter is indicated by a class attribute marked with stereotype 'RESTParameter'. If the original class contained multiple 
        /// parameters, we will use only the first one found (and since order is not necessarily guaranteed, this could result in unwanted behavior).
        /// </summary>
        /// <param name="identifierResource">The Identifier Resource to be processed.</param>
        /// <returns>True if parameter found and written, false when not found.</returns>
        private bool WriteIdentifierParameter(RESTResourceCapability identifierResource)
        {
            bool result = false;
            RESTParameterDeclaration resourceParam = identifierResource.Parameter;
            List<SchemaAttribute> resourceProperties = this._schema.ProcessProperties(identifierResource.CapabilityClass);
            foreach (JSONContentAttribute attrib in resourceProperties)
            {
                if (attrib.Name == resourceParam.Name)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteIdentifierParameter >> Found Identifier property!");
                    this._JSONWriter.WriteStartObject();
                    {
                        this._JSONWriter.WritePropertyName("name"); this._JSONWriter.WriteValue(RESTUtil.GetAssignedRoleName(attrib.Name));
                        this._JSONWriter.WritePropertyName("in"); this._JSONWriter.WriteValue("path");
                        if (!string.IsNullOrEmpty(resourceParam.Description))
                        {
                            this._JSONWriter.WritePropertyName("description"); this._JSONWriter.WriteValue(resourceParam.Description);
                        }
                        this._JSONWriter.WritePropertyName("required"); this._JSONWriter.WriteValue(true);

                        // Collect the JSON Schema for the attribute as a string...
                        string attribText = attrib.GetClassifierAsJSONSchemaText();
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteIdentifierParameter >> Got attribute: '" + attribText + "'...");
                        this._JSONWriter.WriteRaw("," + attribText);
                        if (attrib.IsListRequired)
                        {
                            if (resourceParam.CollectionFormat != RESTParameterDeclaration.QueryCollectionFormat.Unknown &&
                                resourceParam.CollectionFormat != RESTParameterDeclaration.QueryCollectionFormat.NA)
                            {
                                this._JSONWriter.WritePropertyName("collectionFormat");
                                this._JSONWriter.WriteValue(resourceParam.CollectionFormat.ToString().ToLower());
                            }
                            else Logger.WriteWarning("Collection specification is missing in resource '" + this._currentResource.Name + "'!");
                        }
                    }
                    this._JSONWriter.WriteEndObject();
                    result = true;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Writes a series of additional profile selection query parameters for each response profile set used in the current operation.
        /// The profiles are passed as a 'list of lists', in which each entry contains the name of the profile set, followed by the names of all
        /// profiles that are part of that set. The name of the profile is used as the name of the filter parameter.
        /// </summary>
        /// <param name="profileList">Profile descriptors to be converted to parameters.</param>
        private void WriteProfileListQueryParameters(List<Tuple<string, List<string>>> profileList)
        {
            foreach (Tuple<string, List<string>> entry in profileList)
            {
                this._JSONWriter.WriteStartObject();
                {
                    this._JSONWriter.WritePropertyName("name"); this._JSONWriter.WriteValue(RESTUtil.GetAssignedRoleName(entry.Item1));
                    this._JSONWriter.WritePropertyName("in"); this._JSONWriter.WriteValue("query");
                    this._JSONWriter.WritePropertyName("required"); this._JSONWriter.WriteValue(true);
                    this._JSONWriter.WritePropertyName("allowEmptyValue"); this._JSONWriter.WriteValue(false);

                    string enumString = "\"type\": \"string\", \"enum\": [";
                    bool firstOne = true;
                    foreach (string value in entry.Item2)
                    {
                        if (!firstOne) enumString += ",";
                        enumString += "\r\n\"" + value + "\"";
                        firstOne = false;
                    }
                    enumString += "\r\n]";
                    this._JSONWriter.WriteRaw("," + enumString);
                }
                this._JSONWriter.WriteEndObject();
            }
        }

        /// <summary>
        /// Helper function that creates an intermediate (placeholder) class containing all components of a profile request/response.
        /// </summary>
        /// <param name="profileSetResource">The ProfileSet resource class.</param>
        /// <param name="schemaClasses">Array of profile payload classes.</param>
        /// <returns>The qualified name- and instance of the ProfileSet class or NULL on errors.</returns>
        private Tuple<string, MEClass> WriteProfileSet(MEClass profileSetResource, List<Tuple<string, MEClass>> schemaClasses)
        {
            ContextSlt context = ContextSlt.GetContextSlt();

            // We have to create a class as the 'placeholder' for the profile set within the definitions section. We create this class
            // in the DataModel package and assume that each profile has a unique name. We add a prefix to the name in order to make 
            // name clashes even less likely...
            string profileSetName = context.GetConfigProperty(_ProfileSetPrefix) + profileSetResource.Name;
            MEPackage dataModelPkg = this._currentService.DeclarationPkg.FindPackage(context.GetConfigProperty(_DataModelPkgName),
                                                                                     context.GetConfigProperty(_DataModelPkgStereotype));
            string BCStereotype = context.GetConfigProperty(_BusinessComponentStereotype);
            MEClass profileSetClass = dataModelPkg.FindClass(profileSetName, BCStereotype);
            if (profileSetClass != null)
            {
                // Profile already exists, check whether we still have all the right associations...
                // To avoid unnecessary iterations over the repository, we first collect all existing associations of the correct type...
                var assocList = new List<MEAssociation>();
                foreach (MEAssociation assoc in profileSetClass.TypedAssociations(MEAssociation.AssociationType.Association)) assocList.Add(assoc);

                // First, check whether all requested classes exist in the current list...
                var newEntries = new List<Tuple<string, MEClass>>();
                foreach (Tuple<string, MEClass> entry in schemaClasses)
                {
                    bool foundIt = false;
                    foreach (MEAssociation assoc in assocList)
                    {
                        if (assoc.Destination.EndPoint.ElementID == entry.Item2.ElementID)
                        {
                            foundIt = true;
                            break;
                        }
                    }
                    if (!foundIt) newEntries.Add(entry);    // Remember which ones we did NOT find so we can add them later.
                }

                // Next, check whether entries have gone. We can delete these right away...
                foreach (MEAssociation assoc in assocList)
                {
                    bool foundIt = false;
                    foreach (Tuple<string, MEClass> entry in schemaClasses)
                    {
                        if (entry.Item2.ElementID == assoc.Destination.EndPoint.ElementID)
                        {
                            foundIt = true;
                            break;
                        }
                    }
                    if (!foundIt) profileSetClass.DeleteAssociation(assoc);
                }
                schemaClasses = newEntries;                 // Reduce the set of new entries to only those we don't yet know about...
            }
            else
            {
                profileSetClass = dataModelPkg.CreateClass(profileSetName, BCStereotype);
                if (profileSetClass == null)
                {
                    Logger.WriteError("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteProfileSet >> Error creating Profile Set '" +
                                      dataModelPkg.Name + "/" + profileSetName + "'!");
                    return null;
                }
            }

            // Update documentation for the class...
            string annotation = MEChangeLog.GetDocumentationAsText(profileSetResource);
            profileSetClass.Annotation = annotation;

            // Next, update the list of payload associations with this Profile Set.
            var source = new EndpointDescriptor(profileSetClass, "1", profileSetName, null, false);
            foreach (Tuple<string, MEClass> entry in schemaClasses)
            {
                string roleName = entry.Item1.EndsWith("Type") ? entry.Item1.Substring(0, entry.Item1.IndexOf("Type")) : entry.Item1;
                if (roleName.Contains(":")) roleName = roleName.Substring(roleName.IndexOf(':') + 1);  // Remove the namespace token!
                var target = new EndpointDescriptor(entry.Item2, "0..1", roleName, null, true);
                profileSetClass.CreateAssociation(source, target, MEAssociation.AssociationType.Association);
            }

            // Now that the class is up-to-date, we can parse the resulting profile set. Note that all target classes have been parsed
            // earlier and thus we only add the actual profile class at this point.
            // After processing, we finally add the result to the JSON output...
            string qualifiedSetName = this._schema.ProcessClass(profileSetClass, context.GetConfigProperty(_SchemaTokenName) + ":" + profileSetName);

            // Parsing defines the final result of this function...
            if (qualifiedSetName != null) return new Tuple<string, MEClass>(profileSetName, profileSetClass);
            else
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteProfileSet >> Error parsing Profile Set '" +
                                   dataModelPkg.Name + "/" + profileSetName + "'!");
                return null;
            }
        }

        /// <summary>
        /// This method iterates over all attributes of the Operation class, looking for those that have 'Query' scope. When found, a Parameter Object
        /// is created for those attributes in the OpenAPI definition. By selectively processing the attributes, we can have a mix of differently scoped
        /// attributes and still process them in the correct order.
        /// </summary>
        /// <param name="operation">Operation capability that we're currently processing.</param>
        private void WriteQueryParameters(RESTOperationCapability operation)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteQueryParameters >> Looking for query parameters in '" + operation.CapabilityClass.Name + "'...");

            // We create two lists, one Parameter Declaration list that contains attribute metadata and one that containes JSON Scheme definitions.
            // The metadata list is used to determine which JSON attribute definition to insert in the OpenAPI definition...
            var paramList = new SortedList<string, RESTParameterDeclaration>();
            foreach (MEAttribute attrib in operation.CapabilityClass.Attributes) paramList.Add(attrib.Name, new RESTParameterDeclaration(attrib));
            List<SchemaAttribute> queryProperties = this._schema.ProcessProperties(operation.CapabilityClass);

            // First of all, write all 'regular' query parameters (if any)...
            foreach (JSONContentAttribute attrib in queryProperties)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteQueryParameters >> Processing '" + attrib.Name + "'...");
                if (paramList[attrib.Name].Scope == RESTParameterDeclaration.ParameterScope.Query)
                {
                    this._JSONWriter.WriteStartObject();
                    {
                        this._JSONWriter.WritePropertyName("name"); this._JSONWriter.WriteValue(RESTUtil.GetAssignedRoleName(attrib.Name));
                        this._JSONWriter.WritePropertyName("in"); this._JSONWriter.WriteValue("query");
                        if (!string.IsNullOrEmpty(attrib.Annotation))
                        {
                            this._JSONWriter.WritePropertyName("description"); this._JSONWriter.WriteValue(attrib.Annotation);
                        }
                        this._JSONWriter.WritePropertyName("required"); this._JSONWriter.WriteValue(attrib.IsMandatory);
                        this._JSONWriter.WritePropertyName("allowEmptyValue"); this._JSONWriter.WriteValue(paramList[attrib.Name].AllowEmptyValue);

                        // Collect the JSON Schema for the attribute as a string. Note that this includes possible default values, min- and max
                        // values, etc.
                        string attribText = attrib.GetClassifierAsJSONSchemaText();
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteQueryParameters >> Got attribute: '" + attribText + "'...");
                        this._JSONWriter.WriteRaw("," + attribText);
                        if (attrib.IsListRequired)
                        {
                            RESTParameterDeclaration.QueryCollectionFormat collectionFormat = paramList[attrib.Name].CollectionFormat;
                            if (collectionFormat != RESTParameterDeclaration.QueryCollectionFormat.Unknown &&
                                collectionFormat != RESTParameterDeclaration.QueryCollectionFormat.NA)
                            {
                                this._JSONWriter.WritePropertyName("collectionFormat");
                                this._JSONWriter.WriteValue(collectionFormat.ToString().ToLower());
                            }
                            else Logger.WriteWarning("Collection specification is missing in attribute '" + attrib.Name + "'!");
                        }
                    }
                    this._JSONWriter.WriteEndObject();
                }
            }

            // Next, we check whether we're using one or more response profile sets, in which case we must generate some additional query parameters...
            var profileList = new List<Tuple<string, List<string>>>();
            foreach (RESTOperationResultDescriptor resultDsc in operation.ResponseCodeCollection.Collection)
            {
                if (resultDsc.Document != null && resultDsc.Document.Archetype == RESTResourceCapability.ResourceArchetype.ProfileSet)
                {
                    var profileNames = new List<string>();
                    foreach (MEAssociation assoc in resultDsc.Document.CapabilityClass.AssociationList)
                    {
                        if (!string.IsNullOrEmpty(assoc.Name)) profileNames.Add(assoc.Name);
                    }
                    profileList.Add(new Tuple<string, List<string>>(resultDsc.Document.Name, profileNames));
                }
            }
            if (profileList.Count > 0) WriteProfileListQueryParameters(profileList);
        }

        /// <summary>
        /// Helper function that creates a JSON Request Body parameter block in the output. The associated classes must have been parsed into the
        /// schema previously so they can be properly referenced.
        /// </summary>
        /// <param name="qualifiedClassName">Qualified class name to be used in output.</param>
        /// <param name="payload">The actual payload document.</param>
        /// <param name="cardinality">Payload cardinality.</param>
        private void WriteRequestBody(string qualifiedClassName, MEClass payload, Cardinality cardinality)
        {
            this._JSONWriter.WriteStartObject();
            {
                qualifiedClassName = qualifiedClassName.Substring(qualifiedClassName.IndexOf(':') + 1); // Remove namespace token part (not used by REST).
                string propertyName = qualifiedClassName.EndsWith("Type") ? qualifiedClassName.Substring(0, qualifiedClassName.IndexOf("Type")) : qualifiedClassName;
                propertyName = RESTUtil.GetAssignedRoleName(propertyName);
                this._JSONWriter.WritePropertyName("name"); this._JSONWriter.WriteValue(propertyName);
                this._JSONWriter.WritePropertyName("in"); this._JSONWriter.WriteValue("body");
                if (!string.IsNullOrEmpty(payload.Annotation))
                {
                    this._JSONWriter.WritePropertyName("description"); this._JSONWriter.WriteValue(payload.Annotation);
                }
                this._JSONWriter.WritePropertyName("required"); this._JSONWriter.WriteValue(true);

                // Since we 'might' use alias names in classes, the returned name 'might' be different from the offered name. To make sure we're referring
                // to the correct name, we take the returned FQN and remove the token part. Remainder is the 'formal' type name.
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteRequestBody >> Gotten FQN '" + qualifiedClassName + "'.");
                this._JSONWriter.WritePropertyName("schema"); this._JSONWriter.WriteStartObject();
                {
                    if (cardinality.IsList)
                    {
                        this._JSONWriter.WritePropertyName("type"); this._JSONWriter.WriteValue("array");
                        this._JSONWriter.WritePropertyName("items"); this._JSONWriter.WriteStartObject();
                        {
                            this._JSONWriter.WriteRaw("\"$ref\": \"#/definitions/" + qualifiedClassName + "\"");
                        }
                        this._JSONWriter.WriteEndObject();
                        if (cardinality.IsMandatory)
                        {
                            this._JSONWriter.WritePropertyName("minItems"); this._JSONWriter.WriteValue(cardinality.LowerBoundary);
                        }
                        if (cardinality.IsBoundedList)
                        {
                            this._JSONWriter.WritePropertyName("maxItems"); this._JSONWriter.WriteValue(cardinality.UpperBoundary);
                        }
                    }
                    else this._JSONWriter.WriteRaw("\"$ref\": \"#/definitions/" + qualifiedClassName + "\"");
                }
                this._JSONWriter.WriteEndObject();
            }
            this._JSONWriter.WriteEndObject();
        }

        /// <summary>
        /// Writes a request body parameter using the specified Document- or Profile Set resource, which in turn has a reference to one or more
        /// Business Components that acts as the schema root. In case of a Document resource, each operation may have at most ONE body parameter 
        /// and each must refer to an, operation specific, unique message model. The root of this model is a Business Component. In case of
        /// Profile Sets, the operation has a number of body parameters, each being associated with a different message model. In the latter
        /// case, the name of the profile (= message model ID) is used as a prefix for all business components in that model in order to avoid
        /// name space clashes (but only for those components of the message model that actually 'live' inside the profile package.
        /// Only components of the Basic profile do not receive a prefix.
        /// The operation initiates schema generation for the associated message model and writes references to that model. If the cardinality
        /// of the Document Resource association is > 1, we generate an array of references instead of a single reference. In case of Profile
        /// Sets, we introduce an 'intermediate' object that contains reference properties to each of the models. The profile name is used as the
        /// property name. When a Profile Set contains only a single entry, it is treated exactly as a Document!
        /// </summary>
        /// <param name="documentResourceClass">Either the Document- or Profile Set resource that contains the association(s) with our schema root.</param>
        /// <param name="cardinality">The cardinality of the request object association. If upper limit > 1, we must create an array.</param>
        /// <returns>True when processed ok, false on errors.</returns>
        private bool WriteRequestBodyParameter(MEClass documentResourceClass, Cardinality cardinality)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteRequestBodyParameter >> Processing body class '" + documentResourceClass.Name + "'...");
            this._panel.WriteInfo(this._panelIndex + 3, "Processing Document/ProfileSet resource '" + documentResourceClass.Name + "'...");
            bool result = true;
            ContextSlt context = ContextSlt.GetContextSlt();
            string qualifiedClassName = string.Empty;
            string businessComponentStereotype = context.GetConfigProperty(_BusinessComponentStereotype);
            string messageComponentStereotype = context.GetConfigProperty(_GenericMessageClassStereotype);
            string profileStereotype = context.GetConfigProperty(_ProfileStereotype);
            string schemaTokenName = context.GetConfigProperty(_SchemaTokenName);
            List<Tuple<string, MEClass>> schemaClasses = new List<Tuple<string, MEClass>>();

            foreach (MEAssociation association in documentResourceClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
            {
                // Here, we look for ALL target payload classes. In case of a Document, there should be only one but in case of a Profile, there
                // can be any number of payload references. Each will subsequently be parsed into the schema and stored for final processing...
                if (association.Destination.EndPoint.HasStereotype(businessComponentStereotype) ||
                    association.Destination.EndPoint.HasStereotype(messageComponentStereotype))
                {
                    // Note that we might have used an Alias name for our Business Component (in order to create unique / meaningfull names)
                    // and thus the Document Resource class is named after the Alias. We MUST use that name for all references to the Business
                    // Component and thus we use the Document Resource class name to be sure we have the correct name...
                    MEClass target = association.Destination.EndPoint;
                    string profile = string.Empty;
                    if (target.OwningPackage.HasStereotype(context.GetConfigProperty(_ProfileStereotype)) &&
                        target.OwningPackage.Name != context.GetConfigProperty(_BasicProfileName)) profile = target.OwningPackage.Name;
                    string token = schemaTokenName + ":" + profile + target.Name;
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteRequestBodyParameter >> Found Business Component '" + token + "', processing...");
                    qualifiedClassName = this._schema.ProcessClass(target, token);
                    if (!string.IsNullOrEmpty(qualifiedClassName)) schemaClasses.Add(new Tuple<string, MEClass>(qualifiedClassName, target));
                    else
                    {
                        Logger.WriteError("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteRequestBodyParameter >> Error processing class '" + token + "', aborting!");
                        return false;
                    }
                }
            }

            // Final processing. This will create the JSON parameter reference in case of Documents. In case of Profile Sets, we must first
            // create a 'placeholder' class that will have all the actual profile references...
            if (schemaClasses.Count == 1) WriteRequestBody(schemaClasses[0].Item1, schemaClasses[0].Item2, cardinality);
            else if (schemaClasses.Count > 1)
            {
                Tuple<string, MEClass> profileSet = WriteProfileSet(documentResourceClass, schemaClasses);
                WriteRequestBody(profileSet.Item1, profileSet.Item2, cardinality);
            }
            return result;
        }

        /// <summary>
        /// This method iterates over all attributes of the (temporary) Header Parameters class. For each attribute, a Request Header 
        /// Parameter Object is created in the OpenAPI definition. If the class is not defined (or has no attributes), no actions are performed.
        /// </summary>
        private void WriteRequestHeaderParameters()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteRequestHeaderParameters >> Looking for header parameters...");
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            MEClass parameterClass = model.FindClass(context.GetConfigProperty(_APISupportModelPathName),
                                                     context.GetConfigProperty(_RequestHdrParamClassName));
            if (parameterClass == null) return;     // Nothing to do.

            // Create a separate list of all collection formats for each attribute. We need this when we have an attribute with cardinality > 1...
            var sortedCollectionFmtList = new SortedList<string, RESTParameterDeclaration.QueryCollectionFormat>();
            string collectionFmtTagName = context.GetConfigProperty(_CollectionFormatTag);
            foreach (MEAttribute attrib in parameterClass.Attributes)
            {
                string collectionFmtTag = attrib.GetTag(collectionFmtTagName);
                if (!string.IsNullOrEmpty(collectionFmtTag))
                    sortedCollectionFmtList.Add(attrib.Name, EnumConversions<RESTParameterDeclaration.QueryCollectionFormat>.StringToEnum(collectionFmtTag));
                else sortedCollectionFmtList.Add(attrib.Name, RESTParameterDeclaration.QueryCollectionFormat.NA);
            }

            List<SchemaAttribute> headerProperties = this._schema.ProcessProperties(parameterClass);
            foreach (JSONContentAttribute attrib in headerProperties)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteRequestHeaderParameters >> Processing '" + attrib.Name + "'...");
                this._JSONWriter.WriteStartObject();
                {
                    this._JSONWriter.WritePropertyName("name"); this._JSONWriter.WriteValue(attrib.Name);
                    this._JSONWriter.WritePropertyName("in"); this._JSONWriter.WriteValue("header");
                    if (!string.IsNullOrEmpty(attrib.Annotation))
                    {
                        this._JSONWriter.WritePropertyName("description"); this._JSONWriter.WriteValue(attrib.Annotation);
                    }
                    this._JSONWriter.WritePropertyName("required"); this._JSONWriter.WriteValue(attrib.IsMandatory);

                    // Collect the JSON Schema for the attribute as a string. Note that this includes possible default values, min- and max
                    // values, etc....
                    string attribText = attrib.GetClassifierAsJSONSchemaText();
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteRequestHeaderParameters >> Got attribute: '" + attribText + "'...");
                    this._JSONWriter.WriteRaw("," + attribText);
                    if (attrib.IsListRequired)
                    {

                        RESTParameterDeclaration.QueryCollectionFormat collectionFormat = sortedCollectionFmtList[attrib.Name];
                        if (collectionFormat != RESTParameterDeclaration.QueryCollectionFormat.Unknown &&
                            collectionFormat != RESTParameterDeclaration.QueryCollectionFormat.NA)
                        {
                            this._JSONWriter.WritePropertyName("collectionFormat");
                            this._JSONWriter.WriteValue(collectionFormat.ToString().ToLower());
                        }
                        else Logger.WriteWarning("Collection specification is missing in attribute '" + attrib.Name + "'!");
                    }
                }
                this._JSONWriter.WriteEndObject();
            }
        }
        
        /// <summary>
        /// This method receives a reference to a response payload class, parses this class into a JSON structure and inserts the proper reference in
        /// the output schema.
        /// </summary>
        /// <param name="payloadClass">The class associated with the payload.</param>
        /// <param name="cardinality">Cardinality of the payload.</param>
        /// <returns>True on successfull completion, false on errors.</returns>
        private bool WriteResponsePayload(MEClass payloadClass, Cardinality cardinality)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteResponsePayload >> Processing response class '" + payloadClass.Name + "'...");
            this._panel.WriteInfo(this._panelIndex + 3, "Processing Response Payload '" + payloadClass.Name + "'...");
            bool result = false;

            string payloadQName = this._schema.ProcessClass(payloadClass, ContextSlt.GetContextSlt().GetConfigProperty(_SchemaTokenName) + ":" + payloadClass.Name);
            if (payloadQName != string.Empty)
            {
                // Since we 'might' use alias names in classes, the returned name 'might' be different from the offered name. To make sure we're referring
                // to the correct name, we take the returned FQN and remove the token part. Remainder is the 'formal' type name.
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteResponsePayload >> Gotten FQN '" + payloadQName + "'.");
                this._JSONWriter.WritePropertyName("schema"); this._JSONWriter.WriteStartObject();
                {
                    WriteResponseBodyReference(payloadQName.Substring(payloadQName.IndexOf(':') + 1), cardinality);
                } this._JSONWriter.WriteEndObject();
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Writes a response body parameter using the specified message Profile root. Each operation may have zero to many body parameters and each
        /// must refer to an, operation specific, unique message model. The root of this model is specified by a Document Resource class that in turn
        /// contains an association with the Business Component that acts as the actual message root. The method always creates a reference to this
        /// Business Component class. If the association role has a cardinality > 1, we create an array of references.
        /// </summary>
        /// <param name="documentResourceClass">The Resource class describing the payload (either a Document or a ProfileSet).</param>
        /// <param name="cardinality">The cardinality of the Document Resource association. If upper limit > 1, we must create an array.</param>
        /// <returns>True when processed ok, false on errors.</returns>
        private bool WriteResponseBodyParameter(MEClass documentResourceClass, Cardinality cardinality)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteResponseBodyParameter >> Processing document class '" + documentResourceClass.Name + "'...");
            this._panel.WriteInfo(this._panelIndex + 3, "Processing Response Message Body '" + documentResourceClass.Name + "'...");
            ContextSlt context = ContextSlt.GetContextSlt();
            bool result = false;
            string qualifiedClassName = string.Empty;
            string businessComponentStereotype = context.GetConfigProperty(_BusinessComponentStereotype);
            string messageComponentStereotype = context.GetConfigProperty(_GenericMessageClassStereotype);
            string profileStereotype = context.GetConfigProperty(_ProfileStereotype);
            string schemaTokenName = context.GetConfigProperty(_SchemaTokenName);

            // First of all, we check whether we must use response pagination parameters. If so, we create a class structure in the Response package
            // of the current operation and use this to store both the pagination parameters and the actual response object. We then simply refer
            // to this class in the OpenAPI definition response parameter. Reason: not all frameworks support a 'response object' in the response
            // parameter...
            MEClass paginationClass = GetResponsePaginationClass();

            // Next, Locate the associated business component(s). We can't check by name since it might have used an Alias name...
            // We should accept BOTH BusinessComponents (from domain profiles) OR Message Business Information Entities (message 'specials').
            List<Tuple<string, MEClass>> schemaClasses = new List<Tuple<string, MEClass>>();
            foreach (MEAssociation association in documentResourceClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
            {
                // Here, we look for ALL target payload classes. In case of a Document, there should be only one but in case of a Profile, there
                // can be any number of payload references. Each will subsequently be parsed into the schema and stored for final processing...
                if (association.Destination.EndPoint.HasStereotype(businessComponentStereotype) ||
                    association.Destination.EndPoint.HasStereotype(messageComponentStereotype))
                {
                    // Note that we might have used an Alias name for our Business Component (in order to create unique / meaningfull names)
                    // and thus the Document Resource class is named after the Alias. We MUST use that name for all references to the Business
                    // Component and thus we use the Document Resource class name to be sure we have the correct name...
                    MEClass target = association.Destination.EndPoint;
                    string profile = string.Empty;
                    if (target.OwningPackage.HasStereotype(context.GetConfigProperty(_ProfileStereotype)) &&
                        target.OwningPackage.Name != context.GetConfigProperty(_BasicProfileName)) profile = target.OwningPackage.Name;
                    string token = schemaTokenName + ":" + profile + target.Name;
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteResponseBodyParameter >> Found Business Component '" + token + "', processing...");
                    qualifiedClassName = this._schema.ProcessClass(target, token);
                    if (!string.IsNullOrEmpty(qualifiedClassName)) schemaClasses.Add(new Tuple<string, MEClass>(qualifiedClassName, target));
                    else
                    {
                        Logger.WriteError("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteResponseBodyParameter >> Error processing class '" + token + "', aborting!");
                        return false;
                    }
                }
            }

            // If we have multiple payloads, assume we're dealing with a profile. In this case, we explicitly create an intermediate profile
            // class to hold all the profile options and use this as the final payload. In case we have only a single payload, treat this as
            // a simple business document reference.
            // If we have only a single entry (=Document), qualifiedClassName still contains the name of the processed class.
            MEClass payload = null;
            if (schemaClasses.Count == 1) payload = schemaClasses[0].Item2;
            else if (schemaClasses.Count > 1)
            {
                // In case of a Profile Set, we create a new class (or link to an existing one), which contains the actual profile associations.
                // We use this class as the payload of the response...
                Tuple<string, MEClass> profileSet = WriteProfileSet(documentResourceClass, schemaClasses);
                payload = profileSet.Item2;
                qualifiedClassName = profileSet.Item1;
            }
            else
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteResponseBodyParameter >> No valid payload detected for Document / ProfileSet '" + 
                                  documentResourceClass.Name + "', aborting!");
                return false;
            }

            // Next, check whether we must use pagination at all. If not, we can create a simple reference....
            if (paginationClass != null)
            {
                // Pagination is required. The call to GetResponsePaginationClass has either cleaned-up the existing pagination class, or has
                // created a new one. We must establish an association with the payload...
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteResponseBodyParameter >> Pagination, created class and association...");
                string assocCard = cardinality.LowerBoundary.ToString();
                if (cardinality.UpperBoundary == 0) assocCard += "..*";
                else if (cardinality.UpperBoundary != cardinality.LowerBoundary) assocCard += ".." + cardinality.UpperBoundary.ToString();
                string roleName = !string.IsNullOrEmpty(payload.AliasName) ? payload.AliasName : payload.Name;
                if ((cardinality.UpperBoundary == 0 || cardinality.UpperBoundary > 1) && !roleName.EndsWith("List")) roleName += "List";
                var source = new EndpointDescriptor(paginationClass, "1", paginationClass.Name, null, false);
                var target = new EndpointDescriptor(payload, assocCard, roleName, null, true);
                paginationClass.CreateAssociation(source, target, MEAssociation.AssociationType.MessageAssociation);
                qualifiedClassName = this._schema.ProcessClass(paginationClass, context.GetConfigProperty(_SchemaTokenName) + ":" + paginationClass.Name);
                cardinality = new Cardinality(Cardinality._Mandatory);    // Response is mandatory object and actual cardinality with payload has been solved otherwise.
            }

            if (qualifiedClassName != string.Empty)
            {
                // Since we 'might' use alias names in classes, the returned name 'might' be different from the offered name. To make sure we're referring
                // to the correct name, we take the returned FQN and remove the token part. Remainder is the 'formal' type name.
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteResponseBodyParameter >> Gotten FQN '" + qualifiedClassName + "'.");
                string className = qualifiedClassName.Substring(qualifiedClassName.IndexOf(':') + 1);
                this._JSONWriter.WritePropertyName("schema"); this._JSONWriter.WriteStartObject();
                {
                    WriteResponseBodyReference(className, cardinality);
                }
                this._JSONWriter.WriteEndObject();
                result = true;
            }
            return result;
        }

        /// <summary>
        /// This method iterates over all attributes of the (temporary) Header Parameters class. For each attribute, a Response Header 
        /// Parameter Object is created in the OpenAPI definition. If the class is not defined (or has no attributes), no actions are performed.
        /// </summary>
        private void WriteResponseHeaderParameters(RESTOperationResultDescriptor.ResponseCategory category)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteResponseHeaderParameters >> Looking for header parameters...");
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            MEClass parameterClass = model.FindClass(context.GetConfigProperty(_APISupportModelPathName),
                                                     context.GetConfigProperty(_ResponseHdrParamClassName));
            if (parameterClass == null && !this._currentOperation.UseLinkHeaders) return;     // Nothing to do.

            var headerProperties = new List<SchemaAttribute>();
            var sortedParamList = new SortedList<string, RESTParameterDeclaration>();
            if (parameterClass != null) headerProperties = this._schema.ProcessProperties(parameterClass);

            if (this._currentOperation.UseLinkHeaders || headerProperties.Count > 0)
            {
                this._JSONWriter.WritePropertyName("headers"); this._JSONWriter.WriteStartObject();
                {
                    foreach (JSONContentAttribute attrib in headerProperties)
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteResponseHeaderParameters >> Processing '" + attrib.Name + "'...");
                        this._JSONWriter.WritePropertyName(attrib.Name); this._JSONWriter.WriteStartObject();
                        {
                            if (!string.IsNullOrEmpty(attrib.Annotation))
                            {
                                this._JSONWriter.WritePropertyName("description"); this._JSONWriter.WriteValue(attrib.Annotation);
                                this._JSONWriter.WriteRaw(",");
                            }

                            // Collect the JSON Schema for the attribute as a string. Note that this includes possible default values, min- and max
                            // values, etc....
                            string attribText = attrib.GetClassifierAsJSONSchemaText();
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteResponseHeaderParameters >> Got attribute: '" + attribText + "'...");
                            this._JSONWriter.WriteRaw(attribText);
                            if (attrib.IsListRequired)
                            {
                                RESTParameterDeclaration.QueryCollectionFormat collectionFormat = sortedParamList[attrib.Name].CollectionFormat;
                                if (collectionFormat != RESTParameterDeclaration.QueryCollectionFormat.Unknown &&
                                    collectionFormat != RESTParameterDeclaration.QueryCollectionFormat.NA)
                                {
                                    this._JSONWriter.WritePropertyName("collectionFormat");
                                    this._JSONWriter.WriteValue(collectionFormat.ToString().ToLower());
                                }
                                else Logger.WriteWarning("Collection specification is missing in attribute '" + attrib.Name + "'!");
                            }
                        }
                        this._JSONWriter.WriteEndObject();
                    }

                    // When instructed to do so, we create a definition for the 'Link' header parameter as a comma-separated array of strings...
                    // But we ONLY do this for 'success' responses!
                    if (this._currentOperation.UseLinkHeaders && category == RESTOperationResultDescriptor.ResponseCategory.Success)
                    {
                        this._JSONWriter.WritePropertyName("Link"); this._JSONWriter.WriteStartObject();
                        {
                            this._JSONWriter.WritePropertyName("description");
                            this._JSONWriter.WriteValue("The Link header field provides a means for describing a relationship between two " +
                                                        "resources, generally between the requested resource and some other resource. An entity MAY " +
                                                        "include multiple Link values. Links at the metainformation level typically indicate " +
                                                        "relationships like hierarchical structure and navigation paths. The Link field is " +
                                                        "semantically equivalent to the <LINK> element in HTML.");
                            this._JSONWriter.WritePropertyName("type"); this._JSONWriter.WriteValue("array");
                            this._JSONWriter.WritePropertyName("items"); this._JSONWriter.WriteStartObject();
                            {
                                this._JSONWriter.WritePropertyName("type"); this._JSONWriter.WriteValue("string");
                            }
                            this._JSONWriter.WriteEndObject();
                            this._JSONWriter.WritePropertyName("collectionFormat"); this._JSONWriter.WriteValue("csv");
                        }
                        this._JSONWriter.WriteEndObject();
                    }
                }
                this._JSONWriter.WriteEndObject();
            }
        }

        /// <summary>
        /// Is called when we detect an association with a pagination class. The method processes all attributes in that class and writes them as a series of 
        /// query parameters to the current OpenAPI specification.
        /// </summary>
        /// <param name="paginationClass">Pagination class found for our operation.</param>
        /// <returns>True on success, false on errors.</returns>
        private bool WritePagination(MEClass paginationClass)
        {
            bool result = false;
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WritePagination >> Writing pagination parameters from class '" + paginationClass.Name + "'...");
            var paramList = new SortedList<string, RESTParameterDeclaration>();
            foreach (MEAttribute attrib in paginationClass.Attributes) paramList.Add(attrib.Name, new RESTParameterDeclaration(attrib));

            List<SchemaAttribute> paginationProperties = this._schema.ProcessProperties(paginationClass);
            foreach (JSONContentAttribute attrib in paginationProperties)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WritePagination >> Processing '" + attrib.Name + "'...");
                this._JSONWriter.WriteStartObject();
                {
                    this._JSONWriter.WritePropertyName("name"); this._JSONWriter.WriteValue(RESTUtil.GetAssignedRoleName(attrib.Name));
                    this._JSONWriter.WritePropertyName("in"); this._JSONWriter.WriteValue("query");
                    if (!string.IsNullOrEmpty(attrib.Annotation))
                    {
                        this._JSONWriter.WritePropertyName("description"); this._JSONWriter.WriteValue(attrib.Annotation);
                    }
                    this._JSONWriter.WritePropertyName("required"); this._JSONWriter.WriteValue(attrib.IsMandatory ? true : false);

                    // Collect the JSON Schema for the attribute as a string...
                    string attribText = attrib.GetClassifierAsJSONSchemaText();
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WritePagination >> Got attribute: '" + attribText + "'...");
                    this._JSONWriter.WriteRaw("," + attribText);
                    if (attrib.IsListRequired)
                    {
                        RESTParameterDeclaration.QueryCollectionFormat collectionFormat = paramList[attrib.Name].CollectionFormat;
                        if (collectionFormat != RESTParameterDeclaration.QueryCollectionFormat.Unknown &&
                            collectionFormat != RESTParameterDeclaration.QueryCollectionFormat.NA)
                        {
                            this._JSONWriter.WritePropertyName("collectionFormat");
                            this._JSONWriter.WriteValue(collectionFormat.ToString().ToLower());
                        }
                        else Logger.WriteWarning("Collection specification is missing in attribute '" + attrib.Name + "'!");
                    }
                }
                this._JSONWriter.WriteEndObject();
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Helper method that writes a reference to the Response Body, either as a single reference or, in case of cardinality > 1, as an
        /// array of references.
        /// </summary>
        /// <param name="className">Name of the body class that must be referenced.</param>
        /// <param name="cardinality">Cardinality of the association.</param>
        private void WriteResponseBodyReference(string className, Cardinality cardinality)
        {
            if (cardinality.IsList)
            {
                this._JSONWriter.WritePropertyName("type"); this._JSONWriter.WriteValue("array");
                this._JSONWriter.WritePropertyName("items"); this._JSONWriter.WriteStartObject();
                {
                    this._JSONWriter.WriteRaw("\"$ref\": \"#/definitions/" + className + "\"");
                } this._JSONWriter.WriteEndObject();
                if (cardinality.IsMandatory)
                {
                    this._JSONWriter.WritePropertyName("minItems"); this._JSONWriter.WriteValue(cardinality.LowerBoundary);
                }
                if (cardinality.IsBoundedList)
                {
                    this._JSONWriter.WritePropertyName("maxItems"); this._JSONWriter.WriteValue(cardinality.UpperBoundary);
                }
            }
            else this._JSONWriter.WriteRaw("\"$ref\": \"#/definitions/" + className + "\"");
        }
    }
}
