﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace APIManager.SparxEA.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.1.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Temp\\APIManagerLog.txt")]
        public string LogfileName {
            get {
                return ((string)(this["LogfileName"]));
            }
            set {
                this["LogfileName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool UseLogfile {
            get {
                return ((bool)(this["UseLogfile"]));
            }
            set {
                this["UseLogfile"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string RootPath {
            get {
                return ((string)(this["RootPath"]));
            }
            set {
                this["RootPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool CLAddSourceEnumsToDiagram {
            get {
                return ((bool)(this["CLAddSourceEnumsToDiagram"]));
            }
            set {
                this["CLAddSourceEnumsToDiagram"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool CLAddCodeTypesToDiagram {
            get {
                return ((bool)(this["CLAddCodeTypesToDiagram"]));
            }
            set {
                this["CLAddCodeTypesToDiagram"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool AutoIncrementBuildNrs {
            get {
                return ((bool)(this["AutoIncrementBuildNrs"]));
            }
            set {
                this["AutoIncrementBuildNrs"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool SMAddMessageAssemblyToDiagram {
            get {
                return ((bool)(this["SMAddMessageAssemblyToDiagram"]));
            }
            set {
                this["SMAddMessageAssemblyToDiagram"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool SMCreateCommonSchema {
            get {
                return ((bool)(this["SMCreateCommonSchema"]));
            }
            set {
                this["SMCreateCommonSchema"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool SMUseMessageHeaders {
            get {
                return ((bool)(this["SMUseMessageHeaders"]));
            }
            set {
                this["SMUseMessageHeaders"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool SMUseSecurityLevels {
            get {
                return ((bool)(this["SMUseSecurityLevels"]));
            }
            set {
                this["SMUseSecurityLevels"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool DEBusinessTermName {
            get {
                return ((bool)(this["DEBusinessTermName"]));
            }
            set {
                this["DEBusinessTermName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool DEDefinition {
            get {
                return ((bool)(this["DEDefinition"]));
            }
            set {
                this["DEDefinition"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool DEDictionaryEntryName {
            get {
                return ((bool)(this["DEDictionaryEntryName"]));
            }
            set {
                this["DEDictionaryEntryName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool DEUniqueID {
            get {
                return ((bool)(this["DEUniqueID"]));
            }
            set {
                this["DEUniqueID"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool DENotes {
            get {
                return ((bool)(this["DENotes"]));
            }
            set {
                this["DENotes"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(".jpg")]
        public string DiagramSaveType {
            get {
                return ((string)(this["DiagramSaveType"]));
            }
            set {
                this["DiagramSaveType"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool SaveMessageDiagrams {
            get {
                return ((bool)(this["SaveMessageDiagrams"]));
            }
            set {
                this["SaveMessageDiagrams"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("SOAP")]
        public string InterfaceContractType {
            get {
                return ((string)(this["InterfaceContractType"]));
            }
            set {
                this["InterfaceContractType"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool SMAddBusinessMsgToDiagram {
            get {
                return ((bool)(this["SMAddBusinessMsgToDiagram"]));
            }
            set {
                this["SMAddBusinessMsgToDiagram"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool DocGenUseCommon {
            get {
                return ((bool)(this["DocGenUseCommon"]));
            }
            set {
                this["DocGenUseCommon"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string RESTAuthAPIKeys {
            get {
                return ((string)(this["RESTAuthAPIKeys"]));
            }
            set {
                this["RESTAuthAPIKeys"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("OAuth2")]
        public string RESTAuthScheme {
            get {
                return ((string)(this["RESTAuthScheme"]));
            }
            set {
                this["RESTAuthScheme"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AuthorizationCode")]
        public string RESTAuthOAuth2Flow {
            get {
                return ((string)(this["RESTAuthOAuth2Flow"]));
            }
            set {
                this["RESTAuthOAuth2Flow"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string RESTHostName {
            get {
                return ((string)(this["RESTHostName"]));
            }
            set {
                this["RESTHostName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https")]
        public string RESTSchemes {
            get {
                return ((string)(this["RESTSchemes"]));
            }
            set {
                this["RESTSchemes"] = value;
            }
        }
    }
}