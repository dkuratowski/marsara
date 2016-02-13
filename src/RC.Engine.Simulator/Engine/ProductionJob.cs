using System;
using RC.Common.ComponentModel;
using RC.Common.Diagnostics;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Core;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Represents a job in a production line.
    /// </summary>
    public abstract class ProductionJob : HeapedObject
    {
        #region Public interface

        /// <summary>
        /// Gets the product being produced by this job.
        /// </summary>
        public IScenarioElementType Product { get { return this.product; } }

        /// <summary>
        /// Gets the ID of this job.
        /// </summary>
        public int ID { get { return this.jobID.Read(); } }

        /// <summary>
        /// Gets whether this job has been started or not.
        /// </summary>
        public bool IsStarted { get { return this.progress.Read() != -1; } }

        /// <summary>
        /// Gets whether this job has been finished or not.
        /// </summary>
        public bool IsFinished { get { return this.progress.Read() == this.product.BuildTime.Read(); }}

        /// <summary>
        /// Gets the progress of this job.
        /// </summary>
        public int Progress
        {
            get
            {
                if (!this.IsStarted) { throw new InvalidOperationException("This production job has not yet been started!"); }
                return this.progress.Read();
            }
        }

        /// <summary>
        /// Locks the necessary resources for this job.
        /// </summary>
        /// <returns>True if the necessary resources could be locked successfully; otherwise false.</returns>
        public bool LockResources()
        {
            if (this.IsStarted) { throw new InvalidOperationException("This production job has already been started!"); }

            int mineralsNeeded = this.product.MineralCost != null ? this.product.MineralCost.Read() : 0;
            int vespeneGasNeeded = this.product.GasCost != null ? this.product.GasCost.Read() : 0;

            /// Take the necessary amount of minerals and vespene gas from the owner player. (TODO: send error message if not enough resources!)
            if (this.ownerPlayer.Read().TakeResources(mineralsNeeded, vespeneGasNeeded))
            {
                this.lockedMinerals.Write(mineralsNeeded);
                this.lockedVespeneGas.Write(vespeneGasNeeded);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Locks the necessary supplies and starts this job.
        /// </summary>
        /// <returns>True if the job has been started successfully; otherwise false.</returns>
        public bool Start()
        {
            if (this.IsStarted) { throw new InvalidOperationException("This production job has already been started!"); }

            /// Lock the necessary supplies of the owner player to start this job. (TODO: send error message if not enough supply!)
            int supplyNeeded = this.product.SupplyUsed != null ? this.product.SupplyUsed.Read() : 0;
            if (!this.ownerPlayer.Read().LockSupply(supplyNeeded)) { return false; }
            this.lockedSupplies.Write(supplyNeeded);

            /// Execute the optional additional operation of the derived class.
            if (!this.StartImpl()) { return false; }

            /// Start the progress of this job.
            this.progress.Write(0);
            return true;
        }

        /// <summary>
        /// Continues this job.
        /// </summary>
        /// <returns>True if this job has finished working; otherwise false.</returns>
        public bool Continue()
        {
            if (!this.IsStarted) { throw new InvalidOperationException("This production job has not yet been started!"); }
            if (this.IsFinished) { throw new InvalidOperationException("This production job has already been finished!"); }

            /// Execute the optional continue operation of the derived class.
            if (!this.ContinueImpl())
            {
                /// This job cannot be continued for some reason -> minerals and vespene gas taken from the player will be lost.
                this.lockedMinerals.Write(0);
                this.lockedVespeneGas.Write(0);
                return true;
            }

            /// Check if the progress reached the build time of the product being produced.
            this.progress.Write(this.progress.Read() + 1);
            if (this.progress.Read() == this.product.BuildTime.Read())
            {
                /// Current job finished -> create the product using the factory component.
                TraceManager.WriteAllTrace(string.Format("Production of '{0}' completed.", this.product.Name), TraceFilters.INFO);
                if (this.CompleteImpl())
                {
                    /// Job completed successfully -> resources spent.
                    this.lockedMinerals.Write(0);
                    this.lockedVespeneGas.Write(0);
                }

                return true;
            }
            return false;
        }

        #endregion Public interface

        #region Protected members

        /// <summary>
        /// Constructs a new ProductionJob instance with the given ID for the given product.
        /// </summary>
        /// <param name="ownerPlayer">The owner player of this job.</param>
        /// <param name="product">The product to be created by this job.</param>
        /// <param name="jobID">The ID of this job.</param>
        protected ProductionJob(Player ownerPlayer, IScenarioElementType product, int jobID)
        {
            if (ownerPlayer == null) { throw new ArgumentNullException("ownerPlayer"); }
            if (product == null) { throw new ArgumentNullException("product"); }
            if (jobID < 0) { throw new ArgumentOutOfRangeException("jobID", "Job identifier cannot be negative!"); }

            this.elementFactory = ComponentManager.GetInterface<IElementFactory>();

            this.ownerPlayer = this.ConstructField<Player>("ownerPlayer");
            this.jobID = this.ConstructField<int>("jobID");
            this.progress = this.ConstructField<int>("progress");
            this.lockedMinerals = this.ConstructField<int>("lockedMinerals");
            this.lockedVespeneGas = this.ConstructField<int>("lockedVespeneGas");
            this.lockedSupplies = this.ConstructField<int>("lockedSupplies");
            this.ownerPlayer.Write(ownerPlayer);
            this.jobID.Write(jobID);
            this.progress.Write(-1);
            this.lockedMinerals.Write(0);
            this.lockedVespeneGas.Write(0);
            this.lockedSupplies.Write(0);
            this.product = product;
        }

        /// <see cref="HeapedObject.DisposeImpl"/>
        protected override void DisposeImpl()
        {
            if (this.IsStarted && this.progress.Read() < this.product.BuildTime.Read())
            {
                /// If this job is currently running but not yet finished -> abort it.
                this.AbortImpl(this.lockedMinerals.Read(), this.lockedVespeneGas.Read(), this.lockedSupplies.Read());
                this.lockedMinerals.Write(0);
                this.lockedVespeneGas.Write(0);
                this.lockedSupplies.Write(0);
                this.progress.Write(-1);
            }
            else
            {
                /// Otherwise give back the locked resources and supply to the owner player.
                this.ownerPlayer.Read().GiveResources(this.lockedMinerals.Read(), this.lockedVespeneGas.Read());
                this.ownerPlayer.Read().UnlockSupply(this.lockedSupplies.Read());
            }
        }

        /// <summary>
        /// Gets the element factory component.
        /// </summary>
        protected IElementFactory ElementFactory { get { return this.elementFactory; } }

        /// <summary>
        /// Gets the player that this production job belongs to.
        /// </summary>
        protected Player OwnerPlayer { get { return this.ownerPlayer.Read(); } }

        #endregion Protected members

        #region Overridables

        /// <summary>
        /// By overriding this method the derived classes can perform additional operations when this job is being started.
        /// </summary>
        /// <returns>True if this job could be started successfully; otherwise false.</returns>
        protected virtual bool StartImpl() { return true; }

        /// <summary>
        /// By overriding this method the derived classes can perform additional operations when this job is being continued.
        /// </summary>
        /// <returns>True if this job can continue its execution; otherwise false.</returns>
        protected virtual bool ContinueImpl() { return true; }

        /// <summary>
        /// By overriding this method the derived classes can perform additional operations when this job is being aborted.
        /// </summary>
        /// <param name="lockedMinerals">The amount of locked minerals.</param>
        /// <param name="lockedVespeneGas">The amount of locked vespene gas.</param>
        /// <param name="lockedSupplies">The amount of locked supplies.</param>
        /// <remarks>The default implementation gives back all the locked resources to the player of the owner entity.</remarks>
        protected virtual void AbortImpl(int lockedMinerals, int lockedVespeneGas, int lockedSupplies)
        {
            /// Give back the locked resources and supply to the owner player.  
            this.ownerPlayer.Read().GiveResources(lockedMinerals, lockedVespeneGas);
            this.ownerPlayer.Read().UnlockSupply(lockedSupplies);
        }

        /// <summary>
        /// By overriding this method the derived classes can perform additional operations when this job is being completed.
        /// </summary>
        /// <returns>True if completion was successful; otherwise false.</returns>
        protected virtual bool CompleteImpl() { return true; }

        #endregion Overridables

        /// <summary>
        /// The product created by this job.
        /// </summary>
        private readonly IScenarioElementType product;

        /// <summary>
        /// Reference to the player of the owner that this production job belongs to.
        /// </summary>
        private readonly HeapedValue<Player> ownerPlayer;

        /// <summary>
        /// The ID of this job.
        /// </summary>
        private readonly HeapedValue<int> jobID;

        /// <summary>
        /// The progress of this job or -1 if it has not yet been started.
        /// </summary>
        private readonly HeapedValue<int> progress;

        /// <summary>
        /// The amount of locked resources.
        /// </summary>
        private readonly HeapedValue<int> lockedMinerals;
        private readonly HeapedValue<int> lockedVespeneGas;
        private readonly HeapedValue<int> lockedSupplies;

        /// <summary>
        /// Reference to the element factory component.
        /// </summary>
        private readonly IElementFactory elementFactory;
    }
}
