using System.Collections.Generic;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.App.BizLogic.BusinessComponents
{
    /// <summary>
    /// The interface of the business component that is used to access informations about the available tilesets.
    /// </summary>
    [ComponentInterface]
    interface ITilesetManagerBC
    {
        /// <summary>
        /// Gets the tileset with the given name.
        /// </summary>
        /// <param name="name">The name of the tileset to get.</param>
        /// <returns>The tileset with the given name or null if no tileset were loaded with the given name.</returns>
        ITileSet GetTileSet(string name);
    }
}
