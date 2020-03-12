using System.Xml;
using System.Collections.Generic;
using Framework.Model;
using Framework.Logging;

namespace SparxEA.Model
{
    /// <summary>
    /// </summary>
    internal sealed class EAStereotypes
    {
        private const string _StartToken = "Name=";
        private const string _FQNStartToken = "FQName=";
        private const string _EndToken = "@ENDSTEREO;";

        private List<string> _stereotypes;          // List of unqualified stereotype names.
        private List<string> _FQStereotypes;        // List of fully-qualified stereotype names.
        private ModelElementImplementation _owner;  // The element for which we manage stereotypes.
        private EAModelImplementation _model;       // Our currently open model.

        /// <summary>
        /// Returns the primary stereotype for the object (or an empty string if not defined).
        /// </summary>
        internal string Primary { get { return this._stereotypes.Count > 0 ? this._stereotypes[0] : string.Empty; } }

        /// <summary>
        /// Returns the fully-qualified name of the primary stereotype for the object (or an empty string if not defined).
        /// </summary>
        internal string FQPrimary { get { return this._FQStereotypes.Count > 0 ? this._FQStereotypes[0] : string.Empty; } }

        /// <summary>
        /// Returns all stereotypes for the object, the first entry being the primary stereotype.
        /// </summary>
        internal List<string> Stereotypes { get { return this._stereotypes; } }

        /// <summary>
        /// Returns the list of all fully-qualified stereotype names for the object, the first entry being the primary stereotype.
        /// </summary>
        internal List<string> FQStereotypes { get { return this._FQStereotypes; } }

        /// <summary>
        /// Retrieves all stereotypes for the object with the specified GUID. The list is maintained WITHOUT the profile name.
        /// </summary>
        /// <param name="model">Represents the current repository.</param>
        /// <param name="owner">ModelElement for which we want to manage the stereotypes.</param>
        internal EAStereotypes(EAModelImplementation model, ModelElementImplementation owner)
        {
            this._owner = owner;
            this._model = model;
            string query = @"SELECT x.Description AS StereotypeList FROM t_xref x WHERE x.Client = '" + owner.GlobalID + "' AND x.Name = 'Stereotypes';";
            var queryResult = new XmlDocument();
            queryResult.LoadXml(model.Repository.SQLQuery(query));
            XmlNodeList elements = queryResult.GetElementsByTagName("Row");
            this._stereotypes = new List<string>();
            this._FQStereotypes = new List<string>();

            // We should have only a single result...
            if (elements.Count > 0)
            {
                string stereotypes = elements[0]["StereotypeList"].InnerText.Trim();
                string loggerList = string.Empty;
                while (stereotypes.Length > 0)
                {
                    int endTokenIndex = stereotypes.IndexOf(_EndToken);
                    string entry = stereotypes.Substring(0, endTokenIndex);
                    stereotypes = stereotypes.Length > _EndToken.Length ? stereotypes.Substring(endTokenIndex + _EndToken.Length) : string.Empty;
                    string nameEntry = entry.Substring(entry.IndexOf(_StartToken) + _StartToken.Length);
                    string FQNEntry = entry.Substring(entry.IndexOf(_FQNStartToken) + _FQNStartToken.Length);
                    nameEntry = nameEntry.Substring(0, nameEntry.IndexOf(";"));
                    FQNEntry = FQNEntry.Substring(0, FQNEntry.IndexOf(";"));
                    this._stereotypes.Add(nameEntry);
                    this._FQStereotypes.Add(FQNEntry);
                    loggerList += entry + " ";
                }
            }
        }

        /// <summary>
        /// Add a new stereotype to the collection. Note that this is a LOCAL operation only since the class does not know the type
        /// of object to update. If the stereotype already exists, the function does not perform any operations.
        /// The argument can either be a 'fully qualified' stereotype or a 'normalized' stereotype.
        /// Currently, the EAStereotypes class is used ONLY for Packages and Classes. 
        /// </summary>
        /// <param name="stereotype">Stereotype to be added.</param>
        internal void AddStereotype(string stereotype)
        {
            if (!string.IsNullOrEmpty(stereotype) && !this._stereotypes.Contains(stereotype))
            {
                string normalizedType = stereotype.Contains("::") ? stereotype.Substring(stereotype.IndexOf("::") + 2) : stereotype;
                this._stereotypes.Add(normalizedType);
                if (stereotype.Contains("::")) this._FQStereotypes.Add(stereotype);
                string elementStereotypes;

                switch (this._owner.Type)
                {
                    case ModelElementType.Package:
                        EA.Package pkg = this._model.Repository.GetPackageByID(this._owner.ElementID);
                        if (pkg.Element != null)
                        {
                            elementStereotypes = pkg.Element.StereotypeEx;
                            elementStereotypes += (elementStereotypes.Length > 0) ? "," + stereotype : stereotype;
                            pkg.Element.StereotypeEx = elementStereotypes;
                            pkg.Element.Update();
                        }
                        else Logger.WriteError("SparxEA.Model.EAStereotypes >> Package '" + pkg.Name + "' not yet fully initialized!");
                        break;

                    case ModelElementType.DataType:
                    case ModelElementType.Object:
                    case ModelElementType.Enumeration:
                    case ModelElementType.Class:
                        EA.Element el = this._model.Repository.GetElementByID(this._owner.ElementID);
                        elementStereotypes = el.StereotypeEx;
                        elementStereotypes += (elementStereotypes.Length > 0) ? "," + stereotype : stereotype;
                        el.StereotypeEx = elementStereotypes;
                        el.Update();
                        this._model.Repository.AdviseElementChange(el.ElementID);
                        break;
                }
            }
        }

        /// <summary>
        /// The method checks whether the collection contains the given stereotype. The argument can either be a 'fully qualified' stereotype
        /// or a 'normalized' stereotype. When specified as a fully-qualified name, we will be looking in the list of fully qualified stereotypes.
        /// Otherwise, we're looking in the normalized list.
        /// </summary>
        /// <param name="stereotype">Stereotype to check.</param>
        /// <returns>True if a match is found, false otherwise.</returns>
        internal bool HasStereotype(string stereotype)
        {
            if (string.IsNullOrEmpty(stereotype)) return false;
            return stereotype.Contains("::")? this._FQStereotypes.Contains(stereotype): this._stereotypes.Contains(stereotype);
        }

        /// <summary>
        /// The method checks whether the collection contains a stereotype from the list of stereotypes. The argument can either be a list of 'fully qualified' 
        /// stereotypes or a list of 'normalized' stereotypes or even a mix of the two.
        /// </summary>
        /// <param name="stereotypes">List of stereotypes to check.</param>
        /// <returns>True if a match is found, false otherwise.</returns>
        internal bool HasStereotype(List<string> stereotypes)
        {
            if (stereotypes != null) foreach (string stereotype in stereotypes) if (HasStereotype(stereotype)) return true;
            return false;
        }
    }
}
