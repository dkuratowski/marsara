using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.MotionControl;
using RC.Engine.Simulator.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.MotionControl.Test
{
    /// <summary>
    /// Represents a test entity.
    /// </summary>
    class TestEntity : IEntityActuator, ISearchTreeContent
    {
        /// <summary>
        /// Constructs a TestEntity instance.
        /// </summary>
        /// <param name="startPosition">The initial position of the test entity.</param>
        /// <param name="size">The size of the test entity.</param>
        /// <param name="entities">The map content manager that stores the entities.</param>
        public TestEntity(RCNumVector startPosition, RCNumVector size, ISearchTree<TestEntity> entities)
        {
            this.currentPosition = startPosition;
            this.size = size;
            this.currentSpeed = 0;
            this.goal = this.currentPosition;
            this.currentDirection = MapDirection.North;
            this.controller = new MotionController(this);
            this.admissableVelocities = new List<Tuple<RCNumber, MapDirection>>();
            this.selectedVelocity = null;
            this.entities = entities;
        }

        /// <summary>
        /// Updates the velocity of this entity.
        /// </summary>
        public void UpdateVelocity()
        {
            if (this.currentSpeed != 0 || !this.CurrentPosition.Contains(this.goal))
            {
                this.CalculateAdmissibleVelocities();
                this.controller.UpdateVelocity();
            }
        }

        /// <summary>
        /// Updates the velocity of this entity.
        /// </summary>
        public void UpdatePosition()
        {
            if (this.selectedVelocity != null)
            {
                this.currentSpeed = this.selectedVelocity.Item1;
                this.currentDirection = this.selectedVelocity.Item2;
                this.selectedVelocity = null;
                
                RCNumRectangle newPosition = new RCNumRectangle(this.currentPosition + this.CurrentVelocity - this.size / 2, this.size);
                foreach (TestEntity collideWith in this.entities.GetContents(newPosition))
                {
                    if (collideWith != this)
                    {
                        this.currentSpeed = 0;
                        this.currentDirection = MapDirection.North;
                        return;
                    }
                }

                if (this.BoundingBoxChanging != null) { this.BoundingBoxChanging(this); }
                this.currentPosition += this.CurrentVelocity;
                if (this.BoundingBoxChanged != null) { this.BoundingBoxChanged(this); }
            }
        }

        /// <summary>
        /// Sets a new goal to this entity.
        /// </summary>
        /// <param name="newGoal">The new goal of this entity.</param>
        public void SetGoal(RCNumVector newGoal)
        {
            this.goal = newGoal;
        }

        #region IEntityActuator methods

        /// <see cref="IEntityActuator.CurrentPosition"/>
        public RCNumRectangle CurrentPosition
        {
            get { return new RCNumRectangle(this.currentPosition - this.size / 2, this.size); }
        }

        /// <see cref="IEntityActuator.CurrentVelocity"/>
        public RCNumVector CurrentVelocity
        {
            get { return UNIT_VECTORS[(int)this.currentDirection] * this.currentSpeed; }
        }

        /// <see cref="IEntityActuator.PreferredVelocity"/>
        public RCNumVector PreferredVelocity
        {
            get { return this.goal - this.currentPosition; }
        }

        /// <see cref="IEntityActuator.SetVelocity"/>
        public void SetVelocity(int selectedVelocityIndex)
        {
            this.selectedVelocity = this.admissableVelocities[selectedVelocityIndex];
            this.admissableVelocities.Clear();
        }

        /// <see cref="IEntityActuator.GetAdmissibleVelocities"/>
        public List<RCNumVector> GetAdmissibleVelocities()
        {
            List<RCNumVector> retList = new List<RCNumVector>();
            foreach (Tuple<RCNumber, MapDirection> item in this.admissableVelocities)
            {
                retList.Add(UNIT_VECTORS[(int)item.Item2] * item.Item1);
            }
            return retList;
        }

        /// <see cref="IEntityActuator.GetDynamicObstacles"/>
        public List<DynamicObstacleInfo> GetDynamicObstacles()
        {
            List<DynamicObstacleInfo> retList = new List<DynamicObstacleInfo>();
            foreach (TestEntity entityInRange in this.entities.GetContents(new RCNumRectangle(this.currentPosition - new RCNumVector(SIGHT_RANGE, SIGHT_RANGE),
                                                                                              new RCNumVector(SIGHT_RANGE, SIGHT_RANGE) * 2)))
            {
                if (entityInRange != this)
                {
                    retList.Add(new DynamicObstacleInfo() { Position = entityInRange.CurrentPosition, Velocity = entityInRange.CurrentVelocity });
                }
            }
            return retList;
        }

        #endregion IEntityActuator methods

        #region ISearchTreeContent methods

        /// <see cref="ISearchTreeContent.BoundingBox"/>
        public RCNumRectangle BoundingBox
        {
            get { return new RCNumRectangle(this.currentPosition - this.size / 2, this.size); }
        }

        /// <see cref="ISearchTreeContent.BoundingBoxChanging"/>
        public event ContentBoundingBoxChangeHdl BoundingBoxChanging;

        /// <see cref="ISearchTreeContent.BoundingBoxChanged"/>
        public event ContentBoundingBoxChangeHdl BoundingBoxChanged;

        #endregion ISearchTreeContent methods

        /// <summary>
        /// Calculates the currently admissible velocities.
        /// </summary>
        private void CalculateAdmissibleVelocities()
        {
            this.admissableVelocities.Clear();
            if (this.currentSpeed != 0)
            {
                RCNumber increasedSpd = this.currentSpeed + ACCELERATION < MAX_SPEED ? this.currentSpeed + ACCELERATION : MAX_SPEED;
                RCNumber decreasedSpd = this.currentSpeed - ACCELERATION > 0 ? this.currentSpeed - ACCELERATION : 0;
                this.admissableVelocities.Add(new Tuple<RCNumber, MapDirection>(increasedSpd, (MapDirection)(((int)this.currentDirection + 1) % 8)));
                this.admissableVelocities.Add(new Tuple<RCNumber, MapDirection>(increasedSpd, this.currentDirection));
                this.admissableVelocities.Add(new Tuple<RCNumber, MapDirection>(increasedSpd, (MapDirection)(((int)this.currentDirection + 7) % 8)));
                this.admissableVelocities.Add(new Tuple<RCNumber, MapDirection>(this.currentSpeed, (MapDirection)(((int)this.currentDirection + 1) % 8)));
                this.admissableVelocities.Add(new Tuple<RCNumber, MapDirection>(this.currentSpeed, this.currentDirection));
                this.admissableVelocities.Add(new Tuple<RCNumber, MapDirection>(this.currentSpeed, (MapDirection)(((int)this.currentDirection + 7) % 8)));
                if (decreasedSpd != 0)
                {
                    this.admissableVelocities.Add(new Tuple<RCNumber, MapDirection>(decreasedSpd, (MapDirection)(((int)this.currentDirection + 1) % 8)));
                    this.admissableVelocities.Add(new Tuple<RCNumber, MapDirection>(decreasedSpd, this.currentDirection));
                    this.admissableVelocities.Add(new Tuple<RCNumber, MapDirection>(decreasedSpd, (MapDirection)(((int)this.currentDirection + 7) % 8)));
                }
                else
                {
                    this.admissableVelocities.Add(new Tuple<RCNumber, MapDirection>(0, MapDirection.North));
                }
            }
            else
            {
                this.admissableVelocities.Add(new Tuple<RCNumber, MapDirection>(0, MapDirection.North));
                this.admissableVelocities.Add(new Tuple<RCNumber, MapDirection>(ACCELERATION, MapDirection.North));
                this.admissableVelocities.Add(new Tuple<RCNumber, MapDirection>(ACCELERATION, MapDirection.NorthEast));
                this.admissableVelocities.Add(new Tuple<RCNumber, MapDirection>(ACCELERATION, MapDirection.East));
                this.admissableVelocities.Add(new Tuple<RCNumber, MapDirection>(ACCELERATION, MapDirection.SouthEast));
                this.admissableVelocities.Add(new Tuple<RCNumber, MapDirection>(ACCELERATION, MapDirection.South));
                this.admissableVelocities.Add(new Tuple<RCNumber, MapDirection>(ACCELERATION, MapDirection.SouthWest));
                this.admissableVelocities.Add(new Tuple<RCNumber, MapDirection>(ACCELERATION, MapDirection.West));
                this.admissableVelocities.Add(new Tuple<RCNumber, MapDirection>(ACCELERATION, MapDirection.NorthWest));
            }
        }

        /// <summary>
        /// The goal of this entity.
        /// </summary>
        private RCNumVector goal;

        /// <summary>
        /// The current position of this entity.
        /// </summary>
        private RCNumVector currentPosition;

        /// <summary>
        /// The current speed of this entity.
        /// </summary>
        private RCNumber currentSpeed;

        /// <summary>
        /// The current moving direction of this entity.
        /// </summary>
        private MapDirection currentDirection;

        /// <summary>
        /// The size of this entity.
        /// </summary>
        private RCNumVector size;

        /// <summary>
        /// Reference to the controller of this entity.
        /// </summary>
        private MotionController controller;

        /// <summary>
        /// Ordered list of the admissable velocities in the next velocity update.
        /// </summary>
        private List<Tuple<RCNumber, MapDirection>> admissableVelocities;

        /// <summary>
        /// The velocity selected by the controller or null if no new velocity was selected.
        /// </summary>
        private Tuple<RCNumber, MapDirection> selectedVelocity;

        /// <summary>
        /// The map content manager that stores the test entities.
        /// </summary>
        private ISearchTree<TestEntity> entities;

        /// <summary>
        /// The acceleration of the test entities.
        /// </summary>
        private static readonly RCNumber ACCELERATION = (RCNumber)10 / (RCNumber)100;

        /// <summary>
        /// The maximum speed of the test entities.
        /// </summary>
        private static readonly RCNumber MAX_SPEED = 1;

        /// <summary>
        /// The sight range of the test entities.
        /// </summary>
        private static readonly RCNumber SIGHT_RANGE = 10;

        /// <summary>
        /// List of the unit vectors mapped by their directions.
        /// </summary>
        private static readonly RCNumVector[] UNIT_VECTORS = new RCNumVector[8]
        {
            new RCNumVector(0, -1),
            new RCNumVector((RCNumber)1414 / (RCNumber)2000, (-1)*((RCNumber)1414 / (RCNumber)2000)),
            new RCNumVector(1, 0),
            new RCNumVector((RCNumber)1414 / (RCNumber)2000, (RCNumber)1414 / (RCNumber)2000),
            new RCNumVector(0, 1),
            new RCNumVector((-1)*((RCNumber)1414 / (RCNumber)2000), (RCNumber)1414 / (RCNumber)2000),
            new RCNumVector(-1, 0),
            new RCNumVector((-1)*((RCNumber)1414 / (RCNumber)2000), (-1)*((RCNumber)1414 / (RCNumber)2000))
        };
    }
}
