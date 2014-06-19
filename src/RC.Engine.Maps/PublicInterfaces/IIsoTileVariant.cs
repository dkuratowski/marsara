using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Maps.PublicInterfaces
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
        /// Gets the transparent color of this variant.
        /// </summary>
        RCColor TransparentColor { get; }

        /// <summary>
        /// Gets the index of this TileVariant in the tileset. Note that this is the absolute index of this
        /// variant that is not equal with the variant index of the isometric tiles (for getting the variant
        /// index of an isometric tile use the IIsoTile.VariantIdx property)!
        /// </summary>
        int Index { get; }
    }
}
