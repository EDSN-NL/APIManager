using System;
using System.Collections.Generic;
using Framework.Model;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// The REST Resource Container interface is implemented by classes that are able to act as a storage for REST Resources.
    /// Currently, these are RESTInterfaceCapability and RESTResourceCapability.
    /// </summary>
    internal interface IRESTResourceContainer
    {
        IEnumerable<RESTResourceCapability> ResourceList();
        bool AddResources(List<RESTResourceDeclaration> resources, bool newMinorVersion);
        void DeleteResource(MEClass resourceClass, bool newMinorVersion);
        void RenameResource(MEClass resourceClass, string oldName, string newName, bool newMinorVersion);
    }
}
