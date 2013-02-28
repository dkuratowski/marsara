using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.PublicInterfaces
{
    /// <summary>
    /// Defines the interface of an isometric tile-variant.
    /// </summary>
    public interface IIsoTileVariant
    {

        /// <summary>
        /// Gets the cell data changesets of this variant.
        /// </summary>
        IEnumerable<ICellDataChangeSet> CellDataChangesets { get; }

        /// <summary>
        /// Gets the tileset of this variant.
        /// </summary>
        ITileSet Tileset { get; }

        /// <summary>
        /// Gets the image data of this variant.
        /// </summary>
        byte[] ImageData { get; }

        /// <summary>
        /// Gets the index of this TileVariant in the tileset.
        /// </summary>
        int Index { get; }

        /// <summary>
        /// Gets the value of a given property.
        /// </summary>
        /// <param name="propName">The name of the property to get.</param>
        /// <returns>The value of the property of null if the property doesn't exists.</returns>
        string GetProperty(string propName);
    }
}
