using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;

namespace RC.App.PresLogic
{
    /// <summary>
    /// Common interface of a sprite-group. Sprite-groups can be used to handle loading/unloading a group of sprites that are not
    /// loaded as resources.
    /// </summary>
    public abstract class SpriteGroup
    {
        /// <summary>
        /// Creates a SpriteGroup instance.
        /// </summary>
        public SpriteGroup()
        {
            this.spriteList = new List<UISprite>();
            this.isLoaded = false;
        }

        /// <summary>
        /// Loads the sprite group.
        /// </summary>
        public void Load()
        {
            if (this.isLoaded) { throw new InvalidOperationException("The sprite-group has already been loaded!"); }

            this.spriteList = this.Load_i();
            this.isLoaded = true;
        }

        /// <summary>
        /// Unloads the sprite group.
        /// </summary>
        public void Unload()
        {
            if (!this.isLoaded) { throw new InvalidOperationException("The sprite-group is not loaded!"); }

            foreach (UISprite sprite in this.spriteList)
            {
                UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(sprite);
            }
            this.spriteList.Clear();
            this.isLoaded = false;
        }

        /// <summary>
        /// Gets the sprite at the given index.
        /// </summary>
        /// <param name="index">The index of the sprite to get.</param>
        /// <returns>The sprite at the given index.</returns>
        public UISprite this[int index] { get { return this.spriteList[index]; } }

        /// <summary>
        /// Internal implementation of loading the sprite-group that has to be implemented by the derived classes.
        /// </summary>
        /// <returns>The list of the loaded sprites.</returns>
        protected abstract List<UISprite> Load_i();

        /// <summary>
        /// List of the loaded sprites.
        /// </summary>
        private List<UISprite> spriteList;

        /// <summary>
        /// This flag indicates whether this sprite group has been loaded or not.
        /// </summary>
        private bool isLoaded;
    }
}
