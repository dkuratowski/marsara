using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using System.Reflection;

namespace RC.Common.Configuration
{
    /// <summary>
    /// Represents a configuration object attached to the configuration graph.
    /// </summary>
    class ConfigurationObject
    {
        #region Static members

        /// <summary>
        /// Creates a configuration object from the given XML element.
        /// </summary>
        /// <param name="xmlElem">The XML element to parse.</param>
        /// <param name="nodeDir">The directory of the configuration node that this object is attached to.</param>
        /// <returns>The created configuration object.</returns>
        /// <exception cref="ConfigurationException">In case of any error.</exception>
        public static ConfigurationObject FromXML(XElement xmlElem, DirectoryInfo nodeDir)
        {
            if (xmlElem == null) { throw new ArgumentNullException("xmlElem"); }

            XAttribute nameSpaceAttr = xmlElem.Attribute(NAMESPACE_ATTR);
            if (nameSpaceAttr == null) { throw new ConfigurationException(string.Format("'{0}' attribute not found!", NAMESPACE_ATTR)); }

            XAttribute nameAttr = xmlElem.Attribute(NAME_ATTR);
            if (nameAttr == null) { throw new ConfigurationException(string.Format("'{0}' attribute not found!", NAME_ATTR)); }

            XElement loaderElem = xmlElem.Element(LOADER_ELEM);
            if (loaderElem == null) { throw new ConfigurationException(string.Format("<{0}> element not found!", LOADER_ELEM)); }

            XElement contentsElem = xmlElem.Element(CONTENTS_ELEM);
            if (contentsElem == null) { throw new ConfigurationException(string.Format("<{0}> element not found!", CONTENTS_ELEM)); }

            XElement assemblyElem = loaderElem.Element(ASSEMBLY_ELEM);
            if (assemblyElem == null) { throw new ConfigurationException(string.Format("<{0}> element not found!", ASSEMBLY_ELEM)); }

            XElement classElem = loaderElem.Element(CLASS_ELEM);
            if (classElem == null) { throw new ConfigurationException(string.Format("<{0}> element not found!", CLASS_ELEM)); }

            FileInfo objFile = new FileInfo(Path.Combine(nodeDir.FullName, contentsElem.Value));
            if (!objFile.Exists) { throw new ConfigurationException(string.Format("Configuration file {0} doesn't exist!", objFile.FullName)); }

            ConfigurationObject retObj = new ConfigurationObject();
            retObj.objectFile = objFile;
            retObj.loaderAssembly = assemblyElem.Value;
            retObj.loaderClass = classElem.Value;
            retObj.qualifiedName = string.Format("{0}.{1}", nameSpaceAttr.Value, nameAttr.Value);

            return retObj;
        }

        /// <summary>
        /// Unregisters every configuration objects.
        /// </summary>
        public static void UnregisterAllConfigObjects()
        {
            configObjectsByName.Clear();
            configObjectsByPath.Clear();
        }

        /// <summary>
        /// Registers the given configuration object.
        /// </summary>
        /// <param name="configObj">The configuration object you want to register.</param>
        /// <returns>
        /// True if the configuration object has been successfully registered or false if there is another
        /// registered configuration object with the same qualified name and file path.
        /// </returns>
        private static bool RegisterConfigObject(ConfigurationObject configObj)
        {
            bool nameFound = configObjectsByName.ContainsKey(configObj.qualifiedName);
            bool pathFound = configObjectsByPath.ContainsKey(configObj.objectFile.FullName.ToLower());

            if (nameFound && pathFound)
            {
                return false;
            }
            else if (!nameFound && !pathFound)
            {
                configObjectsByName.Add(configObj.qualifiedName, configObj);
                configObjectsByPath.Add(configObj.objectFile.FullName.ToLower(), configObj);
                return true;
            }
            else if (!nameFound && pathFound)
            {
                throw new ConfigurationException(string.Format("Configuration object {0} has been already registered with path {1}!",
                                                               configObjectsByPath[configObj.objectFile.FullName.ToLower()].qualifiedName,
                                                               configObj.objectFile.FullName.ToLower()));
            }
            else
            {
                throw new ConfigurationException(string.Format("Configuration object {0} has been already registered!",
                                                               configObj.qualifiedName));
            }
        }

        /// <summary>
        /// List of the registered configuration objects mapped by their fully qualified names.
        /// </summary>
        private static Dictionary<string, ConfigurationObject> configObjectsByName =
            new Dictionary<string,ConfigurationObject>();

        /// <summary>
        /// List of the registered configuration objects mapped by their paths.
        /// </summary>
        private static Dictionary<string, ConfigurationObject> configObjectsByPath =
            new Dictionary<string, ConfigurationObject>();

        #endregion Static members

        /// <summary>
        /// Loads this configuration object if it has not yet been loaded.
        /// </summary>
        public void Load(ConfigurationContext currentContext)
        {
            if (this.contents != null) { throw new ConfigurationException(string.Format("Configuration object {0} already loaded!", this.objectFile.FullName)); }

            /// Load the configuration object if necessary.
            if (ConfigurationObject.RegisterConfigObject(this))
            {
                Assembly asm = Assembly.Load(this.loaderAssembly);
                this.contents = asm.CreateInstance(this.loaderClass) as ConfigObjectContents;
                if (this.contents == null) { throw new ConfigurationException(string.Format("Could not found loader object {0} in assembly: {1}!", this.loaderClass, this.loaderAssembly)); }

                try
                {
                    XDocument xmlDoc = XDocument.Load(this.objectFile.FullName);
                    currentContext.IsEnabled = true;
                    currentContext.Path = this.objectFile.Directory;
                    this.contents.Load(xmlDoc.Root);
                    currentContext.IsEnabled = false;
                }
                catch (Exception)
                {
                    this.contents = null;
                    throw;
                }
            }
        }

        /// <summary>
        /// Private constructor.
        /// </summary>
        private ConfigurationObject()
        {
            this.qualifiedName = null;
            this.objectFile = null;
            this.loaderAssembly = null;
            this.loaderClass = null;
            this.contents = null;
        }

        /// <summary>
        /// Fully qualified name of this configuration object ("{namespace}.{name}").
        /// </summary>
        private string qualifiedName;

        /// <summary>
        /// The file of this configuration object.
        /// </summary>
        private FileInfo objectFile;

        /// <summary>
        /// Name of the assembly that contains the loader class for this configuration object.
        /// </summary>
        private string loaderAssembly;

        /// <summary>
        /// Fully qualified name of the loader class for this configuration object.
        /// </summary>
        private string loaderClass;

        /// <summary>
        /// Reference to the contents of this configuration object.
        /// </summary>
        private ConfigObjectContents contents;

        /// <summary>
        /// Supported XML elements and attributes in configuration node files.
        /// </summary>
        private const string NAMESPACE_ATTR = "namespace";
        private const string NAME_ATTR = "name";
        private const string LOADER_ELEM = "loader";
        private const string ASSEMBLY_ELEM = "assembly";
        private const string CLASS_ELEM = "class";
        private const string CONTENTS_ELEM = "contents";
    }
}
