using System.Collections.Generic;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Framework.View;
using Plugin.Application.CapabilityModel;
using Plugin.Application.CapabilityModel.API;

namespace Plugin.Application.Events.API
{
    /// <summary>
    /// Helper class that iterates over a given Capability hierarchy, collecting all classes and associations for display on a diagram.
    /// </summary>
    class DiagramItemsCollector
    {
        // Configuration properties used by this module...
        private const string _ServiceCapabilityClassBaseStereotype  = "ServiceCapabilityClassBaseStereotype";
        private const string _MessageAssemblyClassStereotype        = "MessageAssemblyClassStereotype";

        // Keep track of (extra) classes and associations to show in the diagram...
        private List<MEClass> _diagramClassList;
        private List<MEAssociation> _diagramAssocList;
        private bool _mustShowMessageAssembly;
        private bool _mustShowBusinessMessage;
        private string _messageAssemblyStereotype;
        private Diagram _currentDiagram;

        /// <summary>
        /// Properties of the class:
        /// DiagramClassList: Contains the list of all collected classes after invocation of 'Collect'.
        /// DiagramAssociationList: Contains the list of all collected associations after invocation of 'Collect'.
        /// </summary>
        internal List<MEClass> DiagramClassList { get { return this._diagramClassList; } }
        internal List<MEAssociation> DiagramAssociationList { get { return this._diagramAssocList; } }

        /// <summary>
        /// Default constructor, prepares resources for collection.
        /// </summary>
        /// <param name="thisDiagram">The diagram for which we're collecting information.</param>
        internal DiagramItemsCollector(Diagram thisDiagram)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            this._mustShowMessageAssembly = context.GetBoolSetting(FrameworkSettings._SMAddMessageAssemblyToDiagram);
            this._mustShowBusinessMessage = context.GetBoolSetting(FrameworkSettings._SMAddBusinessMsgToDiagram);
            this._messageAssemblyStereotype = context.GetConfigProperty(_MessageAssemblyClassStereotype);
            this._currentDiagram = thisDiagram;
            Reset();
        }

        /// <summary>
        /// This helper method must be invoked before a new iteration is started. It properly re-initialises resources.
        /// </summary>
        internal void Reset()
        {
            this._diagramClassList = new List<MEClass>();
            this._diagramAssocList = new List<MEAssociation>();
        }

        /// <summary>
        /// Traversal function, which iterates across all nodes in the capability hierarchy. 
        /// The function collects items that must be displayed on the updated ServiceModel diagram. It simply collects ALL classes and associations,
        /// irrespective whether they were already on the diagram before. Superfluous elements are properly handled by the View code, so this is
        /// not an issue and makes the code at this level a lot simpler.
        /// </summary>
        /// <param name="svc">My parent service, we ignore this here.</param>
        /// <param name="cap">The current Capability.</param>
        /// <returns>Always 'false', which indicates that traversal must continue until all nodes are processed.</returns>
        internal bool Collect(Service svc, Capability cap)
        {
            if (cap != null) // Safety catch, will be NULL only for Service level.  
            {
                Logger.WriteInfo("Plugin.Application.Events.API.DiagramItemsCollector.Collect >> Traversing capability '" + cap.Name + "'...");
                // In case of SOAP Messages, we might apply some restrictions...
                if (cap is MessageCapability)
                {
                    if (this._mustShowBusinessMessage)
                    {
                        this._diagramClassList.Add(cap.CapabilityClass);
                        // We're at the message level, we might (optionally) collect the Message Assembly component(s)...
                        if (this._mustShowMessageAssembly)
                        {
                            foreach (MEAssociation assoc in cap.CapabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
                            {
                                this._diagramAssocList.Add(assoc);
                                this._diagramClassList.Add(assoc.Destination.EndPoint);
                            }
                        }
                    }
                }
                // REST Operations are shown only on the diagram of their parent Resource capability...
                else if (cap is RESTOperationCapability)
                {
                    // For Operations, we must check the direct parent (the Resource).
                    if (cap is RESTOperationCapability && cap.Parent.OwningPackage == this._currentDiagram.OwningPackage)
                    {
                        this._diagramClassList.Add(cap.CapabilityClass);
                        foreach (MEAssociation assoc in cap.CapabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
                        {
                            this._diagramAssocList.Add(assoc);
                            // If the endpoint of the association is a Message Assembly, we MIGHT have to add it to the diagram manually...
                            if (assoc.Destination.EndPoint.HasStereotype(this._messageAssemblyStereotype) && this._mustShowMessageAssembly)
                            {
                                this._diagramClassList.Add(assoc.Destination.EndPoint);
                            }
                        }
                    }
                }
                // Note that in case of SOAP, the first check will catch ALL SOAP capabilities, since they all live in the package of the
                // Service Model. The second test will select all REST Resources. This might result in some 'clutter', which has to be removed by hand.
                else if (cap.OwningPackage == this._currentDiagram.OwningPackage || cap is RESTResourceCapability)
                {
                    if (!this._diagramClassList.Contains(cap.CapabilityClass))  // Make sure we collect only unique classes...
                    {
                        this._diagramClassList.Add(cap.CapabilityClass);
                        foreach (MEAssociation assoc in cap.CapabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
                        {
                            this._diagramAssocList.Add(assoc);
                            // If the endpoint of the association is a Message Assembly, we MIGHT have to add it to the diagram manually...
                            if (assoc.Destination.EndPoint.HasStereotype(this._messageAssemblyStereotype) && this._mustShowMessageAssembly)
                            {
                                this._diagramClassList.Add(assoc.Destination.EndPoint);
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
