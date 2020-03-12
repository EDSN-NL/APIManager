using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Framework.Util;
using Plugin.Application.CapabilityModel;
using Plugin.Application.CapabilityModel.API;
using Plugin.Application.Forms;

namespace Plugin.Application.Events.API
{
    class AddRESTResourceEvent : MenuEventImplementation
    {
        // Configuration properties used by this module...
        private const string _ResourceClassStereotype           = "ResourceClassStereotype";
        private const string _ServiceModelPkgName               = "ServiceModelPkgName";
        private const string _ServiceDeclPkgStereotype          = "ServiceDeclPkgStereotype";
        private const string _InterfaceContractClassStereotype  = "InterfaceContractClassStereotype";
        private const string _MessageAssemblyClassStereotype    = "MessageAssemblyClassStereotype";
        private const string _ArchetypeTag                      = "ArchetypeTag";

        // Set to TRUE if we're adding resources to the top-level (Interface)...
        private bool _isRootLevelResource; 

        /// <summary>
        /// Resources can be added to other resources or to a RESTInterface capability.
        /// Resources of archetype Unknown or Document can NOT be used as parent.
        /// The default event context checker has verified the Stereotype of the selected class to be either an Interface or a Resource.
        /// We have to check whether we have the correct class and the correct archetype...
        /// </summary>
        /// <returns>True on correct context, false otherwise.</returns>
        internal override bool IsValidState()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            MEClass currentClass = context.CurrentClass;
            try
            {
                if (currentClass != null)
                {
                    if (currentClass.HasStereotype(context.GetConfigProperty(_ResourceClassStereotype)))
                    {
                        string typeTagValue = currentClass.GetTag(context.GetConfigProperty(_ArchetypeTag));
                        if (!string.IsNullOrEmpty(typeTagValue))
                        {
                            var type = EnumConversions<RESTResourceCapability.ResourceArchetype>.StringToEnum(typeTagValue);
                            if (type != RESTResourceCapability.ResourceArchetype.Unknown &&
                                type != RESTResourceCapability.ResourceArchetype.Document) return true;
                        }
                    }
                    else return true;   // Must be an interface (we can only get Interface or Resource in this method)!
                }
            }
            catch
            {
                // We could get exceptions if the context is wrong and we get the wrong enumeration. Ignore all those!
            }
            return false;
        }

        /// <summary>
        /// Processes the 'Add Operations' menu click event.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.AddResourcesEvent.HandleEvent >> Processing an add resource collection menu click...");

            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);
            this._isRootLevelResource = svcContext.InterfaceClass != null;
            MEClass collectionParent = this._isRootLevelResource ? svcContext.InterfaceClass : svcContext.ResourceClass;
            string errorMsg = string.Empty;
            bool isError = false;               // In case errorMsg is not empty, 'true' = error, 'false' = warning.

            // Perform a series of precondition tests...
            if (!svcContext.Valid || svcContext.MyDiagram == null || collectionParent == null)
            {
                if (!svcContext.HasValidRepositoryDescriptor) errorMsg = "No valid Repository Descriptor has been defined for the currently open project!";
                else errorMsg = "Illegal or corrupt context, operation aborted!";
                isError = true;
            }
            else if (svcContext.Type != Service.ServiceArchetype.REST)
                errorMsg = "Operation only suitable for REST Services!";
            else if (!Service.UpdateAllowed(svcContext.ServiceClass))
                errorMsg = "Service must be in checked-out state for resources to be added!";
            else if (!this._isRootLevelResource && svcContext.MyDiagram.OwningPackage != svcContext.ResourceClass.OwningPackage)
                errorMsg = "Child Resources can only be added from the diagram of the owning Resource Collection!";
            else if (this._isRootLevelResource && svcContext.MyDiagram.OwningPackage != svcContext.SVCModelPackage)
                errorMsg = "Root-level resources can only be added from the Service Model diagram!";
            else if (!svcContext.LockModel())
            {
                errorMsg = "Unable to lock the model!";
                isError = true;
            }

            if (errorMsg != string.Empty)
            {
                if (isError)
                {
                    Logger.WriteError("Plugin.Application.Events.API.AddRESTResourceEvent.HandleEvent >> " + errorMsg);
                    MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    Logger.WriteWarning(errorMsg);
                    MessageBox.Show(errorMsg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                svcContext.UnlockModel();
                return;
            }

            Capability parent;
            IRESTResourceContainer resourceContainer;
            var myService = new RESTService(svcContext.Hierarchy, context.GetConfigProperty(_ServiceDeclPkgStereotype));
            if (this._isRootLevelResource)
            {
                var itfCap = new RESTInterfaceCapability(svcContext.InterfaceClass);
                parent = itfCap;
                resourceContainer = itfCap;
            }
            else
            {
                var resourceCap = new RESTResourceCapability(svcContext.ResourceClass);
                parent = resourceCap;
                resourceContainer = resourceCap;
            }

            var newResource = new RESTResourceDeclaration(parent);
            using (var dialog = new RESTResourceDialog(newResource))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    List<RESTResourceDeclaration> resourceList = new List<RESTResourceDeclaration> { dialog.Resource };
                    bool result = resourceContainer.AddResources(resourceList, dialog.MinorVersionIndicator);
                    if (result)
                    {
                        // Mark service as 'modified' for configuration management and add to diagram in different color...
                        myService.Dirty();
                        myService.Paint(svcContext.ServiceDiagram);

                        // Collect the new classes and associations that must be shown on the diagram...
                        DiagramItemsCollector collector = new DiagramItemsCollector(svcContext.MyDiagram);
                        parent.Traverse(collector.Collect);

                        // This updates the selected diagram, which could be in a resource package...
                        svcContext.MyDiagram.AddClassList(collector.DiagramClassList);
                        svcContext.MyDiagram.AddAssociationList(collector.DiagramAssociationList);
                        svcContext.MyDiagram.Redraw();
                        svcContext.DeclarationPackage.RefreshPackage();
                        MessageBox.Show("Resources have been added successfully.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else MessageBox.Show("Failed to add one or more Resources!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                svcContext.UnlockModel();
            }
        }
    }
}
