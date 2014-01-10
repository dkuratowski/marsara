using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.PublicInterfaces
{
    /// <summary>
    /// Interface of views on the scenario metadata.
    /// </summary>
    public interface IMetadataView
    {
        /// <summary>
        /// Gets the map sprite types for each ma object types defined in the game engine metadata.
        /// </summary>
        /// <returns>The list of the map sprite types for map object types.</returns>
        List<SpriteDef> GetMapObjectTypes();
    }
}
