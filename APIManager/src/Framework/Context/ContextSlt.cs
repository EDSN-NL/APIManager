using System.Collections.Generic;
using Framework.Logging;
using Framework.Model;
using Framework.View;
using Framework.Exceptions;

namespace Framework.Context
{
    // ContextScope specifies what type of (model) artifact currently has focus:
    internal enum ContextScope { Class, Package, Diagram, Attribute, Connector, None, Unknown };

    // Indicator for TransformRTF that indicates whether we want to transform to- or from RTF:
    internal enum RTFDirection { ToRTF, FromRTF }

    /// <summary>
    /// This implements the Context Singleton interface class. Each context must have a, platform-dependent, implementation object
    /// that will manage the actual context.
    /// </summary>
    internal sealed class ContextSlt
    {
        // This is the actual Context singleton. It is created automatically on first load.
        private static readonly ContextSlt _contextSlt = new ContextSlt();

        // And here is the internal link to the implementation object that does all the hard work...
        private ContextImplementation _contextImp;

        /// <summary>
        /// Returns the class artifact that is currently in scope (has user focus).
        /// </summary>
        /// <returns>Class that is in focus or NULL if no class is in focus.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal MEClass CurrentClass
        {
            get
            {
                if (this._contextImp != null) return this._contextImp.CurrentClass;
                else throw new MissingImplementationException("ContextImplementation");
            }
        }

        /// <summary>
        /// Returns the diagram that is currently in scope (has user focus).
        /// </summary>
        /// <returns>Diagram that is in focus or NULL if no context implementation object exists or no diagram is in focus.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal Diagram CurrentDiagram
        {
            get
            {
                if (this._contextImp != null) return this._contextImp.CurrentDiagram;
                else throw new MissingImplementationException("ContextImplementation");
            }
        }

        /// <summary>
        /// Returns the package artifact that is currently in scope (has user focus).
        /// </summary>
        /// <returns>Package that is in focus or NULL if no context implementation object exists or no package is in focus.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal MEPackage CurrentPackage
        {
            get
            {
                if (this._contextImp != null) return this._contextImp.CurrentPackage;
                else throw new MissingImplementationException("ContextImplementation");
            }
        }

        /// <summary>
        /// This method removes all existing dynamic context.
        /// </summary>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal void Flush()
        {
            if (this._contextImp != null) this._contextImp.Flush();
            else throw new MissingImplementationException("ContextImplementation");
        }

        /// <summary>
        /// Commits a transaction on a set of Framework updates, MUST be called to complete transaction!
        /// </summary>
        /// <exception cref="MissingImplementationException">No FrameworkSettings object has been loaded (yet).</exception>
        internal void FrameworkCommit()
        {
            if (this._contextImp != null) this._contextImp.FrameworkCommit();
            else throw new MissingImplementationException("ContextImplementation");
        }

        /// <summary>
        /// Initiates batch-update on framework settings. MUST be followed by FrameworkCommit otherwise all changes are lost!
        /// </summary>
        /// <exception cref="MissingImplementationException">No FrameworkSettings object has been loaded (yet).</exception>
        internal void FrameworkStartTransaction()
        {
            if (this._contextImp != null) this._contextImp.FrameworkStartTransaction();
            else throw new MissingImplementationException("ContextImplementation");
        }

        /// <summary>
        /// Returns the ModelElement (Package or Class) that is currently in focus. 
        /// </summary>
        /// <returns>MEPackage or MEClass that is currently in focus or NULL on errors or nothing to report.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal ModelElement GetActiveElement()
        {
            if (this._contextImp != null) return this._contextImp.GetActiveElement();
            else throw new MissingImplementationException("ContextImplementation");
        }

        /// <summary>
        /// Public Context "factory" method. Simply returns the static instance.
        /// </summary>
        /// <returns>Context singleton object</returns>
        internal static ContextSlt GetContextSlt() { return _contextSlt; }

        /// <summary>
        /// Retrieves the modelling class, diagram or package that is currently in scope (has user focus). 
        /// When a reference to an itemID is provided, the method also indirectly returns the identifier of the actual item.
        /// </summary>
        /// <param name="itemID">Receives the itemID.</param>
        /// <returns>Scope type enumeration.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal ContextScope GetCurrentScope(out int itemID, out string uniqueID)
        {
            if (this._contextImp != null) return this._contextImp.GetCurrentScope(out itemID, out uniqueID);
            else throw new MissingImplementationException("ContextImplementation");
        }

        /// <summary>
        /// Retrieve string setting by name.
        /// </summary>
        /// <param name="name">Name of the setting to retrieve.</param>
        /// <returns>Value of setting.</returns>
        /// <exception cref="KeyNotFoundException">Specified name does not exist.</exception>
        /// <exception cref="MissingImplementationException">No implementation object exists. or no FrameworkSettings object has been loaded (yet).</exception>
        internal bool GetBoolSetting(string name)
        {
            if (this._contextImp != null) return this._contextImp.GetBoolSetting(name);
            else throw new MissingImplementationException("ContextImplementation");
        }

        /// <summary>
        /// Function to retrieve a configuration property by name.
        /// </summary>
        /// <param name="propertyName">Name of property to retrieve.</param>
        /// <returns>Property value or empty string if not found.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal string GetConfigProperty(string propertyName)
        {
            if (this._contextImp != null) return this._contextImp.GetConfigProperty(propertyName);
            else throw new MissingImplementationException("ContextImplementation");
        }

        /// <summary>
        /// Function to retrieve a list of all properties that share the same key.
        /// </summary>
        /// <param name="propertyName">Name of properties to retrieve.</param>
        /// <returns>List of property values or empty list if nothing found.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal List<string> GetConfigPropertyList(string propertyName)
        {
            if (this._contextImp != null) return this._contextImp.GetConfigPropertyList(propertyName);
            else throw new MissingImplementationException("ContextImplementation");
        }

        /// <summary>
        /// Returns the application-defined string-type resource with given name.
        /// </summary>
        /// <param name="name">Resource name to retrieve.</param>
        /// <returns>Resource object or NULL if name does not exist.</returns>
        /// <exception cref="MissingImplementationException">No implementation object or no FrameworkSettings object has been loaded (yet).</exception>
        internal string GetResourceString(string name)
        {
            if (this._contextImp != null) return this._contextImp.GetResourceString(name);
            else throw new MissingImplementationException("ContextImplementation");
        }

        /// <summary>
        /// Retrieve string setting by name. This is a pass-through to the 'FrameworkSettings' component.
        /// </summary>
        /// <param name="name">Name of the setting to retrieve.</param>
        /// <returns>Value of setting.</returns>
        /// <exception cref="KeyNotFoundException">Specified name does not exist.</exception>
        /// <exception cref="MissingImplementationException">No implementation object exists or no FrameworkSettings object has been loaded (yet).</exception>
        internal string GetStringSetting(string name)
        {
            if (this._contextImp != null) return this._contextImp.GetStringSetting(name);
            else throw new MissingImplementationException("ContextImplementation");
        }

        /// <summary>
        /// This method is called during startup of the plugin and must initialize all context-specific stuff.
        /// The method is called from the ControllerSlt during its initialization cycle.
        /// <param name="imp">The implementation object to be used.</param>
        /// </summary>
        internal void Initialize(ContextImplementation imp)
        {
            this._contextImp = imp;
            if (this._contextImp != null) this._contextImp.Initialize();
            else Logger.WriteWarning("Framework.Context.ContextSlt.initialize >> Initialized with empty implementation!");
        }

        /// <summary>
        /// Forces a class-selection picker dialog that facilitates selection of a specific class from the
        /// repository.
        /// If the stereotypes parameter is specified, the class to be selected must have one or more of the
        /// stereotypes presented in this list.
        /// <param name="stereotypes">Optional list of mandatory stereotypes.</param>
        /// </summary>
        /// <returns>Selected class or NULL in case of errors / cancel.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal MEClass SelectClass(List<string> stereotypes = null)
        {
            if (this._contextImp != null) return this._contextImp.SelectClass(stereotypes);
            else throw new MissingImplementationException("ContextImplementation");
        }

        /// <summary>
        /// Forces a class-selection picker dialog that facilitates selection of a specific data type from the
        /// repository.
        /// If the isEnumeration parameter is set to true, the method only facilitates selection of enumerated
        /// types and thus also returns an MEEnumeratedType, which is a specialization of MEDataType.
        /// <param name="isEnumeration">When selected, we can select enumeration types.</param>
        /// </summary>
        /// <returns>Selected data type or NULL in case of errors / cancel.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal MEDataType SelectDataType(bool isEnumeration = false)
        {
            if (this._contextImp != null) return this._contextImp.SelectDataType(isEnumeration);
            else throw new MissingImplementationException("ContextImplementation");
        }

        /// <summary>
        /// Forces an object-selection picker dialog that facilitates selection of a specific identifier
        /// object from the Identifiers section of the repository.
        /// </summary>
        /// <returns>Selected identifier object or NULL in case of errors / cancel.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal MEObject SelectIdentifier()
        {
            if (this._contextImp != null) return this._contextImp.SelectIdentifier();
            else throw new MissingImplementationException("ContextImplementation");
        }

        /// <summary>
        /// Forces a package-selection picker dialog that facilitates selection of a specific package from the
        /// repository.
        /// If the stereotypes parameter is specified, the pacakge to be selected must have one or more of the
        /// stereotypes presented in this list.
        /// <param name="stereotypes">Optional list of mandatory stereotypes.</param>
        /// </summary>
        /// <returns>Selected package or NULL in case of errors / cancel.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal MEPackage SelectPackage(List<string> stereotypes = null)
        {
            if (this._contextImp != null) return this._contextImp.SelectPackage(stereotypes);
            else throw new MissingImplementationException("ContextImplementation");
        }

        /// <summary>
        /// Update setting by name, writing the new value both to memory as well as persistent storage.
        /// </summary>
        /// <param name="name">Name of the setting to update.</param>
        /// <param name="value">New value of the setting.</param>
        /// <exception cref="KeyNotFoundException">Specified name does not exist.</exception>
        /// <exception cref="MissingImplementationException">No implementation object exists. or no FrameworkSettings object has been loaded (yet).</exception>
        internal void SetBoolSetting(string name, bool value)
        {
            if (this._contextImp != null) this._contextImp.SetBoolSetting(name, value);
            else throw new MissingImplementationException("ContextImplementation");
        }

        /// <summary>
        /// Function to set a configuration property.
        /// </summary>
        /// <param name="propertyName">Name of property to set.</param>
        /// <param name="propertyValue">Value of property.</param>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal void SetConfigProperty(string propertyName, string propertyValue)
        {
            if (this._contextImp != null) this._contextImp.SetConfigProperty(propertyName, propertyValue);
            else throw new MissingImplementationException("ContextImplementation");
        }

        /// <summary>
        /// The method instructs the implementation to switch to a new logfile with specified name. The method silently fails when no
        /// implementation is present.
        /// If the new filename is NULL or empty, existing logging will be closed.
        /// </summary>
        /// <param name="fileName">New file to be used for logging.</param>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal void SetLogfile(string fileName)
        {
            if (this._contextImp != null) this._contextImp.SetLogfile(fileName);
            else throw new MissingImplementationException("ContextImplementation");
        }

        /// <summary>
        /// Update setting by name, writing the new value both to memory as well as persistent storage.
        /// </summary>
        /// <param name="name">Name of the setting to update.</param>
        /// <param name="value">New value of the setting.</param>
        /// <exception cref="KeyNotFoundException">Specified name does not exist.</exception>
        /// <exception cref="MissingImplementationException">No implementation object exists. or no FrameworkSettings object has been loaded (yet).</exception>
        internal void SetStringSetting(string name, string value)
        {
            if (this._contextImp != null) this._contextImp.SetStringSetting(name, value);
            else throw new MissingImplementationException("ContextImplementation");
        }

        /// <summary>
        /// This method is called during plugin shutdown and must release resources...
        /// The method is called from the ControllerSlt during its shut-down cycle.
        /// </summary>
        internal void ShutDown()
        {
            if (this._contextImp != null) this._contextImp.ShutDown();
            this._contextImp = null;
        }

        /// <summary>
        /// Called when the user has clicked on a modelling element, a diagram or a package. The method is used to pass
        /// this information to the context implementation so that the current scope can be updated. 
        /// </summary>
        /// <param name="newScope">The type of element that has received focus.</param>
        /// <param name="itemID">Identification of that element within the tool repository (tool-dependent format)</param>
        /// <param name="uniqueID">Globally unique identifier of the item.</param>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal void SwitchScope (ContextScope newScope, int itemID, string uniqueID)
        {
            if (this._contextImp != null) this._contextImp.SwitchScope(newScope, itemID, uniqueID);
            else throw new MissingImplementationException("ContextImplementation");
        }

        /// <summary>
        /// Utility function that is used to transform RTF-formatted text to- and from repository-specific format.
        /// Depending on the 'toRTF' parameter, the function either accepts repository format and returns RTF, or the other way
        /// around.
        /// </summary>
        /// <param name="inText">The text to be transformed.</param>
        /// <param name="direction">Set to 'ToRTF' to transform repository to RTF, 'FromRTF' for the other way around.</param>
        /// <returns>Transformed text.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal string TransformRTF(string inText, RTFDirection direction)
        {
            if (this._contextImp != null) return this._contextImp.TransformRTF(inText, (direction == RTFDirection.ToRTF) ? true : false);
            else throw new MissingImplementationException("ContextImplementation");
        }

        /// <summary>
        /// The private constructor is called once on initial load and assures that exactly one valid object is present at all times.
        /// </summary>
        private ContextSlt()
        {
            this._contextImp = null;     // No implementation for now.
        }
    }
}
