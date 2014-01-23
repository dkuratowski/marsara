using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// The base class of items in a binary heap data structure.
    /// </summary>
    public class BinaryHeapItem
    {
        /// <summary>
        /// Constructs a binary heap item.
        /// </summary>
        public BinaryHeapItem()
        {
            this.index = -1;
            this.key = -1;
        }

        /// <summary>
        /// Handles the event when the key of an item in the binary heap has been changed.
        /// </summary>
        /// <param name="itemIndex">The index of the item.</param>
        public delegate void ItemKeyChangedHdl(int itemIndex);

        /// <summary>
        /// This event is raised when the key of an item in the binary heap has been changed.
        /// </summary>
        public event ItemKeyChangedHdl ItemKeyChanged;

        /// <summary>
        /// Gets the index of this binary heap item.
        /// </summary>
        public int HeapIndex
        {
            get { return this.index; }
            internal set { this.index = value; }
        }

        /// <summary>
        /// Gets the key of this binary heap item.
        /// </summary>
        public int Key { get { return this.key; } }

        /// <summary>
        /// Call this method from the derived classes to indicate that the key of this item has changed.
        /// </summary>
        /// <param name="newKey">The new key of this item.</param>
        protected void OnKeyChanged(int newKey)
        {
            this.key = newKey;
            if (this.ItemKeyChanged != null) { this.ItemKeyChanged(this.index); }
        }

        /// <summary>
        /// The key of this item.
        /// </summary>
        private int key;

        /// <summary>
        /// The index of this item.
        /// </summary>
        private int index;
    }

    /// <summary>
    /// Represents a generic binary heap data structure.
    /// </summary>
    public class BinaryHeap<T> where T : BinaryHeapItem
    {
        /// <summary>
        /// Enumerates the possible types of a heap object.
        /// </summary>
        public enum HeapType
        {
            MaxHeap = 0,
            MinHeap = 1
        }

        /// <summary>
        /// Constructs a BinaryHeap object of the given type.
        /// </summary>
        /// <param name="type">The type of the heap.</param>
        public BinaryHeap(HeapType type)
        {
            this.type = type;
            this.nextIndex = 0;
            this.heapArray = new List<T>();
        }

        /// <summary>
        /// Inserts a new item into the heap.
        /// </summary>
        /// <param name="item">The new item.</param>
        public void Insert(T item)
        {
            if (item.Key < 0) { throw new ArgumentException("Key of the item must be non-negative!", "item"); }

            /// Add the new item to the bottom level of the heap.
            item.HeapIndex = this.nextIndex;
            this.heapArray.Add(item);
            item.ItemKeyChanged += this.OnItemKeyChanged;

            /// Restore the heap-property if necessary.
            this.Upheap(this.nextIndex);

            /// Increment the nextIndex pointer to the end of the heap array.
            this.nextIndex++;
        }

        /// <summary>
        /// Deletes the item at the key with the maximum value in case of HeapType.MaxHeap or with the minimum value in case of HeapType.MinHeap.
        /// </summary>
        public void DeleteMaxMin()
        {
            if (this.nextIndex == 0) { throw new InvalidOperationException("The heap is empty!"); }

            this.heapArray[0].HeapIndex = -1;
            this.heapArray[0].ItemKeyChanged -= this.OnItemKeyChanged;

            /// If we have only 1 more element left, just clear the array and return.
            if (this.nextIndex == 1)
            {
                this.heapArray.Clear();
                this.nextIndex = 0;
                return;
            }

            /// Replace the root of the heap with the last element.
            T tmp = this.heapArray[this.nextIndex - 1];
            this.heapArray.RemoveAt(this.nextIndex - 1);
            this.heapArray[0] = tmp;
            this.heapArray[0].HeapIndex = 0;

            /// Decrement the nextIndex pointer to the end of the heap array.
            this.nextIndex--;

            /// Restore the heap-property if necessary.
            this.Downheap(0);
        }

        /// <summary>
        /// Gets the item with the maximum key value in case of HeapType.MaxHeap or with the minimum key value in case of HeapType.MinHeap.
        /// </summary>
        public T MaxMinItem
        {
            get
            {
                if (this.nextIndex == 0) { throw new InvalidOperationException("The heap is empty!"); }
                return this.heapArray[0];
            }
        }

        /// <summary>
        /// Gets the number of items in the heap.
        /// </summary>
        public int Count { get { return this.nextIndex; } }

        /// <summary>
        /// This method is called when the key of a binary heap item has been changed.
        /// </summary>
        /// <param name="itemIdx">The index of the item.</param>
        private void OnItemKeyChanged(int itemIdx)
        {
            this.Upheap(itemIdx);
            this.Downheap(itemIdx);
        }

        /// <summary>
        /// Internal method to restore the heap-property after an Insert operation.
        /// </summary>
        private void Upheap(int fromIdx)
        {
            int currIdx = fromIdx;
            while (currIdx != 0)
            {
                int parentIdx = (currIdx - 1) / 2;
                if ((this.type == HeapType.MaxHeap) ?
                    (this.heapArray[currIdx].Key > this.heapArray[parentIdx].Key) :
                    (this.heapArray[currIdx].Key < this.heapArray[parentIdx].Key))
                {
                    /// Swap the current item with its parent.
                    this.SwapItems(parentIdx, currIdx);

                    /// Go to the parent item and continue the upheap.
                    currIdx = parentIdx;
                }
                else
                {
                    /// The heap-property is now OK.
                    break;
                }
            }
        }

        /// <summary>
        /// Internal method to restore the heap-property after a DeleteMaxMin operation.
        /// </summary>
        private void Downheap(int fromIdx)
        {
            /// If the heap is empty, we have nothing to do
            if (this.nextIndex == 0) { return; }

            int currIdx = fromIdx;
            while (true)
            {
                int leftIdx = 2 * currIdx + 1;
                int rightIdx = 2 * currIdx + 2;

                /// If there is not even a left child, the heap-property is OK.
                if (leftIdx >= this.nextIndex) { break; }

                /// If there is no right child, we only have to compare with the left.
                if (rightIdx >= this.nextIndex)
                {
                    if ((this.type == HeapType.MaxHeap) ?
                        (this.heapArray[leftIdx].Key > this.heapArray[currIdx].Key) :
                        (this.heapArray[leftIdx].Key < this.heapArray[currIdx].Key))
                    {
                        /// Swap the current item with its left child.
                        this.SwapItems(leftIdx, currIdx);

                        currIdx = leftIdx;
                        continue;
                    }
                    else
                    {
                        /// The heap-property is now OK.
                        break;
                    }
                }

                /// If there is a right child, we have to compare with both.
                if ((this.type == HeapType.MaxHeap) ?
                    (this.heapArray[leftIdx].Key > this.heapArray[currIdx].Key && this.heapArray[rightIdx].Key > this.heapArray[currIdx].Key) :
                    (this.heapArray[leftIdx].Key < this.heapArray[currIdx].Key && this.heapArray[rightIdx].Key < this.heapArray[currIdx].Key))
                {
                    /// If the heap-property is wrong with both of the children, we have to swap the current item with its larger child
                    /// in case of HeapType.MaxHeap or with its smaller child in case of HeapType.MinHeap.
                    int childIdx = this.heapArray[leftIdx].Key > this.heapArray[rightIdx].Key ?
                                   (this.type == HeapType.MaxHeap ? leftIdx : rightIdx) :
                                   (this.type == HeapType.MaxHeap ? rightIdx : leftIdx);
                    this.SwapItems(childIdx, currIdx);

                    currIdx = childIdx;
                    continue;
                }
                else if ((this.type == HeapType.MaxHeap) ?
                         (this.heapArray[leftIdx].Key > this.heapArray[currIdx].Key && this.heapArray[rightIdx].Key <= this.heapArray[currIdx].Key) :
                         (this.heapArray[leftIdx].Key < this.heapArray[currIdx].Key && this.heapArray[rightIdx].Key >= this.heapArray[currIdx].Key))
                {
                    /// If the heap-property is wrong only with the left child, we have to swap the current item with the left child
                    this.SwapItems(leftIdx, currIdx);

                    currIdx = leftIdx;
                    continue;
                }
                else if ((this.type == HeapType.MaxHeap) ?
                         (this.heapArray[leftIdx].Key <= this.heapArray[currIdx].Key && this.heapArray[rightIdx].Key > this.heapArray[currIdx].Key) :
                         (this.heapArray[leftIdx].Key >= this.heapArray[currIdx].Key && this.heapArray[rightIdx].Key < this.heapArray[currIdx].Key))
                {
                    /// If the heap-property is wrong only with the right child, we have to swap the current item with the right child
                    this.SwapItems(rightIdx, currIdx);

                    currIdx = rightIdx;
                    continue;
                }
                else
                {
                    /// The heap-property is now OK.
                    break;
                }
            }
        }

        /// <summary>
        /// Swaps two items in the underlying heap array.
        /// </summary>
        /// <param name="idxA">The index of the first item.</param>
        /// <param name="idxB">The index of the second item.</param>
        private void SwapItems(int idxA, int idxB)
        {
            T tmp = this.heapArray[idxA];
            this.heapArray[idxA] = this.heapArray[idxB];
            this.heapArray[idxA].HeapIndex = idxA;
            this.heapArray[idxB] = tmp;
            this.heapArray[idxB].HeapIndex = idxB;
        }

        /// <summary>
        /// The underlying array that stores the items of the heap.
        /// </summary>
        private List<T> heapArray;

        /// <summary>
        /// The next free index in the heap array.
        /// </summary>
        private int nextIndex;

        /// <summary>
        /// The type of this heap object.
        /// </summary>
        private HeapType type;
    }
}
