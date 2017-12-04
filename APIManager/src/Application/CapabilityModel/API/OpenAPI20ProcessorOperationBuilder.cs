﻿using System;
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
        private const string _PaginationClassName               = "PaginationClassName";
        private const string _OperationResultClassName          = "OperationResultClassName";
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
        /// Entry point for Operation Result processing. The method parses the provided Operation Result capability and generates the associated Response Object.
        /// It writes the result code and description and, if specified, generates the response schema.
        /// </summary>
        /// <param name="operationResult">The Operation Result capability to process.</param>
        /// <returns>True when successfully completed, false on errors.</returns>
        private bool BuildOperationResult(RESTOperationResultCapability operationResult)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildOperationResult >> Building result '" + operationResult.Name + "'...");
            ContextSlt context = ContextSlt.GetContextSlt();
            bool result = true;
            this._JSONWriter.WritePropertyName(operationResult.ResultCode); this._JSONWriter.WriteStartObject();
            {
                // Since multi-line documentation does not really work with JSON, we replace line breaks by spaces...
                string resultDocumentation = MEChangeLog.GetDocumentationAsText(operationResult.CapabilityClass, "  ");
                if (!string.IsNullOrEmpty(resultDocumentation))
                {
                    this._JSONWriter.WritePropertyName("description");
                    this._JSONWriter.WriteValue(resultDocumentation);
                }

                // Check whether we must support a default response body or body parameters...
                string defaultResponseClass = context.GetConfigProperty(_OperationResultClassName);
                string msgAssemblyStereotype = context.GetConfigProperty(_MessageAssemblyClassStereotype);

                // Go over each association, looking for stuff to process....
                foreach (MEAssociation assoc in operationResult.CapabilityClass.AssociationList)
                {
                    if (assoc.Destination.EndPoint.Name == defaultResponseClass)
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildOperationResult >> Found default response class, generate reference...");
                        result = WriteDefaultResponse(assoc.Destination.EndPoint);
                    }
                    else if (assoc.Destination.EndPoint.HasStereotype(msgAssemblyStereotype))
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildOperationResult >> Found a body parameter...");
                        result = WriteResponseBodyParameter(assoc.Destination.EndPoint, assoc.GetCardinality(MEAssociation.AssociationEnd.Destination));
                    }
                }
            } this._JSONWriter.WriteEndObject();
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
                        result = WriteRequestBodyParameter(assoc.Destination.EndPoint, assoc.GetCardinality(MEAssociation.AssociationEnd.Destination));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// This method receives a reference to the default response class. Since we want to use a shared object definition, we must make sure that 
        /// the class is parsed first time around so that it is entered in the schema as a type definition.
        /// We use the class-attribute 'defaultResponseClassifier' to check whether the class has been processed. Initially, this is an empty string.
        /// After successfull processing, the attribute contains the classifier name for the class as present in the definitions section.
        /// </summary>
        /// <param name="responseClass">The class associated with the default response.</param>
        /// <returns>True on successfull completion, false on errors.</returns>
        private bool WriteDefaultResponse(MEClass responseClass)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteDefaultResponse >> Processing response class '" + responseClass.Name + "'...");
            this._panel.WriteInfo(this._panelIndex + 3, "Processing Default Response Body '" + responseClass.Name + "'...");

            // We must only parse this class once in order to get a valid definition in the 'definitions' section.
            if (this._defaultResponseClassifier == string.Empty)
            {
                this._defaultResponseClassifier = this._schema.ProcessClass(responseClass, responseClass.Name);
                // Since we 'might' use alias names in classes, the returned name 'might' be different from the offered name. To make sure 
                // we're referring to the correct name, we take the returned FQN and remove the token part. Remainder is the 'formal' type name.
                if (this._defaultResponseClassifier != string.Empty)
                {
                    this._defaultResponseClassifier = this._defaultResponseClassifier.Substring(this._defaultResponseClassifier.IndexOf(':') + 1);
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteDefaultResponse >> Got classifier '" + this._defaultResponseClassifier + "'.");
                }
                else
                {
                    Logger.WriteError("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteDefaultResponse >> Unable to process default response '" + responseClass.Name + "'!");
                    return false;
                }
            }

            // Now, create a schema reference in the output...
            this._JSONWriter.WritePropertyName("schema"); this._JSONWriter.WriteStartObject();
            {
                this._JSONWriter.WriteRaw("\"$ref\": \"#/definitions/" + this._defaultResponseClassifier + "\"");
            } this._JSONWriter.WriteEndObject();
            return true;
        }

        /// <summary>
        /// Writes a request body parameter using the specified message Profile root. Each operation may have at most ONE body parameter and each
        /// must refer to an, operation specific, unique message model. The root of this model is a Message Assembly class that may- or may not
        /// have private properties.
        /// The operation initiates schema generation for the associated message model and writes a reference to that model.
        /// </summary>
        /// <param name="messageProfile">The top of the message profile.</param>
        /// <param name="cardinality">The cardinality of the request object association. If upper limit > 1, we must create an array.</param>
        /// <returns>True when processed ok, false on errors.</returns>
        private bool WriteRequestBodyParameter(MEClass messageProfile, Tuple<int,int> cardinality)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteRequestBodyParameter >> Processing body class '" + messageProfile.Name + "'...");
            this._panel.WriteInfo(this._panelIndex + 3, "Processing Request Message Body '" + messageProfile.Name + "'...");
            string qualifiedClassName = this._schema.ProcessClass(messageProfile, messageProfile.Name);
            bool result = false;
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
                    this._JSONWriter.WritePropertyName("required"); this._JSONWriter.WriteValue(true);

                    // Since we 'might' use alias names in classes, the returned name 'might' be different from the offered name. To make sure we're referring
                    // to the correct name, we take the returned FQN and remove the token part. Remainder is the 'formal' type name.
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteRequestBodyParameter >> Gotten FQN '" + qualifiedClassName + "'.");
                    string className = qualifiedClassName.Substring(qualifiedClassName.IndexOf(':')+1);
                    this._JSONWriter.WritePropertyName("schema"); this._JSONWriter.WriteStartObject();
                    {
                        if (cardinality.Item2 == 0 || cardinality.Item2 > 1)
                        {
                            this._JSONWriter.WritePropertyName("type"); this._JSONWriter.WriteValue("array");
                            this._JSONWriter.WritePropertyName("items"); this._JSONWriter.WriteStartObject();
                            {
                                this._JSONWriter.WriteRaw("\"$ref\": \"#/definitions/" + className + "\"");
                            }
                            this._JSONWriter.WriteEndObject();
                            if (cardinality.Item1 > 1)
                            {
                                this._JSONWriter.WritePropertyName("minItems"); this._JSONWriter.WriteValue(cardinality.Item1);
                            }
                            if (cardinality.Item2 != 0)
                            {
                                this._JSONWriter.WritePropertyName("maxItems"); this._JSONWriter.WriteValue(cardinality.Item2);
                            }
                        }
                        else this._JSONWriter.WriteRaw("\"$ref\": \"#/definitions/" + className + "\"");
                    } this._JSONWriter.WriteEndObject();
                } this._JSONWriter.WriteEndObject();
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Writes a request body parameter using the specified message Profile root. Each operation may have at most ONE body parameter and each
        /// must refer to an, operation specific, unique message model. The root of this model is a Message Assembly class that may- or may not
        /// have private properties. The method always creates a reference to the Message Assembly class. If the association role has a cardinality
        /// > 1, we create an array of references.
        /// The method checks whether the Message Assembly has associations. If so, it is considered a complex model and added as a reference.
        /// If the Message Assemble does not have associations, it is expanded in-line in the response as a response object.
        /// </summary>
        /// <param name="messageProfile">The top of the message profile.</param>
        /// <param name="cardinality">The cardinality of the response object association. If upper limit > 1, we must create an array.</param>
        /// <returns>True when processed ok, false on errors.</returns>
        private bool WriteResponseBodyParameter(MEClass messageProfile, Tuple<int,int> cardinality)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteResponseBodyParameter >> Processing body class '" + messageProfile.Name + "'...");
            this._panel.WriteInfo(this._panelIndex + 3, "Processing Response Message Body '" + messageProfile.Name + "'...");
            bool result = false;
            string qualifiedClassName = this._schema.ProcessClass(messageProfile, messageProfile.Name);

            if (qualifiedClassName != string.Empty)
            {
                // Since we 'might' use alias names in classes, the returned name 'might' be different from the offered name. To make sure we're referring
                // to the correct name, we take the returned FQN and remove the token part. Remainder is the 'formal' type name.
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.WriteBodyParameter >> Gotten FQN '" + qualifiedClassName + "'.");
                string className = qualifiedClassName.Substring(qualifiedClassName.IndexOf(':') + 1);
                this._JSONWriter.WritePropertyName("schema"); this._JSONWriter.WriteStartObject();
                {
                    if (cardinality.Item2 == 0 || cardinality.Item2 > 1)
                    {
                        this._JSONWriter.WritePropertyName("type"); this._JSONWriter.WriteValue("array");
                        this._JSONWriter.WritePropertyName("items"); this._JSONWriter.WriteStartObject();
                        {
                            this._JSONWriter.WriteRaw("\"$ref\": \"#/definitions/" + className + "\"");
                        } this._JSONWriter.WriteEndObject();
                        if (cardinality.Item1 > 1)
                        {
                            this._JSONWriter.WritePropertyName("minItems"); this._JSONWriter.WriteValue(cardinality.Item1);
                        }
                        if (cardinality.Item2 != 0)
                        {
                            this._JSONWriter.WritePropertyName("maxItems"); this._JSONWriter.WriteValue(cardinality.Item2);
                        }
                    }
                    else this._JSONWriter.WriteRaw("\"$ref\": \"#/definitions/" + className + "\"");
                } this._JSONWriter.WriteEndObject();
                result = true;
            }
            return result;
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
                        this._JSONWriter.WritePropertyName("required");     this._JSONWriter.WriteValue(true);

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
                    this._JSONWriter.WritePropertyName("required"); this._JSONWriter.WriteValue(attrib.IsMandatory ? true : false);

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
