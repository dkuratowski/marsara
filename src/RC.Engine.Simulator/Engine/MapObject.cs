using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Commands;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Core;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.MotionControl;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Represents a visualization of a scenario element.
    /// </summary>
    public class MapObject : ISearchTreeContent
    {
        /// <summary>
        /// Constructs a MapObject instance.
        /// </summary>
        /// <param name="owner">Reference to the scenario element that owns this map object.</param>
        public MapObject(ScenarioElement owner)
        {
            if (owner == null) { throw new ArgumentNullException("owner"); }

            this.currentAnimations = new List<AnimationPlayer>();
            this.quadraticPositionCache = new CachedValue<RCIntRectangle>(() =>
            {
                if (this.location == RCNumRectangle.Undefined) { return RCIntRectangle.Undefined; }
                RCIntVector topLeft = this.location.Location.Round();
                RCIntVector bottomRight = (this.location.Location + this.location.Size).Round();
                RCIntRectangle cellRect = new RCIntRectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
                return this.owner.Scenario.Map.CellToQuadRect(cellRect);
            });

            this.owner = owner;
            this.location = RCNumRectangle.Undefined;
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
        public ScenarioElement Owner { get { return this.owner; } }

        /// <summary>
        /// Sets the location of this map object.
        /// </summary>
        /// <param name="newLocation">The new location of this map object.</param>
        public void SetLocation(RCNumRectangle newLocation)
        {
            if (newLocation == RCNumRectangle.Undefined) { throw new ArgumentNullException("newLocation"); }

            if (this.location != newLocation)
            {
                if (this.BoundingBoxChanging != null) { this.BoundingBoxChanging(this); }
                this.location = newLocation;
                this.quadraticPositionCache.Invalidate();
                if (this.BoundingBoxChanged != null) { this.BoundingBoxChanged(this); }
            }
        }

        /// <summary>
        /// Sets the current animation of this map object with the given heading vector.
        /// </summary>
        /// <param name="animationName">The name of the animation to play.</param>
        /// <param name="headingVector">The heading vector.</param>
        public void SetCurrentAnimation(string animationName, IValueRead<RCNumVector> headingVector)
        {
            if (animationName == null) { throw new ArgumentNullException("animationName"); }
            if (headingVector == null) { throw new ArgumentNullException("headingVector"); }
            this.SetCurrentAnimations(new List<Tuple<string, IValueRead<RCNumVector>>>() { Tuple.Create(animationName, headingVector) });
        }

        /// <summary>
        /// Sets the current animations of this map object with the given heading vectors.
        /// </summary>
        /// <param name="animations">The names and the heading vectors of the animations to play.</param>
        public void SetCurrentAnimations(List<Tuple<string, IValueRead<RCNumVector>>> animations)
        {
            if (animations == null) { throw new ArgumentNullException("animations"); }

            this.currentAnimations.Clear();
            foreach (Tuple<string, IValueRead<RCNumVector>> animation in animations)
            {
                if (animation.Item1 == null) { throw new ArgumentException("Name of an animation cannot be null.", "animations"); }
                if (animation.Item2 == null) { throw new ArgumentException("Heading vector of an animation cannot be null.", "animations"); }

                this.currentAnimations.Add(
                    new AnimationPlayer(
                        this.owner.ElementType.AnimationPalette.GetAnimation(animation.Item1),
                        new MapDirValueSrcWrapper(new HeadingToMapDirConverter(animation.Item2))));
            }
        }

        #endregion Public interface

        #region ISearchTreeContent members

        /// <see cref="ISearchTreeContent.BoundingBox"/>
        public RCNumRectangle BoundingBox
        {
            get { return this.location; }
        }

        /// <see cref="ISearchTreeContent.BoundingBoxChanging"/>
        public event ContentBoundingBoxChangeHdl BoundingBoxChanging;

        /// <see cref="ISearchTreeContent.BoundingBoxChanged"/>
        public event ContentBoundingBoxChangeHdl BoundingBoxChanged;

        #endregion ISearchTreeContent members

        /// <summary>
        /// The location of this map object on the map.
        /// </summary>
        private RCNumRectangle location;

        /// <summary>
        /// Reference to the scenario element that owns this map object.
        /// </summary>
        private readonly ScenarioElement owner;

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
