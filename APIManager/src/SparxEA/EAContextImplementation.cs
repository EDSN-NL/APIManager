using System;
using System.Collections.Generic;
using EA;
using Framework.Context;
using Framework.Logging;
using Framework.Model;
using Framework.Exceptions;
using SparxEA.Logging;

namespace SparxEA.Context
{
    internal sealed class EAContextImplementation: ContextImplementation
    {
        private EA.Repository _repository;      // Active EA repository.
        string _lastActiveObjectGUID;
        ModelElement _lastModelElement;

        // Configuration properties used by this module:
        private const string _SystemOutputTabName   = "SystemOutputTabName";
        private const string _IdentifiersPkgPath    = "IdentifiersPkgPath";

        /// <summary>
        /// Getter that returns the repository instance to interested parties
        /// </summary>
        internal Repository Repository {get {return this._repository; }}

        /// <summary>
        /// The constructor loads the EA Repository reference and properly initialized local context.
        /// </summary>
        internal EAContextImplementation(Repository repository) : base()
        {
            this._repository = repository;
            this._lastActiveObjectGUID = string.Empty;
            this._lastModelElement = null;
        }

        /// <summary>
        /// This method is called when the context is being disconnected from the bridge interface.
        /// It has to cleanup state and release resources.
        /// </summary>
        internal override void ShutDown()
        {
            this._repository = null;
            base.ShutDown();
        }

        /// <summary>
        /// The method checks the EA repository for the object that is currently in focus. If this is a diagram, we return the owning package.
        /// If a Package or an Element is in focus, the associated ModelElement is returned. If the focus is on an attribute, we return the
        /// owning Class.
        /// In all other cases, the method returns NULL.
        /// </summary>
        /// <returns>ModelElement that is currently in focus or NULL if there is nothing valid to return.</returns>
        internal override ModelElement GetActiveElement()
        {
            Object currentActiveObject = this._repository.GetContextObject();
            ObjectType activeObjectType = this._repository.GetContextItemType();
            ModelElement element = null;

            try
            {
                switch (activeObjectType)
                {
                    case ObjectType.otDiagram:
                        //If we're on a diagram, fetch the owning package and return this...
                        var currentDiagram = currentActiveObject as EA.Diagram;
                        element = (currentDiagram.DiagramGUID == this._lastActiveObjectGUID) ? this._lastModelElement : new MEPackage(currentDiagram.PackageID);
                        this._lastActiveObjectGUID = currentDiagram.DiagramGUID;
                        break;

                    case ObjectType.otPackage:
                        var currentPackage = currentActiveObject as EA.Package;
                        element = (currentPackage.PackageGUID == this._lastActiveObjectGUID) ? this._lastModelElement : new MEPackage(currentPackage.PackageID);
                        this._lastActiveObjectGUID = currentPackage.PackageGUID;
                        break;

                    case ObjectType.otElement:
                        var currentElement = currentActiveObject as EA.Element;
                        element = (currentElement.ElementGUID == this._lastActiveObjectGUID) ? this._lastModelElement : new MEClass(currentElement.ElementID);
                        this._lastActiveObjectGUID = currentElement.ElementGUID;
                        break;

                    case ObjectType.otAttribute:
                        // In this case, we return the class in which the attribute is located.
                        var currentAttribute = currentActiveObject as EA.Attribute;
                        element = (currentAttribute.AttributeGUID == this._lastActiveObjectGUID) ? this._lastModelElement : new MEClass(currentAttribute.ParentID);
                        this._lastActiveObjectGUID = currentAttribute.AttributeGUID;
                        break;

                    default:
                        Logger.WriteInfo("SparxEA.Context.EAContextImplementation.getActiveElement >> Unknown active object: '" + activeObjectType + "'!");
                        element = null;
                        this._lastActiveObjectGUID = string.Empty;
                        break;
                }
                this._lastModelElement = element;
            }
            catch (System.NullReferenceException)
            {
                Logger.WriteInfo("SparxEA.Context.EAContextImplementation.getActiveElement >> No valid element in focus!");
                this._lastModelElement = null;
                this._lastActiveObjectGUID = string.Empty;
            }
            return element;
        }

        /// <summary>
        /// Displays a picker dialog that facilitates selection of a class from the repository. Returns the selected
        /// class as an MEClass object.
        /// If the 'stereotypes' parameter is included, the class to be selected must have one or more stereotypes
        /// from this list.
        /// </summary>
        /// <param name="stereotypes">An optional list of stereotypes.</param>
        /// <returns>selected MEClass or NULL in case of errors.</returns>
        internal override MEClass SelectClass(List<string> stereotypes)
        {
            Logger.WriteInfo("SparxEA.Context.EAContextImplementation.selectClass >> Going to pick a class...");
            // Because of a bug in EA, we can not use the stereotype filter: the filter seems to recognize stereotypes only if they
            // include the profile name. But since out profile contains a space, it is not recognized at all!
            string filter = string.Empty;
            if (stereotypes != null && stereotypes.Count > 0)
            {
                bool firstOne = true;
                //string st;
                foreach (string stereotype in stereotypes)
                {
                    //st = (stereotype.Contains("::")) ? stereotype.Substring(stereotype.IndexOf("::") + 2) : stereotype;
                    filter += firstOne? stereotype : "," + stereotype;
                    firstOne = false;
                }
            }
            string selection = "IncludedTypes=Class";
            if (filter != string.Empty) selection += ";StereoType=" + filter;
            Logger.WriteInfo("SparxEA.Context.EAContextImplementation.selectClass >> Filter: " + selection);

            int classID = this._repository.InvokeConstructPicker(selection);
            if (classID != 0)
            {
                MEClass selectedClass = new MEClass(classID);
                return stereotypes == null || selectedClass.HasStereotype(stereotypes) ? selectedClass: null;
            }
            return null;
        }

        /// <summary>
        /// Forces a type-selection picker dialog that facilitates selection of a specific data type from the
        /// repository.
        /// If the isEnumeration parameter is set to 'true', the method selects enumerated types instead of
        /// 'standard' data types.
        /// <param name="isEnumeration">Set to true in order to select enumerations.</param>
        /// </summary>
        /// <returns>Selected data type or NULL in case of errors / cancel.</returns>
        internal override MEDataType SelectDataType(bool isEnumeration)
        {
            Logger.WriteInfo("SparxEA.Context.EAContextImplementation.selectDataType >> Going to pick a data type...");
            string filter = "IncludedTypes=";
            filter += (isEnumeration) ? "Enumeration" : "DataType";

            Logger.WriteInfo("SparxEA.Context.EAContextImplementation.selectDataType >> Filter: " + filter);

            int classID = this._repository.InvokeConstructPicker(filter);
            if (classID != 0)
            {
                return (isEnumeration) ? new MEEnumeratedType(classID) : new MEDataType(classID);
            }
            return null;
        }

        /// <summary>
        /// Forces an object-selection picker dialog that facilitates selection of a specific identifier
        /// object from the Identifiers section of the repository.
        /// </summary>
        /// <returns>Selected identifier object or NULL in case of errors / cancel.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal override MEObject SelectIdentifier()
        {
            Logger.WriteInfo("SparxEA.Context.EAContextImplementation.selectIdentifier >> Going to pick an identifier object...");
			const string filter = "IncludedTypes=Object";
            ContextSlt context = ContextSlt.GetContextSlt();
            string identifiersRootPath = context.GetConfigProperty(_IdentifiersPkgPath);

            int classID = this._repository.InvokeConstructPicker(filter);
            return (classID != 0)? new MEObject(classID): null;
        }

        /// <summary>
        /// Displays a picker dialog that facilitates selection of a package from the repository. Returns the selected
        /// package as an MEPackage object.
        /// If the 'stereotypes' parameter is included, the package to be selected must have one or more stereotypes
        /// from this list.
        /// </summary>
        /// <param name="stereotypes">An optional list of stereotypes.</param>
        /// <returns>selected MEPackage or NULL in case of errors.</returns>
        internal override MEPackage SelectPackage(List<string> stereotypes)
        {
            Logger.WriteInfo("SparxEA.Context.EAContextImplementation.selectPackage >> Going to pick a package...");
            string filter = string.Empty;
            if (stereotypes != null && stereotypes.Count > 0)
            {
                bool firstOne = true;
                string st;
                foreach (string stereotype in stereotypes)
                {
                    st = (stereotype.Contains("::")) ? stereotype.Substring(stereotype.IndexOf("::") + 2) : stereotype;
                    filter += firstOne ? st : "," + st;
                    firstOne = false;
                }
            }
            string selection = "IncludedTypes=Package";
            if (filter != string.Empty) selection += ";StereoType=" + filter;
            Logger.WriteInfo("SparxEA.Context.EAContextImplementation.selectPackage >> Filter: " + selection);

            int packageID = this._repository.InvokeConstructPicker(selection);
            return (packageID != 0) ? new MEPackage(packageID) : null;
        }

        /// <summary>
        /// Utility function that is used to transform RTF-formatted text to- and from repository-specific format.
        /// Depending on the 'toRTF' parameter, the function either accepts repository format and returns RTF, or the other way
        /// around.
        /// </summary>
        /// <param name="inText">The text to be transformed.</param>
        /// <param name="toRTF">Set to 'true' to transform repository TO RTF, 'false' for the other way around.</param>
        /// <returns>Transformed text.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal override string TransformRTF(string inText, bool toRTF)
        {
            string result = string.Empty;
            if (inText.Length > 10) // Arbitrary length check to assure that there is at least some contents in this string...
            {
                result = (toRTF) ? this._repository.GetFormatFromField("RTF", inText) : this._repository.GetFieldFromFormat("RTF", inText);
            }
            return result;
        }

        /// <summary>
        /// This method is called from the base class when it's time to create a local log (if any).
        /// </summary>
        protected override void InitializeLog()
        {
            string tabName = this._configuration.GetProperty(_SystemOutputTabName);

            // Register this logger for warning and error only.
            if (!string.IsNullOrEmpty(tabName)) Logger.RegisterLog(new EALogger(this._repository, tabName), "EALogger", false, true, true);
        }
    }
}
