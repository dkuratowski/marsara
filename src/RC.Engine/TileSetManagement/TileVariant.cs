using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine
{
    /// <summary>
    /// Represents a variant of a tile type.
    /// </summary>
    public class TileVariant
    {
        /// <summary>
        /// Constructs a TileVariant object.
        /// </summary>
        /// <param name="imageData">The byte sequence that contains the image data of this variant.</param>
        /// <param name="transparentColor">The transparent color of the image of this variant.</param>
        /// <param name="tileset">Reference to the tileset of this variant.</param>
        public TileVariant(byte[] imageData, TileSet tileset)
        {
            if (imageData == null || imageData.Length == 0) { throw new ArgumentNullException("imageData"); }
            if (tileset == null) { throw new ArgumentNullException("tileset"); }

            this.imageData = imageData;
            this.properties = new Dictionary<string, string>();
            this.overwritings = new List<ITileDataOverwriting>();
            this.tileset = tileset;
        }

        /// <summary>
        /// Adds an overwriting operation to this tile variant.
        /// </summary>
        /// <param name="overwriting">The overwriting operation to add.</param>
        public void AddOverwriting(ITileDataOverwriting overwriting)
        {
            if (overwriting == null) { throw new ArgumentNullException("overwriting"); }
            if (overwriting.Tileset != this.tileset) { throw new TileSetException("The given ITileDataOverwriting is in another TileSet!"); }
            if (this.tileset.IsFinalized) { throw new InvalidOperationException("TileSet already finalized!"); }

            this.overwritings.Add(overwriting);
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

        /// <summary>
        /// Gets the overwriting operations of this variant.
        /// </summary>
        public IEnumerable<ITileDataOverwriting> Overwritings { get { return this.overwritings; } }

        /// <summary>
        /// Gets the tileset of this variant.
        /// </summary>
        public TileSet Tileset { get { return this.tileset; } }

        /// <summary>
        /// Gets the image data of this variant.
        /// </summary>
        public byte[] ImageData { get { return this.imageData; } }

        /// <summary>
        /// Gets the index of this TileVariant in the tileset.
        /// </summary>
        public int Index { get { return this.index; } }

        /// <summary>
        /// Gets the value of a given property.
        /// </summary>
        /// <param name="propName">The name of the property to get.</param>
        /// <returns>The value of the property of null if the property doesn't exists.</returns>
        public string this[string propName]
        {
            get
            {
                if (propName == null) { throw new ArgumentNullException("propName"); }
                return this.properties.ContainsKey(propName) ? this.properties[propName] : null;
            }
        }

        /// <summary>
        /// The byte sequence that contains the image data of this variant.
        /// </summary>
        private byte[] imageData;

        /// <summary>
        /// List of the properties of this tile variant mapped by their name.
        /// </summary>
        private Dictionary<string, string> properties;

        /// <summary>
        /// List of the data overwriting operations of this variant.
        /// </summary>
        private List<ITileDataOverwriting> overwritings;

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
