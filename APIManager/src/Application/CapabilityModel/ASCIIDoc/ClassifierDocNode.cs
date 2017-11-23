using System;
using System.Collections.Generic;
using Framework.Model;
using Framework.Context;
using Framework.Util.SchemaManagement;
using Framework.Logging;

namespace Plugin.Application.CapabilityModel.ASCIIDoc
{
    /// <summary>
    /// Represents documentation for a single classifier.
    /// </summary>
    internal sealed class ClassifierDocNode
    {
        // Configuration properties used by this module:
        private const string _ASCIIDocTypeRow           = "ASCIIDocTypeRow";
        private const string _ASCIIDocSupplementaryRow  = "ASCIIDocSupplementaryRow";
        private const string _ASCIIDocEnumValue         = "ASCIIDocEnumValue";
        private const string _SupplementaryAttStereotype = "SupplementaryAttStereotype";

        private string _ASCIIDoc;
        private string _name;
        private string _contextID;

        /// <summary>
        /// Retrieve the generated classifier documentation as an ASCIIDoc formatted string.
        /// </summary>
        internal string ASCIIDoc { get { return GetASCIIDoc(); } }

        /// <summary>
        /// Returns the classifier name associated with this node.
        /// </summary>
        internal string Name { get { return this._name; } }

        /// <summary>
        /// Create a new ASCIIDoc documentation entry for a classifier. Each instantiated ClassifierDocNode represents a single classifier.
        /// The contextID is used to create context-bound anchors for the classifier, which facilitates cross-referencing between different contexts.
        /// Anchors are defined as 'id_name' (e.g. 'cmn_countertype').
        /// </summary>
        /// <param name="contextID">Context identifier, by definition, this is the namespace token of the namespace associated with the context.</param>
        /// <param name="classifier">Classifier definition.</param>
        /// <param name="facetList">Optional list of facets for this classifier.</param>
        /// <param name="attribList">Optional list of supplementary attributes for this classifier.</param>
        /// <param name="primName">Name of the primary type associated with the classifier.</param>
        /// <param name="qualifiedName">The classifier name that we must use in the documentation (if NULL/empty, use classifier.Name instead).</param>
        /// <param name="level">Defines the indentation level (chapter numbers) of this node within the complete document.</param>
        internal ClassifierDocNode(string contextID, MEDataType classifier, List<Facet> facetList, List<SupplementaryAttribute> attribList, string primName, string qualifiedName, int level)
        {
            const string TypeToken      = "@TYPES@";
            const string FacetToken     = "@FACET@";
            const string TypeNotesToken = "@TYPENOTES@";

            ContextSlt context = ContextSlt.GetContextSlt();
            string indent = string.Empty;
            for (int i = 0; i < level; indent += "=", i++) ;

            this._name = !string.IsNullOrEmpty(qualifiedName) ? qualifiedName : classifier.Name;
            this._contextID = contextID;

            if (classifier.Type == ModelElementType.Enumeration)
            {
                // For enumerations, we ignore Facets and Supplementaries...
                Logger.WriteInfo("Plugin.Application.CapabilityModel.ASCIIDoc.ClassifierDocNode >> Creating enumerated-type classifier '" +
                                 contextID + ":" + classifier.Name + "'...");
                this._ASCIIDoc = context.GetResourceString(FrameworkSettings._ASCIIDocEnumTemplate);
                this._ASCIIDoc = this._ASCIIDoc.Replace("@ENUMNAME@", this._name);
                this._ASCIIDoc = this._ASCIIDoc.Replace("@ENUMANCHOR@", contextID + "_" + this._name.ToLower());
                this._ASCIIDoc = this._ASCIIDoc.Replace("@LVL@", indent);
                this._ASCIIDoc = this._ASCIIDoc.Replace("@NOTES@", classifier.Annotation);
                BuildEnumValues(classifier);
            }
            else
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.ASCIIDoc.ClassifierDocNode >> Creating classifier '" + 
                                 contextID + ":" + classifier.Name + "' of type '" + primName + "'...");
                this._ASCIIDoc = context.GetResourceString(FrameworkSettings._ASCIIDocTypeTemplate);
                this._ASCIIDoc = this._ASCIIDoc.Replace("@CLASSIFIERNAME@", this._name);
                this._ASCIIDoc = this._ASCIIDoc.Replace("@CLASSIFIERANCHOR@", contextID + "_" + this._name.ToLower());
                this._ASCIIDoc = this._ASCIIDoc.Replace("@LVL@", indent);
                this._ASCIIDoc = this._ASCIIDoc.Replace("@NOTES@", classifier.Annotation);

                string type = context.GetConfigProperty(_ASCIIDocTypeRow);

                // Facets are used to annotate the classifier type. We only support a small sub-set here, all others are ignored.
                if (facetList != null && facetList.Count > 0)
                {
                    primName = TypeReplacement(primName, facetList);

                    Tuple<string, string> facetInfo = StringRange(facetList);
                    if (facetInfo.Item1 != string.Empty)
                    {
                        type = type.Replace(FacetToken, facetInfo.Item1);
                        type = type.Replace(TypeNotesToken, facetInfo.Item2);
                    }
                    else
                    {
                        facetInfo = NumericRange(facetList);
                        if (facetInfo.Item1 != string.Empty)
                        {
                            type = type.Replace(FacetToken, facetInfo.Item1);
                            type = type.Replace(TypeNotesToken, facetInfo.Item2);
                        }
                        else
                        {
                            type = type.Replace(FacetToken, string.Empty);
                            type = type.Replace(TypeNotesToken, string.Empty);
                        }
                    }
                    type = type.Replace("@PRIMNAME@", primName);
                    this._ASCIIDoc = this._ASCIIDoc.Replace(TypeToken, type + TypeToken);
                }
                else
                {
                    type = type.Replace("@PRIMNAME@", primName);
                    type = type.Replace(FacetToken, string.Empty);
                    type = type.Replace(TypeNotesToken, string.Empty);
                    this._ASCIIDoc = this._ASCIIDoc.Replace(TypeToken, type + TypeToken);
                }

                if (attribList != null && attribList.Count > 0) BuildSupplementaries(attribList);
                else this._ASCIIDoc = this._ASCIIDoc.Replace(TypeToken, string.Empty);
            }
            Logger.WriteInfo("Plugin.Application.CapabilityModel.ASCIIDoc.ClassifierDocNode >> Created:" + Environment.NewLine + this._ASCIIDoc);
        }

        /// <summary>
        /// This method reads the list of references from the provided CrossReference object and merges this into the class documentation.
        /// </summary>
        /// <param name="references">List of cross-references for this class document node.</param>
        internal void AddXREF(CrossReference references)
        {
            this._ASCIIDoc = this._ASCIIDoc.Replace("@XREF@", references.ReferenceText);
            Logger.WriteInfo("Plugin.Application.CapabilityModel.ASCIIDoc.ClassifierDocNode.AddXREF >> Added list: '" + references.ReferenceText + "'.");
        }

        /// <summary>
        /// Creates a list of all enumerated-type values.
        /// </summary>
        /// <param name="classifier">Classifier of type enumeration.</param>
        private void BuildEnumValues(MEDataType classifier)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string supplementary = context.GetConfigProperty(_SupplementaryAttStereotype);
            string lineTemplate = context.GetConfigProperty(_ASCIIDocEnumValue);
            bool firstOne = true;

            foreach (MEAttribute attrib in classifier.Attributes)
            {
                // We take all attributes that are NOT supplementaries (an enumeration can have 'enum' and/or 'facet' stereotypes,
                // but sometimes has no stereotype at all.
                if (!attrib.HasStereotype(supplementary))
                {
                    string line = lineTemplate;
                    line = line.Replace("@ENUMNAME@", attrib.Name);
                    line = line.Replace("@TYPENOTES@", attrib.Annotation);
                    this._ASCIIDoc = this._ASCIIDoc.Replace("@TYPES@", firstOne ? line + "@TYPES@" : Environment.NewLine + line + "@TYPES@");
                    firstOne = false;
                }
            }
            this._ASCIIDoc = this._ASCIIDoc.Replace("@TYPES@", string.Empty);
        }

        /// <summary>
        /// Create entries for each Supplementary attribute that is defined for this Classifier.
        /// </summary>
        /// <param name="attribList">List of supplementary attributes.</param>
        private void BuildSupplementaries(List<SupplementaryAttribute> attribList)
        {
            const string TypeToken = "@TYPES@";

            bool firstOne = true;
            this._ASCIIDoc = this._ASCIIDoc.Replace(TypeToken, Environment.NewLine + TypeToken);

            foreach (SupplementaryAttribute attrib in attribList)
            {
                // If the classifier is not a primitive type, we consider it to be an enumeration and translate into anchor...
                string classifier = IsPrimClassifier(attrib.Classifier) ? attrib.Classifier : 
                                                                          ("<<" + attrib.NSToken.ToLower() + "_" + attrib.Classifier.ToLower() + 
                                                                           "," + attrib.Classifier + ">>");
                string notes = string.Empty;
                if (attrib.DefaultValue != string.Empty) notes = "Default value: " + attrib.DefaultValue;
                else if (attrib.FixedValue != string.Empty) notes = "Read-only value: " + attrib.FixedValue;

                string suppDoc = ContextSlt.GetContextSlt().GetConfigProperty(_ASCIIDocSupplementaryRow);
                suppDoc = suppDoc.Replace("@SUPNAME@", "Attribute: " + attrib.Name);
                suppDoc = suppDoc.Replace("@SUPTYPE@", classifier);
                suppDoc = suppDoc.Replace("@TYPENOTES@", notes);
                this._ASCIIDoc = this._ASCIIDoc.Replace(TypeToken, firstOne ? suppDoc + TypeToken : Environment.NewLine + suppDoc + TypeToken);
                firstOne = false;
            }
            this._ASCIIDoc = this._ASCIIDoc.Replace(TypeToken, string.Empty);
        }

        /// <summary>
        /// Is called to close the node for further updates and return the collected documentation. Tokens that have not been resolved are
        /// replaced by empty strings.
        /// </summary>
        /// <returns>ASCIIDoc formatted documentation for this Classifier.</returns>
        private string GetASCIIDoc()
        {
            this._ASCIIDoc = this._ASCIIDoc.Replace("@XREF@", string.Empty);
            return this._ASCIIDoc;
        }

        /// <summary>
        /// Simple function that checks whether the provided classifier is a primitive type or not. If not, it's probably an enumeration, since
        /// this is the only allowed alternative!
        /// </summary>
        /// <param name="classifier">Name of classifier to check.</param>
        /// <returns>True if name appears in the supported list of primitive types.</returns>
        private bool IsPrimClassifier(string classifier)
        {
            string[] lookupTable = {"string","integer","anytype","binary","boolean","date","datetime",
                                    "decimal","duration","normalizedstring","time","union"};
            for (int i = 0; i < lookupTable.Length; i++)
                if (string.Compare(classifier, lookupTable[i], StringComparison.OrdinalIgnoreCase) == 0) return true;
            return false;
        }

        /// <summary>
        /// Helper function that extracts string length facets from the facet list and creates the appropriate facet indicator as well as notes.
        /// Possible result are:
        /// TD: Total digits [count].
        /// FD: Fraction digits [.count].
        /// MINI: Minimum, inclusive value [>=min].
        /// MAXI: Maximum, inclusive value [<=max].
        /// MINE: Minimum, exclusive value [>min].
        /// MAXE: Maximum, exclusive value [>max]
        /// </summary>
        /// <param name="facetList">List of all specified facets.</param>
        /// <returns>Tuple containing result indicator and accompanying notes.</returns>
        private Tuple<string, string> NumericRange(List<Facet> facetList)
        {
            string result = string.Empty;
            string notes = string.Empty;
            string totalDigits = string.Empty;
            string fractionDigits = string.Empty;
            string minI = string.Empty;
            string maxI = string.Empty;
            string minE = string.Empty;
            string maxE = string.Empty;

            foreach (Facet facet in facetList)
            {
                switch (facet.FacetToken)
                {
                    case "TD":
                        totalDigits = facet.FacetValue;
                        break;

                    case "FD":
                        // An FD of '0' means that we translate decimal to integer. Please ignore here.
                        if (facet.FacetValue != "0") fractionDigits = facet.FacetValue;
                        break;

                    case "MINI":
                        minI = facet.FacetValue;
                        break;

                    case "MAXI":
                        maxI = facet.FacetValue;
                        break;

                    case "MINE":
                        minE = facet.FacetValue;
                        break;

                    case "MAXE":
                        maxE = facet.FacetValue;
                        break;

                    default:
                        break;
                }
            }

            if (totalDigits != string.Empty)
            {
                if (fractionDigits == string.Empty || fractionDigits == "0")
                {
                    result = "[" + totalDigits + "]";
                    notes = "Integer";
                }
                else
                {
                    result = "[" + totalDigits + "." + fractionDigits + "]";
                    notes = "Decimal";
                }
            }
            else
            {
                if (fractionDigits != string.Empty && fractionDigits != "0")
                {
                    result = "[n." + fractionDigits + "]";
                    notes = "Decimal";
                }
            }

            if (minI != string.Empty || minE != string.Empty)
            {
                string lower = minI != string.Empty ? minI : minE;
                string sep = minI != string.Empty ? " <= " : " < ";
                if (maxI != string.Empty || maxE != string.Empty)
                {
                    string upper = maxI != string.Empty ? maxI : maxE;
                    string sep2 = maxI != string.Empty ? " >= " : " > ";
                    result += ", " + lower + sep + "val" + sep2 + upper;
                    notes += ", Value must be between specified boundaries.";
                }
                else // maxI == empty && maxE == empty
                {
                    result += ", " + lower + sep + "val";
                    notes += ", Value must be greater/equal lower boundary.";
                }
            }
            else // minI == empty && maxE == empty
            {
                if (maxI != string.Empty || maxE != string.Empty)
                {
                    string upper = maxI != string.Empty ? maxI : maxE;
                    string sep = maxI != string.Empty ? " >= " : " > ";
                    result += ", val" + sep + upper;
                    notes += ", Value must be smaller/equal upper boundary.";
                }
            }
            return new Tuple<string, string>(result, notes);
        }

        /// <summary>
        /// Helper function that extracts string length facets from the facet list and creates the appropriate facet indicator as well as notes.
        /// Possible result are:
        /// LEN: Exact length [length].
        /// MINL: Minimum length string [min..].
        /// MAXL: Maximum length string [0..max].
        /// MINL..MAXL: Bounded string [min..max].
        /// </summary>
        /// <param name="facetList">List of all specified facets.</param>
        /// <returns>Tuple containing result indicator and accompanying notes.</returns>
        private Tuple<string, string> StringRange(List<Facet> facetList)
        {
            string result = string.Empty;
            string notes = string.Empty;
            string minLength = string.Empty;
            string maxLength = string.Empty;
            foreach (Facet facet in facetList)
            {
                switch (facet.FacetToken)
                {
                    case "LEN":
                        result += "[" + facet.FacetValue + "]";
                        notes += "String must have exact length.";
                        return new Tuple<string, string>(result, notes);

                    case "MINL":
                        minLength = facet.FacetValue;
                        break;

                    case "MAXL":
                        maxLength = facet.FacetValue;
                        break;

                    default:
                        break;
                }
            }
            if (minLength != string.Empty)
            {
                if (maxLength != string.Empty)
                {
                    result = "[" + minLength + ".." + maxLength + "]";
                    notes = "Bounded string.";
                }
                else
                {
                    result = "[" + minLength + "..]";
                    notes = "Minimum length string";
                }
            }
            else
            {
                if (maxLength != string.Empty)
                {
                    result = "[0.." + maxLength + "]";
                    notes = "Maximum length string.";
                }
            }
            return new Tuple<string, string>(result, notes);
        }

        /// <summary>
        /// Simple function that checks the list of Facets for specific Facets that indicate that the original primitive type must be replaced
        /// by an alternative one. At the moment, the only Facet checked is 'FractionDigits' which, when set to 0, indicates that we interpret 
        /// the type as an Integer (instead of a Decimal).
        /// </summary>
        /// <param name="primName">The original primitive type.</param>
        /// <param name="facetList">List of Facets to check.</param>
        /// <returns></returns>
        private string TypeReplacement(string primName, List<Facet> facetList)
        {
            string newType = primName;
            foreach (Facet facet in facetList)
            {
                if (facet.FacetToken == "FD" && facet.FacetValue == "0") newType = "Integer";
                break;
            }
            return newType;
        }
    }
}
