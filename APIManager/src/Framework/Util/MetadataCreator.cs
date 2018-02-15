using System.Collections.Generic;
using Framework.Logging;
using Framework.Context;
using Framework.Model;

namespace Framework.Util
{
    /// <summary>
    /// A class that aids in the creation of UN/CEFACT Dictionary Entry Names (DEN) and Unique ID's.
    /// </summary>
    internal class MetadataCreator
    {
        // Configuration properties used by this module.
        private const string _NamespacePrefix   = "NamespacePrefix";
        private const string _UniqueIDPrefix    = "UniqueIDPrefix";

        /// <summary>
        /// Creates a Dictionary Entry Name (DEN) for Classes according to UN/CEFACT rules (as far as these can be applied automatically). 
        /// This implies that we take a PascalName and break this in separate tokens (each word in the name becomes a token). These are 
        /// then combined again by inserting a "_ " sequence.
        /// Example:
        /// "LUCIDossierType" results in DEN: "LUCI_ Dossier".
        /// </summary>
        /// <param name="className"> The name of the class.</param>
        /// <returns>DEN name for the class.</returns>
        internal string MakeDEN (string className)
        {
            Logger.WriteInfo("Framework.Util.MetadataCreator.makeDEN (class) >> Translating '" + className + "'...");

            string DEN = string.Empty;
            List<string> tokenList = Tokenize(className);
            PruneClass(ref tokenList);

            for (int i = 0; i < tokenList.Count; i++)
            {
                DEN += (i < tokenList.Count - 1) ? tokenList[i] + "_ " : tokenList[i];
            }

            Logger.WriteInfo("Framework.Util.MetadataCreator.makeDEN >> Constructed DEN: '" + DEN + "'.");
            return DEN;
        }

        /// <summary>
        /// Creates a Dictionary Entry Name (DEN) according to UN/CEFACT rules (as far as these can be applied automatically). This implies that
        /// we take a PascalName and break this in separate tokens (each word in the name becomes a token). These are then combined again by
        /// inserting a "_ " sequence. In case of attributes, we MUST also have a classifier and the DEN is constructed from the class name,
        /// attribute name and classifier name, each group separated by ". ". 
        /// Example:
        /// "LUCIDossierType.ProcessCorrelationID: IdentifierType" results in DEN: "LUCI_ Dossier. Process Correlation. Identifier".
        /// </summary>
        /// <param name="className"> The name of the class.</param>
        /// <param name="attName">Name of attribute to be converted.</param>
        /// <param name="classifierName">Name of attribute classifier, might be absent in case of Enumerations!</param>
        /// <returns></returns>
        internal string  MakeDEN (string className, string attName, string classifierName)
        {
            if (string.IsNullOrEmpty(attName)) attName = string.Empty;
            if (string.IsNullOrEmpty(classifierName)) classifierName = string.Empty;

            Logger.WriteInfo("Framework.Util.MetadataCreator.makeDEN >> Translating '" + className + ":" + attName + " " + classifierName + "'...");

            string DEN = string.Empty;
            List<string> tokenList = Tokenize(className);
            PruneClass(ref tokenList);

            for (int i=0; i<tokenList.Count; i++) DEN += (i < tokenList.Count - 1) ? tokenList[i] + "_ " : tokenList[i];

            if (attName != string.Empty)
            {
                List<string> classifierTokens = null;
                string classifierDEN = string.Empty;

                // If an attribute is specified, the classifier should be present as well. Exception is processing of Data Types and/or
                // Enumerations. In those cases, we ignore the classifier part and use the attribute name only.
                if (classifierName != string.Empty)
                {
                    // Adding classifier...
                    classifierTokens = Tokenize(classifierName);
                    PruneClassifier(ref classifierTokens);
                    for (int i = 0; i < classifierTokens.Count; i++)
                    {
                        classifierDEN += (i < classifierTokens.Count - 1) ? classifierTokens[i] + "_ " : classifierTokens[i];
                        Logger.WriteInfo("Framework.Util.MetadataCreator.makeDEN >> Created Classifier DEN: '" + classifierDEN + "'.");
                    }
                }

                // Adding attribute...
                DEN += ". ";
                tokenList = Tokenize(attName);
                PruneAttribute(ref tokenList, classifierTokens);
                for (int i = 0; i < tokenList.Count; i++) DEN += (i < tokenList.Count - 1) ? tokenList[i] + "_ " : tokenList[i];
                if (classifierDEN != string.Empty) DEN += ". " + classifierDEN;
            }
            else Logger.WriteError("Framework.Util.MetadataCreator.makeDEN >> Atrtribute missing in argument '" + className + "', illegal DEN name!");

            Logger.WriteInfo("Framework.Util.MetadataCreator.makeDEN >> Constructed DEN: '" + DEN + "'.");
            return DEN;
        }

        /// <summary>
        /// Creates a unique ID for the given element. 
        /// The ID is constructed as an URI consisting of a fixed prefix, followed by the element GUID.
        /// This assures that the ID is indeed unique, even across repositories.
        /// </summary>
        /// <param name="element">Element that must be processed.</param>
        /// <returns>Generated URI string.</returns>
        internal string MakeID(ModelElement element)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string URI = context.GetConfigProperty(_NamespacePrefix) + ":" + context.GetConfigProperty(_UniqueIDPrefix) + ":";
            string GUID = element.GlobalID.ToLower();
            if (GUID[0] == '{') GUID = GUID.Substring(1, GUID.Length - 2);
            URI += GUID;
            Logger.WriteInfo("Framework.Util.MetadataCreator.makeID >> Created unique ID: '" + URI + "'.");
            return URI;
        }

        /// <summary>
        /// Simple helper function that checks whether the provided string is all uppercase letters.
        /// </summary>
        /// <param name="input">String to be tested.</param>
        /// <returns>True when all uppercase, false otherwise.</returns>
        private bool IsUpper (string input)
        {
            for (int i=0; i<input.Length; i++)
            {
                if (char.IsLetter(input[i]) && !char.IsUpper(input[i])) return false;
            }
            return true;
        }

        /// <summary>
        /// Helper function that removes 'unwanted' tokens from an attribute name. This avoids constructs like "Measurement_ Code. Code" or
        /// "Order_ ID. Identifier".
        /// </summary>
        /// <param name="tokenList">Reference to the token list to be processed.</param>
        /// <param name="classifierTokens">Our classifier tokens</param>
        private void PruneAttribute(ref List<string> tokenList, List<string> classifierTokens)
        {
            Logger.WriteInfo("Framework.Util.MetadataCreator.pruneAttribute >> Pruning attribute token list...");
            if (classifierTokens == null) return;   // If there is no classifier, we don't prune anything!

            if (tokenList.Count > 1)
            {
                if (tokenList[tokenList.Count - 1] == classifierTokens[0])
                {
                    Logger.WriteInfo("Framework.Util.MetadataCreator.pruneAttribute >> Found and removed token '" + classifierTokens[0] + "'...");
                    tokenList.RemoveAt(tokenList.Count - 1);
                }
                else
                {
                    // Some special conditions...
                    if (tokenList[tokenList.Count - 1] == "ID" && classifierTokens[0] == "Identifier")
                    {
                        Logger.WriteInfo("Framework.Util.MetadataCreator.pruneAttribute >> Found and removed token '" + classifierTokens[0] + "'...");
                        tokenList.RemoveAt(tokenList.Count - 1);
                    }
                    else if (tokenList.Count > 2 && classifierTokens.Count > 1)
                    {
                        // Last two terms match and there is at least one token left for the attribute name!
                        if ((tokenList[tokenList.Count - 1] == classifierTokens[1]) && (tokenList[tokenList.Count - 2] == classifierTokens[0]))
                        {
                            Logger.WriteInfo("Framework.Util.MetadataCreator.pruneAttribute >> Found and removed double token '" + 
                                             classifierTokens[0] + "." + classifierTokens[1] + "'...");
                            tokenList.RemoveAt(tokenList.Count - 1);    // Remove last, reduces count.
                            tokenList.RemoveAt(tokenList.Count - 1);    // Again, remove last.
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Helper function that removes 'unwanted' tokens from a class name. Specifically, "Type" and "BaseType".
        /// We remove "BaseType" only if the original name contains more then 2 tokens. I.e. class name "BaseType" gets
        /// truncated to "Base", but class name "InvoiceBaseType" gets truncated to "Invoice".
        /// </summary>
        /// <param name="tokenList">Reference to the token list to be processed.</param>
        private void PruneClass(ref List<string> tokenList)
        {
            Logger.WriteInfo("Framework.Util.MetadataCreator.pruneClass >> Pruning class name token list...");

            if (tokenList.Count > 1)
            {
                bool typeRemoved = false;

                if (tokenList[tokenList.Count - 1] == "Type")
                {
                    Logger.WriteInfo("Framework.Util.MetadataCreator.pruneClass >> Removed 'Type' suffix.");
                    tokenList.RemoveAt(tokenList.Count - 1);
                    typeRemoved = true;
                }

                if (typeRemoved && tokenList[tokenList.Count - 1] == "Base" && tokenList.Count > 1)
                {
                    Logger.WriteInfo("Framework.Util.MetadataCreator.pruneClass >> Removed 'Base' suffix.");
                    tokenList.RemoveAt(tokenList.Count - 1);
                }
            }
        }

        /// <summary>
        /// Helper function that removed 'unwanted' tokens from a classifier name. We follow the same rules as used for classes
        /// (since a classifier can in fact be a class). This implies that we remove suffixes "Type" and "BaseType".
        /// </summary>
        /// <param name="tokenList">Reference to the token list to be processed.</param>
        private void PruneClassifier(ref List<string> tokenList)
        {
            Logger.WriteInfo("Framework.Util.MetadataCreator.pruneClassifier >> Pruning classifier name token list...");
            PruneClass(ref tokenList);
        }

        /// <summary>
        /// Helper function that breaks a given input string (Pascal case) in separate tokens. I.e. "OSHATroubleTicketType" is converted
        /// to "OSHA, Trouble, Ticket, Type".
        /// </summary>
        /// <param name="input">String to be converted.</param>
        /// <returns>List of separate tokens obtained from input string.</returns>
        private List<string> Tokenize (string input)
        {
            // Break the input string into separate tokens, each starting with an uppercase letter, followed by 0..n lowercase letters (or
            // 0..n uppercase letters)...
            var tokenList = new List<string>();
            string token = string.Empty;
            input = Conversions.ToPascalCase(input);
            Logger.WriteInfo("Framework.Util.MetadataCreator.tokenize >> Translating: '" + input + "'...");
            for (int i = 0; i < input.Length; i++)
            {
                token += input[i];
                if ((char.IsLetter(input[i]) && char.IsLower(input[i])) && (i < input.Length - 1) && (char.IsLetter(input[i+1]) && char.IsUpper(input[i+1])))
                {
                    Logger.WriteInfo("Framework.Util.MetadataCreator.tokenize >> Got token: '" + token + "'.");
                    tokenList.Add(token);
                    token = string.Empty;
                }
                else if ((char.IsLetter(input[i]) && char.IsUpper(input[i])) && i < input.Length - 1 && (char.IsLetter(input[i+1]) && char.IsLower(input[i+1])))
                {
                    // This is either the start of a token (normal case), or, if the token contains multiple uppercase characters, the
                    // change from an all upper-case word, e.g. "OSHATroubleTicket"...
                    if (token.Length > 1 && IsUpper(token))
                    {
                        // We have multiple upper-case characters. Treat this as an all-uppercase token and reset the counter since the
                        // last character tested is actually the beginning of the next token!
                        i--;
                        Logger.WriteInfo("Framework.Util.MetadataCreator.tokenize >> Got token: '" + token + "'.");
                        tokenList.Add(token.Substring(0, token.Length - 1));
                        token = string.Empty;
                    }
                }
            }
            if (token != string.Empty)
            {
                Logger.WriteInfo("Framework.Util.MetadataCreator.tokenize >> Adding final token: '" + token + "'.");
                tokenList.Add(token);
            }
            return tokenList;
        }
    }
}
