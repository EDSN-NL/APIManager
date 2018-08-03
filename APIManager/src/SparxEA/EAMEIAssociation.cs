using System;
using System.Collections.Generic;
using EA;
using Framework.Context;
using Framework.Logging;
using Framework.Model;
using Framework.Util;

namespace SparxEA.Model
{
    /// <summary>
    /// Represents an association within Sparx EA. Associations are represented by components called 'Connector'.
    /// </summary>
    internal sealed class EAMEIAssociation: MEIAssociation
    {
        private EA.Connector _connector;      // EA Association representation.

        // Some configuration properties used by this module...
        private const string _MessageAssociationStereotype = "MessageAssociationStereotype";

        /// <summary>
        /// The internal constructor is called with an EA-specific association identifier.
        /// Note that connectors 'might' have a name, but this is not guaranteed.
        /// </summary>
        internal EAMEIAssociation(EAModelImplementation model, int associationID): base(model)
        {
            this._connector = model.Repository.GetConnectorByID(associationID);
            if (this._connector != null)
            {
                this._name = this._connector.Name;
                this._elementID = associationID;
                this._globalID = this._connector.ConnectorGUID;
				this._aliasName = this._connector.Alias ?? string.Empty;
            }
            else
            {
                Logger.WriteError("SparxEA.Model.EAMEIAssociation >> Failed to retrieve EA Connector with ID: " + associationID);
            }
        }

        /// <summary>
        /// The internal constructor is called with an EA-specific association identifier.
        /// Note that connectors 'might' have a name, but this is not guaranteed.
        /// </summary>
        internal EAMEIAssociation(EAModelImplementation model, string associationGUID) : base(model)
        {
            this._connector = model.Repository.GetConnectorByGuid(associationGUID);
            if (this._connector != null)
            {
                this._name = this._connector.Name;
                this._elementID = this._connector.ConnectorID;
                this._globalID = associationGUID;
                this._aliasName = this._connector.Alias ?? string.Empty;
            }
            else
            {
                Logger.WriteError("SparxEA.Model.EAMEIAssociation >> Failed to retrieve EA Connector with GUID: " + associationGUID);
            }
        }

        /// <summary>
        /// Constructor that creates a new implementation instance based on a provided EA connector instance.
        /// </summary>
        /// <param name="model">The associated model implementation.</param>
        /// <param name="connector">The EA Connector on which the implementation is based.</param>
        internal EAMEIAssociation(EAModelImplementation model, EA.Connector connector) : base(model)
        {
            Logger.WriteInfo("SparxEA.Model.EAMEIAssociation >> Creating implementation based on: " + connector.Name);

            this._connector = connector;
            if (this._connector != null)
            {
                this._name = this._connector.Name;
                this._elementID = connector.ConnectorID;
                this._globalID = this._connector.ConnectorGUID;
				this._aliasName = this._connector.Alias ?? string.Empty;
            }
            else
            {
                Logger.WriteError("SparxEA.Model.EAMEIAssociation >>  Constructor based on empty element!");
            }
        }

        /// <summary>
        /// Adds the specified stereotype to the specified endpoint of the association.
        /// </summary>
        /// <param name="stereotype">Stereotype to add.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal override void AddStereotype(string stereotype, MEAssociation.AssociationEnd endpoint)
        {
            if (!string.IsNullOrEmpty(stereotype))
            {
                string stereoTypes;
                string checkType = (stereotype.Contains("::")) ? stereotype.Substring(stereotype.IndexOf("::") + 2) : stereotype;
                switch (endpoint)
                {
                    case MEAssociation.AssociationEnd.Source:
                        stereoTypes = this._connector.ClientEnd.StereotypeEx;
                        if (!stereoTypes.Contains(checkType))
                        {
                            stereoTypes += (stereoTypes.Length > 0) ? "," + stereotype : stereotype;
                            this._connector.ClientEnd.StereotypeEx = stereoTypes;
                            this._connector.ClientEnd.Update();
                        }
                        break;

                    case MEAssociation.AssociationEnd.Destination:
                        stereoTypes = this._connector.SupplierEnd.StereotypeEx;
                        if (!stereoTypes.Contains(checkType))
                        {
                            stereoTypes += (stereoTypes.Length > 0) ? "," + stereotype : stereotype;
                            this._connector.SupplierEnd.StereotypeEx = stereoTypes;
                            this._connector.SupplierEnd.Update();
                        }
                        break;

                    default:
                        stereoTypes = this._connector.StereotypeEx;
                        if (!stereoTypes.Contains(checkType))
                        {
                            stereoTypes += (stereoTypes.Length > 0) ? "," + stereotype : stereotype;
                            this._connector.StereotypeEx = stereoTypes;
                            this._connector.Update();
                        }
                        break;
                }
                ((EAModelImplementation)this._model).Repository.AdviseConnectorChange(this._connector.ConnectorID);
            }
        }

        /// <summary>
        /// Deletes the specified stereotype from the association or from a specific association end.
        /// </summary>
        /// <param name="stereotype">Stereotype to check.</param>
        /// <param name="endpoint">Which part of the association to inspect.</param>
        internal override void DeleteStereotype(string stereotype, MEAssociation.AssociationEnd endpoint)
        {
            if (!string.IsNullOrEmpty(stereotype))
            {
                string checkType = (stereotype.Contains("::")) ? stereotype.Substring(stereotype.IndexOf("::") + 2) : stereotype;
                switch (endpoint)
                {
                    case MEAssociation.AssociationEnd.Source:
                        if (this._connector.ClientEnd.StereotypeEx.Contains(checkType))
                        {
                            this._connector.ClientEnd.StereotypeEx = DeleteStereotype(checkType, this._connector.ClientEnd.StereotypeEx);
                            this._connector.ClientEnd.Update();
                        }
                        break;

                    case MEAssociation.AssociationEnd.Destination:
                        if (this._connector.SupplierEnd.StereotypeEx.Contains(checkType))
                        {
                            this._connector.SupplierEnd.StereotypeEx = DeleteStereotype(checkType, this._connector.SupplierEnd.StereotypeEx);
                            this._connector.SupplierEnd.Update();
                        }
                        break;

                    default:
                        if (this._connector.StereotypeEx.Contains(checkType))
                        {
                            this._connector.StereotypeEx = DeleteStereotype(checkType, this._connector.StereotypeEx);
                            this._connector.Update();
                        }
                        break;
                }
                ((EAModelImplementation)this._model).Repository.AdviseConnectorChange(this._connector.ConnectorID);
            }
        }

        /// <summary>
        /// Returns element annotation of specified endpoint (if present).
        /// </summary>
        /// <returns>Element annotation or empty string if nothing present.</returns>
        internal override string GetAnnotation(MEAssociation.AssociationEnd endpoint)
        {
            string notes = string.Empty;
            if (endpoint == MEAssociation.AssociationEnd.Association)
            {
                if (!string.IsNullOrEmpty(this._connector.Notes)) notes = this._connector.Notes;
            }
            else
            {
                EA.ConnectorEnd connectorEnd = (endpoint == MEAssociation.AssociationEnd.Source) ? this._connector.ClientEnd : this._connector.SupplierEnd;
                if (!string.IsNullOrEmpty(connectorEnd.RoleNote)) notes = connectorEnd.RoleNote;
            }
            return notes;
        }

        /// <summary>
        /// Returns the type of the association (one of Aggregation, Association, Composition, Generalization, 
        /// MessageAssociation or Usage).
        /// </summary>
        /// <returns>Association type.</returns>
        internal override MEAssociation.AssociationType GetAssociationType()
        {
            MEAssociation.AssociationType associationType = MEAssociation.AssociationType.Unknown;

            if (this._connector.Type == "Generalization") associationType = MEAssociation.AssociationType.Generalization;
            else if (this._connector.Type == "Usage") associationType = MEAssociation.AssociationType.Usage;
            else if (this._connector.Type == "Association")
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                string stereotype = context.GetConfigProperty(_MessageAssociationStereotype);
                stereotype = (stereotype.Contains("::")) ? stereotype.Substring(stereotype.IndexOf("::") + 2) : stereotype;
                if (this._connector.StereotypeEx.Contains(stereotype))
                {
                    associationType = MEAssociation.AssociationType.MessageAssociation;
                }
                else
                {
                    EA.ConnectorEnd sourceEnd = this._connector.ClientEnd;
                    if (sourceEnd.Aggregation == 1) associationType = MEAssociation.AssociationType.Aggregation;
                    else if (sourceEnd.Aggregation == 2) associationType = MEAssociation.AssociationType.Composition;
                    else associationType = MEAssociation.AssociationType.Association;
                }
            }
            return associationType;
        }

        /// <summary>
        /// Returns an integer representation of an EA 'Cardinality' string. In theory, this string can contain litrally anything. In our particular case, we support:
        /// - Single value 'n' is translated to 'exactly n', i.e. minOcc = maxOcc = 'n'. Unless 'n' == 0, in which case minOcc = 0, maxOcc = 1;
        /// - Single value '*' is translated to '0 to unbounded', represented by minOcc = maxOcc = 0;
        /// - Range 'n..m' is translated to minOcc = 'n', maxOcc = 'm'. Unless 'm' = 0, in which case maxOcc = 1. If this leads to minOcc > maxOcc, both values will be swapped!
        /// - Range 'n..*' is translated to minOcc = 'n', maxOcc = 0 (maxOcc == 0 is lateron interpreted as 'unbounded').
        /// All other formates will result in a response values of minOcc = maxOcc = -1.
        /// </summary>
        /// <param name="endpoint">The endpoint to be evaluated.</param>
        /// <returns>Tuple consisting of minOCC, maxOcc. In case of errors, both will be -1.</returns>
        internal override Tuple<int, int> GetCardinality(MEAssociation.AssociationEnd endpoint)
        {
            string cardinality = (endpoint == MEAssociation.AssociationEnd.Source) ? this._connector.ClientEnd.Cardinality.Trim() :
                                                                                     this._connector.SupplierEnd.Cardinality.Trim();
            int minOcc, maxOcc;

            try
            {
                // In case of empty or invalid cardinality, return illegal tuple...
                if (string.IsNullOrEmpty(cardinality)) return new Tuple<int, int>(-1, -1);

                if (cardinality.Contains(".."))
                {
                    // Different lower- and upper boundaries...
                    string lowerBound = cardinality.Substring(0, cardinality.IndexOf('.'));
                    string upperBound = cardinality.Substring(cardinality.LastIndexOf('.') + 1);
                    minOcc = Convert.ToInt16(lowerBound);
                    if (upperBound.Contains("*"))
                    {
                        // maxOcc is lateron translated to 'unbounded'.
                        maxOcc = 0;
                    }
                    else
                    {
                        // If we have a 0..0 cardinality, this is translated to 'optional 0 or 1'.
                        // And if maxOcc < minOcc (and not unbounded), we return an illegal cardinality.
                        maxOcc = Convert.ToInt16(upperBound);
                        if ((maxOcc > 0) && (maxOcc < minOcc))
                        {
                            EA.Element source = ((EAModelImplementation)this._model).Repository.GetElementByID(this._connector.ClientID);
                            EA.Element destination = ((EAModelImplementation)this._model).Repository.GetElementByID(this._connector.SupplierID);
                            Logger.WriteError("SparxEA.Model.EAMEIAssociation.GetCardinality >> Unsupported format in cardinality field of association: '" +
                                              source.Name + "-->" + destination.Name + "'!");
                            minOcc = maxOcc = -1;
                        }
                        if (maxOcc == 0) maxOcc = 1;
                    }
                }
                else
                {
                    // Upper- and lower boundaries are equal...
                    if (cardinality.Trim() == "*")
                    {
                        // A single '*' character is interpreted as: 0 to unbounded, which translates to an upper boundary of 0.
                        minOcc = 0;
                        maxOcc = 0;
                    }
                    else
                    {
                        // A single character is translated to 'exactly n', with the exception of '0', which is translated to 'optional 0 or 1'.
                        minOcc = Convert.ToInt16(cardinality);
                        maxOcc = (minOcc == 0) ? 1 : minOcc;
                    }
                }
                return new Tuple<int, int>(minOcc, maxOcc);
            }
            catch (FormatException exc)
            {
                EA.Element source = ((EAModelImplementation)this._model).Repository.GetElementByID(this._connector.ClientID);
                EA.Element destination = ((EAModelImplementation)this._model).Repository.GetElementByID(this._connector.SupplierID);
                Logger.WriteError("SparxEA.Model.EAMEIAssociation.GetCardinality >> Unsupported format in cardinality field of association: '" + 
                                   source.Name + "-->" + destination.Name + "';" + Environment.NewLine + exc.Message);
            }
            return new Tuple<int, int>(-1, -1);
        }

        /// <summary>
        /// This method searches the specified part of the association for any tags (from the configured list of relevant 
        /// tags) that have contents. The method returns a list of MEDocumentation elements for each non-empty tag found,
        /// including the standard 'Note' tag.
        /// </summary>
        /// <param name="endPoint">Which part of the association to check.</param>
        /// <returns>List of documentation nodes or empty list on error or if no comments available.</returns>
        internal override List<MEDocumentation> GetDocumentation(MEAssociation.AssociationEnd endpoint)
        {
            try
            {
                // Due to a bug (or 'feature') in EA, Notes that are updated within the same program, but using different references
                // to the same repository element, do not show up in other references. So, to be sure, we reload the connector instance
                // before retrieving out notes...
                this._connector = ((EAModelImplementation)this._model).Repository.GetConnectorByID(this._connector.ConnectorID);

                var docList = new List<MEDocumentation>();
                ContextSlt context = ContextSlt.GetContextSlt();
                string sourcePrefix = context.GetConfigProperty(_DocgenSourcePrefix);
                string[] tagList = context.GetConfigProperty(_DocgenTagList).Split(',');

                // First of all, harvest annotation from any of the configured tags (if present) and add to the annotation list...
                foreach (string tag in tagList)
                {
                    string tagVal = this.GetTag(tag, endpoint);
                    if (!string.IsNullOrEmpty(tagVal)) docList.Add(new MEDocumentation(sourcePrefix + tag, tagVal));
                }

                // Finally, appends generic notes to the list (if present)....
                if (endpoint == MEAssociation.AssociationEnd.Association)
                {
                    if (!string.IsNullOrEmpty(this._connector.Notes)) AddNotesToDoclist(ref docList, this._connector.Notes, "en", sourcePrefix + "annotation");
                }
                else
                {
                    EA.ConnectorEnd connectorEnd = (endpoint == MEAssociation.AssociationEnd.Source) ? this._connector.ClientEnd : this._connector.SupplierEnd;
                    string name = (endpoint == MEAssociation.AssociationEnd.Source) ? "sourceAnnotation" : "targetAnnotation";
                    if (!string.IsNullOrEmpty(connectorEnd.RoleNote)) AddNotesToDoclist(ref docList, connectorEnd.RoleNote, "en", sourcePrefix + name);
                }
                return docList;
            }
            catch (Exception exc)
            {
                Logger.WriteError("SparxEA.Model.EAMEIClass.getDocumentation >> Caught an exception: " + exc.Message);
            }
            return null;
        }

        /// <summary>
        /// Simple method that searches an association implementation for the occurence of a tag with specified name. 
        /// If found, the value of the tag is returned.
        /// </summary>
        /// <param name="tagName">The name of the tag.</param>
        /// <param name="endpoint">Which part of the association to inspect.</param>
        /// <returns>Value of specified tag or empty string if nothing could be found.</returns>
        internal override string GetTag(string tagName, MEAssociation.AssociationEnd endpoint)
        {
            try
            {
                if (endpoint == MEAssociation.AssociationEnd.Association)
                {
                    foreach (ConnectorTag t in this._connector.TaggedValues)
                    {
                        if (String.Compare(t.Name, tagName, StringComparison.OrdinalIgnoreCase) == 0) return t.Value; //Case-independent compare.
                    }
                }
                else
                {
                    EA.ConnectorEnd connectorEnd = (endpoint == MEAssociation.AssociationEnd.Source) ? this._connector.ClientEnd : this._connector.SupplierEnd;
                    foreach (RoleTag t in connectorEnd.TaggedValues)
                    {
                        if (String.Compare(t.Tag, tagName, StringComparison.OrdinalIgnoreCase) == 0) return t.Value; //Case-independent compare.
                    }
                }
            }
            catch { /* & ignore all errors */ }
            return string.Empty;
        }

        /// <summary>
        /// The method checks whether one or more stereotypes from the given list of stereotypes are owned by the specified
        /// part of the association.
        /// Note that the method can only check for 'leaf' stereotypes (e.g. it will NOT find generalizations in the stereotype
        /// hierarchy). This is due to limitations in the EA API!
        /// Also note that this is a case sensitive compare!
        /// </summary>
        /// <param name="stereotypes">List of stereotypes to check.</param>
        /// <param name="endpoint">Which part of the association to inspect.</param>
        /// <returns>True if at least one match is found, false otherwise.</returns>
        internal override bool HasStereotype(List<string> stereotypes, MEAssociation.AssociationEnd endpoint)
        {
            string stereotypeList = (endpoint == MEAssociation.AssociationEnd.Association) ? this._connector.StereotypeEx :
                                    ((endpoint == MEAssociation.AssociationEnd.Source) ? this._connector.ClientEnd.StereotypeEx :
                                    this._connector.SupplierEnd.StereotypeEx);
            foreach (string st in stereotypes)
            {
                string normalizedType = (st.Contains("::")) ? st.Substring(st.IndexOf("::") + 2) : st;
                if (stereotypeList.Contains(normalizedType)) return true;
            }
            return false;
        }

        /// <summary>
        /// The method checks whether the given stereotype is owned by the indicated part of the association.
        /// Note that the method can only check for 'leaf' stereotypes (e.g. it will NOT find generalizations in the stereotype
        /// hierarchy). This is due to limitations in the EA API!
        /// Also note that this is a case sensitive compare!
        /// </summary>
        /// <param name="stereotype">Stereotype to check.</param>
        /// <param name="endpoint">Which part of the association to inspect.</param>
        /// <returns>True if a match is found, false otherwise.</returns>
        internal override bool HasStereotype(string stereotype, MEAssociation.AssociationEnd endpoint)
        {
            string stereotypeList = (endpoint == MEAssociation.AssociationEnd.Association) ? this._connector.StereotypeEx :
                                    ((endpoint == MEAssociation.AssociationEnd.Source) ? this._connector.ClientEnd.StereotypeEx :
                                    this._connector.SupplierEnd.StereotypeEx);
            string normalizedType = (stereotype.Contains("::")) ? stereotype.Substring(stereotype.IndexOf("::") + 2) : stereotype;
            return stereotypeList.Contains(normalizedType);
        }

        /// <summary>
        /// Updates annotation of the specified endpoint, or of the connector itself.
        /// </summary>
        /// <param name="text">Annotation text to write.</param>
        /// <param name="endpoint">What to update.</param>
        internal override void SetAnnotation(string text, MEAssociation.AssociationEnd endpoint)
        {
            if (endpoint == MEAssociation.AssociationEnd.Association)
            {
                this._connector.Notes = text;
                this._connector.Update();
            }
            else
            {
                EA.ConnectorEnd connectorEnd = (endpoint == MEAssociation.AssociationEnd.Source) ? this._connector.ClientEnd : this._connector.SupplierEnd;
                connectorEnd.RoleNote = text;
                connectorEnd.Update();
            }
            // Force refresh of connector data...
            this._connector = ((EAModelImplementation)this._model).Repository.GetConnectorByID(this._connector.ConnectorID);
            ((EAModelImplementation)this._model).Repository.AdviseConnectorChange(this._connector.ConnectorID);
        }

        /// <summary>
        /// Either update the alias name of the association, or the role alias of the specified endpoint.
        /// </summary>
        /// <param name="newName">Alias name to be assigned to Connector or Endpoint.</param>
        /// <param name="endpoint">What to update.</param>
        internal override void SetAliasName(string newName, MEAssociation.AssociationEnd endpoint)
        {
            if (endpoint == MEAssociation.AssociationEnd.Association)
            {
                this._connector.Alias = newName;
                this._connector.Update();
                this._aliasName = newName;
            }
            else
            {
                EA.ConnectorEnd connectorEnd = (endpoint == MEAssociation.AssociationEnd.Source) ? this._connector.ClientEnd : this._connector.SupplierEnd;
                connectorEnd.Alias = newName;
                connectorEnd.Update();
            }
            // Force refresh of connector data...
            this._connector = ((EAModelImplementation)this._model).Repository.GetConnectorByID(this._connector.ConnectorID);
            ((EAModelImplementation)this._model).Repository.AdviseConnectorChange(this._connector.ConnectorID);
        }

        /// <summary>
        /// Set the cardinality of the specified endpoint. If item2 is 0, this is translated to "*". And if Item2 < Item1, the functions silently
        /// fails!
        /// </summary>
        /// <param name="card">Cardinality to set.</param>
        /// <param name="endpoint">The endpoint to be evaluated.</param>
        internal override void SetCardinality(Tuple<int,int> card, MEAssociation.AssociationEnd endpoint)
        {
            if (card.Item1 < 0  || card.Item2 < 0 || (card.Item2 != 0 && card.Item2 < card.Item1))
            {
                Logger.WriteError("SparxEA.Model.EAMEIAssociation.GetCardinality >> Cardinality format error '" + card.Item1 + ".." + card.Item2 + "' detected!");
                return;
            }

            string newCard = card.Item1.ToString();
            if (card.Item1 != card.Item2) newCard += ".." + (card.Item2 == 0 ? "*" : card.Item2.ToString());

            if (endpoint == MEAssociation.AssociationEnd.Source)
            {
                this._connector.ClientEnd.Cardinality = newCard;
                this._connector.ClientEnd.Update();
            }
            else if (endpoint == MEAssociation.AssociationEnd.Destination)
            {
                this._connector.SupplierEnd.Cardinality = newCard;
                this._connector.SupplierEnd.Update();
            }
            else
            {
                Logger.WriteError("SparxEA.Model.EAMEIAssociation.GetCardinality >> Unable to change cardinality of association as a whole, select an endpoint instead!");
                return;
            }
            this._connector.Direction = "Source -> Destination";
            this._connector.Update();
            ((EAModelImplementation)this._model).Repository.AdviseConnectorChange(this._connector.ConnectorID);
        }

        /// <summary>
        /// Either update the name of the association, or the role of the specified endpoint.
        /// </summary>
        /// <param name="newName">Name to be assigned to Connector or Endpoint.</param>
        /// <param name="endpoint">What to update.</param>
        internal override void SetName(string newName, MEAssociation.AssociationEnd endpoint)
        {
            if (endpoint == MEAssociation.AssociationEnd.Association)
            {
                this._connector.Name = newName;
                this._connector.Update();
                this._name = newName;
            }
            else
            {
                EA.ConnectorEnd connectorEnd = (endpoint == MEAssociation.AssociationEnd.Source) ? this._connector.ClientEnd : this._connector.SupplierEnd;
                connectorEnd.Role = newName;
                connectorEnd.Update();
            }
            // Force refresh of connector data...
            this._connector = ((EAModelImplementation)this._model).Repository.GetConnectorByID(this._connector.ConnectorID);
            ((EAModelImplementation)this._model).Repository.AdviseConnectorChange(this._connector.ConnectorID);
        }

        /// <summary>
        /// Set the tag with specified name to the specified value. 
        /// If the tag could not be found, nothing will happen, unless the 'createIfNotExists' flag is set to true, in which case the tag will
        /// be created.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="tagValue">Value of the tag.</param>
        /// <param name="createIfNotExist">When set to true, create the tag if it's not there.</param>
        /// <param name="endpoint">Which part of the association to inspect.</param>
        internal override void SetTag(string tagName, string tagValue, bool createIfNotExist, MEAssociation.AssociationEnd endpoint)
        {
            try
            {
                if (endpoint == MEAssociation.AssociationEnd.Association)
                {
                    foreach (ConnectorTag t in this._connector.TaggedValues)
                    {
                        if (String.Compare(t.Name, tagName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            t.Value = tagValue;
                            t.Update();
                            return;
                        }
                    }

                    // Element tag not found, create new one if instructed to do so...
                    if (createIfNotExist)
                    {
                        var newTag = (ConnectorTag)this._connector.TaggedValues.AddNew(tagName, "TaggedValue");
                        newTag.Value = tagValue;
                        newTag.Update();
                        this._connector.TaggedValues.Refresh();
                    }
                }
                else
                {
                    EA.ConnectorEnd connectorEnd = (endpoint == MEAssociation.AssociationEnd.Source) ? this._connector.ClientEnd : this._connector.SupplierEnd;
                    foreach (RoleTag t in connectorEnd.TaggedValues)
                    {
                        if (String.Compare(t.Tag, tagName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            t.Value = tagValue;
                            t.Update();
                            return;
                        }
                    }

                    // Element tag not found, create new one if instructed to do so...
                    if (createIfNotExist)
                    {
                        var newTag = (RoleTag)connectorEnd.TaggedValues.AddNew(tagName, "TaggedValue");
                        newTag.Value = tagValue;
                        newTag.Update();
                        connectorEnd.TaggedValues.Refresh();
                    }
                }
                // Force refresh of connector data...
                this._connector = ((EAModelImplementation)this._model).Repository.GetConnectorByID(this._connector.ConnectorID);
                ((EAModelImplementation)this._model).Repository.AdviseConnectorChange(this._connector.ConnectorID);
            }
            catch (Exception exc)
            {
                Logger.WriteError("SparxEA.Model.EAIAssociation.SetTag >> Caught an exception because: " + Environment.NewLine + exc.Message);
            }
        }

        /// <summary>
        /// Returns an endpoint descriptor for the source-side of the association.
        /// </summary>
        /// <returns>Endpoint descriptor.</returns>
        protected override EndpointDescriptor GetSourceDescriptor()
        {
            EA.ConnectorEnd connectorEnd = this._connector.ClientEnd;
            return GetDescriptor(connectorEnd, this._connector.ClientID);
        }

        /// <summary>
        /// Returns an endpoint descriptor for the destination-side of the association.
        /// </summary>
        /// <returns>Endpoint descriptor.</returns>
        protected override EndpointDescriptor GetDestinationDescriptor()
        {
            EA.ConnectorEnd connectorEnd = this._connector.SupplierEnd;
            return GetDescriptor(connectorEnd, this._connector.SupplierID);
        }

        /// <summary>
        /// Searches the specified, comma separated, list of stereotypes for the occurence of a single stereotype.
        /// It returns the same list, minus the specified stereotype. When the specified type is not in the list, the return list is
        /// identical to the input list. The function uses a case-insensitive compare.
        /// </summary>
        /// <param name="toBeDeleted">Stereotype to remove from the list.</param>
        /// <param name="stereotypeList">Comma separated list of stereotypes.</param>
        /// <returns>List minus specified stereotype.</returns>
        private string DeleteStereotype(string toBeDeleted, string stereotypeList)
        {
            string[] allTypes = stereotypeList.Split(',');
            string resultList = string.Empty;
            bool firstOne = true;
            foreach (string stereoType in allTypes)
            {
                // We simply copy all strings from the original list to a new list, skipping the one to be deleted...
                if (string.Compare(toBeDeleted, stereoType, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    resultList = (!firstOne) ? "," + stereoType : stereoType;
                    firstOne = false;
                }
            }
            ((EAModelImplementation)this._model).Repository.AdviseConnectorChange(this._connector.ConnectorID);
            return resultList;
        }

        /// <summary>
        /// Create the proper descriptor type for the specified connector endpoint.
        /// </summary>
        /// <param name="endpoint">One of source- or destination enpoint repository objects.</param>
        /// <param name="classID">ID of the class that is associated with the endpoint.</param>
        /// <returns>Endpoint descriptor.</returns>
        private EndpointDescriptor GetDescriptor(EA.ConnectorEnd endpoint, int classID)
        {
            // Since the connector might be associated with a data type, we check the type of the endpoint and create either 
            // an appropriate data type or a class...
            MEClass owner = null;
            EA.Element element = ((EAModelImplementation)this._model).Repository.GetElementByID(classID);
            if (element.Type == "Class") owner = new MEClass(classID);
            else // element.Type is not a class, we assume it's a data type instead....
            {
                ModelSlt model = ModelSlt.GetModelSlt();
                owner = model.GetDataType(classID);
            }

            EndpointDescriptor descriptor;

            // Case-insensitive compare...
            if (string.Compare(this._connector.Type, "Generalization", StringComparison.OrdinalIgnoreCase) == 0)
            {
                descriptor = new EndpointDescriptor(owner);
            }
            else
            {
                bool isNavigable = (string.Compare(endpoint.Navigable, "Navigable", StringComparison.OrdinalIgnoreCase) == 0);
                descriptor = new EndpointDescriptor(owner, endpoint.Cardinality, endpoint.Role, endpoint.Alias, isNavigable);
            }
            return descriptor;
        }
    }
}
