using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.Configuration;

namespace RC.UI
{
    /// <summary>
    /// Resource loader for loading named mouse pointers.
    /// </summary>
    class UIPointerLoader : UISpriteLoader
    {
        /// <summary>
        /// Creates a UIPointerLoader instance.
        /// </summary>
        public UIPointerLoader(Dictionary<string, FileInfo> paths, Dictionary<string, string> parameters)
            : base(paths, parameters)
        {
            this.offset = this.HasParameter(OFFSET_PARAM) ? XmlHelper.LoadIntVector(this.GetParameter(OFFSET_PARAM)) : new RCIntVector(0, 0);
            this.loadedPointer = null;
        }

        #region UIResourceLoader methods

        /// <see cref="UIResourceLoader.Load_i"/>
        protected override void Load_i()
        {
            base.Load_i();
            this.loadedPointer = new UIPointer(this.LoadedSprite, this.offset);
        }

        /// <see cref="UIResourceLoader.Unload_i"/>
        protected override void Unload_i()
        {
            base.Unload_i();
            this.loadedPointer = null;
        }

        /// <see cref="UIResourceLoader.GetResource_i"/>
        protected override object GetResource_i()
        {
            return this.loadedPointer;
        }

        #endregion UIResourceLoader methods

        /// <summary>
        /// The loaded UISprite or null if the sprite is not loaded.
        /// </summary>
        private UIPointer loadedPointer;

        /// <summary>
        /// The offset of the pointer.
        /// </summary>
        private RCIntVector offset;

        /// <summary>
        /// The name of the paths and parameters defined by this loader.
        /// </summary>
        private const string OFFSET_PARAM = "offset";
    }
}
