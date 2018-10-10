using System;
using System.Xml;
using System.Xml.Schema;
using System.Collections.Generic;
using Framework.Context;
using Framework.Logging;

namespace Framework.Util
{
    /// <summary>
    /// A very basic helper class for the construction of Model Element Documentation entities. 
    /// Each documentation entry consists of a source indicator (URI), the documentation itself (a string) and an optional language indicator.
    /// If we have an annotation section, we check whether this contains an 'Example' text. If found, this is extracted and stored separately.
    /// </summary>
    internal class MEDocumentation
    {
        // Indicates the type of example text found in the documentation:
        internal enum ExampleFormat { None, Simple, Constructed }

        // Configuration properties for Model Element Implementation...
        private const string _DocgenAnnotation = "DocgenAnnotation";
        private const string _DocGenExampleKey = "DocGenExampleKey";

        private string _sourceURI;
        private string _bodyText;
        private string _languageID;
        private string _example;
        private List<Tuple<string, string>> _structuredExample;
        private ExampleFormat _exampleFormat;

        /// <summary>
        /// Getters that return the individual components of the documentation node...
        /// </summary>
        internal string SourceURI                               { get { return this._sourceURI; } }
        internal string BodyText                                { get { return this._bodyText; } }
        internal string LanguageID                              { get { return this._languageID; } }
        internal ExampleFormat ExampleKind                      { get { return this._exampleFormat; } }
        internal string ExampleText                             { get { return this._example; } }
        internal List<Tuple<string,string>> StructuredExample   { get { return this._structuredExample; } }

        /// <summary>
        /// Create a new MEDocumentation instance with the specified parameters. If 'languageID' is omitted, it defaults to 'nl'.
        /// The languageID must adhere to BCP-47.
        /// If the sourceURI indicates that this is an annotation node, we attempt to extract 'Example' contents, which are stored separately.
        /// Examples come in two flavors: the first is a basic textual example section and the second is a structured example.
        /// Structured examples have format: "Example: {attributeName = "Content";.....}. If we find this, the contents are extracted
        /// and stored as a set of key/value types.
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
            this._structuredExample = new List<Tuple<string, string>>();
            this._exampleFormat = ExampleFormat.None;

            ContextSlt context = ContextSlt.GetContextSlt();
            string annotationTag = context.GetConfigProperty(_DocgenAnnotation);
            string exampleKey = context.GetConfigProperty(_DocGenExampleKey);
            string configKey = annotationTag.Substring(0, annotationTag.IndexOf(":"));
            string tagName = annotationTag.Substring(configKey.Length + 1);
            int indexOfExample = bodyText.IndexOf(exampleKey);

            // If we're dealing with an annotation section and this contains an Example, we extract this and store separately.
            if (sourceURI.Contains(tagName) && indexOfExample >= 0)
            {
                this._example = bodyText.Substring(indexOfExample + exampleKey.Length).Trim();
                this._bodyText = bodyText.Substring(0, indexOfExample);
                this._exampleFormat = ExampleFormat.Simple;

                if (this._example.StartsWith("{") && this._example.EndsWith("}"))
                {
                    // We have a structured example section, parse into KV pairs. Each pair is separated by ';' characters.
                    // Key and value are separated by '=' and may- or may not be surrounded by single- or double quote characters.
                    // If we retrieve a string that has not the correct format, we treat it as a simple example.
                    this._exampleFormat = ExampleFormat.Constructed;
                    this._example = this._example.Substring(1, this._example.Length - 2);
                    string[] kvPairs = this._example.Split(';');
                    foreach (string keyValue in kvPairs)
                    {
                        string[] kv = keyValue.Split('=');
                        if (kv.Length == 2)
                        {
                            string key = kv[0].Trim();
                            string value = kv[1].Trim();
                            if (key[0] == '"' || key[0] == '\'') key = key.Substring(1, key.Length - 2);
                            if (value[0] == '"' || value[0] == '\'') value = value.Substring(1, value.Length - 2);
                            this._structuredExample.Add(new Tuple<string, string>(key, value));
                        }
                        else
                        {
                            // Format error, consider this a simple example...
                            this._exampleFormat = ExampleFormat.Simple;
                            break;
                        }
                    }
                    if (this._exampleFormat == ExampleFormat.Constructed) this._example = string.Empty;
                }
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
            if (this._exampleFormat == ExampleFormat.Simple) bodyText += Environment.NewLine + exampleKey + this._example;
            else if (this._exampleFormat == ExampleFormat.Constructed)
            {
                bodyText += Environment.NewLine + exampleKey;
                foreach (var kv in this._structuredExample)
                {
                    bodyText += kv.Item1 + " = " + kv.Item2 + "; ";
                }
            }

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
