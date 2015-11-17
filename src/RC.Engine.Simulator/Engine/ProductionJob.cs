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

            /// TODO: Check if the player of the owner entity has enough minerals and vespene gas to create this job (send error message & return false if not)!
            /// TODO: Remove the necessary amount of minerals and vespene has from the player of the owner entity!
            this.lockedMinerals.Write(this.product.MineralCost != null ? this.product.MineralCost.Read() : 0);
            this.lockedVespeneGas.Write(this.product.GasCost != null ? this.product.GasCost.Read() : 0);
            return true;
        }

        /// <summary>
        /// Locks the necessary supplies and starts this job.
        /// </summary>
        /// <returns>True if the job has been started successfully; otherwise false.</returns>
        public bool Start()
        {
            if (this.IsStarted) { throw new InvalidOperationException("This production job has already been started!"); }

            /// TODO: Check if the player of the owner entity has enough supplies to start this job (send error message & return false if not)!

            /// Execute the optional additional operation of the derived class.
            if (!this.StartImpl()) { return false; }

            this.lockedSupplies.Write(this.product.FoodCost != null ? this.product.FoodCost.Read() : 0);
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

            /// Increment the progress and execute the optional additional operation of the derived class.
            this.progress.Write(this.progress.Read() + 1);
            if (this.ContinueImpl())
            {
                this.lockedMinerals.Write(0);
                this.lockedVespeneGas.Write(0);
                this.lockedSupplies.Write(0);
                this.progress.Write(-1);
                return true;
            }

            /// Check if the progress reached the build time of the product being produced.
            if (this.progress.Read() >= this.product.BuildTime.Read())
            {
                /// Current job finished -> create the product using the factory component.
                TraceManager.WriteAllTrace(string.Format("Production of '{0}' completed.", this.product.Name), TraceFilters.INFO);
                if (!this.CompleteImpl())
                {
                    /// TODO: if completion was unsuccessful, give back the locked minerals, vespene gas and supply to the player!
                }

                this.lockedMinerals.Write(0);
                this.lockedVespeneGas.Write(0);
                this.lockedSupplies.Write(0);
                this.progress.Write(-1);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Aborts this job.
        /// </summary>
        public void Abort()
        {
            this.AbortImpl();

            this.lockedMinerals.Write(0);
            this.lockedVespeneGas.Write(0);
            this.lockedSupplies.Write(0);
            this.progress.Write(-1);
        }

        #endregion Public interface

        #region Protected members

        /// <summary>
        /// Constructs a new ProductionJob instance with the given ID for the given product.
        /// </summary>
        /// <param name="owner">The owner entity of this job.</param>
        /// <param name="product">The product to be created by this job.</param>
        /// <param name="jobID">The ID of this job.</param>
        protected ProductionJob(Entity owner, IScenarioElementType product, int jobID)
        {
            if (owner == null) { throw new ArgumentNullException("owner"); }
            if (product == null) { throw new ArgumentNullException("product"); }
            if (jobID < 0) { throw new ArgumentOutOfRangeException("jobID", "Job identifier cannot be negative!"); }

            this.elementFactory = ComponentManager.GetInterface<IElementFactory>();
            
            this.owner = this.ConstructField<Entity>("owner");
            this.jobID = this.ConstructField<int>("jobID");
            this.progress = this.ConstructField<int>("progress");
            this.lockedMinerals = this.ConstructField<int>("lockedMinerals");
            this.lockedVespeneGas = this.ConstructField<int>("lockedVespeneGas");
            this.lockedSupplies = this.ConstructField<int>("lockedSupplies");
            this.owner.Write(owner);
            this.jobID.Write(jobID);
            this.progress.Write(-1);
            this.lockedMinerals.Write(0);
            this.lockedVespeneGas.Write(0);
            this.lockedSupplies.Write(0);
            this.product = product;
        }

        /// <summary>
        /// Gets the element factory component.
        /// </summary>
        protected IElementFactory ElementFactory { get { return this.elementFactory; } }

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
        /// <returns>True if this job has finished working; otherwise false.</returns>
        protected virtual bool ContinueImpl() { return false; }

        /// <summary>
        /// By overriding this method the derived classes can perform additional operations when this job is being aborted.
        /// </summary>
        /// <remarks>The default implementation gives back all the locked resources to the player of the owner entity.</remarks>
        protected virtual void AbortImpl()
        {
            /// TODO: Give back the locked resources and supply to the player of the owner entity!
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
        /// The owner entity of this job.
        /// </summary>
        private readonly HeapedValue<Entity> owner;

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
