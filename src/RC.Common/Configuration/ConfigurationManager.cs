using System;
using System.IO;
using RC.Common.Diagnostics;

namespace RC.Common.Configuration
{
    /// <summary>
    /// The ConfigurationManager is the access point of the configuration subsystem. The configuration subsystem
    /// provides an API to access the configuration objects that are described in XML files.
    /// Every configuration object shall be attached to exactly one configuration node. The configuration nodes
    /// are the vertices of a graph that is called 'configuration graph'.
    /// When you initialize the ConfigurationManager you have to give an absolute path to the file that describes
    /// a node in this graph. Every node that is accessible from the root node will be parsed in the order of
    /// appearance and the attached configuration objects will be loaded in the initialization process.
    /// </summary>
    public static class ConfigurationManager
    {
        /// <summary>
        /// Initializes the ConfigurationManager with an absolute path to the file that describes the root
        /// configuration node.
        /// </summary>
        /// <param name="rootPath">
        /// The absolute path to the file that describes the root configuration node.
        /// </param>
        /// <exception cref="ConfigurationException">
        /// If the ConfigurationManager has already been initialized.
        /// In case of any error during the initialization process.
        /// </exception>
        public static void Initialize(string rootPath)
        {
            if (rootPath == null || rootPath.Length == 0) { throw new ArgumentNullException("rootPath"); }
            if (isInitialized) { throw new ConfigurationException("The ConfigurationManager has already been initialized."); }

            ConfigurationObject.UnregisterAllConfigObjects();
            RCPackageFormatMap.Clear();
            ConstantsTable.Clear();
            TraceManager.UnregisterAllTraceFilters();

            currentContext = new ConfigurationContext();
            FileInfo rootNodeFile = new FileInfo(rootPath);

            rootNode = new ConfigurationNode(rootNodeFile);
            rootNode.LoadConfigObjects(currentContext);

            isInitialized = true;
        }

        /// <summary>
        /// Gets the current configuration context.
        /// </summary>
        public static IConfigurationContext CurrentContext
        {
            get
            {
                if (currentContext == null || !currentContext.IsEnabled)
                {
                    throw new InvalidOperationException("ConfigurationContext is not currently enabled!");
                }
                return currentContext;
            }
        }

        /// <summary>
        /// Gets whether the ConfigurationManager has already been initialized.
        /// </summary>
        public static bool IsInitialized { get { return isInitialized; } }

        /// <summary>
        /// Reference to the root node of the configuration graph.
        /// </summary>
        private static ConfigurationNode rootNode;

        /// <summary>
        /// Reference to the current configuration context.
        /// </summary>
        private static ConfigurationContext currentContext;

        /// <summary>
        /// This flag is true if the ConfigurationManager has already been initialized.
        /// </summary>
        private static bool isInitialized = false;
    }

    /// <summary>
    /// Interface to the current context of the configuration initialization process.
    /// </summary>
    public interface IConfigurationContext
    {
        /// <summary>
        /// Gets the path of the configuration object that is currently being loaded.
        /// </summary>
        DirectoryInfo Path { get; }
    }

    /// <summary>
    /// Internal implementation of the IConfigurationContext interface.
    /// </summary>
    class ConfigurationContext : IConfigurationContext
    {
        #region IConfigurationContext Members

        /// <see cref="IConfigurationContext.Path"/>
        /// <remarks>Setter is not the part of the IConfigurationContext interface.</remarks>
        public DirectoryInfo Path
        {
            get
            {
                if (!this.isEnabled) { throw new InvalidOperationException("ConfigurationContext is not currently enabled!"); }
                return this.path;
            }
            
            set
            {
                if (!this.isEnabled) { throw new InvalidOperationException("ConfigurationContext is not currently enabled!"); }
                this.path = value;
            }
        }

        #endregion

        /// <summary>
        /// Gets or sets whether this configuration context is currently enabled.
        /// </summary>
        public bool IsEnabled
        {
            get { return this.isEnabled; }
            set { this.isEnabled = value; }
        }

        /// <summary>
        /// The path of the currently loaded configuration object.
        /// </summary>
        private DirectoryInfo path;

        /// <summary>
        /// True if the context is currently enabled, false otherwise.
        /// </summary>
        private bool isEnabled;
    }
}
