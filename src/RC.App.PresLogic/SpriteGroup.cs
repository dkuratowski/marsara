using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;

namespace RC.App.PresLogic
{
    /// <summary>
    /// Common base class of a sprite-group. Sprite-groups can be used to handle loading/unloading a group of sprites that are not
    /// loaded as resources.
    /// </summary>
    public abstract class SpriteGroup : ISpriteGroup
    {
        /// <summary>
        /// Creates a SpriteGroup instance.
        /// </summary>
        protected SpriteGroup()
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

            /// Destroy the sprites of this sprite group (be aware of duplicated sprites).
            HashSet<UISprite> destroyedSprites = new HashSet<UISprite>();
            foreach (UISprite sprite in this.spriteList)
            {
                if (sprite != null && !destroyedSprites.Contains(sprite))
                {
                    UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(sprite);
                    destroyedSprites.Add(sprite);
                }
            }
            this.spriteList.Clear();
            this.isLoaded = false;
        }

        #region ISpriteGroup members

        /// <see cref="ISpriteGroup.Item"/>
        public UISprite this[int index] { get { return this.spriteList[index]; } }

        #endregion ISpriteGroup members

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
