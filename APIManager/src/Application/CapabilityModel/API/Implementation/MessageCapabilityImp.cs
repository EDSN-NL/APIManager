using System;
using Framework.Model;
using Framework.Logging;
using Framework.Context;
using Framework.Util;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// Message Capability represents a single message, which is part of an operation. Currently, we differentiate between Request and Response
    /// messages, but alternative types may be defined. A Message contains of a CapabilityClass, which is part of the Service Model, and a MessageAssembly,
    /// which represents the message body. The latter is part of the associated message package, which is a child of the Operation package.
    /// </summary>
    internal class MessageCapabilityImp: CapabilityImp
    {
        // All configuration properties that are used by this module:
        private const string _RequestPkgName                    = "RequestPkgName";
        private const string _RequestPkgStereotype              = "RequestPkgStereotype";
        private const string _ResponsePkgName                   = "ResponsePkgName";
        private const string _ResponsePkgStereotype             = "ResponsePkgStereotype";
        private const string _BusinessMessageClassStereotype    = "BusinessMessageClassStereotype";
        private const string _ServiceSupportModelPathName       = "ServiceSupportModelPathName";
        private const string _RequestParentClassName            = "RequestParentClassName";
        private const string _ResponseParentClassName           = "ResponseParentClassName";
        private const string _MessageAssemblyClassStereotype    = "MessageAssemblyClassStereotype";
        private const string _RequestMessageAssemblyClassName   = "RequestMessageAssemblyClassName";
        private const string _ResponseMessageAssemblyClassName  = "ResponseMessageAssemblyClassName";
        private const string _MessageAssemblyRoleName           = "MessageAssemblyRoleName";
        private const string _RequestMessageSuffix              = "RequestMessageSuffix";
        private const string _ResponseMessageSuffix             = "ResponseMessageSuffix";
        private const string _MessageScopeTag                   = "MessageScopeTag";
        private const string _DefaultMessageScope               = "DefaultMessageScope";
        private const string _SequenceKeyTag                    = "SequenceKeyTag";
        private const string _MessageBodyDefaultSequence        = "MessageBodyDefaultSequence";
        private const string _RequestMessageRoleName            = "RequestMessageRoleName";
        private const string _ResponseMessageRoleName           = "ResponseMessageRoleName";

        private OperationCapability _myOperation;               // Operation of which this is a message.
        private MEClass _msgAssembly;                           // The associated Message Assembly class (message body).
        private MessageCapability.MessageType _messageType;     // Specifies the type of message we're associated with.

        /// <summary>
        /// Returns the Operation Capability that acts as parent for this Message Capability.
        /// </summary>
        internal new OperationCapability Parent { get { return this._myOperation; } }

        /// <summary>
        /// Returns the message type.
        /// </summary>
        internal MessageCapability.MessageType Type { get { return this._messageType; } }

        /// <summary>
        /// Creates a new instance of a message capability. The constructor creates the necessary package for message storage and it creates all
        /// required classes and associations. Depending on the message type, it might create different structures.
        /// </summary>
        /// <param name="myOperation">The operation that owns this message.</param>
        /// <param name="messageName">The (unqualified) name of the message.</param>
        /// <param name="type">The message type.</param>
        internal MessageCapabilityImp(OperationCapability myOperation, string messageName, MessageCapability.MessageType type): base(myOperation.RootService)
        {
            this._myOperation = myOperation;
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            bool useHeader = context.GetBoolSetting(FrameworkSettings._SMUseMessageHeaders);
            
            try
            {
                // Create message class in same package as operation.
                MEPackage serviceModelPackage = myOperation.RootService.ModelPkg;
                MEPackage operationPackage = myOperation.OperationPackage;
                var operationEndpoint = new EndpointDescriptor(myOperation.CapabilityClass, "1", myOperation.Name, null, false);
                this._messageType = type;

                switch (type)
                {
                    case MessageCapability.MessageType.Request:
                        {
                            MEPackage msgPackage = operationPackage.CreatePackage(context.GetConfigProperty(_RequestPkgName),
                                                                                  context.GetConfigProperty(_RequestPkgStereotype));
                            this._capabilityClass = serviceModelPackage.CreateClass(messageName + context.GetConfigProperty(_RequestMessageSuffix),
                                                                                    context.GetConfigProperty(_BusinessMessageClassStereotype));
                            // Message class must inherit from generic request root if we want to use headers...
                            if (useHeader)
                            {
                                MEClass requestParent = model.FindClass(context.GetConfigProperty(_ServiceSupportModelPathName),
                                                                        context.GetConfigProperty(_RequestParentClassName));
                                var derived = new EndpointDescriptor(this._capabilityClass);
                                var parent = new EndpointDescriptor(requestParent);
                                model.CreateAssociation(derived, parent, MEAssociation.AssociationType.Generalization);
                            }
                            // Create association with our operation...
                            this._assignedRole = context.GetConfigProperty(_RequestMessageRoleName);
                            var requestEndpoint = new EndpointDescriptor(this._capabilityClass, "1", this._assignedRole, null, true);
                            model.CreateAssociation(operationEndpoint, requestEndpoint, MEAssociation.AssociationType.MessageAssociation);

                            // Create the request message assembly
                            this._msgAssembly = msgPackage.CreateClass(context.GetConfigProperty(_RequestMessageAssemblyClassName),
                                                                       context.GetConfigProperty(_MessageAssemblyClassStereotype));
                            var msgEndpoint = new EndpointDescriptor(this._capabilityClass, "1", Name, null, false);
                            var assemblyEndpoint = new EndpointDescriptor(this._msgAssembly, "1", 
                                                                          context.GetConfigProperty(_MessageAssemblyRoleName), null, true);
                            MEAssociation assoc = model.CreateAssociation(msgEndpoint, assemblyEndpoint, MEAssociation.AssociationType.MessageAssociation);
                            assoc.SetTag(context.GetConfigProperty(_SequenceKeyTag), 
                                         context.GetConfigProperty(_MessageBodyDefaultSequence), 
                                         false, MEAssociation.AssociationEnd.Association);
                            break;
                        }

                    case MessageCapability.MessageType.Response:
                        {
                            MEPackage msgPackage = operationPackage.CreatePackage(context.GetConfigProperty(_ResponsePkgName),
                                                                                  context.GetConfigProperty(_ResponsePkgStereotype));
                            this._capabilityClass = serviceModelPackage.CreateClass(messageName + context.GetConfigProperty(_ResponseMessageSuffix),
                                                                                    context.GetConfigProperty(_BusinessMessageClassStereotype));
                            // Message class must inherit from generic response root if we want to use headers...
                            if (useHeader)
                            {
                                MEClass responseParent = model.FindClass(context.GetConfigProperty(_ServiceSupportModelPathName),
                                                                         context.GetConfigProperty(_ResponseParentClassName));
                                var derived = new EndpointDescriptor(this._capabilityClass);
                                var parent = new EndpointDescriptor(responseParent);
                                model.CreateAssociation(derived, parent, MEAssociation.AssociationType.Generalization);
                            }

                            // Create association with our operation...
                            this._assignedRole = context.GetConfigProperty(_ResponseMessageRoleName);
                            var responseEndpoint = new EndpointDescriptor(this._capabilityClass, "1", this._assignedRole, null, true);
                            model.CreateAssociation(operationEndpoint, responseEndpoint, MEAssociation.AssociationType.MessageAssociation);

                            // Create the request message assembly
                            this._msgAssembly = msgPackage.CreateClass(context.GetConfigProperty(_ResponseMessageAssemblyClassName),
                                                                       context.GetConfigProperty(_MessageAssemblyClassStereotype));
                            var msgEndpoint = new EndpointDescriptor(this._capabilityClass, "1", Name, null, false);
                            // If we have a header, the body is optional, otherwise, it MUST be present...
                            var assemblyEndpoint = new EndpointDescriptor(this._msgAssembly, (useHeader) ? "0..1" : "1", 
                                                                          context.GetConfigProperty(_MessageAssemblyRoleName), null, true);
                            MEAssociation assoc = model.CreateAssociation(msgEndpoint, assemblyEndpoint, MEAssociation.AssociationType.MessageAssociation);
                            assoc.SetTag(context.GetConfigProperty(_SequenceKeyTag), 
                                         context.GetConfigProperty(_MessageBodyDefaultSequence), 
                                         false, MEAssociation.AssociationEnd.Association);
                            break;
                        }
                }
                this._capabilityClass.SetTag(context.GetConfigProperty(_MessageScopeTag), context.GetConfigProperty(_DefaultMessageScope));
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.MessageCapabilityImp (new) >> Error creating capability because: " + exc.Message);
                this._capabilityClass = null;   // Assures that instance is declared invalid.
            }
        }

        /// <summary>
        /// Generic constructor to be used for existing class models. Th constructor initialises local context and creates the subordinate message assembly.
        /// </summary>
        /// <param name="myOperation">The operation for which we create the message.</param>
        /// <param name="messageCapability">The associated message class.</param>
        internal MessageCapabilityImp(OperationCapability myOperation, MEClass messageCapability): base(myOperation.RootService)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.MessageCapabilityImp (existing) >> Creating new instance '" +
                              myOperation.Name + "." + messageCapability.Name + "'...");
            this._myOperation = myOperation;
            ContextSlt context = ContextSlt.GetContextSlt();
            this._capabilityClass = messageCapability;
            this._messageType = (messageCapability.Name.EndsWith("Request") || messageCapability.Name.EndsWith("Message")) ? MessageCapability.MessageType.Request : MessageCapability.MessageType.Response;
            this._msgAssembly = FindChildClass(context.GetConfigProperty(_MessageAssemblyClassStereotype), null);
            this._assignedRole = myOperation.FindChildClassRole(messageCapability.Name, context.GetConfigProperty(_BusinessMessageClassStereotype));
        }

        /// <summary>
        /// Overrides the default Capability.delete in order to assure that the message capabilities are properly deleted. The method removes the 
        /// message package, which in turn will remove everything in it. Subsequently, the capability class is deleted. 
        /// On return, the Capability is INVALID.
        /// </summary>
        internal override void Delete()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.MessageCapabilityImp.delete >> Deleting the message capability resources...");
            this._msgAssembly.OwningPackage.Parent.DeletePackage(this._msgAssembly.OwningPackage);
            base.Delete();
        }

        /// <summary>
        /// Returns a short textual identification of the capability type.
        /// </summary>
        /// <returns>Capability type name.</returns>
        internal override string GetCapabilityType()
        {
            return "Message Schema";
        }

        /// <summary>
        /// Creates an Interface object that matches the current Implementation.
        /// </summary>
        /// <returns>Interface object.</returns>
        internal override Capability GetInterface() { return new MessageCapability(this); }

        /// <summary>
        /// Process the capability (i.e. generate output according to provided processor.).
        /// </summary>
        /// <param name="processor">Capability processor to be used.</param>
        /// <param name="stage">The processing stage we're currently in, passed verbatim to processor.</param>
        /// <returns>True when processing can commence, false on errors.</returns>
        internal override bool HandleCapability(CapabilityProcessor processor, ProcessingStage stage)
        {
            // Since all the actual work is being performed by the processor, simply pass information onwards...
            return processor.ProcessCapability(new MessageCapability(this), stage);
        }

        /// <summary>
        /// This method is called whenever a (new) parent of the Capability has taken ownership of the Capability. If this parent is an Operation,
        /// we have to register the current instance with that Operation. In all other cases, no action is performed.
        /// Note that the Message has a 'myOperation' property, which is initialised in the constructor and is thus associated with the very first
        /// Operation that actually creates the Message. In other words: that is the ONLY Operation that the Message is aware of.
        /// </summary>
        /// <param name="parent">The parent Capability that has taken ownership of this Capability.</param>
        internal override void InitialiseParent(Capability parent)
        {
            if (parent is OperationCapability) parent.AddChild(new MessageCapability(this));
        }

        /// <summary>
        /// Overrides the 'rename' operation for message capabilities. In this case, we have to assure that the name is properly
        /// suffixed, according to the message type.
        /// </summary>
        /// <param name="newName">New name to be assigned to the message.</param>
        internal override void Rename(string newName)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string suffix = (this._messageType == MessageCapability.MessageType.Request) ? context.GetConfigProperty(_RequestMessageSuffix) : 
                                                                                           context.GetConfigProperty(_ResponseMessageSuffix);
            this._capabilityClass.Name = newName + suffix;
        }

        /// <summary>
        /// This method is used to synchronize the major version of the CodeList with its parent service in case that version has changed.
        /// If we detect a major update, the minor version is reset to '0'! 
        /// The method ONLY considers the service major version, minor version of the CodeList is independent of the Service!
        /// </summary>
        internal override void VersionSync()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            Tuple<int, int> myVersion = this._capabilityClass.Version;
            int majorVersion = this._rootService.MajorVersion;

            if (myVersion.Item1 < majorVersion)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.MessageCapabilityImp.versionSync >> Updating major version to: " + majorVersion);
                this._capabilityClass.Version = new Tuple<int, int>(majorVersion, 0);
            }
        }
    }
}
