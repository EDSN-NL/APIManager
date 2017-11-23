using System;
using System.IO;
using System.Collections.Generic;
using Framework.Context;
using Framework.Logging;

namespace Plugin.Application.CapabilityModel.ASCIIDoc
{
    /// <summary>
    /// A documentation context can be considered a 'chapter' in a document. Each context contains class- as well as classifier documentation.
    /// An operation context is formatted as a list of message documentation, followed by operation-specific classes, followed by operation-specific
    /// classifiers.
    /// </summary>
    internal sealed class OperationDocContext: DocContext
    {
        // Configuration properties used by this module:
        const string _RequestMessageRoleName    = "RequestMessageRoleName";
        const string _ResponseMessageRoleName   = "ResponseMessageRoleName";
        const string _RequestMessageSuffix      = "RequestMessageSuffix";
        const string _ResponseMessageSuffix     = "ResponseMessageSuffix";

        internal enum Chapter { Message, MessageClasses, CommonClasses, Unknown }

        /// <summary>
        /// Helper class that keeps track of documentation context per message. This consists of one generic node that represents the
        /// message class and a list documentation nodes for message-specific class definitions.
        /// </summary>
        private class MessageDescriptor
        {
            internal ClassDocNode MessageNode;                        // The message documentation node.
            internal SortedList<string, ClassDocNode> MessageClasses; // List of all message-specific class documentation nodes.

            internal MessageDescriptor(ClassDocNode messageNode)
            {
                this.MessageNode = messageNode;
                this.MessageClasses = new SortedList<string, ClassDocNode>();
            }
        }

        private MessageDescriptor _currentMessage;                  // We can only process one message at a time. This identifies that message.
        private SortedList<string, MessageDescriptor> _messages;    // The list of all messages.
        private SortedList<string, ClassDocNode> _commonClasses;    // The list of all operation-specific class documentation nodes.
        private Chapter _currentChapter;                            // Currently active chapter.

        /// <summary>
        /// Returns the chapter that is currently selected.
        /// </summary>
        internal Chapter ActiveChapter { get { return this._currentChapter; } }

        /// <summary>
        /// Creates a new context with specified ID, name and header text.
        /// </summary>
        /// <param name="contextID">Unique ID of this context. By definition, this must be the namespace token of the associated namespace.</param>
        /// <param name="contextName">The name of the context.</param>
        /// <param name="headerText">Header text for the context.</param>
        /// <param name="level">Defines the indentation level of this context within the complete document.</param>
        internal OperationDocContext(string contextID, string contextName, string headerText, int level): base(contextID, contextName, headerText, level)
        {
            this._messages = new SortedList<string, MessageDescriptor>();
            this._commonClasses = new SortedList<string, ClassDocNode>();
            this._currentMessage = null;
        }

        /// <summary>
        /// Implementation of abstract 'Save' method, which writes the ASCIIDoc-formatted contents of the Operation documentation context to the
        /// specified file stream. The stream is otherwise not modified, which facilitates multiple save operations to the same stream to work
        /// Ok.
        /// </summary>
        /// <param name="stream">Stream that is going to receive contents.</param>
        internal override void Save(StreamWriter stream)
        {
            const string _MessageTemplate   = "@ASCIIDocMessageTemplate@";
            const string _ClassDocNode      = "@CLASSDOCNODE@";
            const string _ClassifierDocNode = "@CLASSIFIERDOCNODE@";

            Logger.WriteInfo("Plugin.Application.CapabilityModel.ASCIIDoc.OperationDocContext.Save >> Writing contents...");
            ContextSlt context = ContextSlt.GetContextSlt();
            string contents = context.GetResourceString(FrameworkSettings._ASCIIDocOperationTemplate);
            string indent = string.Empty;
            for (int i = 0; i < this._level; indent += "=", i++) ;
            contents = contents.Replace("@LVL@", indent);
            contents = contents.Replace("@OPERATIONANCHOR@", this._name.ToLower());
            contents = contents.Replace("@OPERATIONNAME@", this._name);
            contents = contents.Replace("@OPERATIONNOTES@", this._headerText);

            // Build documentation structures for all messages...
            foreach(MessageDescriptor msgDescriptor in this._messages.Values)
            {
                contents = contents.Replace(_MessageTemplate, BuildMessage(msgDescriptor.MessageNode, msgDescriptor.MessageClasses) + _MessageTemplate);
            }
            contents = contents.Replace(_MessageTemplate, string.Empty);
            indent += "=";  // Operation-global classes and data types must be one level down.

            // Build documentation for operation-global classes...
            string cmnClassContents = string.Empty;
            if (this._commonClasses.Count > 0)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.ASCIIDoc.OperationDocContext.GetFormattedContents >> We have operation specific classes...");
                cmnClassContents = context.GetResourceString(FrameworkSettings._ASCIIDocCommonClasses);
                cmnClassContents = cmnClassContents.Replace("@LVL@", indent);
                cmnClassContents = cmnClassContents.Replace("@TITLE@", "Operation-specific Classes");
                bool firstOne = true;
                foreach (ClassDocNode node in this._commonClasses.Values)
                {
                    if (this._xrefList.ContainsKey(node.Name)) node.AddXREF(this._xrefList[node.Name]);
                    cmnClassContents = cmnClassContents.Replace(_ClassDocNode, firstOne? node.ASCIIDoc + _ClassDocNode : 
                                                                                         Environment.NewLine + node.ASCIIDoc + _ClassDocNode);
                    firstOne = false;
                }
                cmnClassContents = cmnClassContents.Replace(_ClassDocNode, string.Empty);
            }
            contents = contents.Replace("@ASCIIDocCommonClasses@", cmnClassContents);

            // Build documentation for operation-global (common) classifiers...
            string cmnClassifiers = string.Empty;
            if (this._classifiers.Count > 0)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.ASCIIDoc.OperationDocContext.GetFormattedContents >> We have operation specific classifiers...");
                cmnClassifiers = context.GetResourceString(FrameworkSettings._ASCIIDocClassifiers);
                cmnClassifiers = cmnClassifiers.Replace("@LVL@", indent);
                cmnClassifiers = cmnClassifiers.Replace("@TITLE@", "Operation-specific Data Types");
                bool firstOne = true;
                foreach (ClassifierDocNode node in this._classifiers.Values)
                {
                    if (this._xrefList.ContainsKey(node.Name)) node.AddXREF(this._xrefList[node.Name]);
                    cmnClassifiers = cmnClassifiers.Replace(_ClassifierDocNode, firstOne ? node.ASCIIDoc + _ClassifierDocNode :
                                                                                           Environment.NewLine + node.ASCIIDoc + _ClassifierDocNode);
                    firstOne = false;
                }
                cmnClassifiers = cmnClassifiers.Replace(_ClassifierDocNode, string.Empty);
            }
            contents = contents.Replace("@ASCIIDocClassifiers@", cmnClassifiers);
            Logger.WriteInfo("Plugin.Application.CapabilityModel.ASCIIDoc.OperationDocContext.GetFormattedContents >> Saving...");

            // Finally, write the formatted contents to the specified stream...
            stream.Write(contents);
        }

        /// <summary>
        /// This method must be called before we're adding attributes, associations and classifier definitions to the context. It assures that
        /// (1) the selected item exists and is ready to receive data and (b) that the correct item is in scope. This is important since we're
        /// recursively creating class definitions and the scope thus changes with each association that is being parsed.
        /// </summary>
        /// <param name="chapter">Identifies the item that we want to be in focus.</param>
        /// <param name="className">Item key.</param>
        /// <param name="notes">Only for first-time initialization, the item body text (inserted after the header).</param>
        /// <param name="isEmpty">When set to 'true', this class is a placeholder only and has no attributes. We need to suppress some output!</param>
        internal void SwitchClass(Chapter chapter, string className, string notes, bool isEmpty)
        {
            switch (chapter)
            {
                case Chapter.Message:
                    // This identifies the message as a whole.
                    if (!this._messages.ContainsKey(className))
                    {
                        this._currentMessage = new MessageDescriptor(new ClassDocNode(this._contextID, className, notes, this._level, isEmpty));
                        this._messages.Add(className, this._currentMessage);
                    }
                    else this._currentMessage = this._messages[className];
                    this._currentNode = this._currentMessage.MessageNode;
                    this._currentChapter = chapter;
                    break;

                case Chapter.MessageClasses:
                    // These are classes that are specific to a single message.
                    if (this._currentMessage != null)
                    {
                        if (!this._currentMessage.MessageClasses.ContainsKey(className))
                        {
                            this._currentNode = new ClassDocNode(this._contextID, className, notes, this._level + 3, isEmpty);
                            this._currentMessage.MessageClasses.Add(className, this._currentNode);
                        }
                        else this._currentNode = this._currentMessage.MessageClasses[className];
                    }
                    this._currentChapter = chapter;
                    break;

                case Chapter.CommonClasses:
                    // These are classes that are common for all messages within the same operation.
                    if (!this._commonClasses.ContainsKey(className))
                    {
                        this._currentNode = new ClassDocNode(this._contextID, className, notes, this._level + 2, isEmpty);
                        this._commonClasses.Add(className, this._currentNode);
                    }
                    else this._currentNode = this._commonClasses[className];
                    this._currentChapter = chapter;
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// For Operations, classifiers are defined at relative level 2...
        /// </summary>
        /// <returns>Current level + 2.</returns>
        protected override int GetClassifierLevel()
        {
            return this._level + 2;
        }

        /// <summary>
        /// Local helper function that creates a complete documentation structure for a message.
        /// </summary>
        /// <param name="msgNode">The message node (typically, Request or Response).</param>
        /// <param name="classList">The list of message-specific classes.</param>
        /// <returns>ASCIIDoc formatted contents.</returns>
        private string BuildMessage(ClassDocNode msgNode, SortedList<string, ClassDocNode> classList)
        {
            const string _ClassDocNode = "@CLASSDOCNODE@";

            ContextSlt context = ContextSlt.GetContextSlt();
            string contents = context.GetResourceString(FrameworkSettings._ASCIIDocMessageTemplate);
            string responseRole = context.GetConfigProperty(_ResponseMessageRoleName);
            string indent = string.Empty;
            for (int i = 0; i < this._level + 1; indent += "=", i++) ;

            // Typically, the message name is simply the name of the message node. However, if we're dealing with the generic 'Request' or 'Response'
            // message, we replace this by the more meaningful name of 'operation'Request or 'operation'Response...
            string msgName = msgNode.Name;
            if (msgNode.Name == context.GetConfigProperty(_RequestMessageRoleName)) 
            {
                msgName = this._name + context.GetConfigProperty(_RequestMessageSuffix);
            }
            else if (msgNode.Name == context.GetConfigProperty(_ResponseMessageRoleName))
            {
                msgName = this._name + context.GetConfigProperty(_ResponseMessageSuffix);
            }

            contents = contents.Replace("@LVL@", indent);
            contents = contents.Replace("@MESSAGEANCHOR@", msgName.ToLower());
            contents = contents.Replace("@MESSAGENAME@", msgName);
            contents = contents.Replace("@MESSAGENOTES@", msgNode.Notes);

            string msgClassContents = string.Empty;
            if (classList.Count > 1 || (classList.Count == 1 && !classList.Values[0].Empty))
            {
                // Build documentation for message-specific classes...
                msgClassContents = context.GetResourceString(FrameworkSettings._ASCIIDocMessageClasses);
                indent += "=";
                msgClassContents = msgClassContents.Replace("@LVL@", indent);
                bool firstOne = true;
                foreach (ClassDocNode node in classList.Values)
                {
                    if (this._xrefList.ContainsKey(node.Name)) node.AddXREF(this._xrefList[node.Name]);
                    msgClassContents = msgClassContents.Replace(_ClassDocNode, firstOne ? node.ASCIIDoc + _ClassDocNode :
                                                                                          Environment.NewLine + node.ASCIIDoc + _ClassDocNode);
                    firstOne = false;
                }
                msgClassContents = msgClassContents.Replace(_ClassDocNode, string.Empty);
            }
            contents = contents.Replace("@ASCIIDocMessageClasses@", msgClassContents);
            return contents;
        }
    }
}
