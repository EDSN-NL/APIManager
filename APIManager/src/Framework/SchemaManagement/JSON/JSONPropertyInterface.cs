using System;
using Newtonsoft.Json.Schema;

namespace Framework.Util.SchemaManagement.JSON
{
    /// <summary>
    /// This interface facilitates construction of JSON properties, which consist of a name, a classifier and an optional sequence number.
    /// Property implementation includes JSONAssociation and JSONAttribute.
    /// </summary>
    internal interface IJSONProperty
    {
        string Name { get; }
        string SchemaName { get; }
        JSchema JSchema { get; }
        int SequenceKey { get; }
        bool IsMandatory { get; }
    }
}
