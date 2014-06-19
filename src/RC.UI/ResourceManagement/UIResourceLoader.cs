using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RC.Common.Configuration;
using RC.Common;

namespace RC.UI
{
    /// <summary>
    /// Abstract base class of loaders of different types of resources.
    /// </summary>
    public abstract class UIResourceLoader
    {
        /// <summary>
        /// Creates a UIResourceLoader instance.
        /// </summary>
        /// <param name="paths">The defined paths for loading the resource.</param>
        /// <param name="parameters">The defined parameters for loading the resource.</param>
        public UIResourceLoader(Dictionary<string, FileInfo> paths, Dictionary<string, string> parameters)
        {
            this.paths = new Dictionary<string, FileInfo>(paths);
            this.parameters = new Dictionary<string, string>(parameters);
            this.isLoaded = false;
        }

        /// <summary>
        /// Loads the defined resource. If the resource has already been loaded then this function has no effect.
        /// </summary>
        public void Load()
        {
            if (!this.isLoaded)
            {
                this.Load_i();
                this.isLoaded = true;
            }
        }

        /// <summary>
        /// Gets the underlying loaded resource.
        /// </summary>
        /// <typeparam name="T">The type of the resource to get.</typeparam>
        /// <exception cref="InvalidOperationException">
        /// If the resource has not yet been loaded.        
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// If the underlying loaded resource is not compatible with T.
        /// </exception>
        public T GetResource<T>() where T : class
        {
            if (!this.isLoaded) { throw new InvalidOperationException("The resource has not yet been loaded!"); }

            /// Get and cast the underlying resource.
            object resObj = this.GetResource_i();
            T resource = resObj as T;

            /// Check the compatibility and return the resource object.
            if (resource == null) { throw new InvalidCastException(string.Format("The resource is not compatible with type '{0}'!", typeof(T).FullName)); }
            return resource;
        }

        /// <summary>
        /// Unloads the defined resource. If the resource has already been unloaded then this function has no effect.
        /// </summary>
        public void Unload()
        {
            if (this.isLoaded)
            {
                this.Unload_i();
                this.isLoaded = false;
            }
        }

        /// <summary>
        /// Gets the value of the given parameter.
        /// </summary>
        /// <param name="paramName">The name of the parameter to get.</param>
        /// <returns>The value of the given parameter.</returns>
        protected string GetParameter(string paramName)
        {
            if (paramName == null) { throw new ArgumentNullException("paramName"); }
            if (!this.parameters.ContainsKey(paramName)) { throw new ArgumentException(string.Format("Parameter '{0}' not defined for resource loader!", paramName), "paramName"); }
            return this.parameters[paramName];
        }

        /// <summary>
        /// Checks whether the given parameter is present or not.
        /// </summary>
        /// <param name="paramName">The name of the parameter to check.</param>
        /// <returns>True if the parameter is present, false otherwise.</returns>
        protected bool HasParameter(string paramName)
        {
            if (paramName == null) { throw new ArgumentNullException("paramName"); }
            return this.parameters.ContainsKey(paramName);
        }

        /// <summary>
        /// Gets the given path.
        /// </summary>
        /// <param name="pathName">The name of the path to get.</param>
        /// <returns>The given path.</returns>
        protected FileInfo GetPath(string pathName)
        {
            if (pathName == null) { throw new ArgumentNullException("pathName"); }
            if (!this.paths.ContainsKey(pathName)) { throw new ArgumentException(string.Format("Path '{0}' not defined for resource loader!", pathName), "pathName"); }
            return this.paths[pathName];
        }

        /// <summary>
        /// Checks whether the given path is present or not.
        /// </summary>
        /// <param name="pathName">The name of the path to check.</param>
        /// <returns>True if the path is present, false otherwise.</returns>
        protected bool HasPath(string pathName)
        {
            if (pathName == null) { throw new ArgumentNullException("pathName"); }
            return this.paths.ContainsKey(pathName);
        }

        /// <summary>
        /// Internal method that actually loads the resource. Must be implemented in the derived classes.
        /// </summary>
        protected abstract void Load_i();

        /// <summary>
        /// Internal method that actually unloads the resource. Must be implemented in the derived classes.
        /// </summary>
        protected abstract void Unload_i();

        /// <summary>
        /// Internal method that returns the underlying loaded resource. Must be implemented in the derived classes.
        /// </summary>
        protected abstract object GetResource_i();

        /// <summary>
        /// The defined paths for loading the resource.
        /// </summary>
        private Dictionary<string, FileInfo> paths;

        /// <summary>
        /// The defined parameters for loading the resource.
        /// </summary>
        private Dictionary<string, string> parameters;

        /// <summary>
        /// This flag indicates whether the resource is loaded.
        /// </summary>
        private bool isLoaded;
    }
}
