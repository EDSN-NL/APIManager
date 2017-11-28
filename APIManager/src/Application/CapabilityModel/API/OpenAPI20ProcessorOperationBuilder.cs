using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using Framework.Logging;
using Framework.Model;
using Framework.Util;
using Framework.Util.SchemaManagement;
using Framework.Util.SchemaManagement.JSON;
using Framework.Context;
using Plugin.Application.CapabilityModel.ASCIIDoc;
using Plugin.Application.CapabilityModel.SchemaGeneration;

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
        private const string _PaginationClassName               = "PaginationClassName";
        private const string _MessageAssemblyClassStereotype    = "MessageAssemblyClassStereotype";

        // Separator between summary text and description text
        private const string _Summary = "summary: ";

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
            bool result = true;
            this._JSONWriter.WritePropertyName(operation.HTTPTypeName);
            this._JSONWriter.WriteStartObject();                            // We start the operation object here, but we can't finish it since the responses have to go in!
            if (this._currentResource.IsTag)
            {
                this._JSONWriter.WritePropertyName("tags");
                this._JSONWriter.WriteStartArray();
                this._JSONWriter.WriteValue(RESTUtil.GetAssignedRoleName(this._currentResource.Name));
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
                } this._JSONWriter.WriteEndArray();
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
                } this._JSONWriter.WriteEndArray();
            }

            this._JSONWriter.WritePropertyName("parameters"); this._JSONWriter.WriteStartArray();
            {
                result = BuildParameters(operation);
            } this._JSONWriter.WriteEndArray();
            return result;
        }

        /// <summary>
        /// Entry point for Operation Result processing. 
        /// </summary>
        /// <param name="operationResult"></param>
        /// <returns></returns>
        private bool BuildOperationResult(RESTOperationResultCapability operationResult)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildOperationResult >> Building result '" + operationResult.Name + "'...");
            this._JSONWriter.WritePropertyName(operationResult.ResultCode);
            this._JSONWriter.WriteStartObject();
            this._JSONWriter.WritePropertyName("description");
            // Since multi-line strings don't really work here, we separate lines by two spaces...
            this._JSONWriter.WriteValue(MEChangeLog.GetDocumentationAsText(operationResult.CapabilityClass, "  "));
            this._JSONWriter.WriteEndObject();
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
            ContextSlt context = ContextSlt.GetContextSlt();
            if (this._currentResource.Archetype == RESTResourceCapability.ResourceArchetype.Identifier)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildParameters >> Identifier resource '" + this._currentResource.Name + "', build path parameter...");
                RESTParameterDeclaration resourceParam = this._currentResource.Parameter;
                if (resourceParam == null)
                {
                    Logger.WriteError("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildParameters >> Required ID parameter in Identifier Resource '" + 
                                      this._currentResource.Name + "' is missing!");
                }
                else result = WriteIdentifierParameter(operation);    // Locate and write the Identifier Parameter.
                if (!result) Logger.WriteError("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildParameters >> Required ID parameter in Identifier Resource '" +
                                                this._currentResource.Name + "' is missing!");
            }

            if (result)
            {
                // Check whether we must support pagination and body parameters...
                string paginationClass = context.GetConfigProperty(_PaginationClassName);
                string msgAssemblyStereotype = context.GetConfigProperty(_MessageAssemblyClassStereotype);

                // Go over each association, looking for stuff to process. Note that this will also retrieve the Operation Result items,
                // but we ignore these at this moment. Operation Result is a Capability in itself and will thus appear in due time through the main
                // processing loop as child capabilities of the Operation.
                foreach (MEAssociation assoc in operation.CapabilityClass.AssociationList)
                {
                    if (assoc.Destination.EndPoint.Name == paginationClass)
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildParameters >> Found Pagination class! Processing query parameters...");
                        result = WritePagination(assoc.Destination.EndPoint);
                    }
                    else if (assoc.Destination.EndPoint.HasStereotype(msgAssemblyStereotype))
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildParameters >> Found a body parameter...");
                        result = WriteBodyParameter(assoc.Destination.EndPoint);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Writes a body parameter using the specified message Profile root. Each operation may have at most ONE body parameter and each must
        /// refer to an, operation specific, unique message model. The root of this model is a Message Assembly class that may- or may not have
        /// private properties.
        /// The operation initiates schema generation for the associated message model and writes a reference to that model.
        /// </summary>
        /// <param name="messageProfile">The top of the message profile.</param>
        /// <returns></returns>
        private bool WriteBodyParameter(MEClass messageProfile)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteBodyParameter >> Processing body class '" + messageProfile.Name + "'...");
            this._panel.WriteInfo(this._panelIndex + 3, "Processing Message Body '" + messageProfile.Name + "'...");
            string qualifiedClassName = this._schema.ProcessClass(messageProfile, messageProfile.Name);
            if (qualifiedClassName != string.Empty)
            {
                this._JSONWriter.WriteStartObject();
                {
                    this._JSONWriter.WritePropertyName("name"); this._JSONWriter.WriteValue("body");
                    this._JSONWriter.WritePropertyName("in"); this._JSONWriter.WriteValue("body");
                    if (!string.IsNullOrEmpty(messageProfile.Annotation))
                    {
                        this._JSONWriter.WritePropertyName("description"); this._JSONWriter.WriteValue(messageProfile.Annotation);
                    }
                    this._JSONWriter.WritePropertyName("required"); this._JSONWriter.WriteValue("true");

                    // Since we 'might' use alias names in classes, the returned name 'might' be different from the offered name. To make sure we're referring
                    // to the correct name, we take the returned FQN and remove the token part. Remainder is the 'formal' type name.
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteBodyParameter >> Gotten FQN '" + qualifiedClassName + "'.");
                    string className = qualifiedClassName.Substring(qualifiedClassName.IndexOf(':')+1);
                    this._JSONWriter.WritePropertyName("schema"); this._JSONWriter.WriteStartObject();
                    {
                        this._JSONWriter.WriteRaw("\"$ref\": \"#/definitions/" + className + "\"");
                    } this._JSONWriter.WriteEndObject();
                } this._JSONWriter.WriteEndObject();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Searches the provided Operation capability for an Identifier Parameter and if found, writes the associated OpenAPI contents.
        /// This method should only be called in case of 'Identifier' resources and there should only be a single matching parameter.
        /// The parameter is indicated by a class attribute marked with stereotype 'RESTParameter' and it should match the parameter as stored
        /// in the Parameter property of the current resource. If the original class contained multiple parameters, we will use only the first
        /// one found (and since order is not necessarily guaranteed, this could result in unwanted behavior).
        /// </summary>
        /// <param name="operation">Operation to be processed.</param>
        /// <returns>True if parameter found and written, false when not found.</returns>
        private bool WriteIdentifierParameter(RESTOperationCapability operation)
        {
            bool result = false;
            RESTParameterDeclaration resourceParam = this._currentResource.Parameter;
            List<SchemaAttribute> resourceProperties = this._schema.ProcessProperties(this._currentResource.CapabilityClass);
            foreach (JSONContentAttribute attrib in resourceProperties)
            {
                if (attrib.Name == resourceParam.Name)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteIdentifierParameter >> Found Identifier property!");
                    this._JSONWriter.WriteStartObject();
                    {
                        this._JSONWriter.WritePropertyName("name");         this._JSONWriter.WriteValue(RESTUtil.GetAssignedRoleName(attrib.Name));
                        this._JSONWriter.WritePropertyName("in");           this._JSONWriter.WriteValue("path");
                        if (!string.IsNullOrEmpty(resourceParam.Description))
                        {
                            this._JSONWriter.WritePropertyName("description"); this._JSONWriter.WriteValue(resourceParam.Description);
                        }
                        this._JSONWriter.WritePropertyName("required");     this._JSONWriter.WriteValue("true");

                        // Collect the JSON Schema for the attribute as a string...
                        string attribText = attrib.GetClassifierAsText();
                        attribText = attribText.Substring(1, attribText.Length - 2);    // Get rid of '{' and '}' from the schema.
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
                            else Logger.WriteWarning("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteIdentifierParameter >> Collection specification is missing in resource '" + this._currentResource.Name + "'!");
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
                    this._JSONWriter.WritePropertyName("required"); this._JSONWriter.WriteValue(attrib.IsMandatory ? "true" : "false");

                    // Collect the JSON Schema for the attribute as a string...
                    string attribText = attrib.GetClassifierAsText();
                    attribText = attribText.Substring(1, attribText.Length - 2);    // Get rid of '{' and '}' from the schema.
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
                        else Logger.WriteWarning("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WritePagination >> Collection specification is missing in attribute '" + attrib.Name + "'!");
                    }
                }
                this._JSONWriter.WriteEndObject();
                result = true;
            }
            return result;
        }
    }
}
