using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// Represents a variant of an isometric tile type.
    /// </summary>
    class IsoTileVariant : IIsoTileVariant
    {
        /// <summary>
        /// Constructs a TileVariant object.
        /// </summary>
        /// <param name="imageData">The byte sequence that contains the image data of this variant.</param>
        /// <param name="transparentColor">The transparent color of the image of this variant.</param>
        /// <param name="tileset">Reference to the tileset of this variant.</param>
        public IsoTileVariant(byte[] imageData, TileSet tileset)
        {
            if (imageData == null || imageData.Length == 0) { throw new ArgumentNullException("imageData"); }
            if (tileset == null) { throw new ArgumentNullException("tileset"); }

            this.imageData = imageData;
            this.properties = new Dictionary<string, string>();
            this.cellDataChangesets = new List<ICellDataChangeSet>();
            this.tileset = tileset;
        }

        /// <summary>
        /// Adds a cell data changeset to this tile variant.
        /// </summary>
        /// <param name="changeset">The changeset operation to add.</param>
        public void AddCellDataChangeset(ICellDataChangeSet changeset)
        {
            if (changeset == null) { throw new ArgumentNullException("changeset"); }
            if (changeset.Tileset != this.tileset) { throw new TileSetException("The given ICellDataChangeSet is in another TileSet!"); }
            if (this.tileset.IsFinalized) { throw new InvalidOperationException("TileSet already finalized!"); }

            this.cellDataChangesets.Add(changeset);
        }

        /// <summary>
        /// Adds a property to this tile variant.
        /// </summary>
        /// <param name="propName">The name of the property.</param>
        /// <param name="propValue">The value of the property.</param>
        public void AddProperty(string propName, string propValue)
        {
            if (propName == null) { throw new ArgumentNullException("propName"); }
            if (propValue == null) { throw new ArgumentNullException("propValue"); }
            if (this.tileset.IsFinalized) { throw new InvalidOperationException("TileSet already finalized!"); }

            if (this.properties.ContainsKey(propName)) { throw new TileSetException(string.Format("Variant already contains a property with name '{0}'!", propName)); }
            this.properties.Add(propName, propValue);
        }

        /// <summary>
        /// Sets the index of this TileVariant in the tileset.
        /// </summary>
        /// <param name="newIndex">The new index.</param>
        public void SetIndex(int newIndex)
        {
            if (this.tileset.IsFinalized) { throw new InvalidOperationException("TileSet already finalized!"); }
            if (newIndex < 0) { throw new ArgumentOutOfRangeException("newIndex", "The index of the TileVariants must be non-negative!"); }
            this.index = newIndex;
        }

        /// <summary>
        /// Check and finalize the TileVariant object. Buildup methods will be unavailable after calling this method.
        /// </summary>
        public void CheckAndFinalize()
        {
        }

        #region IIsoTileVariant methods

        /// <see cref="IIsoTileVariant.CellDataChangesets"/>
        public IEnumerable<ICellDataChangeSet> CellDataChangesets { get { return this.cellDataChangesets; } }

        /// <see cref="IIsoTileVariant.Tileset"/>
        public ITileSet Tileset { get { return this.tileset; } }

        /// <see cref="IIsoTileVariant.ImageData"/>
        public byte[] ImageData { get { return this.imageData; } }

        /// <see cref="IIsoTileVariant.Index"/>
        public int Index { get { return this.index; } }

        /// <see cref="IIsoTileVariant.GetProperty"/>
        public string GetProperty(string propName)
        {
            if (propName == null) { throw new ArgumentNullException("propName"); }
            return this.properties.ContainsKey(propName) ? this.properties[propName] : null;
        }

        #endregion IIsoTileVariant methods

        /// <summary>
        /// The byte sequence that contains the image data of this variant.
        /// </summary>
        private byte[] imageData;

        /// <summary>
        /// List of the properties of this tile variant mapped by their name.
        /// </summary>
        private Dictionary<string, string> properties;

        /// <summary>
        /// List of the cell data changesets of this variant.
        /// </summary>
        private List<ICellDataChangeSet> cellDataChangesets;

        /// <summary>
        /// Reference to the tileset of this variant.
        /// </summary>
        private TileSet tileset;

        /// <summary>
        /// The index of this TileVariant in the tileset.
        /// </summary>
        private int index;
    }
}
