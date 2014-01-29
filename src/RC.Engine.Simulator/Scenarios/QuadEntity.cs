using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// Entities whose position is bound to the quadratic grid of the map.
    /// </summary>
    public abstract class QuadEntity : Entity
    {
        /// <summary>
        /// Constructs a QuadEntity instance.
        /// </summary>
        /// <param name="elementTypeName">The name of the element type of this entity.</param>
        /// <param name="quadCoords">The quadratic coordinates of the entity.</param>
        public QuadEntity(string elementTypeName, RCIntVector quadCoords)
            : base(elementTypeName)
        {
            if (quadCoords == RCIntVector.Undefined) { throw new ArgumentNullException("quadCoords"); }
            this.quadCoords = quadCoords;
        }

        /// <summary>
        /// Gets the quadratic coordinates of this entity.
        /// </summary>
        public RCIntVector QuadCoords { get { return this.quadCoords; } }

        /// <summary>
        /// Sets the quadratic coordinates of the entity.
        /// </summary>
        /// <param name="newQuadCoords">The new quadratic coordinates of the entity.</param>
        public void SetQuadCoords(RCIntVector newQuadCoords)
        {
            if (quadCoords == RCIntVector.Undefined) { throw new ArgumentNullException("quadCoords"); }
            this.quadCoords = newQuadCoords;
            this.SynchPosition();
        }

        /// <see cref="Entity.OnAddedToScenarioImpl"/>
        protected override void OnAddedToScenarioImpl()
        {
            this.SynchPosition();
            this.Scenario.VisibleEntities.AttachContent(this);
        }

        /// <summary>
        /// Synchronizes the position of this entity with the stored quadratic coordinates.
        /// </summary>
        private void SynchPosition()
        {
            ICell topLeftCell = this.Scenario.Map.GetQuadTile(this.quadCoords).GetCell(new RCIntVector(0, 0));
            this.SetPosition(topLeftCell.MapCoords - new RCNumVector(1, 1) / 2 + this.ElementType.Area.Read() / 2);
        }
        
        /// <summary>
        /// The coordinates of the top left quadratic tile where this entity is placed.
        /// </summary>
        private RCIntVector quadCoords;
    }
}
