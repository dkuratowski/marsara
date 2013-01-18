using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchProto
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
    /// Represents a generic binary heap data structure.
    /// </summary>
    public class BinaryHeap<T>
    {
        /// <summary>
        /// Constructs a BinaryHeap object of the given type.
        /// </summary>
        /// <param name="type">The type of the heap.</param>
        public BinaryHeap(HeapType type)
        {
            this.type = type;
            this.nextIndex = 0;
            this.heapArray = new List<Tuple<int, T>>();
        }

        /// <summary>
        /// Inserts a new item with the given key into the heap.
        /// </summary>
        /// <param name="key">The key of the new item.</param>
        /// <param name="item">The new item.</param>
        public void Insert(int key, T item)
        {
            /// Add the new item to the bottom level of the heap.
            this.heapArray.Add(new Tuple<int, T>(key, item));

            /// Restore the heap-property if necessary.
            this.Upheap();

            /// Increment the nextIndex pointer to the end of the heap array.
            this.nextIndex++;
        }

        /// <summary>
        /// Deletes the item at the key with the maximum value in case of HeapType.MaxHeap or with the minimum value in case of HeapType.MinHeap.
        /// </summary>
        public void DeleteMaxMin()
        {
            if (this.nextIndex == 0) { throw new InvalidOperationException("The heap is empty!"); }

            /// If we have only 1 more element left, just clear the array and return.
            if (this.nextIndex == 1)
            {
                this.heapArray.Clear();
                this.nextIndex = 0;
                return;
            }

            /// Replace the root of the heap with the last element.
            Tuple<int, T> tmp = this.heapArray[this.nextIndex - 1];
            this.heapArray.RemoveAt(this.nextIndex - 1);
            this.heapArray[0] = tmp;

            /// Decrement the nextIndex pointer to the end of the heap array.
            this.nextIndex--;

            /// Restore the heap-property if necessary.
            this.Downheap();
        }

        /// <summary>
        /// Gets the key with the maximum value in case of HeapType.MaxHeap or with the minimum value in case of HeapType.MinHeap.
        /// </summary>
        public int MaxMinKey
        {
            get
            {
                if (this.nextIndex == 0) { throw new InvalidOperationException("The heap is empty!"); }
                return this.heapArray[0].Item1;
            }
        }

        /// <summary>
        /// Gets the item at the key with the maximum value in case of HeapType.MaxHeap or with the minimum value in case of HeapType.MinHeap.
        /// </summary>
        public T MaxMinItem
        {
            get
            {
                if (this.nextIndex == 0) { throw new InvalidOperationException("The heap is empty!"); }
                return this.heapArray[0].Item2;
            }
        }

        /// <summary>
        /// Gets the number of items in the heap.
        /// </summary>
        public int Count { get { return this.nextIndex; } }

        /// <summary>
        /// Internal method to restore the heap-property after an Insert operation.
        /// </summary>
        private void Upheap()
        {
            int currIdx = this.nextIndex;
            while (currIdx != 0)
            {
                int parentIdx = (currIdx - 1) / 2;
                if ((this.type == HeapType.MaxHeap) ?
                    (this.heapArray[currIdx].Item1 > this.heapArray[parentIdx].Item1) :
                    (this.heapArray[currIdx].Item1 < this.heapArray[parentIdx].Item1))
                {
                    /// Swap the current item with its parent.
                    Tuple<int, T> tmp = this.heapArray[parentIdx];
                    this.heapArray[parentIdx] = this.heapArray[currIdx];
                    this.heapArray[currIdx] = tmp;

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
        private void Downheap()
        {
            /// If the heap is empty, we have nothing to do
            if (this.nextIndex == 0) { return; }

            int currIdx = 0;
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
                        (this.heapArray[leftIdx].Item1 > this.heapArray[currIdx].Item1) :
                        (this.heapArray[leftIdx].Item1 < this.heapArray[currIdx].Item1))
                    {
                        /// Swap the current item with its left child.
                        Tuple<int, T> tmp = this.heapArray[leftIdx];
                        this.heapArray[leftIdx] = this.heapArray[currIdx];
                        this.heapArray[currIdx] = tmp;

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
                    (this.heapArray[leftIdx].Item1 > this.heapArray[currIdx].Item1 && this.heapArray[rightIdx].Item1 > this.heapArray[currIdx].Item1) :
                    (this.heapArray[leftIdx].Item1 < this.heapArray[currIdx].Item1 && this.heapArray[rightIdx].Item1 < this.heapArray[currIdx].Item1))
                {
                    /// If the heap-property is wrong with both of the children, we have to swap the current item with its larger child
                    /// in case of HeapType.MaxHeap or with its smaller child in case of HeapType.MinHeap.
                    int childIdx = this.heapArray[leftIdx].Item1 > this.heapArray[rightIdx].Item1 ?
                                   (this.type == HeapType.MaxHeap ? leftIdx : rightIdx) :
                                   (this.type == HeapType.MaxHeap ? rightIdx : leftIdx);
                    Tuple<int, T> tmp = this.heapArray[childIdx];
                    this.heapArray[childIdx] = this.heapArray[currIdx];
                    this.heapArray[currIdx] = tmp;

                    currIdx = childIdx;
                    continue;
                }
                else if ((this.type == HeapType.MaxHeap) ?
                         (this.heapArray[leftIdx].Item1 > this.heapArray[currIdx].Item1 && this.heapArray[rightIdx].Item1 <= this.heapArray[currIdx].Item1) :
                         (this.heapArray[leftIdx].Item1 < this.heapArray[currIdx].Item1 && this.heapArray[rightIdx].Item1 >= this.heapArray[currIdx].Item1))
                {
                    /// If the heap-property is wrong only with the left child, we have to swap the current item with the left child
                    Tuple<int, T> tmp = this.heapArray[leftIdx];
                    this.heapArray[leftIdx] = this.heapArray[currIdx];
                    this.heapArray[currIdx] = tmp;

                    currIdx = leftIdx;
                    continue;
                }
                else if ((this.type == HeapType.MaxHeap) ?
                         (this.heapArray[leftIdx].Item1 <= this.heapArray[currIdx].Item1 && this.heapArray[rightIdx].Item1 > this.heapArray[currIdx].Item1) :
                         (this.heapArray[leftIdx].Item1 >= this.heapArray[currIdx].Item1 && this.heapArray[rightIdx].Item1 < this.heapArray[currIdx].Item1))
                {
                    /// If the heap-property is wrong only with the right child, we have to swap the current item with the right child
                    Tuple<int, T> tmp = this.heapArray[rightIdx];
                    this.heapArray[rightIdx] = this.heapArray[currIdx];
                    this.heapArray[currIdx] = tmp;

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
        /// The underlying array that stores the keys and the corresponding items of the heap.
        /// </summary>
        private List<Tuple<int, T>> heapArray;

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
