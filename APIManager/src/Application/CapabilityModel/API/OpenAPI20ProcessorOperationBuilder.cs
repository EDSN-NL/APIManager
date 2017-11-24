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
                //PARAMETERS GO HERE
            } this._JSONWriter.WriteEndArray();

            return true;
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



        /// <summary>
        /// Checks whether the specified class is associated with a Documentation Info Descriptor and if so, writes the attribute values to 
        /// the output stream.
        /// </summary>
        /// <param name="wr">JSON Output Stream.</param>
        /// <param name="thisClass">Class to test.</param>
        private void WriteDocumentationTMP(JsonTextWriter wr, MEClass thisClass)
        {
            string assocClassName = ContextSlt.GetContextSlt().GetConfigProperty(_DocumentationTypeClassName);
            foreach (MEAssociation assoc in thisClass.AssociationList)
            {
                if (assoc.Destination.EndPoint.Name == assocClassName)
                {
                    wr.WritePropertyName("externalDocs");
                    wr.WriteStartObject();
                    foreach (MEAttribute attrib in assoc.Destination.EndPoint.Attributes)
                    {
                        wr.WritePropertyName(attrib.Name.ToLower());
                        wr.WriteValue(attrib.FixedValue);
                    }
                    wr.WriteEndObject();
                    break;
                }
            }
        }




    }
}
