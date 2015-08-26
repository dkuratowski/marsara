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
    public class MapObject : ISearchTreeContent, IDisposable
    {
        /// <summary>
        /// Constructs a MapObject instance.
        /// </summary>
        /// <param name="owner">Reference to the scenario element that owns this map object.</param>
        public MapObject(ScenarioElement owner)
        {
            if (owner == null) { throw new ArgumentNullException("owner"); }
            if (owner.ElementType.AnimationPalette == null) { throw new ArgumentException("The type of the given scenario element has no animation palette defined!", "owner"); }

            this.currentAnimations = new AnimationPlayer[owner.ElementType.AnimationPalette.Count];
            this.quadraticPositionCache = new CachedValue<RCIntRectangle>(() =>
            {
                if (this.location == RCNumRectangle.Undefined) { return RCIntRectangle.Undefined; }
                RCIntVector topLeft = this.location.Location.Round();
                RCIntVector bottomRight = (this.location.Location + this.location.Size).Round();
                RCIntRectangle cellRect = new RCIntRectangle(topLeft.X, topLeft.Y, Math.Max(1, bottomRight.X - topLeft.X), Math.Max(1, bottomRight.Y - topLeft.Y));
                return this.owner.Scenario.Map.CellToQuadRect(cellRect);
            });

            this.owner = owner;
            this.location = RCNumRectangle.Undefined;
        }

        #region Public interface

        /// <summary>
        /// Gets the players of the currently active animations of this map object.
        /// </summary>
        public IEnumerable<AnimationPlayer> CurrentAnimations
        {
            get
            {
                if (this.owner == null) { throw new ObjectDisposedException("MapObject"); }
                return this.currentAnimations.Where(animation => animation != null);
            }
        }

        /// <summary>
        /// Gets the quadratic position of this map object or RCIntRectangle.Undefined if this map object is not attached to the map.
        /// </summary>
        public RCIntRectangle QuadraticPosition
        {
            get
            {
                if (this.owner == null) { throw new ObjectDisposedException("MapObject"); }
                return this.quadraticPositionCache.Value;
            }
        }

        /// <summary>
        /// Gets the reference to the scenario element that owns this map object.
        /// </summary>
        public ScenarioElement Owner
        {
            get
            {
                if (this.owner == null) { throw new ObjectDisposedException("MapObject"); }
                return this.owner;
            }
        }

        /// <summary>
        /// Gets whether this map object is destroyed or not.
        /// </summary>
        public bool IsDestroyed { get { return this.owner == null; } }

        /// <summary>
        /// Gets whether this map object has any animations currently being played.
        /// </summary>
        public bool HasAnyAnimations { get { return this.currentAnimations.Any(anim => anim != null); } }

        /// <summary>
        /// Sets the location of this map object.
        /// </summary>
        /// <param name="newLocation">The new location of this map object.</param>
        public void SetLocation(RCNumRectangle newLocation)
        {
            if (this.owner == null) { throw new ObjectDisposedException("MapObject"); }
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
        /// Starts playing the given animation of this map object with the given heading vector if it is not being played currently.
        /// </summary>
        /// <param name="animationName">The name of the animation to start playing.</param>
        /// <param name="headingVectors">The heading vectors in priority order.</param>
        /// <remarks>If the given animation is already being played then this function has no effect.</remarks>
        public void StartAnimation(string animationName, params IValueRead<RCNumVector>[] headingVectors)
        {
            if (this.owner == null) { throw new ObjectDisposedException("MapObject"); }
            if (animationName == null) { throw new ArgumentNullException("animationName"); }
            if (headingVectors == null) { throw new ArgumentNullException("headingVectors"); }

            Animation animation = this.owner.ElementType.AnimationPalette.GetAnimation(animationName);
            if (this.currentAnimations[animation.LayerIndex] == null)
            {
                this.currentAnimations[animation.LayerIndex] =
                    new AnimationPlayer(animation, new MapDirValueSrcWrapper(new HeadingToMapDirConverter(headingVectors)));
            }
        }

        /// <summary>
        /// Restarts playing the given animation of this map object with the given heading vector.
        /// </summary>
        /// <param name="animationName">The name of the animation to restart playing.</param>
        /// <param name="headingVectors">The heading vectors in priority order.</param>
        /// <remarks>If the given animation is already being played then it will be restarted.</remarks>
        public void RestartAnimation(string animationName, params IValueRead<RCNumVector>[] headingVectors)
        {
            if (this.owner == null) { throw new ObjectDisposedException("MapObject"); }
            if (animationName == null) { throw new ArgumentNullException("animationName"); }
            if (headingVectors == null) { throw new ArgumentNullException("headingVectors"); }

            Animation animation = this.owner.ElementType.AnimationPalette.GetAnimation(animationName);
            this.currentAnimations[animation.LayerIndex] =
                new AnimationPlayer(animation, new MapDirValueSrcWrapper(new HeadingToMapDirConverter(headingVectors)));
        }

        /// <summary>
        /// Stops playing the given animation of this map object.
        /// </summary>
        /// <param name="animationName">The name of the animation to stop playing.</param>
        /// <remarks>If the given animation is not being played, then this function has no effect.</remarks>
        public void StopAnimation(string animationName)
        {
            if (this.owner == null) { throw new ObjectDisposedException("MapObject"); }
            if (animationName == null) { throw new ArgumentNullException("animationName"); }

            Animation animation = this.owner.ElementType.AnimationPalette.GetAnimation(animationName);
            this.currentAnimations[animation.LayerIndex] = null;
        }

        /// <summary>
        /// Stops playing all animations of this map object.
        /// </summary>
        public void StopAllAnimations()
        {
            if (this.owner == null) { throw new ObjectDisposedException("MapObject"); }

            for (int animIndex = 0; animIndex < this.currentAnimations.Length; animIndex++)
            {
                this.currentAnimations[animIndex] = null;
            }
        }

        #endregion Public interface

        #region IDisposable members

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose() { this.owner = null; }

        #endregion IDisposable members

        #region ISearchTreeContent members

        /// <see cref="ISearchTreeContent.BoundingBox"/>
        public RCNumRectangle BoundingBox
        {
            get
            {
                if (this.owner == null) { throw new ObjectDisposedException("MapObject"); }
                return this.location;
            }
        }

        /// <see cref="ISearchTreeContent.BoundingBoxChanging"/>
        public event ContentBoundingBoxChangeHdl BoundingBoxChanging;

        /// <see cref="ISearchTreeContent.BoundingBoxChanged"/>
        public event ContentBoundingBoxChangeHdl BoundingBoxChanged;

        #endregion ISearchTreeContent members

        #region Internal methods

        /// <summary>
        /// Steps the current animations of this map object.
        /// </summary>
        internal void StepAnimations()
        {
            for (int animIndex = 0; animIndex < this.currentAnimations.Length; animIndex++)
            {
                if (this.currentAnimations[animIndex] != null)
                {
                    if (this.currentAnimations[animIndex].IsFinished) { this.currentAnimations[animIndex] = null; }
                    else { this.currentAnimations[animIndex].Step(); }
                }
            }
        }

        #endregion Internal methods

        /// <summary>
        /// The location of this map object on the map.
        /// </summary>
        private RCNumRectangle location;

        /// <summary>
        /// Reference to the scenario element that owns this map object.
        /// </summary>
        private ScenarioElement owner;

        /// <summary>
        /// The players of the currently active animations of this map object for each render layer. If there is no active animation
        /// for a render layer, then the corresponding item in this array is null.
        /// </summary>
        private readonly AnimationPlayer[] currentAnimations;

        /// <summary>
        /// The cached value of the quadratic position of this map object.
        /// </summary>
        private CachedValue<RCIntRectangle> quadraticPositionCache;
    }
}
