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
        private const string _ConsumesMIMEListTag   = "ConsumesMIMEListTag";
        private const string _ProducesMIMEListTag   = "ProducesMIMEListTag";

        // Separator between summary text and description text
        private const string _Summary               = "summary: ";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
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
                    this._JSONWriter.WritePropertyName("summary");
                    this._JSONWriter.WriteValue(summary);
                }
                if (documentation.Count > 1)
                {
                    string description = string.Empty;
                    // We already copied the first line, so now append all additional lines...
                    bool firstLine = true;
                    for (int i = 1; i < documentation.Count; i++)
                    {
                        description += firstLine ? documentation[i] : "\\n" + documentation[i];
                        firstLine = false;
                    }
                    this._JSONWriter.WritePropertyName("description");
                    this._JSONWriter.WriteValue(description);
                }
            }

            // Build the MIME list for this operation, if 
            this._JSONWriter.WritePropertyName("operationId"); this._JSONWriter.WriteValue(Conversions.ToCamelCase(operation.Name));
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
        /// 
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

        private bool BuildParameters(RESTOperationCapability operation)
        {
            if (this._currentResource.Archetype == RESTResourceCapability.ResourceArchetype.Identifier)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildParameters >> Identifier resource '" + this._currentResource.Name + "', build path parameter...");
                RESTParameterDeclaration resourceParam = this._currentResource.Parameter;
                if (resourceParam == null)
                {
                    Logger.WriteError("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildParameters >> Required ID parameter in Identifier Resource '" + 
                                      this._currentResource.Name + "' is missing!");
                    return false;
                }
                // Processing class properties provides us with a classifier definition (and if necessary, registration) of the identifier property...
                // There should be only one property but if there are more, we simply take the one that matches our resource parameter...
                List<SchemaAttribute> resourceProperties = this._schema.ProcessProperties(this._currentResource.CapabilityClass); 
                foreach (JSONContentAttribute attrib in resourceProperties)
                {
                    if (attrib.Name == resourceParam.Name)
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildParameters >> Found Identifier property!");
                        this._JSONWriter.WriteStartObject();
                        {
                            this._JSONWriter.WritePropertyName("name");
                            this._JSONWriter.WriteValue(Conversions.ToCamelCase(attrib.Name));
                            this._JSONWriter.WritePropertyName("in");
                            this._JSONWriter.WriteValue("path");
                            this._JSONWriter.WritePropertyName("description");
                            {
                                // Since multi-line strings don't work here, we replace line breaks by two spaces.
                                string documentation = MEChangeLog.GetDocumentationAsText(operation.CapabilityClass, "  ");
                                this._JSONWriter.WriteValue(resourceParam.Description);
                            }
                            this._JSONWriter.WritePropertyName("required");
                            this._JSONWriter.WriteRawValue("\"true\",");

                            // Collect the JSON Schema for the attribute as a string...
                            string attribText = attrib.GetClassifierAsText();
                            attribText = attribText.Substring(1, attribText.Length - 2);    // Get rid of '{' and '}' from the schema.
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildParameters >> Got attribute: '" + attribText + "'...");
                            this._JSONWriter.WriteRaw(attribText);
                            if (attrib.IsListRequired)
                            {
                                if (resourceParam.CollectionFormat != RESTParameterDeclaration.QueryCollectionFormat.Unknown &&
                                    resourceParam.CollectionFormat != RESTParameterDeclaration.QueryCollectionFormat.NA)
                                {
                                    this._JSONWriter.WritePropertyName("collectionFormat");
                                    this._JSONWriter.WriteValue(resourceParam.CollectionFormat.ToString().ToLower());
                                }
                                else Logger.WriteWarning("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildParameters >> Collection specification is missing in resource '" + this._currentResource.Name + "'!");
                            }
                        } this._JSONWriter.WriteEndObject();
                    }
                }
            }
            return true;
        }
    }
}
