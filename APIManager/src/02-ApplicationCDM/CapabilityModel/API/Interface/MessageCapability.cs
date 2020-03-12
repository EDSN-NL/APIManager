using Framework.Model;
using Framework.Exceptions;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// Message Capability represents a single message, which is part of an operation. Currently, we differentiate between Request and Response
    /// messages, but alternative types may be defined. A Message contains of a CapabilityClass, which is part of the Service Model, and a MessageAssembly,
    /// which represents the message body. The latter is part of the associated message package, which is a child of the Operation package.
    /// </summary>
    internal class MessageCapability: Capability
    {
        // Currently, we differentiate between Request and Response messages:
        internal enum MessageType { Request, Response }

        /// <summary>
        /// Creates a new instance of a message capability. The constructor creates the necessary package for message storage and it creates all
        /// required classes and associations. Depending on the message type, it might create different structures.
        /// </summary>
        /// <param name="myOperation">The operation that owns this message.</param>
        /// <param name="messageName">The (unqualified) name of the message.</param>
        /// <param name="type">The message type.</param>
        internal MessageCapability(OperationCapability myOperation, string messageName, MessageType type): base()
        {
            RegisterCapabilityImp(new MessageCapabilityImp(myOperation, messageName, type));
        }

        /// <summary>
        /// Generic constructor to be used for existing class models. Th constructor initialises local context and creates the subordinate message assembly.
        /// </summary>
        /// <param name="myOperation">The operation for which we create the message.</param>
        /// <param name="messageCapability">The associated message class.</param>
        internal MessageCapability(OperationCapability myOperation, MEClass messageCapability): base(messageCapability.ElementID)
        {
            if (!Valid) RegisterCapabilityImp(new MessageCapabilityImp(myOperation, messageCapability));
        }

        /// <summary>
        /// Creates a Message Capability interface object based on its MEClass object. This object MUST have been constructed earlier (e.g. on instantiating
        /// a Service hierarchy). If the implementation could not be found, a MissingImplementationException is thrown.
        /// </summary>
        /// <param name="capabilityClass">Capability class for the Message Capability.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MessageCapability(MEClass capabilityClass) : base(capabilityClass) { }


        /// <summary>
        /// Create new Message Capability Interface based on existing implementation...
        /// </summary>
        /// <param name="thisImp">Implementation to use.</param>
        internal MessageCapability(MessageCapabilityImp thisImp) : base(thisImp) { }

        /// <summary>
        /// Copy Constructor, using other interface as basis.
        /// </summary>
        /// <param name="thisCap">Interface to use as basis.</param>
        internal MessageCapability(MessageCapability thisCap) : base(thisCap) { }

        /// <summary>
        /// Returns the Operation Capability that is defined as parent for this Message Capability.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal new OperationCapability Parent
        {
            get
            {
                if (this._imp != null) return ((MessageCapabilityImp)this._imp).Parent;
                else throw new MissingImplementationException("MessageCapabilityImp");
            }
        }

        /// <summary>
        /// Returns the type of this message.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MessageType Type
        {
            get
            {
                if (this._imp != null) return ((MessageCapabilityImp)this._imp).Type;
                else throw new MissingImplementationException("MessageCapabilityImp");
            }
        }
    }
}
