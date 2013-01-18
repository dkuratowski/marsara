using System.Collections.Generic;
using System.Xml.Linq;

namespace RC.Common.Configuration
{
    /// <summary>
    /// Loads the contents of a package format configuration file.
    /// </summary>
    class RCPackageFormatLoader : ConfigObjectContents
    {
        #region ConfigObjectContents members

        /// <see cref="ConfigObjectContents.Load_i"/>
        protected override void Load_i(XElement rootElem)
        {
            IEnumerable<XElement> formatElems = rootElem.Elements(PACKAGE_FORMAT_ELEM);

            foreach (XElement fmtElem in formatElems)
            {
                XAttribute formatNamespace = fmtElem.Attribute(NAMESPACE_ATTR);
                XAttribute formatName = fmtElem.Attribute(NAME_ATTR);
                if (formatNamespace != null && formatName != null)
                {
                    string newFormatName = string.Format("{0}.{1}", formatNamespace.Value, formatName.Value);
                    if (RCPackageFormatMap.Contains(newFormatName))
                    {
                        /// Error: package format with the same name already exists
                        throw new ConfigurationException(string.Format("RCPackageFormat {0} already exists!", newFormatName));
                    }
                    RCPackageFormat newFormat = new RCPackageFormat(newFormatName);

                    bool hasFields = false;
                    IEnumerable<XElement> fieldElems = fmtElem.Elements(FIELD_ELEM);
                    foreach (XElement fieldElem in fieldElems)
                    {
                        XAttribute fieldTypeAttr = fieldElem.Attribute(FIELD_TYPE_ATTR);
                        if (fieldTypeAttr != null)
                        {
                            RCPackageFieldType fieldType;
                            if (EnumMap<RCPackageFieldType, string>.Demap(fieldTypeAttr.Value, out fieldType))
                            {
                                newFormat.DefineField(fieldType);
                                hasFields = true;
                            }
                            else
                            {
                                /// Error: unexpected package field type                                
                                throw new ConfigurationException(string.Format("Unexpected package field type {0}!", fieldTypeAttr.Value));
                            }
                        }
                        else
                        {
                            /// Error: no field type defined
                            throw new ConfigurationException("No package field type defined!");
                        }
                    }

                    if (hasFields)
                    {
                        int newFormatID = RCPackageFormat.RegisterFormat(newFormat);
                        RCPackageFormatMap.Add(newFormatName, newFormatID);
                    }
                    else
                    {
                        /// Error: no fields for the current package format
                        throw new ConfigurationException("No package fields defined!");
                    }
                }
                else
                {
                    /// Error: no namespace or name defined
                    throw new ConfigurationException("No namespace or name defined for package format!");
                }
            }

            return;
        }

        #endregion ConfigObjectContents members

        /// <summary>
        /// Supported XML elements and attributes in package format configuration files.
        /// </summary>
        private const string PACKAGE_FORMAT_ELEM = "packageFormat";
        private const string NAMESPACE_ATTR = "namespace";
        private const string NAME_ATTR = "name";
        private const string FIELD_ELEM = "field";
        private const string FIELD_TYPE_ATTR = "type";
    }
}
