using System;
using System.Collections.Generic;
using Framework.Model;
using Framework.Logging;
using Framework.Exceptions;

namespace Framework.Context
{
    internal abstract class ContextImplementation: IDisposable
    {
        protected Configuration _configuration;   // Contains the configuration object, provides access the plugin configuration file.
        protected View.Diagram _currentDiagram;   // Currently selected diagram.
        protected MEClass _currentClass;          // Currently selected class.
        protected MEPackage _currentPackage;      // Currently selected package.
        protected ContextScope _lastScope;        // Last-received scope change.
        protected int _lastItemID;                // The identifier of the item associated with last scope change.
        protected string _lastUniqueID;           // The globally unique identifier of the item associated with last scope change.

        private FileLogger _fileLogger;           // Default file log channel.
        private FrameworkSettings _settings;      // User-specific configuration settings.

        // Rerturns a copy of the last-selected object of the given type. By returning a copy, we can properly manage the instances 
        // without need to worry about when to dispose an interface. It is now the responsibility of the consumer to properly dispose
        // of the received copy.
        internal MEClass CurrentClass        { get { return (this._currentClass != null) ? new MEClass(this._currentClass) : null; } }
        internal View.Diagram CurrentDiagram { get { return (this._currentDiagram != null) ? new View.Diagram(this._currentDiagram) : null; } }
        internal MEPackage CurrentPackage    { get { return (this._currentPackage != null) ? new MEPackage(this._currentPackage) : null; } }

        /// <summary>
        /// We have to implement this since the class manages ModelElements, which are disposable objects.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// This method removes the current class and current package from the context.
        /// </summary>
        internal virtual void Flush()
        {
            this._currentClass = null;
            this._currentPackage = null;
        }

        /// <summary>
        /// Commits a transaction on a set of Framework updates, MUST be called to complete transaction!
        /// </summary>
        /// <exception cref="MissingImplementationException">No FrameworkSettings object has been loaded (yet).</exception>
        internal void FrameworkCommit()
        {
            if (this._settings != null) this._settings.Commit();
            else throw new MissingImplementationException("FrameworkSettings");
        }

        /// <summary>
        /// Initiates batch-update on framework settings. MUST be followed by FrameworkCommit otherwise all changes are lost!
        /// </summary>
        /// <exception cref="MissingImplementationException">No FrameworkSettings object has been loaded (yet).</exception>
        internal void FrameworkStartTransaction()
        {
            if (this._settings != null) this._settings.StartTransaction();
            else throw new MissingImplementationException("FrameworkSettings");
        }

        /// <summary>
        /// This method retrieves the model element that is currently in focus within the tool repository. E.g. it is the last element that has
        /// received user focus.
        /// </summary>
        /// <returns>The ModelElement that is currently in focus or NULL on errors or missing focus.</returns>
        internal abstract ModelElement GetActiveElement();

        /// <summary>
        /// Function to retrieve a configuration property by name.
        /// </summary>
        /// <param name="propertyName">Name of property to retrieve.</param>
        /// <returns>Property value or empty string if not found.</returns>
        internal string GetConfigProperty(string propertyName) { return _configuration.GetProperty(propertyName); }

        /// <summary>
        /// Function to retrieve a list of all properties that share the same key.
        /// </summary>
        /// <param name="propertyName">Name of properties to retrieve.</param>
        /// <returns>List of property values or empty list if nothing found.</returns>
        internal List<string> GetConfigPropertyList(string propertyName) { return _configuration.GetPropertyList(propertyName); }

        /// <summary>
        /// Returns the most recently received scope-change type.
        /// </summary>
        /// <param name="itemID">Will receive the (repository specific) item identifier.</param>
        /// <param name="uniqueID">Will receive the (respository independent) unique item identifier.</param>
        /// <returns>Most recently received scope-change type.</returns>
        internal ContextScope GetCurrentScope(out int itemID, out string uniqueID)
        {
            itemID = this._lastItemID;
            uniqueID = this._lastUniqueID;
            return this._lastScope;
        }

        /// <summary>
        /// Returns the application-defined string-type resource with given name.
        /// </summary>
        /// <param name="name">Resource name to retrieve.</param>
        /// <returns>Resource object or NULL if name does not exist.</returns>
        /// <exception cref="MissingImplementationException">No FrameworkSettings object has been loaded (yet).</exception>
        internal string GetResourceString(string name)
        {
            if (this._settings != null) return this._settings.GetResourceString(name);
            else throw new MissingImplementationException("FrameworkSettings");
        }

        /// <summary>
        /// Retrieve string setting by name. This is a pass-through to the 'FrameworkSettings' component.
        /// </summary>
        /// <param name="name">Name of the setting to retrieve.</param>
        /// <param name="isEncrypted">Set to 'true' if setting must be decrypted before return.</param>
        /// <returns>Value of setting.</returns>
        /// <exception cref="KeyNotFoundException">Specified name does not exist.</exception>
        /// <exception cref="MissingImplementationException">No FrameworkSettings object has been loaded (yet).</exception>
        internal string GetStringSetting(string name, bool isEncrypted)
        {
            if (this._settings != null) return this._settings.GetStringSetting(name, isEncrypted);
            else throw new MissingImplementationException("FrameworkSettings");
        }

        /// <summary>
        /// Retrieve string setting by name.
        /// </summary>
        /// <param name="name">Name of the setting to retrieve.</param>
        /// <returns>Value of setting.</returns>
        /// <exception cref="KeyNotFoundException">Specified name does not exist.</exception>
        /// <exception cref="MissingImplementationException">No FrameworkSettings object has been loaded (yet).</exception>
        internal bool GetBoolSetting(string name)
        {
            if (this._settings != null) return this._settings.GetBoolSetting(name);
            else throw new MissingImplementationException("FrameworkSettings");
        }

        /// <summary>
        /// This method is called by the Context interface after binding with the implementation and should be used to initialize
        /// local resources.
        /// </summary>
        internal virtual void Initialize()
        {
            this._settings = new FrameworkSettings();
            this._configuration = new Configuration();
            this._currentDiagram = null;
            this._currentClass = null;
            this._currentPackage = null;
            this._lastItemID = -1;
            this._lastUniqueID = string.Empty;
            this._lastScope = ContextScope.None;

            try
            {
                Logger.Close(); // Just in case we call this on an open logger.
                if (this._settings.GetBoolSetting(FrameworkSettings._UseLogFile))
                {
                    this._fileLogger = new FileLogger(this._settings.GetStringSetting(FrameworkSettings._LogFileName));
                    Logger.RegisterLog(this._fileLogger, "FileLogger", true, true, true);
                }
                InitializeLog();    // Make sure that derived class(es) have a chance to register their loggers.
                Logger.Open();      // Start logging.
                Logger.WriteInfo("Framework.Context.ContextImplementation.initialize >> Logging initiated!");
            }
            catch { }    // Catch and ignore any errors for now.
        }

        /// <summary>
        /// Forces a class-selection picker dialog that facilitates selection of a specific class from the
        /// repository.
        /// If the stereotypes parameter is specified, the class to be selected must have one or more of the
        /// stereotypes presented in this list.
        /// <param name="stereotypes">Optional list of mandatory stereotypes.</param>
        /// </summary>
        /// <returns>Selected class or NULL in case of errors / cancel.</returns>
        internal abstract MEClass SelectClass(List<string> stereotypes);

        /// <summary>
        /// Forces a type-selection picker dialog that facilitates selection of a specific data type from the
        /// repository.
        /// If the isEnumeration parameter is set to 'true', the method selects enumerated types instead of
        /// 'standard' data types and thus also return an MEEnumeratedType, which is a specialization of MEDataType.
        /// <param name="isEnumeration">Set to true in order to select enumerations.</param>
        /// </summary>
        /// <returns>Selected data type or NULL in case of errors / cancel.</returns>
        internal abstract MEDataType SelectDataType(bool isEnumeration);

        /// <summary>
        /// Forces an object-selection picker dialog that facilitates selection of a specific identifier
        /// object from the Identifiers section of the repository.
        /// </summary>
        /// <returns>Selected identifier object or NULL in case of errors / cancel.</returns>
        internal abstract MEObject SelectIdentifier();

        /// <summary>
        /// Forces a package-selection picker dialog that facilitates selection of a specific package from the
        /// repository.
        /// If the stereotypes parameter is specified, the pacakge to be selected must have one or more of the
        /// stereotypes presented in this list.
        /// <param name="stereotypes">Optional list of mandatory stereotypes.</param>
        /// </summary>
        /// <returns>Selected package or NULL in case of errors / cancel.</returns>
        internal abstract MEPackage SelectPackage(List<string> stereotypes);

        /// <summary>
        /// Update setting by name, writing the new value both to memory as well as persistent storage.
        /// </summary>
        /// <param name="name">Name of the setting to update.</param>
        /// <param name="value">New value of the setting.</param>
        /// <exception cref="KeyNotFoundException">Specified name does not exist.</exception>
        /// <exception cref="MissingImplementationException">No FrameworkSettings object has been loaded (yet).</exception>
        internal void SetBoolSetting(string name, bool value)
        {
            if (this._settings != null) this._settings.SetBoolSetting(name, value);
            else throw new MissingImplementationException("FrameworkSettings");
        }

        /// <summary>
        /// Function to set a configuration property.
        /// </summary>
        /// <param name="propertyName">Name of property to set.</param>
        /// <param name="propertyValue">Value of property.</param>
        internal void SetConfigProperty(string propertyName, string propertyValue)
        {
            if (_configuration != null) _configuration.SetProperty(propertyName, propertyValue);
        }

        /// <summary>
        /// This method is used to change the file used for logging. The existing logger is closed (if it was open) and if the
        /// fileName parameter has valid contents, a new logfile is created using the specified name.
        /// If the new filename is not an empty string, it is also copied to the application settings so it will be used next time.
        /// </summary>
        /// <param name="fileName">File to be used for logging.</param>
        internal void SetLogfile(string fileName)
        {
            if (this._fileLogger == null) this._fileLogger = new FileLogger(fileName);
            else this._fileLogger.SetFileName(fileName);
            if (fileName != string.Empty) this._settings.SetStringSetting(FrameworkSettings._LogFileName, fileName);
        }

        /// <summary>
        /// Update setting by name, writing the new value both to memory as well as persistent storage.
        /// </summary>
        /// <param name="name">Name of the setting to update.</param>
        /// <param name="value">New value of the setting.</param>
        /// <param name="mustEncrypt">Set to 'true' in case the setting must be encrypted before storage.</param>
        /// <exception cref="KeyNotFoundException">Specified name does not exist.</exception>
        /// <exception cref="MissingImplementationException">No FrameworkSettings object has been loaded (yet).</exception>
        internal void SetStringSetting(string name, string value, bool mustEncrypt)
        {
            if (this._settings != null) this._settings.SetStringSetting(name, value, mustEncrypt);
            else throw new MissingImplementationException("FrameworkSettings");
        }

        /// <summary>
        /// This method is called during plugin shutdown and must release resources...
        /// The method is called from the Context interface during its shut-down cycle. It might be overriden by specializations.
        /// </summary>
        internal virtual void ShutDown()
        {
            Logger.WriteInfo("Framework.Context.ContextImplementation.shutDown >> Shutting down...");
            if (this._currentPackage != null) this._currentPackage.Dispose();
            if (this._currentClass != null) this.CurrentClass.Dispose();
            if (this._currentDiagram != null) this._currentDiagram.Dispose();
            this._currentDiagram = null;
            this._currentClass = null;
            this._currentPackage = null;
            this._lastItemID = -1;
            this._lastUniqueID = string.Empty;
            this._configuration = null;
            Logger.WriteInfo("Framework.Context.ContextImplementation.shutDown >> Stopping the log...");
            Logger.Close();
            this._settings = null;
        }

        /// <summary>
        /// This method is invoked whenever the user has selected a particular item in the tool GUI. The method updates the current
        /// scope to match the selected item. 
        /// </summary>
        /// <param name="newScope">The new scope, defines which type of item has been selected.</param>
        /// <param name="itemID">Repository-unique identifier of the item.</param>
        /// <param name="uniqueID">Globally unique identifier of the item.</param>
        internal virtual void SwitchScope(ContextScope newScope, int itemID, string uniqueID)
        {
            Logger.WriteInfo("Framework.Context.ContextImplementation.SwitchScope >> newScope = '" + 
                              newScope + "', itemID = '" + itemID + "' and uniqueID = '" + uniqueID + "'...");
            switch (newScope)
            {
                // No additional actions required for these (at the moment)...
                case ContextScope.Attribute:
                case ContextScope.Connector:
                    this._lastScope = newScope;
                    this._lastItemID = itemID;
                    this._lastUniqueID = uniqueID;
                    break;

                case ContextScope.Class:
                    if (this._currentClass == null || this._currentClass.ElementID != itemID)
                    {
                        if (this._currentClass != null) this._currentClass.Dispose();
                        this._currentClass = new MEClass(itemID);
                        this._lastItemID = itemID;
                        this._lastUniqueID = uniqueID;
                    }
                    this._lastScope = newScope;
                    break;

                case ContextScope.Package:
                    if (this._currentPackage == null || this._currentPackage.ElementID != itemID)
                    {
                        if (this._currentPackage != null) this._currentPackage.Dispose();
                        this._currentPackage = new MEPackage(itemID);
                        this._lastItemID = itemID;
                        this._lastUniqueID = uniqueID;
                    }
                    this._lastScope = newScope;
                    break;

                case ContextScope.Diagram:
                    if (this._currentDiagram == null || this._currentDiagram.DiagramID != itemID)
                    {
                        if (this._currentDiagram != null) this._currentDiagram.Dispose();
                        this._currentDiagram = new View.Diagram(itemID);
                        this._lastItemID = itemID;
                        this._lastUniqueID = uniqueID;
                    }
                    this._lastScope = newScope;
                    break;

                default:
                    Logger.WriteError("Framework.Context.ContextImplementation.switchScope >> Unsupported change, ignore!");
                    break;
            }
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
        internal abstract string TransformRTF(string inText, bool toRTF);

        /// <summary>
        /// Constructor initializes all local data to NULL. Actual initialization is performed by the 'initialize' method!
        /// </summary>
        protected ContextImplementation()
        {
            this._configuration = null;
            this._currentDiagram = null;
            this._currentClass = null;
            this._currentPackage = null;
            this._lastItemID = -1;
            this._lastUniqueID = string.Empty;
            this._lastScope = ContextScope.None;
            this._fileLogger = null;
            this._settings = null;
        }

        /// <summary>
        /// Called on exit and used to explicitly dispose of my model elements.
        /// </summary>
        /// <param name="disposing">True when called from Dispose method.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._currentClass != null) this._currentClass.Dispose();
                if (this._currentPackage != null) this._currentPackage.Dispose();
                if (this._currentDiagram != null) this._currentDiagram.Dispose();
                if (this._fileLogger != null) this._fileLogger.Dispose();
            }
        }

        /// <summary>
        /// Implement this in derived classes to ensure that the logger is created at the correct moment!
        /// </summary>
        protected abstract void InitializeLog();
    }
}
