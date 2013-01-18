using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;

namespace RC.UI
{
    /// <summary>
    /// Resource loader for loading named UIAnimations.
    /// </summary>
    class UIAnimationLoader : UIResourceLoader
    {
        /// <summary>
        /// Creates a UIAnimationLoader instance.
        /// </summary>
        public UIAnimationLoader(Dictionary<string, FileInfo> paths, Dictionary<string, string> parameters)
            : base(paths, parameters)
        {
            if (!this.HasPath(ANIMATION_FILE_PATH)) { throw new ArgumentException(string.Format("Path '{0}' is missing!", ANIMATION_FILE_PATH)); }

            this.animationFile = this.GetPath(ANIMATION_FILE_PATH);
        }

        #region UIResourceLoader methods

        /// <see cref="UIResourceLoader.Load_i"/>
        protected override void Load_i()
        {
            XDocument xmlDoc = XDocument.Load(this.animationFile.FullName);
            XElement rootElem = xmlDoc.Root;
            this.loadedAnimation = new UIAnimation(rootElem);
        }

        /// <see cref="UIResourceLoader.Unload_i"/>
        protected override void Unload_i()
        {
            this.loadedAnimation.Dispose();
            this.loadedAnimation = null;
        }

        /// <see cref="UIResourceLoader.GetResource_i"/>
        protected override object GetResource_i()
        {
            return this.loadedAnimation;
        }

        #endregion UIResourceLoader methods

        /// <summary>
        /// The path of the animation file.
        /// </summary>
        private FileInfo animationFile;

        /// <summary>
        /// Reference to the loaded UIAnimation or null if the animation is not loaded currently.
        /// </summary>
        private UIAnimation loadedAnimation;

        /// <summary>
        /// The name of the paths and parameters defined by this loader.
        /// </summary>
        private const string ANIMATION_FILE_PATH = "animationFile";
    }
}
