using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace RC.Common.Configuration
{
    /// <summary>
    /// Represents a configuration node in the configuration graph.
    /// </summary>
    class ConfigurationNode
    {
        /// <summary>
        /// Constructs a ConfigurationNode object from the given file.
        /// </summary>
        /// <param name="nodeFile">The node file.</param>
        public ConfigurationNode(FileInfo nodeFile)
        {
            if (nodeFile == null) { throw new ArgumentNullException("nodeFile"); }
            if (!nodeFile.Exists) { throw new ConfigurationException(string.Format("Configuration node {0} doesn't exist!", nodeFile.FullName)); }

            this.nodeFile = nodeFile;
            this.nodeReferences = new List<ConfigurationNode>();
            this.configObjects = new List<ConfigurationObject>();

            XDocument xmlDoc = XDocument.Load(nodeFile.FullName);

            Parse(xmlDoc);
        }

        /// <summary>
        /// Loads every configuration objects that is reachable from this configuration node.
        /// </summary>
        public void LoadConfigObjects(ConfigurationContext currentContext)
        {
            foreach (ConfigurationNode node in this.nodeReferences)
            {
                node.LoadConfigObjects(currentContext);
            }

            foreach (ConfigurationObject configObj in this.configObjects)
            {
                configObj.Load(currentContext);
            }
        }

        /// <summary>
        /// Parses the given configuration node XML.
        /// </summary>
        /// <param name="xmlDoc">The XDocument that contains the configuration node description.</param>
        private void Parse(XDocument xmlDoc)
        {
            XElement rootElem = xmlDoc.Root;
            XElement nodeRefsElem = rootElem.Element(CONFIG_NODE_REFERENCES_ELEM);
            XElement configObjectsElem = rootElem.Element(CONFIG_OBJECTS_ELEM);

            if (nodeRefsElem != null)
            {
                IEnumerable<XElement> nodeRefs = nodeRefsElem.Elements(CONFIG_NODE_REFERENCE_ELEM);
                foreach (XElement nodeRefElem in nodeRefs)
                {
                    string referencedNodePath = Path.Combine(this.nodeFile.DirectoryName, nodeRefElem.Value);
                    this.nodeReferences.Add(new ConfigurationNode(new FileInfo(referencedNodePath)));
                }
            }

            if (configObjectsElem != null)
            {
                IEnumerable<XElement> cfgObjects = configObjectsElem.Elements(CONFIG_OBJECT_ELEM);
                foreach (XElement cfgObjElem in cfgObjects)
                {
                    this.configObjects.Add(ConfigurationObject.FromXML(cfgObjElem, this.nodeFile.Directory));
                }
            }
        }

        /// <summary>
        /// References to neighbour configuration nodes.
        /// </summary>
        private List<ConfigurationNode> nodeReferences;

        /// <summary>
        /// References to the ConfigurationObjects attached to this node.
        /// </summary>
        private List<ConfigurationObject> configObjects;

        /// <summary>
        /// Reference to the XML file of this node.
        /// </summary>
        private FileInfo nodeFile;

        /// <summary>
        /// Supported XML elements in configuration node files.
        /// </summary>
        private const string CONFIG_NODE_REFERENCES_ELEM = "configNodeReferences";
        private const string CONFIG_NODE_REFERENCE_ELEM = "configNodeReference";
        private const string CONFIG_OBJECTS_ELEM = "configObjects";
        private const string CONFIG_OBJECT_ELEM = "configObject";
    }
}
