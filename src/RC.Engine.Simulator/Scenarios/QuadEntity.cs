using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// Entities whose position can be bound to the quadratic grid of the map. The position of the QuadEntity cannot be
    /// changed while it is bound to the quadratic grid of the map.
    /// </summary>
    public abstract class QuadEntity : Entity
    {
        /// <summary>
        /// Constructs a QuadEntity instance.
        /// </summary>
        /// <param name="elementTypeName">The name of the element type of this entity.</param>
        /// <param name="quadCoords">The quadratic coordinates of the entity.</param>
        public QuadEntity(string elementTypeName)
            : base(elementTypeName)
        {
            this.lastKnownQuadCoords = this.ConstructField<RCIntVector>("lastKnownQuadCoords");
            this.isBoundToGrid = this.ConstructField<byte>("isBoundToGrid");
            this.lastKnownQuadCoords.Write(RCIntVector.Undefined);
            this.isBoundToGrid.Write(0x00);
        }

        /// <summary>
        /// Gets the last known quadratic coordinates of this QuadEntity or RCIntVectorUndefined if this QuadEntity
        /// has not yet been bound to the quadratic grid of the map.
        /// </summary>
        public RCIntVector LastKnownQuadCoords { get { return this.lastKnownQuadCoords.Read(); } }

        /// <summary>
        /// Gets whether this QuadEntity is currently bound to the quadratic grid or not.
        /// </summary>
        public bool IsBoundToGrid { get { return this.isBoundToGrid.Read() != 0x00; } }
        
        /// <summary>
        /// This method is called when this QuadEntity is being added to the map.
        /// </summary>
        /// <param name="topLeftTile">The quadratic tile at the top-left corner of this QuadEntity.</param>
        /// <returns>True if the given position is valid for this QuadEntity; otherwise false.</returns>
        internal bool OnAttachingToMap(IQuadTile topLeftTile)
        {
            ICell topLeftCell = topLeftTile.GetCell(new RCIntVector(0, 0));
            RCNumVector position = topLeftCell.MapCoords - new RCNumVector(1, 1) / 2 + this.ElementType.Area.Read() / 2;

            bool isValidPosition = this.ValidatePosition(position);
            if (isValidPosition)
            {
                this.lastKnownQuadCoords.Write(topLeftTile.MapCoords);
                this.SetPosition(position);
                this.PositionValue.ValueChanged += this.OnPositionChanged;
                this.isBoundToGrid.Write(0x01);
            }
            return isValidPosition;
        }

        /// <summary>
        /// This event handler is called when this QuadEntity is bound to the quadratic grid of the map and the
        /// position is changed.
        /// </summary>
        private void OnPositionChanged(object sender, EventArgs args)
        {
            if (this.PositionValue.Read() != RCNumVector.Undefined) { throw new InvalidOperationException("Position of a QuadEntity cannot be changed while it is bound to the quadratic grid of the map!"); }

            /// Position became RCNumVector.Undefined -> entity removed from the map so we have to unsubscribe.
            this.PositionValue.ValueChanged -= this.OnPositionChanged;
            this.isBoundToGrid.Write(0x00);
        }

        #region Heaped members

        /// <summary>
        /// The last known quadratic coordinates of this QuadEntity or RCIntVector.Undefined if this QuadEntity
        /// has not yet been bound to the quadratic grid of the map.
        /// </summary>
        private readonly HeapedValue<RCIntVector> lastKnownQuadCoords;

        /// <summary>
        /// This flag indicates whether this QuadEntity is currently bound to the quadratic grid (any value other than 0x00) or not (0x00).
        /// </summary>
        private readonly HeapedValue<byte> isBoundToGrid;

        #endregion Heaped members
    }
}
