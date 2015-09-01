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
        /// <returns>The list of the sprite definitions for the map object types.</returns>
        List<SpriteDef> GetMapObjectSpriteDefs();

        /// <summary>
        /// Gets the HP indicator icon definition for each map object types defined in the game engine metadata.
        /// </summary>
        /// <returns>The list of HP indicator icon definitions for the map object types.</returns>
        List<SpriteDef> GetMapObjectHPIconDefs();
    }
}
