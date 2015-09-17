using System.Linq;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Commands;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Core;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using RC.Engine.Simulator.MotionControl;

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
        /// <param name="behaviors">The list of behaviors of this entity.</param>
        protected Entity(string elementTypeName, params EntityBehavior[] behaviors) : base(elementTypeName)
        {
            if (behaviors == null) { throw new ArgumentNullException("behaviors"); }

            this.isFlying = this.ConstructField<byte>("isFlying");
            this.affectingCmdExecution = this.ConstructField<CmdExecutionBase>("affectingCmdExecution");
            this.locator = this.ConstructField<Locator>("locator");
            this.armour = this.ConstructField<Armour>("armour");
            this.biometrics = this.ConstructField<Biometrics>("biometrics");
            this.motionControl = this.ConstructField<MotionControl>("motionControl");
            this.activeProductionLine = this.ConstructField<ProductionLine>("activeProductionLine");

            this.mapObject = null;
            this.isFlying.Write(0x00);
            this.affectingCmdExecution.Write(null);
            this.locator.Write(new Locator(this));
            this.armour.Write(new Armour(this));
            this.biometrics.Write(new Biometrics(this));
            this.motionControl.Write(new MotionControl(this));
            this.behaviors = new RCSet<EntityBehavior>(behaviors);
            this.productionLines = new RCSet<ProductionLine>();
            this.activeProductionLine.Write(null);
        }

        #region Public interface

        /// <see cref="ScenarioElement.AttachToMap"/>
        public override bool AttachToMap(RCNumVector position)
        {
            if (position == RCNumVector.Undefined) { throw new ArgumentNullException("position"); }

            bool success = this.MotionControl.SetPosition(position);
            if (success)
            {
                this.MotionControl.PositionVector.ValueChanged += this.OnPositionChanged;
                this.mapObject = this.CreateMapObject(this.Area);
            }
            return success;
        }

        /// <see cref="ScenarioElement.DetachFromMap"/>
        public override RCNumVector DetachFromMap()
        {
            RCNumVector currentPosition = this.MotionControl.PositionVector.Read();
            if (currentPosition != RCNumVector.Undefined)
            {
                this.MotionControl.PositionVector.ValueChanged -= this.OnPositionChanged;
                this.MotionControl.SetPosition(RCNumVector.Undefined);
                this.DestroyMapObject(this.mapObject);
                this.mapObject = null;
            }
            return currentPosition;
        }

        /// <summary>
        /// Gets whether this entity is currently flying or not.
        /// </summary>
        public bool IsFlying
        {
            get
            {
                return this.isFlying.Read() != 0x00;
            }
        }

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
        public bool CheckProductAvailability(string productName)
        {
            if (productName == null) { throw new ArgumentNullException("productName"); }

            if (this.activeProductionLine.Read() != null)
            {
                /// First we check if the active production line has reached its capacity or not.
                if (this.activeProductionLine.Read().ItemCount == this.activeProductionLine.Read().Capacity) { return false; }

                /// Then we check if the given product is available at the active production line.
                IScenarioElementType product = this.activeProductionLine.Read().GetProduct(productName);
                return product != null;
            }

            /// If there is no active production line then we have to find a production line where the given product is available.
            return this.productionLines.Any(productionLine => productionLine.GetProduct(productName) != null);
        }

        /// <summary>
        /// Checks whether the given product is currently enabled.
        /// </summary>
        /// <param name="productName">The name of the product to check.</param>
        /// <returns>True if the given product is currently enabled; otherwise false.</returns>
        public bool IsProductionEnabled(string productName)
        {
            if (productName == null) { throw new ArgumentNullException("productName"); }

            /// Search for the product.
            IScenarioElementType product = null;
            foreach (ProductionLine productionLine in this.productionLines)
            {
                product = productionLine.GetProduct(productName);
                if (product != null) { break; }
            }
            if (product == null) { throw new InvalidOperationException(string.Format("Product '{0}' is not available at any of the registered production lines!", productName)); }

            /// Execute custom check.
            if (!this.IsProductionEnabledImpl(product)) { return false; }

            /// Check the requirements of the product.
            foreach (IRequirement requirement in product.Requirements)
            {
                if (!this.Owner.HasEntity(requirement.RequiredBuildingType.Name)) { return false; }
                if (requirement.RequiredAddonType != null && !this.Owner.HasEntity(requirement.RequiredAddonType.Name)) { return false; }
            }

            /// All the requirements were satisfied.
            return true;
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
                    if (line.GetProduct(productName) != null)
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

            /// Start producing the given product with the active production line.
            this.activeProductionLine.Read().StartProduction(productName);
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
        /// The derived classes can implement additional checks whether the given product is currently enabled.
        /// </summary>
        /// <param name="product">The product to be checked.</param>
        /// <returns>True if the given product is currently enabled; otherwise false.</returns>
        /// <remarks>This method can be overriden in the derived classes. The default implementation does nothing.</remarks>
        protected virtual bool IsProductionEnabledImpl(IScenarioElementType product) { return true; }

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
            /// Ask the behaviors to update the map object of this entity.
            foreach (EntityBehavior behavior in this.behaviors) { behavior.UpdateMapObject(this); }
        }

        /// <see cref="HeapedObject.DisposeImpl"/>
        protected override void DisposeImpl()
        {
            if (this.affectingCmdExecution.Read() != null)
            {
                /// Unregister this entity from the command execution it is currently being affected.
                this.affectingCmdExecution.Read().RemoveEntity(this);
                this.affectingCmdExecution.Write(null);
            }

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
        /// The method is called when the position of the motion control has been changed.
        /// </summary>
        private void OnPositionChanged(object sender, EventArgs args)
        {
            RCNumVector newPos = this.MotionControl.PositionVector.Read();
            if (newPos == RCNumVector.Undefined) { throw new InvalidOperationException("Undefined position!"); }

            if (this.mapObject != null)
            {
                this.mapObject.SetLocation(this.Area);
            }
        }

        /// <summary>
        /// This method is called when this entity is being destroyed.
        /// </summary>
        private void OnDestroying()
        {
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
        /// This flag indicates whether the entity is on the ground (0x00) or is flying (any other value).
        /// </summary>
        private readonly HeapedValue<byte> isFlying; 

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
    }
}
