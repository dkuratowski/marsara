using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Commands;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.MotionControl;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Represents a visualization of a scenario element.
    /// </summary>
    public class MapObject : HeapedObject, ISearchTreeContent
    {
        /// <summary>
        /// Constructs a MapObject instance.
        /// </summary>
        /// <param name="owner">Reference to the scenario element that owns this visualization.</param>
        public MapObject(ScenarioElement owner)
        {
            if (owner == null) { throw new ArgumentNullException("owner"); }

            this.owner = this.ConstructField<ScenarioElement>("owner");
            this.location = this.ConstructField<RCNumRectangle>("location");

            this.currentAnimations = new List<AnimationPlayer>();
            this.quadraticPositionCache = new CachedValue<RCIntRectangle>(() =>
            {
                if (this.location.Read() == RCNumRectangle.Undefined) { return RCIntRectangle.Undefined; }
                RCIntVector topLeft = this.location.Read().Location.Round();
                RCIntVector bottomRight = (this.location.Read().Location + this.location.Read().Size).Round();
                RCIntRectangle cellRect = new RCIntRectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
                return this.owner.Read().Scenario.Map.CellToQuadRect(cellRect);
            });

            this.owner.Write(owner);
            this.location.Write(RCNumRectangle.Undefined);
            this.location.ValueChanged += (sender, args) => this.quadraticPositionCache.Invalidate();
        }

        #region Public interface

        /// <summary>
        /// Gets the players of the currently active animations of this map object.
        /// </summary>
        public IEnumerable<AnimationPlayer> CurrentAnimations { get { return this.currentAnimations; } }

        /// <summary>
        /// Gets the quadratic position of this map object or RCIntRectangle.Undefined if this map object is not attached to the map.
        /// </summary>
        public RCIntRectangle QuadraticPosition { get { return this.quadraticPositionCache.Value; } }

        /// <summary>
        /// Gets the reference to the scenario element that owns this map object.
        /// </summary>
        public ScenarioElement Owner { get { return this.owner.Read(); } }

        /// <summary>
        /// Sets the location of this map object.
        /// </summary>
        /// <param name="newLocation">The new location of this map object.</param>
        public void SetLocation(RCNumRectangle newLocation)
        {
            if (newLocation == RCNumRectangle.Undefined) { throw new ArgumentNullException("newLocation"); }

            if (this.BoundingBoxChanging != null) { this.BoundingBoxChanging(this); }
            this.location.Write(newLocation);
            if (this.BoundingBoxChanged != null) { this.BoundingBoxChanged(this); }
        }

        /// <summary>
        /// Sets the current animation of this map object with undefined direction.
        /// </summary>
        /// <param name="animationName">The name of the animation to play.</param>
        public void SetCurrentAnimation(string animationName)
        {
            this.SetCurrentAnimation(animationName, MapDirection.Undefined);
        }

        /// <summary>
        /// Sets the current animation of this map object with the given direction.
        /// </summary>
        /// <param name="animationName">The name of the animation to play.</param>
        /// <param name="direction">The direction of the animation.</param>
        public void SetCurrentAnimation(string animationName, MapDirection direction)
        {
            if (animationName == null) { throw new ArgumentNullException("animationName"); }
            this.SetCurrentAnimations(new List<string>() { animationName }, direction);
        }

        /// <summary>
        /// Sets the current animations of this map object with undefined direction.
        /// </summary>
        /// <param name="animationNames">The names of the animations to play.</param>
        public void SetCurrentAnimations(List<string> animationNames)
        {
            this.SetCurrentAnimations(animationNames, MapDirection.Undefined);
        }

        /// <summary>
        /// Sets the current animations of this map object with the given direction.
        /// </summary>
        /// <param name="animationNames">The names of the animations to play.</param>
        /// <param name="direction">The direction of the animations.</param>
        public void SetCurrentAnimations(List<string> animationNames, MapDirection direction)
        {
            if (animationNames == null) { throw new ArgumentNullException("animationNames"); }

            this.currentAnimations.Clear();
            foreach (string name in animationNames)
            {
                this.currentAnimations.Add(new AnimationPlayer(this.owner.Read().ElementType.AnimationPalette.GetAnimation(name), direction));
            }
        }

        #endregion Public interface

        #region ISearchTreeContent members

        /// <see cref="ISearchTreeContent.BoundingBox"/>
        public RCNumRectangle BoundingBox
        {
            get { return this.location.Read(); }
        }

        /// <see cref="ISearchTreeContent.BoundingBoxChanging"/>
        public event ContentBoundingBoxChangeHdl BoundingBoxChanging;

        /// <see cref="ISearchTreeContent.BoundingBoxChanged"/>
        public event ContentBoundingBoxChangeHdl BoundingBoxChanged;

        #endregion ISearchTreeContent members

        #region Heaped members

        /// <summary>
        /// The location of this map object on the map.
        /// </summary>
        private readonly HeapedValue<RCNumRectangle> location;

        /// <summary>
        /// Reference to the scenario element that owns this map object.
        /// </summary>
        private readonly HeapedValue<ScenarioElement> owner;

        #endregion Heaped members

        /// <summary>
        /// The player of the currently active animations of this map object.
        /// </summary>
        private readonly List<AnimationPlayer> currentAnimations;

        /// <summary>
        /// The cached value of the quadratic position of this map object.
        /// </summary>
        private CachedValue<RCIntRectangle> quadraticPositionCache;
    }
}
