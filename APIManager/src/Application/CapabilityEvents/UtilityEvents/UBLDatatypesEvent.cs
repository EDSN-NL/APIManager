using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Model;

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
        static string _ECDMCDTPath = "ECDMRoot:Framework:CoreModels";
        static string _ECDMCDTPackage = "CoreDataTypes";

        private MEPackage _targetPackage;

        internal override bool IsValidState() { return true; }

        /// <summary>
        /// UBL utility: replace all UBL CDT's by ECDM CDT's
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
                    foreach (MEClass cdt in srcPackage.GetClasses("UDT"))
                    {
                        MoveChildren(cdt);
                    }
                }
                this._targetPackage.Unlock();
                MessageBox.Show("Finished moving UBL Data Types!");
            }
            catch (Exception exc)
            {
                Logger.WriteError("Oops, Caught exception:" + Environment.NewLine + exc.ToString());
            }
        }

        private void MoveChildren(MEClass ubl_cdt)
        {
            Logger.WriteWarning("Processing UBL CDT '" + ubl_cdt.Name + "'...");

            MEClass targetClass = this._targetPackage.FindClass(ubl_cdt.Name);
            EndpointDescriptor targetEnd = new EndpointDescriptor(targetClass);
            if (targetClass != null)
            {
                Logger.WriteWarning("..Found corresponding target class, moving associations...");
                List<Tuple<MEClass, MEAssociation>> obsoleteList = new List<Tuple<MEClass, MEAssociation>>();
                foreach (MEAssociation child in ubl_cdt.ChildAssociationList())
                {
                    Logger.WriteWarning("....Found child class '" + child.Source.EndPoint.Name + "'...");
                    EndpointDescriptor sourceEnd = new EndpointDescriptor(child.Source.EndPoint);
                    if (child.Destination.EndPoint.HasStereotype("UDT"))
                    {
                        Logger.WriteWarning("....Child is associated with UBL CDT '" + child.Destination.EndPoint.Name + "', moving...");
                        child.Source.EndPoint.CreateAssociation(sourceEnd, targetEnd, MEAssociation.AssociationType.Generalization);
                        obsoleteList.Add(new Tuple<MEClass, MEAssociation>(child.Destination.EndPoint, child));
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
        }
    }
}

        