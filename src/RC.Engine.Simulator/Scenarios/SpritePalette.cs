using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// Represents the sprite palettes of the elements of the metadata.
    /// </summary>
    class SpritePalette : ISpritePalette
    {
        /// <summary>
        /// Constructs a sprite palette.
        /// </summary>
        /// <param name="imageData">The byte sequence that contains the image data of this sprite palette.</param>
        /// <param name="transpColorStr">
        /// The string that contains the transparent color of the image data or null if no transparent color is defined.
        /// </param>
        /// <param name="ownerMaskColorStr">
        /// The string that contains the owner mask color of the image data or null if no owner mask color is defined.
        /// </param>
        /// <param name="metadata">The metadata object that this sprite palette belongs to.</param>
        public SpritePalette(byte[] imageData, string transpColorStr, string ownerMaskColorStr, ScenarioMetadata metadata)
        {
            if (metadata == null) { throw new ArgumentNullException("metadata"); }
            if (imageData == null || imageData.Length == 0) { throw new ArgumentNullException("imageData"); }

            this.imageData = imageData;
            this.transparentColorStr = transpColorStr;
            this.ownerMaskColorStr = ownerMaskColorStr;
            this.sourceRegions = new List<RCIntRectangle>();
            this.offsets = new List<RCIntVector>();
            this.indexTable = new Dictionary<string, Dictionary<MapDirection, int>>();

            this.metadata = metadata;
        }

        #region ISpritePalette members

        /// <see cref="ISpritePalette.ImageData"/>
        public byte[] ImageData { get { return this.imageData; } }

        /// <see cref="ISpritePalette.TransparentColorStr"/>
        public string TransparentColorStr { get { return this.transparentColorStr; } }

        /// <see cref="ISpritePalette.OwnerMaskColorStr"/>
        public string OwnerMaskColorStr { get { return this.ownerMaskColorStr; } }

        /// <see cref="ISpritePalette.Index"/>
        public int Index { get { return this.index; } }

        /// <see cref="ISpritePalette.GetSpriteIndex"/>
        public int GetSpriteIndex(string spriteName, MapDirection direction)
        {
            if (spriteName == null) { throw new ArgumentNullException("spriteName"); }
            if (!this.indexTable.ContainsKey(spriteName)) { throw new SimulatorException(string.Format("Sprite with name '{0}' doesn't exist!", spriteName)); }

            return this.indexTable[spriteName].ContainsKey(direction) ? this.indexTable[spriteName][direction] : -1;
        }

        /// <see cref="ISpritePalette.GetSection"/>
        public RCIntRectangle GetSection(int spriteIdx) { return this.sourceRegions[spriteIdx]; }

        /// <see cref="ISpritePalette.GetOffset"/>
        public RCIntVector GetOffset(int spriteIdx) { return this.offsets[spriteIdx]; }

        #endregion ISpritePalette members

        /// <summary>
        /// Checks and finalizes the sprite palette object. Buildup methods will be unavailable after
        /// calling this method.
        /// </summary>
        public void CheckAndFinalize()
        {
            if (!this.metadata.IsFinalized)
            {
            }
        }

        /// <summary>
        /// Sets the index of this sprite palette inside the metadata.
        /// </summary>
        /// <param name="newIndex">The new index.</param>
        public void SetIndex(int newIndex)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (newIndex < 0) { throw new ArgumentOutOfRangeException("newIndex", "The index of the sprite palettes must be non-negative!"); }
            this.index = newIndex;
        }

        /// <summary>
        /// Adds a sprite to this sprite palette.
        /// </summary>
        /// <param name="name">The name of the sprite to add.</param>
        /// <param name="sourceRegion">The source region of the sprite to add.</param>
        /// <param name="offset">The offset of the sprite to add.</param>
        public void AddSprite(string name, MapDirection direction, RCIntRectangle sourceRegion, RCIntVector offset)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (name == null) { throw new ArgumentNullException("name"); }
            if (sourceRegion == RCIntRectangle.Undefined) { throw new ArgumentNullException("sourceRegion"); }
            if (offset == RCIntVector.Undefined) { throw new ArgumentNullException("offset"); }
            if (this.indexTable.ContainsKey(name) && this.indexTable[name].ContainsKey(direction)) { throw new SimulatorException(string.Format("Sprite with name '{0}' and direction '{1}' already exists!", name, direction)); }

            if (!this.indexTable.ContainsKey(name)) { this.indexTable.Add(name, new Dictionary<MapDirection, int>()); }
            this.indexTable[name].Add(direction, this.sourceRegions.Count);
            this.sourceRegions.Add(sourceRegion);
            this.offsets.Add(offset);
        }

        /// <summary>
        /// Force giving index for the MapDirection.Undefined variant of all sprites where it is not defined.
        /// </summary>
        public void AddSpritesWithUndefinedDirection()
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }

            foreach (Dictionary<MapDirection, int> item in this.indexTable.Values)
            {
                if (!item.ContainsKey(MapDirection.Undefined))
                {
                    if (item.ContainsKey(MapDirection.North)) { item[MapDirection.Undefined] = item[MapDirection.North]; }
                    else if (item.ContainsKey(MapDirection.NorthEast)) { item[MapDirection.Undefined] = item[MapDirection.NorthEast]; }
                    else if (item.ContainsKey(MapDirection.East)) { item[MapDirection.Undefined] = item[MapDirection.East]; }
                    else if (item.ContainsKey(MapDirection.SouthEast)) { item[MapDirection.Undefined] = item[MapDirection.SouthEast]; }
                    else if (item.ContainsKey(MapDirection.South)) { item[MapDirection.Undefined] = item[MapDirection.South]; }
                    else if (item.ContainsKey(MapDirection.SouthWest)) { item[MapDirection.Undefined] = item[MapDirection.SouthWest]; }
                    else if (item.ContainsKey(MapDirection.West)) { item[MapDirection.Undefined] = item[MapDirection.West]; }
                    else if (item.ContainsKey(MapDirection.NorthWest)) { item[MapDirection.Undefined] = item[MapDirection.NorthWest]; }
                    else { throw new InvalidOperationException("Unexpected case!"); }
                }
            }
        }

        /// <summary>
        /// The byte sequence that contains the image data of this sprite palette.
        /// </summary>
        private byte[] imageData;

        /// <summary>
        /// The string that contains the transparent color of the image data.
        /// </summary>
        private string transparentColorStr;

        /// <summary>
        /// The string that contains the owner mask color of the image data.
        /// </summary>
        private string ownerMaskColorStr;

        /// <summary>
        /// The index of this sprite palette inside the metadata.
        /// </summary>
        private int index;

        /// <summary>
        /// List of the source regions of the sprites.
        /// </summary>
        private List<RCIntRectangle> sourceRegions;

        /// <summary>
        /// List of the offsets of the sprites.
        /// </summary>
        private List<RCIntVector> offsets;

        /// <summary>
        /// The indices of the sprites mapped by their names and directions.
        /// </summary>
        private Dictionary<string, Dictionary<MapDirection, int>> indexTable;

        /// <summary>
        /// Reference to the metadata object that this sprite palette belongs to.
        /// </summary>
        private ScenarioMetadata metadata;
    }
}
