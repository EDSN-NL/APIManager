using System.Xml;
using System.Xml.Schema;

namespace Framework.Util
{
    /// <summary>
    /// A very basic helper class for the construction of Model Element Documentation entities. 
    /// Each documentation entry consists of a source indicator (URI), 
    /// the documentation itself (a string) and an optional language indicator.
    /// </summary>
    internal class MEDocumentation
    {
        private string _sourceURI;
        private string _bodyText;
        private string _languageID;

        /// <summary>
        /// Getters that return the individual components of the documentation node...
        /// </summary>
        internal string SourceURI       { get { return this._sourceURI; } }
        internal string BodyText        { get { return this._bodyText; } }
        internal string LanguageID      { get { return this._languageID; } }

        /// <summary>
        /// Create a new MEDocumentation instance with the specified parameters. If 'languageID' is omitted, it defaults to 'nl'.
        /// The languageID must adhere to BCP-47.
        /// </summary>
        /// <param name="sourceURI">Identifier for the 'type' of documentation</param>
        /// <param name="bodyText">The actual documentation text</param>
        /// <param name="languageID">Language token according to BCP-47</param>
        internal MEDocumentation(string sourceURI, string bodyText, string languageID = "nl")
        {
            this._sourceURI = sourceURI;
            this._bodyText = bodyText;
            this._languageID = languageID;
        }

        /// <summary>
        /// Returns the XML Schema representation of the documentation node. This includes the language- and source identifiers as well as the actual documentation.
        /// </summary>
        /// <returns>XMLSchemaDocumentation node</returns>
        internal XmlSchemaDocumentation GetXmlDocumentationNode()
        {
            var annotationDoc = new XmlSchemaDocumentation()
            {
                Language = this._languageID,
                Source = this._sourceURI,
                Markup = TextToNodeArray(this._bodyText)
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
