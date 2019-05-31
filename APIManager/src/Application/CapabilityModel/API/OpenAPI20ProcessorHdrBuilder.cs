using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Framework.Logging;
using Framework.Model;
using Framework.Util;
using Framework.Context;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// The OpenAPI 2.0 Processor is a Capability Processor that takes an Interface Capability node and creates an OpenAPI 2.0 definition file, 
    /// otherwise known as 'Swagger file'. It only supports processing from the Interface downwards.
    /// </summary>
    internal partial class OpenAPI20Processor : CapabilityProcessor
    {
        // Possible types of interface contracts (only one type is supported at this time):
        private enum ContractType { Unknown, OpenAPI_20 }

        // Configuration properties used by this module...
        private const string _ContactTypeClassName              = "ContactTypeClassName";
        private const string _LicenseTypeClassName              = "LicenseTypeClassName";
        private const string _DocumentationTypeClassName        = "DocumentationTypeClassName";
        private const string _TermsOfServiceAttributeName       = "TermsOfServiceAttributeName";
        private const string _InterfaceContractTypeTag          = "InterfaceContractTypeTag";
        private const string _InterfaceDefaultSOAPContract      = "InterfaceDefaultSOAPContract";
        private const string _AccessLevelTag                    = "AccessLevelTag";
        private const string _AccessLevels                      = "AccessLevels";
        private const string _DefaultMIMEList                   = "DefaultMIMEList";

        /// <summary>
        /// Writes the header-section of the OpenAPI 2.0 definition file. That is, all generic parameters, up-to and including the 'tags' section.
        /// </summary>
        /// <param name="wr">JSON Writer that receives the output.</param>
        /// <param name="itf">The interface that is currently being processed.</param>
        private void BuildHeader(JsonTextWriter wr, RESTInterfaceCapability itf)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            wr.WritePropertyName("swagger"); wr.WriteValue("2.0");
            wr.WritePropertyName("info"); wr.WriteStartObject();
            {
                wr.WritePropertyName("title"); wr.WriteValue(RESTUtil.GetAssignedRoleName(itf.Name));
                wr.WritePropertyName("description");
                {
                    // Since multi-line strings don't work here, we replace line breaks by two spaces.
                    string documentation = MEChangeLog.GetDocumentationAsText(itf.CapabilityClass, "  ");
                    //wr.WriteValue(documentation);
                    wr.WriteValue("API Version: " +
                                  itf.RootService.Version.Item1 + "." +
                                  itf.RootService.Version.Item2 + "." +
                                  itf.RootService.BuildNumber + " - " + documentation);
                }
                WriteTermsOfService(wr, itf);
                WriteContactInfo(wr, itf);
                WriteLicenseInfo(wr, itf);
                wr.WritePropertyName("version");
                if (context.GetBoolSetting(FrameworkSettings._GENUseMajorVersionOnly)) wr.WriteValue("v" + itf.RootService.MajorVersion.ToString());
                else                                                                   wr.WriteValue(itf.VersionString);
            } wr.WriteEndObject();
            wr.WritePropertyName("host"); wr.WriteValue(context.GetStringSetting(FrameworkSettings._RESTHostName));
            wr.WritePropertyName("basePath");
                wr.WriteValue("/" + RESTUtil.GetAssignedRoleName(itf.Name) + "/v" + itf.RootService.MajorVersion.ToString());
            wr.WritePropertyName("schemes"); wr.WriteStartArray();
            {
                string[] schemes = context.GetStringSetting(FrameworkSettings._RESTSchemes).Split(',');
                for (int i = 0; i < schemes.Length; wr.WriteValue(schemes[i++].Trim()));
            } wr.WriteEndArray();
            string[] defaultMIMEList = context.GetConfigProperty(_DefaultMIMEList).Split(',');
            wr.WritePropertyName("produces"); wr.WriteStartArray();
            {
                for (int i = 0; i < defaultMIMEList.Length; wr.WriteValue(defaultMIMEList[i++].Trim()));
            } wr.WriteEndArray();
            wr.WritePropertyName("consumes"); wr.WriteStartArray();
            {
                for (int i = 0; i < defaultMIMEList.Length; wr.WriteValue(defaultMIMEList[i++].Trim()));
            } wr.WriteEndArray();
            BuildTags(wr, itf);
        }

        /// <summary>
        /// Builds the 'tags' section in the OpenAPI definition file using all resources that have registered themselves as having tags. 
        /// For each tag-resource, a 'tag' entry is constructed, documenting that resource. we ONLY formally If there are no top-level resources 
        /// (unlikely), the operation does not perform any operations.
        /// </summary>
        /// <param name="wr">JSON Output Stream.</param>
        /// <param name="itf">The interface for which we are creating the tags section.</param>
        private void BuildTags(JsonTextWriter wr, RESTInterfaceCapability itf)
        {
            RESTService svc = itf.RootService as RESTService;
            bool tagsInitialized = false;
            List<string> processedTags = new List<string>();
            if (svc != null)
            {
                foreach (RESTResourceCapability resource in svc.TagList)
                {
                    // First time around, make sure we open the Tags section in the output...
                    if (!tagsInitialized)
                    {
                        wr.WritePropertyName("tags");
                        wr.WriteStartArray();
                        tagsInitialized = true;
                    }
                    foreach (string tagName in resource.TagNames)
                    {
                        if (!processedTags.Contains(tagName))
                        {
                            wr.WriteStartObject();
                            {
                                wr.WritePropertyName("name"); wr.WriteValue(tagName);
                                wr.WritePropertyName("description"); wr.WriteValue(MEChangeLog.GetDocumentationAsText(resource.CapabilityClass));
                                WriteDocumentation(wr, resource.CapabilityClass);
                                processedTags.Add(tagName);
                            }
                            wr.WriteEndObject();
                        }
                    }
                }
                if (tagsInitialized) wr.WriteEndArray();
            }
        }

        /// <summary>
        /// Iterates over all assigned operations and determines the most restrictive access level, which is then returned.
        /// If a configuration property indicates that access levels are not required, the function returns "Undefined".
        /// </summary>
        /// <returns>Access level for the interface.</returns>
        private string GetInterfaceAccessLevel(MEClass interfaceClass)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string accessLevel = "Undefined";
            if (context.GetBoolSetting(FrameworkSettings._SMUseSecurityLevels))
            {
                string accessLevelTag = context.GetConfigProperty(_AccessLevelTag);

                // We create a sorted list of all possible levels, indexed by name and indexed by number...
                // Configuration contains a list of tuples <nr>:<name> for each access level.
                SortedList<string, int> levelByName = new SortedList<string, int>();
                SortedList<int, string> levelByNumber = new SortedList<int, string>();
                string[] levelSet = context.GetConfigProperty(_AccessLevels).Split(',');
                foreach (string levelOption in levelSet)
                {
                    int levelNumber = Convert.ToInt16(levelOption.Substring(0, levelOption.IndexOf(':')));
                    string levelName = levelOption.Substring(levelOption.IndexOf(':') + 1);
                    levelByName.Add(levelName, levelNumber);
                    levelByNumber.Add(levelNumber, levelName);
                }

                // Now we iterate over all operations, looking for the most restricted access level (== highest numerical value)...
                int highestLevel = 0;
                foreach (Tuple<string, string> level in this._accessLevels)
                {
                    string selectedLevel = level.Item2;
                    int numericLevel = levelByName[selectedLevel.Substring(selectedLevel.IndexOf(':') + 1)];
                    if (numericLevel > highestLevel) highestLevel = numericLevel;
                }

                // Now we'll use the index number to translate back to the corresponding tag value. We store this in the Interface and return it.
                accessLevel = levelByNumber[highestLevel];
                interfaceClass.SetTag(accessLevelTag, accessLevel);
            }
            return accessLevel;
        }

        /// <summary>
        /// Determines the Interface Contract Type to be generated.
        /// Format is <standard>:<type>
        /// At the moment, this processor ONLY supports 'REST:OpenAPI_2.0'!
        /// </summary>
        /// <param name="interfaceClass">Interface class to be checked.</param>
        /// <returns>Contract type for this Interface.</returns>
        private ContractType GetInterfaceContractTag(MEClass interfaceClass)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            ContractType contractType;
            string contractTag = interfaceClass.GetTag(context.GetConfigProperty(_InterfaceContractTypeTag));
            if (string.IsNullOrEmpty(contractTag)) contractTag = context.GetConfigProperty(_InterfaceDefaultSOAPContract);
            contractTag = contractTag.ToLower();
            string contractStandard = contractTag.Substring(0, contractTag.IndexOf(':'));
            contractTag = contractTag.Substring(contractTag.IndexOf(':') + 1);

            // At the moment, we only support REST-type interface contracts. All other prefixed will be simply ignored.
            if (contractStandard != "rest")
            {
                Logger.WriteWarning("Unsupported contract type: '" + contractStandard + "' specified, ignored!");
                this._panel.WriteWarning(this._panelIndex, "Unsupported contract type: '" + contractStandard + "' specified, ignored!");
            }

            if (contractTag != string.Empty)
            {
                switch (contractTag)
                {
                    case "openapi_2.0":
                        contractType = ContractType.OpenAPI_20;
                        break;

                    default:
                        contractType = ContractType.Unknown;
                        break;
                }
            }
            else contractType = ContractType.OpenAPI_20;
            return contractType;
        }

        /// <summary>
        /// Checks whether the Interface is associated with a Contact Info Descriptor and if so, writes the attribute values to 
        /// the output stream.
        /// </summary>
        /// <param name="wr"></param>
        /// <param name="itf"></param>
        private void WriteContactInfo(JsonTextWriter wr, RESTInterfaceCapability itf)
        {
            string assocClassName = ContextSlt.GetContextSlt().GetConfigProperty(_ContactTypeClassName);
            foreach (MEAssociation assoc in itf.CapabilityClass.AssociationList)
            {
                if (assoc.Destination.EndPoint.Name == assocClassName)
                {
                    wr.WritePropertyName("contact");
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

        /// <summary>
        /// Checks whether the specified class is associated with a Documentation Info Descriptor and if so, writes the attribute values to 
        /// the output stream.
        /// </summary>
        /// <param name="wr">JSON Output Stream.</param>
        /// <param name="thisClass">Class to test.</param>
        private void WriteDocumentation(JsonTextWriter wr, MEClass thisClass)
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

        /// <summary>
        /// Checks whether the Interface is associated with a License Info Descriptor and if so, writes the attribute values to 
        /// the output stream.
        /// </summary>
        /// <param name="wr"></param>
        /// <param name="itf"></param>
        private void WriteLicenseInfo(JsonTextWriter wr, RESTInterfaceCapability itf)
        {
            string assocClassName = ContextSlt.GetContextSlt().GetConfigProperty(_LicenseTypeClassName);
            foreach (MEAssociation assoc in itf.CapabilityClass.AssociationList)
            {
                if (assoc.Destination.EndPoint.Name == assocClassName)
                {
                    wr.WritePropertyName("license");
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

        /// <summary>
        /// Check whether the current interface has a 'Terms of Service' descriptor associated and if so, write its contents to the OpenAPI writer.
        /// </summary>
        /// <param name="wr">JSON Text Writer that will receive the output (if any).</param>
        /// <param name="itf">Interface that we're currently processing.</param>
        private void WriteTermsOfService(JsonTextWriter wr, RESTInterfaceCapability itf)
        {
            MEAttribute tos = itf.CapabilityClass.FindAttribute(ContextSlt.GetContextSlt().GetConfigProperty(_TermsOfServiceAttributeName));
            if (tos != null && !string.IsNullOrEmpty(tos.FixedValue))
            {
                wr.WritePropertyName("termsOfService");
                wr.WriteValue(tos.FixedValue);
            }
        }
    }
}
