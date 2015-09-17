using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Diagnostics;
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
        /// <param name="capacity">The capacity of this production line.</param>
        /// <param name="products">The type of products that can be produced by this production line.</param>
        public ProductionLine(int capacity, List<IScenarioElementType> products)
        {
            if (capacity <= 0) { throw new ArgumentOutOfRangeException("capacity", "Capacity of a production line shall be greater than 0!"); }
            if (products == null) { throw new ArgumentNullException("products"); }
            if (products.Count == 0) { throw new ArgumentException("Production line cannot be created with empty product list!", "products"); }

            this.products = new Dictionary<string, IScenarioElementType>();
            foreach (IScenarioElementType product in products) { this.products.Add(product.Name, product); }

            this.items = new List<IScenarioElementType>(capacity);
            for (int i = 0; i < capacity; i++) { this.items.Add(null); }

            this.startIndex = this.ConstructField<int>("startIndex");
            this.itemCount = this.ConstructField<int>("itemCount");
            this.progress = this.ConstructField<int>("progress");
            this.startIndex.Write(0);
            this.itemCount.Write(0);
            this.progress.Write(-1);
        }

        /// <summary>
        /// Gets the capacity of this production line.
        /// </summary>
        public int Capacity { get { return this.items.Count; } }

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
                return this.progress.Read();
            }
        }

        /// <summary>
        /// Gets the job in this production line at the given index or null if there is no item at the given index.
        /// The currently running production job is always at index 0.
        /// </summary>
        /// <param name="index">The index of the job to get.</param>
        /// <returns>The job at the given index or null if there is no item at the given index.</returns>
        /// <exception cref="InvalidOperationException">If this production line is inactive.</exception>
        public IScenarioElementType this[int index]
        {
            get
            {
                if (this.itemCount.Read() == 0) { throw new InvalidOperationException("This production line is inactive!"); }
                if (index < 0 || index >= this.Capacity) { throw new ArgumentOutOfRangeException("index"); }
                if (index >= this.itemCount.Read()) { return null; }
                
                return this.items[(this.startIndex.Read() + index) % this.Capacity];
            }
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
        /// Start producing the given product.
        /// </summary>
        /// <param name="productName">The name of the product to start producing.</param>
        /// TODO: handle resources!
        public void StartProduction(string productName)
        {
            if (productName == null) { throw new ArgumentNullException("productName"); }
            if (!this.products.ContainsKey(productName)) { throw new InvalidOperationException(string.Format("Product '{0}' cannot be produced at this production line!", productName)); }
            if (this.itemCount.Read() == this.Capacity) { throw new InvalidOperationException("This production line has reached its capacity!"); }

            int indexOfNewItem = (this.startIndex.Read() + this.itemCount.Read()) % this.Capacity;
            this.items[indexOfNewItem] = this.products[productName];
            if (this.itemCount.Read() == 0) { this.progress.Write(0); }
            this.itemCount.Write(this.itemCount.Read() + 1);
        }

        /// <summary>
        /// Continues the current production job.
        /// </summary>
        public void ContinueProduction()
        {
            if (this.itemCount.Read() == 0) { throw new InvalidOperationException("This production is inactive!"); }

            this.progress.Write(this.progress.Read() + 1);
            IScenarioElementType currentJob = this.items[this.startIndex.Read()];
            if (this.progress.Read() >= currentJob.BuildTime.Read())
            {
                /// Current job finished.
                TraceManager.WriteAllTrace(string.Format("Production of '{0}' completed.", currentJob.Name), TraceFilters.INFO);
                this.startIndex.Write((this.startIndex.Read() + 1) % this.Capacity);
                this.itemCount.Write(this.itemCount.Read() - 1);
                this.progress.Write(this.itemCount.Read() > 0 ? 0 : -1);
                /// TODO: create the product entity and place it to the map!
            }
        }

        /// <summary>
        /// The list that contains the items in this production line.
        /// </summary>
        /// TODO: store these objects also in a HeapedArray!
        private readonly List<IScenarioElementType> items;

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
    }
}
