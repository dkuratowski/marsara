using System.Collections.Generic;
using System.Xml.Linq;

namespace RC.Common.Configuration
{
    /// <summary>
    /// Loads the contents of a constant configuration file.
    /// </summary>
    class ConstantsLoader : ConfigObjectContents
    {
        #region ConfigObjectContents members

        /// <see cref="ConfigObjectContents.Load_i"/>
        protected override void Load_i(XElement rootElem)
        {
            IEnumerable<XElement> constElems = rootElem.Elements(CONSTANT_ELEM);

            foreach (XElement constElem in constElems)
            {
                XAttribute constNamespace = constElem.Attribute(NAMESPACE_ATTR);
                XAttribute constName = constElem.Attribute(NAME_ATTR);
                XAttribute constType = constElem.Attribute(TYPE_ATTR);
                if (constNamespace != null && constName != null && constType != null)
                {
                    string newConstName = string.Format("{0}.{1}", constNamespace.Value, constName.Value);
                    ConstantsTable.Add(newConstName, constElem.Value, constType.Value);
                }
                else
                {
                    /// Error: no namespace, name or type defined
                    throw new ConfigurationException("No namespace, name or type defined for constant!");
                }
            }

            return;
        }

        #endregion ConfigObjectContents members

        /// <summary>
        /// Supported XML elements and attributes in constant configuration files.
        /// </summary>
        private const string CONSTANT_ELEM = "constant";
        private const string NAMESPACE_ATTR = "namespace";
        private const string NAME_ATTR = "name";
        private const string TYPE_ATTR = "type";
    }
}
