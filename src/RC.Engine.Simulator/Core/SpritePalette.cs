using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Represents the sprite palettes of the elements of the metadata.
    /// </summary>
    class SpritePalette
    {
        /// <summary>
        /// Constructs a sprite palette.
        /// </summary>
        /// <param name="imageData">The byte sequence that contains the image data of this indicator definition.</param>
        /// <param name="transpColorStr">
        /// The string that contains the transparent color of the image data or null if no transparent color is defined.
        /// </param>
        /// <param name="ownerMaskColorStr">
        /// The string that contains the owner mask color of the image data or null if no owner mask color is defined.
        /// </param>
        /// <param name="metadata">The metadata object that this sprite palette belongs to.</param>
        public SpritePalette(byte[] imageData, string transpColorStr, string ownerMaskColorStr, SimMetadata metadata)
        {
            if (metadata == null) { throw new ArgumentNullException("metadata"); }
            if (imageData == null || imageData.Length == 0) { throw new ArgumentNullException("imageData"); }

            this.imageData = imageData;
            this.transparentColorStr = transpColorStr;
            this.ownerMaskColorStr = ownerMaskColorStr;
            this.sourceRegions = new Dictionary<string, RCIntRectangle>();
            this.offsets = new Dictionary<string, RCIntVector>();

            this.metadata = metadata;
        }

        /// <summary>
        /// Checks and finalizes the indicator definition object. Buildup methods will be unavailable after
        /// calling this method.
        /// </summary>
        public void CheckAndFinalize()
        {
            if (!this.metadata.IsFinalized)
            {

            }
        }

        /// <summary>
        /// Sets the index of this indicator definition inside the metadata.
        /// </summary>
        /// <param name="newIndex">The new index.</param>
        public void SetIndex(int newIndex)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (newIndex < 0) { throw new ArgumentOutOfRangeException("newIndex", "The index of the sprite palettes must be non-negative!"); }
            this.index = newIndex;
        }

        /// <summary>
        /// Adds a frame to this sprite palette.
        /// </summary>
        /// <param name="name">The name of the frame to add.</param>
        /// <param name="sourceRegion">The source region of the frame to add.</param>
        /// <param name="offset">The offset of the frame to add.</param>
        public void AddFrame(string name, RCIntRectangle sourceRegion, RCIntVector offset)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (name == null) { throw new ArgumentNullException("name"); }
            if (sourceRegion == RCIntRectangle.Undefined) { throw new ArgumentNullException("sourceRegion"); }
            if (offset == RCIntVector.Undefined) { throw new ArgumentNullException("offset"); }
            if (this.sourceRegions.ContainsKey(name)) { throw new SimulatorException(string.Format("Frame with name '{0}' already exists!", name)); }

            this.sourceRegions.Add(name, sourceRegion);
            this.offsets.Add(name, offset);
        }

        /// <summary>
        /// Gets the byte sequence that contains the image data of this sprite palette.
        /// </summary>
        public byte[] ImageData { get { return this.imageData; } }

        /// <summary>
        /// Gets the string that contains the transparent color of the image data or null if no transparent color is defined.
        /// </summary>
        public string TransparentColorStr { get { return this.transparentColorStr; } }

        /// <summary>
        /// Gets the string that contains the owner mask color of the image data or null if no owner mask color is defined.
        /// </summary>
        public string OwnerMaskColorStr { get { return this.ownerMaskColorStr; } }

        /// <summary>
        /// Gets the index of this sprite palette inside the metadata.
        /// </summary>
        public int Index { get { return this.index; } }

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
        /// List of the source regions of the frames mapped by their names.
        /// </summary>
        private Dictionary<string, RCIntRectangle> sourceRegions;

        /// <summary>
        /// List of the offsets of the frames mapped by their names.
        /// </summary>
        private Dictionary<string, RCIntVector> offsets;

        /// <summary>
        /// Reference to the metadata object that this sprite palette belongs to.
        /// </summary>
        private SimMetadata metadata;
    }
}
