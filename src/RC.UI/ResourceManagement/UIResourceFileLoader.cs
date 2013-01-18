using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Configuration;
using System.Xml.Linq;
using System.IO;
using System.Reflection;

namespace RC.UI
{
    /// <summary>
    /// Loader for resource definition files.
    /// </summary>
    class UIResourceFileLoader : ConfigObjectContents
    {
        #region ConfigObjectContents members

        /// <see cref="ConfigObjectContents.Load_i"/>
        protected override void Load_i(XElement rootElem)
        {
            foreach (XElement loaderElem in rootElem.Elements(LOADER_ELEM))
            {
                this.LoadLoaderDef(loaderElem);
            }

            foreach (XElement resGroupElem in rootElem.Elements(RESOURCEGROUP_ELEM))
            {
                this.LoadResourceGroupDef(resGroupElem);
            }
        }

        #endregion ConfigObjectContents members

        #region Internal load methods

        /// <summary>
        /// Loads a loader definition from the given XML-element.
        /// </summary>
        /// <param name="loaderElem">The XML-element to load from.</param>
        private void LoadLoaderDef(XElement loaderElem)
        {
            XAttribute nameAttr = loaderElem.Attribute(LOADER_NAME_ATTR);
            XAttribute assemblyAttr = loaderElem.Attribute(LOADER_ASSEMBLY_ATTR);
            XAttribute classAttr = loaderElem.Attribute(LOADER_CLASS_ATTR);
            if (nameAttr != null && assemblyAttr != null && classAttr != null)
            {
                if (!this.loaderAssemblies.ContainsKey(nameAttr.Value))
                {
                    this.loaderAssemblies.Add(nameAttr.Value, assemblyAttr.Value);
                    this.loaderClasses.Add(nameAttr.Value, classAttr.Value);
                }
                else
                {
                    throw new ConfigurationException(string.Format("Loader with name '{0}' already exists!", nameAttr.Value));
                }
            }
            else
            {
                /// Error: no name, assembly or class defined
                throw new ConfigurationException("No name, assembly or class defined for loader!");
            }
        }

        /// <summary>
        /// Loads a resource group definition from the given XML-element.
        /// </summary>
        /// <param name="resGroupElem">The XML-element to load from.</param>
        private void LoadResourceGroupDef(XElement resGroupElem)
        {
            XAttribute namespaceAttr = resGroupElem.Attribute(RESOURCEGROUP_NAMESPACE_ATTR);
            XAttribute nameAttr = resGroupElem.Attribute(RESOURCEGROUP_NAME_ATTR);
            if (namespaceAttr != null && nameAttr != null)
            {
                string resourceGroupName = string.Format("{0}.{1}", namespaceAttr.Value, nameAttr.Value);
                foreach (XElement resourceElem in resGroupElem.Elements(RESOURCE_ELEM))
                {
                    this.LoadResourceDef(resourceGroupName, resourceElem);
                }
            }
            else
            {
                /// Error: no namespace or name defined
                throw new ConfigurationException("No namespace or name defined for resource group!");
            }
        }

        /// <summary>
        /// Loads a resource definition from the given XML-element.
        /// </summary>
        /// <param name="resGroupName">The name of the group of the resource.</param>
        /// <param name="resourceElem">The XML-element to load from.</param>
        private void LoadResourceDef(string resGroupName, XElement resourceElem)
        {
            XAttribute namespaceAttr = resourceElem.Attribute(RESOURCE_NAMESPACE_ATTR);
            XAttribute nameAttr = resourceElem.Attribute(RESOURCE_NAME_ATTR);
            XAttribute loaderAttr = resourceElem.Attribute(RESOURCE_LOADER_ATTR);
            if (namespaceAttr != null && nameAttr != null && loaderAttr != null)
            {
                if (!this.loaderAssemblies.ContainsKey(loaderAttr.Value))
                {
                    throw new ConfigurationException(string.Format("Loader with name '{0}' doesn't exist"));
                }

                string resourceName = string.Format("{0}.{1}", namespaceAttr.Value, nameAttr.Value);

                /// Load the defined paths from the XML
                Dictionary<string, FileInfo> paths = new Dictionary<string, FileInfo>();
                foreach (XElement pathElem in resourceElem.Elements(PATH_ELEM))
                {
                    this.LoadPathElement(pathElem, ref paths);
                }

                /// Load the defined parameters from the XML
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                foreach (XElement paramElem in resourceElem.Elements(PARAMETER_ELEM))
                {
                    this.LoadParameterElement(paramElem, ref parameters);
                }

                /// Load the assembly that contains the resource loader class and create an instance of that class.
                Assembly asm = Assembly.Load(this.loaderAssemblies[loaderAttr.Value]);
                if (asm != null)
                {
                    Type resLoaderType = asm.GetType(this.loaderClasses[loaderAttr.Value]);
                    if (resLoaderType != null)
                    {
                        UIResourceLoader resLoader = Activator.CreateInstance(resLoaderType, new object[2] { paths, parameters }) as UIResourceLoader;
                        if (resLoader != null)
                        {
                            /// Register the created trace object to the trace manager.
                            UIResourceManager.RegisterResource(resGroupName, resourceName, resLoader);
                        }
                        else
                        {
                            throw new ConfigurationException(string.Format("Type {0} doesn't implement abstract class RC.UI.UIResourceLoader!", this.loaderClasses[loaderAttr.Value]));
                        }
                    }
                    else
                    {
                        throw new ConfigurationException(string.Format("Unable to load type {0} from assembly {1}!", this.loaderClasses[loaderAttr.Value], this.loaderAssemblies[loaderAttr.Value]));
                    }
                }
                else
                {
                    throw new ConfigurationException(string.Format("Unable to load assembly {0}!", this.loaderAssemblies[loaderAttr.Value]));
                }
            }
            else
            {
                /// Error: no namespace, name or loader defined
                throw new ConfigurationException("No namespace, name or loader defined for resource!");
            }
        }

        /// <summary>
        /// Loads a path from the given XML-element.
        /// </summary>
        /// <param name="pathElem">The XML-element to load from.</param>
        /// <param name="paths">The container that stores the collected paths.</param>
        private void LoadPathElement(XElement pathElem, ref Dictionary<string, FileInfo> paths)
        {
            XAttribute nameAttr = pathElem.Attribute(PATH_NAME_ATTR);
            if (nameAttr != null)
            {
                /// Load and check the path
                string pathStr = Path.Combine(ConfigurationManager.CurrentContext.Path.FullName, pathElem.Value);
                FileInfo path = new FileInfo(pathStr);
                string pathName = nameAttr.Value;
                if (path.Exists)
                {
                    paths.Add(pathName, path);
                }
                else
                {
                    /// Error: path doesn't exist
                    throw new ConfigurationException(string.Format("Path {0} doesn't exists!", path.FullName));
                }
            }
            else
            {
                /// Error: no name defined
                throw new ConfigurationException("No name defined for path!");
            }
        }

        /// <summary>
        /// Loads a parameter from the given XML-element.
        /// </summary>
        /// <param name="paramElem">The XML-element to load from.</param>
        /// <param name="parameters">The container that stores the collected parameters.</param>
        private void LoadParameterElement(XElement paramElem, ref Dictionary<string, string> parameters)
        {
            XAttribute nameAttr = paramElem.Attribute(PARAMETER_NAME_ATTR);
            if (nameAttr != null)
            {
                /// Load the parameter value
                parameters.Add(nameAttr.Value, paramElem.Value);
            }
            else
            {
                /// Error: no name defined
                throw new ConfigurationException("No name defined for parameter!");
            }
        }

        #endregion Internal load methods

        /// <summary>
        /// The assemblies of the defined loaders mapped by their names.
        /// </summary>
        private Dictionary<string, string> loaderAssemblies = new Dictionary<string, string>();

        /// <summary>
        /// The classes of the defined loaders mapped by their names.
        /// </summary>
        private Dictionary<string, string> loaderClasses = new Dictionary<string, string>();

        /// <summary>
        /// Supported XML elements and attributes in resource definition files.
        /// </summary>
        private const string RESOURCEFILE_ELEM = "resourceFile";
        private const string LOADER_ELEM = "loader";
        private const string LOADER_NAME_ATTR = "name";
        private const string LOADER_ASSEMBLY_ATTR = "assembly";
        private const string LOADER_CLASS_ATTR = "class";
        private const string RESOURCEGROUP_ELEM = "resourceGroup";
        private const string RESOURCEGROUP_NAMESPACE_ATTR = "namespace";
        private const string RESOURCEGROUP_NAME_ATTR = "name";
        private const string RESOURCE_ELEM = "resource";
        private const string RESOURCE_NAMESPACE_ATTR = "namespace";
        private const string RESOURCE_NAME_ATTR = "name";
        private const string RESOURCE_LOADER_ATTR = "loader";
        private const string PATH_ELEM = "path";
        private const string PATH_NAME_ATTR = "name";
        private const string PARAMETER_ELEM = "parameter";
        private const string PARAMETER_NAME_ATTR = "name";
    }
}
