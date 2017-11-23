using System;
using System.Collections.Generic;
using Framework.Logging;
using Framework.Context;

namespace Framework.Model
{
   /// <summary>
    /// Model Element Implementation Class adds another layer of abstraction between the generic Model Element Implementation
    /// and the tool-specific implementation. This facilitates implementation of Model Element type-specific methods at this layer
    /// without the bridge interface needing tool-specific implementation logic.
    /// </summary>
    internal abstract class MEIDataType : MEIClass
    {
        // Configuration properties used to obtain hierarchies:
        private const string _UseFacetsAndSupplementariesTag    = "UseFacetsAndSupplementariesTag";
        private const string _BusinessDataTypeStereotype        = "BusinessDataTypeStereotype";
        private const string _CoreDataTypeStereotype            = "CoreDataTypeStereotype";
        private const string _CoreDataTypeEnumStereotype        = "CoreDataTypeEnumStereotype";
        private const string _PrimitiveDataTypeStereotype       = "PrimitiveDataTypeStereotype";
        private const string _PrimDataTypeEnumStereotype        = "PrimDataTypeEnumStereotype";

        protected MEDataType.MetaDataType _metaType;   // Must be initialized by derived constructors!

        // Getters for local properties.
        internal MEDataType.MetaDataType MetaType { get { return this._metaType; } }

        /// <summary>
        /// Parses the class hierarchy of this data type and extracts all attributes (up to the PRIM level in the hierarchy)
        /// that adhere to the following rules:
        /// - We select only Supplementary and Facet attributes that have either a fixed- or default value;
        /// - If an attribute is redefined at a lower level in the hierarchy, this is used instead of the more abstract versions;
        /// - If the data type has the 'useSupplementaries' tag set to 'true', all Supplementary and Facet attributes are copied
        /// for that class (as long as the previous rule is not broken).
        /// </summary>
        /// <returns>List of MetaDataDescriptor objects or empty list when nothing found.</returns>
        internal virtual List<MEDataType.MetaDataDescriptor> GetMetaData()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string grabAllTagKey = context.GetConfigProperty(_UseFacetsAndSupplementariesTag);
            string BDTStereotype = context.GetConfigProperty(_BusinessDataTypeStereotype);

            var descriptorList = new List<MEDataType.MetaDataDescriptor>();   	// Will receive collected data.
            var processedAttributes = new SortedSet<string>();            		// Keep track of attributes already processed.

            // Retrieve the class hierarchy of this data type, all the way up to the defined root level...
            SortedList <uint, MEClass> hierarchy = GetHierarchy(context.GetConfigProperty(_PrimitiveDataTypeStereotype));
            foreach (MEClass currClass in hierarchy.Values)
            {
                // Check whether we have to grab all Supplementary and Facet attributes. This is only allowed when we are at a 
                // Business Data Type AND the type has a tag that grants permission to grab these attributes...
                string grabAllTag = currClass.GetTag(grabAllTagKey);
                bool grabAll = (!string.IsNullOrEmpty(grabAllTag) && string.Compare(grabAllTag, "true", StringComparison.OrdinalIgnoreCase) == 0) &&
                               currClass.HasStereotype(BDTStereotype);

                foreach (MEAttribute attribute in currClass.Attributes)
                {
                    if ((attribute.Type == ModelElementType.Supplementary || attribute.Type == ModelElementType.Facet) &&
                        !processedAttributes.Contains(attribute.Name))
                    {
                        // Supplementaries must have a classifier, Facets typically have no type...
                        if ((attribute.Type == ModelElementType.Facet) ||
                             ((attribute.Classifier != null) &&
                              (attribute.Classifier.MetaType == MEDataType.MetaDataType.SimpleType ||
                               attribute.Classifier.MetaType == MEDataType.MetaDataType.Enumeration)))
                        {
                            if (grabAll)
                            {
                                descriptorList.Add(new MEDataType.MetaDataDescriptor(attribute));
                                processedAttributes.Add(attribute.Name);
                            }
                            else if (attribute.DefaultValue != string.Empty || attribute.FixedValue != string.Empty)
                            {
                                descriptorList.Add(new MEDataType.MetaDataDescriptor(attribute));
                                processedAttributes.Add(attribute.Name);
                            }
                        }
                    }
                }
            }
            return descriptorList;
        }

        /// <summary>
        /// This method searches the class hierarchy for a class that possesses the CDT stereotype. If found, the name of parent class
        /// (must be the associated PRIM) is returned as the typename, combined with the meta-type of the CDT.
        /// The reason for this split is that, even though the primitive type itself is a simple type, the derived data type might be
        /// a complex type and we want to keep this knowledge (complex + simple = complex).
        /// </summary>
        /// <returns>Tuple containing both the meta-type and the type name.</returns>
        internal Tuple<MEDataType.MetaDataType, string> GetPrimitiveTypeName()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            string PRIMStereotype = context.GetConfigProperty(_PrimitiveDataTypeStereotype);
            string PRIMEnumStereotype = context.GetConfigProperty(_PrimDataTypeEnumStereotype);
            string CDTStereotype = context.GetConfigProperty(_CoreDataTypeStereotype);
            string CDTEnumStereotype = context.GetConfigProperty(_CoreDataTypeEnumStereotype);

            MEDataType.MetaDataType metaType = MEDataType.MetaDataType.Unknown;
            string typeName = string.Empty;

            // Retrieve the class hierarchy all the way to the top (since we don't have one single stereotype to search for :-(
            // Then, search this hierarchy for either a PRIM Type or a PRIM Enumeration...
            SortedList<uint, MEClass> hierarchy = GetHierarchy(null);

            foreach (MEClass currType in hierarchy.Values)
            {
                if (currType.HasStereotype(CDTStereotype) || currType.HasStereotype(CDTEnumStereotype))
                {
                    metaType = model.GetDataType(currType.ElementID).MetaType;
                }
                else if (currType.HasStereotype(PRIMStereotype) || currType.HasStereotype(PRIMEnumStereotype))
                {
                    typeName = currType.Name;
                    // Below statement covers the case where we start at the PRIM level (and thus do not have a CDT):
                    if (metaType == MEDataType.MetaDataType.Unknown) metaType = model.GetDataType(currType.ElementID).MetaType;
                    break;
                }
            }

            if (metaType != MEDataType.MetaDataType.Unknown && typeName != string.Empty)
            {
                return new Tuple<MEDataType.MetaDataType, string>(metaType, typeName);
            }
            else return null;
        }

        /// <summary>
        /// Default constructor, mainly used to pass the model instance to the base constructor and set the correct type.
        /// </summary>
        /// <param name="model">Reference to the associated model implementation object.</param>
        protected MEIDataType (ModelImplementation model): base(model)
        {
            this._type = ModelElementType.DataType;
        }
    }
}