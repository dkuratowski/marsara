using System;

namespace RC.Common
{
    /// <summary>
    /// This is a thread safe generic FIFO queue that has limited capacity.
    /// </summary>
    public class Fifo<T>
    {
        /// <summary>
        /// Constructs a FIFO queue with the given capacity.
        /// </summary>
        /// <param name="capacity">The maximum amount of items that can be stored in this FIFO.</param>
        public Fifo(int capacity)
        {
            if (capacity < 1) { throw new ArgumentOutOfRangeException("capacity"); }

            //this.lockObject = new object();
            this.items = new T[capacity];
            this.readIndex = 0;
            this.writeIndex = 0;
            this.fifoLength = 0;
        }

        /// <summary>
        /// Pushes an item to the end of the FIFO.
        /// </summary>
        /// <param name="item">The item you want to push into the FIFO.</param>
        /// <exception cref="FifoException">If the FIFO is full.</exception>
        public void Push(T item)
        {
            //if (item == null) { throw new ArgumentNullException("item"); }

            //lock (this.lockObject)
            //{
            if (this.fifoLength < this.items.Length)
            {
                this.items[this.writeIndex] = item;
                this.writeIndex++;
                if (this.writeIndex == this.items.Length) { this.writeIndex = 0; }
                this.fifoLength++;
            }
            else { throw new FifoException("The FIFO is full!"); }
            //}
        }

        /// <summary>
        /// Removes an returns the item from the beginning of the FIFO.
        /// </summary>
        /// <returns>The returned item.</returns>
        /// <exception cref="FifoException">If the queue is empty.</exception>
        public T Get()
        {
            //lock (this.lockObject)
            //{
            if (this.fifoLength > 0)
            {
                T retItem = this.items[this.readIndex];
                this.items[this.readIndex] = default(T);
                this.readIndex++;
                if (this.readIndex == this.items.Length) { this.readIndex = 0; }
                this.fifoLength--;
                return retItem;
            }
            else { throw new FifoException("Unable to read from empty FIFO!"); }
            //}
        }

        /// <summary>
        /// Gets the current length of the FIFO.
        /// </summary>
        public int Length
        {
            get
            {
                //lock (this.lockObject)
                //{
                return this.fifoLength;
                //}
            }
        }

        /// <summary>
        /// The items of the FIFO.
        /// </summary>
        private T[] items;

        /// <summary>
        /// The index of the slot in this.items where the next pushed item will be placed.
        /// </summary>
        private int writeIndex;

        /// <summary>
        /// The index of the slot in this.items where the next read item is placed.
        /// </summary>
        private int readIndex;

        /// <summary>
        /// The number of the items in the FIFO.
        /// </summary>
        private int fifoLength;

        /// <summary>
        /// This object is used as a mutex when multiple threads want to use this FIFO.
        /// </summary>
        //private object lockObject;
    }
}
