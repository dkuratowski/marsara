using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using RC.Common.Configuration;

namespace RC.UI
{
    /// <summary>
    /// Resource loader for loading named UISprites.
    /// </summary>
    class UISpriteLoader : UIResourceLoader
    {
        /// <summary>
        /// Creates a UISpriteLoader instance.
        /// </summary>
        public UISpriteLoader(Dictionary<string, FileInfo> paths, Dictionary<string, string> parameters)
            : base(paths, parameters)
        {
            if (!this.HasPath(IMAGE_FILE_PATH)) { throw new ArgumentException(string.Format("Path '{0}' is missing!", IMAGE_FILE_PATH)); }

            this.transparentColor = this.HasParameter(TRANSPARENT_COLOR_PARAM) ?
                                    UIResourceLoader.LoadColor(this.GetParameter(TRANSPARENT_COLOR_PARAM)) :
                                    UIColor.Undefined;

            this.pixelSizeStr = DynamicString.FromString(this.HasParameter(PIXEL_SIZE_PARAM) ?
                                                         this.GetParameter(PIXEL_SIZE_PARAM) :
                                                         "1;1");
            this.loadedSprite = null;
        }

        #region UIResourceLoader methods

        /// <see cref="UIResourceLoader.Load_i"/>
        protected override void Load_i()
        {
            this.loadedSprite =
                UIRoot.Instance.GraphicsPlatform.SpriteManager.LoadSprite(this.GetPath(IMAGE_FILE_PATH).FullName,
                                                                          XmlHelper.LoadVector(this.pixelSizeStr.Value));
            this.loadedSprite.TransparentColor = this.transparentColor;
        }

        /// <see cref="UIResourceLoader.Unload_i"/>
        protected override void Unload_i()
        {
            UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(this.loadedSprite);
            this.loadedSprite = null;
        }

        /// <see cref="UIResourceLoader.GetResource_i"/>
        protected override object GetResource_i()
        {
            return this.loadedSprite;
        }

        #endregion UIResourceLoader methods

        /// <summary>
        /// The transparent color of the loaded UISprite or UIColor.Undefined if transparent color was
        /// not defined.
        /// </summary>
        private UIColor transparentColor;

        /// <summary>
        /// The DynamicString that will contain the pixel size of the sprite as soon as the UIWorkspace has
        /// been initialized.
        /// </summary>
        private DynamicString pixelSizeStr;

        /// <summary>
        /// The loaded UISprite or null if the sprite is not loaded.
        /// </summary>
        private UISprite loadedSprite;

        /// <summary>
        /// The name of the paths and parameters defined by this loader.
        /// </summary>
        private const string IMAGE_FILE_PATH = "imageFile";
        private const string TRANSPARENT_COLOR_PARAM = "transparentColor";
        private const string PIXEL_SIZE_PARAM = "pixelSize";
    }
}
