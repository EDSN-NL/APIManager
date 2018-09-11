using System;
using System.Collections.Generic;
using Framework.Logging;
using Framework.Model;
using Framework.Util;
using Framework.Context;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// The SOAP Processor is a Capability Processor that takes an Interface Capability node and creates a WSDL definition for that Interface.
    /// This WSDL contains operations (and associated schemas) for all operations that are selected by the user.
    /// </summary>
    internal partial class APIProcessor : CapabilityProcessor
    {
        // Possible types of interface contracts (not all are supported as of this time):
        private enum ContractType { Abstract, Concrete, Topic }     

        // Configuration properties used by this module...
        private const string _JMSNamespace                  = "JMSNamespace";
        private const string _JMSNamespaceToken             = "JMSNamespaceToken";
        private const string _JMSTransport                  = "JMSTransport";
        private const string _InterfaceContractTypeTag      = "InterfaceContractTypeTag";
        private const string _InterfaceDefaultSOAPContract  = "InterfaceDefaultSOAPContract";
        private const string _SchemaUseRelativePathName     = "SchemaUseRelativePathName";

        // Interface building blocks...
        private static string _interfaceLeader              = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
        private static string _localToken                   = "itf";
        private static string _interfaceSkeleton            =
@"<definitions xmlns=""http://schemas.xmlsoap.org/wsdl/"" 
   xmlns:xs=""http://www.w3.org/2001/XMLSchema""
   xmlns:soap=""http://schemas.xmlsoap.org/wsdl/soap/"" @JMSNAMESPACE@
   targetNamespace=""@MYNAMESPACE@""
   xmlns:itf=""@MYNAMESPACE@""@NAMESPACEDECL@>

   <types>
       <xs:schema>@IMPORTDECL@
       </xs:schema>
   </types>

@MESSAGEDECL@
   <portType name=""@ITFNAME@PortType"">@OPERATIONDECL@
   </portType> @BINDINGDECL@ @SERVICEDECL@
</definitions>";

        /// <summary>
        /// Helper method that builds a WSDL Binding section (in this case, only JMS binding is supported).
        /// </summary>
        /// <returns>Binding declaration string</returns>
        private string BuildBindingDecl(InterfaceCapability itf)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string bindingDecl = "\n\n   <binding name=\"" + itf.Name + "PortTypeJMSBinding\" type=\"" + _localToken + ":" + itf.Name + "PortType\">";
            //bindingDecl += "\n      <soap:binding style=\"document\" transport=\"" + context.GetConfigProperty(_JMSTransport) + "\"/>";
            bindingDecl += "\n      <" + context.GetConfigProperty(_JMSNamespaceToken) + ":binding messageFormat=\"Text\"/>";

            // Create a backlink to the various operations...
            foreach (ProcessingContext ctx in this._operationContextList)
            {
                bindingDecl += "\n      <operation name=\"" + ctx.Operation.AssignedRole + "\">";
                foreach (Capability message in ctx.Messages)
                {
                    // In case of Notifications, we have only a single message that ends with name 'Message'
                    // All others are ignored here!!
                    if (message.Name.EndsWith("Message"))
                    {
                        bindingDecl += "\n         <soap:operation style=\"document\" soapAction=\"" + ctx.Operation.AssignedRole + "\"/>";
                        bindingDecl += "\n         <input>";
                        bindingDecl += "\n            <soap:body use =\"literal\" parts=\"part1\"/>";
                        bindingDecl += "\n         </input>";
                    }
                }
                bindingDecl += "\n      </operation>";
            }
            bindingDecl += "\n   </binding>";
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.APIProcessor.BuildBindingDecl >> Collected: " + bindingDecl);
            return bindingDecl;
        }

        /// <summary>
        /// This function is called to build a SOAP Interface Contract.
        /// </summary>
        /// <param name="itf">The Interface capability that must be processed.</param>
        /// <returns>True when processed Ok, false on errors.</returns>
        private bool BuildSOAPInterface(InterfaceCapability itf)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.APIProcessor.BuildSOAPInterface >> Building Interface '" + itf.Name + "'...");
            bool result = true;
            try
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                string jmsToken = context.GetConfigProperty(_JMSNamespaceToken);
                ContractType contractType = GetInterfaceContractTag(itf.CapabilityClass);
                this._currentAccessLevel = GetInterfaceAccessLevel(itf.CapabilityClass);
                string headerTemplate = context.GetResourceString(FrameworkSettings._SOAPInterfaceHeader);

                this._interfaceDeclaration = _interfaceLeader + "\n<!--" + BuildHeader(headerTemplate) + "-->\n" + _interfaceSkeleton;
                if (contractType == ContractType.Topic)
                {
                    this._interfaceDeclaration = this._interfaceDeclaration.Replace("@JMSNAMESPACE@", "\n   xmlns:" + jmsToken + "=\"" +
                                                                                    context.GetConfigProperty(_JMSNamespace) + "\"");
                    this._interfaceDeclaration = this._interfaceDeclaration.Replace("@BINDINGDECL@", BuildBindingDecl(itf));
                    this._interfaceDeclaration = this._interfaceDeclaration.Replace("@SERVICEDECL@", BuildServiceDecl(itf));
                }
                else
                {
                    this._interfaceDeclaration = this._interfaceDeclaration.Replace("@JMSNAMESPACE@", string.Empty);
                    this._interfaceDeclaration = this._interfaceDeclaration.Replace("@BINDINGDECL@", string.Empty);
                    this._interfaceDeclaration = this._interfaceDeclaration.Replace("@SERVICEDECL@", string.Empty);
                }

                this._interfaceDeclaration = this._interfaceDeclaration.Replace("@MYNAMESPACE@", this._currentService.GetFQN("SOAPInterface", null, -1));
                this._interfaceDeclaration = this._interfaceDeclaration.Replace("@ITFNAME@", Conversions.ToPascalCase(this._currentCapability.AssignedRole));
                this._interfaceDeclaration = this._interfaceDeclaration.Replace("@NAMESPACEDECL@", BuildNamespaceDecl());
                this._interfaceDeclaration = this._interfaceDeclaration.Replace("@IMPORTDECL@", BuildImportDecl());
                this._interfaceDeclaration = this._interfaceDeclaration.Replace("@MESSAGEDECL@", BuildMessageDecl());
                this._interfaceDeclaration = this._interfaceDeclaration.Replace("@OPERATIONDECL@", BuildOperationDecl());

                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.APIProcessor.BuildSOAPInterface >> Contract constructed:\n" + this._interfaceDeclaration);
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.APIProcessor.BuildSOAPInterface >> Caught exception: " + exc);
                this._panel.WriteError(this._panelIndex, "Caught exception while building Interface! " + exc);
                result = false;
            }

            this._panel.WriteInfo(this._panelIndex, "Finalizing Interface...");
            if (result) result = SaveProcessedCapability();
            return result;
        }

        /// <summary>
        /// Helper method, which creates a set of schema import declarations for each namespace defined for this interface.
        /// </summary>
        /// <returns>Namespace declaration WSDL fragment (import declaration).</returns>
        private string BuildImportDecl()
        {
            string namespaceDecl = string.Empty;
            foreach (ProcessingContext ctx in this._operationContextList)
            {
                string schemaLocation = (ContextSlt.GetContextSlt().GetConfigProperty(_SchemaUseRelativePathName) == "true") ? string.Empty : this._currentService.ServiceBuildPath + "/";
                schemaLocation += ctx.Operation.BaseFileName + ".xsd";
                namespaceDecl += "\n         <xs:import schemaLocation=\"" + schemaLocation + "\" namespace=\"" +
                                 this._currentService.GetFQN("SOAPOperation", ctx.Operation.Name, -1) + "\"/>";
            }
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.APIProcessor.BuildImportDecl>> Collected: " + namespaceDecl);
            return namespaceDecl;
        }

        /// <summary>
        /// Helper method, which creates a set of message declarations for each business message in each operation defined for this interface.
        /// </summary>
        /// <returns>List of message declarations.</returns>
        private string BuildMessageDecl()
        {
            string messageDecl = string.Empty;
            string nsTokenTag = ContextSlt.GetContextSlt().GetConfigProperty(_NSTokenTag);
            foreach (ProcessingContext ctx in this._operationContextList)
            {
                foreach (Capability message in ctx.Messages)
                {
                    string elementName = Conversions.ToPascalCase(ctx.Operation.AssignedRole) + Conversions.ToPascalCase(message.AssignedRole);
                    messageDecl += "   <message name=\"" + message.Name + "\">\n      <part name=\"part1\" element=\"" +
                                    ctx.Operation.CapabilityClass.GetTag(nsTokenTag) + ":" + elementName + "\"/>\n   </message>\n\n";
                }
            }
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.APIProcessor.BuildMessageDecl >> Collected: " + messageDecl);
            return messageDecl;
        }

        /// <summary>
        /// Helper method, which creates a set of namespace declarations for each operation defined for this interface.
        /// </summary>
        /// <returns>List of namespace declarations.</returns>
        private string BuildNamespaceDecl()
        {
            string namespaceDecl = string.Empty;
            string nsTokenTag = ContextSlt.GetContextSlt().GetConfigProperty(_NSTokenTag);
            foreach (ProcessingContext ctx in this._operationContextList)
            {
                namespaceDecl += "\n   xmlns:" + ctx.Operation.CapabilityClass.GetTag(nsTokenTag) + "=\"" + this._currentService.GetFQN("SOAPOperation", ctx.Operation.Name, -1) + "\"";
            }
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.APIProcessor.BuildNamespaceDecl >> Collected: " + namespaceDecl);
            return namespaceDecl;
        }

        /// <summary>
        /// Helper method, which creates a set of operation declarations for each operation defined for this interface.
        /// </summary>
        /// <returns>List of message declarations.</returns>
        private string BuildOperationDecl()
        {
            string operationDecl = string.Empty;
            string requestMessage = string.Empty;
            string responseMessage = string.Empty;
            foreach (ProcessingContext ctx in this._operationContextList)
            {
                operationDecl += "\n      <operation name=\"" + ctx.Operation.AssignedRole + "\">";
                requestMessage = responseMessage = string.Empty;        // Not all operations have a request and a response!
                foreach (MessageCapability message in ctx.Messages)
                {
                    if (message.Type == MessageCapability.MessageType.Request)
                    {
                        requestMessage = "\n         <input message=\"" + _localToken + ":" + message.Name + "\"/>";
                    }
                    else if (message.Type == MessageCapability.MessageType.Response)
                    {
                        responseMessage = "\n         <output message=\"" + _localToken + ":" + message.Name + "\"/>";
                    }
                    else
                    {
                        string errorMsg = "Non-conformant message found with name: " + message.Name;
                        Logger.WriteError("Plugin.Application.CapabilityModel.API.APIProcessor.BuildOperationDecl >> " + errorMsg);
                        this._panel.WriteError(this._panelIndex, errorMsg);
                    }
                }
                operationDecl += requestMessage + responseMessage + "\n      </operation>";  // To make sure the order is always request before response.
            }
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.APIProcessor.BuildOperationDecl >> Collected: " + operationDecl);
            return operationDecl;
        }

        /// <summary>
        /// Helper method that builds a WSDL Service Declaration section (in this case, only JMS topics are supported).
        /// </summary>
        /// <returns>Service declaration string</returns>
        private string BuildServiceDecl(InterfaceCapability itf)
        {
            string jmsToken = ContextSlt.GetContextSlt().GetConfigProperty(_JMSNamespaceToken);
            string serviceDecl = "\n\n   <service name=\"" + this._currentService.Name + "\">";
            serviceDecl += "\n     <port name=\"" + itf.Name + "PortTypeJMS\" binding=\"" + _localToken + ":" + itf.Name + "PortTypeJMSBinding\">";
            serviceDecl += "\n        <" + jmsToken + ":connectionFactory>TopicConnectionFactory</" + jmsToken + ":connectionFactory>";
            serviceDecl += "\n        <" + jmsToken + ":targetAddress destination=\"topic\">replace with actual topic name</" + jmsToken + ":targetAddress>";
            serviceDecl += "\n     </port>";
            serviceDecl += "\n   </service>";
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.APIProcessor.BuildServiceDecl >> Collected: " + serviceDecl);
            return serviceDecl;
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
        /// At the moment, the only supported standard is WSDL. Type must be one of Abstract, Concrete or Topic.
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

            // At the moment, we only support WSDL-type interface contracts. All other prefixed will be simply ignored.
            if (contractStandard != "wsdl")
            {
                Logger.WriteWarning("Unsupported contract type: '" + contractStandard + "' specified, ignored!");
                this._panel.WriteWarning(this._panelIndex, "Unsupported contract type: '" + contractStandard + "' specified, ignored!");
            }

            if (contractTag != string.Empty)
            {
                switch (contractTag)
                {
                    case "concrete":
                        contractType = ContractType.Concrete;
                        break;

                    case "topic":
                        contractType = ContractType.Topic;
                        break;

                    default:
                        contractType = ContractType.Abstract;
                        break;
                }
            }
            else contractType = ContractType.Abstract;
            return contractType;
        }
    }
}
