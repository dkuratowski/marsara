using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Represents a game object on the map.
    /// </summary>
    class GameObject : IGameObject
    {
        /// <summary>
        /// Constructs a game object.
        /// </summary>
        /// <param name="position">The position of this game object on the map.</param>
        public GameObject(RCNumRectangle position)
        {
            if (position == RCNumRectangle.Undefined) { throw new ArgumentNullException("position"); }
            this.position = position;
            this.isSelected = false;
        }

        /// <see cref="IMapContent.Position"/>
        public RCNumRectangle Position { get { return this.position; } }

        /// <see cref="IMapContent.PositionChanging"/>
        public event MapContentPropertyChangeHdl PositionChanging;

        /// <see cref="IMapContent.PositionChanged"/>
        public event MapContentPropertyChangeHdl PositionChanged;

        /// <summary>
        /// The position of this game object.
        /// </summary>
        private RCNumRectangle position;

        /// <summary>
        /// True if this game object is currently selected, false otherwise.
        /// </summary>
        private bool isSelected;
    }
}
