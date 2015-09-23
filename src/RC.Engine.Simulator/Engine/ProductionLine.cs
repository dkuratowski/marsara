using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.Common.Diagnostics;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Core;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Represents a production line of an entity.
    /// </summary>
    public class ProductionLine : HeapedObject
    {
        /// <summary>
        /// Constructs a production line with the given capacity for the given type of products.
        /// </summary>
        /// <param name="owner">The owner of this production line.</param>
        /// <param name="capacity">The capacity of this production line.</param>
        /// <param name="products">The type of products that can be produced by this production line.</param>
        public ProductionLine(Entity owner, int capacity, List<IScenarioElementType> products)
        {
            if (owner == null) { throw new ArgumentNullException("owner"); }
            if (capacity <= 0) { throw new ArgumentOutOfRangeException("capacity", "Capacity of a production line shall be greater than 0!"); }
            if (products == null) { throw new ArgumentNullException("products"); }
            if (products.Count == 0) { throw new ArgumentException("Production line cannot be created with empty product list!", "products"); }

            this.entityFactory = ComponentManager.GetInterface<IEntityFactory>();

            this.products = new Dictionary<string, IScenarioElementType>();
            foreach (IScenarioElementType product in products) { this.products.Add(product.Name, product); }

            this.jobs = new List<IScenarioElementType>(capacity);
            this.jobIDs = new List<int>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                this.jobs.Add(null);
                this.jobIDs.Add(-1);
            }

            this.owner = this.ConstructField<Entity>("owner");
            this.startIndex = this.ConstructField<int>("startIndex");
            this.itemCount = this.ConstructField<int>("itemCount");
            this.progress = this.ConstructField<int>("progress");
            this.startIndex.Write(0);
            this.itemCount.Write(0);
            this.progress.Write(-1);
            this.owner.Write(owner);
        }

        /// <summary>
        /// Gets the capacity of this production line.
        /// </summary>
        public int Capacity { get { return this.jobs.Count; } }

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
                return this.progress.Read() != -1 ? this.progress.Read() : 0;
            }
        }

        /// <summary>
        /// Gets the job in this production line at the given index or null if there is no item at the given index.
        /// The currently running production job is always at index 0.
        /// </summary>
        /// <param name="index">The index of the job to get.</param>
        /// <returns>The job at the given index or null if there is no item at the given index.</returns>
        /// <exception cref="InvalidOperationException">If this production line is inactive.</exception>
        public IScenarioElementType GetProductionJob(int index)
        {
            if (this.itemCount.Read() == 0) { throw new InvalidOperationException("This production line is inactive!"); }
            if (index < 0 || index >= this.Capacity) { throw new ArgumentOutOfRangeException("index"); }
            if (index >= this.itemCount.Read()) { return null; }

            return this.jobs[this.CalculatePhysicalIndex(index)];
        }

        /// <summary>
        /// Gets the ID of the job in this production line at the given index or -1 if there is no item at the given index.
        /// </summary>
        /// <param name="index">The index of the job to get.</param>
        /// <returns>The ID of the job at the given index or -1 if there is no item at the given index.</returns>
        /// <exception cref="InvalidOperationException">If this production line is inactive.</exception>
        public int GetProductionJobID(int index)
        {
            if (this.itemCount.Read() == 0) { throw new InvalidOperationException("This production line is inactive!"); }
            if (index < 0 || index >= this.Capacity) { throw new ArgumentOutOfRangeException("index"); }
            if (index >= this.itemCount.Read()) { return -1; }

            return this.jobIDs[(this.startIndex.Read() + index) % this.Capacity];
        }

        /// <summary>
        /// Gets the given product if it is available at this production line or null if not.
        /// </summary>
        /// <param name="productName">The name of the product.</param>
        /// <returns>The given product if it is available at this production line or null if not.</returns>
        public IScenarioElementType GetProduct(string productName)
        {
            if (productName == null) { throw new ArgumentNullException("productName"); }
            return this.products.ContainsKey(productName) ? this.products[productName] : null;
        }

        /// <summary>
        /// Puts the given job into the end of the production line.
        /// </summary>
        /// <param name="productName">The name of the product to enqueue.</param>
        /// <param name="jobID">The ID of the job to enqueue.</param>
        public void EnqueueJob(string productName, int jobID)
        {
            if (productName == null) { throw new ArgumentNullException("productName"); }
            if (jobID < 0) { throw new ArgumentOutOfRangeException("jobID", "Production job ID must be non-negative!"); }
            if (!this.products.ContainsKey(productName)) { throw new InvalidOperationException(string.Format("Product '{0}' cannot be produced at this production line!", productName)); }
            if (this.itemCount.Read() == this.Capacity) { throw new InvalidOperationException("This production line has reached its capacity!"); }

            IScenarioElementType product = this.products[productName];
            /// TODO: Check if the player has enough minerals and vespene gas to enqueue the product (send error message & return if not)!
            /// TODO: Remove the necessary amount of minerals and vespene has from the player!

            if (this.itemCount.Read() == 0)
            {
                /// If there is no other product in the line, then check if the player has enough supply to start the production.
                /// TODO: implement the check, send error message & return if not!
            }

            /// Mineral & vespene gas check OK -> we can enqueue the product.
            int indexOfNewItem = this.CalculatePhysicalIndex(this.itemCount.Read());
            this.jobs[indexOfNewItem] = this.products[productName];
            this.jobIDs[indexOfNewItem] = jobID;
            if (this.itemCount.Read() == 0) { this.progress.Write(0); } /// Start the production if there is no other product.
            this.itemCount.Write(this.itemCount.Read() + 1);
        }

        /// <summary>
        /// Removes the given job from the production line.
        /// </summary>
        /// <param name="jobID">The ID of the job to remove.</param>
        /// <remarks>If there is no job with the given ID then this function has no effect.</remarks>
        public void RemoveJob(int jobID)
        {
            if (jobID < 0) { throw new ArgumentOutOfRangeException("jobID", "Production job ID must be non-negative!"); }

            TraceManager.WriteAllTrace(string.Format("Removing job '{0}'!", jobID), TraceFilters.INFO);

            int indexOfRemovedJob = -1;
            for (int index = 0; index < this.itemCount.Read(); index++)
            {
                int physicalIndex = this.CalculatePhysicalIndex(index);
                if (indexOfRemovedJob == -1 && this.jobIDs[physicalIndex] == jobID)
                {
                    /// Job found and has to be removed.
                    if (index == 0 && this.progress.Read() != -1)
                    {
                        /// The production job has already been started.
                        /// TODO: give back the locked supply to the player!
                    }

                    /// TODO: given back the locked minerals and vespene gas to the player!
                    indexOfRemovedJob = index;
                }

                /// Replace the current item with the next item if the job has already been removed.
                if (indexOfRemovedJob != -1 && index < this.itemCount.Read() - 1)
                {
                    int physicalIndexOfNext = this.CalculatePhysicalIndex(index + 1);
                    this.jobs[physicalIndex] = this.jobs[physicalIndexOfNext];
                    this.jobIDs[physicalIndex] = this.jobIDs[physicalIndexOfNext];
                }
            }

            /// If the job has been removed, decrement the itemCount.
            if (indexOfRemovedJob != -1) { this.itemCount.Write(this.itemCount.Read() - 1); }

            /// If the first job has been removed, start the next job.
            if (indexOfRemovedJob == 0)
            {
                this.progress.Write(-1);

                /// Check if the player has enough supply to start the next production.
                /// TODO: implement the check, send error message & return if not!

                /// If the player has enough supply, then start the next production.
                this.progress.Write(0);
            }
        }

        /// <summary>
        /// Continues the current production job.
        /// </summary>
        public void ContinueProduction()
        {
            if (this.itemCount.Read() == 0) { throw new InvalidOperationException("This production line is inactive!"); }

            if (this.progress.Read() != -1)
            {
                this.progress.Write(this.progress.Read() + 1);
                IScenarioElementType currentJob = this.jobs[this.CalculatePhysicalIndex(0)];
                if (this.progress.Read() >= currentJob.BuildTime.Read())
                {
                    /// Current job finished.
                    TraceManager.WriteAllTrace(string.Format("Production of '{0}' completed.", currentJob.Name), TraceFilters.INFO);
                    this.startIndex.Write((this.startIndex.Read() + 1) % this.Capacity);
                    this.itemCount.Write(this.itemCount.Read() - 1);

                    /// Create the product using the factory component.
                    if (!this.entityFactory.CreateEntity(currentJob, this.owner.Read().Owner, this.owner.Read()))
                    {
                        /// TODO: if the entity could not be created, give back the locked minerals, vespene gas and supply to the player!
                    }

                    this.progress.Write(-1);
                }
            }

            if (this.itemCount.Read() > 0 && this.progress.Read() == -1)
            {
                /// Check if the player has enough supply to start the next production.
                /// TODO: implement the check, send error message & return if not!
                
                /// If the player has enough supply, then start the next production.
                this.progress.Write(0);
            }
        }

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
        /// TODO: store these objects also in a HeapedArray!
        private readonly List<IScenarioElementType> jobs;

        /// <summary>
        /// The list that contains the IDs of the jobs in this production line.
        /// </summary>
        private readonly List<int> jobIDs;

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
        /// The progress of the currently running production job in this production line or -1 if there is no running production
        /// job in this production line.
        /// </summary>
        private readonly HeapedValue<int> progress;

        /// <summary>
        /// The list of the products that can be produced by this production line mapped by their names.
        /// </summary>
        private readonly Dictionary<string, IScenarioElementType> products;

        /// <summary>
        /// Reference to the entity factory component.
        /// </summary>
        private readonly IEntityFactory entityFactory;
    }
}
