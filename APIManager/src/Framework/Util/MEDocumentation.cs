using System;
using System.Xml;
using System.Xml.Schema;
using Framework.Context;

namespace Framework.Util
{
    /// <summary>
    /// A very basic helper class for the construction of Model Element Documentation entities. 
    /// Each documentation entry consists of a source indicator (URI), the documentation itself (a string) and an optional language indicator.
    /// If we have an annotation section, we check whether this contains an 'Example' text. If found, this is extracted and stored separately.
    /// </summary>
    internal class MEDocumentation
    {
        // Configuration properties for Model Element Implementation...
        private const string _DocgenAnnotation = "DocgenAnnotation";
        private const string _DocGenExampleKey = "DocGenExampleKey";

        private string _sourceURI;
        private string _bodyText;
        private string _languageID;
        private string _example;

        /// <summary>
        /// Getters that return the individual components of the documentation node...
        /// </summary>
        internal string SourceURI       { get { return this._sourceURI; } }
        internal string BodyText        { get { return this._bodyText; } }
        internal string LanguageID      { get { return this._languageID; } }
        internal string ExampleText     { get { return this._example; } }

        /// <summary>
        /// Create a new MEDocumentation instance with the specified parameters. If 'languageID' is omitted, it defaults to 'nl'.
        /// The languageID must adhere to BCP-47.
        /// If the sourceURI indicates that this is an annotation node, we attempt to extract 'Example' contents, which are stored separately.
        /// </summary>
        /// <param name="sourceURI">Identifier for the 'type' of documentation</param>
        /// <param name="bodyText">The actual documentation text</param>
        /// <param name="languageID">Language token according to BCP-47</param>
        internal MEDocumentation(string sourceURI, string bodyText, string languageID = "nl")
        {
            this._sourceURI = sourceURI;
            this._bodyText = bodyText;
            this._languageID = languageID;
            this._example = string.Empty;

            ContextSlt context = ContextSlt.GetContextSlt();
            string annotationTag = context.GetConfigProperty(_DocgenAnnotation);
            string exampleKey = context.GetConfigProperty(_DocGenExampleKey);
            string configKey = annotationTag.Substring(0, annotationTag.IndexOf(":"));
            string tagName = annotationTag.Substring(configKey.Length + 1);
            int indexOfExample = bodyText.IndexOf(exampleKey);

            // If we're dealing with an annotation section and this contains an Example, we extract this and store separately.
            if (sourceURI.Contains(tagName) && indexOfExample >= 0)
            {
                this._example = bodyText.Substring(indexOfExample + exampleKey.Length);
                this._bodyText = bodyText.Substring(0, indexOfExample);
            }
        }

        /// <summary>
        /// Returns the XML Schema representation of the documentation node. This includes the language- and source identifiers as well as the actual documentation.
        /// If we have an example text, this is added to the Markup node (on a new line).
        /// </summary>
        /// <returns>XMLSchemaDocumentation node</returns>
        internal XmlSchemaDocumentation GetXmlDocumentationNode()
        {
            string exampleKey = ContextSlt.GetContextSlt().GetConfigProperty(_DocGenExampleKey);
            string bodyText = this._bodyText;
            if (this._example != string.Empty) bodyText += Environment.NewLine + exampleKey + this._example;

            var annotationDoc = new XmlSchemaDocumentation()
            {
                Language = this._languageID,
                Source = this._sourceURI,
                Markup = TextToNodeArray(bodyText)
            };
            return annotationDoc;
        }

        /// <summary>
        /// Converts a simple text string to a valid XML text node section.
        /// </summary>
        /// <param name="text">Text to be converted.</param>
        /// <returns>Converted text.</returns>
        private XmlNode[] TextToNodeArray(string text)
        {
            var doc = new XmlDocument();
            return new XmlNode[1] { doc.CreateTextNode(text) };
        }
    }
}
