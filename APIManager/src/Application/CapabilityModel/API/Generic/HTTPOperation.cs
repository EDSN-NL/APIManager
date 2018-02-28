using System;
using System.Collections.Generic;
using Framework.Util;
using Framework.Context;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// Helper class for management of HTTP Operations. The type represents an HTTP operation, both as enumeration and as human friendly label.
    /// The type also supports conversions and support for administration of 'available' operations.
    /// </summary>
    internal class HTTPOperation: IEquatable<HTTPOperation>
    {
        // These are the REST Operation types supported by the framework. They correspond with the respective HTTP Method:
        internal enum Type { Unknown, Post, Delete, Head, Get, Put, Options, Patch }

        private static readonly string[,] _OperationTable =
                { {"Delete", "Delete Resource: 'Delete'"},
                  {"Get",    "Read/Find Resource: 'Get'" },
                  {"Head",   "Determine Resource existence: 'Head'"},
                  {"Options","Retrieve Operations on Resource: 'Options'"},
                  {"Patch",  "Update (partial) Resource: 'Patch'"},
                  {"Post",   "Operation on Resource: 'Post'"},
                  {"Put",    "Replace existing Resource: 'Put'" },
                  {"Unknown","Unknown Operation"} };

        private string _label;      // The 'user friendly' label for this HTTP Operation.
        private Type _type;         // The enumerated-type representation of the HTTP Operation.

        /// <summary>
        /// Compares the HTTPOperation against another object. If the other object is also an HTTPOperation, the function returns true if
        /// both HTTPOperations are of the same operaton type. In all other cases, the function returns false.
        /// </summary>
        /// <param name="obj">The thing to compare against.</param>
        /// <returns>True if same object, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var objElement = obj as HTTPOperation;
            return (objElement != null) && Equals(objElement);
        }

        /// <summary>
        /// Compares the HTTPOperation against another HTTPOperation. The function returns true if
        /// both HTTPOperations are of the same operation type. In all other cases, the function returns false.
        /// </summary>
        /// <param name="other">The HTTPOperation to compare against.</param>
        /// <returns>True if same object, false otherwise.</returns>
        public bool Equals(HTTPOperation other)
        {
            return other != null && other._type == this._type;
        }

        /// <summary>
        /// Returns a hashcode that is associated with the HTTP Operation Type.
        /// </summary>
        /// <returns>Hashcode according to HTTP Operation Type.</returns>
        public override int GetHashCode()
        {
            return this._type.GetHashCode();
        }

        /// <summary>
        /// Override of compare operator. Two HTTPOperation objects are equal if they are of the same HTTP Operator type
        /// or if they are both NULL.
        /// </summary>
        /// <param name="elementa">First HTTPOperation to compare.</param>
        /// <param name="elementb">Second HTTPOperation to compare.</param>
        /// <returns>True if both HTTPOperations are NULL or of same HTTP Operation type.</returns>
        public static bool operator ==(HTTPOperation elementa, HTTPOperation elementb)
        {
            // Tricky to implement correctly. These first statements make sure that we check whether we are actually
            // dealing with identical objects and/or whether one or both are NULL.
            if (ReferenceEquals(elementa, elementb)) return true;
            if (ReferenceEquals(elementa, null)) return false;
            if (ReferenceEquals(elementb, null)) return false;
            return elementa.Equals(elementb);
        }

        /// <summary>
        /// Override of compare operator. Two HTTPOperation objects are different if they are of different HTTP Operator type
        /// or if one of them is NULL.
        /// </summary>
        /// <param name="elementa">First HTTPOperation to compare.</param>
        /// <param name="elementb">Second HTTPOperation to compare.</param>
        /// <returns>True if one of the HTTPOperations is NULL or they hace different HTTP Operation types.</returns>
        public static bool operator !=(HTTPOperation elementa, HTTPOperation elementb)
        {
            return !(elementa == elementb);
        }

        /// <summary>
        /// Returns the string representation of the HTTP Operation, which is the 'human friendly' label text.
        /// </summary>
        /// <returns>String representation of HTTP Operation (human-friendly label)</returns>
        public override string ToString() { return this._label; }

        /// <summary>
        /// Returns the HTTP Operation type as an enumerated type.
        /// </summary>
        internal Type TypeEnum { get { return this._type; } }

        /// <summary>
        /// Returns the HTTP Operation type as a string.
        /// </summary>
        internal string TypeName { get { return EnumConversions<Type>.EnumToString(this._type); } }

        /// <summary>
        /// Class constructor, creates a new instance using the provided enumerated type.
        /// </summary>
        /// <param name="type">Logical HTTP operation name.</param>
        internal HTTPOperation(Type type)
        {
            Initialize(type);
        }

        /// <summary>
        /// Default class constructor, creates an 'Unknown' instance.
        /// </summary>
        internal HTTPOperation()
        {
            Initialize(Type.Unknown);
        }

        /// <summary>
        /// Helper function that adds the specified HTTP Operation to the specified list. If the operation is already in the list, no
        /// actions will be performed.
        /// </summary>
        /// <param name="operationList">The list to be updated.</param>
        /// <param name="addType">The HTTP Operation that must be added to the queue.</param>
        internal static void AddToOperationList(ref List<HTTPOperation> operationList, HTTPOperation addType)
        {
            if (!operationList.Contains(addType)) operationList.Add(addType);
        }

        /// <summary>
        /// Helper function that returns a list of resource-specific HTTP Operation objects.
        /// </summary>
        /// <returns>List of all valid HTTP Operation instances.</returns>
        internal static List<HTTPOperation> GetOperationList(string resourceArchetype)
        {
            string allowedOperations = ContextSlt.GetContextSlt().GetConfigProperty("Resource:" + resourceArchetype);
            List<HTTPOperation> operationList = new List<HTTPOperation>();
            if (allowedOperations != string.Empty)
            {
                foreach (string enumName in EnumConversions<Type>.GetNamesArray())
                {
                    if (enumName != "Unknown" && allowedOperations.Contains(enumName)) operationList.Add(new HTTPOperation(enumName));
                }
            }
            return operationList;
        }

        /// <summary>
        /// Helper function that removes the specified HTTP Operation (removeType) from the provided list. If the type is not found in the
        /// list, the function has no effect.
        /// </summary>
        /// <param name="operationList">List to be pruned.</param>
        /// <param name="removeType">The operation to be removed from the list.</param>
        internal static void RemoveFromOperationList(ref List<HTTPOperation> operationList, HTTPOperation removeType)
        {
            foreach(HTTPOperation operation in operationList)
            {
                if (operation.TypeEnum == removeType._type)
                {
                    operationList.Remove(operation);
                    return;
                }
            }
        }

        /// <summary>
        /// Private constructor that creates a new instance from a provided HTTP Name string. We keep this private since we ONLY want
        /// to expose either the enumerated type or the human friendly label.
        /// </summary>
        /// <param name="operationName">HTTP Name string.</param>
        private HTTPOperation (string operationName)
        {
            for (int i = 0; i < _OperationTable.GetLength(0); i++)
            {
                if (String.Compare(operationName, _OperationTable[i, 0], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this._label = _OperationTable[i, 1];
                    this._type = EnumConversions<Type>.StringToEnum(_OperationTable[i, 0]);
                    break;
                }
            }
        }

        /// <summary>
        /// Helper function that translates a specified OperationType to a human-friendly label name.
        /// </summary>
        /// <param name="type">OperationType to translate</param>
        /// <returns>Human-friendly label.</returns>
        private void Initialize(Type type)
        {
            string typeEnumString = EnumConversions<Type>.EnumToString(type);
            for (int i = 0; i < _OperationTable.GetLength(0); i++)
            {
                if (String.Compare(typeEnumString, _OperationTable[i, 0], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this._label = _OperationTable[i, 1];
                    break;
                }
            }
            this._type = type;
        }
    }
}
