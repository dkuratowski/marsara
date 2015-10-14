using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// Interface of views on the scenario metadata.
    /// </summary>
    public interface IMetadataView
    {
        /// <summary>
        /// Gets the sprite definitions for each map object types defined in the game engine metadata.
        /// </summary>
        /// <returns>The list of the sprite data for the map object types.</returns>
        List<SpriteData> GetMapObjectSpriteData();

        /// <summary>
        /// Gets the HP indicator icon definition for each map object types defined in the game engine metadata.
        /// </summary>
        /// <returns>The list of HP indicator icon data for the map object types.</returns>
        List<SpriteData> GetMapObjectHPIconData();

        /// <summary>
        /// Gets the shadow palette defined in the game engine metadata.
        /// </summary>
        /// <returns>
        /// The list of shadow sprite data or and empty list if no sprite palette has been defined in the game engine metadata.
        /// </returns>
        List<SpriteData> GetShadowSpriteData();

        /// <summary>
        /// Gets the displayed names of all map object types mapped by their type IDs.
        /// </summary>
        /// <returns>The displayed names of all map object types mapped by their type IDs.</returns>
        Dictionary<int, string> GetMapObjectDisplayedTypeNames();
    }
}
