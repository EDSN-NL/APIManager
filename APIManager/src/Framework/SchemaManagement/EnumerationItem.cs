using System.Collections.Generic;
using Framework.Logging;

namespace Framework.Util.SchemaManagement
{
    /// <summary>
    /// Helper class that represents a single enumeration item (value). The class is used to keep the item name and optional annotation so
    /// we can export the annotation to the schema together with the name.
    /// </summary>
    internal class EnumerationItem
    {
        private string _name;
        private List<MEDocumentation> _annotation;

        internal EnumerationItem(string name, List<MEDocumentation> annotation)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.EnumerationItem >> Created item with name '" + name + "'.");
            this._name = name;
            this._annotation = annotation;
        }

        /// <summary>
        /// Getters for the enumeration item (single value) and associated annotation...
        /// </summary>
        internal string Name { get { return this._name; } }
        internal List<MEDocumentation> Annotation { get { return this._annotation; } }
    }
}
