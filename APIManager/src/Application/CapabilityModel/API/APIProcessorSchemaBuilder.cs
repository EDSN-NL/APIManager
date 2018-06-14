using System;
using System.Windows.Forms;
using Framework.Logging;
using Framework.Model;
using Framework.Util;
using Framework.Util.SchemaManagement;
using Framework.Context;
using Plugin.Application.Forms;
using Plugin.Application.CapabilityModel.ASCIIDoc;
using Plugin.Application.CapabilityModel.SchemaGeneration;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// The SOAP Processor is a Capability Processor that takes an Interface Capability node and creates a WSDL definition for that Interface.
    /// This WSDL contains operations (and associated schemas) for all operations that are selected by the user.
    /// </summary>
    internal partial class APIProcessor : CapabilityProcessor
    {
        /// <summary>
        /// This function is called to build a schema for a single Operation. It creates schemas for all Messages defined for the Operation and 
        /// subsequently merges these together to form a single Operation schema.
        /// </summary>
        /// <param name="operation">The Operation capability that must be processed.</param>
        /// <returns>True when processed Ok, false on errors.</returns>
        private bool BuildSchema(OperationCapability operation)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.APIProcessor.BuildSchema >> Building schema for operation '" + operation.Name + "'...");
            ContextSlt context = ContextSlt.GetContextSlt();
            string namespaceTag = (this._interfaceType == InterfaceType.SOAP) ? "SOAPOperation" : "RESTOperation";
            string schemaName = this._currentService.Name + "." + operation.Name;
            this._currentSchema = GetSchema(Schema.SchemaType.Operation, schemaName, operation.CapabilityClass.GetTag(context.GetConfigProperty(_NSTokenTag)),
                                            this._currentService.GetFQN(namespaceTag, operation.Name, -1), operation.VersionString);
            bool result = true;
            var processingContext = new ProcessingContext(operation);
            OperationDocContext docContext = DocManagerSlt.GetDocManagerSlt().GetNewOperationDocContext(this._currentSchema.NSToken, 
                                                                                                        operation.AssignedRole, 
                                                                                                        MEChangeLog.GetDocumentationAsText(operation.CapabilityClass));

            if (this._interfaceType == InterfaceType.SOAP)
            {
                // We MUST add the Common Schema as an external namespace to our Operation Schema. If we're using one, it must have been created
                // during pre-processing and we can thus safely reference it here.
                if (this._commonSchema != null)
                {
                    string namespaceLocation = (context.GetConfigProperty(_SchemaUseRelativePathName) == "true") ? string.Empty : this._currentService.ServiceCIPath + "/";
                    namespaceLocation += this._commonSchemaCapability.BaseFileName + ".xsd";
                    this._currentSchema.AddNamespace(this._commonSchemaCapability.NSToken, this._commonSchema.SchemaNamespace, namespaceLocation);
                }
            }
            else // REST
            {
                // In case of REST (JSON Schema), we must physically attach the Common Schema, since it is used to store (and retrieve) definitions...
                // JSON does NOT implement the 'AddNamespace' operation, so we can forget about that here.
                this._currentSchema.AddSchemaReference(this._commonSchema);
            }

            TreeNode<CapabilityImp> hierarchy = operation.CapabilityTree;
            SchemaProcessor schemaProcessor = null;
            foreach (CapabilityImp child in hierarchy.ChildObjects)
            {
                if (child is MessageCapabilityImp)
                {
                    this._panel.WriteInfo(this._panelIndex + 2, "Processing Message '" + child.Name + "'...");
                    var childMsg = new MessageCapability((MessageCapabilityImp)(child));
                    docContext.SwitchClass(OperationDocContext.Chapter.Message, childMsg.AssignedRole, 
                                           MEChangeLog.GetDocumentationAsText(childMsg.CapabilityClass), 
                                           !child.CapabilityClass.HasContents());
                    schemaProcessor = new SchemaProcessor(this._commonSchema, this._panelIndex + 3);
                    if (result = schemaProcessor.ProcessCapability(childMsg, ProcessingStage.PreProcess))
                    {
                        // In case of REST services, we propagate the result of each processed operation to the next. This way,
                        // we can be sure that the operation schema contains ALL definitions required by the operations (unlike
                        // XSD's where a reference is sufficient, for JSON we need access to the actual type definitions)!
                        if (this._interfaceType == InterfaceType.REST) schemaProcessor.MessageSchema.Merge(this._currentSchema);

                        if (result = schemaProcessor.ProcessCapability(childMsg, ProcessingStage.Process))
                        {
                            // When processing was successfull, we merge the message schema into our Operation Schema.
                            // After all processing is done, the current schema contains all necessary elements.
                            this._currentSchema.Merge(schemaProcessor.MessageSchema);
                            schemaProcessor.ProcessCapability(childMsg, ProcessingStage.PostProcess);
                            if (schemaProcessor.HasGeneratedOutput) processingContext.AddMessage(childMsg);
                        }
                    }
                }
            }

            if (result)
            {
                // When done, make sure that we have a valid access level annotation for the header. Also store this for post-processing
                // so that Interface can determine an overall Interface access level.
                // Finally, save the generated schema and if ok, register the operation in the operation list.
                this._panel.WriteInfo(this._panelIndex + 2, "Finalizing operation...");
                this._currentAccessLevel = GetOperationAccessLevel(operation.CapabilityClass);
                this._accessLevels.Add(new Tuple<string, string>(operation.Name, this._currentAccessLevel));
                if (result = SaveProcessedCapability()) this._operationContextList.Add(processingContext);
            }
            return result;
        }

        /// <summary>
        /// This function checks the provided operation class for the definition of a valid Access Level. If not found, the user is asked to provide one.
        /// The selected or exiting level is returned. On return, the value of the access level tag in the class is up-to-date.
        /// When a configuration property indicates that access levels are not required, the function returns 'Undefined'.
        /// </summary>
        /// <param name="operationClass">Operation class to check against.</param>
        /// <returns>Access level.</returns>
        private string GetOperationAccessLevel(MEClass operationClass)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.APIProcessor.GetOperationAccessLevel >> Retrieve privacy access level for operation '" + operationClass.Name + "'...");

            ContextSlt context = ContextSlt.GetContextSlt();
            string accessLevel = "Undefined";
            if (context.GetBoolSetting(FrameworkSettings._SMUseSecurityLevels))
            {
                string accessLevelTag = context.GetConfigProperty(_AccessLevelTag);
                accessLevel = operationClass.GetTag(accessLevelTag);
                if (string.IsNullOrEmpty(accessLevel))
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.APIProcessor.GetOperationAccessLevel >> No level defined yet, ask the user...");
                    using (AccessLevelForm accessLevelForm = new AccessLevelForm())
                    {
                        if (accessLevelForm.ShowDialog() == DialogResult.OK)
                        {
                            string[] levelSet = context.GetConfigProperty(_AccessLevels).Split(',');
                            foreach (string element in levelSet)
                            {
                                if (Convert.ToInt16(element.Substring(0, element.IndexOf(':'))) == accessLevelForm.level)
                                {
                                    string assignedLevel = element.Substring(element.IndexOf(':') + 1);
                                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.APIProcessor.GetOperationAccessLevel >> User selected: " + assignedLevel);
                                    operationClass.SetTag(accessLevelTag, assignedLevel);
                                    accessLevel = assignedLevel;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.APIProcessor.GetOperationAccessLevel >> Returning level: '" + accessLevel + "'.");
            return accessLevel;
        }
    }
}
