using System;
using System.Collections.Generic;
using Plugin.Application.Forms;
using Framework.Model;
using Framework.Logging;
using Framework.Context;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// Manages the collection of request- and/or response header parameters for the associated API. By having the actual collections being managed
    /// centrally, we avoid a lot of duplicated classes and overhead for each of the operations, which now only have to keep track of parameter
    /// identifiers instead of the complete parameter.
    /// </summary>
    internal sealed class RESTServiceHeaderParameterMgr
    {
        // Configuration properties used by this module:
        private const string _RequestHeadersClassName   = "RequestHeadersClassName";
        private const string _ResponseHeadersClassName  = "ResponseHeadersClassName";

        internal enum Scope { Request, Response }                       // To be used to specify the collection to be used for requests.

        private RESTHeaderParameterCollection _requestHeaders;          // All request headers used by this API.
        private RESTHeaderParameterCollection _responseHeaders;         // All response headers used by this API.
        private RESTService _myService = null;                          // Service for which we maintain the API-based list.

        /// <summary>
        /// Returns the service associated with this parameter manager.
        /// </summary>
        internal RESTService Service { get { return this._myService; } }

        /// <summary>
        /// Initialize the header structure for the given service. We attempt to locate the classes and when found, initialize the parameters from these classes.
        /// When no class(es) is/are found, we assume that no headers have been created (yet) and we simple initialize an empty structure.
        /// </summary>
        /// <param name="thisService">Optional Service for which we have to maintain the collection.</param>
        internal RESTServiceHeaderParameterMgr(RESTService thisService)
        {
            this._myService = thisService;
            string requestHeadersName = ContextSlt.GetContextSlt().GetConfigProperty(_RequestHeadersClassName);
            string responseHeadersName = ContextSlt.GetContextSlt().GetConfigProperty(_ResponseHeadersClassName);

            if (thisService.ServiceClass != null)
            {
                List<MEAssociation> target = thisService.ServiceClass.FindAssociationsByAssociationProperties(null, requestHeadersName);
                this._requestHeaders = (target.Count > 0) ? new RESTHeaderParameterCollection(target[0].Destination.EndPoint) :
                                                            new RESTHeaderParameterCollection(RESTCollection.CollectionScope.API);
                target = thisService.ServiceClass.FindAssociationsByAssociationProperties(null, responseHeadersName);
                this._responseHeaders = (target.Count > 0) ? new RESTHeaderParameterCollection(target[0].Destination.EndPoint) :
                                                             new RESTHeaderParameterCollection(RESTCollection.CollectionScope.API);
            }
            else
            {
                this._requestHeaders = new RESTHeaderParameterCollection(RESTCollection.CollectionScope.API);
                this._responseHeaders = new RESTHeaderParameterCollection(RESTCollection.CollectionScope.API);
            }
        }

        /// <summary>
        /// Function used to add a new parameter to the collection identified by 'Scope'. The user is presented with a parameter dialog, which
        /// facilitates specification of the new parameter properties. If the parameter has been added successfully to the collection and 
        /// the collection count is now 1, we serialize the collection to an UML class and create an association between the service and the
        /// collection. This assures us that collection classes are created only when there is a need for them.
        /// </summary>
        /// <param name="scope">Identifies the collection to be used.</param>
        /// <returns>Newly created parameter descriptor or NULL in case of duplicate or user-cancel.</returns>
        internal RESTHeaderParameterDescriptor AddParameter(Scope scope)
        {
            RESTHeaderParameterDescriptor result = null;

            if (scope == Scope.Request)
            {
                string requestHeadersName = ContextSlt.GetContextSlt().GetConfigProperty(_RequestHeadersClassName);
                result = this._requestHeaders.AddParameter();
                if (this._requestHeaders.Collection.Count == 1 && 
                    this._myService.ServiceClass != null && 
                    this._requestHeaders.CollectionClass == null)
                {
                    // Added the first parameter to the collection, let's serialize and create an association...
                    this._requestHeaders.Serialize(requestHeadersName, this._myService.ModelPkg, RESTCollection.CollectionScope.API);
                    if (this._requestHeaders.CollectionClass != null)
                    {
                        var sourceEndpoint = new EndpointDescriptor(this._myService.ServiceClass, "1", string.Empty, null, false);
                        var reqHeadersEndpoint = new EndpointDescriptor(this._requestHeaders.CollectionClass, "1", requestHeadersName, null, true);
                        this._myService.ServiceClass.CreateAssociation(sourceEndpoint, reqHeadersEndpoint, MEAssociation.AssociationType.MessageAssociation);
                    }
                    else Logger.WriteWarning("Unable to serialize Request Headers collection for service '" + this._myService.Name + "'!");
                }
            }
            else
            {
                string responseHeadersName = ContextSlt.GetContextSlt().GetConfigProperty(_ResponseHeadersClassName);
                result = this._responseHeaders.AddParameter();
                if (this._responseHeaders.Collection.Count == 1 && 
                    this._myService.ServiceClass != null && 
                    this._responseHeaders.CollectionClass == null)
                {
                    // Added the first parameter to the collection, let's serialize and create an association...
                    this._responseHeaders.Serialize(responseHeadersName, this._myService.ModelPkg, RESTCollection.CollectionScope.API);
                    if (this._responseHeaders.CollectionClass != null)
                    {
                        var sourceEndpoint = new EndpointDescriptor(this._myService.ServiceClass, "1", string.Empty, null, false);
                        var rspHeadersEndpoint = new EndpointDescriptor(this._responseHeaders.CollectionClass, "1", responseHeadersName, null, true);
                        this._myService.ServiceClass.CreateAssociation(sourceEndpoint, rspHeadersEndpoint, MEAssociation.AssociationType.MessageAssociation);
                    }
                    else Logger.WriteWarning("Unable to serialize Response Headers collection for service '" + this._myService.Name + "'!");
                }
            }
            return result;
        }

        /// <summary>
        /// Function used to add a new parameter to the collection identified by 'Scope'. The user is presented with a parameter dialog, which
        /// facilitates specification of the new parameter properties. If the parameter has been added successfully to the collection and 
        /// the collection count is now 1, we serialize the collection to an UML class and create an association between the service and the
        /// collection. This assures us that collection classes are created only when there is a need for them.
        /// Note that the ID of the issued parameter is ignored by the collections and a formal ID will be assigned instead. The return parameter
        /// contains this formal ID.
        /// </summary>
        /// <param name="scope">Identifies the collection to be used.</param>
        /// <returns>Newly created parameter descriptor or NULL in case of duplicate or user-cancel. The return parameter contains the assigned ID.</returns>
        internal RESTHeaderParameterDescriptor AddParameter(Scope scope, RESTHeaderParameterDescriptor parameter)
        {
            RESTHeaderParameterDescriptor result = null;
            if (scope == Scope.Request)
            {
                string requestHeadersName = ContextSlt.GetContextSlt().GetConfigProperty(_RequestHeadersClassName);
                result = this._requestHeaders.AddParameter(parameter);
                if (this._requestHeaders.Collection.Count == 1 &&
                    this._myService.ServiceClass != null &&
                    this._requestHeaders.CollectionClass == null)
                {
                    // Added the first parameter to the collection, let's serialize and create an association...
                    this._requestHeaders.Serialize(requestHeadersName, this._myService.ModelPkg, RESTCollection.CollectionScope.API);
                    if (this._requestHeaders.CollectionClass != null)
                    {
                        var sourceEndpoint = new EndpointDescriptor(this._myService.ServiceClass, "1", string.Empty, null, false);
                        var reqHeadersEndpoint = new EndpointDescriptor(this._requestHeaders.CollectionClass, "1", requestHeadersName, null, true);
                        this._myService.ServiceClass.CreateAssociation(sourceEndpoint, reqHeadersEndpoint, MEAssociation.AssociationType.MessageAssociation);
                    }
                    else Logger.WriteWarning("Unable to serialize Request Headers collection for service '" + this._myService.Name + "'!");
                }
            }
            else
            {
                string responseHeadersName = ContextSlt.GetContextSlt().GetConfigProperty(_ResponseHeadersClassName);
                result = this._responseHeaders.AddParameter(parameter);
                if (this._responseHeaders.Collection.Count == 1 &&
                    this._myService.ServiceClass != null &&
                    this._responseHeaders.CollectionClass == null)
                {
                    // Added the first parameter to the collection, let's serialize and create an association...
                    this._responseHeaders.Serialize(responseHeadersName, this._myService.ModelPkg, RESTCollection.CollectionScope.API);
                    if (this._responseHeaders.CollectionClass != null)
                    {
                        var sourceEndpoint = new EndpointDescriptor(this._myService.ServiceClass, "1", string.Empty, null, false);
                        var rspHeadersEndpoint = new EndpointDescriptor(this._responseHeaders.CollectionClass, "1", responseHeadersName, null, true);
                        this._myService.ServiceClass.CreateAssociation(sourceEndpoint, rspHeadersEndpoint, MEAssociation.AssociationType.MessageAssociation);
                    }
                    else Logger.WriteWarning("Unable to serialize Response Headers collection for service '" + this._myService.Name + "'!");
                }
            }
            return result;
        }

        /// <summary>
        /// Function used to delete a parameter from the collection identified by 'Scope'. If the name can not be found, the function
        /// fails silently. If, after the delete attempt, the collection is empty, the function removes the collection UML class from
        /// the model and re-initializes the collection.
        /// </summary>
        /// <param name="scope">Identifies the collection to be used.</param>
        /// <returns>True when the parameter has actually been removed, false when the parameter was not found in the collection.</returns>
        internal bool DeleteParameter(Scope scope, string name)
        {
            bool result = false;
            if (scope == Scope.Request)
            {
                result = this._requestHeaders.DeleteParameter(name);
                // If the collection is now empty, we remove the entire collection.
                if (this._requestHeaders.Collection.Count == 0)
                {
                    this._requestHeaders.DeleteCollection();
                    this._requestHeaders = new RESTHeaderParameterCollection(RESTCollection.CollectionScope.API);
                }
            }
            else
            {
                result = this._responseHeaders.DeleteParameter(name);
                // If the collection is now empty, we remove the entire collection.
                if (this._responseHeaders.Collection.Count == 0)
                {
                    this._responseHeaders.DeleteCollection();
                    this._responseHeaders = new RESTHeaderParameterCollection(RESTCollection.CollectionScope.API);
                }
            }
            return result;
        }

        /// <summary>
        /// This function is invoked to edit an existing header parameter. It displays the Header Parameter Dialog, which facilitates the user in 
        /// changing the result object. The updated object is added to the result list for this collection as long as
        /// it (still) has a unique name.
        /// </summary>
        /// <returns>Updated result record or NULL in case of errors or duplicates or user cancel.</returns>
        /// <exception cref="ArgumentException">Thrown when the received name does not match an existing header parameter.</exception>
        internal RESTHeaderParameterDescriptor EditParameter(Scope scope, string name)
        {
            return scope == Scope.Request ? this._requestHeaders.EditParameter(name) : this._responseHeaders.EditParameter(name);
        }

        /// <summary>
        /// Searches the list of request parameter descriptors and returns a list of all descriptors matching ID's from the IDList, which 
        /// must be a comma-separated list of identifiers. All ID's that do not yield a value are simply skipped, so the caller has to check 
        /// whether all requested ID's have indeed been returned.
        /// </summary>
        /// <param name="scope">Specifies whether we must use the request- or response collection.</param>
        /// <param name="IDList">Comma separated list of parameter ID's.</param>
        /// <returns>List of parameter descriptors that match the specified ID.</returns>
        internal List<RESTHeaderParameterDescriptor> FindParametersByID(Scope scope, List<int> IDList)
        {
            return scope == Scope.Request ? this._requestHeaders.FindParametersByID(IDList) : this._responseHeaders.FindParametersByID(IDList);
        }

        /// <summary>
        /// Returns the header parameter collection according to scope.
        /// </summary>
        /// <param name="scope">Specifies whether we must use the request- or response collection.</param>
        /// <returns>Header parameter collection.</returns>
        internal RESTHeaderParameterCollection GetCollection(Scope scope)
        {
            return scope == Scope.Request ? this._requestHeaders : this._responseHeaders;
        }

        /// <summary>
        /// Returns a parameter descriptor for the scoped parameter with the specified ID.
        /// </summary>
        /// <param name="scope">Specifies whether we must use the request- or response collection.</param>
        /// <param name="parameterID">ID of parameter to retrieve.</param>
        /// <returns>Descriptor or NULL when not in collection.</returns>
        internal RESTHeaderParameterDescriptor GetParameter(Scope scope, int parameterID)
        {
            return scope == Scope.Request ? this._requestHeaders.GetParameter(parameterID) : this._responseHeaders.GetParameter(parameterID);
        }

        /// <summary>
        /// Returns a parameter descriptor for the scoped parameter with the specified name.
        /// </summary>
        /// <param name="scope">Specifies whether we must use the request- or response collection.</param>
        /// <param name="parameterName">Name of parameter to retrieve.</param>
        /// <returns>Descriptor or NULL when not in collection.</returns>
        internal RESTHeaderParameterDescriptor GetParameter(Scope scope, string parameterName)
        {
            return scope == Scope.Request ? this._requestHeaders.GetParameter(parameterName) : this._responseHeaders.GetParameter(parameterName);
        }

        /// <summary>
        /// Returns true when the scoped collection contains a header parameter with the specified name.
        /// </summary>
        /// <param name="scope">Identifies the scope, one of Request or Response.</param>
        /// <param name="parameterName">Name of parameter to check.</param>
        /// <returns>True when present in collection, false otherwise.</returns>
        internal bool HasParameter(Scope scope, string parameterName)
        {
            return scope == Scope.Request ? this._requestHeaders.HasParameter(parameterName) : this._responseHeaders.HasParameter(parameterName);
        }

        /// <summary>
        /// Returns true when the scoped collection contains a header parameter with the specified ID.
        /// </summary>
        /// <param name="scope">Identifies the scope, one of Request or Response.</param>
        /// <param name="parameterID">ID of parameter to check.</param>
        /// <returns>True when present in collection, false otherwise.</returns>
        internal bool HasParameter(Scope scope, int parameterID)
        {
            return scope == Scope.Request ? this._requestHeaders.HasParameter(parameterID) : this._responseHeaders.HasParameter(parameterID);
        }

        /// <summary>
        /// This function is invoked when the user wants to add/remove- or edit one or more request header parameters for an operation in this API. 
        /// The function shows the parameter selection dialog, which uses the scoped-parameter collection as a source (and which could 
        /// be empty). The user uses the dialog to populate the collection, remove or modify entries and/or copy parameters to the operation-specific
        /// set. The function receives the current list of parameters defined for the operation, which is used to populate the dialog and is
        /// also used to assure that the user can not select duplicate parameters.
        /// </summary>
        /// <param name="scope">Specifies whether we must manage the request- or response collection.</param>
        /// <param name="existingDescriptors">List of existing descriptors from the requesting party.</param>
        /// <returns>The updated set of parameters after the dialog returns.</returns>
        internal List<RESTHeaderParameterDescriptor> ManageParameters(Scope scope, List<RESTHeaderParameterDescriptor> existingDescriptors)
        {
            using (RESTHeaderParameterDialog dialog = new RESTHeaderParameterDialog(this, scope, existingDescriptors))
            {
                // This dialog ONLY has an OK button, meaning that the user can not cancel the operation. Reason is that the user can modify the
                // managed collection through the dialog, which has effect on all parameters in all operations. To make this more obvious, the
                // dialog can only be left through the Ok button, providing a hint to the user to be careful what to change.
                dialog.ShowDialog();
                return dialog.ResultSet;
            }
        }
    }
}
