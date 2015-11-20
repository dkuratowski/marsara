using System;
using System.Collections.Generic;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Represents a production line of an entity.
    /// </summary>
    public abstract class ProductionLine : HeapedObject
    {
        #region Public interface

        /// <summary>
        /// Gets the capacity of this production line.
        /// </summary>
        public int Capacity { get { return this.jobs.Length; } }

        /// <summary>
        /// Gets the number of items currently in this production line.
        /// </summary>
        public int ItemCount { get { return this.itemCount.Read(); } }

        /// <summary>
        /// Gets the progress of the currently running production job in this production line.
        /// </summary>
        /// <exception cref="InvalidOperationException">If this production line is inactive.</exception>
        public int Progress
        {
            get
            {
                if (this.itemCount.Read() == 0) { throw new InvalidOperationException("This production line is inactive!"); }
                ProductionJob currentJob = this.jobs[this.CalculatePhysicalIndex(0)].Read();
                return currentJob.IsStarted ? currentJob.Progress : 0;
            }
        }

        /// <summary>
        /// Gets the product being produced by the job in this production line at the given index or null if there is no job at the given index.
        /// The currently running production job is always at index 0.
        /// </summary>
        /// <param name="index">The index of the job.</param>
        /// <returns>The product being produced by the job at the given index or null if there is no job at the given index.</returns>
        /// <exception cref="InvalidOperationException">If this production line is inactive.</exception>
        public IScenarioElementType GetProduct(int index)
        {
            if (this.itemCount.Read() == 0) { throw new InvalidOperationException("This production line is inactive!"); }
            if (index < 0 || index >= this.Capacity) { throw new ArgumentOutOfRangeException("index"); }
            if (index >= this.itemCount.Read()) { return null; }

            return this.jobs[this.CalculatePhysicalIndex(index)].Read().Product;
        }

        /// <summary>
        /// Gets the ID of the job in this production line at the given index or -1 if there is no job at the given index.
        /// </summary>
        /// <param name="index">The index of the job.</param>
        /// <returns>The ID of the job at the given index or -1 if there is no job at the given index.</returns>
        /// <exception cref="InvalidOperationException">If this production line is inactive.</exception>
        public int GetJobID(int index)
        {
            if (this.itemCount.Read() == 0) { throw new InvalidOperationException("This production line is inactive!"); }
            if (index < 0 || index >= this.Capacity) { throw new ArgumentOutOfRangeException("index"); }
            if (index >= this.itemCount.Read()) { return -1; }

            return this.jobs[this.CalculatePhysicalIndex(index)].Read().ID;
        }

        /// <summary>
        /// Checks whether the given product is currently available at this production line.
        /// </summary>
        /// <param name="productName">The name of the product to check.</param>
        /// <returns>True if the given product is currently available at this production line; otherwise false.</returns>
        public bool IsProductAvailable(string productName)
        {
            if (productName == null) { throw new ArgumentNullException("productName"); }

            /// First we check if this production line has reached its capacity or not.
            if (this.itemCount.Read() == this.Capacity) { return false; }

            /// Then we check if the given product is available at this production line.
            if (!this.products.ContainsKey(productName)) { return false; }

            /// Check the additional requirements of the derived class if exists.
            return this.IsProductAvailableImpl(productName);
        }

        /// <summary>
        /// Checks whether the given product is currently enabled at this production line.
        /// </summary>
        /// <param name="productName">The name of the product to check.</param>
        /// <returns>True if the given product is currently available at this production line; otherwise false.</returns>
        /// <exception cref="InvalidOperationException">If the given product is not available at this production line.</exception>
        public bool IsProductEnabled(string productName)
        {
            if (productName == null) { throw new ArgumentNullException("productName"); }
            if (!this.IsProductAvailable(productName)) { throw new InvalidOperationException(string.Format("Product '{0}' is not available at this production line!", productName)); }

            /// Check the requirements of the product.
            foreach (IRequirement requirement in this.products[productName].Requirements)
            {
                if (!this.owner.Read().Owner.HasBuilding(requirement.RequiredBuildingType.Name)) { return false; }
                if (requirement.RequiredAddonType != null && !this.owner.Read().Owner.HasAddon(requirement.RequiredAddonType.Name)) { return false; }
            }

            /// Check the additional requirements of the derived class if exists.
            return this.IsProductEnabledImpl(productName);
        }

        /// <summary>
        /// Puts the given job into the end of the production line.
        /// </summary>
        /// <param name="productName">The name of the product to enqueue.</param>
        /// <param name="jobID">The ID of the job to enqueue.</param>
        /// <returns>True if enqueing the job was successful; otherwise false.</returns>
        public bool EnqueueJob(string productName, int jobID)
        {
            if (productName == null) { throw new ArgumentNullException("productName"); }
            if (jobID < 0) { throw new ArgumentOutOfRangeException("jobID", "Production job ID must be non-negative!"); }
            if (!this.products.ContainsKey(productName)) { throw new InvalidOperationException(string.Format("Product '{0}' cannot be produced at this production line!", productName)); }
            if (this.itemCount.Read() == this.Capacity) { throw new InvalidOperationException("This production line has reached its capacity!"); }

            /// Create a job and try to lock the resources of the owner player.
            ProductionJob job = this.CreateJob(productName, jobID);
            if (!job.LockResources())
            {
                /// Unable to lock the necessary resources -> abort the job and cancel.
                job.Abort();
                job.Dispose();
                return false;
            }

            /// If there is no other job in the line, try to start the created job.
            if (this.itemCount.Read() == 0)
            {
                if (!job.Start())
                {
                    /// Unable to start the job -> abort the job and cancel.
                    job.Abort();
                    job.Dispose();
                    return false;
                }
            }

            /// Job created -> enqueue it.
            this.jobs[this.CalculatePhysicalIndex(this.itemCount.Read())].Write(job);
            this.itemCount.Write(this.itemCount.Read() + 1);
            return true;
        }

        /// <summary>
        /// Removes the given job from the production line.
        /// </summary>
        /// <param name="jobID">The ID of the job to remove.</param>
        /// <remarks>If there is no job with the given ID then this function has no effect.</remarks>
        public void RemoveJob(int jobID)
        {
            if (jobID < 0) { throw new ArgumentOutOfRangeException("jobID", "Production job ID must be non-negative!"); }

            int indexOfRemovedJob = -1;
            for (int index = 0; index < this.itemCount.Read(); index++)
            {
                int physicalIndex = this.CalculatePhysicalIndex(index);
                if (indexOfRemovedJob == -1 && this.jobs[physicalIndex].Read().ID == jobID)
                {
                    /// Job found -> abort and remove it from the line.
                    this.jobs[physicalIndex].Read().Abort();
                    this.jobs[physicalIndex].Read().Dispose();
                    this.jobs[physicalIndex].Write(null);
                    indexOfRemovedJob = index;
                }

                /// Replace the current item with the next item if the job has already been removed.
                if (indexOfRemovedJob != -1 && index < this.itemCount.Read() - 1)
                {
                    int physicalIndexOfNext = this.CalculatePhysicalIndex(index + 1);
                    this.jobs[physicalIndex].Write(this.jobs[physicalIndexOfNext].Read());
                    this.jobs[physicalIndexOfNext].Write(null);
                }
            }

            /// If the job has been removed, decrement the itemCount.
            if (indexOfRemovedJob != -1) { this.itemCount.Write(this.itemCount.Read() - 1); }

            /// If the first job has been removed, try to start the next job.
            if (indexOfRemovedJob == 0 && this.itemCount.Read() > 0)
            {
                ProductionJob nextJob = this.jobs[this.CalculatePhysicalIndex(0)].Read();
                nextJob.Start();
            }
        }

        /// <summary>
        /// Continues the current production job.
        /// </summary>
        public void ContinueProduction()
        {
            if (this.itemCount.Read() == 0) { throw new InvalidOperationException("This production line is inactive!"); }

            ProductionJob currentJob = this.jobs[this.CalculatePhysicalIndex(0)].Read();
            if (currentJob.IsStarted)
            {
                /// Current job has been started -> continue it.
                if (currentJob.Continue())
                {
                    /// Current job finished working -> remove it from the line.
                    currentJob.Dispose();
                    this.jobs[this.CalculatePhysicalIndex(0)].Write(null);
                    this.startIndex.Write((this.startIndex.Read() + 1) % this.Capacity);
                    this.itemCount.Write(this.itemCount.Read() - 1);

                    /// If we still have jobs in the line, try to start the next one.
                    if (this.itemCount.Read() > 0)
                    {
                        ProductionJob nextJob = this.jobs[this.CalculatePhysicalIndex(0)].Read();
                        nextJob.Start();
                    }
                }
            }
            else
            {
                /// Try to start the current job.
                currentJob.Start();
            }
        }

        #endregion Public interface

        #region Protected members

        /// <summary>
        /// Constructs a production line with the given capacity for the given type of products.
        /// </summary>
        /// <param name="owner">The owner of this production line.</param>
        /// <param name="capacity">The capacity of this production line.</param>
        /// <param name="products">The type of products that can be produced by this production line.</param>
        protected ProductionLine(Entity owner, int capacity, List<IScenarioElementType> products)
        {
            if (owner == null) { throw new ArgumentNullException("owner"); }
            if (capacity <= 0) { throw new ArgumentOutOfRangeException("capacity", "Capacity of a production line shall be greater than 0!"); }
            if (products == null) { throw new ArgumentNullException("products"); }
            if (products.Count == 0) { throw new ArgumentException("Production line cannot be created with empty product list!", "products"); }

            this.products = new Dictionary<string, IScenarioElementType>();
            foreach (IScenarioElementType product in products) { this.products.Add(product.Name, product); }

            this.jobs = this.ConstructArrayField<ProductionJob>("jobs");
            this.owner = this.ConstructField<Entity>("owner");
            this.startIndex = this.ConstructField<int>("startIndex");
            this.itemCount = this.ConstructField<int>("itemCount");

            this.jobs.New(capacity);
            this.owner.Write(owner);
            this.startIndex.Write(0);
            this.itemCount.Write(0);
        }

        /// <see cref="HeapedObject.DisposeImpl"/>
        protected override void DisposeImpl()
        {
            foreach (IValue<ProductionJob> job in this.jobs)
            {
                job.Read().Dispose();
                job.Write(null);
            }
        }

        /// <summary>
        /// Gets the owner of this production line.
        /// </summary>
        protected Entity Owner { get { return this.owner.Read(); } }

        #endregion Protected members

        #region Overridables

        /// <summary>
        /// Creates a job for producing the given product.
        /// </summary>
        /// <param name="productName">The name of the product to be produced by the job.</param>
        /// <param name="jobID">The ID of the job.</param>
        /// <returns>The created job.</returns>
        protected abstract ProductionJob CreateJob(string productName, int jobID);

        /// <summary>
        /// By overriding this method the derived classes can check additional requirements whether the given product is currently available
        /// at this production line.
        /// </summary>
        /// <param name="productName">The name of the product to check.</param>
        /// <returns>True if the given product is currently available at this production line; otherwise false.</returns>
        protected virtual bool IsProductAvailableImpl(string productName) { return true; }

        /// <summary>
        /// By overriding this method the derived classes can check additional requirements whether the given product is currently enabled
        /// at this production line.
        /// </summary>
        /// <param name="productName">The name of the product to check.</param>
        /// <returns>True if the given product is currently enabled at this production line; otherwise false.</returns>
        protected virtual bool IsProductEnabledImpl(string productName) { return true; }

        #endregion Overridables

        /// <summary>
        /// Calculates the physical index from the given logical index.
        /// </summary>
        /// <param name="logicalIndex">The logical index to calculate from.</param>
        /// <returns>The calculated physical index.</returns>
        private int CalculatePhysicalIndex(int logicalIndex)
        {
            return (this.startIndex.Read() + logicalIndex) % this.Capacity;
        }

        /// <summary>
        /// The list that contains the jobs in this production line.
        /// </summary>
        private readonly HeapedArray<ProductionJob> jobs;

        /// <summary>
        /// Reference to the owner of this production line.
        /// </summary>
        private readonly HeapedValue<Entity> owner;

        /// <summary>
        /// The index of the beginning of this production line.
        /// </summary>
        private readonly HeapedValue<int> startIndex;

        /// <summary>
        /// The number of items currently in this production line.
        /// </summary>
        private readonly HeapedValue<int> itemCount;

        /// <summary>
        /// The list of the products that can be produced by this production line mapped by their names.
        /// </summary>
        private readonly Dictionary<string, IScenarioElementType> products;
    }
}
