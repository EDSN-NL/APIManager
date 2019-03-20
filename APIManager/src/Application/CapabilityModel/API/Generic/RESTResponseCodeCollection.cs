using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Framework.Model;
using Framework.Logging;
using Framework.Context;
using Framework.Util;
using Plugin.Application.Forms;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// Helper class that maintains a series of predefined REST response codes, which the user can use to quickly
    /// assign consistent sets of response codes to REST operations.
    /// </summary>
    internal sealed class RESTResponseCodeCollection
    {
        // Configuration properties used by this module:
        private const string _RESTResponseCodeCollectionStereotype  = "RESTResponseCodeCollectionStereotype";
        private const string _CoreDataTypesPathName                 = "CoreDataTypesPathName";
        private const string _RESTResponseCodeAttributeClassifier   = "RESTResponseCodeAttributeClassifier";

        private MEClass _collectionClass;                           // UML representation of the collection.
        private List<RESTOperationResultDeclaration> _collection;   // The actual collection.
        private MEDataType _attribClassifier;                       // Classifier to be used for attributes.
        private Service _myService;                                 // Service that owns the collection.

        /// <summary>
        /// Get or Set the name of the collection. Note that, if we assign a name to an object that is not yet associated with
        /// an MEClass, we create the class and assign any attributes that are already in the collection.
        /// And if we assign a new name to a collection and a class already exists with that name, an exception will be thrown.
        /// </summary>
        internal string Name
        {
            get { return this._collectionClass != null? this._collectionClass.Name: string.Empty; }
            set { SetName(value); }
        }

        /// <summary>
        /// Returns the list of Operation Result Declaration objects that comprises the collection.
        /// </summary>
        internal List<RESTOperationResultDeclaration> Collection { get { return this._collection; } }

        /// <summary>
        /// Standard constructor. Either initialises a new collection or loads an existing one.
        /// </summary>
        /// <param name="collectionName">User-defined collection name.</param>
        /// <param name="myService">The service that 'owns' the collection.</param>
        /// <exception cref="InvalidOperationException">Is thrown when we can't find the attribute classifier.</exception>
        internal RESTResponseCodeCollection(string collectionName, Service myService)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> Creating instance '" +
                             collectionName + "' for Service '" + myService.Name + "'...");

            this._collection = new List<RESTOperationResultDeclaration>();
            ContextSlt context = ContextSlt.GetContextSlt();
            this._myService = myService;
            this._attribClassifier = ModelSlt.GetModelSlt().FindDataType(context.GetConfigProperty(_CoreDataTypesPathName),
                                                                         context.GetConfigProperty(_RESTResponseCodeAttributeClassifier));
            string collectionStereotype = context.GetConfigProperty(_RESTResponseCodeCollectionStereotype);
            MEPackage owningPkg = myService.ModelPkg;

            if (this._attribClassifier == null)
            {
                string msg = "Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> Unable to find classifier for collection attributes!";
                Logger.WriteError(msg);
                throw new InvalidOperationException(msg);
            }

            this._collectionClass = owningPkg.FindClass(collectionName, collectionStereotype);
            if (this._collectionClass != null)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> Found existing class!");
                foreach (MEAttribute attrib in this._collectionClass.Attributes)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> Adding '" + attrib.Name + "'...");
                    this._collection.Add(new RESTOperationResultDeclaration(attrib.Name, attrib.Annotation));
                }
            }
            else
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> No class yet, creating one...");
                this._collectionClass = owningPkg.CreateClass(collectionName, collectionStereotype);
            }
        }

        /// <summary>
        /// This constructor is called with an existing Collection class and initialises the collection from that class.
        /// </summary>
        /// <param name="collectionClass">Collection class.</param>
        /// <exception cref="InvalidOperationException">Thrown when a collection class is passed that is not of the correct stereotype or 
        /// when we can't find the correct attribute classifier.</exception>
        /// <param name="myService">Service that 'owns' the collection.</param>
        internal RESTResponseCodeCollection(MEClass collectionClass, Service myService)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> Creating instance from class '" +
                              collectionClass.Name + "'...");

            ContextSlt context = ContextSlt.GetContextSlt();
            this._collection = new List<RESTOperationResultDeclaration>();
            this._collectionClass = collectionClass;
            this._myService = myService;
            this._attribClassifier = ModelSlt.GetModelSlt().FindDataType(context.GetConfigProperty(_CoreDataTypesPathName),
                                                                         context.GetConfigProperty(_RESTResponseCodeAttributeClassifier));

            string collectionStereotype = context.GetConfigProperty(_RESTResponseCodeCollectionStereotype);
            if (!collectionClass.HasStereotype(collectionStereotype))
            {
                string msg = "Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> Attempt to create collection from wrong classifier '" + collectionClass.Name + "'!";
                Logger.WriteError(msg);
                throw new InvalidOperationException(msg);
            }
            if (this._attribClassifier == null)
            {
                string msg = "Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> Unable to find classifier for collection attributes!";
                Logger.WriteError(msg);
                throw new InvalidOperationException(msg);
            }

            foreach (MEAttribute attrib in this._collectionClass.Attributes)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> Adding '" + attrib.Name + "'...");
                this._collection.Add(new RESTOperationResultDeclaration(attrib.Name, attrib.Annotation));
            }
        }

        /// <summary>
        /// Default contstructor, creates an empty collection that does not contain any information. In order to turn this into a valid
        /// collection, at least a name must be assigned.
        /// </summary>
        /// <param name="myService">The 'owner' of the collection. This parameter is always required.</param>
        /// <exception cref="InvalidOperationException">Thrown when a collection class is passed that is not of the correct stereotype or 
        /// when we can't find the correct attribute classifier.</exception>
        internal RESTResponseCodeCollection(Service myService)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            this._collectionClass = null;
            this._collection = new List<RESTOperationResultDeclaration>();
            this._myService = myService;
            this._attribClassifier = ModelSlt.GetModelSlt().FindDataType(context.GetConfigProperty(_CoreDataTypesPathName),
                                                                         context.GetConfigProperty(_RESTResponseCodeAttributeClassifier));
            if (this._attribClassifier == null)
            {
                string msg = "Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> Unable to find classifier for collection attributes!";
                Logger.WriteError(msg);
                throw new InvalidOperationException(msg);
            }
        }

        /// <summary>
        /// This function is invoked to add a new result code to this collection. It displays the Response Code Dialog, which
        /// facilitates the user in creating a new result object. The created object is added to the result list for this collection as long as
        /// it has a unique code.
        /// </summary>
        /// <returns>Newly created result record or NULL in case of errors or duplicates or user cancel.</returns>
        internal RESTOperationResultDeclaration AddOperationResult()
        {
            var newResult = new RESTOperationResultDeclaration(RESTOperationResultCapability.ResponseCategory.Unknown);
            ModelSlt model = ModelSlt.GetModelSlt();
            ContextSlt context = ContextSlt.GetContextSlt();
            using (var dialog = new RESTResponseCodeDialog(newResult))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (!this._collection.Contains(dialog.OperationResult))
                    {
                        this._collection.Add(dialog.OperationResult);
                        newResult = dialog.OperationResult;
                        if (this._collectionClass != null)
                            this._collectionClass.CreateAttribute(dialog.OperationResult.ResultCode, this._attribClassifier,
                                                                  AttributeType.Attribute, null, new Cardinality(Cardinality._Mandatory),
                                                                  false, dialog.OperationResult.Description);
                    }
                    else
                    {
                        MessageBox.Show("Duplicate operation result code, please try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        newResult = null;
                    }
                }
                else newResult = null;
            }
            return newResult;
        }

        /// <summary>
        /// Deletes an operation result from the collection. If the collection does not contain the code, the operation is ignored.
        /// </summary>
        /// <param name="code">Operation Result Code to be deleted.</param>
        /// <exception cref="ArgumentException">Thrown when we can't find the specified 'code'.</exception>
        internal void DeleteOperationResult(string code)
        {
            foreach (RESTOperationResultDeclaration decl in this._collection)
            {
                if (decl.ResultCode == code)
                {
                    if (this._collectionClass != null)
                    {
                        MEAttribute delAttr = this._collectionClass.FindAttribute(code);
                        if (delAttr != null) this._collectionClass.DeleteAttribute(delAttr);
                        else break;
                    }
                    this._collection.Remove(decl);
                    return;
                }
            }
            string msg = "Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection.DeleteOperationResult >> Can't find existing attribute '" + this._collectionClass.Name + "." + code + "'!";
            Logger.WriteError(msg);
            throw new ArgumentException(msg);
        }

        /// <summary>
        /// This function is invoked to edit an existing result code. It displays the Response Code Dialog, which facilitates the user in 
        /// changing the result object. The updated object is added to the result list for this collection as long as
        /// it (still) has a unique code.
        /// </summary>
        /// <returns>Updated result record or NULL in case of errors or duplicates or user cancel.</returns>
        /// <exception cref="ArgumentException">Thrown when the received code does not match an existing attribute.</exception>
        internal RESTOperationResultDeclaration EditOperationResult(string code)
        {
            RESTOperationResultDeclaration originalDecl = null;
            RESTOperationResultDeclaration newDecl = null;
            MEAttribute myAttribute = null;                 // will be used to persist the declaration in the model.
            foreach (RESTOperationResultDeclaration decl in this._collection)
            {
                if (decl.ResultCode == code)
                {
                    originalDecl = decl;
                    break;
                }
            }

            if (originalDecl != null)
            {
                if (this._collectionClass != null)
                {
                    foreach (MEAttribute attrib in this._collectionClass.Attributes)
                    {
                        if (attrib.Name == code)
                        {
                            myAttribute = attrib;
                            break;
                        }
                    }

                    if (myAttribute == null)
                    {
                        string msg = "Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection.EditOperationResult >> Can't find existing attribute '" + this._collectionClass.Name + "." + code + "'!";
                        Logger.WriteError(msg);
                        throw new ArgumentException(msg);
                    }
                }

                using (var dialog = new RESTResponseCodeDialog(originalDecl))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        if (dialog.OperationResult.ResultCode == originalDecl.ResultCode || !this._collection.Contains(dialog.OperationResult))
                        {
                            newDecl = dialog.OperationResult;
                            if (myAttribute != null) myAttribute.Annotation = dialog.OperationResult.Description;
                            if (newDecl.ResultCode != originalDecl.ResultCode)
                            {
                                this._collection.Remove(originalDecl);
                                this._collection.Add(newDecl);
                                if (myAttribute != null) myAttribute.Name = newDecl.ResultCode;
                            }
                            else originalDecl.Description = newDecl.Description; 
                        }
                        else
                        {
                            MessageBox.Show("Changing result code resulted in duplicate code, please try again!",
                                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            newDecl = null;
                        }
                    }
                    else newDecl = null;
                }
            }
            else
            {
                string msg = "Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection.EditOperationResult >> Can't find existing attribute '" + this._collectionClass.Name + "." + code + "'!";
                Logger.WriteError(msg);
                throw new ArgumentException(msg);
            }
            return newDecl;
        }

        /// <summary>
        /// Rename the collection. If another collection class already exists with the specified name, an exception will be thrown!
        /// And if the class does not yet exists, it is created with the given name and all currently collected Response Codes will be
        /// added as attributes of the class.
        /// </summary>
        /// <param name="name">(new) name for the class.</param>
        /// <exception cref="ArgumentException">Will be thrown when a class already exists with the given name.</exception>
        private void SetName(string name)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string collectionStereotype = context.GetConfigProperty(_RESTResponseCodeCollectionStereotype);
            MEPackage owningPkg = this._myService.ModelPkg;
            MEClass newClass = owningPkg.FindClass(name, collectionStereotype);

            // First of all, check spurious name assignments (nothing to do in that case)...
            if (this._collectionClass != null && this._collectionClass.Name == name) return; 

            if (newClass != null)
            {
                string msg = "Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection.SetName >> New name '" + this._collectionClass.Name + "' already exists!";
                Logger.WriteError(msg);
                throw new ArgumentException(msg);
            }
            else
            {
                // Check whether we already own a class (with another name)...
                if (this._collectionClass != null)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> Existing class with name '" +
                                     this._collectionClass.Name + "', rename to '" + name + "'...");
                    this._collectionClass.Name = name;
                }
                else
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> No class yet, creating one...");
                    this._collectionClass = owningPkg.CreateClass(name, collectionStereotype);

                    if (this._collection.Count > 0)
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> We already have some attributes, move to new class...");
                        foreach (RESTOperationResultDeclaration attrib in this._collection)
                        {
                            this._collectionClass.CreateAttribute(attrib.ResultCode, this._attribClassifier,
                                                                  AttributeType.Attribute, null, new Cardinality(Cardinality._Mandatory), 
                                                                  false, attrib.Description);
                        }
                    }
                }
            }
        }
    }
}
