using System.Linq;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Behaviors;
using RC.Engine.Simulator.Commands;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Core;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using RC.Engine.Simulator.MotionControl;
using RC.Common.Diagnostics;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Scenario elements that have activities on the map.
    /// </summary>
    public abstract class Entity : ScenarioElement
    {
        /// <summary>
        /// Constructs an entity instance.
        /// </summary>
        /// <param name="elementTypeName">The name of the element type of this entity.</param>
        /// <param name="isFlying">A flag indicating whether this entity is initially flying.</param>
        /// <param name="behaviors">The list of behaviors of this entity.</param>
        protected Entity(string elementTypeName, bool isFlying, params EntityBehavior[] behaviors) : base(elementTypeName)
        {
            if (behaviors == null) { throw new ArgumentNullException("behaviors"); }

            this.affectingCmdExecution = this.ConstructField<CmdExecutionBase>("affectingCmdExecution");
            this.locator = this.ConstructField<Locator>("locator");
            this.armour = this.ConstructField<Armour>("armour");
            this.biometrics = this.ConstructField<Biometrics>("biometrics");
            this.motionControl = this.ConstructField<MotionControl>("motionControl");
            this.activeProductionLine = this.ConstructField<ProductionLine>("activeProductionLine");
            this.nextProductionJobID = this.ConstructField<int>("nextProductionJobID");

            this.mapObject = null;
            this.reservationObject = null;
            this.affectingCmdExecution.Write(null);
            this.locator.Write(new Locator(this));
            this.armour.Write(new Armour(this));
            this.biometrics.Write(new Biometrics(this));
            this.motionControl.Write(new MotionControl(this, isFlying));
            this.behaviors = new RCSet<EntityBehavior>(behaviors);
            this.productionLines = new RCSet<ProductionLine>();
            this.activeProductionLine.Write(null);
            this.nextProductionJobID.Write(0);
        }

        #region Public interface

        /// <summary>
        /// Attaches this entity to the given quadratic tile on the map.
        /// </summary>
        /// <param name="topLeftTile">The quadratic tile at the top-left corner of this entity.</param>
        /// <returns>True if this entity was successfully attached to the map; otherwise false.</returns>
        /// <remarks>Note that the caller has to explicitly call MotionControl.Fix to fix this entity after calling this method.</remarks>
        public bool AttachToMap(IQuadTile topLeftTile)
        {
            ICell topLeftCell = topLeftTile.GetCell(new RCIntVector(0, 0));
            RCNumVector position = topLeftCell.MapCoords - new RCNumVector(1, 1) / 2 + this.ElementType.Area.Read() / 2;

            return this.AttachToMap(position);
        }

        /// <see cref="ScenarioElement.AttachToMap"/>
        public override bool AttachToMap(RCNumVector position)
        {
            if (position == RCNumVector.Undefined) { throw new ArgumentNullException("position"); }

            bool success = this.MotionControl.OnOwnerAttachingToMap(position);
            if (success)
            {
                this.mapObject = this.CreateMapObject(this.Area, this.MotionControl.IsFlying ? MapObjectLayerEnum.AirObjects : MapObjectLayerEnum.GroundObjects);
            }
            return success;
        }

        /// <see cref="ScenarioElement.DetachFromMap"/>
        public override RCNumVector DetachFromMap()
        {
            RCNumVector currentPosition = this.MotionControl.PositionVector.Read();
            if (currentPosition != RCNumVector.Undefined)
            {
                if (this.activeProductionLine.Read() != null) { this.activeProductionLine.Read().RemoveAllJobs(); }
                if (this.MotionControl.Status == MotionControlStatusEnum.Fixed) { this.MotionControl.Unfix(); }
                this.MotionControl.OnOwnerDetachedFromMap();
                this.DestroyMapObject(this.mapObject);
                this.mapObject = null;
                if (this.reservationObject != null)
                {
                    this.DestroyMapObject(this.reservationObject);
                    this.reservationObject = null;
                }
            }
            return currentPosition;
        }

        /// <summary>
        /// Checks whether the placement constraints of this entity allows it to be placed at the given quadratic position and
        /// collects all the violating quadratic coordinates relative to the given position.
        /// </summary>
        /// <param name="position">The position to be checked.</param>
        /// <param name="entitiesToIgnore">
        /// The list of entities to be ignored during the check. All entities in this list shall belong to the scenario of this entity.
        /// </param>
        /// <returns>
        /// The list of the quadratic coordinates (relative to the given position) violating the constraints of this entity.
        /// </returns>
        public RCSet<RCIntVector> CheckPlacementConstraints(RCIntVector position, RCSet<Entity> entitiesToIgnore)
        {
            RCSet<Entity> entitiesToIgnoreSet = new RCSet<Entity>(entitiesToIgnore) { this };
            return this.ElementType.CheckPlacementConstraints(this.Scenario, position, entitiesToIgnoreSet);
        }

        /// <summary>
        /// Checks whether this entity can overlap the given other entity.
        /// </summary>
        /// <param name="otherEntity">The other entity to be checked.</param>
        /// <returns>True if this entity can overlap the given other entity; otherwise false.</returns>
        /// <remarks>
        /// This method can be overriden by the derived classes. By default, overlap between entities is not enabled.
        /// </remarks>
        public virtual bool IsOverlapEnabled(Entity otherEntity) { return false; }

        /// <summary>
        /// Gets the type of the command that is currently being executed by this entity or null if there is no command currently
        /// being executed by this entity.
        /// </summary>
        public string CommandBeingExecuted
        {
            get
            {
                return this.affectingCmdExecution.Read() != null
                    ? this.affectingCmdExecution.Read().CommandBeingExecuted
                    : null;
            }
        }

        /// <summary>
        /// Gets the area of this entity.
        /// </summary>
        public RCNumRectangle Area { get { return this.CalculateArea(this.MotionControl.PositionVector.Read()); } }

        /// <summary>
        /// Gets the locator of this entity.
        /// </summary>
        public Locator Locator { get { return this.locator.Read(); } }

        /// <summary>
        /// Gets the armour of this entity.
        /// </summary>
        public Armour Armour { get { return this.armour.Read(); } }

        /// <summary>
        /// Gets the biometrics of this entity.
        /// </summary>
        public Biometrics Biometrics { get { return this.biometrics.Read(); } }

        /// <summary>
        /// Gets the motion control of this entity.
        /// </summary>
        public MotionControl MotionControl { get { return this.motionControl.Read(); } }

        /// <summary>
        /// Gets the map object that represents this entity on the map or null if this entity is not attached to the map.
        /// </summary>
        public MapObject MapObject { get { return this.mapObject; } }

        /// <summary>
        /// Gets the currently active production line or null if there is no active production line currently.
        /// </summary>
        public ProductionLine ActiveProductionLine { get { return this.activeProductionLine.Read(); } }

        #endregion Public interface

        #region Production management
        
        /// <summary>
        /// Check whether the given product is currently available.
        /// </summary>
        /// <param name="productName">The name of the product to check.</param>
        /// <returns>True if the given product is currently available; otherwise false.</returns>
        public bool IsProductAvailable(string productName)
        {
            if (productName == null) { throw new ArgumentNullException("productName"); }

            /// No product is available if this entity is under construction.
            if (this.biometrics.Read().IsUnderConstruction) { return false; }

            /// Check if the product is available on the active production line.
            if (this.activeProductionLine.Read() != null)
            {
                return this.activeProductionLine.Read().IsProductAvailable(productName);
            }

            /// If there is no active production line then we have to find a production line where the given product is available.
            return this.productionLines.Any(productionLine => productionLine.IsProductAvailable(productName));
        }

        /// <summary>
        /// Checks whether the given product is currently enabled.
        /// </summary>
        /// <param name="productName">The name of the product to check.</param>
        /// <returns>True if the given product is currently enabled; otherwise false.</returns>
        public bool IsProductEnabled(string productName)
        {
            if (productName == null) { throw new ArgumentNullException("productName"); }

            /// Check if the product is enabled on the active production line.
            if (this.activeProductionLine.Read() != null)
            {
                return this.activeProductionLine.Read().IsProductAvailable(productName) && this.activeProductionLine.Read().IsProductEnabled(productName);
            }

            /// If there is no active production line then we have to find a production line where the given product is enabled.
            return this.productionLines.Any(productionLine => productionLine.IsProductAvailable(productName) && productionLine.IsProductEnabled(productName));
        }

        /// <summary>
        /// Start producing the given product.
        /// </summary>
        /// <param name="productName">The name of the product to start producing.</param>
        public void StartProduction(string productName)
        {
            if (productName == null) { throw new ArgumentNullException("productName"); }

            /// If there is no active production line -> activate one.
            if (this.activeProductionLine.Read() == null)
            {
                foreach (ProductionLine line in this.productionLines)
                {
                    if (line.IsProductAvailable(productName))
                    {
                        this.activeProductionLine.Write(line);
                        break;
                    }
                }

                /// Check if we could activate a production line.
                if (this.activeProductionLine.Read() == null)
                {
                    throw new InvalidOperationException(string.Format("Product '{0}' is not available at any of the registered production lines!", productName));
                }
            }

            /// Enqueue the given product into the active production line and increment the production job ID in case of success.
            if (this.activeProductionLine.Read().EnqueueJob(productName, this.nextProductionJobID.Read()))
            {
                this.nextProductionJobID.Write(this.nextProductionJobID.Read() + 1);
            }

            /// Deactivate the production line if it remained empty.
            if (this.activeProductionLine.Read().ItemCount == 0)
            {
                this.activeProductionLine.Write(null);
            }
        }

        /// <summary>
        /// Cancels the given production job.
        /// </summary>
        /// <param name="productionJobID">The ID of the production job to be cancelled.</param>
        public void CancelProduction(int productionJobID)
        {
            if (this.activeProductionLine.Read() != null)
            {
                this.activeProductionLine.Read().RemoveJob(productionJobID);
                if (this.activeProductionLine.Read().ItemCount == 0)
                {
                    /// Deactivate the production line if there is no more job.
                    this.activeProductionLine.Write(null);
                }
            }
        }

        /// <summary>
        /// Registers the given production line to this entity.
        /// </summary>
        /// <param name="productionLine">The production line to be registered.</param>
        protected void RegisterProductionLine(ProductionLine productionLine)
        {
            if (productionLine == null) { throw new ArgumentNullException("productionLine"); }
            if (this.Scenario != null) { throw new InvalidOperationException("Production line cannot be registered while this entity is added to a scenario!"); }
            if (this.productionLines.Contains(productionLine)) { throw new InvalidOperationException("The given production line has already been registered!"); }

            this.productionLines.Add(productionLine);
        }

        /// <summary>
        /// Unregisters the given production line from this entity.
        /// </summary>
        /// <param name="productionLine">The production line to be unregistered.</param>
        protected void UnregisterProductionLine(ProductionLine productionLine)
        {
            if (productionLine == null) { throw new ArgumentNullException("productionLine"); }
            if (this.Scenario != null) { throw new InvalidOperationException("Production line cannot be unregistered while this entity is added to a scenario!"); }
            if (!this.productionLines.Contains(productionLine)) { throw new InvalidOperationException("The given production line has not yet been registered!"); }

            this.productionLines.Remove(productionLine);
        }

        #endregion Production management

        #region Overrides

        /// <summary>
        /// Gets the name of the destruction animation of this entity.
        /// </summary>
        /// <remarks>Can be overriden in the derived classes.</remarks>
        protected virtual string DestructionAnimationName { get { throw new NotSupportedException("Entity.DestructionAnimationName not supported for this entity!"); } }

        /// <summary>
        /// Derived classes can perform additional operations when being destroyed.
        /// </summary>
        protected virtual void OnDestroyingImpl() { }

        /// <see cref="ScenarioElement.UpdateStateImpl"/>
        protected sealed override void UpdateStateImpl()
        {
            /// Check if this entity is still alive.
            if (this.Biometrics.HP == 0)
            {
                this.OnDestroying();
                return;
            }

            /// Ask the behaviors to perform additional updates on this entity.
            foreach (EntityBehavior behavior in this.behaviors) { behavior.UpdateState(this); }

            /// Continue motion and attack.
            this.motionControl.Read().UpdateState();
            this.armour.Read().ContinueAttack();

            /// Continue production.
            if (this.activeProductionLine.Read() != null)
            {
                this.activeProductionLine.Read().ContinueProduction();
                if (this.activeProductionLine.Read().ItemCount == 0)
                {
                    this.activeProductionLine.Write(null);
                }
            }
        }

        /// <see cref="ScenarioElement.UpdateMapObjectsImpl"/>
        protected sealed override void UpdateMapObjectsImpl()
        {
            if (this.mapObject != null)
            {
                /// Ask the behaviors to update the map object of this entity.
                foreach (EntityBehavior behavior in this.behaviors) { behavior.UpdateMapObject(this); }
            }
        }

        /// <see cref="HeapedObject.DisposeImpl"/>
        protected override void DisposeImpl()
        {
            /// Unregister this entity from the command execution it is currently being affected.
            if (this.affectingCmdExecution.Read() != null)
            {
                this.affectingCmdExecution.Read().RemoveEntity(this);
                this.affectingCmdExecution.Write(null);
            }

            /// Dispose the production lines.
            this.activeProductionLine.Write(null);
            foreach (ProductionLine productionLine in this.productionLines) { productionLine.Dispose(); }
            this.productionLines.Clear();

            /// Dispose the other components of this entity.
            this.locator.Read().Dispose();
            this.locator.Write(null);
            this.armour.Read().Dispose();
            this.armour.Write(null);
            this.biometrics.Read().Dispose();
            this.biometrics.Write(null);
            this.motionControl.Read().Dispose();
            this.motionControl.Write(null);
        }

        #endregion Overrides

        #region Internal members
        
        /// <summary>
        /// Reserves a position on the ground for this entity.
        /// </summary>
        /// <param name="positionToReserve">The position to be reserved.</param>
        /// <remarks>This method is used by the MotionControl of this entity.</remarks>
        internal void ReservePositionOnGround(RCNumVector positionToReserve)
        {
            if (positionToReserve == RCNumVector.Undefined) { throw new ArgumentNullException("positionToReserve"); }
            if (this.reservationObject != null) { throw new InvalidOperationException("A position has already been reserved for this entity!"); }

            this.reservationObject = this.CreateMapObject(this.CalculateArea(positionToReserve), MapObjectLayerEnum.GroundReservations);
        }

        /// <summary>
        /// Reserves a position in the air for this entity.
        /// </summary>
        /// <param name="positionToReserve">The position to be reserved.</param>
        /// <remarks>This method is used by the MotionControl of this entity.</remarks>
        internal void ReservePositionInAir(RCNumVector positionToReserve)
        {
            if (positionToReserve == RCNumVector.Undefined) { throw new ArgumentNullException("positionToReserve"); }
            if (this.reservationObject != null) { throw new InvalidOperationException("A position has already been reserved for this entity!"); }

            this.reservationObject = this.CreateMapObject(this.CalculateArea(positionToReserve), MapObjectLayerEnum.AirReservations);
        }

        /// <summary>
        /// Removes the reservation of this entity.
        /// </summary>
        /// <remarks>This method is used by the MotionControl of this entity.</remarks>
        internal void RemoveReservation()
        {
            if (this.reservationObject == null) { throw new InvalidOperationException("There is no position currently reserved for this entity!"); }

            this.DestroyMapObject(this.reservationObject);
            this.reservationObject = null;
        }

        /// <summary>
        /// Fixes this entity in its current quadratic position.
        /// </summary>
        /// <remarks>This method is used by the MotionControl of this entity.</remarks>
        internal void OnFixed()
        {
            //TraceManager.WriteAllTrace(string.Format("OnFixed(): position = {0}; location = {1}; quadposition = {2}", this.MotionControl.PositionVector.Read(), this.MapObject.Location, this.MapObject.QuadraticPosition), TraceFilters.INFO);
            for (int col = this.MapObject.QuadraticPosition.Left; col < this.MapObject.QuadraticPosition.Right; col++)
            {
                for (int row = this.MapObject.QuadraticPosition.Top; row < this.MapObject.QuadraticPosition.Bottom; row++)
                {
                    if (this.MapContext.FixedEntities[col, row] != null) { throw new InvalidOperationException(string.Format("Another entity is already fixed to quadratic tile at ({0};{1})!", col, row)); }
                    this.MapContext.FixedEntities[col, row] = this;
                }
            }
        }

        /// <summary>
        /// Unfixes this entity from its current quadratic position.
        /// </summary>
        /// <remarks>This method is used by the MotionControl of this entity.</remarks>
        internal void OnUnfixed()
        {
            //TraceManager.WriteAllTrace(string.Format("OnUnfixed(): position = {0}; location = {1}; quadposition = {2}", this.MotionControl.PositionVector.Read(), this.MapObject.Location, this.MapObject.QuadraticPosition), TraceFilters.INFO);
            for (int col = this.MapObject.QuadraticPosition.Left; col < this.MapObject.QuadraticPosition.Right; col++)
            {
                for (int row = this.MapObject.QuadraticPosition.Top; row < this.MapObject.QuadraticPosition.Bottom; row++)
                {
                    if (this.MapContext.FixedEntities[col, row] != this) { throw new InvalidOperationException(string.Format("Entity is not fixed to quadratic tile at ({0};{1})!", col, row)); }
                    this.MapContext.FixedEntities[col, row] = null;
                }
            }
        }

        /// <summary>
        /// This method is called when a command execution starts to affecting this entity.
        /// </summary>
        /// <param name="cmdExecution">The command execution.</param>
        internal void OnCommandExecutionStarted(CmdExecutionBase cmdExecution)
        {
            if (this.Scenario == null) { throw new InvalidOperationException("The entity doesn't not belong to a scenario!"); }
            if (this.affectingCmdExecution.Read() != null)
            {
                /// Unregister this entity from the command execution it is currently being affected.
                this.affectingCmdExecution.Read().RemoveEntity(this);
            }
            this.affectingCmdExecution.Write(cmdExecution);
        }

        /// <summary>
        /// This method is called when the currently affecting command execution stops affecting this entity.
        /// </summary>
        internal void OnCommandExecutionStopped()
        {
            if (this.Scenario == null) { throw new InvalidOperationException("The entity doesn't not belong to a scenario!"); }
            if (this.affectingCmdExecution.Read() == null) { throw new InvalidOperationException("The entity is not being affected by any command executions!"); }
            this.affectingCmdExecution.Write(null);
        }

        /// <summary>
        /// This method is called when this entity is being destroyed.
        /// </summary>
        private void OnDestroying()
        {
            this.OnDestroyingImpl();

            if (this.activeProductionLine.Read() != null) { this.activeProductionLine.Read().RemoveAllJobs(); }

            EntityWreck wreck = new EntityWreck(this, this.DestructionAnimationName);
            this.Scenario.AddElementToScenario(wreck);
            wreck.AttachToMap(this.MotionControl.PositionVector.Read());

            if (this.Owner != null) { this.Owner.RemoveEntity(this); }
            this.DetachFromMap();
            this.Scenario.RemoveElementFromScenario(this);
        }

        #endregion Internal members

        #region Heaped members

        /// <summary>
        /// Reference to the command execution that is affecting this entity or null if this entity is not affected by
        /// any command execution.
        /// </summary>
        private readonly HeapedValue<CmdExecutionBase> affectingCmdExecution;

        /// <summary>
        /// Reference to the locator of this entity.
        /// </summary>
        private readonly HeapedValue<Locator> locator;

        /// <summary>
        /// Reference to the armour of this entity.
        /// </summary>
        private readonly HeapedValue<Armour> armour;

        /// <summary>
        /// Reference to the biometrics of this entity.
        /// </summary>
        private readonly HeapedValue<Biometrics> biometrics;

        /// <summary>
        /// Reference to the motion control of this entity.
        /// </summary>
        private readonly HeapedValue<MotionControl> motionControl;

        /// <summary>
        /// The ID of the next production job.
        /// </summary>
        private readonly HeapedValue<int> nextProductionJobID;

        #endregion Heaped members

        /// <summary>
        /// The behaviors of this entity.
        /// </summary>
        /// TODO: store these objects also in a HeapedArray!
        private readonly RCSet<EntityBehavior> behaviors;

        /// <summary>
        /// The registered production lines of this entity.
        /// </summary>
        /// TODO: store these objects also in a HeapedArray!
        private readonly RCSet<ProductionLine> productionLines;

        /// <summary>
        /// Reference to the active production line or null if there is no active production line currently.
        /// </summary>
        private readonly HeapedValue<ProductionLine> activeProductionLine;

        /// <summary>
        /// Reference to the map object that represents this entity on the map or null if this entity is not currently
        /// attached to the map.
        /// </summary>
        private MapObject mapObject;

        /// <summary>
        /// Reference to the map object that reserves an area for this entity on the map during takeoff and landing operations.
        /// </summary>
        private MapObject reservationObject;
    }
}
