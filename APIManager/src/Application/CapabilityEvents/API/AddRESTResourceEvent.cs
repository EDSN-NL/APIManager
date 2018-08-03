using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Framework.Util;
using Framework.View;
using Plugin.Application.CapabilityModel;
using Plugin.Application.CapabilityModel.API;
using Plugin.Application.Forms;

namespace Plugin.Application.Events.API
{
    class AddRESTResourceEvent : EventImplementation
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

            if (!svcContext.Valid || svcContext.MyDiagram == null || collectionParent == null)
            {
                Logger.WriteError("Plugin.Application.Events.API.AddResourcesEvent.HandleEvent >> Illegal or corrupt context, event aborted!");
                return;
            }
            else if (svcContext.Type != ServiceContext.ServiceType.REST)
            {
                Logger.WriteWarning("Operation only suitable for REST Services!");
                return;
            }

            // Check what type of diagram has been selected, must be top-level for root-level resource collections...
            if (this._isRootLevelResource && svcContext.MyDiagram.OwningPackage != svcContext.SVCModelPackage)
            {
                MessageBox.Show("Root-level Resources can only be added from the Service Model diagram!", 
                                "Wrong Diagram", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check the Resource that we want to use as parent. If this is a root collection, we can only add the child Resource
            // when we are at the diagram of that root collection!
            // RULE: A child Resource has EXACTLY ONE parent Resource Collection! 
            if (!this._isRootLevelResource && svcContext.MyDiagram.OwningPackage != svcContext.ResourceClass.OwningPackage)
            {
                MessageBox.Show("Child Resources can only be added from the diagram of the owning Resource Collection!",
                                "Wrong Diagram", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var myService = new RESTService(svcContext.Hierarchy, context.GetConfigProperty(_ServiceDeclPkgStereotype));
            Capability parent;
            IRESTResourceContainer resourceContainer;
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
                if (svcContext.LockModel() && dialog.ShowDialog() == DialogResult.OK)
                {
                    List<RESTResourceDeclaration> resourceList = new List<RESTResourceDeclaration> { dialog.Resource };
                    bool result = resourceContainer.AddResources(resourceList, dialog.MinorVersionIndicator);
                    if (result)
                    {
                        // Mark service as 'modified' for configuration management and add to diagram in different color...
                        myService.Dirty();
                        myService.Paint(svcContext.MyDiagram);

                        // Collect the new classes and associations that must be shown on the diagram...
                        DiagramItemsCollector collector = new DiagramItemsCollector(svcContext.MyDiagram);
                        parent.Traverse(collector.Collect);

                        // This updates the selected diagram, which could be in a resource package...
                        svcContext.MyDiagram.AddClassList(collector.DiagramClassList);
                        svcContext.MyDiagram.AddAssociationList(collector.DiagramAssociationList);
                        svcContext.MyDiagram.Redraw();
                        svcContext.DeclarationPackage.Refresh();
                        MessageBox.Show("Resources have been added successfully.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else MessageBox.Show("Failed to add one or more Resources!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                svcContext.UnlockModel();
            }
        }
    }
}
