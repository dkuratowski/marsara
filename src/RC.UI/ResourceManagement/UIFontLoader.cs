using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;

namespace RC.UI
{
    /// <summary>
    /// Resource loader for loading named UIFonts.
    /// </summary>
    class UIFontLoader : UIResourceLoader
    {
        /// <summary>
        /// Creates a UIFontLoader instance.
        /// </summary>
        public UIFontLoader(Dictionary<string, FileInfo> paths, Dictionary<string, string> parameters)
            : base(paths, parameters)
        {
            if (!this.HasPath(MAPPING_FILE_PATH)) { throw new ArgumentException(string.Format("Path '{0}' is missing!", MAPPING_FILE_PATH)); }
            if (!this.HasPath(IMAGE_FILE_PATH)) { throw new ArgumentException(string.Format("Path '{0}' is missing!", IMAGE_FILE_PATH)); }

            this.mappingFile = this.GetPath(MAPPING_FILE_PATH);
            this.imageFile = this.GetPath(IMAGE_FILE_PATH);
        }

        #region UIResourceLoader methods

        /// <see cref="UIResourceLoader.Load_i"/>
        protected override void Load_i()
        {
            XDocument xmlDoc = XDocument.Load(this.mappingFile.FullName);
            XElement rootElem = xmlDoc.Root;
            byte[] imageData = File.ReadAllBytes(this.imageFile.FullName);
            this.loadedFont = new UIFont(rootElem, imageData);
        }

        /// <see cref="UIResourceLoader.Unload_i"/>
        protected override void Unload_i()
        {
            this.loadedFont.Dispose();
            this.loadedFont = null;
        }

        /// <see cref="UIResourceLoader.GetResource_i"/>
        protected override object GetResource_i()
        {
            return this.loadedFont;
        }

        #endregion UIResourceLoader methods

        /// <summary>
        /// The path of the font mapping file.
        /// </summary>
        private FileInfo mappingFile;

        /// <summary>
        /// The name of the font sprite resource.
        /// </summary>
        private FileInfo imageFile;

        /// <summary>
        /// Reference to the loaded UIFont or null if the font is not loaded currently.
        /// </summary>
        private UIFont loadedFont;

        /// <summary>
        /// The name of the paths and parameters defined by this loader.
        /// </summary>
        private const string MAPPING_FILE_PATH = "mappingFile";
        private const string IMAGE_FILE_PATH = "imageFile";
    }
}
