using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EAAddinFramework;
using SparxEA.Context;
using SparxEA.Model;
using SparxEA.ModelTransform;
using Framework.Controller;
using Framework.Context;
using Framework.Logging;
using Framework.Event;
using Framework.Model;
using EA;

// This defines the application (and default) namespace for the plugin.
namespace APIManager.SparxEA
{
    /// <summary>
    /// This is the main entry point for the plugin. It accepts EA Events and takes action accordingly.
    /// The class is only used to translate EA Events to generic framework events. The Framework Controller performs all of the 'real' work in order
    /// to keep the implementation-specific part as thin as possible.
    /// </summary>
    [ComVisible(true)]
    public sealed class EAController : EAAddinBase
    {
        #region Generic-Controller-Functions
  
        // Configuration properties for the top-level controller...
        private const string _TopLevelMenuName = "TopLevelMenuName";     // Identifies the top-level menu name configuration property.

        private const string _ClassType = "Class";                      // Used to check whether we got a class from the EA repository.
        private const string _ObjectType = "Object";                    // Used to check whether we got an object from the EA repository.

        /// <summary>
        /// The private constructor is called once on initial load and initializes all static controller properties.
        /// </summary>
        public EAController() : base()
        {
            // Can't do much here since the repository is not available yet.
            //MessageBox.Show("Starting EA...");  // Uncomment this to create a 'breakpoint' during startup.
        }

        /// <summary>
        /// Is invoked when EA is being closed down. The method releases the context in order to release resources.
        /// </summary>
        public override void EA_Disconnect()
        {
            Logger.WriteInfo("SparxEA.Controller.EAController.EA_Disconnect >> Closing down the shop!");

            // Controller performs all necessary cleanup stuff...
            ControllerSlt.GetControllerSlt().ShutDown();
            base.EA_Disconnect();
        }

        /// <summary>
        /// The EA_FileNew event enables the Add-In to respond to a File New event. When Enterprise Architect creates a new model file, this event 
        /// is raised and passed to all Add-Ins implementing this method.
        /// We treat this like a File Open and use it to determine the Repository type of the (new) model.
        /// </summary>
        /// <param name="Repository">An EA.Repository object representing the currently open Enterprise Architect model.
        /// Poll its members to retrieve model data and user interface status information.</param>
        public override void EA_FileNew(EA.Repository repository)
        {
            EA_FileOpen(repository);
        }

        /// <summary>
        /// The EA_FileOpen event enables the Add-In to respond to a File Open event. When Enterprise Architect opens a new model file, this event is 
        /// raised and passed to all Add-Ins implementing this method.
        /// The event occurs when the model being viewed by the Enterprise Architect user changes, for whatever reason (through user interaction or 
        /// Add-In activity) and we use it to determine the Repository type.
        /// </summary>
        /// <param name="Repository">An EA.Repository object representing the currently open Enterprise Architect model.
        /// Poll its members to retrieve model data and user interface status information.</param>
        public override void EA_FileOpen(EA.Repository repository)
        {
            const string DBTypeTag = "dbtype=";
            ModelSlt model = ModelSlt.GetModelSlt(); 
            Logger.WriteInfo("SparxEA.Controller.EAController.EA_FileOpen >> Project file is being opened, determine repository type...");
            string connectionString = repository.ConnectionString.ToLower();

            if (connectionString.Contains("eap")) model.ModelRepositoryType = ModelSlt.RepositoryType.Local;
            else
            {
                string dbType = connectionString.Substring(connectionString.IndexOf(DBTypeTag) + DBTypeTag.Length, 1);
                switch (dbType)
                {
                    case "0":
                        model.ModelRepositoryType = ModelSlt.RepositoryType.MySQL;
                        break;

                    case "1":
                        model.ModelRepositoryType = ModelSlt.RepositoryType.SQLServer;
                        break;

                    case "2":
                        // ADO Jet behaves like local EAP, so we treat them the same...
                        model.ModelRepositoryType = ModelSlt.RepositoryType.Local;
                        break;

                    case "3":
                        model.ModelRepositoryType = ModelSlt.RepositoryType.Oracle;
                        break;

                    case "4":
                        model.ModelRepositoryType = ModelSlt.RepositoryType.PostgreSQL;
                        break;

                    default:
                        // All others are currently unsupported.
                        model.ModelRepositoryType = ModelSlt.RepositoryType.Unknown;
                        break;
                }
            }
            Logger.WriteInfo("SparxEA.Controller.EAController.EA_FileOpen >> Repository type set to: '" + model.ModelRepositoryType + "'.");
        }

        /// <summary>
        /// The EA_GetMenuItems event enables the Add-In to provide the Enterprise Architect user interface with additional Add-In menu options in 
        /// various context and main menus. When a user selects an Add-In menu option, an event is raised and passed back to the Add-In that 
        /// originally defined that menu option. This event is raised just before Enterprise Architect has to show particular menu options to 
        /// the user, and its use is described in the Define Menu Items topic.
        /// </summary>
        /// <param name="repository">An EA.Repository object representing the currently open Enterprise Architect model.
        /// Poll its members to retrieve model data and user interface status information.</param>
        /// <param name="menuLocation">String representing the part of the user interface that brought up the menu. 
        /// Can be TreeView, MainMenu or Diagram.</param>
        /// <param name="menuName">The name of the parent menu for which sub-items are to be defined. In the case of the top-level menu it is 
        /// an empty string.</param>
        /// <returns>One of the following types:
        /// - A string indicating the label for a single menu option.
        /// - An array of strings indicating a multiple menu options.
        /// - NULL to indicate that no menu should be displayed.
        /// In the case of the top-level menu it should be a single string or an array containing only one item, or NULL.</returns>
        public override object EA_GetMenuItems(EA.Repository repository, string menuLocation, string menuName)
        {
            string topLevelMenu = ContextSlt.GetContextSlt().GetConfigProperty(_TopLevelMenuName);
            Object returnObject;

            if (string.IsNullOrEmpty(menuName))
            {
                if (topLevelMenu[0] != '-') topLevelMenu = "-" + topLevelMenu; // Assures that name starts with hyphen (to force sub-menus)...
                if (topLevelMenu[1] != '&') topLevelMenu = "-&" + topLevelMenu.Substring(1);    // Assures that second position has '&'...
                returnObject = topLevelMenu;
            }
            else
            {
                TreeScope scope;
                switch (menuLocation)
                {
                    case "TreeView":
                        scope = TreeScope.PackageTree;
                        break;

                    case "Diagram":
                        scope = TreeScope.Diagram;
                        break;

                    case "MainMenu":
                        scope = TreeScope.Controller;
                        break;

                    default:
                        scope = TreeScope.Undefined;
                        break;
                }
                returnObject = GetMenuItems(scope, menuName);
            }
            return returnObject;
        }

        /// <summary>
        /// The EA_GetMenuState event enables the Add-In to set a particular menu option to either enabled or disabled. This is useful when dealing 
        /// with locked packages and other situations where it is convenient to show a menu option, but not enable it for use.
        /// This event is raised just before Enterprise Architect has to show particular menu options to the user. Its use is described in the 
        /// Define Menu Items topic.
        /// </summary>
        /// <param name="repository">An EA.Repository object representing the currently open Enterprise Architect model.
        /// Poll its members to retrieve model data and user interface status information.</param>
        /// <param name="menuLocation">String representing the part of the user interface that brought up the menu. 
        /// Can be TreeView, MainMenu or Diagram.</param>
        /// <param name="menuName">The name of the parent menu for which sub-items must be defined. In the case of the top-level menu it is an empty string.</param>
        /// <param name="itemName">The name of the option actually clicked, for example, Create a New Invoice.</param>
        /// <param name="isEnabled">Boolean. Set to False to disable this particular menu option.</param>
        /// <param name="isChecked">Boolean. Set to True to check this particular menu option.</param>
        public override void EA_GetMenuState(EA.Repository repository, string menuLocation, string menuName, string itemName, ref bool isEnabled, ref bool isChecked)
        {
            bool state = false;
            string topLevelMenuName = ContextSlt.GetContextSlt().GetConfigProperty(_TopLevelMenuName);

            if (IsProjectOpen(repository))
            {
                try
                {
                    // If we get names from EA that start with '-' and/or '&', we remove these in order to get to the internal event names....
                    // If the menu name is not specified, it is forced to the root name.
                    if (!string.IsNullOrEmpty(menuName))
                    {
                        if (menuName[0] == '-') menuName = menuName.Substring(1);
                        if (menuName[0] == '&') menuName = menuName.Substring(1);
                    }
                    else menuName = topLevelMenuName;
                    if (menuName == topLevelMenuName) menuName = "root";

                    if (itemName[0] == '-') itemName = itemName.Substring(1);
                    if (itemName[0] == '&') itemName = itemName.Substring(1);

                    TreeScope scope;
                    switch (menuLocation)
                    {
                        case "TreeView":
                            scope = TreeScope.PackageTree;
                            break;

                        case "Diagram":
                            scope = TreeScope.Diagram;
                            break;

                        case "MainMenu":
                            scope = TreeScope.Controller;
                            break;

                        default:
                            scope = TreeScope.Undefined;
                            break;
                    }

                    state = ControllerSlt.GetControllerSlt().IsValidState(scope, menuName, itemName);
                }
                catch (Exception exc)
                {
                    Logger.WriteError("SparxEA.Controller.EAController.EA_GetMenuState >> Caught an exception: " + exc.ToString());
                }
            }
            isEnabled = state;
        }

        /// <summary>
        /// EA_MenuClick events are received by an Add-In in response to user selection of a menu option.
        /// The event is raised when the user clicks on a particular menu option. When a user clicks on one of your non-parent menu options, your Add-In receives a MenuClick event, defined as follows:
        /// Sub EA_MenuClick(Repository As EA.Repository, ByVal MenuName As String, ByVal ItemName As String)
        /// Notice that your code can directly access Enterprise Architect data and UI elements using Repository methods.
        /// Also look at EA_GetMenuItems.
        /// </summary>
        /// <param name="repository">An EA.Repository object representing the currently open Enterprise Architect model.
        /// Poll its members to retrieve model data and user interface status information.</param>
        /// <param name="menuLocation">String representing the part of the user interface that brought up the menu. 
        /// Can be TreeView, MainMenu or Diagram.</param>
        /// <param name="menuName">The name of the parent menu for which sub-items must be defined. In the case of the top-level menu it is an empty string.</param>
        /// <param name="itemName">The name of the option actually clicked, for example, Create a New Invoice.</param>
        public override void EA_MenuClick(EA.Repository repository, string menuLocation, string menuName, string itemName)
        {
            string topLevelMenuName = ContextSlt.GetContextSlt().GetConfigProperty(_TopLevelMenuName);
            try
            {
                // If we get names from EA that start with '-' and/or '&', we remove these in order to get to the internal event names....
                // If the menu name is not specified, it is forced to the root name.
                if (!string.IsNullOrEmpty(menuName))
                {
                    if (menuName[0] == '-') menuName = menuName.Substring(1);
                    if (menuName[0] == '&') menuName = menuName.Substring(1);
                }
                else menuName = topLevelMenuName;
                if (menuName == topLevelMenuName) menuName = "root";

                if (itemName[0] == '-') itemName = itemName.Substring(1);
                if (itemName[0] == '&') itemName = itemName.Substring(1);

                TreeScope scope;
                switch (menuLocation)
                {
                    case "TreeView":
                        scope = TreeScope.PackageTree;
                        break;

                    case "Diagram":
                        scope = TreeScope.Diagram;
                        break;

                    case "MainMenu":
                        scope = TreeScope.Controller;
                        break;

                    default:
                        scope = TreeScope.Undefined;
                        break;
                }
                ControllerSlt.GetControllerSlt().HandleEvent(scope, menuName, itemName);
            }
            catch (Exception exc)
            {
                Logger.WriteError("SparxEA.Controller.EAController.EA_MenuClick >> Caught an exception: " + exc.ToString());
            }
        }

        /// <summary>
        /// EA_OnNotifyContextItemModified notifies Add-Ins that the current context item has been modified.
        /// This event occurs when a user has modified the context item. Add-Ins that require knowledge of when an item has been modified 
        /// can subscribe to this broadcast function.
        /// Also look at EA_OnContextItemChanged and EA_OnContextItemDoubleClicked.
        /// </summary>
        /// <param name="Repository">An EA.Repository object representing the currently open Enterprise Architect model.
        /// Poll its members to retrieve model data and user interface status information.</param>
        /// <param name="GUID">Contains the GUID of the new context item. 
        /// This value corresponds to the following properties, depending on the value of the ot parameter:
        /// ot (ObjectType)	- GUID value
        /// otElement  		- Element.ElementGUID
        /// otPackage 		- Package.PackageGUID
        /// otDiagram		- Diagram.DiagramGUID
        /// otAttribute		- Attribute.AttributeGUID
        /// otMethod		- Method.MethodGUID
        /// otConnector		- Connector.ConnectorGUID
        /// </param>
        /// <param name="ot">Specifies the type of the new context item.</param>
        public override void EA_OnNotifyContextItemModified(EA.Repository repository, string GUID, EA.ObjectType ot)
        {
            switch (ot)
            {
                case EA.ObjectType.otAttribute:
                    ControllerSlt.GetControllerSlt().HandleObjectEvent(ObjectEventType.Modified, Framework.Event.ObjectType.Attribute, -1, GUID);
                    return;

                case EA.ObjectType.otConnector:
                    ControllerSlt.GetControllerSlt().HandleObjectEvent(ObjectEventType.Modified, Framework.Event.ObjectType.Connector, -1, GUID);
                    return;

                case EA.ObjectType.otElement:
                    // Make sure we're dealing with actual class/object entities instead of other 'element' artifacts....
                    EA.Element element = repository.GetElementByGuid(GUID) as EA.Element;
                    if (element != null)
                    { 
                        if (element.Type == _ClassType)
                            ControllerSlt.GetControllerSlt().HandleObjectEvent(ObjectEventType.Modified, Framework.Event.ObjectType.Class, -1, GUID);
                        else if (element.Type == _ObjectType)
                            ControllerSlt.GetControllerSlt().HandleObjectEvent(ObjectEventType.Modified, Framework.Event.ObjectType.Object, -1, GUID);
                    }
                    return;

                case EA.ObjectType.otPackage:
                    ControllerSlt.GetControllerSlt().HandleObjectEvent(ObjectEventType.Modified, Framework.Event.ObjectType.Package, -1, GUID);
                    return;

                case EA.ObjectType.otDiagram:
                    Logger.WriteInfo("SparxEA.Controller.EAController.EA_OnContextItemModified >> Got Diagram GUID '" + GUID + "'...");
                    // For some reason the API returns an Object in stead of a Diagram. Some casting required.
                    // In EA 13.5 I have eens a SchemaPropertyClass to be returned here. So we treat this even more carefully now...
                    object o = repository.GetDiagramByGuid(GUID);
                    if (o is EA.Diagram)
                    {
                        ControllerSlt.GetControllerSlt().HandleObjectEvent(ObjectEventType.Modified, Framework.Event.ObjectType.Diagram, -1, GUID);
                    }
                    else
                    {
                        Logger.WriteWarning("Reported type 'otDiagram' does in fact returns a '" + o.GetType() + "'!");
                    }
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// EA_OnContextItemChanged notifies Add-Ins that a different item is now in context.
        /// This event occurs after a user has selected an item anywhere in the Enterprise Architect GUI. 
        /// If ot = otRepository, then this function behaves the same as EA_FileOpen.
        /// Also look at EA_OnContextItemDoubleClicked and EA_OnNotifyContextItemModified.
        /// </summary>
        /// <param name="repository">An EA.Repository object representing the currently open Enterprise Architect model.
        /// Poll its members to retrieve model data and user interface status information.</param>
        /// <param name="GUID">Contains the GUID of the new context item. 
        /// This value corresponds to the following properties, depending on the value of the ot parameter:
        /// ot (ObjectType)	- GUID value
        /// otElement  		- Element.ElementGUID
        /// otPackage 		- Package.PackageGUID
        /// otDiagram		- Diagram.DiagramGUID
        /// otAttribute		- Attribute.AttributeGUID
        /// otMethod		- Method.MethodGUID
        /// otConnector		- Connector.ConnectorGUID
        /// otRepository	- NOT APPLICABLE, GUID is an empty string
        /// </param>
        /// <param name="ot">Specifies the type of the new context item.</param>
        public override void EA_OnContextItemChanged(EA.Repository repository, string GUID, EA.ObjectType ot)
        {
            ControllerSlt controller = ControllerSlt.GetControllerSlt();

            switch (ot)
            {
                case EA.ObjectType.otAttribute:
                    int attributeID = repository.GetAttributeByGuid(GUID).AttributeID;
                    controller.SwitchScope(ContextScope.Attribute, attributeID, GUID);
                    controller.HandleObjectEvent(ObjectEventType.Selected, Framework.Event.ObjectType.Attribute, attributeID, GUID);
                    return;

                case EA.ObjectType.otConnector:
                    int connectorID = repository.GetConnectorByGuid(GUID).ConnectorID;
                    controller.SwitchScope(ContextScope.Connector, connectorID, GUID);
                    controller.HandleObjectEvent(ObjectEventType.Selected, Framework.Event.ObjectType.Connector, connectorID, GUID);
                    return;

                case EA.ObjectType.otElement:
                    EA.Element element = repository.GetElementByGuid(GUID);
                    if (element != null)
                    {
                        controller.SwitchScope(ContextScope.Class, element.ElementID, GUID);
                        if (element.Type == _ClassType)
                            ControllerSlt.GetControllerSlt().HandleObjectEvent(ObjectEventType.Selected, Framework.Event.ObjectType.Class, -1, GUID);
                        else if (element.Type == _ObjectType)
                            ControllerSlt.GetControllerSlt().HandleObjectEvent(ObjectEventType.Selected, Framework.Event.ObjectType.Object, -1, GUID);
                    }
                    return;

                case EA.ObjectType.otPackage:
                    int packageID = repository.GetPackageByGuid(GUID).PackageID;
                    controller.SwitchScope(ContextScope.Package, packageID, GUID);
                    controller.HandleObjectEvent(ObjectEventType.Selected, Framework.Event.ObjectType.Package, packageID, GUID);
 
                    return;

                case EA.ObjectType.otDiagram:
                    Logger.WriteInfo("SparxEA.Controller.EAController.EA_OnContextItemChanged >> Got Diagram GUID '" + GUID + "'...");
                    // For some reason the API returns an Object in stead of a Diagram. Some casting required.
                    // In EA 13.5 I have eens a SchemaPropertyClass to be returned here. So we treat this even more carefully now...
                    object o = repository.GetDiagramByGuid(GUID);
                    if (o is EA.Diagram)
                    {
                        controller.SwitchScope(ContextScope.Diagram, ((EA.Diagram)o).DiagramID, GUID);
                        controller.HandleObjectEvent(ObjectEventType.Selected, Framework.Event.ObjectType.Diagram, ((EA.Diagram)o).DiagramID, GUID);
                    }
                    else Logger.WriteWarning("Reported type 'otDiagram' does in fact returns a '" + o.GetType() + "'!");
                    return;

                default:        // All other types are ignored here.
                    return;
            }
        }

        /// <summary>
        /// EA_OnPostInitialized notifies Add-Ins that the Repository object has finished loading and any necessary initialization 
        /// steps can now be performed on the object.
        /// For example, the Add-In can create an Output tab using Repository.CreateOutputTab.
        /// </summary>
        /// <param name="repository">An EA.Repository object representing the currently open Enterprise Architect model.
        /// Poll its members to retrieve model data and user interface status information.</param>
        public override void EA_OnPostInitialized(EA.Repository repository)
        {
            try
            {
                // Controller singleton receives the implementation objects for context and model and performs subsequent initialization.
                ControllerSlt.GetControllerSlt().Initialize(new EAContextImplementation(repository), new EAModelImplementation(repository));
            }
            catch (System.SystemException exc)
            {
                Logger.WriteError("SparxEA.Controller.EAController.EA_OnPostInitialized >> initialization failed because: " + exc.Message);
            }
        }

        /// <summary>
        /// EA_OnPostNewElement notifies Add-Ins that a new element has been created on a diagram. It enables Add-Ins to modify the element upon creation.
        /// This event occurs after a user has dragged a new element from the Toolbox or Resources window onto a diagram. 
        /// The notification is provided immediately after the element is added to the model. Set Repository.SuppressEADialogs to true to suppress 
        /// Enterprise Architect from showing its default dialogs.
        /// Also look at EA_OnPreNewElement.
        /// </summary>
        /// <param name="repository">An EA.Repository object representing the currently open Enterprise Architect model.
        /// Poll its members to retrieve model data and user interface status information.</param>
        /// <param name="info">Contains the following EventProperty object for the new element:
        /// - ElementID: A long value corresponding to Element.ElementID. </param>
        /// <returns>Return True if the element has been updated during this notification. Return False otherwise.</returns>
        public override bool EA_OnPostNewElement(EA.Repository repository, EA.EventProperties info) 
        {
            int objectID;
            if (int.TryParse((string)info.Get("ElementID").Value, out objectID))
            {
                EA.Element element = repository.GetElementByID(objectID);
                if (element != null)
                {
                    if (element.Type == _ClassType)
                        return ControllerSlt.GetControllerSlt().HandleObjectEvent(ObjectEventType.Created, Framework.Event.ObjectType.Class, objectID, element.ElementGUID);
                    else if (element.Type == _ObjectType)
                        return ControllerSlt.GetControllerSlt().HandleObjectEvent(ObjectEventType.Created, Framework.Event.ObjectType.Object, objectID, element.ElementGUID);
                }
            }
            return false;
        }

        /// <summary>
        /// EA_OnPostNewConnector notifies Add-Ins that a new connector has been created on a diagram. It enables Add-Ins to modify the connector upon creation.
        /// This event occurs after a user has dragged a new connector from the Toolbox or Resources window onto a diagram. 
        /// The notification is provided immediately after the connector is added to the model. Set Repository.SuppressEADialogs to true to suppress 
        /// Enterprise Architect from showing its default dialogs.
        /// Also look at EA_OnPreNewConnector.
        /// </summary>
        /// <param name="repository">An EA.Repository object representing the currently open Enterprise Architect model.
        /// Poll its members to retrieve model data and user interface status information.</param>
        /// <param name="info">Contains the following EventProperty object for the new connector:
        /// - ConnectorID: A long value corresponding to Connector.ConnectorID.
        /// </param>
        /// <returns>Return True if the connector has been updated during this notification. Return False otherwise.</returns>
        public override bool EA_OnPostNewConnector(EA.Repository repository, EA.EventProperties info) 
        {
            int objectID;
            if (int.TryParse((string)info.Get("ConnectorID").Value, out objectID))
                return ControllerSlt.GetControllerSlt().HandleObjectEvent(ObjectEventType.Created, Framework.Event.ObjectType.Connector, objectID, string.Empty);
            else return false;
        }

        /// <summary>
        /// EA_OnPostNewDiagram notifies Add-Ins that a new diagram has been created. It enables Add-Ins to modify the diagram upon creation.
        /// Set Repository.SuppressEADialogs to true to suppress Enterprise Architect from showing its default dialogs.
        /// Also look at EA_OnPreNewDiagram.
        /// </summary>
        /// <param name="repository">An EA.Repository object representing the currently open Enterprise Architect model.
        /// Poll its members to retrieve model data and user interface status information.</param>
        /// <param name="info">Contains the following EventProperty object for the new diagram:
        /// - DiagramID: A long value corresponding to Diagram.DiagramID.</param>
        /// <returns>Return True if the diagram has been updated during this notification. Return False otherwise.</returns>
        public override bool EA_OnPostNewDiagram(EA.Repository repository, EA.EventProperties info) 
        {
            int objectID;
            if (int.TryParse((string)info.Get("DiagramID").Value, out objectID))
                return ControllerSlt.GetControllerSlt().HandleObjectEvent(ObjectEventType.Created, Framework.Event.ObjectType.Diagram, objectID, string.Empty);
            else return false;
        }

        /// <summary>
        /// EA_OnPostNewDiagramObject notifies Add-Ins that a new object has been created on a diagram. It enables Add-Ins to modify the object upon creation.
        /// This event occurs after a user has dragged a new object from the Project Browser or Resources window onto a diagram. 
        /// The notification is provided immediately after the object is added to the diagram. Set Repository.SuppressEADialogs to true to suppress 
        /// Enterprise Architect from showing its default dialogs.
        /// Also look at EA_OnPreNewDiagramObject.
        /// </summary>
        /// <param name="repository">An EA.Repository object representing the currently open Enterprise Architect model.
        /// Poll its members to retrieve model data and user interface status information.</param>
        /// <param name="info">Contains the following EventProperty object for the new element:
        /// - ID: A long value corresponding to Diagram.ObjectID.
        /// - DiagramID: A long value corresponding to DiagramID.
        /// <returns>Return True if the element has been updated during this notification. Return False otherwise.</returns>
        public override bool EA_OnPostNewDiagramObject(EA.Repository repository, EA.EventProperties info) 
        {
            int objectID;
            int diagramID;
            if (int.TryParse((string)info.Get("ID").Value, out objectID) && int.TryParse((string)info.Get("DiagramID").Value, out diagramID))
                return ControllerSlt.GetControllerSlt().HandleObjectEvent(ObjectEventType.Created, Framework.Event.ObjectType.DiagramObject, objectID, string.Empty, diagramID);
            else return false;
        }

        /// <summary>
        /// EA_OnPostNewAttribute notifies Add-Ins that a new attribute has been created on a diagram. It enables Add-Ins to modify the attribute upon creation.
        /// This event occurs when a user creates a new attribute on an element by either drag-dropping from the Project Browser, using the Attributes Properties 
        /// dialog, or using the in-place editor on the diagram. The notification is provided immediately after the attribute is created. 
        /// Set Repository.SuppressEADialogs to true to suppress Enterprise Architect from showing its default dialogs.
        /// Also look at EA_OnPreNewAttribute.
        /// </summary>
        /// <param name="repository">An EA.Repository object representing the currently open Enterprise Architect model.
        /// Poll its members to retrieve model data and user interface status information.</param>
        /// <param name="info">Contains the following EventProperty object for the new attribute:
        /// - AttributeID: A long value corresponding to Attribute.AttributeID.</param>
        /// <returns>Return True if the attribute has been updated during this notification. Return False otherwise.</returns>
        public override bool EA_OnPostNewAttribute(EA.Repository repository, EA.EventProperties info) 
        {
            int objectID;
            if (int.TryParse((string)info.Get("AttributeID").Value, out objectID))
                return ControllerSlt.GetControllerSlt().HandleObjectEvent(ObjectEventType.Created, Framework.Event.ObjectType.Attribute, objectID, string.Empty);
            else return false;
        }

        /// <summary>
        /// EA_OnPostNewPackage notifies Add-Ins that a new package has been created on a diagram. It enables Add-Ins to modify the package upon creation.
        /// This event occurs when a user drags a new package from the Toolbox or Resources window onto a diagram, or by selecting the New Package icon from the Project Browser. Set Repository.SuppressEADialogs to true to suppress Enterprise Architect from showing its default dialogs.
        /// Also look at EA_OnPreNewPackage.
        /// </summary>
        /// <param name="repository">An EA.Repository object representing the currently open Enterprise Architect model.
        /// Poll its members to retrieve model data and user interface status information.</param>
        /// <param name="info">Contains the following EventProperty object for the new package:
        /// - PackageID: A long value corresponding to Package.PackageID.</param>
        /// <returns>Return True if the package has been updated during this notification. Return False otherwise.</returns>
        public override bool EA_OnPostNewPackage(EA.Repository repository, EA.EventProperties info) 
        {
            int objectID;
            if (int.TryParse((string)info.Get("PackageID").Value, out objectID))
                return ControllerSlt.GetControllerSlt().HandleObjectEvent(ObjectEventType.Created, Framework.Event.ObjectType.Package, objectID, string.Empty);
            else return false;
        }

        /// <summary>
        /// EA_OnPostOpenDiagram notifies Add-Ins that a diagram has been opened. We use this event to set the scope to "Diagram", which
        /// assures that the opened diagram is reflected in the current context. We also raise a "Diagram Opened" event that can be processed
        /// by custom event handlers.
        /// </summary>
        /// <param name="repository">An EA.Repository object representing the currently open Enterprise Architect model.
        /// Poll its members to retrieve model data and user interface status information.</param>
        /// <param name="diagramID">Contains the Diagram ID of the diagram that was opened.</param>
        public override void EA_OnPostOpenDiagram(EA.Repository repository, int diagramID)
        {
            Logger.WriteInfo("SparxEA.Controller.EAController.EA_OnPostOpenDiagram >> Got Diagram ID '" + diagramID + "'...");
            ControllerSlt.GetControllerSlt().SwitchScope(ContextScope.Diagram, diagramID, repository.GetDiagramByID(diagramID).DiagramGUID);
            ControllerSlt.GetControllerSlt().HandleObjectEvent(ObjectEventType.Opened, Framework.Event.ObjectType.Diagram, diagramID, string.Empty);
        }

        /// <summary>
        /// EA_OnPostCloseDiagram notifies Add-Ins that a diagram has been closed.
        /// Also look at EA_OnPostOpenDiagram.
        /// </summary>
        /// <param name="Repository">An EA.Repository object representing the currently open Enterprise Architect model.
        /// Poll its members to retrieve model data and user interface status information.</param>
        /// <param name="DiagramID">Contains the Diagram ID of the diagram that was closed.</param>
        public override void EA_OnPostCloseDiagram(EA.Repository repository, int diagramID)
        {
            ControllerSlt.GetControllerSlt().HandleObjectEvent(ObjectEventType.Closed, Framework.Event.ObjectType.Diagram, diagramID, string.Empty);
        }

        /// <summary>
        /// EA_OnPreDeleteElement notifies Add-Ins that an element is to be deleted from the model. It enables Add-Ins to permit or deny deletion of the element.
        /// This event occurs when a user deletes an element from the Project Browser or on a diagram. 
        /// The notification is provided immediately before the element is deleted, so that the Add-In can disable deletion of the element.
        /// </summary>
        /// <param name="repository">An EA.Repository object representing the currently open Enterprise Architect model.
        /// Poll its members to retrieve model data and user interface status information.</param>
        /// <param name="info">Contains the following EventProperty object for the element to be deleted:
        /// - ElementID: A long value corresponding to Element.ElementID.</param>	
        /// <returns>Return True to enable deletion of the element from the model. Return False to disable deletion of the element.</returns>
        public override bool EA_OnPreDeleteElement(EA.Repository repository, EA.EventProperties info) 
        {
            int objectID;
            if (int.TryParse((string)info.Get("ElementID").Value, out objectID))
            {
                EA.Element element = repository.GetElementByID(objectID);
                if (element != null)
                {
                    if (element.Type == _ClassType)
                        return ControllerSlt.GetControllerSlt().HandleObjectEvent(ObjectEventType.Deleted, Framework.Event.ObjectType.Class, objectID, element.ElementGUID);
                    else if (element.Type == _ObjectType)
                        return ControllerSlt.GetControllerSlt().HandleObjectEvent(ObjectEventType.Deleted, Framework.Event.ObjectType.Object, objectID, element.ElementGUID);
                }
            }
            return true;
        }

        /// <summary>
        /// EA_OnPreDeleteAttribute notifies Add-Ins that an attribute is to be deleted from the model. It enables Add-Ins to permit or deny deletion of the attribute.
        /// This event occurs when a user deletes an attribute from the Project Browser or on a diagram. 
        /// The notification is provided immediately before the attribute is deleted, so that the Add-In can disable deletion of the attribute.
        /// </summary>
        /// <param name="repository">An EA.Repository object representing the currently open Enterprise Architect model.
        /// Poll its members to retrieve model data and user interface status information.</param>
        /// <param name="info">Contains the following EventProperty object for the Attribute to be deleted:
        /// - AttributeID: A long value corresponding to Attribute.AttributeID.</param>	
        /// <returns>Return True to enable deletion of the attribute from the model. Return False to disable deletion of the attribute.</returns>
        public override bool EA_OnPreDeleteAttribute(EA.Repository repository, EA.EventProperties info)
        {
            int objectID;
            if (int.TryParse((string)info.Get("AttributeID").Value, out objectID))
                return ControllerSlt.GetControllerSlt().HandleObjectEvent(ObjectEventType.Deleted, Framework.Event.ObjectType.Attribute, objectID, string.Empty);
            else return true;
        }

        /// <summary>
        /// EA_OnPreDeleteConnector notifies Add-Ins that an connector is to be deleted from the model. It enables Add-Ins to permit or deny deletion of the connector.
        /// This event occurs when a user attempts to permanently delete a connector on a diagram.
        /// The notification is provided immediately before the connector is deleted, so that the Add-In can disable deletion of the connector.
        /// </summary>
        /// <param name="repository">An EA.Repository object representing the currently open Enterprise Architect model.
        /// Poll its members to retrieve model data and user interface status information.</param>
        /// <param name="info">Contains the following EventProperty object for the connector to be deleted:
        /// - ConnectorID: A long value corresponding to Connector.ConnectorID.</param>	
        /// <returns>Return True to enable deletion of the connector from the model. Return False to disable deletion of the connector.</returns>
        public override bool EA_OnPreDeleteConnector(EA.Repository repository, EA.EventProperties info) 
        {
            int objectID;
            if (int.TryParse((string)info.Get("ConnectorID").Value, out objectID))
                return ControllerSlt.GetControllerSlt().HandleObjectEvent(ObjectEventType.Deleted, Framework.Event.ObjectType.Connector, objectID, string.Empty);
            else return true;
        }

        /// <summary>
        /// EA_OnPreDeleteDiagram notifies Add-Ins that an diagram is to be deleted from the model. It enables Add-Ins to permit or deny deletion of the diagram.
        /// This event occurs when a user attempts to permanently delete a diagram from the Project Browser.
        /// The notification is provided immediately before the diagram is deleted, so that the Add-In can disable deletion of the diagram.
        /// </summary>
        /// <param name="repository">An EA.Repository object representing the currently open Enterprise Architect model.
        /// Poll its members to retrieve model data and user interface status information.</param>
        /// <param name="info">Contains the following EventProperty object for the diagram to be deleted:
        /// - DiagramID: A long value corresponding to Diagram.DiagramID.</param>	
        /// <returns>Return True to enable deletion of the diagram from the model. Return False to disable deletion of the diagram.</returns>
        public override bool EA_OnPreDeleteDiagram(EA.Repository repository, EA.EventProperties info) 
        {
            int objectID;
            if (int.TryParse((string)info.Get("DiagramID").Value, out objectID))
                return ControllerSlt.GetControllerSlt().HandleObjectEvent(ObjectEventType.Deleted, Framework.Event.ObjectType.Diagram, objectID, string.Empty);
            else return true;
        }

        /// <summary>
        /// EA_OnPreDeleteDiagramObject notifies Add-Ins that a diagram object is to be deleted from the model. It enables Add-Ins to permit or deny deletion of the element.
        /// This event occurs when a user attempts to permanently delete an element from a diagram. The notification is provided immediately before the element is deleted, so that the Add-In can disable deletion of the element.
        /// </summary>
        /// <param name="repository">An EA.Repository object representing the currently open Enterprise Architect model.
        /// Poll its members to retrieve model data and user interface status information.</param>
        /// <param name="info">Contains the following EventProperty objects for the element to be deleted:
        /// · ID: A long value corresponding to DiagramObject.ElementID</param>
        /// <returns>Return True to enable deletion of the element from the model. Return False to disable deletion of the element.</returns>
        public override bool EA_OnPreDeleteDiagramObject(EA.Repository repository, EA.EventProperties info) 
        {
            int objectID;
            if (int.TryParse((string)info.Get("ID").Value, out objectID))
                return ControllerSlt.GetControllerSlt().HandleObjectEvent(ObjectEventType.Deleted, Framework.Event.ObjectType.DiagramObject, objectID, string.Empty);
            else return true;
        }

        /// <summary>
        /// EA_OnPreDeletePackage notifies Add-Ins that an package is to be deleted from the model. It enables Add-Ins to permit or deny deletion of the package.
        /// This event occurs when a user attempts to permanently delete a package from the Project Browser.
        /// The notification is provided immediately before the package is deleted, so that the Add-In can disable deletion of the package.
        /// </summary>
        /// <param name="repository">An EA.Repository object representing the currently open Enterprise Architect model.
        /// Poll its members to retrieve model data and user interface status information.</param>
        /// <param name="info">Contains the following EventProperty object for the package to be deleted:
        /// - PackageID: A long value corresponding to Package.PackageID.</param>	
        /// <returns>Return True to enable deletion of the package from the model. Return False to disable deletion of the package.</returns>
        public override bool EA_OnPreDeletePackage(EA.Repository repository, EA.EventProperties info) 
        {
            int objectID;
            if (int.TryParse((string)info.Get("PackageID").Value, out objectID))
                return ControllerSlt.GetControllerSlt().HandleObjectEvent(ObjectEventType.Deleted, Framework.Event.ObjectType.Package, objectID, string.Empty);
            else return true;
        }

        /// <summary>
        /// EA_OnTabChanged notifies Add-Ins that the currently open tab has changed.
        /// Diagrams do not generate the message when they are first opened - use the broadcast event EA_OnPostOpenDiagram for this purpose.
        /// Switching TAB's leaves the 'old' diagram selected. By capturing this event and forcing a scope switch, we assure that the
        /// current diagram is updated properly.
        /// </summary>
        /// <param name="repository">An EA.Repository object representing the currently open Enterprise Architect model.
        /// Poll its members to retrieve model data and user interface status information.</param>
        /// <param name="tabName">The name of the tab to which focus has been switched.</param>
        /// <param name="diagramID">The diagram ID, or 0 if switched to an Add-In tab.</param>
        public override void EA_OnTabChanged(EA.Repository repository, string tabName, int diagramID)
        {
            Logger.WriteInfo("SparxEA.Controller.EAController.EA_OnTabChanged >> Activated Tab '" + tabName + "'...");
            if (diagramID != 0)
            {
                // Could be 0 if we switched to a non-diagram tab, in which case we just ignore the event.
                ControllerSlt.GetControllerSlt().SwitchScope(ContextScope.Diagram, diagramID, repository.GetDiagramByID(diagramID).DiagramGUID);
            }
        }

        /// <summary>
        /// Retrieves a list of subordinate menu names for the given parent menu.
        /// </summary>
        /// <param name="scope">Defines the menu scope (Tree, Main Menu or Diagram)</param>
        /// <param name="parentName">Name of the parent menu.</param>
        /// <returns>List of menu items or null in case of errors.</returns>
        private object GetMenuItems(TreeScope scope, string parentName)
        {
            string topLevelMenuName = ContextSlt.GetContextSlt().GetConfigProperty(_TopLevelMenuName);
            try
            {
                // If we get names from EA that start with '-' and/or '&', we remove these to get to the internal event names....
                if (parentName[0] == '-') parentName = parentName.Substring(1);
                if (parentName[0] == '&') parentName = parentName.Substring(1);

                List<EventManager.MenuEventNode> nodeList = (parentName == topLevelMenuName) ? ControllerSlt.GetControllerSlt().GetEventList(scope, "root") :
                                                                                           ControllerSlt.GetControllerSlt().GetEventList(scope, parentName);
                var nameList = new string[nodeList.Count];
                int idx = 0;
                foreach (EventManager.MenuEventNode node in nodeList)
                {
                    // Group names must be preceded by a '-', wich indicated to EA that this is a menu group and not an item.
                    // Individual names are all preceded by a '&' character.
                    string name = string.Empty;
                    if (node.IsGroup) name += '-';
                    name += '&';
                    name += node.Name;
                    nameList[idx++] = name;
                }
                return nameList;
            }
            catch (Exception exc)
            {
                Logger.WriteError("SparxEA.Controller.EAController.getMenuItems >> Caught an exception: " + exc.ToString());
            }
            return null;
        }
        #endregion
        #region EA-Model-Transform
        // This region contains a number of entry points that are used to aid in the transformation of reference models to ECDM.
        // The methods are called from the transformation scripts and implementation is provided by the EAModelTransformSlt singleton.

        /// <summary>
        /// This operation must be called at the beginning of a new transformation session. It opens the logfile and clears caches and buffers.
        /// Parameters are:
        /// repository = the EA repository object.
        /// argList[0] = name of the logfile to be opened.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="argList"></param>
        /// <returns>ok</returns>
        public object Initialize(EA.Repository repository, object argList)
        {
            string fileName = ((string[])argList)[0];
            return EAModelTransformSlt.GetModelTransformSlt().Initialize(repository, fileName);
        }

        /// <summary>
        /// This must (may) be called at the end of the transformation session and is used to disconnect from the caches.
        /// Parameters are required by the EA call interface, but are not used here.
        /// </summary>
        /// <param name="repository">Ignored.</param>
        /// <param name="argList">Ignored.</param>
        /// <returns>ok</returns>
        public object CloseDown(EA.Repository repository, object argList)
        {
            return EAModelTransformSlt.GetModelTransformSlt().CloseDown();
        }

        /// <summary>
        /// This operation checks whether the provided class has a generalization and if so, whether the parent class has the provided stereotype.
        /// Parameters are:
        /// repository  = the EA repository object;
        /// argList[0]  = the name of the child class (logging purposes only);
        /// argList[1]  = the GUID of the child class.
        /// </summary>
        /// <param name="repository">EA repository object.</param>
        /// <param name="argList">Class name and Class GUID</param>
        /// <returns>'true' if parent has stereotype, 'false' of no parent or parent without specified stereotype.</returns>
        public object CheckParentStereotype(EA.Repository repository, object argList)
        {
            string className = ((string[])argList)[0];          // Name of the class to be examined (logging purposes only)
            string classGUID = ((string[])argList)[1];          // GUID of the class.
            string parentStereotype = ((string[])argList)[2];   // Stereotype to be checked.
            return EAModelTransformSlt.GetModelTransformSlt().CheckParentStereotype(className, classGUID, parentStereotype);
        }

        /// <summary>
        /// Checks whether the given class, identified by GUID, contains the specified stereotype.
        /// Parameters are:
        /// repository  = the EA repository object;
        /// argList[0]  = the class GUID;
        /// argList[1]  = the stereotype (case insentitive);
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="argList"></param>
        /// <returns>"true" if stereotype is defined for the class, "false" otherwise.</returns>
        public object ClassHasStereotype(EA.Repository repository, object argList)
        {
            string classGUID = ((string[])argList)[0];
            string stereotype = ((string[])argList)[1];
            return EAModelTransformSlt.GetModelTransformSlt().ClassHasStereotype(classGUID, stereotype);
        }

        /// <summary>
        /// This operation is called to create a template that is used by the EA transformation scripts to create a generalization association between the specifiec class
        /// and the specified CDT type. It can be used to force inheritance of reference classes that have been declared as 'stand alone' while in practice they should
        /// have a parent. Best example is Enumerations, which in most models are declared as stand-alone primitive types. In our model, however, an Enumeration must be
        /// a specialization of the EnumType, which in itself is a specialization of the Enum primitive. This construction is required for the creation of proper schemas.
        /// Parameters are:
        /// repository = the EA repository object;
        /// argList[0] = transformation namespace root package;
        /// argList[1] = Source class package path;
        /// argList[2] = Source class name;
        /// argList[3] = Source class GUID;
        /// argList[4] = Classifier name of target class, must be a CDT or 'reserved name' GUID;
        /// argList[5] = in case the classifier name is 'GUID', this parameter contains the GUID of the target instead of the name;
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="argList"></param>
        /// <returns>Connector intermediate code or empty string in case of errors.</returns>
        public object CreateParentConnector(EA.Repository repository, object argList)
        {
            string namespaceName = ((string[])argList)[0];                                      // First parameter is the name of the current transformation template namespace.
            string[] packagePath = ((string[])argList)[1].Split(new Char[] { '.' });            // This argument contains '.' separated path components.
            string sourceClassName = ((string[])argList)[2];                                    // Name of source class.
            string sourceGUID = ((string[])argList)[3];                                         // Second parameter is the GUID of the source class.
            string targetClassName = ((string[])argList)[4];                                    // Third parameter is the name of the target class. Must be a CDT.
            string targetGUID = (targetClassName == "GUID") ? ((string[])argList)[5] : "";      // If classifier name is 'GUID', the next parameter contains this GUID.
            return EAModelTransformSlt.GetModelTransformSlt().CreateParentConnector(namespaceName, packagePath, sourceClassName, sourceGUID, targetClassName, targetGUID);
        }

        /// <summary>
        /// This operation receives a class identifier that we have classified as a BDT. Since a BDT must be derived from a CDT, we have to examine the class attributes to determine the value type
        /// and subsequently the type of CDT that we must assign the 'parent role'. A BDT must only have a single 'value attribute', since BDT's must not be constructed. On the other hand, a BDT 
        /// may have multiple supplementary attributes. 
        /// The operation is ONLY called on CIM data types and the chosen approach is to check for the data type of the 'value' attribute (all CIM data types have exactly one such attribute).
        /// The operation is NOT used for OAGIS transformations, since OAGIS BDT's already have the proper base class.
        /// Note that, although in theory we do not need the namespace, this assures that we're searching the correct part of the package tree in case of classes that have been transformed in 
        /// the past and thus exist in multiple namespaces!
        /// Parameters are:
        /// repository = the EA repository object.
        /// args[0] = Namespace root name;
        /// args[1] = Package Path, a list of comma-separated package names all the way from transformation root to current package;
        /// args[2] = Class Name;
        /// </summary>
        /// <param name="repository">Current EA Repository.</param>
        /// <param name="argList">List of run-time arguments.</param>
        /// <returns>Empty string in case no suitable translation is possible, otherwise a constructed string: TYPE+space+GUID in which TYPE is one of 'CDTSimpleType' or 'CDTComplexType'</returns>
        public object DetermineBDTParent(EA.Repository repository, object argList)
        {
            string namespaceRoot = ((string[])argList)[0];                              // Namespace root (top of package tree)
            string[] packagePath = ((string[])argList)[1].Split(new Char[] { '.' });    // This argument contains '.' separated path components.
            string className = ((string[])argList)[2];                                  // Name of the class to be examined.
            return EAModelTransformSlt.GetModelTransformSlt().DetermineBDTParent(namespaceRoot, packagePath, className);
        }

        /// <summary>
        /// This operation is invoked on [OAGIS] BDT checks. Some reference models use 'internal' OAGIS packages that contain references to different CDT/PRIM packages.
        /// We want to replace these by our own packages and thus must inspect these packages. This particular method is called from Class transformation to obtain
        /// the 'complexity stereotype' of the eventual parent class. The operation determince whether we have to switch classes and if so, whether the new parent is
        /// a simple- or a complex type. The actual parent switch is processed from the Connector transformation (TransformCDTReference).
        /// </summary>
        /// <param name="repository">EA repository object.</param>
        /// <param name="argList">GUID and Name of class to be examined (name for logging only) and list of 'packages to be replaced'</param>
        /// <returns>Empty string in case no suitable replacement can be found, or one of 'CDTSimpleType' or 'CDTComplexType'</returns>
        public object DetermineBDTStereotype(EA.Repository repository, object argList)
        {
            string classGUID = ((string[])argList)[0];      // GUID of class to be examined.
            string className = ((string[])argList)[1];      // Name of the class to be examined.
            string forbiddenPackages = ((string[])argList)[2].ToLower();  // Comma separated list of packages that require replacement.  
            return EAModelTransformSlt.GetModelTransformSlt().DetermineBDTStereotype(classGUID, className, forbiddenPackages);
        }

        /// <summary>
        /// This operation receives a class identifier that we have classified as an EDSN LDT. Since EDSN derives LDT's directly from a PRIM, we have to 
        /// replace this PRIM parent by the correct CDT parent. A helper function, transformEDSNLogicalDataType, is used to retrieve the correct parent type + GUID.
        /// Parameters are:
        /// repository = the EA repository object.
        /// args[0] = Class Name;
        /// args[1] = Class GUID;
        /// </summary>
        /// <param name="repository">Current EA Repository</param>
        /// <param name="argList">List of arguments from script.</param>
        /// <returns>Empty string in case no suitable translation is possible, otherwise a constructed string: TYPE+space+GUID in which TYPE is one of 'CDTSimpleType' or 'CDTComplexType'</returns>
        public object DetermineEDSNLDTParent(EA.Repository repository, object argList)
        {
            string className = ((String[])argList)[0];          // Name of the class to be examined.
            string classGUID = ((String[])argList)[1];          // GUID of the class to be examined.
            return EAModelTransformSlt.GetModelTransformSlt().DetermineEDSNLDTParent(className, classGUID);
        }

        /// <summary>
        /// Receives a CDT GUID and returns the associated class name.
        /// Parameters are:
        /// repository = the EA repository object.
        /// args[0] = The GUID of the class we want to check.
        /// </summary>
        /// <param name="repository">Current EA repository</param>
        /// <param name="argList">List of arguments</param>
        /// <returns>Name of the class or empty string if nothing found.</returns>
        public object GetCDTNameByGUID(EA.Repository repository, object argList)
        {
            string GUID = ((string[])argList)[0];    // The GUID of the type to be located.
            return EAModelTransformSlt.GetModelTransformSlt().GetCDTNameByGUID(GUID);
        }

        /// <summary>
        /// Return the GUID of a Core Data Type classifier identified by args[0], if it can be found in the classifiers list...
        /// The operation also attempts to detect whether the associated type is an OAGIS CDT type or even a Primitive type and replaces these with the
        /// proper CDT type before returning the GUID. The operation is thus also used to transform 'illegal' primitive types to the correct types.
        /// Parameters are:
        /// repository = the EA repository object.
        /// args[0] = The name of the attribute for which we're trying to lookup the type..
        /// args[1] = Classifier of attribute type.
        /// args[2] = Attribute-owning class meta type: PRIM, CDT, BDT or ABIE/ACC.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="argList"></param>
        /// <returns>CDT-name+space+CDT-GUID, or empty string is nothing to replace.</returns>
        public object GetCDTClassifierGUID(EA.Repository repository, object argList)
        {
            string attributeName = ((string[])argList)[0].ToLower().Trim();  // First parameter is the attribute name.
            string classifier = ((string[])argList)[1].ToLower().Trim();     // Second parameter contains type classifier to be located.
            string classMetaType = ((string[])argList)[2];                   // Type of class we're dealing with.
            return EAModelTransformSlt.GetModelTransformSlt().GetCDTClassifierGUID(attributeName, classifier, classMetaType);
        }

        /// <summary>
        /// Return the GUID of a Primitive Data Type classifier identified by args[0], if it can be found in the classifiers list...
        /// The operation also attempts to detect whether the associated type is an OAGIS CDT type or even a Primitive type and replaces these with the
        /// proper PRIM type before returning the GUID. The operation is thus also used to transform 'illegal' primitive types to the correct types.
        /// This operation must ONLY be called on attributes that are destined to be XSD Atrributes!!
        /// Parameters are:
        /// repository = the EA repository object.
        /// args[0] = The name of the attribute for which we're trying to lookup the type..
        /// args[1] = Classifier of attribute type.
        /// args[2] = Attribute-owning class meta type: PRIM, CDT, BDT or ABIE/ACC.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="argList"></param>
        /// <returns>PRIM-name+space+PRIM-GUID, or empty string is nothing to replace.</returns>
        public object GetPRIMClassifierGUID(EA.Repository repository, object argList)
        {
            string attributeName = ((string[])argList)[0].ToLower().Trim();  // First parameter is the attribute name.
            string classifier = ((string[])argList)[1].ToLower().Trim();     // Second parameter contains type classifier to be located.
            string classMetaType = ((string[])argList)[2];                   // Type of class we're dealing with.
            return EAModelTransformSlt.GetModelTransformSlt().GetPRIMClassifierGUID(attributeName, classifier, classMetaType);
        }

        /// <summary>
        /// The method checks whether the given class name is either a primitive type or a CDT. This is primarily used to check whether we receive enumeration declarations that already have
        /// a base class (which we don't allow since we explicitly add a base class to enumerations)!
        /// We also check whether the provided name exists as one of the HR-XML 'Qualified Base Classes', these are considered CDT as well.
        /// Parameters are:
        /// repository = the EA repository object.
        /// args[0] = The name of the class we want to check.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="argList"></param>
        /// <returns>"true" if this is a primitive or a CDT type.</returns>
        public object IsPrimitiveOrCDT(EA.Repository repository, object argList)
        {
            string className = ((string[])argList)[0].ToLower().Trim();   // First parameter contains the class name to check.
            return EAModelTransformSlt.GetModelTransformSlt().IsPrimitiveOrCDT(className);
        }

        /// <summary>
        /// Explicitly called by the transformation scripts and used to load specified types. 
        /// The function is called thrice, once to load the Primitive types, once to load the CDT types based on those primitives and
        /// once to load the ECDM Data Types.
        /// This results in a classifier list that contains name/GUID tuples for the PRIM and CDT/BDT types.
        /// Parameters are:
        /// repository = the EA repository object.
        /// args[0] = The Fully Qualified Name of the package to be loaded.
        /// args[1] = Whether these are primitive types ('PRIM'), core data types ('CDT') or business data types ('BDT').
        /// We currently are interested only in the difference between PRIM and other stuff.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="argList"></param>
        /// <returns></returns>
        public object LoadClassifierTypes(EA.Repository repository, object argList)
        {
            string fullyQualifiedName = ((string[])argList)[0];                 // First parameter contains FQN of package to load.           
            bool isPrim = ((string[])argList)[1] == "PRIM";                     // 'PRIM' indicates that these are primitive types.
            return EAModelTransformSlt.GetModelTransformSlt().LoadClassifierTypes(fullyQualifiedName, isPrim);
        }

        /// <summary>
        /// This operation is called when transforming the target of a class association. It verifies whether this target is a CDT and if this is the case,
        /// the operation returns the GUID of the 'proper' CDT. Note that this might leave a number of unreferenced, 'illegal' CDT declarations in the 
        /// reference model that are not referenced anymore.  
        /// The operation also checks whether the parent of the specified class exists in one of the specified 'suspect' non-CDT packages and if so, replaces
        /// this parent by the proper CDT. 
        /// Parameters are:
        /// repository = the EA repository object.
        /// args[0] = The GUID of the source class.
        /// args[1] = The list of non-CDT (but looking like CDT) packages we must skip.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="argList"></param>
        /// <returns>new association GUID or empty string if nothing to replace.</returns>
        public object TransformCDTReference(EA.Repository repository, object argList)
        {
            string sourceGUID = ((string[])argList)[0].ToLower().Trim();    // The GUID of the source class.
            string targetClass = ((string[])argList)[1].ToLower().Trim();   // The name of the target class.
            string forbiddenPackages = ((string[])argList)[2].ToLower().Trim();     // List of non-CDT package names.  
            return EAModelTransformSlt.GetModelTransformSlt().TransformCDTReference(sourceGUID, targetClass, forbiddenPackages);
        }

        /// <summary>
        /// Write a concatenation of all string elements specified in argList to the logfile (no whitespace added)....
        /// Parameters are:
        /// repository = the EA repository object.
        /// argList[0...] = set of strings that will be concatenated and written to the logfile as one entry.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="argList"></param>
        /// <returns>ok</returns>
        public object WriteLogfile(EA.Repository repository, object argList)
        {
            string[] logElements = (string[])argList;
            Logger.WriteInfo(string.Concat(logElements));
            return "ok";
        }

        /// <summary>
        /// Return the GUID of any existing classifier that is present 'somewhere' in the namespace identified by args[0]. The operation first attempts to find the
        /// classifier in the current package. If not found, the search will be widened to any package that is part of the provided namespace root. 
        /// The operation ALWAYS skips the CCLibrary and PRIMLibrary (the first because it is not referenced by any reference model and the second since no
        /// references to PRIM are allowed, other then via the CDT types.
        /// Parameters are:
        /// repository = EA repository object.
        /// args[0] = Root of the namespace to be searched.
        /// args[1] = Current package path (e.g. the dot-separated path from the namespace root to the package that is currently being transformed). 
        /// args[2] = Classifier to be located (e.g. Type Name).
        /// 
        /// Known limitations: If a classifier exists in the current package, but has not been transformed yet (e.g. does not yet exist in the new namespace), but it DOES 
        /// exist in another package in the new namespace, the function will select this classifier as the target, even though this might be completely wrong!
        /// Solution would be to copy the original FQN for the qualifier and look in the new namespace for a similar FQN. Not sure this is feasible, though.
        /// 
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="argList"></param>
        /// <returns>GUID of classifier or empty string if not found.</returns>
        public object FindClassifierGUID(EA.Repository repository, object argList)
        {
            string namespaceRoot = ((string[])argList)[0].Trim();  // First parameter contains the (root) namespace name.
            string[] packagePath = ((string[])argList)[1].Split(new Char[] { '.' });  // This argument contains '.' separated path components.
            string classifier = ((string[])argList)[2].Trim();  // Third parameter contains qualifier typename to be located.
            return EAModelTransformSlt.GetModelTransformSlt().FindClassifierGUID(namespaceRoot, packagePath, classifier);
        }
        #endregion
    }
}
