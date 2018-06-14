﻿using System;
using System.Collections.Generic;
using APIManager.SparxEA.Properties;        // Addresses the "settings" environment so we can retrieve run-time settings.
using Framework.Util;
using Framework.Logging;

namespace Framework.Context
{
    /// <summary>
    /// Provides an interface to .Net specific configuration settings. We prefer to centralize in order to keep changes to these
    /// settings local to one single module.
    /// </summary>
    internal class FrameworkSettings
    {
        // REST Authentication schemes...
        internal const string _RESTAuthSchemeAPIKey           = "APIKey";
        internal const string _RESTAuthSchemeBasic            = "Basic";
        internal const string _RESTAuthSchemeNone             = "None";
        internal const string _RESTAuthSchemeOAuth2           = "OAuth2";

        // REST Default Interface settings...
        internal const string _RESTHostName                   = "RESTHostName";
        internal const string _RESTSchemes                    = "RESTSchemes";

        // REST OAuth2 Flows...
        internal const string _RESTAuthOAuth2FlowImplicit     = "Implicit";
        internal const string _RESTAuthOAuth2FlowPassword     = "Password";
        internal const string _RESTAuthOAuth2FlowClientCredentials = "Client Credentials";
        internal const string _RESTAuthOAuth2FlowAuthCode     = "Authorization Code";

        // These are the names of all currently defined settings:
        internal const string _UseLogFile                     = "UseLogFile";                   // Use logfile when set to 'true'.
        internal const string _LogFileName                    = "LogFileName";                  // Fully qualified logfile name (including path).
        internal const string _CLAddSourceEnumsToDiagram      = "CLAddSourcEnumsToDiagram";     // CodeList: add source enumerations to diagram when 'true'.
        internal const string _CLAddCodeTypesToDiagram        = "CLAddCodeTypesToDiagram";      // CodeList: add generated CodeType to diagram when 'true'.
        internal const string _SMAddMessageAssemblyToDiagram  = "SMAddMessageAssemblyToDiagram";    // SOAP: add MessageAssembly classes to service model diagram when 'true'.
        internal const string _SMAddBusinessMsgToDiagram      = "SMAddBusinessMsgToDiagram";    // SOAP: add BusinessMessage classes to service model diagram when 'true'.
        internal const string _SMCreateCommonSchema           = "SMCreateCommonSchema";         // SOAP: force creation/parse of common schema when 'true'.
        internal const string _SMUseMessageHeaders            = "SMUseMessageHeaders";          // SOAP: force use of message headers when creating new diagram when 'true'.
        internal const string _SMUseSecurityLevels            = "SMUseSecurityLevels";          // SOAP: force checking/creation of security levels when 'true'.
        internal const string _DEBusinessTermName             = "DEBusinessTermName";           // Documentation: export BusinessTermName when 'true'.
        internal const string _DEDefinition                   = "DEDefinition";                 // Documentation: export Definition when 'true'.
        internal const string _DEDictionaryEntryName          = "DEDictionaryEntryName";        // Documentation: export DictionaryEntryName when 'true'.
        internal const string _DEUniqueID                     = "DEUniqueID";                   // Documentation: export UniqueID when 'true'.
        internal const string _DENotes                        = "DENotes";                      // Documentation: export Notes when 'true'.
        internal const string _DiagramSaveType                = "DiagramSaveType";              // User-configured diagram type.
        internal const string _InterfaceContractType          = "InterfaceContractType";        // Indicates whether we're building SOAP or REST services (also used to select schema type: XSD/JSON).
        internal const string _SaveMessageDiagrams            = "SaveMessageDiagrams";          // Not Yet Implemented! Save message diagrams in documentation when 'true'.
        internal const string _DocGenUseCommon                = "DocGenUseCommon";              // ADoc generation: use separate 'common' definitions file when 'true'.
        internal const string _DocGenUseGenerateDoc           = "DocGenGenerateDoc";            // Adoc generation: enable document generation when 'true'.
        internal const string _SupplementaryPrefixCode        = "SupplementaryPrefixCode";      // JSON: string to use in schema to indicate element is a supplementary attribute.
        internal const string _UseConfigurationManagement     = "UseConfigurationManagement";   // Set to 'true' when output must be written to Configuration Management Tool.
        internal const string _GLUserName                     = "GLUserName";                   // Central Repository: user name.
        internal const string _GLAccessToken                  = "GLAccessToken";                // Central Repository: access token (encrypted).
        internal const string _GLEMail                        = "GLEMail";                      // Central Repository: e-mail address of user.
        internal const string _GLRepositoryBaseURL            = "GLRepositoryBaseURL";          // Central Repository: partial URL to top or remote repository.
        internal const string _GLRepositoryNamespace          = "GLRepositoryNamespace";        // Relative path from repository base to root of all repositories.
        internal const string _GLCloneOnCreate                = "GLCloneOnCreate";              // When 'true', we use a clone from remote when creating a local, new, repository.
        internal const string _RepositoryRootPath             = "RepositoryRootPath";           // Local Repository: path to root of local GIT repositories.
        internal const string _GITIgnoreEntries               = "GITIgnoreEntries";             // Comma-separated list of GIT-Ignore entries for the session.

        // These are the names of all currently defined resources:
        internal const string _CodeListHeader                 = "CodeListHeader";
        internal const string _GenericodeTemplate             = "GenericodeTemplate";
        internal const string _GenericodeSetTemplate          = "GenericodeSetTemplate";
        internal const string _EDSNLDTMappingTable            = "EDSN_LDT_MappingTable";
        internal const string _SOAPInterfaceHeader            = "SOAPInterfaceHeader";
        internal const string _SOAPSchemaHeader               = "SOAPSchemaHeader";
        internal const string _ASCIIDocCommonTemplate         = "ASCIIDocCommonTemplate";
        internal const string _ASCIIDocClassTemplate          = "ASCIIDocClassTemplate";
        internal const string _ASCIIDocEmptyClassTemplate     = "ASCIIDocEmptyClassTemplate";
        internal const string _ASCIIDocTypeTemplate           = "ASCIIDocTypeTemplate";
        internal const string _ASCIIDocEnumTemplate           = "ASCIIDocEnumTemplate";
        internal const string _ASCIIDocOperationListTemplate  = "ASCIIDocOperationListTemplate";
        internal const string _ASCIIDocOperationTemplate      = "ASCIIDocOperationTemplate";
        internal const string _ASCIIDocMessageTemplate        = "ASCIIDocMessageTemplate";
        internal const string _ASCIIDocMessageClasses         = "ASCIIDocMessageClasses";
        internal const string _ASCIIDocCommonClasses          = "ASCIIDocCommonClasses";
        internal const string _ASCIIDocClassifiers            = "ASCIIDocClassifiers";
        internal const string _RESTAuthScheme                 = "RESTAuthScheme";
        internal const string _RESTAuthAPIKeys                = "RESTAuthAPIKeys";
        internal const string _RESTAuthOAuth2Flow             = "RESTAuthOAuth2Flow";
        internal const string _OpenAPI20Header                = "OpenAPI20Header";
        internal const string _UseAutomaticLocking            = "UseAutomaticLocking";
        internal const string _PersistentModelLocks           = "PersistentModelLocks";

        // We use this as an extra security precaution when encrypting/decrypting settings. Don't change this value or you can't retrieve
        // existing encrypted values anymore!
        internal const string _Salt                           = "FrameworkSaltyStuff";

        private SortedList<string, string> _stringSettings;
        private SortedList<string, bool> _boolSettings;
        private bool _inTransaction;

        /// <summary>
        /// Default constructor reads all settings from configuration to local storage for quick reference.
        /// </summary>
        internal FrameworkSettings()
        {
            this._boolSettings = new SortedList<string, bool>();
            this._stringSettings = new SortedList<string, string>();
            this._inTransaction = false;

            this._boolSettings.Add(_UseLogFile, Settings.Default.UseLogfile);
            this._boolSettings.Add(_CLAddSourceEnumsToDiagram, Settings.Default.CLAddSourceEnumsToDiagram);
            this._boolSettings.Add(_CLAddCodeTypesToDiagram, Settings.Default.CLAddCodeTypesToDiagram);
            this._boolSettings.Add(_SMAddMessageAssemblyToDiagram, Settings.Default.SMAddMessageAssemblyToDiagram);
            this._boolSettings.Add(_SMAddBusinessMsgToDiagram, Settings.Default.SMAddBusinessMsgToDiagram);
            this._boolSettings.Add(_SMCreateCommonSchema, Settings.Default.SMCreateCommonSchema);
            this._boolSettings.Add(_SMUseMessageHeaders, Settings.Default.SMUseMessageHeaders);
            this._boolSettings.Add(_SMUseSecurityLevels, Settings.Default.SMUseSecurityLevels);
            this._boolSettings.Add(_DEBusinessTermName, Settings.Default.DEBusinessTermName);
            this._boolSettings.Add(_DEDefinition, Settings.Default.DEDefinition);
            this._boolSettings.Add(_DEDictionaryEntryName, Settings.Default.DEDictionaryEntryName);
            this._boolSettings.Add(_DENotes, Settings.Default.DENotes);
            this._boolSettings.Add(_DEUniqueID, Settings.Default.DEUniqueID);
            this._boolSettings.Add(_SaveMessageDiagrams, Settings.Default.SaveMessageDiagrams);
            this._boolSettings.Add(_DocGenUseCommon, Settings.Default.DocGenUseCommon);
            this._boolSettings.Add(_DocGenUseGenerateDoc, Settings.Default.DocGenGenerateDoc);
            this._boolSettings.Add(_UseAutomaticLocking, Settings.Default.UseAutomaticLocking);
            this._boolSettings.Add(_PersistentModelLocks, Settings.Default.PersistentModelLocks);
            this._boolSettings.Add(_UseConfigurationManagement, Settings.Default.UseConfigurationManagement);
            this._boolSettings.Add(_GLCloneOnCreate, Settings.Default.GLCloneOnCreate);

            this._stringSettings.Add(_LogFileName, Settings.Default.LogfileName);
            this._stringSettings.Add(_DiagramSaveType, Settings.Default.DiagramSaveType);
            this._stringSettings.Add(_InterfaceContractType, Settings.Default.InterfaceContractType);
            this._stringSettings.Add(_RESTAuthAPIKeys, Settings.Default.RESTAuthAPIKeys);
            this._stringSettings.Add(_RESTAuthScheme, Settings.Default.RESTAuthScheme);
            this._stringSettings.Add(_RESTAuthOAuth2Flow, Settings.Default.RESTAuthOAuth2Flow);
            this._stringSettings.Add(_RESTHostName, Settings.Default.RESTHostName);
            this._stringSettings.Add(_RESTSchemes, Settings.Default.RESTSchemes);
            this._stringSettings.Add(_SupplementaryPrefixCode, Settings.Default.SupplementaryPrefix);
            this._stringSettings.Add(_GLUserName, Settings.Default.GLUserName);
            this._stringSettings.Add(_GLAccessToken, Settings.Default.GLAccessToken);
            this._stringSettings.Add(_GLEMail, Settings.Default.GLEMail);
            this._stringSettings.Add(_GLRepositoryBaseURL, Settings.Default.GLRepositoryURL);
            this._stringSettings.Add(_GLRepositoryNamespace, Settings.Default.GLRepositoryNamespace);
            this._stringSettings.Add(_RepositoryRootPath, Settings.Default.RepositoryRootPath);
            this._stringSettings.Add(_GITIgnoreEntries, Settings.Default.GITIgnoreEntries);
        }

        /// <summary>
        /// MUST be called after a 'StartTransaction' in order to close the transaction and save settings.
        /// </summary>
        internal void Commit()
        {
            if (this._inTransaction)
            {
                Settings.Default.Save();
                this._inTransaction = false;
            }
        }

        /// <summary>
        /// Returns the application-defined string-type resource with given name.
        /// </summary>
        /// <param name="name">Resource name to retrieve.</param>
        /// <returns>Resource object or NULL if name does not exist.</returns>
        internal string GetResourceString (string name)
        {
            switch (name)
            {
                case _CodeListHeader:
                    return Resources.CodeListHeader;

                case _GenericodeTemplate:
                    return Resources.GenericodeTemplate;

                case _GenericodeSetTemplate:
                    return Resources.GenericodeSetTemplate;

                case _EDSNLDTMappingTable:
                    return Resources.EDSN_LDT_MappingTable;

                case _SOAPInterfaceHeader:
                    return Resources.SOAPInterfaceHeader;

                case _SOAPSchemaHeader:
                    return Resources.SOAPSchemaHeader;

                case _ASCIIDocClassTemplate:
                    return Resources.ASCIIDocClassTemplate;

                case _ASCIIDocEmptyClassTemplate:
                    return Resources.ASCIIDocEmptyClassTemplate;

                case _ASCIIDocCommonTemplate:
                    return Resources.ASCIIDocCommonTemplate;

                case _ASCIIDocTypeTemplate:
                    return Resources.ASCIIDocTypeTemplate;

                case _ASCIIDocEnumTemplate:
                    return Resources.ASCIIDocEnumTemplate;

                case _ASCIIDocOperationListTemplate:
                    return Resources.ASCIIDocOperationListTemplate;

                case _ASCIIDocOperationTemplate:
                    return Resources.ASCIIDocOperationTemplate;

                case _ASCIIDocMessageTemplate:
                    return Resources.ASCIIDocMessageTemplate;

                case _ASCIIDocMessageClasses:
                    return Resources.ASCIIDocMessageClasses;

                case _ASCIIDocCommonClasses:
                    return Resources.ASCIIDocCommonClasses;

                case _ASCIIDocClassifiers:
                    return Resources.ASCIIDocClassifiers;

                case _OpenAPI20Header:
                    return Resources.OpenAPI20Header;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Retrieve string setting by name.
        /// </summary>
        /// <param name="name">Name of the setting to retrieve.</param>
        /// <param name="isEncrypted">True when string setting must be decrypted before return.</param>
        /// <returns>Value of setting.</returns>
        /// <exception cref="KeyNotFoundException">Specified name does not exist.</exception>
        internal string GetStringSetting(string name, bool isEncrypted = false)
        {
            if (isEncrypted)
            {
                try
                {
                    var cryptor = new CryptedString();
                    Logger.WriteInfo("Framework.Context.FrameworkSettings.GetStringSetting >> Encrypted value '" + name + "." + this._stringSettings[name] + "' read.");
                    return cryptor.Decrypt(this._stringSettings[name], _Salt);
                }
                catch (Exception exc)
                {
                    Logger.WriteError("Framework.Context.FrameworkSettings.GetStringSetting >> Error decrypting value, resetting contents because: " + exc.Message);
                    SetStringSetting(name, string.Empty, true);
                    return string.Empty;
                }
            }
            else return this._stringSettings[name];
        }

        /// <summary>
        /// Retrieve string setting by name.
        /// </summary>
        /// <param name="name">Name of the setting to retrieve.</param>
        /// <returns>Value of setting.</returns>
        /// <exception cref="KeyNotFoundException">Specified name does not exist.</exception>
        internal bool GetBoolSetting(string name)
        {
            return this._boolSettings[name];
        }

        /// <summary>
        /// Update setting by name, writing the new value both to memory as well as persistent storage.
        /// Update is only performed is the new value is different from the old value.
        /// If optional parameter 'mustEncrypt' is set to 'true', the string value is stored in encrypted form.
        /// </summary>
        /// <param name="name">Name of the setting to update.</param>
        /// <param name="newValue">New value of the setting.</param>
        /// <param name="mustEncrypt">Optional indicator that states whether or not the string must be encypted before storage.</param>
        /// <exception cref="KeyNotFoundException">Specified name does not exist.</exception>
        internal void SetStringSetting(string name, string value, bool mustEncrypt = false)
        {
            if (mustEncrypt)
            {
                var cryptor = new CryptedString();
                value = cryptor.Encrypt(value, _Salt);
                Logger.WriteInfo("Framework.Context.FrameworkSettings.SetStringSetting >> Encrypted value '" + name + "." + value + "' written.");
            }

            if (this._stringSettings[name] != value)
            {
                this._stringSettings[name] = value;
                switch (name)
                {
                    case _LogFileName:
                        Settings.Default.LogfileName = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _DiagramSaveType:
                        Settings.Default.DiagramSaveType = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _InterfaceContractType:
                        Settings.Default.InterfaceContractType = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _RESTAuthScheme:
                        Settings.Default.RESTAuthScheme = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _RESTAuthAPIKeys:
                        Settings.Default.RESTAuthAPIKeys = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _RESTAuthOAuth2Flow:
                        Settings.Default.RESTAuthOAuth2Flow = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _RESTHostName:
                        Settings.Default.RESTHostName = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _RESTSchemes:
                        Settings.Default.RESTSchemes = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _SupplementaryPrefixCode:
                        Settings.Default.SupplementaryPrefix = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _GLUserName:
                        Settings.Default.GLUserName = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _GLAccessToken:
                        Settings.Default.GLAccessToken = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _GLEMail:
                        Settings.Default.GLEMail = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _GLRepositoryBaseURL:
                        Settings.Default.GLRepositoryURL = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _GLRepositoryNamespace:
                        Settings.Default.GLRepositoryNamespace = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _RepositoryRootPath:
                        Settings.Default.RepositoryRootPath = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _GITIgnoreEntries:
                        Settings.Default.GITIgnoreEntries = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Update setting by name, writing the new value both to memory as well as persistent storage.
        /// Update is only performed is the new value is different from the old value.
        /// </summary>
        /// <param name="name">Name of the setting to update.</param>
        /// <param name="value">New value of the setting.</param>
        /// <exception cref="KeyNotFoundException">Specified name does not exist.</exception>
        internal void SetBoolSetting(string name, bool value)
        {
            if (this._boolSettings[name] != value)
            {
                this._boolSettings[name] = value;
                switch (name)
                {
                    case _UseLogFile:
                        Settings.Default.UseLogfile = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _CLAddCodeTypesToDiagram:
                        Settings.Default.CLAddCodeTypesToDiagram = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _CLAddSourceEnumsToDiagram:
                        Settings.Default.CLAddSourceEnumsToDiagram = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _SMAddMessageAssemblyToDiagram:
                        Settings.Default.SMAddMessageAssemblyToDiagram = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _SMAddBusinessMsgToDiagram:
                        Settings.Default.SMAddBusinessMsgToDiagram = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _SMCreateCommonSchema:
                        Settings.Default.SMCreateCommonSchema = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _SMUseMessageHeaders:
                        Settings.Default.SMUseMessageHeaders = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _SMUseSecurityLevels:
                        Settings.Default.SMUseSecurityLevels = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _DEBusinessTermName:
                        Settings.Default.DEBusinessTermName = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _DEDefinition:
                        Settings.Default.DEDefinition = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _DEDictionaryEntryName:
                        Settings.Default.DEDictionaryEntryName = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _DENotes:
                        Settings.Default.DENotes = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _DEUniqueID:
                        Settings.Default.DEUniqueID = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _SaveMessageDiagrams:
                        Settings.Default.SaveMessageDiagrams = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _DocGenUseCommon:
                        Settings.Default.DocGenUseCommon = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _DocGenUseGenerateDoc:
                        Settings.Default.DocGenGenerateDoc = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _UseAutomaticLocking:
                        Settings.Default.UseAutomaticLocking = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _PersistentModelLocks:
                        Settings.Default.PersistentModelLocks = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _UseConfigurationManagement:
                        Settings.Default.UseConfigurationManagement = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    case _GLCloneOnCreate:
                        Settings.Default.GLCloneOnCreate = value;
                        if (!this._inTransaction) Settings.Default.Save();
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Indicates that we want to execute a number of updates in a single transaction (don't save after each update).
        /// You MUST call 'Commit' in order to store the settings otherwise all are lost!
        /// </summary>
        internal void StartTransaction()
        {
            this._inTransaction = true;
        }
    }
}
