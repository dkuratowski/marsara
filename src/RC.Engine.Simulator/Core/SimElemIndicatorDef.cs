using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Represents an indicator definition for a simulation element type.
    /// </summary>
    class SimElemIndicatorDef
    {
        /// <summary>
        /// Constructs an indicator definition object for a simulation element type.
        /// </summary>
        /// <param name="elementType">The name of the corresponding element type.</param>
        /// <param name="imageData">The byte sequence that contains the image data of this indicator definition.</param>
        /// <param name="transpColorStr">
        /// The string that contains the transparent color of the image data or null if no transparent color is defined.
        /// </param>
        /// <param name="ownerMaskColorStr">
        /// The string that contains the owner mask color of the image data or null if no owner mask color is defined.
        /// </param>
        public SimElemIndicatorDef(string elementType, byte[] imageData, string transpColorStr, string ownerMaskColorStr)
        {
            if (elementType == null) { throw new ArgumentNullException("elementType"); }
            if (imageData == null || imageData.Length == 0) { throw new ArgumentNullException("imageData"); }

            this.isFinalized = false;
            this.elementType = elementType;
            this.imageData = imageData;
            this.transparentColorStr = transpColorStr;
            this.ownerMaskColorStr = ownerMaskColorStr;
            this.animations = new Dictionary<string, List<SimElemAnimFrame>>();
        }

        /// <summary>
        /// Checks and finalizes the indicator definition object. Buildup methods will be unavailable after
        /// calling this method.
        /// </summary>
        public void CheckAndFinalize()
        {
            this.isFinalized = true;
        }

        /// <summary>
        /// Sets the index of this indicator definition inside the metadata.
        /// </summary>
        /// <param name="newIndex">The new index.</param>
        public void SetIndex(int newIndex)
        {
            if (this.isFinalized) { throw new InvalidOperationException("SimElemIndicatorDef already finalized!"); }
            if (newIndex < 0) { throw new ArgumentOutOfRangeException("newIndex", "The index of the indicator definitions must be non-negative!"); }
            this.index = newIndex;
        }

        /// <summary>
        /// Adds an animation to this indicator definition.
        /// </summary>
        /// <param name="name">The name of the animation to add.</param>
        /// <param name="frames">The frames of the animation to add.</param>
        public void AddAnimation(string name, List<SimElemAnimFrame> frames)
        {
            if (this.isFinalized) { throw new InvalidOperationException("SimElemIndicatorDef already finalized!"); }
            if (name == null) { throw new ArgumentNullException("name"); }
            if (frames == null || frames.Count == 0) { throw new ArgumentNullException("frames"); }
            if (this.animations.ContainsKey(name)) { throw new SimulatorException(string.Format("Animation with name '{0}' already defined for simulation element type '{1}'!", name, this.elementType)); }

            this.animations.Add(name, new List<SimElemAnimFrame>(frames));
        }

        /// <summary>
        /// Gets the name of the element type that this indicator definition belongs to.
        /// </summary>
        public string ElementType { get { return this.elementType; } }

        /// <summary>
        /// Gets the byte sequence that contains the image data of this indicator definition.
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
        /// Gets the index of this indicator definition inside the metadata.
        /// </summary>
        public int Index { get { return this.index; } }

        /// <summary>
        /// The name of the element type that this indicator definition belongs to.
        /// </summary>
        private string elementType;

        /// <summary>
        /// The byte sequence that contains the image data of this indicator definition.
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
        /// The index of this indicator definition inside the metadata.
        /// </summary>
        private int index;

        /// <summary>
        /// List of the animations of this indicator definition mapped by their names.
        /// </summary>
        private Dictionary<string, List<SimElemAnimFrame>> animations;

        /// <summary>
        /// Becomes true when this indicator definition is finalized.
        /// </summary>
        private bool isFinalized;
    }

    /// <summary>
    /// Represents a frame of an animation of a simulation element indicator.
    /// </summary>
    struct SimElemAnimFrame
    {
        /// <summary>
        /// Constructs an animation frame.
        /// </summary>
        /// <param name="sourceRegion">
        /// The region inside the image of the corresponding indicator to be displayed in this frame.
        /// </param>
        /// <param name="offset">
        /// The offset vector from the top left corner of the indicator's position to the top left corner
        /// of the displayed image.
        /// </param>
        public SimElemAnimFrame(RCIntRectangle sourceRegion, RCIntVector offset)
        {
            if (sourceRegion == RCIntRectangle.Undefined) { throw new ArgumentNullException("sourceRegion"); }
            if (offset == RCIntVector.Undefined) { throw new ArgumentNullException("offset"); }

            this.sourceRegion = sourceRegion;
            this.offset = offset;
        }

        /// <summary>
        /// Gets the region inside the image of the corresponding indicator to be displayed in this frame.
        /// </summary>
        public RCIntRectangle SourceRegion { get { return this.sourceRegion; } }

        /// <summary>
        /// Gets the offset vector from the top left corner of the indicator's position to the top left corner
        /// of the displayed image.
        /// </summary>
        public RCIntVector Offset { get { return this.offset; } }

        /// <summary>
        /// The region inside the image of the corresponding indicator to be displayed in this frame.
        /// </summary>
        private RCIntRectangle sourceRegion;

        /// <summary>
        /// The offset vector from the top left corner of the indicator's position to the top left corner
        /// of the displayed image.
        /// </summary>
        private RCIntVector offset;
    }
}
