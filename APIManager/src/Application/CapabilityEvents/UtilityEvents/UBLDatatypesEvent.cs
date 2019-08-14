using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Model;
using Framework.Context;

namespace Plugin.Application.Events.Util
{
    /// <summary>
    /// A utility that iterates over all UBL 2.1 Core Data Types in the Reference section and replaces the specialization association
    /// of all child data types with the corresponding ECDM CDT.
    /// </summary>
    class UBLDatatypesEvent : EventImplementation
    {
        static string _UBLCDTPath = "ECDMRoot:ReferenceMaterial:ReferenceModels:UBL-2-1:Common";
        static string _UBLCDTPackage = "UBL-UnqualifiedDataTypes-2.1";
        static string _UBLBDTPackage = "UBL-CommonBasicComponents-2.1";
        static string _ECDMCDTPath = "ECDMRoot:Framework:CoreModels";
        static string _ECDMCDTPackage = "CoreDataTypes";

        private MEPackage _targetPackage;

        internal override bool IsValidState() { return true; }

        /// <summary>
        /// UBL utility: replace all UBL CDT's by ECDM CDT's and update all BDT's to be of the correct data type.
        /// </summary>
        internal override void HandleEvent()
        {
            try
            {
                ModelSlt model = ModelSlt.GetModelSlt();
                this._targetPackage = model.FindPackage(_ECDMCDTPath, _ECDMCDTPackage);
                if (this._targetPackage == null)
                {
                    MessageBox.Show("Can't find target package '" + _ECDMCDTPath + ":" + _ECDMCDTPackage + "'! Aborting...");
                    return;
                }

                this._targetPackage.Lock();
                MEPackage srcPackage = model.FindPackage(_UBLCDTPath, _UBLCDTPackage);
                if (srcPackage != null)
                {
                    int counter = 0;
                    foreach (MEClass cdt in srcPackage.GetClasses("UDT"))
                    {
                        counter += MoveChildren(cdt);
                    }
                    Logger.WriteWarning("Processed '" + counter + "' CDT elements.");
                }
                this._targetPackage.Unlock();

                MEPackage bdtPackage = model.FindPackage(_UBLCDTPath, _UBLBDTPackage);
                if (bdtPackage != null)
                {
                    bdtPackage.Lock();
                    int counter = 0;
                    foreach (MEClass bdt in bdtPackage.GetClasses())
                    {
                        UpdateType(bdt);
                        counter++;
                    }
                    Logger.WriteWarning("Updated '" + counter + "' Data types.");
                }

                MessageBox.Show("Finished moving UBL Data Types!");
            }
            catch (Exception exc)
            {
                Logger.WriteError("Oops, Caught exception:" + Environment.NewLine + exc.ToString());
            }
        }

        private int MoveChildren(MEClass cdt)
        {
            Logger.WriteWarning("Processing UBL CDT '" + cdt.Name + "'...");

            int counter = 0;
            MEClass targetClass = this._targetPackage.FindClass(cdt.Name);
            EndpointDescriptor targetEnd = new EndpointDescriptor(targetClass);
            if (targetClass != null)
            {
                Logger.WriteWarning("..Found corresponding target class, moving associations...");
                List<Tuple<MEClass, MEAssociation>> obsoleteList = new List<Tuple<MEClass, MEAssociation>>();
                foreach (MEAssociation child in cdt.ChildAssociationList())
                {
                    Logger.WriteWarning("....Found child class '" + child.Source.EndPoint.Name + "'...");
                    EndpointDescriptor sourceEnd = new EndpointDescriptor(child.Source.EndPoint);
                    if (child.Destination.EndPoint.HasStereotype("UDT"))
                    {
                        Logger.WriteWarning("....Child is associated with UBL CDT '" + child.Destination.EndPoint.Name + "', moving...");
                        child.Source.EndPoint.CreateAssociation(sourceEnd, targetEnd, MEAssociation.AssociationType.Generalization);
                        obsoleteList.Add(new Tuple<MEClass, MEAssociation>(child.Destination.EndPoint, child));
                        counter++;
                    }
                }

                // Since we're now done with the iterator, we can safely delete all obsolete associations...
                foreach (var tuple in obsoleteList)
                {
                    Logger.WriteWarning("....Removing obsolete association between '" + tuple.Item1.Name + "' and '" +
                                        tuple.Item2.Destination.EndPoint.Name + "'...");
                    tuple.Item1.DeleteAssociation(tuple.Item2);
                }
            }
            return counter;
        }

        /// <summary>
        /// Assures that the BDT is indeed a BDT and has the correct stereotype...
        /// </summary>
        /// <param name="bdt"></param>
        private void UpdateType(MEClass bdt)
        {
            Logger.WriteWarning("Processing UBL BDT '" + bdt.Name + "'...");
            ModelSlt model = ModelSlt.GetModelSlt();
            ContextSlt context = ContextSlt.GetContextSlt();
            MEClass parent = bdt.Parents[0];
            string simpleStereotype = context.GetConfigProperty("SimpleBDTStereotype");
            string complexStereotype = context.GetConfigProperty("ComplexBDTStereotype");

            if (parent.HasStereotype("CDTSimpleType")) model.UpdateModelElementType(bdt, simpleStereotype);
            else model.UpdateModelElementType(bdt, complexStereotype);
        }
    }
}

        