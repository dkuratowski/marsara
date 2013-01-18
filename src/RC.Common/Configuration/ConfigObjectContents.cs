using System;
using System.Xml.Linq;

namespace RC.Common.Configuration
{
    /// <summary>
    /// The base class for any user defined classes for loading different types of configuration objects.
    /// </summary>
    public abstract class ConfigObjectContents
    {
        /// <summary>
        /// Loads the contents of the underlying configuration object from the given XML element.
        /// </summary>
        /// <param name="rootElem">The root XML element of the file that describes the configuration object.</param>
        /// <exception cref="ConfigurationException">In case of any error during the load.</exception>
        public void Load(XElement rootElem)
        {
            if (rootElem == null) { throw new ArgumentNullException("rootElem"); }
            Load_i(rootElem);
        }

        /// <summary>
        /// Saves the contents of the underlying configuration object to an XML element.
        /// </summary>
        /// <returns>
        /// The XML element that contains the new contents of the underlying configuration object or null if
        /// the derived class doesn't support saving configuration data.
        /// </returns>
        /// <exception cref="ConfigurationException">In case of any error during the save.</exception>
        public XElement Save()
        {
            return Save_i();
        }

        /// <summary>
        /// Internal function to load the contents of the underlying configuration object.
        /// </summary>
        /// <param name="rootElem">The root XML element of the file that describes the configuration object.</param>
        /// <exception cref="ConfigurationException">In case of any error during the load.</exception>
        /// <remarks>Must be implemented in the derived classes.</remarks>
        protected abstract void Load_i(XElement rootElem);

        /// <summary>
        /// Internal function for saving the contents of the underlying configuration object to an XML element.
        /// </summary>
        /// <returns>
        /// The XML element that contains the new contents of the underlying configuration object or null if
        /// the derived class doesn't support saving configuration data.
        /// </returns>
        /// <exception cref="ConfigurationException">In case of any error during the save.</exception>
        /// <remarks>
        /// Can be implemented in derived classes that support saving configuration data. Otherwise this default
        /// implementation must be used.
        /// </remarks>
        protected virtual XElement Save_i() { return null; }
    }
}
