using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Common.Diagnostics;
using RC.Engine.Simulator.ComponentInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// PROTOTYPE CODE
    /// Represents a game object on the map.
    /// </summary>
    class GameObject : IGameObject, IElementOfSimulation
    {
        /// <summary>
        /// Constructs a game object.
        /// </summary>
        /// <param name="position">The position of this game object on the map.</param>
        /// <param name="maxSpeed">The maximum speed of this object.</param>
        /// <param name="pathFinder">Reference to the pathfinder component.</param>
        public GameObject(RCNumRectangle position, RCNumber maxSpeed, IPathFinder pathFinder)
        {
            if (position == RCNumRectangle.Undefined) { throw new ArgumentNullException("position"); }
            if (pathFinder == null) { throw new ArgumentNullException("pathFinder"); }

            this.position = position;
            this.maxSpeed = maxSpeed;
            this.pathFinder = pathFinder;
            this.targetPosition = RCNumVector.Undefined;
            this.id = RandomService.DefaultGenerator.Next(10000);
            this.currentPath = null;
            this.currentSectionIdx = -1;
        }

        #region IMapContent members

        /// <see cref="IMapContent.Position"/>
        public RCNumRectangle Position { get { return this.position; } }

        /// <see cref="IMapContent.PositionChanging"/>
        public event MapContentPropertyChangeHdl PositionChanging;

        /// <see cref="IMapContent.PositionChanged"/>
        public event MapContentPropertyChangeHdl PositionChanged;

        #endregion IMapContent members

        #region IGameObject members

        /// <see cref="IGameObject.SendCommand"/>
        public void SendCommand(RCNumVector targetPoint)
        {
            TraceManager.WriteAllTrace(string.Format("SendCommand {0}: {1}", this.id, targetPoint), TraceFilters.INFO);

            this.targetPosition = targetPoint;
            this.currentPath = null;
            this.currentSectionIdx = -1;

        }

        /// <see cref="IGameObject.IsStopped"/>
        public bool IsStopped { get { return this.currentPath == null; } }

        #endregion IGameObject members

        #region IElementOfSimulation members

        /// <see cref="IElementOfSimulation.Update"/>
        public void Update()
        {
            /// Do nothing if we have no target.
            if (this.targetPosition == RCNumVector.Undefined) { return; }

            /// If we have target but no path, search a new path to the target.
            if (this.currentPath == null)
            {
                this.currentPath = this.pathFinder.FindPath(new RCIntVector(((this.position.Left + this.position.Right) / 2).Round(), ((this.position.Top + this.position.Bottom) / 2).Round()),
                                                            new RCIntVector(this.targetPosition.X.Round(), this.targetPosition.Y.Round()),
                                                            this.position.Size);
                this.currentSectionIdx = 0;
            }

            /// Find the best next position for this game object.
            RCNumRectangle bestNextPos = RCNumRectangle.Undefined;
            RCNumber bestUtilityVal = -1;
            foreach (RCNumRectangle pos in this.ComputePossibleNextPositions())
            {
                RCNumber utilityVal = this.ComputeUtilityValue(pos);
                if (utilityVal != -1 && (bestUtilityVal == -1 || utilityVal < bestUtilityVal))
                {
                    bestUtilityVal = utilityVal;
                    bestNextPos = pos;
                }
            }

            /// Move to the new position.
            if (bestNextPos != RCNumRectangle.Undefined)
            {
                if (this.PositionChanging != null) { this.PositionChanging(this); }
                this.position = bestNextPos;
                if (this.PositionChanged != null) { this.PositionChanged(this); }

                /// Update state.
                if (this.currentSectionIdx < this.currentPath.Length - 1 &&
                    RCNumRectangle.Intersect(this.position, this.currentPath[this.currentSectionIdx + 1]) != RCNumRectangle.Undefined)
                {
                    this.currentSectionIdx++;
                }
                else if (this.currentSectionIdx == this.currentPath.Length - 1 &&
                         this.position.Contains(this.targetPosition.X, this.targetPosition.Y))
                {
                    TraceManager.WriteAllTrace(string.Format("TargetReached {0}: {1}", this.id, this.targetPosition), TraceFilters.INFO);
                    this.currentPath = null;
                    this.currentSectionIdx = -1;
                    this.targetPosition = RCNumVector.Undefined;
                }
            }
        }

        #endregion IElementOfSimulation members

        #region Internal movement methods

        /// <summary>
        /// Computes the list of the possible next positions of this game object.
        /// </summary>
        /// <returns>The list of the possible next positions of this game object.</returns>
        private List<RCNumRectangle> ComputePossibleNextPositions()
        {
            return new List<RCNumRectangle>()
            {
                this.position + new RCNumVector(0, -this.maxSpeed),
                this.position + new RCNumVector(this.maxSpeed, -this.maxSpeed),
                this.position + new RCNumVector(this.maxSpeed, 0),
                this.position + new RCNumVector(this.maxSpeed, this.maxSpeed),
                this.position + new RCNumVector(0, this.maxSpeed),
                this.position + new RCNumVector(-this.maxSpeed, this.maxSpeed),
                this.position + new RCNumVector(-this.maxSpeed, 0),
                this.position + new RCNumVector(-this.maxSpeed, -this.maxSpeed)
            };
        }

        /// <summary>
        /// Computes the utility value of the given position.
        /// </summary>
        /// <param name="position">The position for which to compute the utility value.</param>
        /// <returns>The utility value for the given position or -1 if it is not possible to move to the given position.</returns>
        /// TODO: this is a dummy implementation!
        private RCNumber ComputeUtilityValue(RCNumRectangle position)
        {
            if (this.pathFinder.CheckObstacleIntersection(position)) { return -1; }

            RCNumVector posCenter = new RCNumVector((position.Left + position.Right) / 2, (position.Top + position.Bottom) / 2);
            if (this.currentSectionIdx < this.currentPath.Length - 1)
            {
                RCNumRectangle nextSection = this.currentPath[this.currentSectionIdx + 1];
                if (RCNumRectangle.Intersect(position, this.currentPath[this.currentSectionIdx]) == RCNumRectangle.Undefined &&
                    RCNumRectangle.Intersect(position, nextSection) == RCNumRectangle.Undefined)
                {
                    return -1;
                }

                RCNumVector nextSectionCenter = new RCNumVector((nextSection.Left + nextSection.Right) / 2, (nextSection.Top + nextSection.Bottom) / 2);
                return MapUtils.ComputeDistance(posCenter, nextSectionCenter);
            }
            else if (this.currentSectionIdx == this.currentPath.Length - 1)
            {
                return MapUtils.ComputeDistance(posCenter, this.targetPosition);
            }
            else
            {
                return -1;
            }
        }

        #endregion Internal movement methods

        /// <summary>
        /// Reference to the pathfinder component.
        /// </summary>
        private IPathFinder pathFinder;

        /// <summary>
        /// The position of this game object.
        /// </summary>
        private RCNumRectangle position;

        /// <summary>
        /// The maximum velocity of this game object.
        /// </summary>
        private RCNumber maxSpeed;

        /// <summary>
        /// The ID of this game object.
        /// TODO: only for debugging.
        /// </summary>
        private int id;

        /// <summary>
        /// The target position of this game object or RCNumVector.Undefined if there is no move command currently being executed.
        /// </summary>
        private RCNumVector targetPosition;

        /// <summary>
        /// Reference to the path currently being followed or null if there is no path being followed.
        /// </summary>
        private IPath currentPath;

        /// <summary>
        /// The index of the current section along the followed path or -1 if there is no path being followed.
        /// </summary>
        private int currentSectionIdx;
    }
}
