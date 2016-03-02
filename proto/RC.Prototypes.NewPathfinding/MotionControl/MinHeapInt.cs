using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Prototypes.NewPathfinding.MotionControl
{
    /// <summary>
    /// Represents a heap data structure for minimum search.
    /// </summary>
    public class MinHeapInt
    {
        /// <summary>
        /// Constructs a MinHeapInt object
        /// </summary>
        public MinHeapInt()
        {
            this.nextIndex = 0;
            this.heapArray = new List<int>();
        }

        /// <summary>
        /// Inserts a new item into the heap.
        /// </summary>
        /// <param name="item">The new item.</param>
        public void Insert(int item)
        {
            /// Add the new item to the bottom level of the heap.
            this.heapArray.Add(item);

            /// Restore the heap-property if necessary.
            this.Upheap(this.nextIndex);

            /// Increment the nextIndex pointer to the end of the heap array.
            this.nextIndex++;
        }

        /// <summary>
        /// Removes the first occurance of the given item from the heap.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        public void Delete(int item)
        {
            int itemIndex = this.heapArray.IndexOf(item);
            if (itemIndex == -1) { return; }
            
            /// If we have only 1 more element left, just clear the array and return.
            if (this.nextIndex == 1)
            {
                this.heapArray.Clear();
                this.nextIndex = 0;
                return;
            }

            /// Replace the found item with the last element.
            int tmp = this.heapArray[this.nextIndex - 1];
            this.heapArray.RemoveAt(this.nextIndex - 1);
            this.heapArray[itemIndex] = tmp;

            /// Decrement the nextIndex pointer to the end of the heap array.
            this.nextIndex--;

            /// Restore the heap-property if necessary.
            this.Downheap(itemIndex);
        }

        /// <summary>
        /// Deletes the item at the top of the heap.
        /// </summary>
        public void DeleteTop()
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
            int tmp = this.heapArray[this.nextIndex - 1];
            this.heapArray.RemoveAt(this.nextIndex - 1);
            this.heapArray[0] = tmp;

            /// Decrement the nextIndex pointer to the end of the heap array.
            this.nextIndex--;

            /// Restore the heap-property if necessary.
            this.Downheap(0);
        }

        /// <summary>
        /// Gets the item at the top of the heap.
        /// </summary>
        public int TopItem
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
        /// Gets the items of this heap in the order as they are stored in the underlying array.
        /// </summary>
        public IEnumerable<int> Items { get { return this.heapArray; } }

        /// <summary>
        /// This method is called when the key of a heap item has been changed.
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
                if (this.heapArray[currIdx] < this.heapArray[parentIdx])
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
        /// Internal method to restore the heap-property after a DeleteTop operation.
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
                    if (this.heapArray[leftIdx] < this.heapArray[currIdx])
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
                if (this.heapArray[leftIdx] < this.heapArray[currIdx] && this.heapArray[rightIdx] < this.heapArray[currIdx])
                {
                    /// If the heap-property is wrong with both of the children, we have to swap the current item with its smaller child.
                    int childIdx = this.heapArray[leftIdx] > this.heapArray[rightIdx] ? rightIdx : leftIdx;
                    this.SwapItems(childIdx, currIdx);

                    currIdx = childIdx;
                    continue;
                }
                else if (this.heapArray[leftIdx] < this.heapArray[currIdx] && this.heapArray[rightIdx] >= this.heapArray[currIdx])
                {
                    /// If the heap-property is wrong only with the left child, we have to swap the current item with the left child.
                    this.SwapItems(leftIdx, currIdx);

                    currIdx = leftIdx;
                    continue;
                }
                else if (this.heapArray[leftIdx] >= this.heapArray[currIdx] && this.heapArray[rightIdx] < this.heapArray[currIdx])
                {
                    /// If the heap-property is wrong only with the right child, we have to swap the current item with the right child.
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
            int tmp = this.heapArray[idxA];
            this.heapArray[idxA] = this.heapArray[idxB];
            this.heapArray[idxB] = tmp;
        }

        /// <summary>
        /// The underlying array that stores the items of the heap.
        /// </summary>
        private readonly List<int> heapArray;

        /// <summary>
        /// The next free index in the heap array.
        /// </summary>
        private int nextIndex;
    }
}
