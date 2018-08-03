using System;
using System.Collections.Generic;
using EA;
using Framework.Logging;
using Framework.Model;

namespace SparxEA.Model
{
    /// <summary>
    /// Represents a Model Profiler within Sparx EA. Profilers are represented by components called 'Element' of EA type 'Artifact'.
    /// </summary>
    internal sealed class EAMEIProfiler : MEIProfiler
    {
        private EA.Element _element;    // EA Profile representation (an element of type 'Artifact').

        /// <summary>
        /// Creates a new EA Profile implementation based on repository ID.
        /// </summary>
        internal EAMEIProfiler(EAModelImplementation model, int profileID) : base(model)
        {
            this._element = model.Repository.GetElementByID(profileID);
            if (this._element != null && this._element.Type == "Artifact")
            {
                this._name = this._element.Name;
                this._elementID = profileID;
                this._globalID = this._element.ElementGUID;
				this._aliasName = this._element.Alias ?? string.Empty;
            }
            else
            {
                Logger.WriteError("SparxEA.Model.EAMEIProfiler >> Failed to retrieve EA Profile Element with ID: " + profileID);
            }
        }

        /// <summary>
        /// Creates a new EA Profile implementation based on repository GUID.
        /// </summary>
        internal EAMEIProfiler(EAModelImplementation model, string profileGUID) : base(model)
        {
            this._element = model.Repository.GetElementByGuid(profileGUID);
            if (this._element != null && this._element.Type == "Artifact")
            {
                this._name = this._element.Name;
                this._elementID = this._element.ElementID;
                this._globalID = profileGUID;
                this._aliasName = this._element.Alias ?? string.Empty;
            }
            else
            {
                Logger.WriteError("SparxEA.Model.EAMEIProfiler >> Failed to retrieve EA Profile Element with GUID: " + profileGUID);
            }
        }

        /// <summary>
        /// Constructor that creates a new implementation instance based on a provided EA element instance.
        /// </summary>
        /// <param name="model">The associated model implementation.</param>
        /// <param name="element">The EA Element on which the implementation is based.</param>
        internal EAMEIProfiler(EAModelImplementation model, EA.Element element) : base(model)
        {
            this._element = element;
            if (this._element != null && this._element.Type == "Artifact")
            {
                this._name = this._element.Name;
                this._elementID = element.ElementID;
                this._globalID = this._element.ElementGUID;
				this._aliasName = this._element.Alias ?? string.Empty;
            }
            else
            {
                Logger.WriteError("SparxEA.Model.EAMEIProfiler >>  Constructor based on empty- or wrong type of element!");
            }
        }

        /// <summary>
        /// Assignes the specified stereotype to this Profiler.
        /// </summary>
        /// <param name="stereotype">Stereotype to be assigned.</param>
        internal override void AddStereotype(string stereotype)
        {
            if (!string.IsNullOrEmpty(stereotype))
            {
                string stereoTypes = this._element.StereotypeEx;
                if (!this._element.HasStereotype(stereotype))
                {
                    stereoTypes += (stereoTypes.Length > 0) ? "," + stereotype : stereotype;
                    this._element.StereotypeEx = stereoTypes;
                    this._element.Update();
                }
            }
        }

        /// <summary>
        /// Returns annotation (if present).
        /// </summary>
        /// <returns>Profile annotation or empty string if nothing present.</returns>
        internal override string GetAnnotation()
        {
            string notes = this._element.Notes;
            return (!string.IsNullOrEmpty(notes))? notes.Trim(): string.Empty;
        }

        /// <summary>
        /// Retrieve the package in which this Profiler is declared.
        /// </summary>
        /// <returns>Owning package.</returns>
        internal override MEPackage GetOwningPackage()
        {
            return new MEPackage(this._element.PackageID);
        }

        /// <summary>
        /// Simple method that searches a Profiler implementation for the occurence of a tag with specified name. 
        /// If found, the value of the tag is returned.
        /// </summary>
        /// <param name="tagName">The name of the tag.</param>
        /// <returns>Value of specified tag or empty string if nothing could be found.</returns>
        internal override string GetTag(string tagName)
        {
            string retVal = string.Empty;
            try
            {
                foreach (TaggedValue t in this._element.TaggedValues)
                {
                    if (String.Compare(t.Name, tagName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        retVal = t.Value;
                        break;
                    }
                }
            }
            catch { /* & ignore all errors */ }
            return retVal;
        }

        /// <summary>
        /// The method checks whether one or more stereotypes from the given list of stereotypes are owned by the Profiler.
        /// Since HasStereotype supports fully-qualified stereotype names, we don't have to strip the (optional) profile names in this case.
        /// </summary>
        /// <param name="stereotypes">List of stereotypes to check.</param>
        /// <returns>True if at least one match is found, false otherwise.</returns>
        internal override bool HasStereotype(List<string> stereotypes)
        {
            foreach (string stereotype in stereotypes)
            {
                if (this._element.HasStereotype(stereotype)) return true;
            }
            return false;
        }

        /// <summary>
        /// The method checks whether the given stereotype is owned by the Profiler. Since HasStereotype supports fully-qualified stereotype
        /// names, we don't have to strip the (optional) profile names in this case.
        /// </summary>
        /// <param name="stereotype">Stereotype to check.</param>
        /// <returns>True if a match is found, false otherwise.</returns>
        internal override bool HasStereotype(string stereotype)
        {
            return this._element.HasStereotype(stereotype);
        }

        /// <summary>
        /// Copies the profile definition from the specified source Profiler to the current Profiler. 
        /// In EA, the profile definition is stored in the database as an XML document and the method thus copies this document from the 
        /// source- to the destination profile.
        /// </summary>
        /// <param name="sourceProfiler">Profiler to copy.</param>
        internal override void LoadProfile(MEProfiler sourceProfiler)
        {
            Logger.WriteInfo("SparxEA.Model.EAMEIClass.LoadProfile >> Copy source profile '" + sourceProfiler.Name + "' into '" + this.Name + "'...");

            EAMEIProfiler sourceObject = sourceProfiler.Implementation as EAMEIProfiler;
            if (sourceObject != null)
            {
                EA.Repository repository = ((EAModelImplementation)this._model).Repository;
                if (sourceObject._element.Type == "Artifact")
                {
                    Logger.WriteInfo("SparxEA.Model.EAMEIClass.LoadProfile >> Source element is of type 'Artifact', which is good...");

                    string update = "UPDATE t_document SET StrContent = " +
                                       "(SELECT REPLACE(d.StrContent,'<description name=\"" + sourceProfiler.Name + "\"','<description name = \"" + this.Name + "\"') " +
                                         "FROM (SELECT * FROM t_document) AS d WHERE d.ElementType = 'SC_MessageProfile' AND d.ElementID = '" + sourceProfiler.GlobalID + "') " +
                                    "WHERE ElementType = 'SC_MessageProfile' AND ElementID = '" + this.GlobalID + "'";
                    Logger.WriteInfo("SparxEA.Model.EAMEIClass.LoadProfile >> Going to execute query: " + Environment.NewLine + update);
                    repository.Execute(update);     // Undocumented method, might dissapear at some time, at which we will be screwed :-)
                    this._element.Refresh();
                    repository.AdviseElementChange(this._element.ElementID);
                }
            }
        }

        /// <summary>
        /// Updates Class annotation.
        /// </summary>
        /// <param name="text">The annotation text to write.</param>
        internal override void SetAnnotation(string text)
        {
            this._element.Notes = text;
            this._element.Update();
            ((EAModelImplementation)this._model).Repository.AdviseElementChange(this._element.ElementID);
        }

        /// <summary>
        /// Update the alias name of the Class.
        /// </summary>
        /// <param name="newAliasName">New name to be assigned.</param>
        internal override void SetAliasName(string newAliasName)
        {
            this._element.Alias = newAliasName;
            this._aliasName = newAliasName;
            this._element.Update();
            ((EAModelImplementation)this._model).Repository.AdviseElementChange(this._element.ElementID);
        }

        /// <summary>
        /// Update the name of the Class.
        /// </summary>
        /// <param name="newName">New name to be assigned.</param>
        internal override void SetName(string newName)
        {
            this._element.Name = newName;
            this._name = newName;
            this._element.Update();
            ((EAModelImplementation)this._model).Repository.AdviseElementChange(this._element.ElementID);
        }

        /// <summary>
        /// Set the tag with specified name to the specified value for the current class. 
        /// If the tag could not be found, nothing will happen, unless the 'createIfNotExists' flag is set to true, in which case the tag will
        /// be created.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="tagValue">Value of the tag.</param>
        /// <param name="createIfNotExist">When set to true, create the tag if it's not there.</param>
        internal override void SetTag(string tagName, string tagValue, bool createIfNotExist)
        {
            try
            {
                foreach (TaggedValue t in this._element.TaggedValues)
                {
                    if (String.Compare(t.Name, tagName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        t.Value = tagValue;
                        t.Update();
                        this._element.TaggedValues.Refresh();
                        ((EAModelImplementation)this._model).Repository.AdviseElementChange(this._element.ElementID);
                        return;
                    }
                }

                // Element tag not found, create new one if instructed to do so...
                if (createIfNotExist)
                {
                    var newTag = this._element.TaggedValues.AddNew(tagName, "TaggedValue") as TaggedValue;
                    newTag.Value = tagValue;
                    newTag.Update();
                    this._element.TaggedValues.Refresh();
                    ((EAModelImplementation)this._model).Repository.AdviseElementChange(this._element.ElementID);
                }
            }
            catch { /* & ignore all errors */ }
        }
    }
}
