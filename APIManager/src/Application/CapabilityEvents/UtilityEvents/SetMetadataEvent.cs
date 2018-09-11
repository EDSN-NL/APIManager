using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Framework.Util;
using Plugin.Application.CapabilityModel;

namespace Plugin.Application.Events.Util
{
    class SetMetadataEvent : EventImplementation
    {
        // Configuration properties used by this module:
        private const string _DictionaryEntryNameTag            = "DictionaryEntryNameTag";
        private const string _UniqueIDTag                       = "UniqueIDTag";
        private const string _SupplementaryAttStereotype        = "SupplementaryAttStereotype";
        private const string _FacetAttStereotype                = "FacetAttStereotype";
        private const string _ContentAttStereotype              = "ContentAttStereotype";
        private const string _BusinessDataTypeEnumStereotype    = "BusinessDataTypeEnumStereotype";

        /// <summary>
        /// No specific validation, we basically accept all locations.
        /// </summary>
        /// <returns>True</returns>
        internal override bool IsValidState() {return true;}

        /// <summary>
        /// Process the "Set Metadata Event", which assigns a DEN (Dictionary Entry Name) and a UniqueID to either a single selected class, 
        /// or all classes in a selected package. When processing a class, the class itself, all attributes and all outbound associations 
        /// (with the exception of generalization) are given DEN and UniqueID. The DEN is (as much as possible) based on UN/CEFACT naming 
        /// conventions for DEN's.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.Util.SetMetadataEvent.handleEvent >> Processing event...");
            ContextSlt context = ContextSlt.GetContextSlt();

            try
            {
                // We can be called from a class on a diagram, from a class in a package or from a package. In the latter case, the event will
                // process all classes in that package.
                // For a class, all outbound associations will be processed as well (with the exception of generalization).
                ModelSlt model = ModelSlt.GetModelSlt();
                ModelElement element = context.GetActiveElement();
                if (element.Type == ModelElementType.Class)
                {
                    if (model.LockModel(((MEClass)element).OwningPackage))
                    {
                        ProcessClass(element as MEClass);
                        model.UnlockModel(((MEClass)element).OwningPackage);
                        MessageBox.Show("Metadata assignment completed.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else if (element.Type == ModelElementType.Package)
                {
                    if (model.LockModel((MEPackage)element))
                    {
                        ProcessPackage(element as MEPackage);
                        model.UnlockModel((MEPackage)element);
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.Events.Util.SetMetadataEvent.handleEvent >> Caught an exception: '" + exc.Message + "'.");
            }
        }

        /// <summary>
        /// Creates metadata for the specified class. Sets DEN and ID for the class, all attributes and all relevant outbound associations.
        /// </summary>
        /// <param name="thisClass">Class to be processed.</param>
        void ProcessClass (MEClass thisClass)
        {
            Logger.WriteInfo("Plugin.Application.Events.Util.SetMetadataEvent.processClass >> Processing Class '" + thisClass.Name + "'...");
            var metadataCreator = new MetadataCreator();
            ContextSlt context = ContextSlt.GetContextSlt();
            string DENTag = context.GetConfigProperty(_DictionaryEntryNameTag);
            string IDTag = context.GetConfigProperty(_UniqueIDTag);

            thisClass.SetTag(DENTag, metadataCreator.MakeDEN(thisClass.Name));
            thisClass.SetTag(IDTag, metadataCreator.MakeID(thisClass));

            bool isEnum = (thisClass.HasStereotype(context.GetConfigProperty(_BusinessDataTypeEnumStereotype))) ? true : false;
            string facetStereotype = context.GetConfigProperty(_FacetAttStereotype);
            string contentStereotype = context.GetConfigProperty(_ContentAttStereotype);
            string supplementaryStereotype = context.GetConfigProperty(_SupplementaryAttStereotype);
            string[] stereotypeList = { contentStereotype, facetStereotype, supplementaryStereotype };

            foreach (MEAttribute attrib in thisClass.Attributes)
            {
                // In case of Enumerations or Facet stereotype, we don't (always) have a classifier...
                if (isEnum)
                {
                    Logger.WriteInfo("Plugin.Application.Events.Util.SetMetadataEvent.processClass >> Enumeration, no classifier!");
                    if (!attrib.HasStereotype(facetStereotype)) attrib.AddStereotype(facetStereotype);
                    attrib.SetTag(DENTag, metadataCreator.MakeDEN(thisClass.Name, attrib.Name, null));

                }
                else if (attrib.HasStereotype(facetStereotype))
                {
                    Logger.WriteInfo("Plugin.Application.Events.Util.SetMetadataEvent.processClass >> Facet, no classifier!");
                    attrib.SetTag(DENTag, metadataCreator.MakeDEN(thisClass.Name, attrib.Name, null));
                }
                else
                {
                    // No stereotype and no facet. Check if we have ANY stereotype (we need one in order to attach the metadata)...
                    if (!attrib.HasStereotype(new List<string>(stereotypeList))) attrib.AddStereotype(contentStereotype);
                    attrib.SetTag(DENTag, metadataCreator.MakeDEN(thisClass.Name, attrib.Name, attrib.Classifier.Name));
                }
                attrib.SetTag(IDTag, metadataCreator.MakeID(attrib));
            }

            foreach (MEAssociation assoc in thisClass.AssociationList)
            {
                if (assoc.TypeOfAssociation == MEAssociation.AssociationType.Aggregation || 
                    assoc.TypeOfAssociation == MEAssociation.AssociationType.Association ||
                    assoc.TypeOfAssociation == MEAssociation.AssociationType.Composition)
                {
                    assoc.SetTag(DENTag, metadataCreator.MakeDEN(thisClass.Name, assoc.Destination.Role, assoc.Destination.EndPoint.Name), 
                                 true, MEAssociation.AssociationEnd.Association);
                    assoc.SetTag(IDTag, metadataCreator.MakeID(assoc), true, MEAssociation.AssociationEnd.Association);
                }
            }
        }

        /// <summary>
        /// Process all classes within the specified package. Note that we do NOT recursively process sub-packages.
        /// </summary>
        /// <param name="thisPackage">Package to be processed.</param>
        void ProcessPackage (MEPackage thisPackage)
        {
            Logger.WriteInfo("Plugin.Application.Events.Util.SetMetadataEvent.processPackage >> Processing Package '" + thisPackage.Name + "'...");
            var metadataCreator = new MetadataCreator();
            ContextSlt context = ContextSlt.GetContextSlt();
            ProgressPanelSlt panel = ProgressPanelSlt.GetProgressPanelSlt();
            panel.ShowPanel("Processing Package: " + thisPackage.Name, thisPackage.ClassCount);
            panel.WriteInfo(0, "Metadata assignment started.");

            foreach (MEClass currClass in thisPackage.Classes)
            {
                panel.WriteInfo(1, "Processing Class: '" + currClass.Name + "'...");
                ProcessClass(currClass);
                panel.IncreaseBar(1);
            }
            panel.IncreaseBar(4);   // Just some additional steps to assure that bar is indeed at end of range (looks better ;-)
            panel.WriteInfo(0, "Metadata assignment completed.");
            panel.Done();
        }
    }
}
